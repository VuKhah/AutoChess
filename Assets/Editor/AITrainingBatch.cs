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

    private const int   PROD_POP       = 120;
    private const int   PROD_GEN       = 120;
    private const int   PROD_MATCHES   = 20;

    private const float MUTATION_RATE  = 0.08f;
    private const float MUTATION_MAG   = 0.12f;
    private const int   TOURNAMENT_K   = 3;
    private const float IMMIGRANT_RATE = 0.12f;
    private const float MIN_LIBRARY_DISTANCE = 0.18f;

    [MenuItem("Tools/AI/Train AI — Quick (30 pop × 40 gen)")]
    public static void RunQuickFromMenu() => RunQuick();

    [MenuItem("Tools/AI/Train AI — Production (120 pop × 120 gen)")]
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
        csv.WriteLine("gen,best,avg,worst,std_dev,pct_babylon,pct_niles,pct_other,best_babylon,best_niles,best_summoner,best_resilient");
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
                case 2: // Summoner — summon/reborn/consume chain, giữ shells
                    c.genes[14] = Random.Range(0.75f, 1.0f);
                    c.genes[5]  = Random.Range(0.70f, 1.0f);
                    c.genes[8]  = Random.Range(0.65f, 1.0f);
                    c.genes[34] = Random.Range(0.65f, 1.0f);
                    c.genes[35] = Random.Range(0.50f, 0.85f);
                    c.genes[27] = Random.Range(0.00f, 0.15f);
                    c.genes[0]  = Random.Range(0.10f, 0.40f);
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
        const int   PLATEAU_PATIENCE = 15;
        const float PLATEAU_EPS      = 0.5f;
        int   plateauCount = 0;
        float prevStdDev   = -1f;

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
                    MatchResult result = sim.EvaluateMatch(new BotAgent(chromo), new BotAgent(population[oppIdx]));
                    chromo.fitness += result.scoreA;
                }

                if ((ci + 1) % 10 == 0 || ci == popSize - 1)
                    Debug.Log($"[AITraining] Gen {g}/{generations} fitness {ci + 1}/{popSize}");
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
            float bestB = BestOrZero(population.Where(IsBabylon));
            float bestN = BestOrZero(population.Where(IsNile));
            float bestS = population.Max(SummonerScore);
            float bestR = population.Max(ResilientScore);

            csv.WriteLine($"{g},{best:F0},{avg:F1},{worst:F0},{stdDev:F2},{pctB:F1},{pctN:F1},{pctO:F1},{bestB:F0},{bestN:F0},{bestS:F2},{bestR:F2}");
            Debug.Log($"[AITraining] Gen {g}/{generations}  Best={best:F0}  Avg={avg:F1}  Worst={worst:F0}  Std={stdDev:F1}  B={pctB:F0}% N={pctN:F0}% O={pctO:F0}%  BestB={bestB:F0} BestN={bestN:F0}");

            if (pctB < 10f || pctN < 10f)
                Debug.LogWarning($"[AITraining] Diversity low at gen {g}: B={pctB:F0}% N={pctN:F0}%. Injecting seeded immigrants next gen.");

            // ── Early stopping — std_dev plateau ─────────────────────────────
            if (prevStdDev >= 0f && Mathf.Abs(stdDev - prevStdDev) < PLATEAU_EPS)
                plateauCount++;
            else
                plateauCount = 0;
            prevStdDev = stdDev;

            if (plateauCount >= PLATEAU_PATIENCE)
            {
                Debug.LogWarning($"[AITraining] Early stop tại gen {g} — std_dev plateau {PLATEAU_PATIENCE} gen liên tiếp (Δ<{PLATEAU_EPS}).");
                break;
            }

            population = BreedNextGen(population, popSize, pctB, pctN);
        }

        // ── Selection cuối ────────────────────────────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();
        float avgFinal  = population.Average(c => c.fitness);
        float threshold = avgFinal * 0.8f;
        var   viable    = population.Where(c => c.fitness >= threshold).ToList();

        var selected = new List<Chromosome>();
        var hard = population[0];
        selected.Add(hard);

        var babylon = SelectDistinct(population.Where(IsBabylon), selected, c => c.fitness);
        if (babylon != null) selected.Add(babylon);

        var nile = SelectDistinct(population.Where(IsNile), selected, c => c.fitness);
        if (nile != null) selected.Add(nile);

        var summoner = SelectDistinct(viable.Count > 0 ? viable : population, selected, SummonerScore);
        if (summoner != null) selected.Add(summoner);

        var resilient = SelectDistinct(viable.Count > 0 ? viable : population, selected, ResilientScore);

        var library = new AILibrary
        {
            hardBot = hard.Clone(),
            babylonBot = babylon?.Clone(),
            nileBot = nile?.Clone(),
            summonerBot = summoner?.Clone(),
            resilientBot = resilient?.Clone(),
        };

        // Log kết quả cuối
        Debug.Log($"[AITraining] Hard      fitness={library.hardBot.fitness:F0}  rank=1");
        LogResult("Babylon",  library.babylonBot,  library.babylonBot  != null ? $"sBabylon={library.babylonBot.genes[18]:F3}"   : "not found");
        LogResult("Nile",     library.nileBot,      library.nileBot     != null ? $"sNiles={library.nileBot.genes[20]:F3}"        : "not found");
        LogResult("Summoner", library.summonerBot, library.summonerBot != null ? $"score={SummonerScore(library.summonerBot):F2}" : "not found");
        LogResult("Resilient",library.resilientBot,library.resilientBot!= null ? $"score={ResilientScore(library.resilientBot):F2}" : "not found");

        csv.Close();
        return library;
    }

    // ──────────────────────────────────────────────────────────────────────────
    private static float SummonerScore(Chromosome c) =>
        c.genes[14] * 2.5f
        + c.genes[5]  * 2.0f
        + c.genes[8]  * 1.5f
        + c.genes[34] * 1.5f
        + c.genes[35] * 0.8f
        + c.genes[12] * 0.6f
        - c.genes[9]  * 0.8f
        - c.genes[0]  * 0.5f
        - c.genes[27] * 1.2f;

    private static float ResilientScore(Chromosome c) =>
        c.genes[1] * 1.4f
        + c.genes[4]
        + c.genes[5] * 1.5f
        + c.genes[6]
        + c.genes[10]
        + c.genes[11] * 0.5f
        - c.genes[0] * 0.4f
        - c.genes[9] * 0.5f
        - c.genes[24] * 0.3f;

    private static void LogResult(string name, Chromosome bot, string extra) =>
        Debug.Log(bot != null
            ? $"[AITraining] {name,-10} fitness={bot.fitness:F0}  {extra}"
            : $"[AITraining] {name,-10} NOT FOUND in population");

    // ──────────────────────────────────────────────────────────────────────────
    private static Chromosome SelectDistinct(IEnumerable<Chromosome> candidates, List<Chromosome> selected, System.Func<Chromosome, float> score)
    {
        var list = candidates.ToList();
        if (list.Count == 0) return null;

        var distinct = list
            .Where(c => selected.All(s => GeneDistance(c, s) >= MIN_LIBRARY_DISTANCE))
            .ToList();

        var pool = distinct.Count > 0 ? distinct : list;
        return pool
            .OrderByDescending(c => score(c) + DiversityBonus(c, selected) * 100f)
            .FirstOrDefault();
    }

    private static float DiversityBonus(Chromosome c, List<Chromosome> selected)
    {
        if (selected.Count == 0) return 0f;
        return selected.Min(s => GeneDistance(c, s));
    }

    private static float GeneDistance(Chromosome a, Chromosome b)
    {
        float sum = 0f;
        for (int i = 0; i < Chromosome.GeneCount; i++)
        {
            float d = a.genes[i] - b.genes[i];
            sum += d * d;
        }
        return Mathf.Sqrt(sum / Chromosome.GeneCount);
    }

    private static List<Chromosome> BreedNextGen(List<Chromosome> population, int popSize, float pctB, float pctN)
    {
        int eliteCount = Mathf.Max(2, popSize / 20);
        int immigrantCount = Mathf.Max(2, Mathf.RoundToInt(popSize * IMMIGRANT_RATE));
        if (pctB < 10f) immigrantCount += 2;
        if (pctN < 10f) immigrantCount += 2;

        var nextGen = new List<Chromosome>();
        AddTopClones(nextGen, population, c => true, eliteCount);
        AddTopClones(nextGen, population, IsBabylon, 2);
        AddTopClones(nextGen, population, IsNile, 2);
        AddTopClones(nextGen, population.OrderByDescending(SummonerScore), c => true, 2);
        AddTopClones(nextGen, population.OrderByDescending(ResilientScore), c => true, 2);

        int breedTarget = Mathf.Max(nextGen.Count, popSize - immigrantCount);
        while (nextGen.Count < breedTarget)
            nextGen.Add(CrossoverAndMutate(population));

        while (nextGen.Count < popSize)
            nextGen.Add(CreateSeededChromosome(Random.Range(0, 5)));

        return nextGen;
    }

    private static void AddTopClones(List<Chromosome> target, IEnumerable<Chromosome> source, System.Func<Chromosome, bool> predicate, int count)
    {
        foreach (var c in source.Where(predicate).Take(count))
            target.Add(c.Clone());
    }

    private static Chromosome CrossoverAndMutate(List<Chromosome> pool)
    {
        var a = TournamentSelect(pool);
        var b = TournamentSelect(pool);
        var child = new Chromosome();

        // 2-point crossover: bảo toàn co-adapted gene complexes (epistasis)
        int pt1 = Random.Range(0, Chromosome.GeneCount);
        int pt2 = Random.Range(0, Chromosome.GeneCount);
        if (pt1 > pt2) { int tmp = pt1; pt1 = pt2; pt2 = tmp; }

        for (int i = 0; i < Chromosome.GeneCount; i++)
        {
            child.genes[i] = (i >= pt1 && i < pt2) ? b.genes[i] : a.genes[i];
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

    private static Chromosome CreateSeededChromosome(int group)
    {
        var c = new Chromosome();
        switch (group)
        {
            case 0:
                c.genes[18] = Random.Range(0.7f, 1.0f);
                c.genes[19] = Random.Range(0.0f, 0.3f);
                c.genes[20] = Random.Range(0.0f, 0.3f);
                break;
            case 1:
                c.genes[20] = Random.Range(0.7f, 1.0f);
                c.genes[18] = Random.Range(0.0f, 0.3f);
                c.genes[19] = Random.Range(0.0f, 0.3f);
                break;
            case 2:
                c.genes[14] = Random.Range(0.75f, 1.0f);
                c.genes[5]  = Random.Range(0.70f, 1.0f);
                c.genes[8]  = Random.Range(0.65f, 1.0f);
                c.genes[34] = Random.Range(0.65f, 1.0f);
                c.genes[35] = Random.Range(0.50f, 0.85f);
                c.genes[27] = Random.Range(0.00f, 0.15f);
                c.genes[0]  = Random.Range(0.10f, 0.40f);
                break;
            case 3:
                c.genes[1]  = Random.Range(0.75f, 1.0f);
                c.genes[4]  = Random.Range(0.70f, 1.0f);
                c.genes[5]  = Random.Range(0.70f, 1.0f);
                c.genes[6]  = Random.Range(0.70f, 1.0f);
                c.genes[10] = Random.Range(0.70f, 1.0f);
                break;
        }
        return c;
    }

    private static bool IsBabylon(Chromosome c) => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20];
    private static bool IsNile(Chromosome c) => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18];
    private static float BestOrZero(IEnumerable<Chromosome> pool) => pool.Any() ? pool.Max(c => c.fitness) : 0f;

    private static void SaveLibrary(AILibrary library)
    {
        string dir  = Path.Combine(Application.dataPath, "Resources");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "AI_Library.json");
        File.WriteAllText(path, JsonUtility.ToJson(library, true));
        Debug.Log($"[AITraining] Đã lưu: {path}");
    }
}
