using System.Collections.Generic;

public class BotAgent
{
    public Chromosome brain;
    public EconomyManager economy;
    public List<CardInstance> board = new List<CardInstance>(new CardInstance[GameManager.BoardSlotCount]);

    public BotAgent(Chromosome brain)
    {
        this.brain = brain;
        this.economy = new EconomyManager();
    }

    public void DecidePrepPhase(List<CardDefinition> shop)
    {
        economy.ResetEconomy();

        // BUG-AI-01 FIX: Dùng bản copy để tiêu thụ slot shop sau mỗi lần mua,
        // tránh mua lặp cùng 1 thẻ nhiều lần.
        var availableShop = new List<CardDefinition>(shop);

        bool bought = true;
        while (bought)
        {
            bought = false;
            CardDefinition bestCard = null;
            float bestScore = -1f;

            foreach (var card in availableShop)
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
                availableShop.Remove(bestCard);
                bought = true;
            }
        }
    }

    private float Evaluate(CardDefinition c)
    {
        // BUG-AI-02 FIX: Spell không có stats chiến đấu, không đặt lên board được.
        if (c.cardType == CardType.Spell) return -1f;

        float s = c.baseATK * brain.genes[0] + c.baseHP * brain.genes[1];

        // BUG-AI-04 FIX: Passive keywords độc lập với abilities list —
        // một thẻ có hasTaunt nhưng abilities rỗng vẫn phải được tính điểm đúng.
        if (c.hasTaunt)     s += 8  * brain.genes[3];
        if (c.hasReborn)    s += 10 * brain.genes[5];
        if (c.hasSafeguard) s += 6  * brain.genes[3];

        if (c.abilities != null)
        {
            foreach (var a in c.abilities)
            {
                if (a == null) continue;
                if (a.trigger == TriggerType.OnTakeDamage && a.effect == EffectType.DealDamage)
                    s += 8  * brain.genes[3];
                if (a.trigger == TriggerType.StartOfBattle && a.effect == EffectType.AddStats)
                    s += 12 * brain.genes[4];
                if (a.trigger == TriggerType.OnDeath && a.effect == EffectType.AddStats)
                    s += 10 * brain.genes[2];
            }
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
