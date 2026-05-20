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
    // BUG-AI-08 FIX: Guard null — nếu chưa train AI (file chưa tồn tại), fallback chromosome ngẫu nhiên.
    public Chromosome GetBrain(string difficulty)
    {
        if (loadedLibrary == null)
        {
            Debug.LogWarning("[AI] Chưa có AI Library — dùng chromosome ngẫu nhiên.");
            return new Chromosome();
        }
        switch (difficulty)
        {
            case "Easy":   return loadedLibrary.easyBot   ?? new Chromosome();
            case "Medium": return loadedLibrary.mediumBot ?? new Chromosome();
            default:       return loadedLibrary.hardBot   ?? new Chromosome();
        }
    }
}