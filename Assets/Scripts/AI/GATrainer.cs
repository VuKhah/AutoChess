using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GATrainer : MonoBehaviour
{
    [Header("Cấu hình huấn luyện")]
    public int populationSize  = 30;   // test nhanh: 30 | production: 120
    public int generations     = 40;   // test nhanh: 40 | production: 180
    public int matchesPerChrom = 5;    // test nhanh: 5  | production: 20
    [Range(0.05f, 0.25f)]
    public float mutationRate  = 0.10f;
    [Range(0.05f, 0.2f)]
    public float mutationMag   = 0.12f;
    [Range(0.05f, 0.25f)]
    public float immigrantRate = 0.12f;
    public float minLibraryDistance = 0.18f;

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
                case 2: // Summoner seed — summon/reborn/consume chain, giữ shells
                    c.genes[14] = Random.Range(0.75f, 1.0f); // eSummon (covers SummonConsumed, isConsume)
                    c.genes[5]  = Random.Range(0.70f, 1.0f); // wReborn — vòng lặp tái sinh
                    c.genes[8]  = Random.Range(0.65f, 1.0f); // tOnDeath — kích khi unit chết
                    c.genes[34] = Random.Range(0.65f, 1.0f); // tOnAllyGroup — OnAllyDeath/Summon/Reborn
                    c.genes[35] = Random.Range(0.50f, 0.85f); // tOnAllyDeploy — khi shell triệu hồi
                    c.genes[27] = Random.Range(0.00f, 0.15f); // wProactiveSell thấp — giữ shells
                    c.genes[0]  = Random.Range(0.10f, 0.40f); // wATK thấp
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
        csv.WriteLine("gen,best,avg,worst,std_dev,pct_babylon,pct_niles,pct_other,best_babylon,best_niles,best_summoner,best_resilient,raw_best,avg_ema,best_gain,best_late,avg_late");

        Debug.Log($"=== HUẤN LUYỆN AI === Genes:{Chromosome.GeneCount} | Pop:{populationSize} | Gen:{generations} ===");
        Debug.Log($"[GATrainer] CSV → {csvPath}");

        const int   PLATEAU_PATIENCE = 25;   // gen liên tiếp best không tăng ≥ EPS → dừng
        const float PLATEAU_EPS      = 100f; // cải thiện < 100 điểm không tính là "đổi"
        int   plateauCount = 0;
        float prevBestEver = float.MinValue;
        Chromosome hallOfFame = null;
        float bestEver = float.MinValue;
        float bestBabylonEver = 0f;
        float bestNileEver = 0f;
        float bestSummonerEver = float.MinValue;
        float bestResilientEver = float.MinValue;
        float avgEma = 0f;
        bool hasAvgEma = false;
        var benchmarkOpponents = CreateBenchmarkOpponents();

        for (int g = 0; g < generations; g++)
        {
            if (g > 0 && g % 30 == 0)
                benchmarkOpponents = CreateBenchmarkOpponents();

            var lateScores = new Dictionary<Chromosome, float>();

            // ── Đánh giá Fitness ─────────────────────────────────────────────
            foreach (var chromo in population)
            {
                chromo.fitness = 0f;
                lateScores[chromo] = 0f;
                for (int m = 0; m < matchesPerChrom; m++)
                {
                    BotAgent me      = new BotAgent(chromo);
                    int selfIdx      = population.IndexOf(chromo);
                    int oppIdx       = Random.Range(0, population.Count - 1);
                    if (oppIdx >= selfIdx) oppIdx++;
                    BotAgent opp     = new BotAgent(population[oppIdx]);
                    MatchResult result = sim.EvaluateMatch(me, opp);
                    chromo.fitness += result.scoreA;
                    lateScores[chromo] += result.lateScoreA;
                }

                foreach (var benchmark in benchmarkOpponents)
                {
                    MatchResult result = sim.EvaluateMatch(new BotAgent(chromo), new BotAgent(benchmark));
                    chromo.fitness += result.scoreA * 0.5f;
                    lateScores[chromo] += result.lateScoreA * 0.5f;
                }
            }

            // ── Sắp xếp ─────────────────────────────────────────────────────
            population = population.OrderByDescending(c => c.fitness).ToList();

            float best  = population[0].fitness;
            float avg   = population.Average(c => c.fitness);
            float worst = population[population.Count - 1].fitness;
            float stdDev = Mathf.Sqrt(population.Average(c => (c.fitness - avg) * (c.fitness - avg)));
            float bestLate = lateScores.TryGetValue(population[0], out float late) ? late : 0f;
            float avgLate = population.Average(c => lateScores.TryGetValue(c, out float v) ? v : 0f);
            if (hallOfFame == null || best > bestEver)
            {
                hallOfFame = population[0].Clone();
                bestEver = best;
            }
            avgEma = hasAvgEma ? Mathf.Lerp(avgEma, avg, 0.25f) : avg;
            hasAvgEma = true;

            // ── Tribe distribution ───────────────────────────────────────────
            int cntB = population.Count(c => c.genes[18] > c.genes[19] && c.genes[18] > c.genes[20]);
            int cntN = population.Count(c => c.genes[20] > c.genes[19] && c.genes[20] > c.genes[18]);
            int cntO = populationSize - cntB - cntN;
            float pctB = cntB * 100f / populationSize;
            float pctN = cntN * 100f / populationSize;
            float pctO = cntO * 100f / populationSize;
            float bestB = BestOrZero(population.Where(IsBabylon));
            float bestN = BestOrZero(population.Where(IsNile));
            float bestS = population.Max(SummonerScore);
            float bestR = population.Max(ResilientScore);
            bestBabylonEver = Mathf.Max(bestBabylonEver, bestB);
            bestNileEver = Mathf.Max(bestNileEver, bestN);
            bestSummonerEver = Mathf.Max(bestSummonerEver, bestS);
            bestResilientEver = Mathf.Max(bestResilientEver, bestR);

            // ── Ghi CSV ──────────────────────────────────────────────────────
            csv.WriteLine($"{g},{bestEver:F0},{avg:F1},{worst:F0},{stdDev:F2},{pctB:F1},{pctN:F1},{pctO:F1},{bestBabylonEver:F0},{bestNileEver:F0},{bestSummonerEver:F2},{bestResilientEver:F2},{best:F0},{avgEma:F1},{bestEver - best:F0},{bestLate:F0},{avgLate:F0}");
            Debug.Log($"Gen {g,3}/{generations}  Best={bestEver,5:F0}  Raw={best,5:F0}  Avg={avg,5:F1}  Late={avgLate:F0}  Worst={worst,5:F0}  " +
                      $"Std={stdDev:F1}  B={pctB:F0}% N={pctN:F0}% O={pctO:F0}%  BestB={bestB:F0} BestN={bestN:F0}");

            if (pctB < 10f || pctN < 10f)
                Debug.LogWarning($"[GATrainer] Diversity low at gen {g}: B={pctB:F0}% N={pctN:F0}%. Injecting seeded immigrants next gen.");

            // ── Early stopping — best fitness plateau ────────────────────────
            if (bestEver - prevBestEver < PLATEAU_EPS)
                plateauCount++;
            else
                plateauCount = 0;
            prevBestEver = bestEver;

            if (plateauCount >= PLATEAU_PATIENCE)
            {
                Debug.LogWarning($"[GATrainer] Early stop tại gen {g} — best fitness plateau {PLATEAU_PATIENCE} gen (Δ<{PLATEAU_EPS:F0}).");
                break;
            }

            // ── Elitism + breed ───────────────────────────────────────────────
            if (g == generations - 1)
                break;

            population = BreedNextGen(g, pctB, pctN, pctO);
            yield return null;
        }

        // ── Sort lần cuối ─────────────────────────────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();
        float avgFinal    = population.Average(c => c.fitness);
        float threshold   = avgFinal * 0.8f; // ngưỡng "đủ tốt" để chọn specialist
        var   viable      = population.Where(c => c.fitness >= threshold).ToList();

        // ── Chọn 5 bot ───────────────────────────────────────────────────────
        var selected = new List<Chromosome>();
        var hard = hallOfFame ?? population[0];
        selected.Add(hard);

        var babylon = SelectDistinct(population.Where(IsBabylon), selected, c => c.fitness);
        if (babylon != null) selected.Add(babylon);

        var nile = SelectDistinct(population.Where(IsNile), selected, c => c.fitness);
        if (nile != null) selected.Add(nile);

        var summoner = SelectDistinct(viable.Count > 0 ? viable : population, selected, SummonerScore);
        if (summoner != null) selected.Add(summoner);

        var resilient = SelectDistinct(viable.Count > 0 ? viable : population, selected, ResilientScore);

        library.hardBot = hard.Clone();
        library.babylonBot = babylon?.Clone();
        library.nileBot = nile?.Clone();
        library.summonerBot = summoner?.Clone();
        library.resilientBot = resilient?.Clone();

        // ── Log kết quả ──────────────────────────────────────────────────────
        LogBot("Hard",     library.hardBot,     "rank=1");
        LogBot("Babylon",  library.babylonBot,  library.babylonBot  != null ? $"sBabylon={library.babylonBot.genes[18]:F3}"   : "");
        LogBot("Nile",     library.nileBot,      library.nileBot     != null ? $"sNiles={library.nileBot.genes[20]:F3}"        : "");
        LogBot("Summoner", library.summonerBot, library.summonerBot != null ? $"score={SummonerScore(library.summonerBot):F2}" : "");
        LogBot("Resilient",library.resilientBot,library.resilientBot!= null ? $"score={ResilientScore(library.resilientBot):F2}" : "");

        csv.Close();
        SaveLibrary();
        Debug.Log($"=== HUẤN LUYỆN HOÀN TẤT === CSV: {csvPath}");
    }

    // ── Scoring functions cho archetype selection ─────────────────────────────
    private static float SummonerScore(Chromosome c)
    {
        if (c == null) return float.MinValue;
        return c.genes[20] * 2.0f   // sNiles — Bastet/Sekhmet/Osiris đều là Niles
             + c.genes[14] * 2.5f   // eSummon — summon units trong combat
             + c.genes[34] * 2.0f   // tOnAllyGroup — OnAllySummon kích Bastet mỗi 2 lần
             + c.genes[13] * 1.5f   // eAddStats — Bastet buff +ATK/+HP cho toàn bộ Niles
             + c.genes[15] * 1.0f   // eDealDmg — damage effects trong chain
             + c.genes[2]  * 0.8f   // wTierBonus — cần tier 5 Bastet, tier 6 Sekhmet/Osiris
             + c.genes[8]  * 0.8f   // tOnDeath — death chain triggers
             - c.genes[9]  * 0.8f   // phạt OnAttack — không phải combat bot
             - c.genes[0]  * 0.5f   // phạt wATK raw
             - c.genes[27] * 1.2f   // phạt bán chủ động — giữ số lượng unit
             - c.genes[21] * 0.8f;  // phạt wMerge — số lượng > chất lượng
    }

    private static float ResilientScore(Chromosome c)
    {
        if (c == null) return float.MinValue;
        return c.genes[1] * 1.4f
             + c.genes[4]
             + c.genes[5] * 1.5f
             + c.genes[6]
             + c.genes[10]
             + c.genes[11] * 0.5f
             - c.genes[0] * 0.4f
             - c.genes[9] * 0.5f
             - c.genes[24] * 0.3f;
    }

    private void LogBot(string name, Chromosome bot, string extra)
    {
        if (bot != null)
            Debug.Log($"► <b>{name} Bot</b>  fitness={bot.fitness:F0}  {extra}");
        else
            Debug.LogWarning($"[GATrainer] {name} specialist không tìm thấy trong population cuối.");
    }

    // ── GA helpers ────────────────────────────────────────────────────────────
    private Chromosome SelectDistinct(IEnumerable<Chromosome> candidates, List<Chromosome> selected, System.Func<Chromosome, float> score)
    {
        var list = candidates.ToList();
        if (list.Count == 0) return null;

        var distinct = list
            .Where(c => selected.All(s => GeneDistance(c, s) >= minLibraryDistance))
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

    private List<Chromosome> BreedNextGen(int generation, float pctB, float pctN, float pctO)
    {
        float progress = TrainingProgress(generation, generations);
        float currentMutationRate = CurrentMutationRate(progress);
        float currentMutationMag = CurrentMutationMag(progress);
        int tournamentK = CurrentTournamentSize(progress);

        int eliteCount = Mathf.Max(3, Mathf.RoundToInt(Mathf.Lerp(populationSize / 18f, populationSize / 8f, progress)));
        int immigrantCount = Mathf.Max(2, Mathf.RoundToInt(populationSize * CurrentImmigrantRate(progress)));
        if (pctB < 12f) immigrantCount += 2;
        if (pctN < 12f) immigrantCount += 2;
        if (pctO < 8f) immigrantCount += 3;

        var nextGen = new List<Chromosome>();
        AddTopClones(nextGen, population, c => true, eliteCount);
        AddTopClones(nextGen, population, IsBabylon, 2);
        AddTopClones(nextGen, population, IsNile, 2);
        AddTopClones(nextGen, population.OrderByDescending(SummonerScore), c => true, 2);
        AddTopClones(nextGen, population.OrderByDescending(ResilientScore), c => true, 2);

        int breedTarget = Mathf.Max(nextGen.Count, populationSize - immigrantCount);
        int refineTarget = progress < 0.35f ? 0 : Mathf.RoundToInt(populationSize * Mathf.Lerp(0.05f, 0.20f, progress));
        foreach (var elite in population.Take(eliteCount))
        {
            if (refineTarget-- <= 0 || nextGen.Count >= breedTarget) break;
            nextGen.Add(MutateClone(elite, currentMutationRate * 0.65f, currentMutationMag * 0.45f));
        }

        while (nextGen.Count < breedTarget)
        {
            nextGen.Add(CrossoverAndMutate(
                TournamentSelect(population, tournamentK),
                TournamentSelect(population, tournamentK),
                currentMutationRate,
                currentMutationMag));
        }

        while (nextGen.Count < populationSize)
        {
            // Nếu pct_other sụp xuống < 8%: ép immigration sang summoner/resilient
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

    private Chromosome CrossoverAndMutate(Chromosome a, Chromosome b, float currentMutationRate, float currentMutationMag)
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
            if (Random.value < currentMutationRate)
            {
                float u1 = Mathf.Max(1e-6f, Random.value);
                float z  = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * Random.value);
                child.genes[i] += z * currentMutationMag;
            }
            child.genes[i] = Mathf.Clamp01(child.genes[i]);
        }
        return child;
    }

    private static Chromosome MutateClone(Chromosome parent, float currentMutationRate, float currentMutationMag)
    {
        Chromosome child = parent.Clone();
        for (int i = 0; i < Chromosome.GeneCount; i++)
        {
            if (Random.value < currentMutationRate)
            {
                float u1 = Mathf.Max(1e-6f, Random.value);
                float z = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * Random.value);
                child.genes[i] = Mathf.Clamp01(child.genes[i] + z * currentMutationMag);
            }
        }
        return child;
    }

    private float TrainingProgress(int generation, int totalGenerations)
    {
        return totalGenerations <= 1 ? 1f : Mathf.Clamp01(generation / (float)(totalGenerations - 1));
    }

    private float CurrentMutationRate(float progress)
    {
        return Mathf.Lerp(mutationRate, 0.035f, Mathf.SmoothStep(0f, 1f, progress));
    }

    private float CurrentMutationMag(float progress)
    {
        return Mathf.Lerp(mutationMag, 0.035f, Mathf.SmoothStep(0f, 1f, progress));
    }

    private float CurrentImmigrantRate(float progress)
    {
        return Mathf.Lerp(immigrantRate, 0.04f, Mathf.SmoothStep(0f, 1f, progress));
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
            case 2: // summonerBot — Niles summon chain, kích Bastet escalating buff
                c.genes[20] = Random.Range(0.75f, 1.0f);  // sNiles — tribe identity
                c.genes[14] = Random.Range(0.75f, 1.0f);  // eSummon — value summon effects
                c.genes[34] = Random.Range(0.75f, 1.0f);  // tOnAllyGroup — OnAllySummon kích Bastet
                c.genes[13] = Random.Range(0.65f, 0.95f); // eAddStats — Bastet buff là AddStats
                c.genes[15] = Random.Range(0.55f, 0.85f); // eDealDmg — damage trong chain
                c.genes[2]  = Random.Range(0.60f, 0.90f); // wTierBonus — Bastet tier 5, Sekhmet tier 6
                c.genes[8]  = Random.Range(0.45f, 0.75f); // tOnDeath — death chain (hỗ trợ)
                c.genes[27] = Random.Range(0.00f, 0.15f); // wProactiveSell LOW — giữ số lượng unit
                c.genes[21] = Random.Range(0.00f, 0.25f); // wMerge LOW — số lượng > 3-star
                c.genes[9]  = Random.Range(0.00f, 0.20f); // tOnAttack LOW
                c.genes[5]  = Random.Range(0.25f, 0.55f); // wReborn MEDIUM — không phải identity
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
