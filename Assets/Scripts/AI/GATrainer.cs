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
        csv.WriteLine("gen,best,avg,worst,std_dev,pct_babylon,pct_niles,pct_other,best_babylon,best_niles,best_aggressor,best_resilient");

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
                    MatchResult result = sim.EvaluateMatch(me, opp);
                    chromo.fitness += result.scoreA;
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
            float bestB = BestOrZero(population.Where(IsBabylon));
            float bestN = BestOrZero(population.Where(IsNile));
            float bestS = population.Max(SummonerScore);
            float bestR = population.Max(ResilientScore);

            // ── Ghi CSV ──────────────────────────────────────────────────────
            csv.WriteLine($"{g},{best:F0},{avg:F1},{worst:F0},{stdDev:F2},{pctB:F1},{pctN:F1},{pctO:F1},{bestB:F0},{bestN:F0},{bestS:F2},{bestR:F2}");
            Debug.Log($"Gen {g,3}/{generations}  Best={best,5:F0}  Avg={avg,5:F1}  Worst={worst,5:F0}  " +
                      $"Std={stdDev:F1}  B={pctB:F0}% N={pctN:F0}% O={pctO:F0}%  BestB={bestB:F0} BestN={bestN:F0}");

            if (pctB < 10f || pctN < 10f)
                Debug.LogWarning($"[GATrainer] Diversity low at gen {g}: B={pctB:F0}% N={pctN:F0}%. Injecting seeded immigrants next gen.");

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
            population = BreedNextGen(pctB, pctN);
            yield return null;
        }

        // ── Sort lần cuối ─────────────────────────────────────────────────────
        population = population.OrderByDescending(c => c.fitness).ToList();
        float avgFinal    = population.Average(c => c.fitness);
        float threshold   = avgFinal * 0.8f; // ngưỡng "đủ tốt" để chọn specialist
        var   viable      = population.Where(c => c.fitness >= threshold).ToList();

        // ── Chọn 5 bot ───────────────────────────────────────────────────────
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
        return c.genes[14] * 2.5f   // eSummon — SummonConsumed ×1.2 trong EffectWeight
             + c.genes[5]  * 2.0f   // wReborn — vòng lặp tái sinh
             + c.genes[8]  * 1.5f   // tOnDeath — deathrattle / summon khi chết
             + c.genes[34] * 1.5f   // tOnAllyGroup — OnAllyDeath/Summon/Reborn chain
             + c.genes[35] * 0.8f   // tOnAllyDeploy — khi shell được triệu hồi lên sân
             + c.genes[12] * 0.6f   // tOnDeploy
             - c.genes[9]  * 0.8f   // phạt OnAttack — không phải playstyle này
             - c.genes[0]  * 0.5f   // phạt wATK raw
             - c.genes[27] * 1.2f;  // phạt bán chủ động — phải giữ shells
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

    private List<Chromosome> BreedNextGen(float pctB, float pctN)
    {
        int eliteCount = Mathf.Max(2, populationSize / 20);
        int immigrantCount = Mathf.Max(2, Mathf.RoundToInt(populationSize * immigrantRate));
        if (pctB < 10f) immigrantCount += 2;
        if (pctN < 10f) immigrantCount += 2;

        var nextGen = new List<Chromosome>();
        AddTopClones(nextGen, population, c => true, eliteCount);
        AddTopClones(nextGen, population, IsBabylon, 2);
        AddTopClones(nextGen, population, IsNile, 2);
        AddTopClones(nextGen, population.OrderByDescending(SummonerScore), c => true, 2);
        AddTopClones(nextGen, population.OrderByDescending(ResilientScore), c => true, 2);

        int breedTarget = Mathf.Max(nextGen.Count, populationSize - immigrantCount);
        while (nextGen.Count < breedTarget)
        {
            nextGen.Add(CrossoverAndMutate(
                TournamentSelect(population, 3),
                TournamentSelect(population, 3)));
        }

        while (nextGen.Count < populationSize)
            nextGen.Add(CreateSeededChromosome(Random.Range(0, 5)));

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
