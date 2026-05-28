using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GATrainer : MonoBehaviour
{
    [Header("Cấu hình huấn luyện")]
    public int populationSize  = 30;   // test nhanh: 30 | production: 120
    public int generations     = 40;   // test nhanh: 40 | production: 120
    public int matchesPerChrom = 5;    // test nhanh: 5  | production: 20
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

    private StreamWriter _csv; // giữ reference để đóng khi ForceRetrain gián đoạn

    [ContextMenu("Retrain AI (Force)")]
    public void ForceRetrain()
    {
        StopAllCoroutines();
        _csv?.Close();
        _csv = null;
        BeginTraining();
    }

    // Chỉ yêu cầu hardBot — các specialist bot là bonus, không bắt buộc.
    private bool IsLibraryValid()
    {
        TextAsset file = Resources.Load<TextAsset>("AI_Library");
        if (file == null) return false;
        var lib = JsonUtility.FromJson<AILibrary>(file.text);
        return lib?.hardBot?.genes != null && lib.hardBot.genes.Length >= Chromosome.GeneCount;
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

        // ── 5 seeded sub-population (mỗi nhóm 20%) ───────────────────────────
        // Duy trì diversity để GA tìm được specialist cho từng archetype.
        int groupSize = Mathf.Max(1, populationSize / 5);

        for (int i = 0; i < populationSize; i++)
        {
            var c = new Chromosome();
            int group = Mathf.Min(i / groupSize, 4); // 0-4, nhóm 4 = random

            switch (group)
            {
                case 0: // Babylon tribe seed
                    c.genes[18] = Random.Range(0.7f, 1.0f);
                    c.genes[19] = Random.Range(0.0f, 0.3f);
                    c.genes[20] = Random.Range(0.0f, 0.3f);
                    break;
                case 1: // Niles tribe seed
                    c.genes[20] = Random.Range(0.7f, 1.0f);
                    c.genes[18] = Random.Range(0.0f, 0.3f);
                    c.genes[19] = Random.Range(0.0f, 0.3f);
                    break;
                case 2: // Aggressor seed — ATK rush, reroll hết tiền, bỏ growth
                    c.genes[0]  = Random.Range(0.75f, 1.0f); // wATK
                    c.genes[9]  = Random.Range(0.70f, 1.0f); // tOnAttack
                    c.genes[24] = Random.Range(0.75f, 1.0f); // wRerollThresh
                    c.genes[11] = Random.Range(0.00f, 0.2f); // tEndTurnShop (thấp)
                    c.genes[26] = Random.Range(0.00f, 0.2f); // wRerollKeep (thấp — tiêu tiền nhanh)
                    break;
                case 3: // Resilient seed — bền bỉ, phản đòn, không chết dễ
                    c.genes[1]  = Random.Range(0.75f, 1.0f); // wHP
                    c.genes[4]  = Random.Range(0.70f, 1.0f); // wTaunt
                    c.genes[5]  = Random.Range(0.70f, 1.0f); // wReborn
                    c.genes[6]  = Random.Range(0.70f, 1.0f); // wSafeguard
                    c.genes[10] = Random.Range(0.70f, 1.0f); // tOnTakeDmg
                    break;
                // case 4: random hoàn toàn — đóng góp cho hardBot
            }
            population.Add(c);
        }

        Debug.LogWarning($"[GATrainer] BẮT ĐẦU TRAINING — Genes:{Chromosome.GeneCount} Pop:{populationSize} Gen:{generations}");
        StartCoroutine(TrainRoutine());
    }

    IEnumerator TrainRoutine()
    {
        // ── Mở CSV log ────────────────────────────────────────────────────────
        string csvDir  = Path.Combine(Application.dataPath, "Document", "02_Data", "Train");
        Directory.CreateDirectory(csvDir);
        string stamp   = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string csvPath = Path.Combine(csvDir, $"training_{stamp}.csv");

        _csv = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8) { AutoFlush = true };
        var csv = _csv;
        csv.WriteLine("gen,best,avg,worst,std_dev,pct_babylon,pct_niles,pct_other");

        Debug.Log($"=== HUẤN LUYỆN AI === Genes:{Chromosome.GeneCount} | Pop:{populationSize} | Gen:{generations} ===");
        Debug.Log($"[GATrainer] CSV → {csvPath}");

        const int   PLATEAU_PATIENCE = 15;   // số gen liên tiếp không đổi → dừng
        const float PLATEAU_EPS      = 0.5f; // ngưỡng thay đổi std_dev tính là "không đổi"
        int   plateauCount = 0;
        float prevStdDev   = -1f;

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
                    BotAgent opp     = new BotAgent(population[oppIdx]);
                    int result       = sim.SimulateMatch(me, opp);

                    if (result ==  1) chromo.fitness += 10f;
                    else if (result == 0) chromo.fitness +=  2f;
                }
            }

            // ── Sắp xếp ─────────────────────────────────────────────────────
            population = population.OrderByDescending(c => c.fitness).ToList();

            float best  = population[0].fitness;
            float avg   = population.Average(c => c.fitness);
            float worst = population[population.Count - 1].fitness;
            float stdDev = Mathf.Sqrt(population.Average(c => (c.fitness - avg) * (c.fitness - avg)));

            // ── Tribe distribution ───────────────────────────────────────────
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

            // ── Early stopping — std_dev plateau ─────────────────────────────
            if (prevStdDev >= 0f && Mathf.Abs(stdDev - prevStdDev) < PLATEAU_EPS)
                plateauCount++;
            else
                plateauCount = 0;
            prevStdDev = stdDev;

            if (plateauCount >= PLATEAU_PATIENCE)
            {
                Debug.LogWarning($"[GATrainer] Early stop tại gen {g} — std_dev plateau {PLATEAU_PATIENCE} gen liên tiếp (Δ<{PLATEAU_EPS}).");
                break;
            }

            // ── Elitism + breed ───────────────────────────────────────────────
            int eliteCount    = Mathf.Max(2, populationSize / 10);
            List<Chromosome> nextGen = population.Take(eliteCount).Select(c => c.Clone()).ToList();
            while (nextGen.Count < populationSize)
            {
                nextGen.Add(CrossoverAndMutate(
                    TournamentSelect(population, 3),
                    TournamentSelect(population, 3)));
            }
            population = nextGen;
            yield return null;
        }

        // ── Sort lần cuối ─────────────────────────────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();
        float avgFinal    = population.Average(c => c.fitness);
        float threshold   = avgFinal * 0.8f; // ngưỡng "đủ tốt" để chọn specialist
        var   viable      = population.Where(c => c.fitness >= threshold).ToList();

        // ── Chọn 5 bot ───────────────────────────────────────────────────────
        library.hardBot = population[0].Clone();

        library.babylonBot = population
            .FirstOrDefault(c => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20])
            ?.Clone();

        library.nileBot = population
            .FirstOrDefault(c => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18])
            ?.Clone();

        library.aggressorBot = viable
            .OrderByDescending(c => AggressorScore(c))
            .FirstOrDefault()?.Clone();

        library.resilientBot = viable
            .OrderByDescending(c => ResilientScore(c))
            .FirstOrDefault()?.Clone();

        // ── Log kết quả ──────────────────────────────────────────────────────
        LogBot("Hard",      library.hardBot,      "rank=1");
        LogBot("Babylon",   library.babylonBot,   library.babylonBot   != null ? $"sBabylon={library.babylonBot.genes[18]:F3}"    : "");
        LogBot("Nile",      library.nileBot,       library.nileBot      != null ? $"sNiles={library.nileBot.genes[20]:F3}"          : "");
        LogBot("Aggressor", library.aggressorBot, library.aggressorBot != null ? $"score={AggressorScore(library.aggressorBot):F2}" : "");
        LogBot("Resilient", library.resilientBot, library.resilientBot != null ? $"score={ResilientScore(library.resilientBot):F2}" : "");

        csv.Close();
        SaveLibrary();
        Debug.Log($"=== HUẤN LUYỆN HOÀN TẤT === CSV: {csvPath}");
    }

    // ── Scoring functions cho archetype selection ─────────────────────────────
    private static float AggressorScore(Chromosome c)
    {
        if (c == null) return float.MinValue;
        return c.genes[0] * 2f    // wATK — quan trọng nhất
             + c.genes[9]         // tOnAttack
             + c.genes[24]        // wRerollThresh
             - c.genes[11] * 0.5f; // tEndTurnShop thấp là tốt
    }

    private static float ResilientScore(Chromosome c)
    {
        if (c == null) return float.MinValue;
        return c.genes[1]         // wHP
             + c.genes[4]         // wTaunt
             + c.genes[5] * 1.5f  // wReborn — quan trọng nhất
             + c.genes[6]         // wSafeguard
             + c.genes[10];       // tOnTakeDmg
    }

    private void LogBot(string name, Chromosome bot, string extra)
    {
        if (bot != null)
            Debug.Log($"► <b>{name} Bot</b>  fitness={bot.fitness:F0}  {extra}");
        else
            Debug.LogWarning($"[GATrainer] {name} specialist không tìm thấy trong population cuối.");
    }

    // ── GA helpers ────────────────────────────────────────────────────────────
    private Chromosome TournamentSelect(List<Chromosome> pool, int k)
    {
        Chromosome best = null;
        for (int i = 0; i < k; i++)
        {
            var c = pool[Random.Range(0, pool.Count)];
            if (best == null || c.fitness > best.fitness) best = c;
        }
        return best;
    }

    private Chromosome CrossoverAndMutate(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();

        // 2-point crossover: bảo toàn co-adapted gene complexes (epistasis)
        // child = [a|b|a] — đoạn giữa [pt1, pt2) lấy từ b, hai đầu từ a
        int pt1 = Random.Range(0, Chromosome.GeneCount);
        int pt2 = Random.Range(0, Chromosome.GeneCount);
        if (pt1 > pt2) { int tmp = pt1; pt1 = pt2; pt2 = tmp; }

        for (int i = 0; i < Chromosome.GeneCount; i++)
        {
            child.genes[i] = (i >= pt1 && i < pt2) ? b.genes[i] : a.genes[i];
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

    private void SaveLibrary()
    {
        string dirPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        string path = Path.Combine(dirPath, "AI_Library.json");
        File.WriteAllText(path, JsonUtility.ToJson(library, true));
        Debug.Log("<b>Đã lưu AI_Library.json:</b> " + path);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
