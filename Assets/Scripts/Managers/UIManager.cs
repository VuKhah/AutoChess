using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Các Panel chính")]
    public GameObject shopPanel;
    public GameObject enemyBoardPanel;
    public GameObject handPanel;
    public Image backgroundImage;
    public Sprite shopPhaseBackground;
    public Sprite combatPhaseBackground;

    [Header("Chỉ số hiển thị")]
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerCupText; // Luôn hiển thị số Cup
    public TextMeshProUGUI resourceText; // Hiển thị số Coin hoặc số Cup
    public Image resourceIcon;           // Icon thay đổi giữa Coin/Cup

    [Header("Hệ thống Nút bấm")]
    public Button actionButton;          // Nút Start / Next Turn
    public TextMeshProUGUI actionText;   // Chữ trên nút Action
    public Button rollButton;
    public Button lockButton;
    public Image lockButtonImage;        // Để đổi màu nút khi khóa bài

    [Header("Màu sắc giao diện")]
    public Color lockActiveColor = Color.cyan;
    public Color lockNormalColor = Color.white;

    [Header("End Game")]
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameText;
    public Button restartButton;

    [Header("AI Difficulty")]
    public GameObject difficultyPanel;
    public Button easyBtn;
    public Button mediumBtn;
    public Button hardBtn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Gán sự kiện cho các nút bấm
        actionButton?.onClick.AddListener(OnActionPressed);
        rollButton?.onClick.AddListener(OnRollPressed);
        lockButton?.onClick.AddListener(OnLockPressed);
        if (restartButton != null)
            restartButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        easyBtn?.onClick.AddListener(()   => SelectDifficulty("Easy"));
        mediumBtn?.onClick.AddListener(() => SelectDifficulty("Medium"));
        hardBtn?.onClick.AddListener(()   => SelectDifficulty("Hard"));
        if (difficultyPanel != null) difficultyPanel.SetActive(true);

        // Build sẵn panel bán card (ẩn) để Drop nhanh không bị giật frame đầu
        SellZone.EnsureExists();
    }

    /// <summary>
    /// Cập nhật các chỉ số cơ bản lên màn hình
    /// </summary>
    public void UpdateStats(int hp, int cups, int coins)
    {
        if (playerHPText != null)
            playerHPText.text = hp.ToString();

        if (playerCupText != null)
            playerCupText.text = cups.ToString();

        // Nếu đang trong trận đấu thì hiện Cup, nếu ở Shop thì hiện Coin
        if (resourceText == null)
            return;

        if (GameManager.Instance.isCombatActive)
        {
            resourceText.text = cups.ToString();
        }
        else
        {
            int incomeBonus = GameManager.Instance.permanentIncomeBonus;
            resourceText.text = incomeBonus > 0
                ? $"{coins}G (+{incomeBonus})"
                : coins.ToString() + "G";
        }
    }

    /// <summary>
    /// Thay đổi trạng thái toàn bộ UI giữa Shop và Combat
    /// </summary>
    public void UpdateUIState(bool isCombat)
    {
        // 1. Chuyển đổi Panel
        shopPanel.SetActive(!isCombat);
        enemyBoardPanel.SetActive(isCombat);
        if (handPanel != null)
        {
            handPanel.SetActive(!isCombat);
            if (!isCombat)
            {
                // Bottom_Zone là cha của handPanel; gắn hiệu ứng chia bài lên đó
                Transform bottomZone = handPanel.transform.parent != null ? handPanel.transform.parent : handPanel.transform;
                BottomZoneDeal deal = bottomZone.GetComponent<BottomZoneDeal>();
                if (deal == null) deal = bottomZone.gameObject.AddComponent<BottomZoneDeal>();
                deal.Play();
            }
        }
        if (backgroundImage != null)
            backgroundImage.sprite = isCombat ? combatPhaseBackground : shopPhaseBackground;

        BattlePhaseLayout.Instance?.ApplyPhase(isCombat);

        if (AudioManager.Instance != null)
        {
            if (isCombat) AudioManager.Instance.PlayCombatBGM();
            else          AudioManager.Instance.PlayPrepBGM();
        }

        // 2. Chuyển đổi Icon và Text tài nguyên
        if (resourceIcon != null)
            resourceIcon.sprite = isCombat ? GameManager.Instance.cupIcon : GameManager.Instance.coinIcon;

        // 3. Ẩn/Hiện các nút chức năng Shop
        if (rollButton != null)
            rollButton.gameObject.SetActive(!isCombat);
        if (lockButton != null)
            lockButton.gameObject.SetActive(!isCombat);
        if (actionButton != null)
            actionButton.interactable = !isCombat;

        // 4. Thay đổi nội dung nút hành động chính
        if (actionText != null)
            actionText.text = isCombat ? "LƯỢT TIẾP" : "BẮT ĐẦU";

        // Cập nhật lại chỉ số ngay lập tức để tránh bị trễ hiển thị
        UpdateStats(GameManager.Instance.playerHP, GameManager.Instance.playerCups, GameManager.Instance.playerCoins);

        // ShopTierPanel chỉ hiện khi ở phase Shop
        if (ShopTierPanel.Instance != null)
        {
            ShopTierPanel.Instance.gameObject.SetActive(!isCombat);
            if (!isCombat) ShopTierPanel.Instance.Refresh();
        }
    }

    // --- CÁC SỰ KIỆN NÚT BẤM ---

    private void OnActionPressed()
    {
        if (GameManager.Instance.isCombatActive)
        {
            Debug.Log("<color=grey>UI: Ignore action while combat is active.</color>");
            return;
        }

        if (!GameManager.Instance.isCombatActive)
        {
            // Nếu đang ở Shop -> Nhấn để Bắt đầu Combat
            Debug.Log("<color=yellow>UI: Bắt đầu trận đấu!</color>");
            GameManager.Instance.StartCombatPhase();
        }
        else
        {
            // Nếu đang ở Combat -> Nhấn để sang Lượt mới
            Debug.Log("<color=green>UI: Chuyển sang lượt tiếp theo.</color>");
            GameManager.Instance.ExecuteNextTurn();
        }
    }

    private void OnRollPressed()
    {
        GameManager.Instance.RollShop();
    }

    private void OnLockPressed()
    {
        GameManager.Instance.ToggleLock();

        // Thay đổi màu sắc nút Lock để người chơi biết đang ở trạng thái nào
        if (lockButtonImage != null)
        {
            lockButtonImage.color = GameManager.Instance.isShopFrozen ? lockActiveColor : lockNormalColor;
        }
    }

    private void SelectDifficulty(string difficulty)
    {
        GameManager.Instance.SetDifficulty(difficulty);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
    }

    private void ShowEndGame(string message)
    {
        if (endGamePanel != null) endGamePanel.SetActive(true);
        if (endGameText  != null) endGameText.text = message;
        if (actionButton != null) actionButton.interactable = false;
        if (rollButton != null) rollButton.interactable = false;
        if (lockButton != null) lockButton.interactable = false;
    }

    public void ShowVictory()  => ShowEndGame("CHIẾN THẮNG!");
    public void ShowGameOver() => ShowEndGame("THUA CUỘC!");
}
