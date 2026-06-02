using UnityEngine;

public struct MatchResult
{
    public int result;
    public int hpA;
    public int hpB;
    public int turns;
    public float scoreA;
    public float lateScoreA;
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
        float lateScoreA = 0f;
        float lateScoreB = 0f;

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

            if (currentTurn >= 9)
            {
                float lateWeight = Mathf.InverseLerp(9f, MaxTurns, currentTurn);
                lateScoreA += BoardPower(botA) * (1f + lateWeight);
                lateScoreB += BoardPower(botB) * (1f + lateWeight);
            }

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

            if (currentTurn >= 9)
            {
                float lateWeight = Mathf.InverseLerp(9f, MaxTurns, currentTurn);
                lateScoreA += BoardPower(botA) * (1.2f + lateWeight);
                lateScoreB += BoardPower(botB) * (1.2f + lateWeight);
            }
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
            scoreA = ScoreFromA(result, hpA, hpB, turnsPlayed, lateScoreA, lateScoreB),
            lateScoreA = lateScoreA
        };
    }

    private static float ScoreFromA(int result, int hpA, int hpB, int turns, float lateScoreA, float lateScoreB)
    {
        float score = result > 0 ? 120f : result == 0 ? 70f : 35f;
        score += hpA * 6f;
        score -= hpB * 3f;
        score += Mathf.Max(0f, lateScoreA) * 0.18f;
        score += Mathf.Clamp(lateScoreA - lateScoreB, -300f, 300f) * 0.12f;

        if (turns >= 12)
            score += (turns - 11) * 3f;

        return Mathf.Max(1f, score);
    }

    private static float BoardPower(BotAgent bot)
    {
        float score = 0f;
        foreach (var unit in bot.board)
        {
            if (unit == null || unit.IsDead) continue;

            score += unit.currentATK * 3f;
            score += unit.currentHP * 2f;
            score += unit.Data.tier * 8f;
            score += unit.mergeLevel * 24f;
            if (unit.isTaunt) score += 8f;
            if (unit.isReborn && !unit.hasRebornUsed) score += 18f;
            if (unit.safeguardActive) score += 12f;
            if (unit.Data.abilities != null) score += unit.Data.abilities.Count * 4f;
        }
        return score;
    }
}
