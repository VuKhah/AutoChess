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

    private const int   PROD_POP       = 360;
    private const int   PROD_GEN       = 280;
    private const int   PROD_MATCHES   = 36;

    private const float MUTATION_RATE_EARLY = 0.10f;
    private const float MUTATION_RATE_LATE  = 0.06f;  // 0.035 → 0.06: giữ khả năng explore ở gen cuối
    private const float MUTATION_MAG_EARLY  = 0.12f;
    private const float MUTATION_MAG_LATE   = 0.06f;  // 0.035 → 0.06: bước nhảy lớn hơn khi bị stuck
    private const float IMMIGRANT_RATE_EARLY = 0.12f;
<<<<<<< HEAD
    private const float IMMIGRANT_RATE_LATE  = 0.07f;
=======
    private const float IMMIGRANT_RATE_LATE  = 0.08f; // 0.04 → 0.08: nhiều máu mới hơn ở giai đoạn cuối
>>>>>>> fe8dc8718f501ce5200bedd8c768b0ae1769ed05
    private const float MIN_LIBRARY_DISTANCE = 0.18f;

    [MenuItem("Tools/AI/Train AI — Quick (30 pop × 40 gen)")]
    public static void RunQuickFromMenu() => RunQuick();

    [MenuItem("Tools/AI/Train AI - Production (360 pop x 280 gen)")]
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
        csv.WriteLine("gen,best,avg,worst,std_dev,pct_babylon,pct_niles,pct_other,best_babylon,best_niles,best_summoner,best_resilient,raw_best,avg_ema,best_gain,best_late,avg_late,best_card,avg_card");
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
                case 2: // Summoner/Niles chain
                    c.genes[20] = Random.Range(0.75f, 1.0f);
                    c.genes[14] = Random.Range(0.75f, 1.0f);
                    c.genes[34] = Random.Range(0.75f, 1.0f);
                    c.genes[13] = Random.Range(0.65f, 0.95f);
                    c.genes[15] = Random.Range(0.55f, 0.85f);
                    c.genes[2]  = Random.Range(0.60f, 0.90f);
                    c.genes[8]  = Random.Range(0.45f, 0.75f);
                    c.genes[27] = Random.Range(0.00f, 0.15f);
                    c.genes[21] = Random.Range(0.00f, 0.25f);
                    c.genes[9]  = Random.Range(0.00f, 0.20f);
                    c.genes[5]  = Random.Range(0.25f, 0.55f);
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

        var benchmarkOpponents = CreateBenchmarkOpponents();

        // ── GA loop ───────────────────────────────────────────────────────────
<<<<<<< HEAD
        const int   PLATEAU_PATIENCE = 40;   // hardcore: only stop when best/avg/late/card all stall for a long tail
        const float PLATEAU_EPS      = 120f;
        int minStopGen = Mathf.RoundToInt(generations * 0.90f);
=======
        const int   PLATEAU_PATIENCE = 30;   // gen liên tiếp best không tăng ≥ EPS → dừng
        const float PLATEAU_EPS      = 30f;  // 100 → 30: ngay cả cải thiện nhỏ cũng reset counter
