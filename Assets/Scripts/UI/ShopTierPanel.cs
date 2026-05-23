using UnityEngine;
using TMPro;

public class ShopTierPanel : MonoBehaviour
{
    public static ShopTierPanel Instance;

    public TextMeshProUGUI shopTierText;
    public TextMeshProUGUI[] dropRateTexts = new TextMeshProUGUI[6];

    private void Awake() { Instance = this; }

    private void Start() { Refresh(); }

    public void Refresh()
    {
        if (GameManager.Instance == null || CardDatabase.Instance == null) return;

        int tier = GameManager.Instance.GetCurrentShopTier();
        if (shopTierText != null)
            shopTierText.text = tier.ToString();

        int[] rates = CardDatabase.Instance.GetDropRates(tier);
        for (int i = 0; i < dropRateTexts.Length && i < rates.Length; i++)
            if (dropRateTexts[i] != null)
                dropRateTexts[i].text = rates[i] + "%";
    }
}
