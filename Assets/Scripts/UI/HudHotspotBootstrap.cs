using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIManager))]
public class HudHotspotBootstrap : MonoBehaviour
{
    private static readonly Rect RollRect = Rect.MinMaxRect(0.655f, 0.840f, 0.770f, 0.915f);
    private static readonly Rect FreezeRect = Rect.MinMaxRect(0.779f, 0.840f, 0.870f, 0.915f);
    private static readonly Rect FightRect = Rect.MinMaxRect(0.835f, 0.030f, 0.985f, 0.285f);

    private UIManager manager;

    private void Awake()
    {
        manager = GetComponent<UIManager>();

        RectTransform hotspotRoot = FindHotspotRoot();
        if (hotspotRoot == null)
        {
            Debug.LogWarning("[HUD] Không tìm thấy Main_Layout để tạo hotspot.");
            return;
        }

        manager.rollButton = GetOrCreateButton(hotspotRoot, "Roll_Hotspot", RollRect, manager.rollButton);
        manager.lockButton = GetOrCreateButton(hotspotRoot, "Freeze_Hotspot", FreezeRect, manager.lockButton);
        manager.actionButton = GetOrCreateButton(hotspotRoot, "Fight_Hotspot", FightRect, manager.actionButton);
    }

    private RectTransform FindHotspotRoot()
    {
        GameObject mainLayout = GameObject.Find("Main_Layout");
        return mainLayout != null ? mainLayout.GetComponent<RectTransform>() : null;
    }

    private static Button GetOrCreateButton(RectTransform parent, string objectName, Rect normalizedRect, Button existingButton)
    {
        if (existingButton != null)
            return existingButton;

        Transform existingChild = parent.Find(objectName);
        if (existingChild != null && existingChild.TryGetComponent(out Button existingHotspot))
            return existingHotspot;

        GameObject hotspot = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        hotspot.layer = LayerMask.NameToLayer("UI");
        hotspot.transform.SetParent(parent, false);
        hotspot.transform.SetAsLastSibling();

        RectTransform rectTransform = hotspot.GetComponent<RectTransform>();
        rectTransform.anchorMin = normalizedRect.min;
        rectTransform.anchorMax = normalizedRect.max;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = hotspot.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);
        image.raycastTarget = true;

        Button button = hotspot.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        return button;
    }
}
