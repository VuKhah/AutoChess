using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardSlot : MonoBehaviour, IDropHandler
{
    public enum SlotType { Shop, PlayerBoard, EnemyBoard, Hand }
    public SlotType slotType;

    public void OnDrop(PointerEventData eventData)
    {
        // 1. Lấy thông tin lá bài đang được kéo
        CardDraggable draggedCard = eventData.pointerDrag.GetComponent<CardDraggable>();
        if (draggedCard == null) return;

        CardUI draggedUI = draggedCard.GetComponent<CardUI>();
        CardSlot sourceSlot = draggedCard.parentReturnTo.GetComponent<CardSlot>();

        // 2. Kiểm tra an ninh: Không cho phép tương tác với sân đối thủ
        if (this.slotType == SlotType.EnemyBoard) return;

        // 3. XỬ LÝ RIÊNG CHO BÀI PHÉP (SPELL)
        if (draggedUI.currentInstance.Data.cardType == CardType.Spell)
        {
            HandleSpellDrop(draggedCard, draggedUI, sourceSlot);
            return;
        }

        // 4. XỬ LÝ CHO ĐƠN VỊ (UNIT)
        HandleUnitDrop(draggedCard, draggedUI, sourceSlot);
    }

    private void HandleSpellDrop(CardDraggable spellCard, CardUI spellUI, CardSlot sourceSlot)
    {
        CardUI targetUnitUI = GetComponentInChildren<CardUI>();

        // TH 1: Thả bài phép lên một Unit (để kích hoạt phép)
        if (targetUnitUI != null && targetUnitUI.currentInstance.Data.cardType == CardType.Unit)
        {
            if (sourceSlot != null && sourceSlot.slotType == SlotType.Shop)
            {
                if (!GameManager.Instance.TryBuyCard(spellUI.currentInstance.Data.cost)) return;
            }

            GameManager.Instance.resolver.ApplySpellToUnit(spellUI.currentInstance, targetUnitUI.currentInstance);
            AudioManager.Instance?.Spell();
            targetUnitUI.Setup(targetUnitUI.currentInstance);
            Destroy(spellCard.gameObject);
            Debug.Log("<color=purple>Spell:</color> Đã sử dụng phép lên " + targetUnitUI.currentInstance.Data.cardName);
        }
        // TH 2: Thả bài phép vào ô Hand (để mua về cất túi hoặc sắp xếp túi)
        else if (this.slotType == SlotType.Hand && this.transform.childCount == 0)
        {
            if (sourceSlot != null && sourceSlot.slotType == SlotType.Shop)
            {
                if (GameManager.Instance.TryBuyCard(spellUI.currentInstance.Data.cost))
                    spellCard.parentReturnTo = this.transform;
            }
            else
            {
                spellCard.parentReturnTo = this.transform;
            }
        }
        // TH 3: Spell không cần target → cast ngay khi thả vào ô trống (Board hoặc Hand)
        else if (IsTargetlessSpell(spellUI.currentInstance.Data))
        {
            if (sourceSlot != null && sourceSlot.slotType == SlotType.Shop)
            {
                if (!GameManager.Instance.TryBuyCard(spellUI.currentInstance.Data.cost)) return;
            }
            GameManager.Instance.resolver.ApplySpellToUnit(spellUI.currentInstance, null);
            AudioManager.Instance?.Spell();
            Destroy(spellCard.gameObject);
            Debug.Log("<color=purple>Spell:</color> Đã kích hoạt " + spellUI.currentInstance.Data.cardName + " (không cần target).");
        }
        else
        {
            Debug.Log("Bài phép phải dùng lên Unit hoặc để trong Hand!");
        }
    }

    // Spell không cần chọn unit target: effects chỉ thuộc nhóm economy/random (6, 10, 11, 12, 14, 16)
    private static bool IsTargetlessSpell(CardDefinition data)
    {
        if (data?.spellEffects == null || data.spellEffects.Count == 0) return false;
        foreach (var fx in data.spellEffects)
        {
            int e = fx.effect;
            if (e != 6 && e != 10 && e != 11 && e != 12 && e != 14 && e != 16)
                return false;
        }
        return true;
    }

    private void HandleUnitDrop(CardDraggable unitCard, CardUI unitUI, CardSlot sourceSlot)
    {
        // --- BÁN BÀI ---
        if (this.slotType == SlotType.Shop)
        {
            if (sourceSlot != null && sourceSlot.slotType != SlotType.Shop)
            {
                // Bắn OnSell + broadcast OnAllySell trước khi hủy card
                if (unitUI.currentInstance != null)
                {
                    GameManager.Instance.SyncBoards();
                    Debug.Log($"<color=red>[EVENT]</color> Bắn sự kiện OnSell cho {unitUI.currentInstance.Data.cardName}");
                    GameManager.Instance.resolver.TriggerAbility(
                        TriggerType.OnSell,
                        unitUI.currentInstance,
                        null,
                        GameManager.Instance.playerBoard,
                        GameManager.Instance.enemyBoard
                    );
                    GameManager.Instance.resolver.BroadcastAllyEvent(
                        TriggerType.OnAllySell,
                        unitUI.currentInstance,
                        GameManager.Instance.playerBoard,
                        GameManager.Instance.enemyBoard
                    );
                }

                GameManager.Instance.SellCard();
                Destroy(unitCard.gameObject);
            }
            return;
        }

        // --- KIỂM TRA Ô TRỐNG ---
        if (this.transform.childCount > 0) return;

        // --- MUA BÀI ---
        if (sourceSlot != null && sourceSlot.slotType == SlotType.Shop)
        {
            if (this.slotType == SlotType.Hand || this.slotType == SlotType.PlayerBoard)
            {
                if (GameManager.Instance.TryBuyCard(unitUI.currentInstance.Data.cost))
                {
                    unitCard.parentReturnTo = this.transform;

                    bool shouldDeploy = this.slotType == SlotType.PlayerBoard;
                    StartCoroutine(CheckMergeNextFrame(unitUI.currentInstance.Data.cardID, unitUI.currentInstance.mergeLevel, shouldDeploy, unitUI));
                }
            }
        }
        // --- DI CHUYỂN BÀI ---
        else if (this.slotType == SlotType.Hand || this.slotType == SlotType.PlayerBoard)
        {
            unitCard.parentReturnTo = this.transform;
            AudioManager.Instance?.MoveCard();

            // Chỉ bắn OnDeploy khi kéo từ Hand lên Board (không phải đổi chỗ trong Board)
            bool shouldDeploy = this.slotType == SlotType.PlayerBoard && sourceSlot.slotType != SlotType.PlayerBoard;
            StartCoroutine(CheckMergeNextFrame(unitUI.currentInstance.Data.cardID, unitUI.currentInstance.mergeLevel, shouldDeploy, unitUI));
        }
    }

    // ==========================================
    // HỆ THỐNG TRIGGER HỖ TRỢ
    // ==========================================
    private void TriggerOnDeploy(CardUI unitUI)
    {
        if (unitUI.currentInstance != null)
        {
            Debug.Log($"<color=green>[EVENT]</color> Bắn sự kiện OnDeploy cho {unitUI.currentInstance.Data.cardName}");
            GameManager.Instance.resolver.TriggerAbility(
                TriggerType.OnDeploy,
                unitUI.currentInstance,
                null,
                GameManager.Instance.playerBoard,
                GameManager.Instance.enemyBoard
            );
        }
    }

    // ==========================================
    // HỆ THỐNG MERGE
    // ==========================================

    private IEnumerator CheckMergeNextFrame(string cardID, int mergeLevel, bool shouldDeploy = false, CardUI deployedUI = null)
    {
        yield return null; // Chờ OnEndDrag reparent lá bài vào slot mới
        if (shouldDeploy && deployedUI != null)
        {
            bool isRepeating = deployedUI.currentInstance.Data.isRepeatingDeploy;
            bool alreadyDeployed = !isRepeating && deployedUI.currentInstance.hasDeployed;

            if (alreadyDeployed)
            {
                // Đã deploy rồi (cùng merge level) — chỉ sync vị trí, không kích OnDeploy lại
                GameManager.Instance.SyncBoards();
            }
            else
            {
                if (!isRepeating) deployedUI.currentInstance.hasDeployed = true;
                // Sync board từ UI hiện tại (lá bài đã ở slot mới sau yield) — cập nhật slotIndex
                GameManager.Instance.SyncBoards();
                TriggerOnDeploy(deployedUI);
                // Broadcast OnAllyDeploy cho các unit khác trên board (VD: Utu phản ứng)
                GameManager.Instance.resolver.BroadcastAllyEvent(
                    TriggerType.OnAllyDeploy,
                    deployedUI.currentInstance,
                    GameManager.Instance.playerBoard,
                    GameManager.Instance.enemyBoard);
                // Flush pending summons từ OnDeploy (VD: shop-phase summon cards)
                GameManager.Instance.resolver.FlushShopPendingSummons(
                    GameManager.Instance.playerBoard, GameManager.Instance.enemyBoard);
                // Xóa UI của unit bị Consume/Destroy bởi OnDeploy ability (ví dụ: Asag, Upamaki)
                GameManager.Instance.CleanupDeadBoardUnitsUI();
            }
        }
        CheckForMerge(cardID, mergeLevel);
        // Đảm bảo shop blink hint luôn cập nhật sau buy/move (kể cả khi vào Hand)
        GameManager.Instance.SyncBoards();
    }

    private void CheckForMerge(string cardID, int mergeLevel)
    {
        List<CardUI> matches = new List<CardUI>();
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (gm.playerSlots != null)
            foreach (var slot in gm.playerSlots)
            {
                if (slot == null) continue;
                CardUI ui = slot.GetComponentInChildren<CardUI>();
                if (ui != null && ui.currentInstance?.Data?.cardID == cardID
                    && ui.currentInstance.mergeLevel == mergeLevel
                    && !ui.currentInstance.isBattleSpawned)
                    matches.Add(ui);
            }

        if (gm.handSlots != null)
            foreach (var slot in gm.handSlots)
            {
                if (slot == null) continue;
                CardUI ui = slot.GetComponentInChildren<CardUI>();
                if (ui != null && ui.currentInstance?.Data?.cardID == cardID
                    && ui.currentInstance.mergeLevel == mergeLevel
                    && !ui.currentInstance.isBattleSpawned)
                    matches.Add(ui);
            }

        int required = CardInstance.MergeRequiredCount(mergeLevel);
        if (matches.Count >= required)
        {
            // Trim về đúng số cần thiết — tránh tiêu thụ bản sao dư khi player có nhiều hơn mức cần
            var toMerge = matches.Count > required ? matches.GetRange(0, required) : matches;
            PerformMerge(toMerge);
        }
    }

    private void PerformMerge(List<CardUI> cards)
    {
        // BUG FIX: Chọn keeper là lá bài có tổng bonus cao nhất thay vì luôn lấy cards[0].
        // Đảm bảo: merged unit luôn mạnh hơn bất kỳ nguyên liệu nào (không bao giờ stat regression).
        // Công thức: currentATK = baseATK × tier + 0.7 × (permanent + growth)
        // Khi tier tăng 1 (mergeLevel++), phần base tăng nhưng phần bonus giữ nguyên.
        // Nếu keeper có bonus thấp hơn một nguyên liệu khác → merged < nguyên liệu đó.
        int keeperIdx = 0;
        int bestBonus = int.MinValue;
        for (int i = 0; i < cards.Count; i++)
        {
            var inst = cards[i].currentInstance;
            int totalBonus = inst.permanentATKBonus + inst.permanentHPBonus
                           + inst.growthATKBonus    + inst.growthHPBonus;
            if (totalBonus > bestBonus) { bestBonus = totalBonus; keeperIdx = i; }
        }

        CardUI keeper = cards[keeperIdx];
        keeper.currentInstance.mergeLevel++;
        keeper.currentInstance.ResetStats();
        keeper.currentInstance.hasDeployed = false; // Cho phép OnDeploy lại sau khi merge
        keeper.Setup(keeper.currentInstance);

        for (int i = 0; i < cards.Count; i++)
        {
            if (i != keeperIdx) Destroy(cards[i].gameObject);
        }

        Debug.Log($"<color=gold>[MERGE]</color> {cards.Count}x {keeper.currentInstance.Data.cardName} hợp nhất thành cấp {keeper.currentInstance.mergeLevel + 1}! (keeper: slot bonus={bestBonus})");

        AudioManager.Instance?.StarUp();

        CardVisuals vis = keeper.GetComponent<CardVisuals>();
        if (vis != null) StartCoroutine(vis.BurstAnimation());

        GameManager.Instance.SyncBoards();

        // Kiểm tra tiếp nếu vừa tạo ra quân đủ bộ 3 ở cấp mới
        StartCoroutine(CheckMergeNextFrame(keeper.currentInstance.Data.cardID, keeper.currentInstance.mergeLevel));
    }
}