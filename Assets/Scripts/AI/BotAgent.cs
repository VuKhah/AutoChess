using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotAgent
{
    public Chromosome brain;
    public EconomyManager economy;
    public List<CardInstance> board = new List<CardInstance>(new CardInstance[GameManager.BoardSlotCount]);

    private const int FrontlineCount = 4;
    private int _shopTier = 1;
    private List<CardDefinition> _currentUnitShop = new List<CardDefinition>();

    public int startingCoins = 10;

    // Freeze: giữ nguyên shop từ lượt này sang lượt sau (giống player bấm Freeze)
    public bool                 isShopFrozen    = false;
    public List<CardDefinition> frozenUnitShop  = null;
    public List<CardDefinition> frozenSpellShop = null;

    public BotAgent(Chromosome brain, int coinsPerTurn = 10)
    {
        this.brain         = brain;
        this.economy       = new EconomyManager();
        this.startingCoins = coinsPerTurn;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // END COMBAT PHASE
    // ──────────────────────────────────────────────────────────────────────────
    public void EndCombatPhase()
    {
        for (int i = 0; i < board.Count; i++)
        {
            var unit = board[i];
            if (unit == null) continue;
            if (unit.IsDead) board[i] = null;
            else unit.ResetStats();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // END TURN SHOP
    // ──────────────────────────────────────────────────────────────────────────
    public void TriggerEndTurnShop()
    {
        if (brain == null) return;
        foreach (var unit in board)
        {
            if (unit == null || unit.IsDead || unit.Data.abilities == null) continue;
            foreach (var ability in unit.Data.abilities)
            {
                if (ability == null || ability.trigger != TriggerType.EndTurnShop) continue;
                if (ability.effect == EffectType.AddStats)
                {
                    if (ability.isPermanent)
                    {
                        unit.permanentATKBonus += ability.effectValue1;
                        unit.permanentHPBonus  += ability.effectValue2;
                    }
                    else
                    {
                        unit.growthATKBonus += ability.effectValue1;
                        unit.growthHPBonus  += ability.effectValue2;
                    }
                    unit.ResetStats();
                }
                else if (ability.effect == EffectType.GainCoin)
                {
                    economy.AddBonus(ability.effectValue1);
                }
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PREP PHASE — vòng quyết định chính của bot, giống người chơi thực:
    //   1. Reroll nếu shop kém
    //   2. Mua unit tốt nhất
    //   3. Mua và dùng spell
    //   4. Bán unit thừa (proactive)
    //   5. Merge
    // ──────────────────────────────────────────────────────────────────────────
    public void DecidePrepPhase(List<CardDefinition> unitShop,
                                List<CardDefinition> spellShop = null,
                                int shopTier = 1)
    {
        if (brain == null) return;
        _shopTier = shopTier;

        // Áp frozen shop từ lượt trước (nếu có) rồi xóa flag
        if (isShopFrozen && frozenUnitShop != null)
        {
            unitShop  = frozenUnitShop;
            spellShop = frozenSpellShop;
        }
        isShopFrozen    = false;
        frozenUnitShop  = null;
        frozenSpellShop = null;

        economy.ResetEconomy();
        int coinDiff = 10 - startingCoins;
        if (coinDiff > 0) economy.TrySpend(coinDiff);

        // 1. Reroll
        RerollPhase(ref unitShop, ref spellShop);
        _currentUnitShop = unitShop != null ? new List<CardDefinition>(unitShop) : new List<CardDefinition>();

        // 2. Mua unit
        BuyUnitsPhase(unitShop);

        // 3. Mua và dùng spell
        if (spellShop != null && spellShop.Count > 0)
            BuySpellsPhase(spellShop);

        // 4. Bán unit điểm thấp để dọn board
        ProactiveSellPhase();

        // 5. Merge
        TryMerge();

        // 6. Sắp xếp lại board tối ưu (giống player kéo unit vào đúng vị trí)
        RepositionPhase();

        // 7. Freeze shop nếu còn unit đáng mua mà chưa đủ coin
        FreezePhase(unitShop, spellShop);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // FREEZE PHASE — lưu shop lại cho lượt sau nếu có unit muốn nhưng chưa đủ tiền
    // ──────────────────────────────────────────────────────────────────────────
    private void FreezePhase(List<CardDefinition> unitShop, List<CardDefinition> spellShop)
    {
        // Bot thích reroll nhiều (gene[24] cao) → ít có xu hướng freeze
        float freezeTendency = 1f - brain.genes[24];
        if (freezeTendency < 0.35f) return;

        float saveThreshold = brain.genes[23] * 3f;
        // Có unit tốt trong shop nhưng không đủ coin để mua lượt này → freeze để giữ lại
        bool wantedButCantAfford = unitShop != null && unitShop.Exists(c =>
            c != null && c.cost > economy.CurrentCoin && Evaluate(c) >= saveThreshold);

        if (wantedButCantAfford)
        {
            isShopFrozen    = true;
            frozenUnitShop  = new List<CardDefinition>(unitShop);
            frozenSpellShop = spellShop != null ? new List<CardDefinition>(spellShop) : null;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // REPOSITION PHASE — sắp xếp lại board sau khi mua/bán/merge xong
    //   Giống player kéo unit vào đúng vị trí: Taunt lên frontline trước,
    //   rồi sắp xếp theo điểm mạnh giảm dần để unit tốt nhất tấn công sớm hơn.
    // ──────────────────────────────────────────────────────────────────────────
    private void RepositionPhase()
    {
        var units = board.Where(u => u != null && !u.IsDead).ToList();
        if (units.Count == 0) return;

        // Taunt lên đầu (explicit frontline), sau đó theo PositionScore
        units = units
            .OrderByDescending(u => u.isTaunt
                ? brain.genes[22] * 20f + PositionScore(u)
                : PositionScore(u))
            .ToList();

        for (int i = 0; i < board.Count; i++) board[i] = null;

        int frontSlot = 0, backSlot = FrontlineCount;
        foreach (var unit in units)
        {
            if (frontSlot < FrontlineCount)
            {
                unit.slotIndex = frontSlot;
                board[frontSlot++] = unit;
            }
            else if (backSlot < board.Count)
            {
                unit.slotIndex = backSlot;
                board[backSlot++] = unit;
            }
        }
    }

    // Điểm vị trí: tách biệt với EvaluateInstance — có tính đến role của ability.
    // Unit cần sống lâu (Aura, OnAllyDeath accumulator) bị giảm điểm → đẩy về backline.
    // Unit muốn chết nhanh (deathrattle OnDeath) được tăng điểm → lên frontline.
    // Gene[17] (eGiveBuff/support value) kiểm soát mức độ ưu tiên này.
    private float PositionScore(CardInstance unit)
    {
        float score = EvaluateInstance(unit);
        if (unit.Data.abilities == null) return score;

        foreach (var ability in unit.Data.abilities)
        {
            if (ability == null) continue;
            switch (ability.trigger)
            {
                // Unit cần sống để phát huy tác dụng → backline (giảm điểm frontline)
                case TriggerType.Aura:
                case TriggerType.OnAllyDeath:
                case TriggerType.OnAllyReborn:
                case TriggerType.OnAllySummon:
                case TriggerType.EndTurnShop:
                    score -= brain.genes[17] * 15f;
                    break;
                // Deathrattle → muốn chết để kích → frontline ổn (tăng điểm nhẹ)
                case TriggerType.OnDeath:
                    score += brain.genes[8] * 5f;
                    break;
            }
        }
        return score;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 1. REROLL PHASE
    //    Trả 1 coin để xem shop mới nếu shop hiện tại không đủ tốt so với board.
    // ──────────────────────────────────────────────────────────────────────────
    private void RerollPhase(ref List<CardDefinition> unitShop, ref List<CardDefinition> spellShop)
    {
        if (brain.genes[24] < 0.05f) return; // gene quá thấp → không bao giờ reroll

        int maxRerolls = Mathf.FloorToInt(brain.genes[25] * 3f) + 1; // [1..4]
        int keepCoins  = Mathf.FloorToInt(brain.genes[26] * 4f);     // [0..4]

        for (int r = 0; r < maxRerolls; r++)
        {
            // Cần ít nhất (keepCoins + 1) coin để reroll mà không phá vỡ ngưỡng dự phòng
            if (economy.CurrentCoin < keepCoins + 1) break;

            float shopBest  = BestUnitScore(unitShop);
            float boardBest = board.Where(u => u != null && !u.IsDead)
                                   .Select(u => EvaluateInstance(u))
                                   .DefaultIfEmpty(4f)
                                   .Max();

            // Shop đã đủ tốt → dừng
            if (shopBest >= brain.genes[24] * boardBest) break;

            economy.TrySpend(1);
            unitShop  = CardDatabase.Instance.GetRandomUnitShop(5, _shopTier);
            spellShop = CardDatabase.Instance.GetRandomSpellShop(2, _shopTier);
        }
    }

    private float BestUnitScore(List<CardDefinition> shop)
    {
        float best = 0f;
        foreach (var c in shop)
            if (c != null) best = Mathf.Max(best, Evaluate(c));
        return best;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 2. BUY UNITS PHASE
    // ──────────────────────────────────────────────────────────────────────────
    private void BuyUnitsPhase(List<CardDefinition> unitShop)
    {
        if (unitShop == null || unitShop.Count == 0) return;

        const int MaxBuyActions = 20;
        int actions = 0;
        bool bought = true;
        while (bought && actions++ < MaxBuyActions)
        {
            bought = false;
            CardDefinition bestCard  = null;
            float          bestScore = float.MinValue;
            int            bestIndex = -1;

            for (int i = 0; i < unitShop.Count; i++)
            {
                var card = unitShop[i];
                if (card == null || card.cost > economy.CurrentCoin) continue;
                float score = Evaluate(card);
                if (score > bestScore) { bestScore = score; bestCard = card; bestIndex = i; }
            }

            float saveThreshold = brain.genes[23] * 3f;
            if (bestCard == null || bestScore < saveThreshold) break;

            if (!HasEmptySlot())
            {
                int   worstIdx   = FindWorstUnitIndex();
                float worstScore = worstIdx >= 0 ? EvaluateInstance(board[worstIdx]) : float.MaxValue;
                float sellBar    = worstScore * (1.5f + brain.genes[23]);
                if (worstIdx >= 0 && bestScore > sellBar)
                {
                    var worstUnit = board[worstIdx];
                    FireTrigger(TriggerType.OnSell, worstUnit);
                    BroadcastAllyTrigger(TriggerType.OnAllySell, worstUnit);
                    board[worstIdx] = null;
                    economy.Sell();
                }
            }

            if (!HasEmptySlot()) break;

            if (BuyAndPlace(bestCard))
            {
                if (bestIndex >= 0 && bestIndex < unitShop.Count)
                    unitShop.RemoveAt(bestIndex);
                bought = true;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 3. BUY SPELLS PHASE
    //    Mua spell nếu đủ tốt, rồi dùng ngay lên unit tốt nhất trên board.
    // ──────────────────────────────────────────────────────────────────────────
    private void BuySpellsPhase(List<CardDefinition> spellShop)
    {
        float buyThreshold = brain.genes[28] * 3f;

        // Sắp xếp theo điểm giảm dần để ưu tiên spell tốt nhất
        var sorted = spellShop
            .Where(s => s != null && s.cardType == CardType.Spell)
            .OrderByDescending(s => EvaluateSpell(s))
            .ToList();

        foreach (var spell in sorted)
        {
            if (spell.cost > economy.CurrentCoin) continue;
            if (EvaluateSpell(spell) < buyThreshold) continue;

            economy.TryBuy(spell.cost);
            ApplySpellToBoard(spell);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // EVALUATE SPELL — chấm điểm spell để quyết định có mua không
    // ──────────────────────────────────────────────────────────────────────────
    private float EvaluateSpell(CardDefinition spell)
    {
        if (spell?.spellEffects == null || spell.spellEffects.Count == 0) return float.MinValue;

        float score = 0f;
        foreach (var fx in spell.spellEffects)
        {
            switch (fx.effect)
            {
                case 1:  // BuffStats
                    score += (fx.effectValue1 * brain.genes[0] + fx.effectValue2 * brain.genes[1])
                           * (fx.isPermanent ? 2.5f : 1f);
                    break;
                case 6:  // GainCoin
                    score += fx.effectValue1 * brain.genes[16] * brain.genes[31];
                    break;
                case 10: // GetRandomUnit
                    score += brain.genes[2] * 6f;
                    break;
                case 11: // StealFromShop — giá trị tương đương GetRandomUnit nhưng cùng tier shop
                    score += brain.genes[2] * 7f;
                    break;
                case 13: // GetSameRealmUnit — lấy unit cùng tribe, hỗ trợ synergy
                    score += brain.genes[2] * 5f + brain.genes[7] * 4f;
                    break;
                case 12: // GainIncome
                    score += fx.effectValue1 * brain.genes[16] * brain.genes[31]
                           * (fx.isPermanent ? 12f : 1.5f);
                    break;
                case 17: // UpgradeTierUnit
                    score += brain.genes[2] * 12f;
                    break;
                case 22: // GetUnitAtNextTier — unit cao hơn tier hiện tại, giá trị cao hơn case 10
                    score += brain.genes[2] * (7f + _shopTier * 0.4f);
                    break;
                case 18: // GiveDoubleAtkAndSafeguard
                    score += brain.genes[0] * 8f + brain.genes[6] * 8f;
                    break;
                case 19: // ToggleTaunt
                    score += brain.genes[4] * 6f;
                    break;
                case 20: // BuffByShopTier
                    score += _shopTier * (brain.genes[0] + brain.genes[1]) * 0.6f;
                    break;
                case 21: // GiveEndTurnBuff
                    score += (fx.effectValue1 * brain.genes[0] + fx.effectValue2 * brain.genes[1])
                           * brain.genes[11] * brain.genes[31] * 3f;
                    break;
                case 14: // LoseLife — nguy hiểm, âm điểm nặng
                    score -= 25f;
                    break;
                case 15: // TransferStats — phá hủy unit, rủi ro cao
                    score -= 8f;
                    break;
            }
        }

        if (spell.cost > 0)
            score = score / spell.cost * (1f + brain.genes[3]);

        return score;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // APPLY SPELL TO BOARD — áp dụng spell trực tiếp lên board của bot
    //   Không gọi GameManager → hoạt động cả trong GameSimulator lẫn gameplay.
    // ──────────────────────────────────────────────────────────────────────────
    private void ApplySpellToBoard(CardDefinition spell)
    {
        if (spell?.spellEffects == null) return;

        foreach (var fx in spell.spellEffects)
        {
            switch (fx.effect)
            {
                case 1:  // BuffStats
                {
                    var targets = FindSpellTargets(fx);
                    foreach (var t in targets)
                    {
                        if (fx.isPermanent)
                        {
                            t.permanentATKBonus += fx.effectValue1;
                            t.permanentHPBonus  += fx.effectValue2;
                            if (fx.isTaunt) { t.isTaunt = true; t.Data.hasTaunt = true; }
                        }
                        else
                        {
                            t.tempSpellATKBonus += fx.effectValue1;
                            t.tempSpellHPBonus  += fx.effectValue2;
                        }
                        t.ResetStats();
                    }
                    break;
                }
                case 6:  // GainCoin
                    economy.Earn(fx.effectValue1);
                    break;

                case 10: // GetRandomUnit — thêm unit ngẫu nhiên vào board
                {
                    int tier = fx.effectValue1 > 0 ? fx.effectValue1 : _shopTier;
                    var pool = CardDatabase.Instance.GetAllUnits().FindAll(c => c.tier == tier);
                    if (pool.Count == 0)
                        pool = CardDatabase.Instance.GetAllUnits().FindAll(c => c.tier <= Mathf.Max(tier, 1));
                    int count = Mathf.Max(fx.targetCount, 1);
                    for (int i = 0; i < count && HasEmptySlot() && pool.Count > 0; i++)
                    {
                        var data = pool[Random.Range(0, pool.Count)];
                        PlaceOnBoard(new CardInstance(data, 0));
                    }
                    break;
                }

                case 12: // GainIncome
                    if (fx.isPermanent) economy.AddPermanentIncome(fx.effectValue1);
                    else               economy.Earn(fx.effectValue1);
                    break;

                case 17: // UpgradeTierUnit — nâng mergeLevel unit cùng tribe
                {
                    CardInstance best = FindBestSpellTarget(preferMerged: true);
                    if (best != null) { best.mergeLevel = Mathf.Min(best.mergeLevel + 1, 2); best.ResetStats(); }
                    break;
                }

                case 18: // GiveDoubleAtkAndSafeguard
                {
                    CardInstance best = FindBestSpellTarget();
                    if (best != null) { best.permanentATKBonus += best.currentATK; best.Data.hasSafeguard = true; best.ResetStats(); }
                    break;
                }

                case 19: // ToggleTaunt
                {
                    CardInstance best = FindBestSpellTarget();
                    if (best != null)
                    {
                        if (best.isTaunt)
                        {
                            best.isTaunt = false; best.Data.hasTaunt = false;
                            best.permanentATKBonus += fx.effectValue1;
                        }
                        else
                        {
                            best.isTaunt = true; best.Data.hasTaunt = true;
                            best.permanentHPBonus += fx.effectValue2;
                            best.maxHP            += fx.effectValue2;
                        }
                        best.ResetStats();
                    }
                    break;
                }

                case 20: // BuffByShopTier
                {
                    CardInstance best = FindBestSpellTarget();
                    if (best != null) { best.permanentATKBonus += _shopTier; best.permanentHPBonus += _shopTier; best.ResetStats(); }
                    break;
                }

                case 21: // GiveEndTurnBuff
                {
                    CardInstance best = FindBestSpellTarget();
                    if (best != null)
                    {
                        if (best.Data.abilities == null)
                            best.Data.abilities = new List<AbilityData>();
                        best.Data.abilities.Add(new AbilityData
                        {
                            trigger      = TriggerType.EndTurnShop,
                            target       = TargetType.Self,
                            effect       = EffectType.AddStats,
                            effectValue1 = fx.effectValue1,
                            effectValue2 = fx.effectValue2,
                            isPermanent  = true
                        });
                        best.abilityTriggerCounts.Add(0);
                        best.abilityEscalationBonuses.Add(0);
                    }
                    break;
                }
                case 11: // StealFromShop — lấy unit ngẫu nhiên từ shop hiện tại đặt lên board
                {
                    var available = _currentUnitShop.FindAll(c => c != null);
                    if (available.Count > 0 && HasEmptySlot())
                    {
                        int idx = Random.Range(0, available.Count);
                        CardDefinition stolen = available[idx];
                        _currentUnitShop.Remove(stolen);
                        PlaceOnBoard(new CardInstance(stolen, 0));
                    }
                    break;
                }

                case 13: // GetSameRealmUnit — nhận unit cùng Tộc với unit tốt nhất trên board
                {
                    CardInstance refUnit = FindBestSpellTarget();
                    if (refUnit != null && HasEmptySlot())
                    {
                        var pool = CardDatabase.Instance.GetAllUnits()
                            .FindAll(c => c.tribe == refUnit.Data.tribe && c.cardID != refUnit.Data.cardID);
                        if (pool.Count > 0)
                            PlaceOnBoard(new CardInstance(pool[Random.Range(0, pool.Count)], 0));
                    }
                    break;
                }

                case 22: // GetUnitAtNextTier — đặt unit ngẫu nhiên Tier+1 lên board
                {
                    int targetTier = Mathf.Min(_shopTier + 1, 6);
                    var pool = CardDatabase.Instance.GetAllUnits().FindAll(c => c.tier == targetTier);
                    if (pool.Count == 0)
                        pool = CardDatabase.Instance.GetAllUnits().FindAll(c => c.tier <= targetTier);
                    if (pool.Count > 0 && HasEmptySlot())
                        PlaceOnBoard(new CardInstance(pool[Random.Range(0, pool.Count)], 0));
                    break;
                }

                // 14=LoseLife, 15=TransferStats, 16=ConditionalCoinGain: bỏ qua — không phù hợp với context bot
            }
        }
    }

    // Tìm danh sách target cho spell effect (theo target code của SpellEffectData)
    private List<CardInstance> FindSpellTargets(SpellEffectData fx)
    {
        var alive = board.FindAll(u => u != null && !u.IsDead);
        int n = Mathf.Max(fx.targetCount, 1);

        switch (fx.target)
        {
            case 3:  // AllAllies
                return alive;
            case 2:  // RandomAlly (1 target)
            case 13: // RandomAlliesInBattle (N targets)
            {
                var shuffled = alive.OrderBy(_ => Random.value).Take(n).ToList();
                return shuffled;
            }
            case 12: // ChosenAlly — bot chọn target tốt nhất
            default:
                var best = FindBestSpellTarget();
                return best != null ? new List<CardInstance> { best } : new List<CardInstance>();
        }
    }

    // Chọn unit tốt nhất để dùng spell, cân bằng bởi gene[29] (mạnh) và gene[30] (merged)
    private CardInstance FindBestSpellTarget(bool preferMerged = false)
    {
        var alive = board.FindAll(u => u != null && !u.IsDead);
        if (alive.Count == 0) return null;

        return alive.OrderByDescending(u =>
            EvaluateInstance(u) * brain.genes[29]
            + u.mergeLevel      * brain.genes[30] * 5f
            + (preferMerged ? u.mergeLevel * 10f : 0f)
        ).First();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 4. PROACTIVE SELL PHASE
    //    Bán unit có điểm quá thấp để làm chỗ cho các mua lượt sau.
    // ──────────────────────────────────────────────────────────────────────────
    private void ProactiveSellPhase()
    {
        if (brain.genes[27] < 0.05f) return;
        float sellBelow = brain.genes[27] * 3f;

        for (int i = 0; i < board.Count; i++)
        {
            var unit = board[i];
            if (unit == null || unit.IsDead || unit.isBattleSpawned) continue;
            if (EvaluateInstance(unit) < sellBelow)
            {
                FireTrigger(TriggerType.OnSell, unit);
                BroadcastAllyTrigger(TriggerType.OnAllySell, unit);
                board[i] = null;
                economy.Sell();
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MERGE
    // ──────────────────────────────────────────────────────────────────────────
    private void TryMerge()
    {
        int safety = 0;
        bool merged = true;
        while (merged && safety++ < 200)
        {
            merged = false;
            for (int i = 0; i < board.Count; i++)
            {
                var unit = board[i];
                if (unit == null || unit.mergeLevel >= 2) continue;

                var copies = new List<int> { i };
                for (int j = i + 1; j < board.Count; j++)
                {
                    if (board[j] != null
                        && board[j].Data.cardID == unit.Data.cardID
                        && board[j].mergeLevel  == unit.mergeLevel)
                        copies.Add(j);
                }

                int required = CardInstance.MergeRequiredCount(unit.mergeLevel);
                if (copies.Count < required) continue;

                // Trim về đúng số cần thiết để không tiêu thụ bản sao dư
                if (copies.Count > required) copies = copies.Take(required).ToList();

                int keepIdx   = copies[0];
                int bestBonus = int.MinValue;
                foreach (int idx in copies)
                {
                    var u = board[idx];
                    int bonus = u.permanentATKBonus + u.permanentHPBonus
                              + u.growthATKBonus    + u.growthHPBonus;
                    if (bonus > bestBonus) { bestBonus = bonus; keepIdx = idx; }
                }

                board[keepIdx].mergeLevel++;
                board[keepIdx].ResetStats();
                foreach (int idx in copies)
                    if (idx != keepIdx) board[idx] = null;

                SimulateMergeReward();
                merged = true;
                break;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // ABILITY TRIGGER — fire shop-phase triggers (OnDeploy, OnSell, OnAllyDeploy, OnAllySell)
    //   Bot không dùng AbilityEngine để giữ simulator độc lập với MonoBehaviour.
    //   Chỉ xử lý các effect phổ biến trong shop phase.
    // ──────────────────────────────────────────────────────────────────────────
    private void FireTrigger(TriggerType trigger, CardInstance source, CardInstance subject = null)
    {
        if (source?.Data?.abilities == null) return;
        int scaleFactor = source.mergeLevel + 1;
        for (int i = 0; i < source.Data.abilities.Count; i++)
        {
            var ability = source.Data.abilities[i];
            if (ability == null || ability.trigger != trigger) continue;

            // Đảm bảo mảng đếm đủ phần tử
            while (source.abilityTriggerCounts.Count <= i) source.abilityTriggerCounts.Add(0);

            int effectiveLimit = ability.isScaledTriggerLimit
                ? ability.triggerLimit * scaleFactor : ability.triggerLimit;
            if (effectiveLimit > 0 && source.abilityTriggerCounts[i] >= effectiveLimit) continue;

            // subjectTribe filter cho OnAllyDeploy / OnAllySell
            if (ability.subjectTribe != 0 && subject != null
                && (int)subject.Data.tribe != ability.subjectTribe) continue;

            source.abilityTriggerCounts[i]++;
            if (ability.conditionCount > 0 && source.abilityTriggerCounts[i] % ability.conditionCount != 0) continue;

            foreach (var target in ResolveBotTargets(ability, source, subject))
                ApplyBotEffect(ability, target, source, scaleFactor);
        }
    }

    private List<CardInstance> ResolveBotTargets(AbilityData ability, CardInstance source, CardInstance subject)
    {
        var alive = board.Where(u => u != null && !u.IsDead).ToList();
        switch (ability.target)
        {
            case TargetType.Self:                return new List<CardInstance> { source };
            case TargetType.AllAllies:           return alive.Where(u => u != source).ToList();
            case TargetType.AllAlliesExceptSelf: return alive.Where(u => u != source).ToList();
            case TargetType.TriggerSubject:      return subject != null ? new List<CardInstance> { subject } : new List<CardInstance>();
            case TargetType.AllBabylonAllies:    return alive.Where(u => u.Data.tribe == Tribe.Babylon).ToList();
            case TargetType.AllNilesAllies:      return alive.Where(u => u.Data.tribe == Tribe.Niles).ToList();
            case TargetType.LowestHealthAlly:
            {
                var t = alive.Where(u => u != source).OrderBy(u => u.currentHP).FirstOrDefault();
                return t != null ? new List<CardInstance> { t } : new List<CardInstance>();
            }
            case TargetType.RandomAlly:
            {
                var others = alive.Where(u => u != source).ToList();
                if (others.Count == 0) return new List<CardInstance>();
                return new List<CardInstance> { others[Random.Range(0, others.Count)] };
            }
            default: return new List<CardInstance>();
        }
    }

    private void ApplyBotEffect(AbilityData ability, CardInstance target, CardInstance source, int scaleFactor)
    {
        if (target == null) return;
        switch (ability.effect)
        {
            case EffectType.AddStats:
                if (ability.isPermanent)
                {
                    target.permanentATKBonus += ability.effectValue1 * scaleFactor;
                    target.permanentHPBonus  += ability.effectValue2 * scaleFactor;
                }
                target.ResetStats();
                break;
            case EffectType.GiveBuff:
                if (ability.isReborn)    { target.isReborn = true; target.hasRebornUsed = false; }
                if (ability.isTaunt)     { target.isTaunt = true; target.Data.hasTaunt = true; }
                if (ability.isSafeguard)   target.safeguardActive = true;
                break;
            case EffectType.GainCoin:
                economy.Earn(ability.effectValue1 * scaleFactor);
                break;
        }
    }

    private void BroadcastAllyTrigger(TriggerType trigger, CardInstance subject)
    {
        var snapshot = board.Where(u => u != null && !u.IsDead && u != subject).ToList();
        foreach (var ally in snapshot)
            FireTrigger(trigger, ally, subject);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // EVALUATE
    // ──────────────────────────────────────────────────────────────────────────
    private float Evaluate(CardDefinition c)
    {
        float s = c.baseATK * brain.genes[0]
                + c.baseHP  * brain.genes[1]
                + (c.tier - 1) * brain.genes[2] * 5f;

        if (c.hasTaunt)     s += brain.genes[4] * 10f;
        if (c.hasReborn)    s += brain.genes[5] * 12f;
        if (c.hasSafeguard) s += brain.genes[6] * 8f;

        if (c.abilities != null)
        {
            int sameTribeCount = 0;
            if (c.tribe != Tribe.None)
                foreach (var u in board)
                    if (u != null && !u.IsDead && u.Data.tribe == c.tribe)
                        sameTribeCount++;

            foreach (var a in c.abilities)
            {
                if (a == null) continue;
                float tw = TriggerWeight(a.trigger);
                if (a.trigger == TriggerType.OnAllyDeath ||
                    a.trigger == TriggerType.OnAllySummon ||
                    a.trigger == TriggerType.OnAllyReborn)
                    tw *= Mathf.Clamp01(sameTribeCount / 2f);

                float ew = EffectWeight(a.effect, a.isConsume);
                s += tw * ew * 10f;
                if (a.isEscalating) s += tw * ew * 3f;
            }
        }

        float sw = SynergyWeight(c.tribe);
        if (sw > 0f)
        {
            int same = 0;
            foreach (var u in board)
                if (u != null && !u.IsDead && u.Data.tribe == c.tribe) same++;
            s += same * sw * 4f;
        }

        // Merge proximity — tính cả mergeLevel > 0
        {
            int copies = 0;
            foreach (var u in board)
                if (u != null && !u.IsDead && u.Data.cardID == c.cardID && u.mergeLevel == 0)
                    copies++;
            s += copies * brain.genes[21] * (copies == 2 ? 16f : 8f);
        }

        if (c.hasTaunt)
        {
            int emptyFront = 0;
            for (int i = 0; i < FrontlineCount && i < board.Count; i++)
                if (board[i] == null) emptyFront++;
            s += emptyFront * brain.genes[22] * 2f;
        }

        if (c.cost > 0)
            s = s / c.cost * (1f + brain.genes[3]);

        return s;
    }

    private float EvaluateInstance(CardInstance unit)
    {
        float s = unit.currentATK * brain.genes[0]
                + unit.currentHP  * brain.genes[1]
                + unit.mergeLevel * brain.genes[2] * 5f;
        if (unit.isTaunt)         s += brain.genes[4] * 10f;
        if (unit.isReborn)        s += brain.genes[5] * 12f;
        if (unit.safeguardActive) s += brain.genes[6] * 8f;
        float sw = SynergyWeight(unit.Data.tribe);
        if (sw > 0f)
        {
            int same = 0;
            foreach (var u in board)
                if (u != null && !u.IsDead && u.Data.tribe == unit.Data.tribe) same++;
            s += same * sw * 4f;
        }
        return s;
    }

    private int FindWorstUnitIndex()
    {
        int   worstIdx   = -1;
        float worstScore = float.MaxValue;
        for (int i = 0; i < board.Count; i++)
        {
            if (board[i] == null || board[i].IsDead) continue;
            float sc = EvaluateInstance(board[i]);
            if (sc < worstScore) { worstScore = sc; worstIdx = i; }
        }
        return worstIdx;
    }

    private float TriggerWeight(TriggerType t)
    {
        switch (t)
        {
            case TriggerType.StartOfBattle: return brain.genes[7];
            case TriggerType.OnDeath:       return brain.genes[8];
            case TriggerType.OnAttack:      return brain.genes[9];
            case TriggerType.OnTakeDamage:  return brain.genes[10];
            case TriggerType.EndTurnShop:   return brain.genes[11];
            case TriggerType.OnDeploy:      return brain.genes[12];
            case TriggerType.OnAllyDeath:
            case TriggerType.OnAllySummon:
            case TriggerType.OnAllyReborn:  return brain.genes[34];
            case TriggerType.Aura:          return brain.genes[32];
            case TriggerType.OnSell:        return brain.genes[33];
            case TriggerType.OnAllySell:    return brain.genes[36];
            case TriggerType.OnStatGain:    return brain.genes[13] * 0.4f;
            case TriggerType.OnAllyDeploy:  return brain.genes[35];
            default: return 0f;
        }
    }

    private float EffectWeight(EffectType e, bool isConsume = false)
    {
        switch (e)
        {
            case EffectType.AddStats:        return brain.genes[13];
            case EffectType.Summon:          return brain.genes[14];
            case EffectType.SummonConsumed:  return brain.genes[14] * 1.2f;
            case EffectType.Destroy:
                return isConsume ? brain.genes[14] * 0.7f : brain.genes[15] * 0.8f;
            case EffectType.DealDamage:      return brain.genes[15];
            case EffectType.GainCoin:        return brain.genes[16];
            case EffectType.GiveBuff:        return brain.genes[17];
            case EffectType.Reborn:          return brain.genes[17] * 1.1f;
            case EffectType.TriggerAbility:  return brain.genes[13] * 0.5f;
            case EffectType.AbsorbStats:     return brain.genes[13] * 1.5f;  // hút toàn bộ chỉ số target
            case EffectType.GiveStats:       return brain.genes[13] * 1.2f;
            case EffectType.ScaleTargetStats:return brain.genes[13] * 1.3f;  // nhân chỉ số (Osiris)
            case EffectType.GiveCard:        return brain.genes[14] * 0.8f;
            default: return 0f;
        }
    }

    private float SynergyWeight(Tribe t)
    {
        switch (t)
        {
            case Tribe.Babylon: return brain.genes[18];
            case Tribe.Olympus: return brain.genes[19];
            case Tribe.Niles:   return brain.genes[20];
            default: return 0f;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PLACEMENT HELPERS
    // ──────────────────────────────────────────────────────────────────────────
    private bool BuyAndPlace(CardDefinition c)
    {
        int idx;
        if (c.hasTaunt)
        {
            idx = FindEmptySlot(0, FrontlineCount);
            if (idx < 0) idx = FindEmptySlot(FrontlineCount, board.Count);
        }
        else
        {
            idx = FindEmptySlot(FrontlineCount, board.Count);
            if (idx < 0) idx = FindEmptySlot(0, FrontlineCount);
        }

        if (idx >= 0)
        {
            if (!economy.TryBuy(c.cost)) return false;
            var newUnit = new CardInstance(c, idx);
            board[idx] = newUnit;
            FireTrigger(TriggerType.OnDeploy, newUnit);
            BroadcastAllyTrigger(TriggerType.OnAllyDeploy, newUnit);
            return true;
        }

        return false;
    }

    // Mô phỏng hiệu ứng "Tinh Hoa Hợp Nhất": đặt 1 unit ngẫu nhiên Tier+1 lên board
    private void SimulateMergeReward()
    {
        if (!HasEmptySlot()) return;
        int targetTier = Mathf.Min(_shopTier + 1, 6);
        var pool = CardDatabase.Instance?.GetAllUnits().FindAll(c => c.tier == targetTier);
        if (pool == null || pool.Count == 0) return;
        PlaceOnBoard(new CardInstance(pool[Random.Range(0, pool.Count)], 0));
    }

    private void PlaceOnBoard(CardInstance unit)
    {
        int idx = FindEmptySlot(0, board.Count);
        if (idx < 0) return;
        unit.slotIndex = idx;
        board[idx] = unit;
        FireTrigger(TriggerType.OnDeploy, unit);
        BroadcastAllyTrigger(TriggerType.OnAllyDeploy, unit);
    }

    private int FindEmptySlot(int from, int to)
    {
        for (int i = from; i < to && i < board.Count; i++)
            if (board[i] == null) return i;
        return -1;
    }

    private bool HasEmptySlot() => board.Exists(s => s == null);
}
