using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Cần thiết để dùng ToList() và Where()

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;

    private List<CardDefinition> unitList  = new List<CardDefinition>();
    private List<CardDefinition> spellList = new List<CardDefinition>();

    // ==========================================
    // BẢNG TRỌNG SỐ TỶ LỆ XUẤT HIỆN THEO TIER (DROP RATES)
    // Dòng: Shop Level (1 đến 6)
    // Cột: Tỷ lệ % ra Tier (1 đến 6)
    // ==========================================
    private readonly int[,] shopDropRates = new int[6, 6] {
        // T1,  T2,  T3,  T4,  T5,  T6   (Tổng = 100%)
        { 100,   0,   0,   0,   0,   0 }, // Shop Level 1
        {  70,  30,   0,   0,   0,   0 }, // Shop Level 2
        {  50,  35,  15,   0,   0,   0 }, // Shop Level 3
        {  25,  40,  25,  10,   0,   0 }, // Shop Level 4
        {  15,  25,  35,  15,  10,   0 }, // Shop Level 5
        {  10,  15,  20,  25,  20,   10 }  // Shop Level 6
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Đảm bảo Singleton duy nhất

        LoadDatabase();
    }

    public void LoadDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("CardsData");
        if (jsonFile == null)
        {
            Debug.LogError("LỖI: Không tìm thấy file CardsData trong thư mục Resources!");
            return;
        }

        CardDataWrapper data = JsonUtility.FromJson<CardDataWrapper>(jsonFile.text);
        unitList.Clear();
        spellList.Clear();
        foreach (var card in data.cards)
        {
            if (card.cardType == CardType.Unit)
                unitList.Add(card);
            else if (card.cardType == CardType.Spell)
                spellList.Add(card);
        }
        Debug.Log($"<color=green>[DATABASE]</color> Đã nạp thành công {unitList.Count + spellList.Count} lá bài.");
    }

    public List<CardDefinition> GetAllCards()
    {
        return unitList.Concat(spellList).ToList();
    }

    // ==========================================
    // LOGIC SHOP: ROLL THEO TRỌNG SỐ (WEIGHTED RANDOM)
    // ==========================================
    public List<CardDefinition> GetRandomUnitShop(int count, int currentShopLevel)
    {
        List<CardDefinition> shop = new List<CardDefinition>();
        for (int i = 0; i < count; i++)
        {
            int rolledTier = RollTier(currentShopLevel);
            List<CardDefinition> tierPool = unitList.Where(c => c.tier == rolledTier).ToList();
            if (tierPool.Count == 0)
            {
                Debug.LogWarning($"[DATABASE] Không có unit nào ở Tier {rolledTier}. Đang lấy unit Tier thấp hơn bù vào!");
                tierPool = unitList.Where(c => c.tier <= rolledTier).ToList();
            }
            if (tierPool.Count > 0)
                shop.Add(tierPool[Random.Range(0, tierPool.Count)]);
        }
        return shop;
    }

    public List<CardDefinition> GetRandomSpellShop(int count, int currentShopLevel)
    {
        List<CardDefinition> shop = new List<CardDefinition>();
        for (int i = 0; i < count; i++)
        {
            int rolledTier = RollTier(currentShopLevel);
            List<CardDefinition> tierPool = spellList.Where(c => c.tier == rolledTier).ToList();
            if (tierPool.Count == 0)
                tierPool = spellList.ToList(); // fallback: lấy bất kỳ spell nào
            if (tierPool.Count > 0)
                shop.Add(tierPool[Random.Range(0, tierPool.Count)]);
        }
        return shop;
    }

    // Thuật toán quay số dựa trên Bảng tỷ lệ phần trăm
    private int RollTier(int shopLevel)
    {
        // Chặn level trong khoảng 0-5 (tương ứng index mảng 2 chiều của cấp 1-6)
        int levelIndex = Mathf.Clamp(shopLevel - 1, 0, 5);

        // Quay số từ 0 đến 99
        int roll = Random.Range(0, 100);
        int cumulative = 0;

        for (int tierIndex = 0; tierIndex < 6; tierIndex++)
        {
            cumulative += shopDropRates[levelIndex, tierIndex];
            if (roll < cumulative)
            {
                return tierIndex + 1; // Tier bắt đầu từ 1, index bắt đầu từ 0
            }
        }
        return 1; // Mặc định an toàn
    }

    public CardDefinition GetCard(string id)
    {
        var unit = unitList.FirstOrDefault(card => card.cardID == id);
        if (unit != null) return unit;

        var spell = spellList.FirstOrDefault(card => card.cardID == id);
        if (spell != null) return spell;

        return null;
    }

    public int[] GetDropRates(int shopLevel)
    {
        int levelIndex = Mathf.Clamp(shopLevel - 1, 0, 5);
        var rates = new int[6];
        for (int i = 0; i < 6; i++)
            rates[i] = shopDropRates[levelIndex, i];
        return rates;
    }

    public List<CardDefinition> GetAllUnits()  => unitList;
    public List<CardDefinition> GetAllSpells() => spellList;
}