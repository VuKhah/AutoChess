using System.Collections.Generic;

public enum CardType { Unit, Magic }
public enum Tribe { None, Babylon, Olympus, Niles }
public enum MagicGroup {None, StatBoost, AddAbility, Economy }

[System.Serializable]
public class CardDefinition
{
    public string cardID;
    public string cardName;
    public CardType cardType;   // Unit, Magic
    public Tribe tribe;      // None, Babylon, Olympus...
    public int baseATK;
    public int baseHP;
    public int cost;
    public int tier;

    public AbilityData ability; // ID kỹ năng (1: Enrage, 4: Slain...)
    public string description; // Mô tả kỹ năng, có thể dùng để hiển thị tooltip

    // Các trường bổ sung cho Bài Phép (Magic)
    public MagicGroup magicGroup;     // 1: tăng chỉ số, 2: gắn Ability, 3: kinh tế
    public int statBonusATK;
    public int statBonusHP;
}

[System.Serializable]
public class CardDataWrapper { public List<CardDefinition> cards; }