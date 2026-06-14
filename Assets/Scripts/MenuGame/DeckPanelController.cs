using System.Collections.Generic;
using UnityEngine;

public class DeckPanelController : MonoBehaviour
{
    [Header("UI")]
    public GameObject deckPanel;

    [Header("Cards")]
    public Transform contentParent;
    public GameObject cardPrefab;

    private bool loaded = false;

    public void OpenDeck()
    {
        Debug.Log("OpenDeck được gọi");

        deckPanel.SetActive(true);

        if (!loaded)
        {
            LoadCards();
            loaded = true;
        }
    }

    public void CloseDeck()
    {
        deckPanel.SetActive(false);
    }

    private void LoadCards()
    {
        Debug.Log("=== LoadCards bắt đầu ===");

        if (CardDatabase.Instance == null)
        {
            Debug.LogError("CardDatabase.Instance = NULL");
            return;
        }

        if (contentParent == null)
        {
            Debug.LogError("contentParent = NULL");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("cardPrefab = NULL");
            return;
        }

        List<CardDefinition> cards =
            CardDatabase.Instance.GetAllCards();

        Debug.Log("Số card tìm thấy: " + cards.Count);

        foreach (CardDefinition cardData in cards)
        {
            Debug.Log("Đang tạo: " + cardData.cardName);

            GameObject obj =
                Instantiate(cardPrefab, contentParent);

            Debug.Log("Spawn xong: " + obj.name);

            CardUI ui =
                obj.GetComponent<CardUI>();

            if (ui == null)
            {
                Debug.LogError("Prefab không có CardUI");
                continue;
            }

            CardInstance instance =
                new CardInstance(cardData, -1);

            ui.Setup(instance);

            CardDraggable drag =
                obj.GetComponent<CardDraggable>();

            if (drag != null)
                drag.enabled = false;
        }

        Debug.Log("=== LoadCards hoàn tất ===");
    }
}