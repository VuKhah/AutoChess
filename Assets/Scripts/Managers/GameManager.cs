using System.Collections.Generic;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Chỉ số Người chơi")]
    public int playerHP = 7;
    public int playerCups = 0;
    public int currentTurn = 1;

    private readonly EconomyManager economy = new EconomyManager();
    public int playerCoins => economy.CurrentCoin;

    // Dùng bởi AbilityEngine khi kỹ năng cộng coin ngay lập tức
    public void AddCoin(int amount) { economy.Earn(amount); UpdateCoinUI(); }
    // Dùng bởi AbilityEngine khi phép cộng coin vào đầu lượt sau
    public void AddBonusCoin(int amount) => economy.AddBonus(amount);

    [Header("Điều kiện Thắng/Thua")]
    public int maxTurns = 20;
    public int winConditionCups = 10;

    [Header("Cấu hình Shop")]
    public int rollCost = 1;
    public bool isShopFrozen = false;

    [Header("Cấu hình UI & Prefabs")]
    public GameObject cardPrefab;
    public Sprite coinIcon;
    public Sprite cupIcon;

    [Header("Hệ thống Slots")]
    public Transform[] playerSlots;
    public Transform[] enemySlots;
    public Transform[] shopSlots;
    public Transform[] handSlots;

    [Header("Trạng thái Game")]
    public bool isCombatActive = false;
    private bool isGameEnded   = false;

    // Snapshot sân player cuối Shop Phase — đảm bảo chỉ các unit gốc tồn tại qua turn
    private (int slotIdx, CardInstance unit)[] preCombatSnapshot;

    [Header("AI Difficulty")]
    public string selectedDifficulty = "Medium";
    private BotAgent enemyBot;

    public CombatResolver resolver = new CombatResolver();

    public List<CardInstance> playerBoard = new List<CardInstance>(new CardInstance[6]);
    public List<CardInstance> enemyBoard  = new List<CardInstance>(new CardInstance[6]);

    private void Awake() => Instance = this;

    public void SetDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        if (AIManager.Instance != null && AIManager.Instance.loadedLibrary != null)
            enemyBot = new BotAgent(AIManager.Instance.GetBrain(difficulty));
        Debug.Log($"<color=cyan>[AI]</color> Độ khó đặt thành: {difficulty}");
    }

    void Start()
    {
        currentTurn = 1;
        playerCups  = 0;
        economy.ResetEconomy();
        SetDifficulty(selectedDifficulty);
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        UIManager.Instance.UpdateUIState(false);
        RefreshShop();
    }

    public void UpdateCoinUI()
    {
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
    }

    public void ExecuteNextTurn()
    {
        if (isGameEnded) return;
        currentTurn++;
        if (currentTurn > maxTurns) { WinGame(); return; }
        isCombatActive = false;
        // Reset về đúng 10 Coin cố định + bonus coin từ spell lượt trước
        economy.ResetEconomy();

        // Kích EndTurnShop cho tất cả unit trên sân qua TTE engine
        foreach (var unit in playerBoard)
        {
            if (unit != null && !unit.IsDead)
                resolver.TriggerAbility(TriggerType.EndTurnShop, unit, null, playerBoard, enemyBoard);
        }

        if (!isShopFrozen) RefreshShop();
        else isShopFrozen = false;
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        UIManager.Instance.UpdateUIState(false);
    }

    void WinGame()
    {
        if (isGameEnded) return;
        isGameEnded    = true;
        isCombatActive = false;
        StopAllCoroutines();
        Debug.Log("<color=yellow>BẠN ĐÃ CHIẾN THẮNG!</color>");
        UIManager.Instance.ShowVictory();
    }

    void GameOver()
    {
        if (isGameEnded) return;
        isGameEnded    = true;
        isCombatActive = false;
        StopAllCoroutines();
        Debug.Log("<color=red>GAME OVER!</color>");
        UIManager.Instance.ShowGameOver();
    }
}
