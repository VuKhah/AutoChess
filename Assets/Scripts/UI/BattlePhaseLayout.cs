using UnityEngine;

public class BattlePhaseLayout : MonoBehaviour
{
    public static BattlePhaseLayout Instance;

    private readonly Vector2[] playerCombatAnchors =
    {
        new Vector2(0.342f, 0.494f),
        new Vector2(0.445f, 0.494f),
        new Vector2(0.552f, 0.494f),
        new Vector2(0.662f, 0.494f),
        new Vector2(0.390f, 0.370f),
        new Vector2(0.500f, 0.370f),
        new Vector2(0.607f, 0.370f),
    };

    private readonly Vector2[] enemyCombatAnchors =
    {
        new Vector2(0.342f, 0.714f),
        new Vector2(0.445f, 0.714f),
        new Vector2(0.552f, 0.714f),
        new Vector2(0.662f, 0.714f),
        new Vector2(0.396f, 0.846f),
        new Vector2(0.500f, 0.846f),
        new Vector2(0.604f, 0.846f),
    };

    private RectTransform[] playerSlots;
    private RectTransform[] enemySlots;
    private SlotState[] playerShopStates;
    private SlotState[] enemyShopStates;
    private RectTransform actionButtonRect;
    private SlotState actionShopState;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CacheSlots();
        CacheActionButton();
    }

    public void ApplyPhase(bool isCombat)
    {
        if (playerSlots == null || enemySlots == null)
            CacheSlots();
        if (actionButtonRect == null)
            CacheActionButton();

        if (isCombat)
        {
            ApplyCombatSlots(playerSlots, playerCombatAnchors);
            ApplyCombatSlots(enemySlots, enemyCombatAnchors);
            ApplyActionCombatRect();
        }
        else
        {
            RestoreSlots(playerSlots, playerShopStates);
            RestoreSlots(enemySlots, enemyShopStates);
            RestoreRect(actionButtonRect, actionShopState);
        }
    }

    private void CacheSlots()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null) return;

        playerSlots = ToRectTransforms(manager.playerSlots);
        enemySlots = ToRectTransforms(manager.enemySlots);
        playerShopStates = Capture(playerSlots);
        enemyShopStates = Capture(enemySlots);
    }

    private void CacheActionButton()
    {
        if (UIManager.Instance == null || UIManager.Instance.actionButton == null) return;
        actionButtonRect = UIManager.Instance.actionButton.GetComponent<RectTransform>();
        actionShopState = Capture(actionButtonRect);
    }

    private static RectTransform[] ToRectTransforms(Transform[] transforms)
    {
        RectTransform[] result = new RectTransform[transforms.Length];
        for (int i = 0; i < transforms.Length; i++)
            result[i] = transforms[i] as RectTransform;
        return result;
    }

    private static SlotState[] Capture(RectTransform[] rects)
    {
        SlotState[] result = new SlotState[rects.Length];
        for (int i = 0; i < rects.Length; i++)
            result[i] = Capture(rects[i]);
        return result;
    }

    private static SlotState Capture(RectTransform rect)
    {
        return new SlotState(rect.anchorMin, rect.anchorMax, rect.anchoredPosition, rect.sizeDelta, rect.localScale);
    }

    private static void RestoreSlots(RectTransform[] rects, SlotState[] states)
    {
        for (int i = 0; i < rects.Length && i < states.Length; i++)
            RestoreRect(rects[i], states[i]);
    }

    private static void RestoreRect(RectTransform rect, SlotState state)
    {
        if (rect == null) return;
        rect.anchorMin = state.anchorMin;
        rect.anchorMax = state.anchorMax;
        rect.anchoredPosition = state.anchoredPosition;
        rect.sizeDelta = state.sizeDelta;
        rect.localScale = state.localScale;
    }

    private static void ApplyCombatSlots(RectTransform[] rects, Vector2[] anchors)
    {
        if (rects == null) return;
        for (int i = 0; i < rects.Length && i < anchors.Length; i++)
        {
            RectTransform rect = rects[i];
            if (rect == null) continue;
            rect.anchorMin = anchors[i];
            rect.anchorMax = anchors[i];
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(145f, 110f);
            rect.localScale = Vector3.one;
        }
    }

    private void ApplyActionCombatRect()
    {
        if (actionButtonRect == null) return;
        actionButtonRect.anchorMin = new Vector2(0.822f, 0.085f);
        actionButtonRect.anchorMax = new Vector2(0.962f, 0.275f);
        actionButtonRect.anchoredPosition = Vector2.zero;
        actionButtonRect.sizeDelta = Vector2.zero;
        actionButtonRect.localScale = Vector3.one;
    }

    private readonly struct SlotState
    {
        public readonly Vector2 anchorMin;
        public readonly Vector2 anchorMax;
        public readonly Vector2 anchoredPosition;
        public readonly Vector2 sizeDelta;
        public readonly Vector3 localScale;

        public SlotState(Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector3 localScale)
        {
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.anchoredPosition = anchoredPosition;
            this.sizeDelta = sizeDelta;
            this.localScale = localScale;
        }
    }
}