>>>>>>> fe8dc8718f501ce5200bedd8c768b0ae1769ed05
        int   plateauCount = 0;
        float prevProgressScore = float.MinValue;
        Chromosome hallOfFame = null;
        var hardCandidates = new List<Chromosome>();
        float bestEver = float.MinValue;
        float bestBabylonEver = 0f;
        float bestNileEver = 0f;
        float bestSummonerEver = float.MinValue;
        float bestResilientEver = float.MinValue;
        float avgEma = 0f;
        bool hasAvgEma = false;

        for (int g = 0; g < generations; g++)
        {
            if (g > 0 && g % 30 == 0)
                benchmarkOpponents = CreateBenchmarkOpponents();

            Debug.Log($"[AITraining] >> Gen {g}/{generations} bắt đầu đánh giá fitness...");
            var lateScores = new Dictionary<Chromosome, float>();
            var cardScores = new Dictionary<Chromosome, float>();

            // Đánh giá fitness
            for (int ci = 0; ci < popSize; ci++)
            {
                var chromo = population[ci];
                chromo.fitness = 0f;
                lateScores[chromo] = 0f;
                cardScores[chromo] = 0f;
                for (int m = 0; m < matchesPerChrom; m++)
                {
                    int oppIdx = Random.Range(0, popSize - 1);
                    if (oppIdx >= ci) oppIdx++;
                    MatchResult result = sim.EvaluateMatch(new BotAgent(chromo), new BotAgent(population[oppIdx]));
                    chromo.fitness += result.scoreA;
                    lateScores[chromo] += result.lateScoreA;
                    cardScores[chromo] += result.cardScoreA;
                }

                foreach (var benchmark in benchmarkOpponents)
                {
                    MatchResult result = sim.EvaluateMatch(new BotAgent(chromo), new BotAgent(benchmark));
                    chromo.fitness += result.scoreA * 0.5f;
                    lateScores[chromo] += result.lateScoreA * 0.5f;
                    cardScores[chromo] += result.cardScoreA * 0.5f;
                }

                if ((ci + 1) % 10 == 0 || ci == popSize - 1)
                    Debug.Log($"[AITraining] Gen {g}/{generations} fitness {ci + 1}/{popSize}");
            }

            population = population.OrderByDescending(c => c.fitness).ToList();

            float best   = population[0].fitness;
            float avg    = population.Average(c => c.fitness);
            float worst  = population[population.Count - 1].fitness;
            float stdDev = Mathf.Sqrt(population.Average(c => (c.fitness - avg) * (c.fitness - avg)));
            float bestLate = lateScores.TryGetValue(population[0], out float late) ? late : 0f;
            float avgLate = population.Average(c => lateScores.TryGetValue(c, out float v) ? v : 0f);
            float bestCard = cardScores.TryGetValue(population[0], out float card) ? card : 0f;
            float avgCard = population.Average(c => cardScores.TryGetValue(c, out float v) ? v : 0f);
            if (hallOfFame == null || best > bestEver)
            {
                hallOfFame = population[0].Clone();
                bestEver = best;
                AddHardCandidate(hardCandidates, hallOfFame, 24);
            }
            AddHardCandidates(hardCandidates, population.Take(4), 24);
            avgEma = hasAvgEma ? Mathf.Lerp(avgEma, avg, 0.25f) : avg;
            hasAvgEma = true;

            int cntB  = population.Count(c => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20]);
            int cntN  = population.Count(c => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18]);
            float pctB = cntB * 100f / popSize;
            float pctN = cntN * 100f / popSize;
            float pctO = (popSize - cntB - cntN) * 100f / popSize;
            float bestB = BestOrZero(population.Where(IsBabylon));
            float bestN = BestOrZero(population.Where(IsNile));
            float bestS = population.Max(SummonerScore);
            float bestR = population.Max(ResilientScore);
            bestBabylonEver = Mathf.Max(bestBabylonEver, bestB);
            bestNileEver = Mathf.Max(bestNileEver, bestN);
            bestSummonerEver = Mathf.Max(bestSummonerEver, bestS);
            bestResilientEver = Mathf.Max(bestResilientEver, bestR);

            csv.WriteLine($"{g},{bestEver:F0},{avg:F1},{worst:F0},{stdDev:F2},{pctB:F1},{pctN:F1},{pctO:F1},{bestBabylonEver:F0},{bestNileEver:F0},{bestSummonerEver:F2},{bestResilientEver:F2},{best:F0},{avgEma:F1},{bestEver - best:F0},{bestLate:F0},{avgLate:F0},{bestCard:F0},{avgCard:F0}");
            Debug.Log($"[AITraining] Gen {g}/{generations}  Best={bestEver:F0}  Raw={best:F0}  Avg={avg:F1}  Late={avgLate:F0}  Card={avgCard:F0}  Worst={worst:F0}  Std={stdDev:F1}  B={pctB:F0}% N={pctN:F0}% O={pctO:F0}%  BestB={bestB:F0} BestN={bestN:F0}");

            if (pctB < 10f || pctN < 10f)
                Debug.LogWarning($"[AITraining] Diversity low at gen {g}: B={pctB:F0}% N={pctN:F0}%. Injecting seeded immigrants next gen.");

            // ── Early stopping — composite progress plateau ─────────────────
            float progressScore = bestEver + avgEma * 0.45f + avgLate * 0.08f + avgCard * 0.035f;
            if (progressScore - prevProgressScore < PLATEAU_EPS)
                plateauCount++;
            else
                plateauCount = 0;
            prevProgressScore = progressScore;

            if (g >= minStopGen && plateauCount >= PLATEAU_PATIENCE)
            {
                Debug.LogWarning($"[AITraining] Early stop tại gen {g} — composite progress plateau {PLATEAU_PATIENCE} gen (Δ<{PLATEAU_EPS:F0}).");
                break;
            }

            if (g == generations - 1)
                break;

            population = BreedNextGen(population, popSize, g, generations, pctB, pctN, pctO);
        }

        // ── Selection cuối ────────────────────────────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();
        float avgFinal  = population.Average(c => c.fitness);
        float threshold = avgFinal * 0.8f;
        var   viable    = population.Where(c => c.fitness >= threshold).ToList();

        var selected = new List<Chromosome>();
        AddHardCandidates(hardCandidates, population.Take(Mathf.Min(20, population.Count)), 32);
        var hard = SelectHardByGauntlet(hardCandidates.Count > 0 ? hardCandidates : population.Take(12), sim) ?? hallOfFame ?? population[0];
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
        c.genes[20] * 2.0f
        + c.genes[14] * 2.5f
        + c.genes[34] * 2.0f
        + c.genes[13] * 1.5f
        + c.genes[15] * 1.0f
        + c.genes[2]  * 0.8f
        + c.genes[8]  * 0.8f
        - c.genes[9]  * 0.8f
        - c.genes[0]  * 0.5f
        - c.genes[27] * 1.2f
        - c.genes[21] * 0.8f;

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

    private static void AddHardCandidates(List<Chromosome> target, IEnumerable<Chromosome> candidates, int maxCount)
    {
        foreach (var candidate in candidates)
            AddHardCandidate(target, candidate, maxCount);
    }

    private static void AddHardCandidate(List<Chromosome> target, Chromosome candidate, int maxCount)
    {
        if (candidate == null) return;
        if (target.Any(c => GeneDistance(c, candidate) < 0.025f)) return;

        target.Add(candidate.Clone());
        target.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        while (target.Count > maxCount)
            target.RemoveAt(target.Count - 1);
    }

    private static Chromosome SelectHardByGauntlet(IEnumerable<Chromosome> candidates, GameSimulator sim)
    {
        var pool = candidates
            .Where(c => c != null)
            .OrderByDescending(c => c.fitness)
            .Take(16)
            .Select(c => c.Clone())
            .ToList();

        if (pool.Count == 0) return null;

        var benchmarks = CreateBenchmarkOpponents();
        for (int i = 0; i < 3; i++)
            benchmarks.AddRange(CreateBenchmarkOpponents());

        Chromosome best = null;
        float bestScore = float.MinValue;
        foreach (var candidate in pool)
        {
            float score = 0f;
            foreach (var benchmark in benchmarks)
            {
                MatchResult result = sim.EvaluateMatch(new BotAgent(candidate), new BotAgent(benchmark));
                score += result.scoreA + result.lateScoreA * 0.08f + result.cardScoreA * 0.035f;
            }

            candidate.fitness = score;
            if (best == null || score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }

        Debug.Log($"[AITraining] Final hard gauntlet candidates={pool.Count} benchmarks={benchmarks.Count} winner={bestScore:F0}");
        return best;
    }

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

    private static List<Chromosome> BreedNextGen(List<Chromosome> population, int popSize, int generation, int generations, float pctB, float pctN, float pctO)
    {
        float progress = TrainingProgress(generation, generations);
        float mutationRate = CurrentMutationRate(progress);
        float mutationMag = CurrentMutationMag(progress);
        int tournamentK = CurrentTournamentSize(progress);

        int eliteCount = Mathf.Max(3, Mathf.RoundToInt(Mathf.Lerp(popSize / 18f, popSize / 8f, progress)));
        int immigrantCount = Mathf.Max(2, Mathf.RoundToInt(popSize * CurrentImmigrantRate(progress)));
        if (pctB < 12f) immigrantCount += 2;
        if (pctN < 12f) immigrantCount += 2;
        if (pctO < 8f) immigrantCount += 3;

        var nextGen = new List<Chromosome>();
        AddTopClones(nextGen, population, c => true, eliteCount);
        AddTopClones(nextGen, population, IsBabylon, 2);
        AddTopClones(nextGen, population, IsNile, 2);
        AddTopClones(nextGen, population.OrderByDescending(SummonerScore), c => true, 2);
        AddTopClones(nextGen, population.OrderByDescending(ResilientScore), c => true, 2);

        int breedTarget = Mathf.Max(nextGen.Count, popSize - immigrantCount);
        int refineTarget = progress < 0.35f ? 0 : Mathf.RoundToInt(popSize * Mathf.Lerp(0.05f, 0.20f, progress));
        foreach (var elite in population.Take(eliteCount))
        {
            if (refineTarget-- <= 0 || nextGen.Count >= breedTarget) break;
            nextGen.Add(MutateClone(elite, mutationRate * 0.65f, mutationMag * 0.45f));
        }

        while (nextGen.Count < breedTarget)
            nextGen.Add(CrossoverAndMutate(population, tournamentK, mutationRate, mutationMag));

        while (nextGen.Count < popSize)
        {
            // Nếu pct_other sụp < 8%: ép immigration sang summoner/resilient
            // thay vì random → ngăn Babylon colonize toàn bộ quần thể
            int group = (pctO < 8f) ? (2 + (nextGen.Count % 2)) : Random.Range(0, 5);
            nextGen.Add(CreateSeededChromosome(group));
        }

        return nextGen;
    }

    private static void AddTopClones(List<Chromosome> target, IEnumerable<Chromosome> source, System.Func<Chromosome, bool> predicate, int count)
    {
        foreach (var c in source.Where(predicate).Take(count))
            target.Add(c.Clone());
    }

    private static Chromosome CrossoverAndMutate(List<Chromosome> pool, int tournamentK, float mutationRate, float mutationMag)
    {
        var a = TournamentSelect(pool, tournamentK);
        var b = TournamentSelect(pool, tournamentK);
        var child = new Chromosome();

        // 2-point crossover: bảo toàn co-adapted gene complexes (epistasis)
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

    private static Chromosome MutateClone(Chromosome parent, float mutationRate, float mutationMag)
    {
        Chromosome child = parent.Clone();
        for (int i = 0; i < Chromosome.GeneCount; i++)
        {
            if (Random.value < mutationRate)
            {
                float u1 = Mathf.Max(1e-6f, Random.value);
                float z = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * Random.value);
                child.genes[i] = Mathf.Clamp01(child.genes[i] + z * mutationMag);
            }
        }
        return child;
    }

    private static Chromosome TournamentSelect(List<Chromosome> pool, int tournamentK)
    {
        Chromosome best = null;
        for (int i = 0; i < tournamentK; i++)
        {
            var c = pool[Random.Range(0, pool.Count)];
            if (best == null || c.fitness > best.fitness) best = c;
        }
        return best;
    }

    private static float TrainingProgress(int generation, int generations)
    {
        return generations <= 1 ? 1f : Mathf.Clamp01(generation / (float)(generations - 1));
    }

    private static float CurrentMutationRate(float progress)
    {
        return Mathf.Lerp(MUTATION_RATE_EARLY, MUTATION_RATE_LATE, Mathf.SmoothStep(0f, 1f, progress));
    }

    private static float CurrentMutationMag(float progress)
    {
        return Mathf.Lerp(MUTATION_MAG_EARLY, MUTATION_MAG_LATE, Mathf.SmoothStep(0f, 1f, progress));
    }

    private static float CurrentImmigrantRate(float progress)
    {
        return Mathf.Lerp(IMMIGRANT_RATE_EARLY, IMMIGRANT_RATE_LATE, Mathf.SmoothStep(0f, 1f, progress));
    }

    private static int CurrentTournamentSize(float progress)
    {
        if (progress < 0.35f) return 3;
        if (progress < 0.75f) return 4;
        return 5;
    }

    private static List<Chromosome> CreateBenchmarkOpponents()
    {
        var benchmarks = new List<Chromosome>();
        for (int group = 0; group < 5; group++)
        {
            benchmarks.Add(CreateSeededChromosome(group));
            benchmarks.Add(CreateSeededChromosome(group));
        }
        return benchmarks;
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
                c.genes[20] = Random.Range(0.75f, 1.0f);
                c.genes[14] = Random.Range(0.75f, 1.0f);
                c.genes[34] = Random.Range(0.75f, 1.0f);
                c.genes[13] = Random.Range(0.65f, 0.95f);
                c.genes[15] = Random.Range(0.55f, 0.85f);
                c.genes[2]  = Random.Range(0.60f, 0.90f);
                c.genes[8]  = Random.Range(0.45f, 0.75f);
                c.genes[27] = Random.Range(0.00f, 0.15f);
                c.genes[21] = Random.Range(0.00f, 0.25f);
                c.genes[9]  = Random.Range(0.00f, 0.20f);
                c.genes[5]  = Random.Range(0.25f, 0.55f);
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
