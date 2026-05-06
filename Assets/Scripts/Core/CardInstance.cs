using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class CardInstance
{
    public CardDefinition Data;

    // 1. Chỉ số cộng thêm vĩnh viễn (Permanent - không reset, chỉ thay đổi khi có hiệu ứng vĩnh viễn)
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

    // 4. Merge level (0=gốc, 1=đã merge 1 lần, 2=đã merge 2 lần)
    public int mergeLevel = 0;

    // 5. Taunt runtime — độc lập với AbilityData, không bị ghi đè khi thay ability
    public bool isTaunt;

    public CardInstance(CardDefinition data, int slot)
    {
        this.Data = data;
        this.isTaunt = data.ability != null && data.ability.isTaunt;
        ResetStats();
        this.slotIndex = slot;
    }

    public void ResetStats()
    {
        const float keepRatio = 0.7f;
        int tier = mergeLevel + 1;
        currentATK = Mathf.RoundToInt(Data.baseATK * tier
                + keepRatio * (growthATKBonus + permanentATKBonus));
        currentHP  = Mathf.RoundToInt(Data.baseHP  * tier
                 + keepRatio * (growthHPBonus  + permanentHPBonus));
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