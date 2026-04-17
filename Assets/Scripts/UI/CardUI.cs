using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Visual Elements")]
    public Image characterArt;
    public Image abilityIcon;
    public Image tierIcon;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI hpText;

    [HideInInspector]
    public CardInstance currentInstance;

    [Header("Visual Feedback")]
    public Color normalHealthColor = Color.white;
    public Color damagedHealthColor = Color.red;

    public void Setup(CardInstance instance)
    {
        // 0. Kiểm tra dữ liệu đầu vào
        if (instance == null) return;

        // --- BÀI KIỂM TRA AN TOÀN TRƯỚC KHI CHẠY ---
        if (characterArt == null || atkText == null || hpText == null)
        {
            Debug.LogError($"<color=red>[LỖI]</color> Thiếu thành phần UI (Image/Text) trên Prefab: {gameObject.name}. Hãy kéo thả lại trong Inspector!");
            return;
        }

        currentInstance = instance; // Lưu lại instance để tham chiếu sau này (ví dụ: khi bị tấn công, kích hoạt kỹ năng...)

        string folder = (instance.Data.cardType == CardType.Magic) ? "Magic" : "Units";

        // Đường dẫn bây giờ cực kỳ gọn: Sprites/Cards/Units/U_01
        // Tribe giờ chỉ dùng để tính toán cộng điểm, không dùng để tìm ảnh nữa
        string artPath = $"Sprites/Cards/{folder}/{instance.Data.cardID}";

        Sprite art = Resources.Load<Sprite>(artPath);
        if (art != null) characterArt.sprite = art;

        // 2. Cập nhật chỉ số ATK và HP
        atkText.text = instance.currentATK.ToString();
        hpText.text = instance.currentHP.ToString();

        // Đổi màu chữ HP nếu bị mất máu (Phản hồi trực quan)
        hpText.color = instance.IsDamaged ? damagedHealthColor : normalHealthColor;

        // 3. Nạp Hình 1: Icon Đặc tính (Ability)
        if (abilityIcon != null)
        {
            // Ép kiểu Enum sang Int để lấy số (Ví dụ: Enrage -> 1)
            int abilityID = (int)instance.Data.ability;

            if (abilityID == 0) // 0 là None
            {
                abilityIcon.gameObject.SetActive(false);
            }
            else
            {
                // Nạp ảnh theo định dạng: Icons/Abilities/Abi_1.png, Icons/Abilities/Abi_2.png...
                string iconName = "Abi_" + abilityID;
                Sprite s = Resources.Load<Sprite>("Sprites/Icons/Abilities/" + iconName);

                if (s != null)
                {
                    abilityIcon.sprite = s;
                    abilityIcon.gameObject.SetActive(true);
                }
                else
                {
                    abilityIcon.gameObject.SetActive(false);
                    Debug.LogWarning($"Thiếu ảnh: Resources/Sprites/Icons/Abilities/{iconName}.png");
                }
            }
        }

        // 4. Nạp Hình 2: Icon Tier (Cấp độ bài)
        if (tierIcon != null)
        {
            // Quy tắc đặt tên ảnh trong Resources/Icons: Tier_1, Tier_2, Tier_3...
            string tierPath = "Sprites/Icons/Tiers/Tier_" + instance.Data.tier;
            Sprite tSprite = Resources.Load<Sprite>(tierPath);

            if (tSprite != null)
            {
                tierIcon.sprite = tSprite;
                tierIcon.gameObject.SetActive(true);
            }
            else
            {
                tierIcon.gameObject.SetActive(false);
                // Nếu chưa có ảnh Tier, chỉ log lỗi nếu Tier > 0
                if (instance.Data.tier > 0) Debug.LogWarning($"[UI] Không tìm thấy Icon Tier tại: {tierPath}");
            }
        }
    }
}