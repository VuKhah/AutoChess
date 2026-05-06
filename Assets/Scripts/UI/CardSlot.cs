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
            if (sourceSlot.slotType == SlotType.Shop)
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
            if (sourceSlot.slotType == SlotType.Shop)
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
                    StartCoroutine(CheckMergeNextFrame(unitUI.currentInstance.Data.cardID, unitUI.currentInstance.mergeLevel));
                }
            }
        }
        // --- DI CHUYỂN BÀI ---
        else if (this.slotType == SlotType.Hand || this.slotType == SlotType.PlayerBoard)
        {
            unitCard.parentReturnTo = this.transform;
            StartCoroutine(CheckMergeNextFrame(unitUI.currentInstance.Data.cardID, unitUI.currentInstance.mergeLevel));
        }
    }

    // ==========================================
    // HỆ THỐNG MERGE
    // ==========================================

    private IEnumerator CheckMergeNextFrame(string cardID, int mergeLevel)
    {
        yield return null; // Chờ OnEndDrag chạy xong để bài vào đúng slot
        CheckForMerge(cardID, mergeLevel);
    }

    private void CheckForMerge(string cardID, int mergeLevel)
    {
        List<CardUI> matches = new List<CardUI>();

        foreach (var slot in GameManager.Instance.playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui != null && ui.currentInstance.Data.cardID == cardID && ui.currentInstance.mergeLevel == mergeLevel)
                matches.Add(ui);
        }
        foreach (var slot in GameManager.Instance.handSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui != null && ui.currentInstance.Data.cardID == cardID && ui.currentInstance.mergeLevel == mergeLevel)
                matches.Add(ui);
        }

        if (matches.Count >= 3)
            PerformMerge(matches);
    }
    private void PerformMerge(List<CardUI> cards)
    {
        CardUI keeper = cards[0];
        keeper.currentInstance.mergeLevel++;
        keeper.currentInstance.ResetStats();
        keeper.Setup(keeper.currentInstance);

        for (int i = 1; i < 3; i++)
            Destroy(cards[i].gameObject);

        Debug.Log($"<color=gold>[MERGE]</color> 3x {keeper.currentInstance.Data.cardName} hợp nhất thành cấp {keeper.currentInstance.mergeLevel + 1}!");

        // Kiểm tra tiếp nếu vừa tạo ra quân đủ bộ 3 ở cấp mới
        StartCoroutine(CheckMergeNextFrame(keeper.currentInstance.Data.cardID, keeper.currentInstance.mergeLevel));
    }
}