using UnityEngine;

public class GameSimulator
{
    private CombatResolver resolver = new CombatResolver();

    public int SimulateMatch(BotAgent botA, BotAgent botB)
    {
        int hpA = 7, hpB = 7;
        int maxTurns = 20;

        for (int i = 0; i < maxTurns; i++)
        {
            int currentTurn = i + 1;
            int shopTier = Mathf.Clamp((currentTurn + 1) / 2, 1, 6);

            // Cả 2 bot nhận unit shop + spell shop (giống người chơi thật)
            var unitShopA  = CardDatabase.Instance.GetRandomUnitShop(5, shopTier);
            var spellShopA = CardDatabase.Instance.GetRandomSpellShop(2, shopTier);
            var unitShopB  = CardDatabase.Instance.GetRandomUnitShop(5, shopTier);
            var spellShopB = CardDatabase.Instance.GetRandomSpellShop(2, shopTier);

            botA.DecidePrepPhase(unitShopA, spellShopA, shopTier);
            botB.DecidePrepPhase(unitShopB, spellShopB, shopTier);

            resolver.ResolveTurn(botA.board, botB.board, new TurnRecord());

            bool aAlive = botA.board.Exists(u => u != null && !u.IsDead);
            bool bAlive = botB.board.Exists(u => u != null && !u.IsDead);

            if (!aAlive && bAlive) hpA--;
            else if (aAlive && !bAlive) hpB--;

            if (hpA <= 0 || hpB <= 0) break;

            botA.EndCombatPhase();
            botB.EndCombatPhase();
            botA.TriggerEndTurnShop();
            botB.TriggerEndTurnShop();
        }

        if (hpA > hpB) return 1;
        if (hpB > hpA) return -1;
        return 0;
    }
}
