using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckPanelController : MonoBehaviour
{
    [Header("UI")]
    public GameObject deckPanel;

    [Header("Cards")]
    public Transform contentParent;
    public GameObject cardPrefab;

    private bool loaded = false;

    // Thứ tự nhóm Tribe khi hiển thị Kho bài — Spell (Tribe.None) luôn ở nhóm cuối
    private static readonly Tribe[] TribeGroupOrder = { Tribe.Niles, Tribe.Babylon, Tribe.Olympus, Tribe.None };

    public void OpenDeck()
    {
        Debug.Log("OpenDeck được gọi");

        deckPanel.SetActive(true);

        if (!loaded)
        {
            LoadCards();
            loaded = true;
        }
    }

    public void CloseDeck()
    {
        deckPanel.SetActive(false);
    }

    // Thông tin thanh chia nhóm Tribe — RebuildCards nhận diện qua kiểu này
    private readonly struct HeaderItem
    {
        public readonly string Label;
        public readonly Color Color;
        public HeaderItem(string label, Color color) { Label = label; Color = color; }
    }

    // Gom card theo nhóm Tribe (theo TribeGroupOrder), mỗi nhóm sắp tier giảm dần,
    // chèn HeaderItem trước mỗi nhóm để RebuildCards tạo thanh chia.
    private static List<object> BuildTribeGroupedItems(List<CardDefinition> source)
    {
        var result = new List<object>();
        foreach (Tribe t in TribeGroupOrder)
        {
            var group = source
                .Where(c => c.tribe == t)
                .OrderByDescending(c => c.tier)
                .ThenBy(c => c.cardName)
                .ToList();

            if (group.Count == 0) continue;

            result.Add(new HeaderItem(GetTribeLabel(t), GetTribeColor(t)));
            result.AddRange(group);
        }
        return result;
    }

    private static string GetTribeLabel(Tribe t)
    {
        switch (t)
        {
            case Tribe.Niles: return "NILES";
            case Tribe.Babylon: return "BABYLON";
            case Tribe.Olympus: return "OLYMPUS";
            default: return "SPELLS";
        }
    }

    private static Color GetTribeColor(Tribe t)
    {
        switch (t)
        {
            case Tribe.Niles: return new Color(0.85f, 0.70f, 0.25f, 0.95f); // vàng cát
            case Tribe.Babylon: return new Color(0.30f, 0.55f, 0.90f, 0.95f); // xanh dương
            case Tribe.Olympus: return new Color(0.65f, 0.40f, 0.90f, 0.95f); // tím
            default: return new Color(0.55f, 0.55f, 0.60f, 0.95f); // xám — Spells
        }
    }

    private void LoadCards()
    {
        Debug.Log("=== LoadCards bắt đầu ===");

        if (CardDatabase.Instance == null)
        {
            Debug.LogError("CardDatabase.Instance = NULL");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("contentParent = NULL");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("cardPrefab = NULL");
            return;
        }

        List<CardDefinition> allCards = CardDatabase.Instance.GetAllCards();

        Debug.Log("Số card tìm thấy: " + allCards.Count);

        RebuildCards(BuildTribeGroupedItems(allCards));

        Debug.Log("=== LoadCards hoàn tất ===");
    }

    // Tạo container GridLayoutGroup riêng cho các card của 1 nhóm Tribe — VerticalLayoutGroup
    // trên contentParent sẽ kéo container này full-width, nên grid bên trong tự tính số cột
    // theo chiều rộng thật, và mỗi nhóm luôn bắt đầu ở một hàng mới, không lệ thuộc thanh chia.
    private Transform CreateCardRowContainer(string label)
    {
        GameObject row = new GameObject("CardRow_" + label, typeof(RectTransform), typeof(GridLayoutGroup));
        row.transform.SetParent(contentParent, false);

        GridLayoutGroup g = row.GetComponent<GridLayoutGroup>();
        g.cellSize = new Vector2(220f, 250f);
        g.spacing = new Vector2(20f, 20f);
        g.constraint = GridLayoutGroup.Constraint.Flexible;
        g.startCorner = GridLayoutGroup.Corner.UpperLeft;
        g.startAxis = GridLayoutGroup.Axis.Horizontal;
        g.childAlignment = TextAnchor.UpperCenter;

        return row.transform;
    }

    // Xóa toàn bộ nội dung hiện có trong contentParent rồi tạo lại theo danh sách items.
    // Mỗi HeaderItem tạo 1 thanh chia full-width + 1 container card riêng cho nhóm Tribe đó;
    // các CardDefinition tiếp theo được thêm vào container của nhóm hiện tại.
    private void RebuildCards(List<object> items)
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        Transform currentRow = contentParent;

        foreach (object item in items)
        {
            if (item is HeaderItem header)
            {
                CreateGroupHeader(header.Label, header.Color);
                currentRow = CreateCardRowContainer(header.Label);
                continue;
            }

            CardDefinition cardData = (CardDefinition)item;
            GameObject obj = Instantiate(cardPrefab, currentRow);

            CardUI ui = obj.GetComponent<CardUI>();
            if (ui == null)
            {
                Debug.LogError("Prefab không có CardUI");
                continue;
            }

            CardInstance instance = new CardInstance(cardData, -1);
            ui.Setup(instance);

            // Card trong Kho bài bị GridLayoutGroup kéo rộng hơn kích thước gốc của prefab,
            // khiến Tribe_Icon (anchor giữa, offset cố định) lệch trái so với góc trên-phải.
            // Bù lại bằng cách đẩy icon qua phải 20pt và lên trên 4pt, chỉ cho card ở Kho bài.
            if (ui.tribeIcon != null)
            {
                RectTransform tribeRT = ui.tribeIcon.rectTransform;
                tribeRT.anchoredPosition += new Vector2(21f, 3f);
            }

            // ATK/HP text cũng bị lệch tương tự — đẩy lên 4pt, ATK sang phải 8pt, HP sang trái 8pt.
            if (ui.atkText != null)
                ui.atkText.rectTransform.anchoredPosition += new Vector2(9f, 4f);
            if (ui.hpText != null)
                ui.hpText.rectTransform.anchoredPosition += new Vector2(-8f, 4f);

            CardDraggable drag = obj.GetComponent<CardDraggable>();
            if (drag != null)
                drag.enabled = false;
        }

        ScrollRect scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    // Thanh chia nhóm Tribe — 1 phần tử riêng trong VerticalLayoutGroup của contentParent,
    // được kéo full-width tự động, cao cố định 40px.
    private void CreateGroupHeader(string label, Color barColor)
    {
        GameObject bar = new GameObject("GroupHeader_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        bar.transform.SetParent(contentParent, false);
        bar.GetComponent<Image>().color = barColor;

        LayoutElement le = bar.GetComponent<LayoutElement>();
        le.minHeight = 40f;
        le.preferredHeight = 40f;

        GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(bar.transform, false);
        RectTransform lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.pivot = new Vector2(0.5f, 0.5f);
        lrt.anchoredPosition = Vector2.zero;
        lrt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = labelGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
    }
}
