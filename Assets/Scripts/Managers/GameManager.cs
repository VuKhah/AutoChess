using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
    public int bonusCoinNextTurn = 0; // Coin từ magic kinh tế, cộng vào đầu turn sau

    private CombatResolver resolver = new CombatResolver();

    // Dữ liệu thực thể để tính toán
    public List<CardInstance> playerBoard = new List<CardInstance>(new CardInstance[6]);
    public List<CardInstance> enemyBoard = new List<CardInstance>(new CardInstance[6]);

    private void Awake() => Instance = this;

    void Start()
    {
        currentTurn = 1;
        playerCups = 0;
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
            isShopFrozen = false;
            RefreshShop();
            UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        }
    }

    public void ToggleLock() => isShopFrozen = !isShopFrozen;

    //Hàm tính toán Tier dựa trên Turn
    public int GetCurrentShopTier()
    {
        // Công thức: Turn 1-2: Tier 1 | Turn 3-4: Tier 2 | Turn 5-6: Tier 3...
        // Tối đa là Tier 6.
        int calculatedTier = (currentTurn + 1) / 2;
        return Mathf.Clamp(calculatedTier, 1, 6);
    }
    public void RefreshShop()
    {
        if (isShopFrozen) return;
        foreach (var slot in shopSlots)
            foreach (Transform child in slot) Destroy(child.gameObject);

        int currentTier = GetCurrentShopTier();
        List<CardDefinition> shopData = CardDatabase.Instance.GetRandomShop(shopSlots.Length, currentTier);
        for (int i = 0; i < shopData.Count; i++)
        {
            if (i < shopSlots.Length) CreateCardInSlot(shopData[i], shopSlots[i]);
        }
    }

    private void CreateCardInSlot(CardDefinition data, Transform slot)
    {
        GameObject cardObj = Instantiate(cardPrefab, slot);
        cardObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        cardObj.transform.localScale = Vector3.one;
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

    public void SyncBoards()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            CardUI ui = playerSlots[i].GetComponentInChildren<CardUI>();
            playerBoard[i] = (ui != null) ? ui.currentInstance : null;
        }
        for (int i = 0; i < enemySlots.Length; i++)
        {
            CardUI ui = enemySlots[i].GetComponentInChildren<CardUI>();
            enemyBoard[i] = (ui != null) ? ui.currentInstance : null;
        }
    }

    public void StartCombatPhase()
    {
        if (isCombatActive) return;

        // 1. Triệu hồi đối thủ trước
        SummonEnemyTeam();

        // 2. Đồng bộ dữ liệu lên Board tính toán
        SyncBoards();

        isCombatActive = true;
        UIManager.Instance.UpdateUIState(true);

        // 3. Bắt đầu diễn biến
        StartCoroutine(CombatSequence());
    }

    // --- ĐẠO DIỄN TRẬN ĐẤU ---
    private IEnumerator CombatSequence()
    {
        // Giai đoạn A: Chuẩn bị sân khấu (Đảm bảo tọa độ UI chính xác)
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerSlots[0].parent.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemySlots[0].parent.GetComponent<RectTransform>());
        yield return new WaitForEndOfFrame(); // Chờ Unity ổn định vị trí

        // Giai đoạn B: Viết kịch bản (Tính toán toàn bộ trận đấu trong 1 tích tắc)
        TurnRecord combatLog = new TurnRecord();
        // Bộ não CombatResolver sẽ tính toán thắng thua, ripple target, synergy...
        resolver.ResolveTurn(playerBoard, enemyBoard, combatLog);

        // Giai đoạn C: Trình diễn (Đọc từng dòng kịch bản và cho lính đấm nhau)
        foreach (var action in combatLog.actions)
        {
            // VisualizeAction sẽ điều khiển các lá bài bay lên, rung lắc và trừ máu
            yield return StartCoroutine(VisualizeAction(action));

            // Tốc độ giữa các đòn đánh (chỉnh nhỏ lại để trận đấu kịch tính)
            yield return new WaitForSeconds(0.1f);
        }

        // Giai đoạn D: Hạ màn (Chờ 1 giây để người chơi nhìn kết quả cuối cùng)
        yield return new WaitForSeconds(1.0f);

        // Giai đoạn E: Khôi phục & Dọn dẹp
        // 1. Tính toán Cup và HP cho người chơi dựa trên kết quả Board
        CheckVictoryConditions();

        // 2. Hồi máu về mức (Base + Buff vĩnh viễn), hiện lại các lính bị ẩn
        EndCombatAndPrepareNextTurn();

        Debug.Log("<color=yellow>--- TRẬN ĐẤU KẾT THÚC, QUAY LẠI SHOP ---</color>");
    }

    // --- HÀM DIỄN CHI TIẾT TỪNG CÚ ĐẤM ---
    private IEnumerator VisualizeAction(CombatAction action)
    {
        // Tìm "diễn viên" trên sân dựa vào Index và phe tấn công
        CardVisuals attackerVis = (action.isPlayerAttacking ? playerSlots : enemySlots)[action.attackerIdx].GetComponentInChildren<CardVisuals>(true);
        CardVisuals targetVis = (action.isPlayerAttacking ? enemySlots : playerSlots)[action.targetIdx].GetComponentInChildren<CardVisuals>(true);

        // Nếu một trong hai biến mất (đã chết từ action trước) thì bỏ qua
        if (attackerVis == null || targetVis == null) yield break;

        // 1. Lao vào va chạm: Tính trung điểm giữa 2 lá bài
        Vector3 impactPos = (attackerVis.transform.position + targetVis.transform.position) / 2;
        yield return StartCoroutine(attackerVis.AttackAnimation(impactPos, 0.12f));

        // 2. Cập nhật HP: Ép máu của UI khớp chính xác với kết quả Resolver đã tính
        CardUI atkUI = attackerVis.GetComponent<CardUI>();
        CardUI tarUI = targetVis.GetComponent<CardUI>();

        atkUI.currentInstance.currentHP = action.atkHPAfter;
        tarUI.currentInstance.currentHP = action.defHPAfter;

        // Vẽ lại con số máu trên lá bài
        atkUI.Setup(atkUI.currentInstance);
        tarUI.Setup(tarUI.currentInstance);

        // 3. Xử lý tử trận: Nếu máu về 0 thì chạy animation nứt vỡ/ẩn bài
        if (atkUI.currentInstance.IsDead) StartCoroutine(attackerVis.DieAnimation());
        if (tarUI.currentInstance.IsDead) StartCoroutine(targetVis.DieAnimation());

        yield return new WaitForSeconds(0.2f);
    }

    // --- HÀM KHÔI PHỤC TRẠNG THÁI ---
    public void EndCombatAndPrepareNextTurn()
    {
        // Duyệt qua toàn bộ ô Board và Hand để hồi máu
        ResetAllCardsInSlots(playerSlots);
        ResetAllCardsInSlots(handSlots);

        // Gọi logic chuyển sang lượt mới (cộng tiền, roll shop...)
        ExecuteNextTurn();
    }

    private void ResetAllCardsInSlots(Transform[] slots)
    {
        foreach (var slot in slots)
        {
            // Tìm CardUI (kể cả những cái đang bị Deactivate)
            CardUI ui = slot.GetComponentInChildren<CardUI>(true);
            if (ui != null)
            {
                // Reset máu về (Gốc + Buff vĩnh viễn)
                ui.currentInstance.ResetStats();

                // Hiện lại lá bài, reset độ mờ và kích thước
                CardVisuals vis = ui.GetComponent<CardVisuals>();
                if (vis != null) vis.ResetVisuals();

                // Vẽ lại UI sạch sẽ
                ui.Setup(ui.currentInstance);
            }
        }
    }
    private void CheckVictoryConditions()
    {
        bool pAlive = playerBoard.Any(u => u != null && !u.IsDead);
        bool eAlive = enemyBoard.Any(u => u != null && !u.IsDead);

        if (pAlive && !eAlive) playerCups++;
        else if (!pAlive && eAlive) playerHP--;

        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);

        if (playerHP <= 0) GameOver();
        if (playerCups >= winConditionCups) WinGame();
    }
    void FinalCleanup()
    {
        for (int i = 0; i < 6; i++)
        {
            if (playerBoard[i] != null && playerBoard[i].IsDead) playerBoard[i] = null;
            if (enemyBoard[i] != null && enemyBoard[i].IsDead) enemyBoard[i] = null;
        }
    }

    private void SummonEnemyTeam()
    {
        foreach (var slot in enemySlots)
            foreach (Transform child in slot) Destroy(child.gameObject);

        List<CardDefinition> allUnits = CardDatabase.Instance.GetAllCards().FindAll(c => c.cardType == CardType.Unit);
        if (allUnits.Count == 0) return;

        int count = 3;
        for (int i = 0; i < count; i++)
        {
            if (i >= enemySlots.Length) break;
            CardDefinition data = allUnits[Random.Range(0, allUnits.Count)];
            GameObject cardObj = Instantiate(cardPrefab, enemySlots[i]);
            cardObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            cardObj.transform.localScale = Vector3.one;
            if (cardObj.TryGetComponent<CardDraggable>(out var drg)) drg.enabled = false;

            CardInstance instance = new CardInstance(data, i);
            cardObj.GetComponent<CardUI>().Setup(instance);
        }
    }

    public void ExecuteNextTurn()
    {
        currentTurn++;
        if (currentTurn > maxTurns) { WinGame(); return; }
        isCombatActive = false;
        // 1. Reset về đúng 10 Coin cố định + bonus coin từ magic lượt trước
        playerCoins = 10 + bonusCoinNextTurn;
        bonusCoinNextTurn = 0;

        // 2. Tính Coin từ các Unit Kinh tế đang có trên bàn cờ
        foreach (var unit in playerBoard)
        {
            if (unit != null && !unit.IsDead && unit.Data.ability != null)
            {
                if (unit.Data.ability.trigger == TriggerType.StartOfBattle && unit.Data.ability.effect == EffectType.GainCoin)
                {
                    playerCoins += unit.Data.ability.effectValue1;
                    Debug.Log($"<color=yellow>[ECONOMY]</color> {unit.Data.cardName} đào được {unit.Data.ability.effectValue1} Coin!");
                }
            }
        }

        // 3. Refresh Shop sau khi đã có tổng số tiền chính xác
        if (!isShopFrozen) RefreshShop();
        else isShopFrozen = false;
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        UIManager.Instance.UpdateUIState(false);
    }

    void WinGame() { Debug.Log("BẠN ĐÃ CHIẾN THẮNG!"); }
    void GameOver() { Debug.Log("GAME OVER!"); }
}