using System.Collections.Generic;
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

            // BUG-AI-06 FIX: Shop size 5 khớp với real game (SummonEnemyTeam dùng 5).
            // BUG-AI-02 FIX: Lọc Unit only — Spell không có stats chiến đấu, không dùng được trong simulation.
            var shopA = CardDatabase.Instance.GetRandomShop(5, currentTier)
                            .FindAll(c => c.cardType == CardType.Unit);
            var shopB = CardDatabase.Instance.GetRandomShop(5, currentTier)
                            .FindAll(c => c.cardType == CardType.Unit);

            botA.DecidePrepPhase(shopA);
            botB.DecidePrepPhase(shopB);

            resolver.ResolveTurn(botA.board, botB.board, new TurnRecord());

            bool aAlive = botA.board.Exists(u => u != null && !u.IsDead);
            bool bAlive = botB.board.Exists(u => u != null && !u.IsDead);

            if (!aAlive && bAlive) hpA--;
            else if (aAlive && !bAlive) hpB--;

            // BUG-AI-03 FIX: Reset board sau mỗi trận, khớp với real game:
            // - Unit chết / isBattleSpawned → null (loại khỏi board)
            // - Unit sống sót → ResetStats() (phục hồi HP, xóa triggerCounts, escalation bonuses, consumedCardIDs)
            ResetBoardAfterCombat(botA.board);
            ResetBoardAfterCombat(botB.board);

            if (hpA <= 0 || hpB <= 0) break;
        }

        if (hpA > hpB) return 1;
        if (hpB > hpA) return -1;
        return 0;
    }

    private static void ResetBoardAfterCombat(List<CardInstance> board)
    {
        for (int i = 0; i < board.Count; i++)
        {
            var unit = board[i];
            if (unit == null) continue;

            if (unit.isBattleSpawned || unit.IsDead)
                board[i] = null;
            else
                unit.ResetStats();
        }
    }
}