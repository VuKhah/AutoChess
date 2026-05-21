using UnityEngine;

public static class CardSlotFitter
{
    private const float ShopFillMultiplier = 1.10f;
    private const float BoardFillMultiplier = 1.10f;

    public static void FitToSlot(RectTransform cardRect, Transform slot)
    {
        if (cardRect == null || slot == null) return;

        RectTransform slotRect = slot as RectTransform;
        if (slotRect == null) return;

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;

        Vector2 slotSize = slotRect.rect.size;
        if (slotSize.x <= 0f || slotSize.y <= 0f) return;

        Vector2 nativeSize = cardRect.rect.size;
        if (nativeSize.x <= 0f || nativeSize.y <= 0f)
            nativeSize = cardRect.sizeDelta;
        if (nativeSize.x <= 0f || nativeSize.y <= 0f) return;

        CardSlot cardSlot = slot.GetComponent<CardSlot>();
        bool isBoardSlot = cardSlot != null
            && (cardSlot.slotType == CardSlot.SlotType.PlayerBoard
                || cardSlot.slotType == CardSlot.SlotType.EnemyBoard);

        Vector2 visualSize = isBoardSlot
            ? new Vector2(nativeSize.y, nativeSize.x)
            : nativeSize;

        float widthScale = slotSize.x / visualSize.x;
        float heightScale = slotSize.y / visualSize.y;
        float fillMultiplier = cardSlot != null && cardSlot.slotType == CardSlot.SlotType.Shop
            ? ShopFillMultiplier
            : isBoardSlot ? BoardFillMultiplier : 1f;
        float scale = Mathf.Min(widthScale, heightScale) * fillMultiplier;
        cardRect.localScale = new Vector3(scale, scale, 1f);
    }
}
