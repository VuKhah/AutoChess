using System.Collections.Generic;

public enum CardType { Unit, Magic }
public enum Tribe { None, Babylon, Olympus, Niles }
[System.Serializable]
public class CardDefinition
{
    public string cardID;
    public string cardName;
    public CardType cardType;   // Unit, Magic
    public Tribe tribe;      // None, Babylon, Olympus...
    public int baseATK;
    public int baseHP;
    public AbilityType ability; // ID kỹ năng (1: Enrage, 4: Slain...)
    public int abilityValue;
    public int cost;
    public int tier;

    public string description; // Mô tả kỹ năng, có thể dùng để hiển thị tooltip

    // Các trường bổ sung cho Bài Phép (Magic)
    public int magicGroup;     // 1, 2, 3
    public int statBonusATK;
    public int statBonusHP;
}

[System.Serializable]
public class CardDataWrapper { public List<CardDefinition> cards; }