public class CardInstance
{
    public CardDefinition Data;
    public int currentATK;
    public int currentHP;
    public int slotIndex;
    public bool hasRebornUsed = false;

    public CardInstance(CardDefinition data, int slot)
    {   
        this.Data = data;
        this.currentATK = data.baseATK;
        this.currentHP = data.baseHP;
        this.slotIndex = slot;
    }

    // Hàm nhận sát thương
    public void TakeDamage(int amount)
    {
        currentHP -= amount;
    }

    public void Revive(int hp)
    {
        currentHP = hp;
        hasRebornUsed = true;
    }

    public bool IsDead => currentHP <= 0;
    public bool IsDamaged => currentHP < Data.baseHP;
}