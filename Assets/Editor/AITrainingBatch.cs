using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Chạy training GA đồng bộ không cần mở Unity Editor GUI.
///
/// Từ menu:  Tools → AI → Train AI (Quick / Production)
///
/// Từ command line (không cần mở Editor):
///   "C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe"
///     -batchmode -quit
///     -projectPath "D:\workspaces\specialized-essays\AutoChess"
///     -executeMethod AITrainingBatch.RunQuick
///     -logFile training_log.txt
///
/// Kết quả lưu vào: Assets/Resources/AI_Library.json
/// </summary>
public static class AITrainingBatch
{
    // ── Cấu hình Quick (test nhanh ~1-2 phút) ─────────────────────────────
    private const int   QUICK_POP       = 30;
    private const int   QUICK_GEN       = 40;
    private const int   QUICK_MATCHES   = 5;

    // ── Cấu hình Production (~15-30 phút) ─────────────────────────────────
    private const int   PROD_POP        = 100;
    private const int   PROD_GEN        = 150;
    private const int   PROD_MATCHES    = 15;

    // ── Hyper-parameters (dùng chung) ─────────────────────────────────────
    private const float MUTATION_RATE   = 0.08f;
    private const float MUTATION_MAG    = 0.12f;
    private const int   TOURNAMENT_K    = 3;

    // ──────────────────────────────────────────────────────────────────────
    // MENU ITEMS
    // ──────────────────────────────────────────────────────────────────────

    [MenuItem("Tools/AI/Train AI — Quick (30 pop × 40 gen)")]
    public static void RunQuickFromMenu() => RunQuick();

    [MenuItem("Tools/AI/Train AI — Production (100 pop × 150 gen)")]
    public static void RunProductionFromMenu() => RunProduction();

    // ──────────────────────────────────────────────────────────────────────
    // ENTRY POINTS cho -executeMethod (tên phải là static public void)
    // ──────────────────────────────────────────────────────────────────────

    public static void RunQuick()      => Execute(QUICK_POP, QUICK_GEN, QUICK_MATCHES);
    public static void RunProduction() => Execute(PROD_POP,  PROD_GEN,  PROD_MATCHES);

    // ──────────────────────────────────────────────────────────────────────
    // CORE
    // ──────────────────────────────────────────────────────────────────────

    private static void Execute(int popSize, int generations, int matchesPerChrom)
    {
        if (!InitCardDatabase()) return;

        Debug.Log($"[AI Training] Bắt đầu: pop={popSize}, gen={generations}, matches={matchesPerChrom}, genes={Chromosome.GeneCount}");

        var library = RunGA(popSize, generations, matchesPerChrom);
        SaveLibrary(library);

        Debug.Log("[AI Training] Hoàn tất! Kết quả: Assets/Resources/AI_Library.json");
        AssetDatabase.Refresh();
    }

    // ──────────────────────────────────────────────────────────────────────
    // CARD DATABASE INIT
    // Tạo tạm một GameObject với CardDatabase, khởi tạo data từ Resources.
    // Hoạt động trong cả batchmode lẫn Editor thường.
    // ──────────────────────────────────────────────────────────────────────

    private static bool InitCardDatabase()
    {
        if (CardDatabase.Instance != null) return true;

        var go = new GameObject("CardDatabase_Training");
        var db = go.AddComponent<CardDatabase>();

        // Awake() không tự chạy trong edit mode → set thủ công
        if (CardDatabase.Instance == null)
        {
            CardDatabase.Instance = db;
            db.LoadDatabase();
        }

        if (CardDatabase.Instance == null || CardDatabase.Instance.GetAllCards().Count == 0)
        {
            Debug.LogError("[AI Training] Không load được CardDatabase — kiểm tra Assets/Resources/CardsData.json");
            Object.DestroyImmediate(go);
            return false;
        }

        int units  = CardDatabase.Instance.GetAllUnits().Count;
        int spells = CardDatabase.Instance.GetAllSpells().Count;
        Debug.Log($"[AI Training] CardDatabase: {units} units, {spells} spells");
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────
    // GA — chạy đồng bộ (không dùng coroutine)
    // ──────────────────────────────────────────────────────────────────────

    private static AILibrary RunGA(int popSize, int generations, int matchesPerChrom)
    {
        var population = Enumerable.Range(0, popSize).Select(_ => new Chromosome()).ToList();
        var library    = new AILibrary();
        var sim        = new GameSimulator();

        int easyAt   = Mathf.Max(1, generations / 4);   // 25%
        int mediumAt = Mathf.Max(2, generations / 2);   // 50%

        for (int g = 0; g < generations; g++)
        {
            // ── Đánh giá fitness ───────────────────────────────────────────
            for (int ci = 0; ci < popSize; ci++)
            {
                var chromo = population[ci];
                chromo.fitness = 0f;
                for (int m = 0; m < matchesPerChrom; m++)
                {
                    // Uniform opponent selection (không bias)
                    int oppIdx = Random.Range(0, popSize - 1);
                    if (oppIdx >= ci) oppIdx++;
                    int result = sim.SimulateMatch(new BotAgent(chromo), new BotAgent(population[oppIdx]));
                    if (result ==  1) chromo.fitness += 10f;
                    else if (result == 0) chromo.fitness +=  2f;
                }
            }

            // ── Sắp xếp ────────────────────────────────────────────────────
            population = population.OrderByDescending(c => c.fitness).ToList();

            float best  = population[0].fitness;
            float avg   = population.Average(c => c.fitness);
            float worst = population[population.Count - 1].fitness;
            Debug.Log($"Gen {g,3}/{generations}  Best={best,5:F0}  Avg={avg,4:F1}  Worst={worst,4:F0}");

            // ── Snapshots ──────────────────────────────────────────────────
            if (g == easyAt)   { library.easyBot   = population[0].Clone(); Debug.Log($"► Easy bot snapshot (Gen {g})"); }
            if (g == mediumAt) { library.mediumBot  = population[0].Clone(); Debug.Log($"► Medium bot snapshot (Gen {g})"); }

            // ── Elitism + Breeding ─────────────────────────────────────────
            population = BreedNextGen(population, popSize);
        }

        // Hard bot = tốt nhất của thế hệ cuối đã được đánh giá
        population = population.OrderByDescending(c => c.fitness).ToList();
        library.hardBot = population[0].Clone();
        Debug.Log($"► Hard bot saved (fitness={library.hardBot.fitness:F0})");

        return library;
    }

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
        var a     = TournamentSelect(pool);
        var b     = TournamentSelect(pool);
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

    // ──────────────────────────────────────────────────────────────────────
    // SAVE
    // ──────────────────────────────────────────────────────────────────────

    private static void SaveLibrary(AILibrary library)
    {
        string dir  = Path.Combine(Application.dataPath, "Resources");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "AI_Library.json");
        File.WriteAllText(path, JsonUtility.ToJson(library, true));
        Debug.Log($"[AI Training] Lưu: {path}");
    }
}
