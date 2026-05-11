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

        // 3. XỬ LÝ RIÊNG CHO BÀI PHÉP (MAGIC)
        if (draggedUI.currentInstance.Data.cardType == CardType.Magic) // 1 là Magic
        {
            HandleMagicDrop(draggedCard, draggedUI, sourceSlot);
            return;
        }

        // 4. XỬ LÝ CHO ĐƠN VỊ (UNIT)
        HandleUnitDrop(draggedCard, draggedUI, sourceSlot);
    }

    private void HandleMagicDrop(CardDraggable magicCard, CardUI magicUI, CardSlot sourceSlot)
    {
        // Tìm xem ô này có lính (Unit) nào đang đứng không
        CardUI targetUnitUI = GetComponentInChildren<CardUI>();

        // TH 1: Thả bài phép lên một Unit (để kích hoạt phép)
        if (targetUnitUI != null && targetUnitUI.currentInstance.Data.cardType == CardType.Unit)
        {
            // Nếu mua từ Shop thì phải trừ tiền trước
            if (sourceSlot != null && sourceSlot.slotType == SlotType.Shop)
            {
                if (!GameManager.Instance.TryBuyCard(magicUI.currentInstance.Data.cost)) return;
            }

            // Kích hoạt hiệu ứng
            GameManager.Instance.resolver.ApplyMagicToUnit(magicUI.currentInstance, targetUnitUI.currentInstance);

            // Cập nhật lại hiển thị cho Unit (để nhảy số ATK/HP mới)
            targetUnitUI.Setup(targetUnitUI.currentInstance);

            // Xóa lá bài phép sau khi dùng
            Destroy(magicCard.gameObject);
            Debug.Log("<color=purple>Magic:</color> Đã sử dụng phép lên " + targetUnitUI.currentInstance.Data.cardName);
        }
        // TH 2: Thả bài phép vào ô Hand (để mua về cất túi hoặc sắp xếp túi)
        else if (this.slotType == SlotType.Hand && this.transform.childCount == 0)
        {
            if (sourceSlot != null && sourceSlot.slotType == SlotType.Shop)
            {
                if (GameManager.Instance.TryBuyCard(magicUI.currentInstance.Data.cost))
                {
                    magicCard.parentReturnTo = this.transform;
                }
            }
            else
            {
                magicCard.parentReturnTo = this.transform;
            }
        }
        // TH 3: Thả bài phép vào ô Board trống -> KHÔNG CHO PHÉP
        else
        {
            Debug.Log("Bài phép phải dùng lên Unit hoặc để trong Hand!");
        }
    }

    private void HandleUnitDrop(CardDraggable unitCard, CardUI unitUI, CardSlot sourceSlot)
    {
        // --- BÁN BÀI ---
        if (this.slotType == SlotType.Shop)
        {
            if (sourceSlot != null && sourceSlot.slotType != SlotType.Shop)
            {
                // [THÊM MỚI] BẮN SỰ KIỆN ONSELL TRƯỚC KHI BAY MÀU
                if (unitUI.currentInstance != null)
                {
                    // Sync trước khi bắn: lá bài vẫn đang nằm ở slot cũ nên SyncBoards bắt được
                    GameManager.Instance.SyncBoards();
                    Debug.Log($"<color=red>[EVENT]</color> Bắn sự kiện OnSell cho {unitUI.currentInstance.Data.cardName}");
                    GameManager.Instance.resolver.TriggerAbility(
                        TriggerType.OnSell,
                        unitUI.currentInstance,
                        null,
                        GameManager.Instance.playerBoard,
                        GameManager.Instance.enemyBoard  // BUG FIX: enemyBoard luôn là list 6-null, không bao giờ null
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
            // Sync board từ UI hiện tại (lá bài đã ở slot mới sau yield)
            GameManager.Instance.SyncBoards();
            TriggerOnDeploy(deployedUI);
        }
        CheckForMerge(cardID, mergeLevel);
    }

    private void CheckForMerge(string cardID, int mergeLevel)
    {
        List<CardUI> matches = new List<CardUI>();

        foreach (var slot in GameManager.Instance.playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui != null && ui.currentInstance.Data.cardID == cardID
                && ui.currentInstance.mergeLevel == mergeLevel
                && !ui.currentInstance.isBattleSpawned)
                matches.Add(ui);
        }
        foreach (var slot in GameManager.Instance.handSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui != null && ui.currentInstance.Data.cardID == cardID
                && ui.currentInstance.mergeLevel == mergeLevel
                && !ui.currentInstance.isBattleSpawned)
                matches.Add(ui);
        }

        if (matches.Count >= 3)
            PerformMerge(matches);
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
        for (int i = 0; i < 3; i++)
        {
            var inst = cards[i].currentInstance;
            int totalBonus = inst.permanentATKBonus + inst.permanentHPBonus
                           + inst.growthATKBonus    + inst.growthHPBonus;
            if (totalBonus > bestBonus) { bestBonus = totalBonus; keeperIdx = i; }
        }

        CardUI keeper = cards[keeperIdx];
        keeper.currentInstance.mergeLevel++;
        keeper.currentInstance.ResetStats();
        keeper.Setup(keeper.currentInstance);

        for (int i = 0; i < 3; i++)
        {
            if (i != keeperIdx) Destroy(cards[i].gameObject);
        }

        Debug.Log($"<color=gold>[MERGE]</color> 3x {keeper.currentInstance.Data.cardName} hợp nhất thành cấp {keeper.currentInstance.mergeLevel + 1}! (keeper: slot bonus={bestBonus})");

        CardVisuals vis = keeper.GetComponent<CardVisuals>();
        if (vis != null) StartCoroutine(vis.BurstAnimation());

        GameManager.Instance.SyncBoards();

        // Kiểm tra tiếp nếu vừa tạo ra quân đủ bộ 3 ở cấp mới
        StartCoroutine(CheckMergeNextFrame(keeper.currentInstance.Data.cardID, keeper.currentInstance.mergeLevel));
    }
}