using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance;
    public AILibrary loadedLibrary;

    void Awake()
    {
        Instance = this;
        LoadAI();
    }

    void LoadAI()
    {
        TextAsset file = Resources.Load<TextAsset>("AI_Library");
        if (file != null)
        {
            loadedLibrary = JsonUtility.FromJson<AILibrary>(file.text);
            Debug.Log("Đã nạp dữ liệu AI: Dễ/Vừa/Khó sẵn sàng!");
        }
    }

    // Hàm để lấy Gen theo độ khó
    public Chromosome GetBrain(string difficulty)
    {
        switch (difficulty)
        {
            case "Easy": return loadedLibrary.easyBot;
            case "Medium": return loadedLibrary.mediumBot;
            default: return loadedLibrary.hardBot;
        }
    }
}