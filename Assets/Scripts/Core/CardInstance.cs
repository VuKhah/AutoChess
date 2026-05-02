public class CardInstance
{
    public CardDefinition Data;

    // 1. Chỉ số cộng thêm vĩnh viễn từ bài Phép
    public int permanentATKBonus;
    public int permanentHPBonus;

    // 2. Tăng trưởng tích lũy (Growth - cộng dồn qua các turn, không reset)
    public int growthATKBonus;
    public int growthHPBonus;

    // 3. Chỉ số thực tế (Dùng để trừ máu, buff tạm thời combat sẽ cộng thẳng vào đây)
    public int currentATK;
    public int currentHP;
    public int slotIndex;
    public bool hasRebornUsed = false;

    public CardInstance(CardDefinition data, int slot)
    {
        this.Data = data;
        ResetStats();
        this.slotIndex = slot;
    }
    public void ResetStats()
    {
        // Công thức: Thực tế = Gốc + Phép vĩnh viễn + Tăng trưởng (buff tạm thời combat bị xóa)
        currentATK = Data.baseATK + permanentATKBonus + growthATKBonus;
        currentHP = Data.baseHP + permanentHPBonus + growthHPBonus;
        hasRebornUsed = false;
    }

    public void Revive(int hp)
    {
        currentHP = hp;
        hasRebornUsed = true;
    }

    public bool IsDead => currentHP <= 0;
    public bool IsDamaged => currentHP < Data.baseHP;
}