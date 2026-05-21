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
    public int maxHP;       // HP tối đa tính theo tier + bonus, set bởi ResetStats
    public int slotIndex;
    public bool hasRebornUsed = false;

    // 4. Merge level (0=gốc, 1=đã merge 1 lần, 2=đã merge 2 lần)
    public int mergeLevel = 0;

    // 5. Passive keywords runtime — khởi tạo từ CardDefinition, có thể được grant bởi abilities
    public bool isTaunt;
    public bool isReborn;
    public bool safeguardActive;

    // 6. Đếm số lần mỗi trigger kích hoạt trong combat này (cho triggerLimit + conditionCount)
    public List<int> abilityTriggerCounts = new List<int>();
    // Cộng thêm vào effectValue1/2 mỗi lần fire (cho isEscalating — reset mỗi combat)
    public List<int> abilityEscalationBonuses = new List<int>();

    // 7. Danh sách cardID của các unit đã bị Consume (Sekhmet mechanic)
    public List<string> consumedCardIDs = new List<string>();

    // 8. Tracking cái chết cho Death Stack — reset mỗi combat
    public bool onDeathProcessed = false;   // Đã được đưa vào death stack chưa
    public CardInstance lastAttacker = null; // Kẻ gây đòn chết cuối cùng (cho OnDeath directEnemy)

    // 9. Đánh dấu unit được tạo ra bởi SummonUnit trong Battle — không được phép Merge
    public bool isBattleSpawned = false;

    public CardInstance(CardDefinition data, int slot)
    {
        // BUG FIX: Clone abilities list để tránh AddAbility spell mutate CardDefinition chung
        this.Data = data.Clone();
        ResetStats();
        this.slotIndex = slot;
    }

    public void ResetStats()
    {
        const float keepRatio = 0.7f;
        int tier = mergeLevel + 1;
        currentATK = Mathf.RoundToInt(Data.baseATK * tier
                + keepRatio * (growthATKBonus + permanentATKBonus));
        maxHP      = Mathf.RoundToInt(Data.baseHP  * tier
                 + keepRatio * (growthHPBonus  + permanentHPBonus));
        currentHP  = maxHP;
        // Reset passives từ data gốc (mỗi combat bắt đầu lại từ đầu)
        isTaunt         = Data.hasTaunt;
        isReborn        = Data.hasReborn;
        safeguardActive = Data.hasSafeguard;
        hasRebornUsed   = false;
        onDeathProcessed = false;
        lastAttacker     = null;
        int abCount = Data.abilities != null ? Data.abilities.Count : 0;
        abilityTriggerCounts      = new List<int>(new int[abCount]);
        abilityEscalationBonuses  = new List<int>(new int[abCount]);
        // consumedCardIDs KHÔNG reset — phải tồn tại qua các turn để SummonConsumed hoạt động đúng.
        // Chỉ bị clear khi SummonConsumed thực sự kích hoạt (trong AbilityEngine.ExecuteEffect).
    }

    public void Revive(int hp)
    {
        currentHP = hp;
        hasRebornUsed    = true;
        onDeathProcessed = false; // Reset để có thể chết lại lần nữa sau Reborn
        lastAttacker     = null;
    }

    // Passive Reborn: hồi sinh với chỉ số mặc định (ATK + HP đầy như đầu combat),
    // xóa flag isReborn (round đó không revive nữa), giữ nguyên ability counters/buff combat.
    public void ReviveDefault()
    {
        const float keepRatio = 0.7f;
        int tier = mergeLevel + 1;
        currentATK = Mathf.RoundToInt(Data.baseATK * tier
                   + keepRatio * (growthATKBonus + permanentATKBonus));
        maxHP      = Mathf.RoundToInt(Data.baseHP  * tier
                   + keepRatio * (growthHPBonus  + permanentHPBonus));
        currentHP  = maxHP;
        isReborn   = false;   // mất passive trong round này
        hasRebornUsed    = true;
        onDeathProcessed = false;
        lastAttacker     = null;
    }

    public bool IsDead    => currentHP <= 0;
    public bool IsDamaged => currentHP < maxHP;
}