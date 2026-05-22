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
            int currentTier = Mathf.Clamp((currentTurn + 1) / 2, 1, 6);

            // Shop 5 unit — khớp với game thật (shopUnitCount = 5)
            botA.DecidePrepPhase(CardDatabase.Instance.GetRandomUnitShop(5, currentTier));
            botB.DecidePrepPhase(CardDatabase.Instance.GetRandomUnitShop(5, currentTier));

            resolver.ResolveTurn(botA.board, botB.board, new TurnRecord());

            bool aAlive = botA.board.Exists(u => u != null && !u.IsDead);
            bool bAlive = botB.board.Exists(u => u != null && !u.IsDead);

            if (!aAlive && bAlive) hpA--;
            else if (aAlive && !bAlive) hpB--;

            if (hpA <= 0 || hpB <= 0) break;

            // Kết thúc combat: xóa dead, giữ alive, tích lũy EndTurnShop (giống gameplay thật)
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
