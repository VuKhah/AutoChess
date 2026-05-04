using System.Collections.Generic;

public enum CardType { Unit, Magic }
public enum Tribe { None, Babylon, Olympus, Niles }

[System.Serializable]
public class CardDefinition
{
    public string cardID;
    public string cardName;
    public CardType cardType;
    public Tribe tribe;
    public int baseATK;
    public int baseHP;
    public int cost;
    public int tier;

    public AbilityData ability;
    public string description;

    // Magic fields — magicGroup là string để JSON dùng tên trực tiếp (StatBoost, AddAbility, AddTaunt, Economy)
    public string magicGroup;
    public int statBonusATK;
    public int statBonusHP;
}

[System.Serializable]
public class CardDataWrapper { public List<CardDefinition> cards; }