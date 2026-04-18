using UnityEngine;


// Lưu ý: AbilityData giờ đã được nâng cấp lên một class phức tạp hơn, có thể chứa nhiều loại hiệu ứng khác nhau (TTE) và mục tiêu khác nhau (TargetType).

// TriggerType: Xác định khi nào kỹ năng được kích hoạt
public enum TriggerType
{
    None = 0,
    OnTakeDamage = 1,   // Bị đánh
    OnAttack = 2,       // Khi tấn công
    OnDeath = 3,        // Khi chết (Slain)
    OnTurnStart = 4     // Đầu hiệp (dành cho Growth)
}
// TargetType: Xác định mục tiêu của kỹ năng
public enum TargetType
{
    None = 0,
    Self = 1,           // Bản thân
    RandomAlly = 2,     // Đồng minh ngẫu nhiên
    AllAllies = 3,      // Toàn bộ đồng minh
    Enemy = 4,          // Kẻ địch trực tiếp
    RandomEnemy = 5     // Kẻ địch ngẫu nhiên
}
// EffectType: Xác định loại hiệu ứng của kỹ năng (TTE)
public enum EffectType
{
    None = 0,
    AddStats = 1,       // Tăng ATK/HP (Enrage, Growth)
    DealDamage = 2,     // Trừ máu (Thorns)
    Reborn = 4,          // Hồi sinh (Đặc biệt)
    GainCoin = 5,       // Thêm Coin (Kinh tế)
    Heal = 6,           // Hồi máu
}

[System.Serializable]
public class AbilityData
{
    public TriggerType trigger;
    public TargetType target;
    public int targetCount;    // Số lượng mục tiêu (VD: 2 đồng minh)

    public EffectType effect;
    public int effectValue1;   // Giá trị 1 (VD: Tăng bao nhiêu ATK, hoặc Hồi bao nhiêu HP)
    public int effectValue2;   // Giá trị 2 (VD: Tăng bao nhiêu HP)

    // Đánh dấu thẻ có Taunt (Taunt là cơ chế Pasive đặc biệt, không theo tuần tự TTE)
    public bool isTaunt;
}


//Enrage: Tăng ATK khi bị đánh (Trigger: OnTakeDamage, Effect: AddStats, Target: Self)
//Growth: Tăng ATK/HP vào đầu hiệp (Trigger: OnTurnStart, Effect: AddStats, Target: Self)
//Thorns: Trả đòn khi bị đánh (Trigger: OnTakeDamage, Effect: DealDamage, Target: Enemy)
//reborn: Hồi sinh một lần sau khi chết (Trigger: OnDeath, Effect: Reborn, Target: Self)
//Economy: Thêm Coin vào đầu hiệp (Trigger: OnTurnStart, Effect: GainCoin, Target: Self) 


//taunt: Khi có Taunt, thẻ sẽ tự động được coi là có hiệu ứng Taunt, bất kể TriggerType hay EffectType nào khác. Taunt sẽ khiến kẻ địch phải tấn công thẻ này trước khi tấn công các thẻ khác không có Taunt. (Taunt không cần Trigger hay Effect cụ thể, chỉ cần isTaunt = true là đủ)