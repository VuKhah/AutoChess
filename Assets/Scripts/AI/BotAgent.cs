using System.Collections.Generic;

public class BotAgent
{
    public Chromosome brain;
    public EconomyManager economy;
    public List<CardInstance> board = new List<CardInstance>(new CardInstance[6]);

    public BotAgent(Chromosome brain)
    {
        this.brain = brain;
        this.economy = new EconomyManager();
    }

    public void DecidePrepPhase(List<CardDefinition> shop)
    {
        economy.ResetEconomy();

        // Mua cho đến khi hết tiền hoặc hết chỗ
        bool bought = true;
        while (bought)
        {
            bought = false;
            CardDefinition bestCard = null;
            float bestScore = -1f;

            foreach (var card in shop)
            {
                if (card.cost > economy.CurrentCoin) continue;
                float score = Evaluate(card);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCard = card;
                }
            }

            if (bestCard != null && HasEmptySlot())
            {
                BuyAndPlace(bestCard);
                bought = true;
            }
        }
    }

    private float Evaluate(CardDefinition c)
    {
        // 1. Chấm điểm dựa trên chỉ số gốc
        float s = c.baseATK * brain.genes[0] + c.baseHP * brain.genes[1];

        // 2. Chấm điểm dựa trên hệ thống Kỹ năng TTE mới
        if (c.abilities != null && c.abilities.Count > 0)
        {
            if (c.hasTaunt)
                s += 8 * brain.genes[3];

            if (c.abilities.Exists(a => a != null && a.trigger == TriggerType.OnTakeDamage && a.effect == EffectType.DealDamage))
                s += 8 * brain.genes[3];

            if (c.abilities.Exists(a => a != null && a.trigger == TriggerType.StartOfBattle && a.effect == EffectType.AddStats))
                s += 12 * brain.genes[4];

            if (c.hasReborn)
                s += 10 * brain.genes[5];

            if (c.abilities.Exists(a => a != null && a.trigger == TriggerType.OnDeath && a.effect == EffectType.AddStats))
                s += 10 * brain.genes[2];
        }

        return s;
    }

    private bool HasEmptySlot() => board.Exists(slot => slot == null);

    private void BuyAndPlace(CardDefinition c)
    {
        int idx = board.FindIndex(slot => slot == null);
        if (idx != -1)
        {
            board[idx] = new CardInstance(c, idx);
            economy.TryBuy(c.cost);
        }
    }
}