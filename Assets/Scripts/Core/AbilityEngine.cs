using System.Collections.Generic;
using UnityEngine;

// TTE Engine: Trigger → Target → Effect
public partial class AbilityEngine
{
    private static void CLog(string m)  { if (!Application.isBatchMode) Debug.Log(m); }
    private static void CLogW(string m) { if (!Application.isBatchMode) Debug.LogWarning(m); }

    private System.Action<CardInstance, List<CardInstance>> onUnitSummoned;
    private System.Action<CardInstance, List<CardInstance>, FlashType> onStatChanged;

    public void SetSummonObserver(System.Action<CardInstance, List<CardInstance>> observer)
        => onUnitSummoned = observer;

    public void SetStatChangeObserver(System.Action<CardInstance, List<CardInstance>, FlashType> observer)
        => onStatChanged = observer;

    // Stack-based summon queue: mỗi unit chỉ summon 1 lần ngay lập tức,
    // các unit còn lại trong batch đợi CombatResolver.FlushDeathStack pop ra
    // SAU KHI death chain của unit trước đó resolve hoàn toàn.
    private struct PendingSummonEntry
    {
        public string cardID;
        public List<CardInstance> allyBoard;
        public List<CardInstance> enemyBoard;
    }
    private readonly Queue<PendingSummonEntry> _pendingSummons = new Queue<PendingSummonEntry>();

    // Guard: prevents AddStats from firing OnStatGain recursively when Gilgamesh reacts to its own gain
    private bool _firingOnStatGain = false;

    public bool HasPendingSummons => _pendingSummons.Count > 0;
    public void ClearPendingSummons() => _pendingSummons.Clear();

    public void ProcessNextPendingSummon()
    {
        if (_pendingSummons.Count == 0) return;
        var e = _pendingSummons.Dequeue();
        CardInstance summoned = SummonUnit(e.cardID, e.allyBoard);
        if (summoned == null)
        {
            CLog($"<color=gray>[SUMMON]</color> Pending {e.cardID}: hết slot, unit biến mất theo stack.");
            return;
        }
        CLog($"<color=green>[SUMMON]</color> (pending) triệu hồi {summoned.Data.cardName}!");
        TriggerAbility(TriggerType.OnDeploy, summoned, null, e.allyBoard, e.enemyBoard);
        BroadcastAllyEvent(TriggerType.OnAllySummon, summoned, e.allyBoard, e.enemyBoard);
    }


    public void TriggerAbility(TriggerType triggerContext, CardInstance source, CardInstance directEnemy, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        if (source == null || source.Data.abilities == null || source.Data.abilities.Count == 0) return;

        int scaleFactor = source.mergeLevel + 1;

        for (int i = 0; i < source.Data.abilities.Count; i++)
        {
            AbilityData ability = source.Data.abilities[i];
            if (ability == null || ability.trigger != triggerContext) continue;

            int effectiveLimit = ability.isScaledTriggerLimit
                ? ability.triggerLimit * scaleFactor : ability.triggerLimit;
            if (effectiveLimit > 0 && source.abilityTriggerCounts[i] >= effectiveLimit) continue;

            // subjectTribe filter: OnAllySell / OnAllyDeploy chỉ phản ứng đúng bộ tộc (0 = bất kỳ)
            if (ability.subjectTribe != 0
                && (ability.trigger == TriggerType.OnAllySell || ability.trigger == TriggerType.OnAllyDeploy)
                && (directEnemy == null || (int)directEnemy.Data.tribe != ability.subjectTribe)) continue;

            // StartOfBattle + AddStats = Growth (tăng trưởng vĩnh viễn)
            bool isGrowth = triggerContext == TriggerType.StartOfBattle && ability.effect == EffectType.AddStats;

            List<CardInstance> targets = FindTargets(ability, source, directEnemy, allyBoard, enemyBoard);
            int esc = (ability.isEscalating && i < source.abilityEscalationBonuses.Count)
                      ? source.abilityEscalationBonuses[i] : 0;

            // triggerLimit: chỉ đếm khi thực sự có mục tiêu hợp lệ.
            // Tránh tình huống unit thứ hai (SekhmetB) bị trừ lượt khi mục tiêu đã bị
            // unit thứ nhất (SekhmetA) consume mất — TriggerSubject trả rỗng → skip, không mất count.
            // conditionCount: luôn đếm (semantics = "fire mỗi N lần trigger").
            if (ability.triggerLimit > 0)
            {
                if (targets.Count == 0) continue;
                source.abilityTriggerCounts[i]++;
            }
            else
            {
                source.abilityTriggerCounts[i]++;
                // conditionCount > 0: chỉ thực thi vào đúng lần thứ N, 2N, 3N...
                if (ability.conditionCount > 0 && source.abilityTriggerCounts[i] % ability.conditionCount != 0) continue;
            }

            foreach (var target in targets)
            {
                if (isGrowth) ApplyGrowth(ability, target, scaleFactor, esc, allyBoard);
                else ExecuteEffect(ability, target, source, allyBoard, enemyBoard, scaleFactor, directEnemy, esc);
            }

            // Escalating: tăng effectValue mỗi lần fire — dùng counter riêng, không mutate AbilityData
            if (ability.isEscalating && i < source.abilityEscalationBonuses.Count)
                source.abilityEscalationBonuses[i]++;
        }
    }

