using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Trigger zone: vùng kéo bài vào để kích hoạt / thả để bán — ẩn, chỉ nhận raycast.
// Notification panel: vùng hiển thị "BÁN +1G" — tách biệt, không nhận raycast.
public class SellZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private static SellZone instance;
    private CanvasGroup triggerCG;
    private CanvasGroup notificationCG;

    // Gọi khi bắt đầu kéo từ Hand/PlayerBoard: trigger zone sẵn sàng nhận hover
    public static void Standby()
    {
        EnsureExists();
        if (instance != null) instance.SetTriggerActive(true);
    }

    public static void Hide()
    {
        if (instance != null) instance.SetHidden();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetNotificationVisible(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNotificationVisible(false);
    }

    public static SellZone EnsureExists()
    {
        if (instance != null) return instance;

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) return null;

        // --- Trigger zone (vùng kéo bài vào, ẩn hoàn toàn) ---
        GameObject trigger = new GameObject(
            "SellZoneTrigger",
            typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(CanvasGroup), typeof(SellZone));
        trigger.transform.SetParent(canvas.transform, false);
        trigger.transform.SetAsLastSibling();

        RectTransform triggerRT = trigger.GetComponent<RectTransform>();
        triggerRT.anchorMin = new Vector2(0.30f, 0.78f);
        triggerRT.anchorMax = new Vector2(0.70f, 0.95f);
        triggerRT.offsetMin = new Vector2(-270f, -280f);
        triggerRT.offsetMax = new Vector2(452f, -128f);

        // Image trong suốt để nhận raycast mà không hiện ra
        trigger.GetComponent<Image>().color = Color.clear;

        // --- Notification panel (vùng hiển thị "BÁN +1G") ---
        GameObject notif = new GameObject(
            "SellZoneNotification",
            typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(CanvasGroup));
        notif.transform.SetParent(canvas.transform, false);
        notif.transform.SetAsLastSibling();

        RectTransform notifRT = notif.GetComponent<RectTransform>();
        notifRT.anchorMin = new Vector2(0.30f, 0.78f);
        notifRT.anchorMax = new Vector2(0.70f, 0.95f);
        notifRT.offsetMin = Vector2.zero;
        notifRT.offsetMax = Vector2.zero;

        notif.GetComponent<Image>().color = new Color(0.85f, 0.15f, 0.15f, 0.35f);

        GameObject textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(notif.transform, false);
        RectTransform trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

        TextMeshProUGUI t = textGo.AddComponent<TextMeshProUGUI>();
        t.text = "BÁN  +1G";
        t.fontSize = 48;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(1f, 1f, 1f, 0.95f);

        instance = trigger.GetComponent<SellZone>();
        instance.triggerCG = trigger.GetComponent<CanvasGroup>();
        instance.notificationCG = notif.GetComponent<CanvasGroup>();
        instance.SetHidden();
        return instance;
    }

    private void Awake()
    {
        if (triggerCG == null) triggerCG = GetComponent<CanvasGroup>();
        if (instance == null) instance = this;
    }

    private void Update()
    {
        // Fallback: nếu chuột nhả ngoài game window (WebGL), tự tắt
        if (triggerCG != null && triggerCG.blocksRaycasts && !Input.GetMouseButton(0))
            SetHidden();
    }

    private void SetTriggerActive(bool active)
    {
        if (triggerCG == null) return;
        triggerCG.blocksRaycasts = active;
        triggerCG.interactable   = active;
        if (!active) SetNotificationVisible(false);
    }

    private void SetNotificationVisible(bool visible)
    {
        if (notificationCG == null) return;
        notificationCG.alpha          = visible ? 1f : 0f;
        notificationCG.blocksRaycasts = false;
        notificationCG.interactable   = false;
    }

    private void SetHidden()
    {
        SetTriggerActive(false);
        SetNotificationVisible(false);
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

        if (sourceSlot.slotType != CardSlot.SlotType.Hand
            && sourceSlot.slotType != CardSlot.SlotType.PlayerBoard)
        {
            Hide();
            return;
        }

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

        GameManager.Instance.SellCard();
        Destroy(draggedCard.gameObject);
        Hide();
    }
}
