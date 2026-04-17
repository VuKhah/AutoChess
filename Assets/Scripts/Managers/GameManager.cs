using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Chỉ số Người chơi")]
    public int playerHP = 7;
    public int playerCoins = 10;
    public int playerCups = 0;
    public int currentTurn = 1;

    [Header("Điều kiện Thắng/Thua")]
    public int maxTurns = 20;
    public int winConditionCups = 10;

    [Header("Cấu hình Shop")]
    public int rollCost = 2;
    public bool isShopFrozen = false; // Trạng thái đóng băng shop

    [Header("Cấu hình UI & Prefabs")]
    public GameObject cardPrefab;
    public Sprite coinIcon;
    public Sprite cupIcon;

    [Header("Hệ thống Slots")]
    public Transform[] playerSlots; // 6 ô Board nhà
    public Transform[] enemySlots;  // 6 ô Board địch
    public Transform[] shopSlots;   // 3-6 ô Shop
    public Transform[] handSlots;   // 8 ô Bench/Hand

    [Header("Trạng thái Game")]
    public bool isCombatActive = false;

    private CombatResolver resolver = new CombatResolver();

    // Lưu trữ dữ liệu thực thể
    public List<CardInstance> playerBoard = new List<CardInstance>(new CardInstance[6]);
    public List<CardInstance> enemyBoard = new List<CardInstance>(new CardInstance[6]);

    private void Awake() => Instance = this;

    void Start()
    {
        // Khởi tạo lượt 1
        currentTurn = 1;
        playerCups = 0;

        // Cập nhật UI lần đầu (Pha Shop)
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        UIManager.Instance.UpdateUIState(false);

        RefreshShop();
    }

    // --- HỆ THỐNG SHOP & KINH TẾ ---

    public void RollShop()
    {
        if (playerCoins >= rollCost)
        {
            playerCoins -= rollCost;
            isShopFrozen = false; // Roll sẽ phá băng tự động
            RefreshShop();
            UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
            Debug.Log("<color=cyan>[SHOP]</color> Đã đổi bài mới (Tốn 2G)");
        }
        else
        {
            Debug.Log("<color=red>Kinh tế:</color> Không đủ tiền để Roll!");
        }
    }

    public void ToggleLock()
    {
        isShopFrozen = !isShopFrozen;
        Debug.Log(isShopFrozen ? "Đã KHÓA Shop" : "Đã MỞ KHÓA Shop");
        // Bạn có thể gọi thêm hiệu ứng visual tại đây (VD: Đổi màu nút Lock)
    }

    public void RefreshShop()
    {
        // Nếu đang khóa thì không làm mới
        if (isShopFrozen) return;

        // Dọn dẹp slot cũ
        foreach (var slot in shopSlots)
        {
            foreach (Transform child in slot) Destroy(child.gameObject);
        }

        // Lấy bài mới dựa trên Tier (Cứ 2 turn tăng 1 tier)
        int maxTier = (currentTurn / 2) + 1;
        List<CardDefinition> shopData = CardDatabase.Instance.GetRandomShop(3); // Lấy 3 lá

        for (int i = 0; i < shopData.Count; i++)
        {
            if (i < shopSlots.Length)
                CreateCardInSlot(shopData[i], shopSlots[i]);
        }
    }

    private void CreateCardInSlot(CardDefinition data, Transform slot)
    {
        GameObject cardObj = Instantiate(cardPrefab, slot);

        // Setup Transform
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        cardObj.transform.localScale = Vector3.one;

        // Nạp dữ liệu vào CardUI
        CardInstance instance = new CardInstance(data, 0);
        cardObj.GetComponent<CardUI>().Setup(instance);
    }

    public bool TryBuyCard(int cost)
    {
        if (playerCoins >= cost)
        {
            playerCoins -= cost;
            UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
            return true;
        }
        return false;
    }

    public void SellCard()
    {
        playerCoins += 1;
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
    }

    // --- HỆ THỐNG CHIẾN ĐẤU ---

    public void StartCombatPhase()
    {
        if (isCombatActive) return;

        isCombatActive = true;
        UIManager.Instance.UpdateUIState(true); // Chuyển sang mode Combat

        SummonEnemyTeam();
        ProcessCombat();
    }

    public void ExecuteNextTurn()
    {
        currentTurn++;

        // Kiểm tra điều kiện thắng theo Turn
        if (currentTurn > maxTurns) { WinGame(); return; }

        isCombatActive = false;
        playerCoins += 10; // Tiền trợ cấp mỗi lượt

        // Nếu không khóa bài thì làm mới shop
        if (!isShopFrozen) RefreshShop();
        else isShopFrozen = false; // Tự động mở khóa cho lượt tiếp theo

        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        UIManager.Instance.UpdateUIState(false); // Quay lại mode Shop

        Debug.Log($"<color=green>LƯỢT MỚI: {currentTurn}</color>");
    }

    private void SummonEnemyTeam()
    {
        // 1. Dọn sân cũ
        foreach (var slot in enemySlots)
            foreach (Transform child in slot) Destroy(child.gameObject);


        // 2. Lọc danh sách lính
        List<CardDefinition> allUnits = CardDatabase.Instance.GetAllCards().FindAll(c => c.cardType == CardType.Unit);


        if (allUnits.Count == 0) return;

        // 3. Chỉ triệu hồi 3 con ngẫu nhiên (hoặc số lượng tùy ý < 6)
        int countToSummon = 3;
        for (int i = 0; i < countToSummon; i++)
        {
            if (i >= enemySlots.Length) break; // Bảo vệ nếu slot ít hơn số quân muốn triệu hồi

            // Chọn một quân bài ngẫu nhiên từ danh sách lính
            CardDefinition randomUnit = allUnits[Random.Range(0, allUnits.Count)];

            GameObject cardObj = Instantiate(cardPrefab, enemySlots[i]);
            cardObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            cardObj.transform.localScale = Vector3.one;

            if (cardObj.TryGetComponent<CardDraggable>(out var drg)) drg.enabled = false;

            // Lưu vào bộ nhớ chiến đấu
            enemyBoard[i] = new CardInstance(randomUnit, i);
            cardObj.GetComponent<CardUI>().Setup(enemyBoard[i]);
        }
    }

    public void ProcessCombat()
    {
        Debug.Log("<color=white>Đang tính toán kết quả trận đấu...</color>");
        TurnRecord log = new TurnRecord();
        resolver.ResolveTurn(playerBoard, enemyBoard, log);

        bool pAlive = playerBoard.Exists(u => u != null && u.currentHP > 0);
        bool eAlive = enemyBoard.Exists(u => u != null && u.currentHP > 0);

        Debug.Log($"Kết quả: Player còn {playerBoard.Count(u => u != null && u.currentHP > 0)} quân | Enemy còn {enemyBoard.Count(u => u != null && u.currentHP > 0)} quân");

        if (pAlive && !eAlive)
        {
            playerCups++; // Thắng -> Được Cup
            Debug.Log("<color=yellow>THẮNG: Nhận được 1 Cup!</color>");
        }
        else if (!pAlive && eAlive)
        {
            playerHP--; // Thua -> Mất máu
            Debug.Log("<color=red>THUA: Bị trừ 1 HP!</color>");
        }

        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);

        if (playerHP <= 0) GameOver();
        if (playerCups >= winConditionCups) WinGame();


    }

    void WinGame() { Debug.Log("BẠN ĐÃ CHIẾN THẮNG!"); }
    void GameOver() { Debug.Log("GAME OVER!"); }
}