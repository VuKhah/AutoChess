using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class GATrainer : MonoBehaviour
{
    [Header("Cấu hình huấn luyện")]
    public int populationSize  = 30;   // test nhanh: 30 | production: 100
    public int generations     = 40;   // test nhanh: 40 | production: 150
    public int matchesPerChrom = 5;    // test nhanh: 5  | production: 15
    [Range(0.05f, 0.25f)]
    public float mutationRate  = 0.08f;
    [Range(0.05f, 0.2f)]
    public float mutationMag   = 0.12f; // biên độ đột biến Gaussian

    private List<Chromosome> population = new List<Chromosome>();
    private GameSimulator sim = new GameSimulator();
    private AILibrary library = new AILibrary();

    void Start()
    {
        if (IsLibraryValid())
        {
            Debug.Log("[GATrainer] AI_Library.json đã có đủ 3 bot — bỏ qua training.");
            return;
        }
        BeginTraining();
    }

    // Gọi từ Inspector (chuột phải vào component) để train lại từ đầu dù file đã có.
    [ContextMenu("Retrain AI (Force)")]
    public void ForceRetrain()
    {
        StopAllCoroutines();
        BeginTraining();
    }

    private bool IsLibraryValid()
    {
        TextAsset file = Resources.Load<TextAsset>("AI_Library");
        if (file == null) return false;
        var lib = JsonUtility.FromJson<AILibrary>(file.text);
        return lib?.easyBot?.genes  != null && lib.easyBot.genes.Length  >= Chromosome.GeneCount
            && lib.mediumBot?.genes != null && lib.mediumBot.genes.Length >= Chromosome.GeneCount
            && lib.hardBot?.genes   != null && lib.hardBot.genes.Length   >= Chromosome.GeneCount;
    }

    private void BeginTraining()
    {
        if (CardDatabase.Instance == null)
        {
            Debug.LogError("[GATrainer] CardDatabase chưa sẵn sàng — training bị huỷ.");
            return;
        }
        population.Clear();
        library = new AILibrary();
        for (int i = 0; i < populationSize; i++)
            population.Add(new Chromosome());
        Debug.LogWarning($"[GATrainer] BẮT ĐẦU TRAINING — Genes:{Chromosome.GeneCount} Pop:{populationSize} Gen:{generations}");
        StartCoroutine(TrainRoutine());
    }

    IEnumerator TrainRoutine()
    {
        Debug.Log($"=== HUẤN LUYỆN AI === Genes: {Chromosome.GeneCount} | Pop: {populationSize} | Gen: {generations} ===");

        for (int g = 0; g < generations; g++)
        {
            // ── Đánh giá Fitness ─────────────────────────────────────────────
            foreach (var chromo in population)
            {
                chromo.fitness = 0f;
                for (int m = 0; m < matchesPerChrom; m++)
                {
                    BotAgent me       = new BotAgent(chromo);
                    int oppIdx = Random.Range(0, population.Count - 1);
                    if (population[oppIdx] == chromo) oppIdx = population.Count - 1;
                    BotAgent opponent = new BotAgent(population[oppIdx]);
                    int result = sim.SimulateMatch(me, opponent);

                    if (result ==  1) chromo.fitness += 10f;
                    else if (result == 0) chromo.fitness +=  2f;
                }
            }

            // ── Sắp xếp ─────────────────────────────────────────────────────
            population = population.OrderByDescending(c => c.fitness).ToList();

            float best = population[0].fitness;
            float avg  = population.Average(c => c.fitness);
            float worst = population[population.Count - 1].fitness;
            Debug.Log($"Gen {g,3}/{generations}  Best={best,5:F0}  Avg={avg,5:F1}  Worst={worst,5:F0}");

            // ── Snapshots — tính theo % tổng gen để hoạt động mọi config ────────
            int easyAt   = Mathf.Max(1, generations / 4);   // 25% đầu
            int mediumAt = Mathf.Max(2, generations / 2);   // 50% giữa
            int halfAt   = Mathf.Max(3, generations * 3/4); // 75%

            if (g == easyAt)   { library.easyBot  = population[0].Clone(); Debug.Log($"<color=green>► Easy Bot snapshot (Gen {g})</color>"); }
            if (g == mediumAt) { library.mediumBot = population[0].Clone(); Debug.Log($"<color=yellow>► Medium Bot snapshot (Gen {g})</color>"); }
            if (g == halfAt)   Debug.Log($"<color=cyan>── Checkpoint (Gen {g}/{generations}) ──</color>");

            // ── Elitism: clone top 10%, breed phần còn lại ───────────────────
            int eliteCount = Mathf.Max(2, populationSize / 10);
            List<Chromosome> elite = population.Take(eliteCount)
                                               .Select(c => c.Clone())
                                               .ToList();

            List<Chromosome> nextGen = new List<Chromosome>(elite);
            while (nextGen.Count < populationSize)
            {
                Chromosome parentA = TournamentSelect(population, 3);
                Chromosome parentB = TournamentSelect(population, 3);
                nextGen.Add(CrossoverAndMutate(parentA, parentB));
            }

            population = nextGen;
            yield return null; // nhường frame
        }

        // ── Hard Bot: kết quả sau training đầy đủ ────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();
        library.hardBot = population[0].Clone();

        string geneStr = string.Join(", ", library.hardBot.genes.Select(g2 => g2.ToString("F3")));
        Debug.Log($"<color=red>► Hard Bot snapshot (Gen Final)</color>\nGenes: [{geneStr}]");

        SaveLibrary();
        Debug.Log("=== HUẤN LUYỆN HOÀN TẤT ===");
    }

    // Tournament selection: chọn 1 winner từ k đấu thủ ngẫu nhiên
    private Chromosome TournamentSelect(List<Chromosome> pool, int k)
    {
        Chromosome best = null;
        for (int i = 0; i < k; i++)
        {
            Chromosome candidate = pool[Random.Range(0, pool.Count)];
            if (best == null || candidate.fitness > best.fitness)
                best = candidate;
        }
        return best;
    }

    private Chromosome CrossoverAndMutate(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();
        for (int i = 0; i < Chromosome.GeneCount; i++)
        {
            // Uniform crossover
            child.genes[i] = Random.value > 0.5f ? a.genes[i] : b.genes[i];

            if (Random.value < mutationRate)
            {
                // Box-Muller → phân phối chuẩn N(0, mutationMag)
                float u1 = Mathf.Max(1e-6f, Random.value);
                float z  = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * Random.value);
                child.genes[i] += z * mutationMag;
            }

            child.genes[i] = Mathf.Clamp01(child.genes[i]);
        }
        return child;
    }

    void SaveLibrary()
    {
        string dirPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        string json = JsonUtility.ToJson(library, true);
        string path = Path.Combine(dirPath, "AI_Library.json");
        File.WriteAllText(path, json);
        Debug.Log("<b>Đã lưu AI_Library.json:</b> " + path);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
