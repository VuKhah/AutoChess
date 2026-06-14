using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Deck Window")]
    public GameObject cardBagPanel;
    public DeckPanelController deckController;

    [Header("Battle Scene")]
    public string battleSceneName;

    public void OpenDeck()
    {
        if (cardBagPanel != null)
            cardBagPanel.SetActive(true);

        if (deckController != null)
            deckController.OpenDeck();
    }

    public void CloseDeck()
    {
        if (cardBagPanel != null)
            cardBagPanel.SetActive(false);
    }

    public void StartBattle()
    {
        SceneManager.LoadScene(battleSceneName);
    }
}