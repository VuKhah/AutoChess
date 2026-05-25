using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Panel mờ hiện ra khi đang kéo card từ Hand hoặc PlayerBoard.
// Thả card vào panel sẽ bán card với giá 1 vàng cộng vào số tiền hiện tại trong round.
// Tự build UI runtime nếu chưa có trong scene → không cần setup thủ công.
public class SellZone : MonoBehaviour, IDropHandler
{
    private static SellZone instance;
    private CanvasGroup canvasGroup;

    public static void Show()
    {
        EnsureExists();
        if (instance != null) instance.SetVisible(true);
    }

    public static void Hide()
    {
        if (instance != null) instance.SetVisible(false);
    }

    public static SellZone EnsureExists()
    {
        if (instance != null) return instance;

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) return null;

        GameObject panel = new GameObject(
            "SellZonePanel",
            typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(CanvasGroup), typeof(SellZone));
        panel.transform.SetParent(canvas.transform, false);
        panel.transform.SetAsLastSibling();

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.30f, 0.78f);
        rt.anchorMax = new Vector2(0.70f, 0.95f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.GetComponent<Image>();
        img.color = new Color(0.85f, 0.15f, 0.15f, 0.35f);

        GameObject textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(panel.transform, false);
        RectTransform trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        TextMeshProUGUI t = textGo.AddComponent<TextMeshProUGUI>();
        t.text = "BÁN  +1G";
        t.fontSize = 48;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(1f, 1f, 1f, 0.95f);

        instance = panel.GetComponent<SellZone>();
        instance.canvasGroup = panel.GetComponent<CanvasGroup>();
        instance.SetVisible(false);
        return instance;
    }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (instance == null) instance = this;
    }

    private void SetVisible(bool v)
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) return;
        canvasGroup.alpha = v ? 1f : 0f;
        canvasGroup.blocksRaycasts = v;
        canvasGroup.interactable   = v;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) { Hide(); return; }

        CardDraggable draggedCard = eventData.pointerDrag.GetComponent<CardDraggable>();
        if (draggedCard == null) { Hide(); return; }

        CardUI ui = draggedCard.GetComponent<CardUI>();
        if (ui == null || ui.currentInstance == null) { Hide(); return; }

        CardSlot sourceSlot = draggedCard.parentReturnTo != null
            ? draggedCard.parentReturnTo.GetComponent<CardSlot>()
            : null;
        if (sourceSlot == null) { Hide(); return; }

        // Chỉ cho bán từ Hand hoặc PlayerBoard
        if (sourceSlot.slotType != CardSlot.SlotType.Hand
            && sourceSlot.slotType != CardSlot.SlotType.PlayerBoard)
        {
            Hide();
            return;
        }

        // Bắn OnSell + broadcast OnAllySell cho Unit
        if (ui.currentInstance.Data.cardType == CardType.Unit)
        {
            GameManager.Instance.SyncBoards();
            GameManager.Instance.resolver.TriggerAbility(
                TriggerType.OnSell,
                ui.currentInstance,
                null,
                GameManager.Instance.playerBoard,
                GameManager.Instance.enemyBoard
            );
            GameManager.Instance.resolver.BroadcastAllyEvent(
                TriggerType.OnAllySell,
                ui.currentInstance,
                GameManager.Instance.playerBoard,
                GameManager.Instance.enemyBoard
            );
        }

        GameManager.Instance.SellCard();             // +1 vàng + update UI
        Destroy(draggedCard.gameObject);
        Hide();
    }
}
