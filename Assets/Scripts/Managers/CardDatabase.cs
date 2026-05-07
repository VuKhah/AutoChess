using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Cần thiết để dùng ToList() và Where()

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;

    private List<CardDefinition> unitList = new List<CardDefinition>();
    private List<CardDefinition> magicList = new List<CardDefinition>();

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

    void LoadDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("CardsData");
        if (jsonFile == null)
        {
            Debug.LogError("LỖI: Không tìm thấy file CardsData trong thư mục Resources!");
            return;
        }

        CardDataWrapper data = JsonUtility.FromJson<CardDataWrapper>(jsonFile.text);
        unitList.Clear();
        magicList.Clear();
        foreach (var card in data.cards)
        {
            if (card.cardType == CardType.Unit)
                unitList.Add(card);
            else if (card.cardType == CardType.Magic)
                magicList.Add(card);
        }
        Debug.Log($"<color=green>[DATABASE]</color> Đã nạp thành công {unitList.Count + magicList.Count} lá bài.");
    }

    public List<CardDefinition> GetAllCards()
    {
        return unitList.Concat(magicList).ToList();
    }

    // ==========================================
    // LOGIC SHOP: ROLL THEO TRỌNG SỐ (WEIGHTED RANDOM)
    // ==========================================
    public List<CardDefinition> GetRandomShop(int count, int currentShopLevel)
    {
        List<CardDefinition> shop = new List<CardDefinition>();
        var allCards = GetAllCards();

        for (int i = 0; i < count; i++)
        {
            // 1. Roll xem slot này sẽ ra bài Tier mấy
            int rolledTier = RollTier(currentShopLevel);

            // 2. Lọc danh sách bài để chỉ lấy đúng Tier vừa roll
            List<CardDefinition> tierPool = allCards.Where(c => c.tier == rolledTier).ToList();

            // FALLBACK AN TOÀN: Nếu lỡ bạn chưa tạo data bài cho Tier này, hệ thống tự động giáng cấp xuống để tránh lỗi rỗng
            if (tierPool.Count == 0)
            {
                Debug.LogWarning($"[DATABASE] Không có bài nào ở Tier {rolledTier}. Đang lấy bài Tier thấp hơn bù vào!");
                tierPool = allCards.Where(c => c.tier <= rolledTier).ToList();
            }

            // 3. Bốc 1 lá ngẫu nhiên từ Pool vừa lọc
            if (tierPool.Count > 0)
            {
                shop.Add(tierPool[Random.Range(0, tierPool.Count)]);
            }
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

        var magic = magicList.FirstOrDefault(card => card.cardID == id);
        if (magic != null) return magic;

        return null;
    }

    public List<CardDefinition> GetAllUnits() => unitList;
    public List<CardDefinition> GetAllMagics() => magicList;
}