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
            ApplyMagicEffect(magicUI.currentInstance, targetUnitUI.currentInstance);

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
                }
            }
        }
        // --- DI CHUYỂN BÀI ---
        else if (this.slotType == SlotType.Hand || this.slotType == SlotType.PlayerBoard)
        {
            unitCard.parentReturnTo = this.transform;
        }
    }

    private void ApplyMagicEffect(CardInstance magic, CardInstance unit)
    {
        switch (magic.Data.magicGroup)
        {
            case 1: // Nhóm 1: Tăng chỉ số
                unit.currentATK += magic.Data.statBonusATK;
                unit.currentHP += magic.Data.statBonusHP;
                break;
            case 2: // Nhóm 2: Cấp Ability
                unit.Data.ability = magic.Data.ability;
                unit.Data.abilityValue = magic.Data.abilityValue;
                break;
            case 3: // Nhóm 3: Kinh tế (Ví dụ đơn giản)
                GameManager.Instance.playerCoins += 1;
                // Có thể thêm logic GiveRandomCardFromTribe ở đây
                break;
        }
    }
}