using UnityEngine;

// TriggerType: Khi nào kỹ năng được kích hoạt? 
public enum TriggerType
{
    None = 0,
    OnTakeDamage = 1,   // Khi bị đánh
    OnAttack = 2,       // Khi tấn công
    OnDeath = 3,        // Khi chết (Slain)
    StartOfBattle = 4,     // Đầu trận 
    EndTurnShop = 5,        // Cuối Turn Shop
    OnDeploy = 6,       // Khi được triển khai 
    OnSell = 7,         // Khi bán thẻ (Bị loại bỏ khỏi bàn)
    OnAllyDeath = 8,    // Khi đồng minh chết (Đặc biệt, chỉ kích hoạt khi đồng minh cùng bộ tộc chết)
    OnAllySummon = 9,   // Khi đồng minh được triệu hồi (Đặc biệt, chỉ kích hoạt khi đồng minh cùng bộ tộc được triệu hồi)
    OnAllyReborn = 10,   // Khi đồng minh hồi sinh (Đặc biệt, chỉ kích hoạt khi đồng minh cùng bộ tộc hồi sinh)
    Aura = 11,           // Hiệu ứng Aura (Các hiệu ứng khác lập lại 1 lần khi thẻ này tồn tại trên bàn)
}
// Target: Kỹ năng nhắm vào ai?
public enum TargetType
{
    None = 0,
    Self = 1,           // Bản thân
    RandomAlly = 2,     // Đồng minh ngẫu nhiên
    AllAllies = 3,      // Toàn bộ đồng minh
    RandomEnemy = 4,     // Kẻ địch ngẫu nhiên
    DirectEnemy = 5,     // Kẻ địch trực tiếp
    LowestHealthAlly = 6, // Đồng minh có HP thấp nhất
    LeftAlly = 7,         // Đồng minh bên trái
    RightAlly = 8,        // Đồng minh bên phải
    AllNilesAllies = 9,   // Toàn bộ đồng minh thuộc bộ tộc Niles (Đặc biệt, chỉ áp dụng cho thẻ có bộ tộc Niles)
    AllBabylonAllies = 10, // Toàn bộ đồng minh thuộc bộ tộc Babylon (Đặc biệt, chỉ áp dụng cho thẻ có bộ tộc Babylon)
    TriggerSubject = 11,  // Đơn vị gây ra sự kiện (VD: unit vừa được triệu hồi trong OnAllySummon, đồng minh vừa chết trong OnAllyDeath)
    AllAlliesExceptSelf = 12, // Toàn bộ đồng minh trừ bản thân — dùng cho Aura
}


// Effect: Kỹ năng làm gì?
public enum EffectType
{
    None = 0,
    AddStats = 1,       // Tăng ATK/HP (Enrage, Growth)
    DealDamage = 2,     // Gây sát thương
    GiveBuff = 3,      // Ban hiệu ứng trạng thái (VD: Taunt) — dùng flag isTaunt trong AbilityData
    Summon = 4,          // Triệu hồi (Đặc biệt)
    Destroy = 5,         // Hủy diệt tức thì (Banish) — set HP về 0, trigger OnDeath bình thường
    GainCoin = 6,       // Thêm Coin (Kinh tế)
    Reborn = 7,         // Hồi sinh (Đặc biệt)
    TriggerAbility = 8,   // Kích hoạt ability của target (copy battlecry/deathrattle đồng minh) — không chain nếu target cũng là TriggerAbility
    SummonConsumed = 9,   // Triệu hồi lại tất cả unit đã bị Consume bởi source (dùng với OnDeath)
}

[System.Serializable]
public class AbilityData
{
    [Header("Core TTE")]
    public TriggerType trigger;
    public TargetType target;
    public int targetCount;    // Số lượng mục tiêu (VD: Buff 2 đồng minh ngẫu nhiên)
    public EffectType effect;

    [Header("Values")]
    public int effectValue1;   // Giá trị 1 (VD: Tăng bao nhiêu ATK, hoặc Hồi bao nhiêu HP)
    public int effectValue2;   // Giá trị 2 (VD: Tăng bao nhiêu HP)
    public string summonCardID; // ID của thẻ sẽ triệu hồi (Mummy, Warrior...)
    [Header("Conditions & Limits")]
    public bool isPermanent;    // Buff có giữ lại sau trận đấu không?
    public int triggerLimit;    // Giới hạn số lần kích hoạt (VD: Chỉ kích hoạt 3 lần)
    public int conditionCount;   // Số lần điều kiện đã được đáp ứng (VD: Đã bị đánh 2 lần)


    [Header("Special Flags — chỉ dùng cho GiveBuff effect")]
    public bool isTaunt;        // GiveBuff: grant Taunt cho target
    public bool isReborn;       // GiveBuff: grant Reborn cho target
    public bool isSafeguard;    // GiveBuff: grant Safeguard cho target
    public bool isConsume;      // Destroy: "nuốt" unit — lưu cardID để SummonConsumed dùng sau
    public int targetTribe;    // Bộ tộc mục tiêu — cast sang Tribe enum: 0=All, 1=Babylon, 2=Olympus, 3=Niles

    // Hàm hỗ trợ kiểm tra xem kỹ năng có tác dụng vĩnh viễn không
    public string GetDescription()
    {
        return isPermanent ? "Permanently" : "This battle";
    }
}


//taunt: Khi có Taunt, thẻ sẽ tự động được coi là có hiệu ứng Taunt, bất kể TriggerType hay EffectType nào khác. Taunt sẽ khiến kẻ địch phải tấn công thẻ này trước khi tấn công các thẻ khác không có Taunt. (Taunt không cần Trigger hay Effect cụ thể, chỉ cần isTaunt = true là đủ)