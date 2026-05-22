using System.Collections.Generic;
using UnityEngine;

public class BotAgent
{
    public Chromosome brain;
    public EconomyManager economy;
    public List<CardInstance> board = new List<CardInstance>(new CardInstance[GameManager.BoardSlotCount]);

    private const int FrontlineCount = 4;

    // startingCoins: Easy=7, Medium=9, Hard=10 — handicap cân bằng độ khó
    public int startingCoins = 10;

    public BotAgent(Chromosome brain, int coinsPerTurn = 10)
    {
        this.brain        = brain;
        this.economy      = new EconomyManager();
        this.startingCoins = coinsPerTurn;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // END COMBAT PHASE — gọi sau mỗi trận:
    //   • Xóa unit chết khỏi slot (giải phóng chỗ trống)
    //   • ResetStats unit sống (khôi phục HP đầy cho trận sau)
    // ──────────────────────────────────────────────────────────────────────────
    public void EndCombatPhase()
    {
        for (int i = 0; i < board.Count; i++)
        {
            var unit = board[i];
            if (unit == null) continue;
            if (unit.IsDead)
                board[i] = null;
            else
                unit.ResetStats();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // END TURN SHOP — gọi khi kết thúc lượt chuẩn bị:
    //   Áp dụng hiệu ứng EndTurnShop (growth, coin…) cho unit còn sống.
    //   Chỉ xử lý AddStats và GainCoin — đủ cho cơ chế growth cốt lõi.
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
    // PREP PHASE — mua bài từ shop, điền vào slot trống.
    //   Board KHÔNG bị reset — chỉ slot null mới được điền.
    //   Nếu board đầy: cân nhắc bán unit yếu nhất để mua unit tốt hơn.
    //   Sau khi mua: thử merge nếu đủ 3 bản sao.
    // ──────────────────────────────────────────────────────────────────────────
    public void DecidePrepPhase(List<CardDefinition> shop)
    {
        if (brain == null) return;
        economy.ResetEconomy();
        // Handicap: Easy/Medium nhận ít coin hơn Hard
        int coinDiff = 10 - startingCoins;
        if (coinDiff > 0) economy.TrySpend(coinDiff);

        bool bought = true;
        while (bought)
        {
            bought = false;
            CardDefinition bestCard  = null;
            float          bestScore = float.MinValue;

            foreach (var card in shop)
            {
                if (card == null || card.cost > economy.CurrentCoin) continue;
                float score = Evaluate(card);
                if (score > bestScore) { bestScore = score; bestCard = card; }
            }

            float saveThreshold = brain.genes[23] * 3f;
            if (bestCard == null || bestScore < saveThreshold) break;

            // Nếu board đầy: bán unit yếu nhất nếu card mới vượt trội đủ nhiều
            if (!HasEmptySlot())
            {
                int   worstIdx   = FindWorstUnitIndex();
                float worstScore = worstIdx >= 0 ? EvaluateInstance(board[worstIdx]) : float.MaxValue;
                // gene[23] làm ngưỡng quyết định bán: cao hơn → khó bán hơn
                float sellBar = worstScore * (1.5f + brain.genes[23]);
                if (worstIdx >= 0 && bestScore > sellBar)
                {
                    board[worstIdx] = null;
                    economy.Sell();
                }
            }

            if (!HasEmptySlot()) break;

            BuyAndPlace(bestCard);
            bought = true;
        }

        TryMerge();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MERGE — gộp 3 bản sao cùng cardID + mergeLevel thành 1 unit cấp cao hơn
    // ──────────────────────────────────────────────────────────────────────────
    private void TryMerge()
    {
        bool merged = true;
        while (merged) // lặp để bắt merge chain (star-1 → star-2 → star-3)
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
                        && board[j].Data.cardID  == unit.Data.cardID
                        && board[j].mergeLevel   == unit.mergeLevel)
                        copies.Add(j);
                }

                if (copies.Count < 3) continue;

                // Giữ bản sao có bonus cao nhất
                int keepIdx = copies[0];
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

                merged = true;
                break; // restart scan sau mỗi merge
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // EVALUATE CardDefinition — điểm kỳ vọng khi mua card này
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

                if (a.trigger == TriggerType.OnAllyDeath  ||
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

        // Merge proximity
        {
            int copies = 0;
            foreach (var u in board)
                if (u != null && !u.IsDead && u.Data.cardID == c.cardID && u.mergeLevel == 0)
                    copies++;
            s += copies * brain.genes[21] * (copies == 2 ? 16f : 8f);
        }

        // Frontline bias
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

    // ──────────────────────────────────────────────────────────────────────────
    // EVALUATE CardInstance — điểm thực tế của unit đang có trên sân
    // Dùng cho quyết định bán: so sánh unit cũ vs card mới
    // ──────────────────────────────────────────────────────────────────────────
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
            case TriggerType.OnAllyReborn:  return brain.genes[12] * 0.8f;
            case TriggerType.Aura:          return brain.genes[7]  * 0.6f;
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

    private void BuyAndPlace(CardDefinition c)
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
            board[idx] = new CardInstance(c, idx);
            economy.TryBuy(c.cost);
        }
    }

    private int FindEmptySlot(int from, int to)
    {
        for (int i = from; i < to && i < board.Count; i++)
            if (board[i] == null) return i;
        return -1;
    }

    private bool HasEmptySlot() => board.Exists(s => s == null);
}