    private void ApplyGrowth(AbilityData ability, CardInstance target, int scaleFactor, int escalationBonus = 0, List<CardInstance> allyBoard = null)
    {
        if (target == null || target.IsDead) return;
        int atkGain = (ability.effectValue1 + escalationBonus) * scaleFactor;
        int hpGain  = (ability.effectValue2 + escalationBonus) * scaleFactor;
        target.growthATKBonus += atkGain;
        target.growthHPBonus  += hpGain;
        // BUG FIX: Cộng dồn vào currentHP/ATK hiện tại thay vì recalculate từ base,
        // giữ nguyên bonus từ tribe synergy và các buff trước đó trong cùng combat.
        target.currentATK += atkGain;
        target.currentHP  += hpGain;
        CLog($"<color=lime>[GROWTH]</color> {target.Data.cardName} tăng trưởng +{atkGain}ATK/+{hpGain}HP (tổng: +{target.growthATKBonus}/+{target.growthHPBonus})");
        onStatChanged?.Invoke(target, allyBoard, FlashType.Buff);
    }

    private void ExecuteEffect(AbilityData ability, CardInstance target, CardInstance source,
        List<CardInstance> allyBoard, List<CardInstance> enemyBoard, int scaleFactor = 1, CardInstance directEnemy = null, int escalationBonus = 0)
    {
        if (target == null) return;

        // Reborn, Summon, SummonConsumed, GiveBuff được phép thực thi dù target đã chết.
        // GiveBuff vào target dead: nếu grant Reborn → CleanupBoard sẽ revive;
        // nếu grant Taunt/Safeguard → được reset ở ResetStats() đầu turn sau, không gây hại.
        bool isDeathSafeEffect = ability.effect == EffectType.Reborn
                              || ability.effect == EffectType.Summon
                              || ability.effect == EffectType.SummonConsumed
                              || ability.effect == EffectType.GiveBuff;
        if (target.IsDead && !isDeathSafeEffect) return;

        switch (ability.effect)
        {
            case EffectType.AddStats:
            {
                int atk = (ability.effectValue1 + escalationBonus) * scaleFactor;
                int hp  = (ability.effectValue2 + escalationBonus) * scaleFactor;
                target.currentATK += atk;
                target.currentHP  += hp;
                if (ability.isPermanent)
                {
                    target.permanentATKBonus += atk;
                    target.permanentHPBonus  += hp;
                    // BUG FIX: Giữ maxHP đồng bộ với permanent HP buff trong combat.
                    // Không cập nhật → IsDamaged sai, heal-to-max tính thiếu HP.
                    if (hp > 0) target.maxHP += hp;
                }
                CLog($"<color=cyan>[ABILITY]</color> {target.Data.cardName} được buff +{atk}ATK / +{hp}HP");
                onStatChanged?.Invoke(target, allyBoard, (atk > 0 || hp > 0) ? FlashType.Buff : FlashType.Debuff);
                // Kích hoạt OnStatGain sau visual callback để Gilgamesh flash theo đúng thứ tự
                if (ability.isPermanent && (atk > 0 || hp > 0) && !_firingOnStatGain)
                {
                    _firingOnStatGain = true;
                    try { TriggerAbility(TriggerType.OnStatGain, target, directEnemy, allyBoard, enemyBoard); }
                    finally { _firingOnStatGain = false; }
                }
                break;
            }

            case EffectType.GiveBuff:
            {
                if (ability.isTaunt)      { target.isTaunt = true;         CLog($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Taunt!"); }
                if (ability.isReborn)     { target.isReborn = true; target.hasRebornUsed = false; CLog($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Reborn!"); }
                if (ability.isSafeguard)  { target.safeguardActive = true; CLog($"<color=cyan>[BUFF]</color> {target.Data.cardName} nhận Safeguard!"); }
                onStatChanged?.Invoke(target, allyBoard, FlashType.Status);
                break;
            }

            case EffectType.DealDamage:
            {
                if (target.safeguardActive)
                {
                    target.safeguardActive = false;
                    CLog($"<color=cyan>[SAFEGUARD]</color> {target.Data.cardName} chặn sát thương kỹ năng!");
                    onStatChanged?.Invoke(target, allyBoard, FlashType.Status);
                    break;
                }
                int dmg = ability.effectValue1 * scaleFactor;
                target.currentHP -= dmg;
                // Track kẻ gây đòn chết để death stack dùng làm directEnemy trong OnDeath
                if (target.IsDead) target.lastAttacker = source;
                CLog($"<color=orange>[ABILITY]</color> {target.Data.cardName} chịu {dmg} sát thương kỹ năng");
                onStatChanged?.Invoke(target, allyBoard, FlashType.Debuff);
                break;
            }

            case EffectType.Destroy:
                target.currentHP = 0;
                target.lastAttacker = source;
                if (ability.isConsume)
                {
                    source.consumedCardIDs.Add(target.Data.cardID);
                    // BUG FIX: Unit bị Consume không được phép hồi sinh qua Reborn passive.
                    // Nếu không chặn, CleanupBoard sẽ revive unit đó → unit vừa sống trên board
                    // vừa nằm trong consumedCardIDs, gây nhân bản khi consumer chết.
                    target.hasRebornUsed = true;
                }
                CLog($"<color=red>[ABILITY]</color> {source.Data.cardName} tiêu diệt {target.Data.cardName}{(ability.isConsume ? " (Consumed!)" : "")}");
                break;

            case EffectType.Reborn:
                if (!target.hasRebornUsed)
                {
                    // Mathf.Max(1,...): đảm bảo reviveHP > 0 tránh IsDead ngay sau revive → OnDeath fire 2 lần
                    int reviveHP = Mathf.Max(1, ability.effectValue1 * scaleFactor);
                    target.Revive(reviveHP);
                    CLog($"<color=magenta>[ABILITY]</color> {target.Data.cardName} HỒI SINH với {reviveHP} HP!");
                    BroadcastAllyEvent(TriggerType.OnAllyReborn, target, allyBoard, enemyBoard);
                }
                break;

            case EffectType.Summon:
            {
                int summonCount = (ability.effectValue1 > 0 ? ability.effectValue1 : 1) * scaleFactor;
                bool isShopPhase = GameManager.Instance != null && !GameManager.Instance.isCombatActive;

                if (isShopPhase)
                {
                    // Shop phase: thêm vào Hand thay vì Board trực tiếp
                    for (int s = 0; s < summonCount; s++)
                    {
                        CardDefinition toAdd;
                        if (string.IsNullOrEmpty(ability.summonCardID))
                        {
                            // Ngẫu nhiên unit: tier >= tier của source, lọc theo targetTribe nếu có
                            var pool = CardDatabase.Instance.GetAllUnits()
                                .FindAll(c => c.tier >= source.Data.tier
                                    && (ability.targetTribe == 0 || (int)c.tribe == ability.targetTribe));
                            if (pool.Count == 0)
                                pool = CardDatabase.Instance.GetAllUnits()
                                    .FindAll(c => ability.targetTribe == 0 || (int)c.tribe == ability.targetTribe);
                            if (pool.Count == 0) pool = CardDatabase.Instance.GetAllUnits();
                            toAdd = pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
                        }
                        else
                        {
                            toAdd = CardDatabase.Instance?.GetCard(ability.summonCardID);
                        }
                        if (toAdd != null)
                        {
                            GameManager.Instance.AddUnitToHand(toAdd);
                            CLog($"<color=green>[SUMMON]</color> {source.Data.cardName} thêm {toAdd.cardName} vào Hand!");
                        }
                    }
                }
                else
                {
                    // Combat phase: triệu hồi trực tiếp lên board (behavior gốc)
                    CardInstance first = SummonUnit(ability.summonCardID, allyBoard);
                    if (first != null)
                    {
                        CLog($"<color=green>[SUMMON]</color> {source.Data.cardName} triệu hồi {first.Data.cardName} (1/{summonCount})!");
                        TriggerAbility(TriggerType.OnDeploy, first, null, allyBoard, enemyBoard);
                        BroadcastAllyEvent(TriggerType.OnAllySummon, first, allyBoard, enemyBoard);
                    }
                    // Stack-based summon: các unit tiếp theo vào pending queue để tránh chain nuốt nhau
                    for (int s = 1; s < summonCount; s++)
                        _pendingSummons.Enqueue(new PendingSummonEntry
                            { cardID = ability.summonCardID, allyBoard = allyBoard, enemyBoard = enemyBoard });
                }
                break;
            }

            case EffectType.GainCoin:
            {
                int coins = ability.effectValue1 * scaleFactor;
                GameManager.Instance?.AddCoin(coins); // null-safe: GameManager không tồn tại khi training standalone
                CLog($"<color=yellow>[ABILITY]</color> {source.Data.cardName} cộng {coins} Coin! (Deploy)");
                break;
            }

            case EffectType.TriggerAbility:
                // Replay OnDeath abilities của target scaleFactor lần
                // Guard: không cho phép OnDeath abilities có effect TriggerAbility gây chain vô hạn
                if (target.Data.abilities != null)
                {
                    for (int r = 0; r < scaleFactor; r++)
                        TriggerAbility(TriggerType.OnDeath, target, directEnemy, allyBoard, enemyBoard);
                }
                break;

            case EffectType.GiveCard:
            {
                CardDefinition cardDef = CardDatabase.Instance?.GetCard(ability.summonCardID);
                if (cardDef != null)
                    GameManager.Instance?.AddUnitToHand(cardDef);
                else
                    CLogW($"[ABILITY] GiveCard: không tìm thấy card '{ability.summonCardID}'");
                break;
            }

            case EffectType.AbsorbStats:
            {
                // source hấp thụ toàn bộ chỉ số hiện tại của target (vĩnh viễn)
                int gainATK = target.currentATK;
                int gainHP  = target.maxHP;
                source.currentATK        += gainATK;
                source.currentHP         += gainHP;
                source.maxHP             += gainHP;
                source.permanentATKBonus += gainATK;
                source.permanentHPBonus  += gainHP;
                // isConsume: phá hủy target sau khi hấp thụ (VD: Asag devour đồng minh 2 bên)
                if (ability.isConsume && !target.IsDead)
                {
                    target.currentHP     = 0;
                    target.lastAttacker  = source;
                    target.hasRebornUsed = true; // chặn Reborn của unit bị tiêu thụ
                }
                CLog($"<color=magenta>[ABSORB]</color> {source.Data.cardName} hấp thụ +{gainATK}ATK/+{gainHP}HP từ {target.Data.cardName}{(ability.isConsume ? " (devoured)" : "")}");
                onStatChanged?.Invoke(source, allyBoard, FlashType.Buff);
                if (!_firingOnStatGain)
                {
                    _firingOnStatGain = true;
                    try { TriggerAbility(TriggerType.OnStatGain, source, directEnemy, allyBoard, enemyBoard); }
                    finally { _firingOnStatGain = false; }
                }
                break;
            }

            case EffectType.GiveStats:
            {
                // target nhận toàn bộ chỉ số hiện tại của source (vĩnh viễn)
                int giveATK = source.currentATK;
                int giveHP  = source.maxHP;
                target.currentATK        += giveATK;
                target.currentHP         += giveHP;
                target.maxHP             += giveHP;
                target.permanentATKBonus += giveATK;
                target.permanentHPBonus  += giveHP;
                CLog($"<color=magenta>[GIVE]</color> {source.Data.cardName} trao +{giveATK}ATK/+{giveHP}HP cho {target.Data.cardName}");
                onStatChanged?.Invoke(target, allyBoard, FlashType.Buff);
                if (!_firingOnStatGain)
                {
                    _firingOnStatGain = true;
                    try { TriggerAbility(TriggerType.OnStatGain, target, directEnemy, allyBoard, enemyBoard); }
                    finally { _firingOnStatGain = false; }
                }
                break;
            }

            case EffectType.SummonConsumed:
                if (source.consumedCardIDs != null && source.consumedCardIDs.Count > 0)
                {
                    CLog($"<color=magenta>[CONSUME]</color> {source.Data.cardName} giải phóng {source.consumedCardIDs.Count} unit đã tiêu thụ!");
                    foreach (var cardID in source.consumedCardIDs)
                    {
                        CardInstance released = SummonUnit(cardID, allyBoard);
                        if (released != null)
                        {
                            TriggerAbility(TriggerType.OnDeploy, released, null, allyBoard, enemyBoard);
                            BroadcastAllyEvent(TriggerType.OnAllySummon, released, allyBoard, enemyBoard);
                        }
                    }
                    source.consumedCardIDs.Clear();
                }
                break;
        }
    }

    public void BroadcastAllyEvent(TriggerType context, CardInstance subject, List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
    {
        // Snapshot tránh unit được summon trong broadcast nhận event của chính lần summon đó
        var snapshot = new List<CardInstance>(allyBoard);
        foreach (var unit in snapshot)
        {
            if (unit == null || unit.IsDead || unit == subject) continue;
            // BUG FIX: OnAllySummon / OnAllyReborn chỉ kích hoạt khi đồng minh CÙNG BỘ TỘC liên quan
            // (theo đặc tả trong AbilityData.cs enum comment)
            if ((context == TriggerType.OnAllySummon || context == TriggerType.OnAllyReborn)
                && unit.Data.tribe != subject.Data.tribe) continue;
            TriggerAbility(context, unit, subject, allyBoard, enemyBoard);
        }
    }

    private CardInstance SummonUnit(string cardID, List<CardInstance> board)
    {
        CardDefinition data = CardDatabase.Instance.GetCard(cardID);
        if (data == null) return null;

        // Ưu tiên 1: slot trống hoàn toàn
        for (int i = 0; i < board.Count; i++)
        {
            if (board[i] == null)
            {
                board[i] = new CardInstance(data, i) { isBattleSpawned = true };
                CLog($"<color=green>[SUMMON]</color> Đã triệu hồi {data.cardName} vào slot {i}");
                onUnitSummoned?.Invoke(board[i], board);
                return board[i];
            }
        }

        // Ưu tiên 2: slot dead + đã xử lý OnDeath + KHÔNG có Reborn chờ
        // (tránh overwrite unit đang chờ Reborn trong CleanupBoard)
        for (int i = 0; i < board.Count; i++)
        {
            if (board[i] != null && board[i].IsDead && board[i].onDeathProcessed && !board[i].isReborn)
            {
                board[i] = new CardInstance(data, i) { isBattleSpawned = true };
                CLog($"<color=green>[SUMMON]</color> Đã triệu hồi {data.cardName} vào slot {i} (thay chỗ dead)");
                onUnitSummoned?.Invoke(board[i], board);
                return board[i];
            }
        }

        return null;
    }

    public void ApplySpellToUnit(CardInstance spell, CardInstance targetUnit)
    {
        if (spell == null) return;
        if (spell.Data.spellEffects == null || spell.Data.spellEffects.Count == 0)
        {
            CLogW($"[SPELL] {spell.Data.cardName} không có spellEffects.");
            return;
        }

        foreach (var fx in spell.Data.spellEffects)
            ApplySpellEffect(fx, targetUnit, spell);

        if (targetUnit != null) targetUnit.ResetStats();
        CLog($"<color=cyan>[SPELL]</color> Đã dùng phép {spell.Data.cardName}{(targetUnit != null ? " lên " + targetUnit.Data.cardName : "")}.");
    }

    private void ApplySpellEffect(SpellEffectData fx, CardInstance targetUnit, CardInstance spell)
    {
        switch (fx.effect)
        {
            case 1: // BuffStats
            {
                var targets = ResolveSpellTargets(fx.target, fx.targetCount, targetUnit);
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
                        // Buff tạm thời: lưu vào tempSpell thay vì cộng thẳng vào currentATK/HP.
                        // ResetStats() trong ApplySpellToUnit sẽ áp dụng rồi xóa temp → buff sống
                        // qua combat, bị xóa bởi ResetStats() sau trận (RestorePreCombatPlayerBoard).
                        t.tempSpellATKBonus += fx.effectValue1;
                        t.tempSpellHPBonus  += fx.effectValue2;
                    }
                    CLog($"<color=cyan>[SPELL]</color> {t.Data.cardName} nhận +{fx.effectValue1}/+{fx.effectValue2}{(fx.isTaunt ? " + Taunt" : "")}");
                }
                break;
            }

            case 6: // GainCoin
                GameManager.Instance.AddCoin(fx.effectValue1);
                CLog($"<color=yellow>[SPELL]</color> Nhận {fx.effectValue1} đồng.");
                break;

            case 10: // GetRandomUnit — nhận N unit ngẫu nhiên Tier X vào Hand
            {
                int tier  = fx.effectValue1;
                int count = Mathf.Max(fx.targetCount, 1);
                var pool  = CardDatabase.Instance.GetAllUnits().FindAll(c => c.tier == tier);
                if (pool.Count == 0)
                    pool = CardDatabase.Instance.GetAllUnits().FindAll(c => c.tier <= Mathf.Max(tier, 1));
                for (int i = 0; i < count; i++)
                    if (pool.Count > 0)
                        GameManager.Instance.AddUnitToHand(pool[Random.Range(0, pool.Count)]);
                break;
            }

            case 11: // StealFromShop — lấy unit ngẫu nhiên từ Shop hiện tại
                GameManager.Instance.StealRandomUnitFromShop();
                break;

            case 12: // GainIncome — tăng thu nhập (vĩnh viễn hoặc tạm thời)
                if (fx.isPermanent)
                    GameManager.Instance.AddPermanentIncome(fx.effectValue1);
                else
                    GameManager.Instance.AddCoin(fx.effectValue1);
                CLog($"<color=yellow>[SPELL]</color> Thu nhập {(fx.isPermanent ? "vĩnh viễn" : "tạm thời")} +{fx.effectValue1}.");
                break;

            case 13: // GetSameRealmUnit — nhận unit cùng Tộc với unit đã chọn
            {
                if (targetUnit == null) break;
                Tribe tribe = targetUnit.Data.tribe;
                var pool = CardDatabase.Instance.GetAllUnits()
                    .FindAll(c => c.tribe == tribe && c.cardID != targetUnit.Data.cardID);
                if (pool.Count > 0)
                    GameManager.Instance.AddUnitToHand(pool[Random.Range(0, pool.Count)]);
                else
                    CLogW($"[SPELL] Không tìm thấy unit cùng Tộc {tribe} để thêm vào Hand.");
                break;
            }

            case 14: // LoseLife
                GameManager.Instance.LoseLife(fx.effectValue1);
                break;

            case 15: // TransferStats — hủy unit đã chọn, chuyển chỉ số cho đồng minh ngẫu nhiên
                GameManager.Instance.TransferStatsToRandom(targetUnit);
                break;

            case 16: // ConditionalCoinGain — thắng trận kế tiếp nhận coin
                GameManager.Instance.RegisterWagerReward(fx.effectValue1);
                break;

            case 17: // UpgradeTierUnit — nâng cấp 1 unit cùng Tộc lên +1 sao
                GameManager.Instance.UpgradeSameTribeUnit(targetUnit);
                break;

            case 18: // GiveDoubleAtkAndSafeguard — nhân đôi ATK + Safeguard
            {
                if (targetUnit == null) break;
                targetUnit.permanentATKBonus += targetUnit.currentATK;
                targetUnit.Data.hasSafeguard  = true; // tồn tại qua ResetStats (Data đã clone)
                CLog($"<color=cyan>[SPELL]</color> {targetUnit.Data.cardName}: ATK x2 + Safeguard.");
                break;
            }

            case 19: // ToggleTaunt
            {
                if (targetUnit == null) break;
                if (targetUnit.isTaunt)
                {
                    targetUnit.isTaunt       = false;
                    targetUnit.Data.hasTaunt = false;
                    targetUnit.currentATK        += fx.effectValue1;
                    targetUnit.permanentATKBonus += fx.effectValue1;
                    CLog($"<color=cyan>[SPELL]</color> {targetUnit.Data.cardName}: xóa Taunt + +{fx.effectValue1} ATK.");
                }
                else
                {
                    targetUnit.isTaunt       = true;
                    targetUnit.Data.hasTaunt = true;
                    targetUnit.currentHP         += fx.effectValue2;
                    targetUnit.permanentHPBonus  += fx.effectValue2;
                    targetUnit.maxHP             += fx.effectValue2;
                    CLog($"<color=cyan>[SPELL]</color> {targetUnit.Data.cardName}: nhận Taunt + +{fx.effectValue2} HP.");
                }
                break;
            }

            case 20: // BuffByShopTier — ATK và HP = Tier Shop hiện tại
            {
                if (targetUnit == null) break;
                int shopTier = GameManager.Instance.GetCurrentShopTier();
                targetUnit.permanentATKBonus += shopTier;
                targetUnit.permanentHPBonus  += shopTier;
                CLog($"<color=cyan>[SPELL]</color> {targetUnit.Data.cardName}: +{shopTier}/+{shopTier} (Shop Tier {shopTier}).");
                break;
            }

            case 21: // GiveEndTurnBuff — ban khả năng nhận buff cuối mỗi lượt Shop
            {
                if (targetUnit == null) break;
                if (targetUnit.Data.abilities == null)
                    targetUnit.Data.abilities = new System.Collections.Generic.List<AbilityData>();
                targetUnit.Data.abilities.Add(new AbilityData
                {
                    trigger      = TriggerType.EndTurnShop,
                    target       = TargetType.Self,
                    effect       = EffectType.AddStats,
                    effectValue1 = fx.effectValue1,
                    effectValue2 = fx.effectValue2,
                    isPermanent  = true
                });
                targetUnit.abilityTriggerCounts.Add(0);
                targetUnit.abilityEscalationBonuses.Add(0);
                CLog($"<color=cyan>[SPELL]</color> {targetUnit.Data.cardName}: nhận buff +{fx.effectValue1}/+{fx.effectValue2} mỗi cuối lượt Shop.");
                break;
            }

            default:
                CLogW($"[SPELL] Effect {fx.effect} của '{spell.Data.cardName}' chưa được triển khai.");
                break;
        }
    }

    private List<CardInstance> ResolveSpellTargets(int targetCode, int targetCount, CardInstance chosenUnit)
    {
        var list = new List<CardInstance>();
        int n = Mathf.Max(targetCount, 1);

        switch (targetCode)
        {
            case 12: // ChosenAlly
                if (chosenUnit != null) list.Add(chosenUnit);
                break;

            case 3: // AllAllies
            {
                var board = GameManager.Instance?.playerBoard;
                if (board != null) list.AddRange(board.FindAll(u => u != null && !u.IsDead));
                break;
            }

            case 2:  // RandomAlly
            case 13: // RandomAlliesInBattle (N ngẫu nhiên)
            {
                var board = GameManager.Instance?.playerBoard;
                if (board == null) break;
                var alive = board.FindAll(u => u != null && !u.IsDead);
                // Fisher-Yates shuffle rồi lấy n đầu
                for (int i = alive.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (alive[i], alive[j]) = (alive[j], alive[i]);
                }
                list.AddRange(alive.GetRange(0, Mathf.Min(n, alive.Count)));
                break;
            }

            default:
                if (chosenUnit != null) list.Add(chosenUnit);
                break;
        }
        return list;
    }
}
