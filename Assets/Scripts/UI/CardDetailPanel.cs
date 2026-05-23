using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDetailPanel : MonoBehaviour
{
    public static CardDetailPanel Instance;

    [Header("Structure")]
    public Transform cardHolder;
    public GameObject cardPrefab;

    [Header("Info Panel")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descriptionText;

    [Header("Styled Images (sprite assigned at runtime)")]
    public Image contentPanelImage;
    public Image descBoxImage;

    private GameObject cardClone;

    private void Awake()
    {
        Instance = this;
        if (contentPanelImage != null)
            contentPanelImage.sprite = CreateRoundedSprite(128, 128, 24);
        if (descBoxImage != null)
            descBoxImage.sprite = CreateRoundedSprite(96, 96, 16);
        gameObject.SetActive(false);
    }

    private Sprite CreateRoundedSprite(int w, int h, int r)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            int cx = Mathf.Clamp(x, r, w - r - 1);
            int cy = Mathf.Clamp(y, r, h - r - 1);
            float dx = x - cx, dy = y - cy;
            float a = Mathf.Clamp01(r + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
            pixels[y * w + x] = new Color(1f, 1f, 1f, a);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
    }

    public void Show(CardInstance instance)
    {
        if (instance == null) return;
        if (cardClone != null) Destroy(cardClone);

        if (cardNameText != null) cardNameText.text = instance.Data.cardName;

        if (statsText != null)
        {
            bool spell = instance.Data.cardType == CardType.Spell;
            if (spell)
                statsText.text = "Cost: " + instance.Data.cost + "G";
            else
                statsText.text = "ATK: " + instance.currentATK
                               + "     HP: " + instance.currentHP
                               + "     Tier " + instance.Data.tier;
        }

        if (descriptionText != null)
            descriptionText.text = instance.Data.description ?? "";

        if (cardPrefab != null && cardHolder != null)
        {
            cardClone = Instantiate(cardPrefab, cardHolder);
            var rt = cardClone.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(160f, 240f);
            }
            cardClone.transform.localScale = new Vector3(3f, 3f, 1f);

            var ui = cardClone.GetComponent<CardUI>();
            if (ui != null) ui.Setup(instance);

            var drag = cardClone.GetComponent<CardDraggable>();
            if (drag != null) drag.enabled = false;

            var cg = cardClone.GetComponent<CanvasGroup>();
            if (cg != null) { cg.blocksRaycasts = false; cg.interactable = false; }
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (cardClone != null) { Destroy(cardClone); cardClone = null; }
        gameObject.SetActive(false);
    }
}
