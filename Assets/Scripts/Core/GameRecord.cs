using System.Collections;
using System.Collections.Generic;
[System.Serializable]
public class GameRecord
{
    public string matchID;
    public List<TurnRecord> turns = new List<TurnRecord>();
}


[System.Serializable]
public class CardSnapshot
{
    public string cardID;
    public int hp;
    public int atk;

    public CardSnapshot(string id, int h, int a)
    {
        cardID = id; hp = h; atk = a;
    }
}

[System.Serializable]
public enum CombatActionType
{
    Clash,
    Summon
}

[System.Serializable]
public class CombatAction
{
    public CombatActionType actionType = CombatActionType.Clash;

    public int attackerIdx;
    public int targetIdx;
    public bool isPlayerAttacking;

    public string attackerName;

    public string targetName;
    public int atkHPBefore, atkHPAfter;
    public int defHPBefore, defHPAfter;

    // Passive Reborn: đánh dấu để visualizer biết phải hồi sinh card sau DieAnimation
    public bool attackerReborn;
    public bool defenderReborn;
    public int  attackerRevivedHP;
    public int  defenderRevivedHP;

    public int summonSlotIdx;
    public bool isPlayerSummon;
    public string summonCardID;
    public int summonHP;
    public int summonATK;

    public CombatAction(int atk, int target, bool isPlayer, string aName, string tName, int aBefore, int aAfter, int dBefore, int dAfter)
    {
        actionType = CombatActionType.Clash;
        attackerIdx = atk;
        targetIdx = target;
        isPlayerAttacking = isPlayer;
        attackerName = aName;
        targetName = tName;
        atkHPBefore = aBefore;
        atkHPAfter = aAfter;
        defHPBefore = dBefore;
        defHPAfter = dAfter;
    }

    public static CombatAction Summon(int slotIdx, bool isPlayerSide, CardInstance unit)
    {
        return new CombatAction(-1, -1, isPlayerSide, null, unit.Data.cardName, 0, 0, 0, 0)
        {
            actionType = CombatActionType.Summon,
            summonSlotIdx = slotIdx,
            isPlayerSummon = isPlayerSide,
            summonCardID = unit.Data.cardID,
            summonHP = unit.currentHP,
            summonATK = unit.currentATK
        };
    }
}

[System.Serializable]
public class TurnRecord
{
    public int turnIndex;
    // Danh sách các hành động để GameManager "diễn" Visual
    public List<CombatAction> actions = new List<CombatAction>();

    // Trạng thái Board sau trận để AI phân tích (Snapshot)
    public List<CardSnapshot> playerBoardFinal = new List<CardSnapshot>();
    public List<CardSnapshot> enemyBoardFinal = new List<CardSnapshot>();

    public void AddAction(CombatAction action) => actions.Add(action);
}
