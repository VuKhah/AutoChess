using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Chạy training GA đồng bộ — dùng cho batchmode (train_ai.ps1) và menu Editor.
///
/// Từ menu:  Tools → AI → Train AI (Quick / Production)
///
/// Từ command line:
///   Unity.exe -batchmode -quit -projectPath "..." -executeMethod AITrainingBatch.RunQuick
///
/// Output: Assets/Resources/AI_Library.json
///         Assets/Document/02_Data/training_TIMESTAMP.csv
/// </summary>
public static class AITrainingBatch
{
    private const int   QUICK_POP      = 30;
    private const int   QUICK_GEN      = 40;
    private const int   QUICK_MATCHES  = 5;

    private const int   PROD_POP       = 100;
    private const int   PROD_GEN       = 150;
    private const int   PROD_MATCHES   = 15;

    private const float MUTATION_RATE  = 0.08f;
    private const float MUTATION_MAG   = 0.12f;
    private const int   TOURNAMENT_K   = 3;

    [MenuItem("Tools/AI/Train AI — Quick (30 pop × 40 gen)")]
    public static void RunQuickFromMenu() => RunQuick();

    [MenuItem("Tools/AI/Train AI — Production (100 pop × 150 gen)")]
    public static void RunProductionFromMenu() => RunProduction();

    public static void RunQuick()      => Execute(QUICK_POP, QUICK_GEN, QUICK_MATCHES);
    public static void RunProduction() => Execute(PROD_POP,  PROD_GEN,  PROD_MATCHES);

    // ──────────────────────────────────────────────────────────────────────────
    private static void Execute(int popSize, int generations, int matchesPerChrom)
    {
        if (!InitCardDatabase()) return;

        Debug.Log($"[AITraining] Bắt đầu — pop={popSize} gen={generations} matches={matchesPerChrom} genes={Chromosome.GeneCount}");

        var library = RunGA(popSize, generations, matchesPerChrom);
        SaveLibrary(library);

        Debug.Log("[AITraining] Hoàn tất → Assets/Resources/AI_Library.json");
        AssetDatabase.Refresh();
    }

    // ──────────────────────────────────────────────────────────────────────────
    private static bool InitCardDatabase()
    {
        if (CardDatabase.Instance != null) return true;

        var go = new GameObject("CardDatabase_Training");
        var db = go.AddComponent<CardDatabase>();

        if (CardDatabase.Instance == null)
        {
            CardDatabase.Instance = db;
            db.LoadDatabase();
        }

        if (CardDatabase.Instance == null || CardDatabase.Instance.GetAllCards().Count == 0)
        {
            Debug.LogError("[AITraining] Không load được CardDatabase — kiểm tra Assets/Resources/CardsData.json");
            Object.DestroyImmediate(go);
            return false;
        }

        Debug.Log($"[AITraining] CardDatabase: {CardDatabase.Instance.GetAllUnits().Count} units, {CardDatabase.Instance.GetAllSpells().Count} spells");
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────────
    private static AILibrary RunGA(int popSize, int generations, int matchesPerChrom)
    {
        var sim = new GameSimulator();

        // ── Mở CSV — ghi trực tiếp, KHÔNG qua Unity logFile ─────────────────
        string csvDir  = Path.Combine(Application.dataPath, "Document", "02_Data", "Train");
        Directory.CreateDirectory(csvDir);
        string stamp   = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string csvPath = Path.Combine(csvDir, $"training_{stamp}.csv");

        var csv = new StreamWriter(csvPath, false, System.Text.Encoding.UTF8) { AutoFlush = true };
        csv.WriteLine("gen,best,avg,worst,std_dev,pct_babylon,pct_niles,pct_other");
        Debug.Log($"[AITraining] CSV → {csvPath}");

        // ── 5 seeded sub-population (mỗi nhóm 20%) ───────────────────────────
        var population = new List<Chromosome>(popSize);
        int groupSize  = Mathf.Max(1, popSize / 5);

        for (int i = 0; i < popSize; i++)
        {
            var c     = new Chromosome();
            int group = Mathf.Min(i / groupSize, 4);

            switch (group)
            {
                case 0: // Babylon tribe
                    c.genes[18] = Random.Range(0.7f, 1.0f);
                    c.genes[19] = Random.Range(0.0f, 0.3f);
                    c.genes[20] = Random.Range(0.0f, 0.3f);
                    break;
                case 1: // Niles tribe
                    c.genes[20] = Random.Range(0.7f, 1.0f);
                    c.genes[18] = Random.Range(0.0f, 0.3f);
                    c.genes[19] = Random.Range(0.0f, 0.3f);
                    break;
                case 2: // Aggressor — ATK rush, reroll heavy
                    c.genes[0]  = Random.Range(0.75f, 1.0f);
                    c.genes[9]  = Random.Range(0.70f, 1.0f);
                    c.genes[24] = Random.Range(0.75f, 1.0f);
                    c.genes[11] = Random.Range(0.00f, 0.20f);
                    c.genes[26] = Random.Range(0.00f, 0.20f);
                    break;
                case 3: // Resilient — HP/Taunt/Reborn defensive
                    c.genes[1]  = Random.Range(0.75f, 1.0f);
                    c.genes[4]  = Random.Range(0.70f, 1.0f);
                    c.genes[5]  = Random.Range(0.70f, 1.0f);
                    c.genes[6]  = Random.Range(0.70f, 1.0f);
                    c.genes[10] = Random.Range(0.70f, 1.0f);
                    break;
                // case 4: random — contributes to hardBot
            }
            population.Add(c);
        }

        // ── GA loop ───────────────────────────────────────────────────────────
        for (int g = 0; g < generations; g++)
        {
            Debug.Log($"[AITraining] >> Gen {g}/{generations} bắt đầu đánh giá fitness...");

            // Đánh giá fitness
            for (int ci = 0; ci < popSize; ci++)
            {
                var chromo = population[ci];
                chromo.fitness = 0f;
                for (int m = 0; m < matchesPerChrom; m++)
                {
                    int oppIdx = Random.Range(0, popSize - 1);
                    if (oppIdx >= ci) oppIdx++;
                    int result = sim.SimulateMatch(new BotAgent(chromo), new BotAgent(population[oppIdx]));
                    if (result ==  1) chromo.fitness += 10f;
                    else if (result == 0) chromo.fitness +=  2f;
                }
            }

            population = population.OrderByDescending(c => c.fitness).ToList();

            float best   = population[0].fitness;
            float avg    = population.Average(c => c.fitness);
            float worst  = population[population.Count - 1].fitness;
            float stdDev = Mathf.Sqrt(population.Average(c => (c.fitness - avg) * (c.fitness - avg)));

            int cntB  = population.Count(c => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20]);
            int cntN  = population.Count(c => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18]);
            float pctB = cntB * 100f / popSize;
            float pctN = cntN * 100f / popSize;
            float pctO = (popSize - cntB - cntN) * 100f / popSize;

            csv.WriteLine($"{g},{best:F0},{avg:F1},{worst:F0},{stdDev:F2},{pctB:F1},{pctN:F1},{pctO:F1}");
            Debug.Log($"[AITraining] Gen {g}/{generations}  Best={best:F0}  Avg={avg:F1}  Worst={worst:F0}  Std={stdDev:F1}  B={pctB:F0}% N={pctN:F0}% O={pctO:F0}%");

            population = BreedNextGen(population, popSize);
        }

