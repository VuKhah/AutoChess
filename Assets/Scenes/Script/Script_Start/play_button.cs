using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class play_button : MonoBehaviour
{
    [SerializeReference] private Button play_btn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        play_btn.onClick.AddListener(PlayGame);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Home_Scene");
    }


}
