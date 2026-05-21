using System.Collections.Generic;
using UnityEngine;

public class BotAgent
{
    public Chromosome brain;
    public EconomyManager economy;
    public List<CardInstance> board = new List<CardInstance>(new CardInstance[GameManager.BoardSlotCount]);

    private const int FrontlineCount = 4; // slot 0-3 frontline, 4-6 backline

    public BotAgent(Chromosome brain)
    {
        this.brain = brain;
        this.economy = new EconomyManager();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PREP PHASE: Mua bài tốt nhất cho đến khi hết tiền / hết chỗ / điểm quá thấp
    // ──────────────────────────────────────────────────────────────────────────
    public void DecidePrepPhase(List<CardDefinition> shop)
    {
        economy.ResetEconomy();

        bool bought = true;
        while (bought)
        {
            bought = false;
            CardDefinition bestCard = null;
            float bestScore = float.MinValue;

            foreach (var card in shop)
            {
                if (card == null || card.cost > economy.CurrentCoin) continue;
                float score = Evaluate(card);
                if (score > bestScore) { bestScore = score; bestCard = card; }
            }

            // genes[23]: ngưỡng tiết kiệm — nếu điểm tốt nhất quá thấp, dừng mua
            float saveThreshold = brain.genes[23] * 3f;
            if (bestCard == null || bestScore < saveThreshold || !HasEmptySlot()) break;

            BuyAndPlace(bestCard);
            bought = true;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // EVALUATE: Tính điểm một card dựa trên 24 genes
    // ──────────────────────────────────────────────────────────────────────────
    private float Evaluate(CardDefinition c)
    {
        // ── Nhóm 1: Chỉ số gốc ────────────────────────────────────────────────
        float s = c.baseATK * brain.genes[0]
                + c.baseHP  * brain.genes[1]
                + (c.tier - 1) * brain.genes[2] * 5f;

        // ── Nhóm 2: Passives ──────────────────────────────────────────────────
        if (c.hasTaunt)     s += brain.genes[4] * 10f;
        if (c.hasReborn)    s += brain.genes[5] * 12f;
        if (c.hasSafeguard) s += brain.genes[6] * 8f;

        // ── Nhóm 3+4: Abilities — trigger_weight × effect_weight × 10 ─────────
        // Tách biệt trigger và effect cho phép GA khám phá tổ hợp phức tạp hơn.
        if (c.abilities != null)
        {
            foreach (var a in c.abilities)
            {
                if (a == null) continue;
                float tw = TriggerWeight(a.trigger);
                float ew = EffectWeight(a.effect);
                s += tw * ew * 10f;

                // Escalating ability: thêm bonus vì giá trị tăng dần theo trận
                if (a.isEscalating) s += tw * ew * 3f;
            }
        }

        // ── Nhóm 5: Tribe synergy ─────────────────────────────────────────────
        float sw = SynergyWeight(c.tribe);
        if (sw > 0f)
        {
            int same = 0;
            foreach (var u in board)
                if (u != null && !u.IsDead && u.Data.tribe == c.tribe) same++;

            // Babylon/Olympus kích hoạt ≥2, Niles ≥3 → tính theo proximity
            s += same * sw * 4f;
        }

        // ── Nhóm 6a: Merge bonus ─────────────────────────────────────────────
        {
            int copies = 0;
            foreach (var u in board)
                if (u != null && !u.IsDead && u.Data.cardID == c.cardID
                    && u.mergeLevel == 0) // chỉ tính star-1 để merge lên star-2
                    copies++;

            // 1 copy → gần merge; 2 copies → cần đúng lá này để lên sao
            s += copies * brain.genes[21] * (copies == 2 ? 16f : 8f);
        }

        // ── Nhóm 6b: Frontline bonus ─────────────────────────────────────────
        // Taunt unit có giá trị cao hơn khi frontline (0-3) còn chỗ trống
        if (c.hasTaunt)
        {
            int emptyFront = 0;
            for (int i = 0; i < FrontlineCount && i < board.Count; i++)
                if (board[i] == null) emptyFront++;
            s += emptyFront * brain.genes[22] * 2f;
        }

        // ── Nhóm 6c: Cost efficiency ─────────────────────────────────────────
        if (c.cost > 0)
            s = s / c.cost * (1f + brain.genes[3]);

        return s;
    }

    // Trọng số theo Trigger — genes[7..12]
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

    // Trọng số theo Effect — genes[13..17]
    private float EffectWeight(EffectType e)
    {
        switch (e)
        {
            case EffectType.AddStats:        return brain.genes[13];
            case EffectType.Summon:          return brain.genes[14];
            case EffectType.SummonConsumed:  return brain.genes[14] * 1.2f; // summon nhiều
            case EffectType.Destroy:         return brain.genes[14] * 0.7f; // consume để dùng sau
            case EffectType.DealDamage:      return brain.genes[15];
            case EffectType.GainCoin:        return brain.genes[16];
            case EffectType.GiveBuff:        return brain.genes[17];
            case EffectType.Reborn:          return brain.genes[17] * 1.1f;
            case EffectType.TriggerAbility:  return brain.genes[13] * 0.5f;
            default: return 0f;
        }
    }

    // Trọng số synergy theo Tribe — genes[18..20]
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
    // PLACEMENT: Taunt → frontline; các unit khác → backline trước
    // ──────────────────────────────────────────────────────────────────────────
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
