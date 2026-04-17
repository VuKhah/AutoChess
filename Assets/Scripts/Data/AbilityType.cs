public enum AbilityType
{
    None = 0,
    Enrage = 1,  // Nhận sát thương -> Tăng ATK
    Thorns = 2,  // Phản sát thương cố định (2)
    Growth = 3,  // Tăng ATK/HP sau mỗi lượt Prep
    Reborn = 4,  // Hồi sinh 1 lần với 1 HP
    Economy = 5, // Thắng/Sống sót -> Thêm Coin lượt sau
    Taunt = 6    // Mục tiêu bắt buộc phải tấn công
}