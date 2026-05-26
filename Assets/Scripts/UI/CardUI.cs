using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardUI : MonoBehaviour, IPointerClickHandler
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

    [Header("Description")]
    public TextMeshProUGUI descriptionText;

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

    private int _prevATK = -1;
    private int _prevHP  = -1;
    private Coroutine _atkPunchRoutine;
    private Coroutine _hpPunchRoutine;

    private float _lastClickTime = -999f;
    private const float DoubleClickThreshold = 0.3f;

    public void Setup(CardInstance instance)
    {
        if (instance == null) return;

        if (characterArt == null || atkText == null || hpText == null)
        {
            Debug.LogError($"[CardUI] Thiếu Visual Elements trên prefab: {gameObject.name}");
            return;
        }

        // Reset theo dõi khi đổi sang card instance khác
        if (currentInstance != instance) { _prevATK = -1; _prevHP = -1; }
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
        else Debug.LogWarning($"[CardUI] Không tìm thấy art: Sprites/Cards/{folder}/{artFile} — kiểm tra Texture Type = Sprite trong Unity.");

        // --- Stats ---
        if (isSpell)
        {
            int newCost = instance.Data.cost;
            atkText.text = newCost.ToString();
            hpText.text  = "";
            hpText.color = normalHealthColor;
            if (_prevATK >= 0 && newCost != _prevATK)
                TriggerStatChange(ref _atkPunchRoutine, atkText.transform, atkText, newCost > _prevATK);
            _prevATK = newCost;
        }
        else
        {
            int newATK = instance.currentATK;
            int newHP  = instance.currentHP;
            // Set text & colour TRƯỚC để coroutine đọc được settled colour ngay khi start
            atkText.text = newATK.ToString();
            hpText.text  = newHP.ToString();
            hpText.color = instance.IsDamaged ? damagedHealthColor : normalHealthColor;
            if (_prevATK >= 0 && newATK != _prevATK)
                TriggerStatChange(ref _atkPunchRoutine, atkText.transform, atkText, newATK > _prevATK);
            if (_prevHP  >= 0 && newHP  != _prevHP)
                TriggerStatChange(ref _hpPunchRoutine,  hpText.transform,  hpText,  newHP  > _prevHP);
            _prevATK = newATK;
            _prevHP  = newHP;
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

        // --- Star icons: chỉ hiện cho Unit, spell không có sao ---
        if (!isSpell)
            UpdateStarIcons(instance.mergeLevel);
        else if (starIcons != null)
            foreach (var s in starIcons) { if (s != null) s.gameObject.SetActive(false); }

        // --- Description ---
        if (descriptionText != null)
            descriptionText.text = instance.Data.description ?? "";
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

    private static readonly Color StatIncreaseColor = new Color(0.25f, 1f, 0.35f);
    private static readonly Color StatDecreaseColor = new Color(1f, 0.15f, 0.15f);

    private void TriggerStatChange(ref Coroutine slot, Transform target, TextMeshProUGUI text, bool increased)
    {
        if (!isActiveAndEnabled) return;
        if (slot != null) StopCoroutine(slot);
        slot = StartCoroutine(StatChangeRoutine(target, text, increased));
    }

    private IEnumerator StatChangeRoutine(Transform target, TextMeshProUGUI text, bool increased)
    {
        // text.color đã được Setup() set trước khi coroutine này chạy
        Color settledColor = text.color;
        Color flashColor   = increased ? StatIncreaseColor : StatDecreaseColor;

        if (increased)
        {
            Vector3 original = target.localScale;
            Vector3 big      = original * 1.45f;
            float   upTime   = 0.07f;
            float   downTime = 0.15f;
            float   elapsed  = 0f;

            text.color = flashColor;
            while (elapsed < upTime)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(original, big, elapsed / upTime);
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < downTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / downTime;
                target.localScale = Vector3.Lerp(big, original, t);
                text.color        = Color.Lerp(flashColor, settledColor, t);
                yield return null;
            }
            target.localScale = original;
        }
        else
        {
            float elapsed  = 0f;
            float duration = 0.28f;
            text.color = flashColor;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.color = Color.Lerp(flashColor, settledColor, elapsed / duration);
                yield return null;
            }
        }
        text.color = settledColor;
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)    { StopCoroutine(blinkRoutine);    blinkRoutine    = null; }
        if (_atkPunchRoutine != null) { StopCoroutine(_atkPunchRoutine); _atkPunchRoutine = null; }
        if (_hpPunchRoutine  != null) { StopCoroutine(_hpPunchRoutine);  _hpPunchRoutine  = null; }
        if (frameOriginalCached && frameBackground != null) frameBackground.color = frameOriginalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        if (now - _lastClickTime <= DoubleClickThreshold)
        {
            if (currentInstance != null && CardDetailPanel.Instance != null)
                CardDetailPanel.Instance.Show(currentInstance);
            _lastClickTime = -999f;
        }
        else
        {
            _lastClickTime = now;
        }
    }
}
