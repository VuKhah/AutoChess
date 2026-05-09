using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
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

    [HideInInspector] public CardInstance currentInstance;

    [Header("Visual Feedback")]
    public Color normalHealthColor  = Color.white;
    public Color damagedHealthColor = Color.red;

    public void Setup(CardInstance instance)
    {
        if (instance == null) return;

        if (characterArt == null || atkText == null || hpText == null)
        {
            Debug.LogError($"[CardUI] Thiếu Visual Elements trên prefab: {gameObject.name}");
            return;
        }

        currentInstance = instance;

        // --- Art ---
        string folder  = instance.Data.cardType == CardType.Magic ? "Magic" : "Units";
        Sprite art = Resources.Load<Sprite>($"Sprites/Cards/{folder}/{instance.Data.cardID}");
        if (art != null) characterArt.sprite = art;

        // --- Stats ---
        atkText.text  = instance.currentATK.ToString();
        hpText.text   = instance.currentHP.ToString();
        hpText.color  = instance.IsDamaged ? damagedHealthColor : normalHealthColor;

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
        SetPassiveIcon(tauntIcon,     instance.isTaunt,        "Taunt");
        SetPassiveIcon(rebornIcon,    instance.isReborn,        "Reborn");
        SetPassiveIcon(safeguardIcon, instance.safeguardActive, "Safeguard");

        // --- Tier icon ---
        if (tierIcon != null)
        {
            Sprite tSprite = Resources.Load<Sprite>("Sprites/Icons/Tiers/Tier_" + instance.Data.tier);
            tierIcon.sprite = tSprite;
            tierIcon.gameObject.SetActive(tSprite != null);
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
}
