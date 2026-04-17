public class CardInstance
{
    public CardDefinition Data;

    // 1. Chỉ số cộng thêm vĩnh viễn từ bài Phép
    public int permanentATKBonus;
    public int permanentHPBonus;

    // 2. Buff tạm thời (Chỉ dùng trong 1 trận Combat - Reset sau trận)
    public int combatATKBonus;
    public int combatHPBonus;
    // 3. Chỉ số thực tế (Dùng để trừ máu)
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
        // Công thức: Thực tế = Gốc + Vĩnh viễn + Tạm thời
        currentATK = Data.baseATK + permanentATKBonus;
        currentHP = Data.baseHP + permanentHPBonus;
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