        // ── Selection cuối ────────────────────────────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();
        float avgFinal  = population.Average(c => c.fitness);
        float threshold = avgFinal * 0.8f;
        var   viable    = population.Where(c => c.fitness >= threshold).ToList();

        var library = new AILibrary
        {
            hardBot = population[0].Clone(),

            babylonBot = population
                .FirstOrDefault(c => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20])
                ?.Clone(),

            nileBot = population
                .FirstOrDefault(c => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18])
                ?.Clone(),

            aggressorBot = viable.OrderByDescending(c => AggressorScore(c)).FirstOrDefault()?.Clone(),
            resilientBot = viable.OrderByDescending(c => ResilientScore(c)).FirstOrDefault()?.Clone(),
        };

        // Log kết quả cuối
        Debug.Log($"[AITraining] Hard      fitness={library.hardBot.fitness:F0}  rank=1");
        LogResult("Babylon",   library.babylonBot,   library.babylonBot != null   ? $"sBabylon={library.babylonBot.genes[18]:F3}"    : "not found");
        LogResult("Nile",      library.nileBot,       library.nileBot != null      ? $"sNiles={library.nileBot.genes[20]:F3}"          : "not found");
        LogResult("Aggressor", library.aggressorBot, library.aggressorBot != null ? $"score={AggressorScore(library.aggressorBot):F2}" : "not found");
        LogResult("Resilient", library.resilientBot, library.resilientBot != null ? $"score={ResilientScore(library.resilientBot):F2}" : "not found");

        csv.Close();
        return library;
    }

    // ──────────────────────────────────────────────────────────────────────────
    private static float AggressorScore(Chromosome c) =>
        c.genes[0] * 2f + c.genes[9] + c.genes[24] - c.genes[11] * 0.5f;

    private static float ResilientScore(Chromosome c) =>
        c.genes[1] + c.genes[4] + c.genes[5] * 1.5f + c.genes[6] + c.genes[10];

    private static void LogResult(string name, Chromosome bot, string extra) =>
        Debug.Log(bot != null
            ? $"[AITraining] {name,-10} fitness={bot.fitness:F0}  {extra}"
            : $"[AITraining] {name,-10} NOT FOUND in population");

    // ──────────────────────────────────────────────────────────────────────────
    private static List<Chromosome> BreedNextGen(List<Chromosome> population, int popSize)
    {
        int eliteCount = Mathf.Max(2, popSize / 10);
        var nextGen = population.Take(eliteCount).Select(c => c.Clone()).ToList();
        while (nextGen.Count < popSize)
            nextGen.Add(CrossoverAndMutate(population));
        return nextGen;
    }

    private static Chromosome CrossoverAndMutate(List<Chromosome> pool)
    {
        var a = TournamentSelect(pool);
        var b = TournamentSelect(pool);
        var child = new Chromosome();
        for (int i = 0; i < Chromosome.GeneCount; i++)
        {
            child.genes[i] = Random.value > 0.5f ? a.genes[i] : b.genes[i];
            if (Random.value < MUTATION_RATE)
            {
                float u1 = Mathf.Max(1e-6f, Random.value);
                float z  = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * Random.value);
                child.genes[i] += z * MUTATION_MAG;
            }
            child.genes[i] = Mathf.Clamp01(child.genes[i]);
        }
        return child;
    }

    private static Chromosome TournamentSelect(List<Chromosome> pool)
    {
        Chromosome best = null;
        for (int i = 0; i < TOURNAMENT_K; i++)
        {
            var c = pool[Random.Range(0, pool.Count)];
            if (best == null || c.fitness > best.fitness) best = c;
        }
        return best;
    }

    private static void SaveLibrary(AILibrary library)
    {
        string dir  = Path.Combine(Application.dataPath, "Resources");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "AI_Library.json");
        File.WriteAllText(path, JsonUtility.ToJson(library, true));
        Debug.Log($"[AITraining] Đã lưu: {path}");
    }
}
