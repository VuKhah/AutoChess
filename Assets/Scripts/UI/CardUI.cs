using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Frame")]
    public Image frameBackground;   // Khung nền của card — gắn sprite trực tiếp ở prefab

    [Header("Name")]
    public TextMeshProUGUI nameText;     // Tên thẻ — lấy từ CardDefinition.cardName

    [Header("Stats")]
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI hpText;
    public Image characterArt;

    [Header("Ability Icon (TTE)")]
    public Image abilityIcon;       // Sprite: Sprites/Icons/Abilities/Abi_{effectID}

    [Header("Passive Keyword Icons")]
    public Image tauntIcon;         // Sprite: Sprites/Icons/Passives/Taunt   — khiên lớn
    public Image rebornIcon;        // Sprite: Sprites/Icons/Passives/Reborn  — bình thuốc
    public Image safeguardIcon;     // Sprite: Sprites/Icons/Passives/Safeguard — vòng bảo vệ

    [Header("Tier Icon")]
    public Image tierIcon;          // Sprite: Sprites/Icons/Tiers/Tier_{n}

    [Header("Star Icons (Merge Level)")]
    // Gắn 3 Image vào đây theo thứ tự: [0]=sao 1, [1]=sao 2, [2]=sao 3.
    // mergeLevel=0 → chỉ hiện sao 1; mergeLevel=1 → sao 1+2; mergeLevel=2 → cả 3 sao.
    public Image[] starIcons = new Image[3];

    [Header("Merge Hint Blink")]
    public Color blinkColor = new Color(1f, 0.9f, 0.3f, 1f);
    public float blinkSpeed = 4f;   // Số lần nhấp nháy mỗi giây (Hz)

    [HideInInspector] public CardInstance currentInstance;

    [Header("Visual Feedback")]
    public Color normalHealthColor = Color.white;
    public Color damagedHealthColor = Color.red;

    private Coroutine blinkRoutine;
    private Color frameOriginalColor;
    private bool frameOriginalCached;

    public void Setup(CardInstance instance)
    {
        if (instance == null) return;

        if (characterArt == null || atkText == null || hpText == null)
        {
            Debug.LogError($"[CardUI] Thiếu Visual Elements trên prefab: {gameObject.name}");
            return;
        }

        currentInstance = instance;

        // --- Name ---
        if (nameText != null) nameText.text = instance.Data.cardName;

        // --- Art ---
        // Spell dùng fileName (spell-art_XX); Unit dùng cardID (U_01_Babylon)
        bool isSpell = instance.Data.cardType == CardType.Spell;
        string folder = isSpell ? "Spells" : "Units";
        string artFile = isSpell && !string.IsNullOrEmpty(instance.Data.fileName)
                         ? instance.Data.fileName
                         : instance.Data.cardID;
        Sprite art = Resources.Load<Sprite>($"Sprites/Cards/{folder}/{artFile}");
        if (art != null) characterArt.sprite = art;

        // --- Stats ---
        if (isSpell)
        {
            // Spell không có ATK/HP — hiện cost thay vào ô ATK, ẩn HP
            atkText.text = instance.Data.cost.ToString();
            hpText.text = "";
            hpText.color = normalHealthColor;
        }
        else
        {
            atkText.text = instance.currentATK.ToString();
            hpText.text = instance.currentHP.ToString();
            hpText.color = instance.IsDamaged ? damagedHealthColor : normalHealthColor;
        }

        // --- TTE Ability icon (chỉ effect của TTE, không lẫn passive) ---
        if (abilityIcon != null)
        {
            AbilityData firstActive = instance.Data.abilities?.Find(a => a != null && a.trigger != TriggerType.None);
            if (firstActive != null)
            {
                string iconName = "Abi_" + (int)firstActive.effect;
                Sprite s = Resources.Load<Sprite>("Sprites/Icons/Abilities/" + iconName);
                abilityIcon.sprite = s;
                abilityIcon.gameObject.SetActive(s != null);
                if (s == null) Debug.LogWarning($"[CardUI] Không tìm thấy: Sprites/Icons/Abilities/{iconName}");
            }
            else
            {
                abilityIcon.gameObject.SetActive(false);
            }
        }

        // --- Passive keyword icons (độc lập, không ghi đè nhau) ---
        SetPassiveIcon(tauntIcon, instance.isTaunt, "Taunt");
        SetPassiveIcon(rebornIcon, instance.isReborn, "Reborn");
        SetPassiveIcon(safeguardIcon, instance.safeguardActive, "Safeguard");

        // --- Tier icon ---
        if (tierIcon != null)
        {
            Sprite tSprite = Resources.Load<Sprite>("Sprites/Icons/Tiers/Tier_" + instance.Data.tier);
            tierIcon.sprite = tSprite;
            tierIcon.gameObject.SetActive(tSprite != null);
        }

        // --- Star icons theo mergeLevel (0/1/2 → 1/2/3 sao) ---
        UpdateStarIcons(instance.mergeLevel);
    }

    private void UpdateStarIcons(int mergeLevel)
    {
        if (starIcons == null) return;
        int starsToShow = Mathf.Clamp(mergeLevel + 1, 1, starIcons.Length);
        for (int i = 0; i < starIcons.Length; i++)
        {
            if (starIcons[i] == null) continue;
            starIcons[i].gameObject.SetActive(i < starsToShow);
        }
    }

    private void SetPassiveIcon(Image icon, bool active, string spriteName)
    {
        if (icon == null) return;
        if (active)
        {
            Sprite s = Resources.Load<Sprite>("Sprites/Icons/Passives/" + spriteName);
            if (s != null) icon.sprite = s;
        }
        icon.gameObject.SetActive(active);
    }

    // ==========================================
    // MERGE HINT BLINK — shop card nhấp nháy khi có thể lên sao
    // ==========================================
    public void SetMergeHint(bool on)
    {
        if (frameBackground == null) return;

        if (!frameOriginalCached)
        {
            frameOriginalColor = frameBackground.color;
            frameOriginalCached = true;
        }

        if (on)
        {
            if (blinkRoutine == null && isActiveAndEnabled)
                blinkRoutine = StartCoroutine(BlinkLoop());
        }
        else
        {
            if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
            frameBackground.color = frameOriginalColor;
        }
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
            frameBackground.color = Color.Lerp(frameOriginalColor, blinkColor, t);
            yield return null;
        }
    }

    private void OnDisable()
    {
        if (blinkRoutine != null) { StopCoroutine(blinkRoutine); blinkRoutine = null; }
        if (frameOriginalCached && frameBackground != null) frameBackground.color = frameOriginalColor;
    }
}
