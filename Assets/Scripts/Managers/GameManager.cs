using System.Collections.Generic;
using UnityEngine;

public partial class GameManager : MonoBehaviour
{
    public const int BoardSlotCount = 7;

    public static GameManager Instance;

    [Header("Chỉ số Người chơi")]
    public int playerHP = 7;
    public int playerCups = 0;
    public int currentTurn = 1;

    private readonly EconomyManager economy = new EconomyManager();
    public int playerCoins => economy.CurrentCoin;
    public int permanentIncomeBonus => economy.PermanentIncomeBonus;

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
    public int shopUnitCount = 5;   // Số slot unit trong shop (các slot còn lại là spell)
    [HideInInspector] public int pendingWagerCoins = 0;

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
    private readonly List<BotAgent> enemyBots = new List<BotAgent>();
    private int currentOpponentIndex = 0;
    private BotAgent CurrentOpponent => enemyBots.Count > 0 ? enemyBots[currentOpponentIndex] : null;

    public CombatResolver resolver = new CombatResolver();

    public List<CardInstance> playerBoard = new List<CardInstance>(new CardInstance[BoardSlotCount]);
    public List<CardInstance> enemyBoard  = new List<CardInstance>(new CardInstance[BoardSlotCount]);

    private void Awake()
    {
        Instance = this;
        EnsureBoardCapacity(playerBoard);
        EnsureBoardCapacity(enemyBoard);
    }

    private static void EnsureBoardCapacity(List<CardInstance> board)
    {
        while (board.Count < BoardSlotCount)
            board.Add(null);
        while (board.Count > BoardSlotCount)
            board.RemoveAt(board.Count - 1);
    }

    public void SetDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        enemyBots.Clear();
        currentOpponentIndex = 0;

        var lib = AIManager.Instance?.loadedLibrary;
        if (lib == null) { Debug.LogWarning("[AI] AILibrary chưa sẵn sàng — dùng đội ngẫu nhiên."); return; }

        // 3 đối thủ cố định: Easy / Medium / Hard brain, handicap coin khác nhau
        AddBot(lib.easyBot,    7,  "Easy");
        AddBot(lib.mediumBot,  9,  "Medium");
        AddBot(lib.hardBot,   10,  "Hard");

        Debug.Log($"<color=cyan>[AI]</color> {enemyBots.Count} đối thủ sẵn sàng (độ khó chọn: {difficulty})");
    }

    private void AddBot(Chromosome brain, int coins, string label)
    {
        if (brain?.genes != null && brain.genes.Length >= Chromosome.GeneCount)
            enemyBots.Add(new BotAgent(brain, coins));
        else
            Debug.LogWarning($"[AI] Brain '{label}' không hợp lệ — bỏ qua đối thủ này.");
    }

    void Start()
    {
        currentTurn = 1;
        playerCups  = 0;
        economy.ResetEconomy();
        SetDifficulty(selectedDifficulty);
        SetupShopSummonObserver();
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

        // Kích EndTurnShop cho player (qua TTE engine đầy đủ)
        foreach (var unit in playerBoard)
        {
            if (unit != null && !unit.IsDead)
                resolver.TriggerAbility(TriggerType.EndTurnShop, unit, null, playerBoard, enemyBoard);
        }

        // Kích EndTurnShop cho tất cả đối thủ (growth tích lũy giống player)
        foreach (var bot in enemyBots) bot.TriggerEndTurnShop();

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
        // Phát SFX TRƯỚC StopAllCoroutines để không bị race condition
        if (AudioManager.Instance != null) { AudioManager.Instance.StopBGM(); AudioManager.Instance.Win(); }
        StopAllCoroutines();
        Debug.Log("<color=yellow>BẠN ĐÃ CHIẾN THẮNG!</color>");
        UIManager.Instance.ShowVictory();
    }

    void GameOver()
    {
        if (isGameEnded) return;
        isGameEnded    = true;
        isCombatActive = false;
        // Phát SFX TRƯỚC StopAllCoroutines để không bị race condition
        if (AudioManager.Instance != null) { AudioManager.Instance.StopBGM(); AudioManager.Instance.Lose(); }
        StopAllCoroutines();
        Debug.Log("<color=red>GAME OVER!</color>");
        UIManager.Instance.ShowGameOver();
    }
}
