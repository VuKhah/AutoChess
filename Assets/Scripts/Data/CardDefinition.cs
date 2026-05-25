using System.Collections.Generic;

public enum CardType { Unit, Spell }
public enum Tribe { None, Babylon, Olympus, Niles }

[System.Serializable]
public class SpellEffectData
{
    public int effect;        // 1=BuffStats, 6=GainCoin, 10-21: xem _legend trong CardsSpells.json
    public int target;        // 2=RandomAlly, 3=AllAllies, 12=ChosenAlly, 13=RandomAlliesInBattle
    public int targetCount;
    public int effectValue1;
    public int effectValue2;
    public bool isPermanent;
    public bool isTaunt;
    public bool isReborn;
    public bool isSafeguard;
}

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

    public List<AbilityData> abilities;

    // Passive keywords — độc lập hoàn toàn với hệ thống TTE
    public bool hasTaunt;
    public bool hasReborn;
    public bool hasSafeguard;

    public string description;
    public bool isToken;           // Token: không xuất hiện trong shop, không được spell GetRandomUnit chọn
    public bool isRepeatingDeploy; // Bỏ qua giới hạn hasDeployed — OnDeploy kích hoạt mỗi lần deploy

    // Spell fields
    public string fileName;
    public List<SpellEffectData> spellEffects;

    // BUG FIX: Mỗi CardInstance phải có bản sao abilities riêng,
    // tránh AddAbility spell mutate CardDefinition chung trong database.
    public CardDefinition Clone()
    {
        var c = (CardDefinition)MemberwiseClone();
        c.abilities     = abilities     != null ? new List<AbilityData>(abilities)         : null;
        c.spellEffects  = spellEffects  != null ? new List<SpellEffectData>(spellEffects)  : null;
        return c;
    }
}

[System.Serializable]
public class CardDataWrapper { public List<CardDefinition> cards; }