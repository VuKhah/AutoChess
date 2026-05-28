using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GATrainer : MonoBehaviour
{
    [Header("Cấu hình huấn luyện")]
    public int populationSize  = 30;   // test nhanh: 30 | production: 100
    public int generations     = 40;   // test nhanh: 40 | production: 150
    public int matchesPerChrom = 5;    // test nhanh: 5  | production: 15
    [Range(0.05f, 0.25f)]
    public float mutationRate  = 0.08f;
    [Range(0.05f, 0.2f)]
    public float mutationMag   = 0.12f;

    private List<Chromosome> population = new List<Chromosome>();
    private GameSimulator sim = new GameSimulator();
    private AILibrary library = new AILibrary();

    void Start()
    {
        if (IsLibraryValid())
        {
            Debug.Log("[GATrainer] AI_Library.json đã hợp lệ — bỏ qua training.");
            return;
        }
        BeginTraining();
    }

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
        return lib?.easyBot?.genes   != null && lib.easyBot.genes.Length   >= Chromosome.GeneCount
            && lib.mediumBot?.genes  != null && lib.mediumBot.genes.Length  >= Chromosome.GeneCount
            && lib.hardBot?.genes    != null && lib.hardBot.genes.Length    >= Chromosome.GeneCount;
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

        // Seeded sub-population: 20% Babylon + 20% Niles + 60% random
        // Giúp GA giữ đa dạng tribe để tìm được specialist cuối training.
        int babylonSeed = populationSize / 5;
        int nilesSeed   = populationSize / 5;

        for (int i = 0; i < populationSize; i++)
        {
            var c = new Chromosome();
            if (i < babylonSeed)
            {
                c.genes[18] = Random.Range(0.7f, 1.0f); // sBabylon cao
                c.genes[19] = Random.Range(0.0f, 0.3f);
                c.genes[20] = Random.Range(0.0f, 0.3f);
            }
            else if (i < babylonSeed + nilesSeed)
            {
                c.genes[20] = Random.Range(0.7f, 1.0f); // sNiles cao
                c.genes[18] = Random.Range(0.0f, 0.3f);
                c.genes[19] = Random.Range(0.0f, 0.3f);
            }
            population.Add(c);
        }

        Debug.LogWarning($"[GATrainer] BẮT ĐẦU TRAINING — Genes:{Chromosome.GeneCount} Pop:{populationSize} Gen:{generations}");
        StartCoroutine(TrainRoutine());
    }

    IEnumerator TrainRoutine()
    {
        // Mở CSV log — ghi thẳng từ C#, không qua Unity logFile
        string csvDir = Path.Combine(Application.dataPath, "Document", "02_Data");
        Directory.CreateDirectory(csvDir);
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string csvPath   = Path.Combine(csvDir, $"training_{timestamp}.csv");

        var csv = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8) { AutoFlush = true };
        csv.WriteLine("gen,best,avg,worst,std_dev,pct_babylon,pct_niles,pct_olympus");

        Debug.Log($"=== HUẤN LUYỆN AI === Genes:{Chromosome.GeneCount} | Pop:{populationSize} | Gen:{generations} ===");
        Debug.Log($"[GATrainer] CSV → {csvPath}");

        for (int g = 0; g < generations; g++)
        {
            // ── Đánh giá Fitness ─────────────────────────────────────────────
            foreach (var chromo in population)
            {
                chromo.fitness = 0f;
                for (int m = 0; m < matchesPerChrom; m++)
                {
                    BotAgent me      = new BotAgent(chromo);
                    int selfIdx      = population.IndexOf(chromo);
                    int oppIdx       = Random.Range(0, population.Count - 1);
                    if (oppIdx >= selfIdx) oppIdx++;
                    BotAgent opponent = new BotAgent(population[oppIdx]);
                    int result        = sim.SimulateMatch(me, opponent);

                    if (result ==  1) chromo.fitness += 10f;
                    else if (result == 0) chromo.fitness +=  2f;
                }
            }

            // ── Sắp xếp ─────────────────────────────────────────────────────
            population = population.OrderByDescending(c => c.fitness).ToList();

            float best  = population[0].fitness;
            float avg   = population.Average(c => c.fitness);
            float worst = population[population.Count - 1].fitness;

            float variance = population.Average(c => (c.fitness - avg) * (c.fitness - avg));
            float stdDev   = Mathf.Sqrt(variance);

            // ── Tribe distribution — đo đa dạng tribe trong population ───────
            int cntB = population.Count(c => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20]);
            int cntN = population.Count(c => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18]);
            int cntO = populationSize - cntB - cntN;
            float pctB = cntB * 100f / populationSize;
            float pctN = cntN * 100f / populationSize;
            float pctO = cntO * 100f / populationSize;

            // ── Ghi CSV ──────────────────────────────────────────────────────
            csv.WriteLine($"{g},{best:F0},{avg:F1},{worst:F0},{stdDev:F2},{pctB:F1},{pctN:F1},{pctO:F1}");

            Debug.Log($"Gen {g,3}/{generations}  Best={best,5:F0}  Avg={avg,5:F1}  Worst={worst,5:F0}  " +
                      $"Std={stdDev:F1}  B={pctB:F0}% N={pctN:F0}% O={pctO:F0}%");

            // ── Elitism + breed ───────────────────────────────────────────────
            int eliteCount = Mathf.Max(2, populationSize / 10);
            List<Chromosome> elite  = population.Take(eliteCount).Select(c => c.Clone()).ToList();
            List<Chromosome> nextGen = new List<Chromosome>(elite);
            while (nextGen.Count < populationSize)
            {
                Chromosome parentA = TournamentSelect(population, 3);
                Chromosome parentB = TournamentSelect(population, 3);
                nextGen.Add(CrossoverAndMutate(parentA, parentB));
            }
            population = nextGen;
            yield return null;
        }

        // ── Sort lần cuối ─────────────────────────────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();

        // ── Rank-based: 3 bot theo thứ hạng fitness ──────────────────────────
        library.hardBot   = population[0].Clone();
        library.mediumBot = population[Mathf.Min(populationSize / 3,     populationSize - 1)].Clone();
        library.easyBot   = population[Mathf.Min(populationSize * 2 / 3, populationSize - 1)].Clone();

        // ── Tribe-filtered: best chromosome có tribe gene tương ứng cao nhất ─
        library.babylonBot = population
            .FirstOrDefault(c => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20])
            ?.Clone();
        library.nileBot = population
            .FirstOrDefault(c => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18])
            ?.Clone();

        // ── Log kết quả ──────────────────────────────────────────────────────
        Debug.Log($"<color=red>►  Hard Bot</color>    rank=1      fitness={library.hardBot.fitness:F0}");
        Debug.Log($"<color=yellow>►  Medium Bot</color>  rank={populationSize/3}     fitness={library.mediumBot.fitness:F0}");
        Debug.Log($"<color=green>►  Easy Bot</color>    rank={populationSize*2/3}     fitness={library.easyBot.fitness:F0}");

        if (library.babylonBot != null)
            Debug.Log($"<color=orange>►  Babylon Bot</color>          fitness={library.babylonBot.fitness:F0}  sBabylon={library.babylonBot.genes[18]:F3}");
        else
            Debug.LogWarning("[GATrainer] Babylon specialist không tồn tại trong population cuối — babylonBot sẽ null.");

        if (library.nileBot != null)
            Debug.Log($"<color=cyan>►  Nile Bot</color>             fitness={library.nileBot.fitness:F0}  sNiles={library.nileBot.genes[20]:F3}");
        else
            Debug.LogWarning("[GATrainer] Nile specialist không tồn tại trong population cuối — nileBot sẽ null.");

        csv.Close();
        SaveLibrary();
        Debug.Log($"=== HUẤN LUYỆN HOÀN TẤT === CSV: {csvPath}");
    }

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
            child.genes[i] = Random.value > 0.5f ? a.genes[i] : b.genes[i];

            if (Random.value < mutationRate)
            {
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
