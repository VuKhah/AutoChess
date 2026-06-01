using UnityEngine;

public struct MatchResult
{
    public int result;
    public int hpA;
    public int hpB;
    public int turns;
    public float scoreA;
}

public class GameSimulator
{
    private CombatResolver resolver = new CombatResolver();
    private const int StartingHP = 7;
    private const int MaxTurns = 20;

    public int SimulateMatch(BotAgent botA, BotAgent botB)
    {
        return EvaluateMatch(botA, botB).result;
    }

    public MatchResult EvaluateMatch(BotAgent botA, BotAgent botB)
    {
        int hpA = StartingHP, hpB = StartingHP;
        int turnsPlayed = 0;

        for (int i = 0; i < MaxTurns; i++)
        {
            turnsPlayed = i + 1;
            int currentTurn = i + 1;
            int shopTier = Mathf.Clamp((currentTurn + 1) / 2, 1, 6);

            // Cả 2 bot nhận shop mới, TRỪ KHI đã freeze lượt trước → giữ nguyên shop cũ
            var unitShopA  = (botA.isShopFrozen && botA.frozenUnitShop  != null)
                ? botA.frozenUnitShop  : CardDatabase.Instance.GetRandomUnitShop(5, shopTier);
            var spellShopA = (botA.isShopFrozen && botA.frozenSpellShop != null)
                ? botA.frozenSpellShop : CardDatabase.Instance.GetRandomSpellShop(2, shopTier);
            var unitShopB  = (botB.isShopFrozen && botB.frozenUnitShop  != null)
                ? botB.frozenUnitShop  : CardDatabase.Instance.GetRandomUnitShop(5, shopTier);
            var spellShopB = (botB.isShopFrozen && botB.frozenSpellShop != null)
                ? botB.frozenSpellShop : CardDatabase.Instance.GetRandomSpellShop(2, shopTier);

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

        int result = 0;
        if (hpA > hpB) result = 1;
        else if (hpB > hpA) result = -1;

        return new MatchResult
        {
            result = result,
            hpA = hpA,
            hpB = hpB,
            turns = turnsPlayed,
            scoreA = ScoreFromA(result, hpA, hpB, turnsPlayed)
        };
    }

    private static float ScoreFromA(int result, int hpA, int hpB, int turns)
    {
        float score = result > 0 ? 120f : result == 0 ? 70f : 35f;
        score += hpA * 6f;
        score -= hpB * 3f;

        if (result > 0)
            score += (MaxTurns - turns) * 2f;

        return Mathf.Max(1f, score);
    }
}
