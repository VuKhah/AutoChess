using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class GATrainer : MonoBehaviour
{
    [Header("Cấu hình huấn luyện")]
    public int populationSize = 50;
    public int generations = 100;

    private List<Chromosome> population = new List<Chromosome>();
    private GameSimulator sim = new GameSimulator();
    private AILibrary library = new AILibrary();

    void Start()
    {
        // 1. Khởi tạo quần thể F0 với các gene ngẫu nhiên
        for (int i = 0; i < populationSize; i++)
        {
            population.Add(new Chromosome());
        }

        // Bắt đầu tiến trình huấn luyện
        StartCoroutine(TrainRoutine());
    }

    IEnumerator TrainRoutine()
    {
        Debug.Log("=== BẮT ĐẦU TIẾN TRÌNH HUẤN LUYỆN AI ===");

        for (int g = 0; g < generations; g++)
        {
            // 2. Đánh giá Fitness (Mỗi cá thể đấu với 10 đối thủ ngẫu nhiên trong quần thể)
            foreach (var chromo in population)
            {
                chromo.fitness = 0;
                for (int i = 0; i < 10; i++)
                {
                    BotAgent me = new BotAgent(chromo);
                    // Chọn đối thủ ngẫu nhiên từ quần thể hiện tại
                    BotAgent opponent = new BotAgent(population[Random.Range(0, populationSize)]);

                    int result = sim.SimulateMatch(me, opponent);

                    if (result == 1) chromo.fitness += 10;      // Thắng
                    else if (result == 0) chromo.fitness += 2;  // Hòa
                }
            }

            // 3. Sắp xếp quần thể theo Fitness giảm dần (Thằng giỏi nhất lên đầu)
            population = population.OrderByDescending(c => c.fitness).ToList();

            // 4. CHỤP SNAPSHOT (Lưu lại trí tuệ tại các mốc thời gian)
            if (g == 10)
            {
                library.easyBot = population[0].Clone();
                Debug.Log("<color=green>Đã lưu snapshot: Easy Bot (Gen 10)</color>");
            }
            if (g == 50)
            {
                library.mediumBot = population[0].Clone();
                Debug.Log("<color=yellow>Đã lưu snapshot: Medium Bot (Gen 50)</color>");
            }

            // 5. Chọn lọc & Tiến hóa (Elitism: Giữ lại 10% cá thể tốt nhất)
            List<Chromosome> nextGen = population.Take(populationSize / 10).ToList();

            // 6. Lai ghép & Đột biến để lấp đầy số lượng quần thể mới
            while (nextGen.Count < populationSize)
            {
                Chromosome parentA = nextGen[Random.Range(0, nextGen.Count)];
                Chromosome parentB = nextGen[Random.Range(0, nextGen.Count)];

                nextGen.Add(CrossoverAndMutate(parentA, parentB));
            }

            population = nextGen;
            Debug.Log($"Thế hệ {g}: Fitness cao nhất = {population[0].fitness}");

            // Tạm dừng một frame để Unity không bị treo máy khi xử lý hàng nghìn trận đấu
            yield return null;
        }

        // 7. Kết thúc: Lưu Hard Bot và xuất file JSON
        library.hardBot = population[0].Clone();
        Debug.Log("<color=red>Huấn luyện hoàn tất! Đã lưu snapshot: Hard Bot (Gen Final)</color>");

        SaveLibrary();
    }

    private Chromosome CrossoverAndMutate(Chromosome a, Chromosome b)
    {
        Chromosome child = new Chromosome();
        for (int i = 0; i < 8; i++)
        {
            // Lai ghép đồng nhất (Uniform Crossover)
            child.genes[i] = Random.value > 0.5f ? a.genes[i] : b.genes[i];

            // Đột biến Gaussian (5% cơ hội thay đổi giá trị gene)
            if (Random.value < 0.05f)
            {
                child.genes[i] += Random.Range(-0.1f, 0.1f);
            }

            // Đảm bảo gene luôn nằm trong khoảng [0, 1]
            child.genes[i] = Mathf.Clamp01(child.genes[i]);
        }
        return child;
    }

    void SaveLibrary()
    {
        // Đảm bảo thư mục Resources tồn tại
        string dirPath = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string json = JsonUtility.ToJson(library, true);
        string path = Path.Combine(dirPath, "AI_Library.json");

        File.WriteAllText(path, json);
        Debug.Log("<b>LƯU THÀNH CÔNG:</b> " + path);

        // Refresh AssetDatabase để Unity thấy file mới ngay lập tức (chỉ dùng trong Editor)
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}