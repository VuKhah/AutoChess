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
    Summon,
    StatChange  // Cập nhật chỉ số từ ability (buff/debuff/growth) — không có delay khi visualize
}

public enum FlashType
{
    None,
    Buff,             // xanh lá — tăng chỉ số
    Debuff,           // đỏ — nhận sát thương / giảm chỉ số
    Status,           // xanh dương — nhận status (Taunt, Safeguard, Reborn)
    SynergyBabylon,   // vàng — synergy Babylon
    SynergyOlympus,   // cyan — synergy Olympus
    SynergyNiles,     // lục — synergy Niles
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

    // StatChange fields
    public int       statSlotIdx;
    public bool      statIsPlayerSide;
    public int       statNewATK;
    public int       statNewHP;
    public FlashType flashType;

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

    public static CombatAction StatChange(int slotIdx, bool isPlayerSide, int newATK, int newHP,
                                          FlashType flash = FlashType.None)
    {
        return new CombatAction(-1, -1, isPlayerSide, null, null, 0, 0, 0, 0)
        {
            actionType       = CombatActionType.StatChange,
            statSlotIdx      = slotIdx,
            statIsPlayerSide = isPlayerSide,
            statNewATK       = newATK,
            statNewHP        = newHP,
            flashType        = flash
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
