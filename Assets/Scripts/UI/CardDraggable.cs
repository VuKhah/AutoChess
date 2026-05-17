using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentReturnTo = null;
    private CanvasGroup canvasGroup;
    private Transform draggingLayer;
    private CardSlot sourceSlot;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Tìm lớp trên cùng để hiển thị khi kéo
        draggingLayer = GameObject.Find("--- DRAGGING_LAYER ---").transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.isCombatActive)
        {
            Debug.Log("Đang trong trận đấu, không thể di chuyển quân bài!");
            return;
        }
        
        parentReturnTo = this.transform.parent;
        sourceSlot = parentReturnTo.GetComponent<CardSlot>();

        // Nhấc lá bài lên lớp trên cùng để không bị che
        this.transform.SetParent(draggingLayer);

        // Tắt raycast để chuột có thể "nhìn thấy" Slot bên dưới lá bài
        canvasGroup.blocksRaycasts = false;

        Debug.Log("Bắt đầu kéo lá bài");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Lá bài bay theo con trỏ chuột
        this.transform.position = eventData.position;
        Debug.Log("Đang kéo bài...");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Trả về parent cũ (nếu không tìm thấy slot mới) hoặc slot mới (do script Slot xử lý)
        this.transform.SetParent(parentReturnTo);

        // Ép lá bài khít với slot mới sau khi thả.
        RectTransform rect = GetComponent<RectTransform>();
        CardSlotFitter.FitToSlot(rect, parentReturnTo);
        CardVisuals visuals = GetComponent<CardVisuals>();
        CardSlot destinationSlot = parentReturnTo.GetComponent<CardSlot>();
        if (visuals != null && destinationSlot != null)
        {
            visuals.RefreshSettledScale();
            bool deployedFromShop = sourceSlot != null
                && sourceSlot.slotType == CardSlot.SlotType.Shop
                && destinationSlot.slotType == CardSlot.SlotType.PlayerBoard;

            if (deployedFromShop)
                visuals.PlayDeployToBoard();
            else if (destinationSlot.slotType == CardSlot.SlotType.PlayerBoard)
                visuals.SetBoardPose();
            else
                visuals.SetUprightPose();
        }

        // Bật lại raycast để có thể kéo lần sau
        canvasGroup.blocksRaycasts = true;
    }
}
