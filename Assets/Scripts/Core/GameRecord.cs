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
public class CombatAction
{
    public int attackerIdx;
    public int targetIdx;
    public bool isPlayerAttacking;

    public string attackerName;

    public string targetName;
    public int atkHPBefore, atkHPAfter;
    public int defHPBefore, defHPAfter;

    public CombatAction(int atk, int target, bool isPlayer, string aName, string tName, int aBefore, int aAfter, int dBefore, int dAfter)
    {
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