using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    public void RollShop()
    {
        if (economy.TrySpend(rollCost))
        {
            isShopFrozen = false;
            RefreshShop();
            UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
            AudioManager.Instance?.Roll();
        }
    }

    public void ToggleLock()
    {
        isShopFrozen = !isShopFrozen;
        AudioManager.Instance?.Freeze();
    }

    // Công thức: Turn 1-2: Tier 1 | Turn 3-4: Tier 2 | Turn 5-6: Tier 3... Tối đa Tier 6.
    public int GetCurrentShopTier()
    {
        int calculatedTier = (currentTurn + 1) / 2;
        return Mathf.Clamp(calculatedTier, 1, 6);
    }

    public void RefreshShop()
    {
        if (isShopFrozen) return;
        foreach (var slot in shopSlots)
            foreach (Transform child in slot) Destroy(child.gameObject);

        int currentTier = GetCurrentShopTier();
        List<CardDefinition> shopData = CardDatabase.Instance.GetRandomShop(shopSlots.Length, currentTier);
        for (int i = 0; i < shopData.Count; i++)
        {
            if (i < shopSlots.Length) CreateCardInSlot(shopData[i], shopSlots[i]);
        }
        StartCoroutine(UpdateShopMergeHintsNextFrame());
    }

    private IEnumerator UpdateShopMergeHintsNextFrame()
    {
        yield return null;
        UpdateShopMergeHints();
    }

    // Quét shop: nếu player đã có >=2 lá cùng cardID + cùng mergeLevel (board+hand, không phải battleSpawned)
    // thì card đó trong shop sẽ nhấp nháy → mua thêm 1 lá nữa sẽ merge lên sao.
    public void UpdateShopMergeHints()
    {
        if (shopSlots == null) return;
        foreach (var slot in shopSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui == null || ui.currentInstance == null) continue;

            // Spell không merge — đảm bảo tắt blink
            if (ui.currentInstance.Data.cardType != CardType.Unit)
            {
                ui.SetMergeHint(false);
                continue;
            }

            string cardID = ui.currentInstance.Data.cardID;
            int mergeLevel = ui.currentInstance.mergeLevel;
            int matches = CountOwnedMatches(cardID, mergeLevel);
            ui.SetMergeHint(matches >= 2);
        }
    }

    private int CountOwnedMatches(string cardID, int mergeLevel)
    {
        int count = 0;
        foreach (var slot in playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui != null && ui.currentInstance != null
                && ui.currentInstance.Data.cardID == cardID
                && ui.currentInstance.mergeLevel == mergeLevel
                && !ui.currentInstance.isBattleSpawned)
                count++;
        }
        foreach (var slot in handSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui != null && ui.currentInstance != null
                && ui.currentInstance.Data.cardID == cardID
                && ui.currentInstance.mergeLevel == mergeLevel
                && !ui.currentInstance.isBattleSpawned)
                count++;
        }
        return count;
    }

    private void CreateCardInSlot(CardDefinition data, Transform slot)
    {
        GameObject cardObj = Instantiate(cardPrefab, slot);
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        CardSlotFitter.FitToSlot(cardRect, slot);
        StartCoroutine(FitCardAfterLayout(cardRect, slot));
        cardObj.GetComponent<CardUI>().Setup(new CardInstance(data, 0));
        cardObj.GetComponent<CardVisuals>()?.SetUprightPose();
    }

    private IEnumerator FitCardAfterLayout(RectTransform cardRect, Transform slot)
    {
        yield return null;
        CardSlotFitter.FitToSlot(cardRect, slot);
        cardRect.GetComponent<CardVisuals>()?.RefreshSettledScale();
    }

    public bool TryBuyCard(int cost)
    {
        if (!economy.TrySpend(cost)) return false;
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        AudioManager.Instance?.Buy();
        return true;
    }

    public void SellCard()
    {
        economy.Sell();
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
    }
}
