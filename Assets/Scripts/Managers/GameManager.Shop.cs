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
        int unitCount  = Mathf.Min(shopUnitCount, shopSlots.Length);
        int spellCount = shopSlots.Length - unitCount;

        List<CardDefinition> unitData  = CardDatabase.Instance.GetRandomUnitShop(unitCount, currentTier);
        List<CardDefinition> spellData = CardDatabase.Instance.GetRandomSpellShop(spellCount, currentTier);

        // Slot 0..(unitCount-1) → unit
        for (int i = 0; i < unitData.Count && i < unitCount; i++)
            CreateCardInSlot(unitData[i], shopSlots[i]);

        // Slot unitCount..(end) → spell
        for (int i = 0; i < spellData.Count; i++)
        {
            int slotIndex = unitCount + i;
            if (slotIndex < shopSlots.Length)
                CreateCardInSlot(spellData[i], shopSlots[slotIndex]);
        }

        StartCoroutine(UpdateShopMergeHintsNextFrame());
    }

    private IEnumerator UpdateShopMergeHintsNextFrame()
    {
        yield return null;
        UpdateShopMergeHints();
    }

    // Quét shop: nếu player đã có đủ (MergeRequiredCount-1) lá cùng cardID + mergeLevel (board+hand, không phải battleSpawned)
    // thì card đó trong shop sẽ nhấp nháy → mua thêm 1 lá nữa là đủ bộ để merge.
    public void UpdateShopMergeHints()
    {
        if (shopSlots == null) return;
        foreach (var slot in shopSlots)
        {
            if (slot == null) continue;
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui == null || ui.currentInstance == null) continue;

            // Spell không merge — đảm bảo tắt blink
            if (ui.currentInstance.Data == null || ui.currentInstance.Data.cardType != CardType.Unit)
            {
                ui.SetMergeHint(false);
                continue;
            }

            string cardID = ui.currentInstance.Data.cardID;
            int mergeLevel = ui.currentInstance.mergeLevel;
            int matches = CountOwnedMatches(cardID, mergeLevel);
            // Blink nếu mua thêm 1 lá này là đủ bộ để merge
            ui.SetMergeHint(matches >= CardInstance.MergeRequiredCount(mergeLevel) - 1);
        }
    }

    private int CountOwnedMatches(string cardID, int mergeLevel)
    {
        int count = 0;
        if (playerSlots != null)
        {
            foreach (var slot in playerSlots)
            {
                if (slot == null) continue;
                CardUI ui = slot.GetComponentInChildren<CardUI>();
                if (ui != null && ui.currentInstance != null
                    && ui.currentInstance.Data != null
                    && ui.currentInstance.Data.cardID == cardID
                    && ui.currentInstance.mergeLevel == mergeLevel
                    && !ui.currentInstance.isBattleSpawned)
                    count++;
            }
        }
        if (handSlots != null)
        {
            foreach (var slot in handSlots)
            {
                if (slot == null) continue;
                CardUI ui = slot.GetComponentInChildren<CardUI>();
                if (ui != null && ui.currentInstance != null
                    && ui.currentInstance.Data != null
                    && ui.currentInstance.Data.cardID == cardID
                    && ui.currentInstance.mergeLevel == mergeLevel
                    && !ui.currentInstance.isBattleSpawned)
                    count++;
            }
        }
        return count;
    }

    private void CreateCardInSlot(CardDefinition data, Transform slot)
    {
        GameObject cardObj = Instantiate(cardPrefab, slot);
        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        CardSlotFitter.FitToSlot(cardRect, slot);
        StartCoroutine(FitCardAfterLayout(cardRect, slot));
        CardInstance instance = new CardInstance(data, 0);
        ApplyGlobalPermBuffToNewUnit(instance); // áp global tribe buff tích lũy nếu có
        cardObj.GetComponent<CardUI>().Setup(instance);
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

    // ==========================================
    // SPELL EFFECT HELPERS
    // ==========================================

    public void AddUnitToHand(CardDefinition data)
    {
        foreach (var slot in handSlots)
        {
            if (slot.childCount == 0)
            {
                CreateCardInSlot(data, slot);
                UpdateShopMergeHints();
                Debug.Log($"<color=green>[SPELL]</color> Đã thêm {data.cardName} vào Hand.");
                return;
            }
        }
        Debug.LogWarning("[SPELL] Không còn ô Hand trống để nhận unit.");
    }

    public void StealRandomUnitFromShop()
    {
        var available = new System.Collections.Generic.List<Transform>();
        int unitSlotCount = Mathf.Min(shopUnitCount, shopSlots.Length);
        for (int i = 0; i < unitSlotCount; i++)
            if (shopSlots[i].childCount > 0) available.Add(shopSlots[i]);

        if (available.Count == 0)
        {
            Debug.LogWarning("[SPELL] Không có unit nào trong Shop để lấy.");
            return;
        }
        Transform chosen = available[Random.Range(0, available.Count)];
        CardUI ui = chosen.GetComponentInChildren<CardUI>();
        if (ui?.currentInstance == null) return;
        CardDefinition data = ui.currentInstance.Data;
        Destroy(ui.gameObject);
        AddUnitToHand(data);
        Debug.Log($"<color=cyan>[SPELL]</color> Sanctum Heist: lấy {data.cardName} từ Shop.");
    }

    public void LoseLife(int amount)
    {
        playerHP -= amount;
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        Debug.Log($"<color=red>[SPELL]</color> Mất {amount} Mạng! HP còn: {playerHP}");
        if (playerHP <= 0) GameOver();
    }

    public void RegisterWagerReward(int coins)
    {
        pendingWagerCoins += coins;
        Debug.Log($"<color=yellow>[SPELL]</color> Wager: thắng trận kế tiếp → nhận {coins} đồng.");
    }

    public void AddPermanentIncome(int amount)
    {
        economy.AddPermanentIncome(amount);
        UIManager.Instance.UpdateStats(playerHP, playerCups, playerCoins);
        Debug.Log($"<color=yellow>[SPELL]</color> Thu nhập vĩnh viễn tăng +{amount}.");
    }

    public void TransferStatsToRandom(CardInstance fromUnit)
    {
        if (fromUnit == null) return;
        int gainATK = fromUnit.currentATK;
        int gainHP  = fromUnit.maxHP;

        var candidates = new System.Collections.Generic.List<CardInstance>();
        foreach (var slot in playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance != null && ui.currentInstance != fromUnit && !ui.currentInstance.isBattleSpawned)
                candidates.Add(ui.currentInstance);
        }
        foreach (var slot in handSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance != null && ui.currentInstance != fromUnit && !ui.currentInstance.isBattleSpawned)
                candidates.Add(ui.currentInstance);
        }

        if (candidates.Count > 0)
        {
            CardInstance receiver = candidates[Random.Range(0, candidates.Count)];
            receiver.permanentATKBonus += gainATK;
            receiver.permanentHPBonus  += gainHP;
            receiver.maxHP             += gainHP;
            receiver.ResetStats();
            RefreshCardUI(receiver);
            Debug.Log($"<color=cyan>[SPELL]</color> {fromUnit.Data.cardName} hy sinh → {receiver.Data.cardName} nhận +{gainATK}ATK / +{gainHP}HP.");
        }
        DestroyUnit(fromUnit);
    }

    public void UpgradeSameTribeUnit(CardInstance reference)
    {
        if (reference == null) return;
        var candidates = new System.Collections.Generic.List<CardUI>();
        foreach (var slot in playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance != null && ui.currentInstance != reference
                && ui.currentInstance.Data.tribe == reference.Data.tribe
                && !ui.currentInstance.isBattleSpawned)
                candidates.Add(ui);
        }
        foreach (var slot in handSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance != null && ui.currentInstance != reference
                && ui.currentInstance.Data.tribe == reference.Data.tribe
                && !ui.currentInstance.isBattleSpawned)
                candidates.Add(ui);
        }
        if (candidates.Count == 0)
        {
            Debug.LogWarning("[SPELL] Không tìm thấy unit cùng Tộc để nâng cấp.");
            return;
        }
        CardUI target = candidates[Random.Range(0, candidates.Count)];
        target.currentInstance.mergeLevel = Mathf.Min(target.currentInstance.mergeLevel + 1, 2);
        target.currentInstance.hasDeployed = false;
        target.currentInstance.ResetStats();
        target.Setup(target.currentInstance);
        Debug.Log($"<color=cyan>[SPELL]</color> Đã nâng cấp {target.currentInstance.Data.cardName} lên sao {target.currentInstance.mergeLevel + 1}!");
    }

    private void DestroyUnit(CardInstance unit)
    {
        if (unit == null) return;
        foreach (var slot in playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance == unit) { Destroy(ui.gameObject); return; }
        }
        foreach (var slot in handSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance == unit) { Destroy(ui.gameObject); return; }
        }
    }

    public void RefreshCardUI(CardInstance unit)
    {
        if (unit == null) return;
        foreach (var slot in playerSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance == unit) { ui.Setup(unit); return; }
        }
        foreach (var slot in handSlots)
        {
            CardUI ui = slot.GetComponentInChildren<CardUI>();
            if (ui?.currentInstance == unit) { ui.Setup(unit); return; }
        }
    }
}
