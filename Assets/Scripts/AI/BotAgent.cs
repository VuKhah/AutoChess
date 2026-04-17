using System.Collections.Generic;

public class BotAgent {
    public Chromosome brain;
    public EconomyManager economy;
    public List<CardInstance> board = new List<CardInstance>(new CardInstance[6]);

    public BotAgent(Chromosome brain) {
        this.brain = brain;
        this.economy = new EconomyManager();
    }

    public void DecidePrepPhase(List<CardDefinition> shop) {
        economy.ResetEconomy();
        
        // Mua cho đến khi hết tiền hoặc hết chỗ
        while (economy.CurrentCoin >= 3) {
            CardDefinition bestCard = null;
            float bestScore = -1f;

            foreach (var card in shop) {
                float score = Evaluate(card);
                if (score > bestScore) {
                    bestScore = score;
                    bestCard = card;
                }
            }

            if (bestCard != null && HasEmptySlot()) {
                BuyAndPlace(bestCard);
            } else break;
        }
    }

    private float Evaluate(CardDefinition c) {
        float s = c.baseATK * brain.genes[0] + c.baseHP * brain.genes[1];
        if (c.ability == AbilityType.Economy) s += 10 * brain.genes[2];
        if (c.ability == AbilityType.Taunt || c.ability == AbilityType.Thorns) s += 8 * brain.genes[3];
        if (c.ability == AbilityType.Growth) s += 12 * brain.genes[4];
        if (c.ability == AbilityType.Reborn) s += 10 * brain.genes[5];
        return s;
    }

    private bool HasEmptySlot() => board.Exists(slot => slot == null);
    private void BuyAndPlace(CardDefinition c) {
        int idx = board.FindIndex(slot => slot == null);
        board[idx] = new CardInstance(c, idx);
        economy.Buy();
    }
}