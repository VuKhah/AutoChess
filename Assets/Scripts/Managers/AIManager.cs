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
            Debug.Log("[AIManager] AI_Library nạp thành công: Hard / Babylon / Nile / Summoner / Resilient");
        }
    }

    public Chromosome GetBrain(string botName)
    {
        if (loadedLibrary == null) return null;
        switch (botName)
        {
            case "Babylon":   return loadedLibrary.babylonBot;
            case "Nile":      return loadedLibrary.nileBot;
            case "Summoner":  return loadedLibrary.summonerBot;
            case "Resilient": return loadedLibrary.resilientBot;
            default:          return loadedLibrary.hardBot;
        }
    }
}