# CHƯƠNG 2: CƠ SỞ LÝ THUYẾT

---

## 2.1 Thuật Toán Di Truyền (Genetic Algorithm)

### 2.1.1 Bài Toán Tối Ưu Hóa và Giới Hạn Của Các Phương Pháp Cổ Điển

Phần lớn các bài toán kỹ thuật thực tiễn đều có thể được đưa về dạng tổng quát: *tìm một cấu hình tham số x trong không gian tìm kiếm S sao cho hàm mục tiêu f(x) đạt giá trị tốt nhất*. Trong toán học, ta viết:

```
x* = argmax f(x),   x ∈ S ⊆ ℝⁿ
```

Khi hàm f có tính chất tốt — liên tục, khả vi, lồi — các phương pháp giải tích cổ điển như gradient descent hay Newton's method có thể tìm nghiệm tối ưu một cách nhanh chóng và đảm bảo. Thế nhưng phần lớn bài toán thực tế lại không có những tính chất đó. Không gian tham số có thể có hàng chục hay hàng trăm chiều, hàm mục tiêu có thể không liên tục, không có công thức giải tích, hoặc chứa vô số cực tiểu địa phương khiến mọi phương pháp dựa trên đạo hàm đều sa lầy.

Bài toán thiết kế AI cho game Auto Chess trong đề tài này là một ví dụ điển hình. Hàm mục tiêu ở đây là: *chất lượng chiến lược chơi game của một bot*, đo bằng tỉ lệ thắng khi đối đầu với các đối thủ khác. Không gian tham số là tập hợp tất cả bộ trọng số có thể ("ưu tiên ATK hay HP nhiều hơn?", "khi nào nên reroll?", "nên xây đội theo bộ tộc nào?") — một không gian liên tục, đa chiều, và quan trọng hơn: không có công thức toán học nào để tính đạo hàm theo hướng "cải thiện chiến lược". Cách duy nhất để biết một bộ tham số có tốt hay không là *cho nó chơi thật và xem kết quả*.

Trong bối cảnh đó, Thuật toán Di truyền (Genetic Algorithm — GA) nổi lên như một giải pháp tự nhiên và hiệu quả. GA thuộc họ thuật toán tối ưu hóa theo quần thể (population-based optimization) — thay vì đi theo một đường duy nhất từ một điểm khởi đầu như gradient descent, GA duy trì và cải thiện đồng thời một *tập hợp* nhiều nghiệm tiềm năng, cho phép khám phá nhiều vùng trong không gian tìm kiếm song song. Đây chính là ưu điểm then chốt: GA không dễ bị kẹt ở một cực tiểu địa phương vì nó không bao giờ chỉ "nhìn" vào một điểm duy nhất.

---

### 2.1.2 Nền Tảng Sinh Học Và Mô Hình Tính Toán

Ý tưởng nền tảng của GA đến từ sinh học tiến hóa. Năm 1859, Charles Darwin trong công trình *On the Origin of Species* đề xuất cơ chế **chọn lọc tự nhiên**: trong một quần thể có biến thể di truyền, những cá thể có đặc điểm phù hợp với môi trường sẽ có xác suất sống sót và sinh sản cao hơn, truyền lại đặc điểm đó cho thế hệ sau. Trải qua nhiều thế hệ, quần thể tích lũy dần các đặc điểm có lợi — đây là cơ chế mà tự nhiên dùng để "tối ưu hóa" sinh vật mà không cần một nhà thiết kế trung tâm.

John Holland tại Đại học Michigan nhận ra rằng cơ chế này có thể được hình thức hóa thành thuật toán tính toán. Trong công trình *Adaptation in Natural and Artificial Systems* (1975), ông đề xuất một khung toán học để mô phỏng quá trình tiến hóa: **quần thể các nghiệm tiềm năng cạnh tranh với nhau, những nghiệm tốt hơn có xác suất cao hơn để sinh ra thế hệ con, và qua nhiều thế hệ, chất lượng trung bình của quần thể tăng dần**. Đây là nền tảng của GA hiện đại.

Để ánh xạ ý tưởng sinh học vào không gian tính toán, GA thiết lập một hệ thuật ngữ song song: mỗi *nghiệm tiềm năng* tương ứng với một **cá thể** (individual), *cách biểu diễn nghiệm* tương ứng với **nhiễm sắc thể** (chromosome), *mỗi thành phần của nghiệm* tương ứng với một **gene**, *tập hợp các cá thể* là **quần thể** (population), và *hàm đánh giá chất lượng* là **độ thích nghi** (fitness). Sự ánh xạ này không chỉ là ẩn dụ — nó cho phép "mượn" trực tiếp ba toán tử sinh học đã được tự nhiên kiểm nghiệm hàng triệu năm: **chọn lọc** (selection), **lai ghép** (crossover) và **đột biến** (mutation).

---

### 2.1.3 Biểu Diễn Nghiệm — Thiết Kế Chromosome

Trước khi có thể áp dụng bất kỳ toán tử nào, câu hỏi đầu tiên và quan trọng nhất cần trả lời là: *làm thế nào để mã hóa một nghiệm dưới dạng chromosome*? Câu trả lời không có một đáp án duy nhất đúng — nó phụ thuộc hoàn toàn vào bản chất của bài toán.

Trong lịch sử phát triển GA, có ba chiến lược mã hóa phổ biến nhất. **Binary encoding** biểu diễn mỗi gene dưới dạng bit 0 hoặc 1, đây là dạng mã hóa nguyên thủy mà Holland đề xuất ban đầu. Nó đơn giản và dễ phân tích lý thuyết, nhưng để biểu diễn một số thực chính xác cần nhiều bit, và các phép toán crossover/mutation trên chuỗi bit không tự nhiên phản ánh ý nghĩa của tham số. **Permutation encoding** dùng khi nghiệm là một thứ tự sắp xếp — phù hợp cho bài toán như lên lịch hay tối ưu hóa tuyến đường, nhưng không áp dụng được cho bài toán tham số liên tục. **Real-valued encoding** biểu diễn mỗi gene trực tiếp là một số thực — đây là lựa chọn tự nhiên nhất khi nghiệm là một tập hợp tham số liên tục.

Đề tài sử dụng **real-valued encoding**: mỗi chromosome là một mảng 37 số thực, mỗi giá trị trong đoạn [0, 1]:

```csharp
// Chromosome.cs
public const int GeneCount = 37;
public float[] genes = new float[GeneCount];
```

Lý do lựa chọn này rất cụ thể. Bài toán cần giải là: *ước lượng mức độ ưu tiên của bot đối với từng yếu tố trong game* — ví dụ, "bot này ưu tiên ATK cao như thế nào so với HP?" hay "bot này sẵn sàng reroll mạnh đến đâu?". Đây là những câu hỏi về **mức độ**, không phải về **có/không**. Binary encoding sẽ buộc ta phải chọn: ưu tiên ATK (1) hay không (0) — một sự đơn giản hóa thô bạo đánh mất sắc thái quan trọng nhất của bài toán. Real-valued encoding giữ nguyên tính liên tục đó: gene[0] = 0.2 nghĩa là "coi trọng ATK ít", gene[0] = 0.9 nghĩa là "coi trọng ATK rất nhiều", và mọi mức độ trung gian đều được biểu diễn tự nhiên.

Quyết định thứ hai trong thiết kế chromosome là **chuẩn hóa tất cả giá trị về đoạn [0, 1]**. Điều này quan trọng vì nó giúp các gene có thể so sánh và kết hợp một cách có nghĩa. Nếu gene[0] có thể nhận giá trị trong [0, 100] trong khi gene[18] chỉ trong [0, 1], thì crossover sẽ tạo ra những chromosome không nhất quán về "quy mô cảm nhận" của từng gene. Việc chuẩn hóa về [0, 1] cũng đơn giản hóa toán tử mutation — luôn biết rằng kết quả sau mutation cần được clamp về [0, 1].

Thêm vào đó, 37 gene được phân nhóm có chủ ý thành 9 cụm chức năng:

| Nhóm | Gene | Chức năng |
|------|------|-----------|
| 1 | [0–3] | Đánh giá chỉ số cơ bản (ATK, HP, Tier, Cost) |
| 2 | [4–6] | Giá trị passive keyword (Taunt, Reborn, Safeguard) |
| 3 | [7–12] | Trọng số loại trigger (StartOfBattle, OnDeath, ...) |
| 4 | [13–17] | Trọng số loại effect (AddStats, Summon, DealDmg, ...) |
| 5 | [18–20] | Ưu tiên bộ tộc (Babylon, Olympus, Niles) |
| 6 | [21–23] | Ngữ cảnh board (Merge, Frontline, Threshold) |
| 7 | [24–27] | Hành vi reroll (Threshold, Max, Reserve, Sell) |
| 8 | [28–31] | Hành vi spell (Threshold, Target, Economy) |
| 9 | [32–36] | Trigger con độc lập (Aura, OnSell, OnAllyGroup...) |

Sự phân nhóm này không ảnh hưởng đến quá trình training (các gene vẫn được xử lý như nhau), nhưng có ý nghĩa lớn về mặt diễn giải kết quả: sau khi training, ta có thể nhìn vào một chromosome và ngay lập tức đọc được "cá tính chiến lược" của bot — bot này ưu tiên bộ tộc nào, thích reroll hay tiết kiệm, đánh giá cao ability loại nào. Đây là điều các phương pháp như Neural Network không làm được — các trọng số của NN không mang ý nghĩa diễn giải được (interpretability).

> **[HÌNH 2.1 — Cấu trúc Chromosome 37 Gene]** *Biểu đồ thanh ngang phân nhóm 9 cụm gene theo màu sắc: nhóm Chỉ số cơ bản (0–3), Keywords (4–6), Trigger weights (7–12), Effect weights (13–17), Tribe synergy (18–20), Board context (21–23), Reroll behavior (24–27), Spell behavior (28–31), Trigger con (32–36). Mỗi nhóm một màu nền, kèm nhãn tên gene.*

---

### 2.1.4 Hàm Đánh Giá Độ Thích Nghi — Cầu Nối Giữa Toán Học và Thực Tế

Nếu chromosome là "thân xác" của một nghiệm, thì **hàm fitness** là "môi trường" mà nghiệm đó phải đối mặt. Đây là cầu nối duy nhất giữa không gian tham số trừu tượng và bài toán thực tế cần giải — GA không biết gì về game, về chiến lược hay về Auto Chess, nó chỉ biết rằng chromosome nào có fitness cao hơn thì tốt hơn. Chính vì vậy, thiết kế hàm fitness cẩn thận là điều kiện tiên quyết để toàn bộ hệ thống hoạt động.

Một hàm fitness tốt cần thỏa mãn ba yêu cầu đồng thời. Thứ nhất, nó phải **phân biệt được** — hai nghiệm khác nhau cần có fitness khác nhau, nếu không GA không có cơ sở để ưu tiên bên nào. Thứ hai, nó phải **tương quan thực sự với mục tiêu** — fitness cao phải nghĩa là "bot chơi tốt hơn" theo nghĩa thực tiễn, không chỉ theo một tiêu chí gián tiếp xa rời thực tế. Thứ ba, nó phải **tính toán được trong thời gian chấp nhận** — vì mỗi chromosome cần được đánh giá nhiều lần qua mỗi thế hệ, độ phức tạp của hàm fitness nhân trực tiếp với tổng thời gian training.

Trong đề tài, hàm fitness được đo qua **số điểm tích lũy từ các trận đấu mô phỏng**: mỗi chromosome thi đấu `matchesPerChrom` trận (5 trận quick mode, 20 trận production mode) với các đối thủ chọn ngẫu nhiên từ quần thể. Điểm mỗi trận gồm ba thành phần (chi tiết đầy đủ ở Mục 5.4.3):

```
score(trận) = kết_quả_nhị_phân  +  biên_thắng  +  tốc_độ_thắng
            = {120 thắng / 70 hòa / 35 thua}
            + hpA × 6 − hpB × 3
            + (nếu thắng) (MaxTurns − turns) × 2

fitness(c) = Σᵢ score(trận i),   i ∈ [0, matchesPerChrom)
```

Có một chi tiết thiết kế đáng chú ý: điểm hòa được đặt là 70 (không phải 0). Nếu hòa = 0, chromosome có chiến lược phòng thủ bền bỉ — không thắng nhanh nhưng cũng không thua nhanh — sẽ không bao giờ được chọn, dù đó có thể là chiến lược hợp lý. Công thức ba thành phần cho phép bot học đồng thời: thắng (120/70/35), thắng áp đảo (biên HP), và thắng nhanh (tốc độ) — thay vì chỉ tối ưu một chiều.

Một thách thức cố hữu của cách đánh giá này là **fitness noise** — kết quả trận đấu có yếu tố ngẫu nhiên (shop roll, thứ tự các lá trong pool), nên cùng một chromosome có thể nhận điểm khác nhau ở hai lần chạy khác nhau. Đây là lý do cần `matchesPerChrom ≥ 5`: càng nhiều trận, độ nhiễu càng giảm. Production mode với 20 trận/chromosome giúp ước lượng fitness đủ ổn định để quá trình chọn lọc có ý nghĩa.

---

### 2.1.5 Vòng Lặp Tiến Hóa — Ba Toán Tử Cốt Lõi

Sau khi có chromosome và hàm fitness, vòng lặp chính của GA có thể được viết gọn trong sơ đồ sau:

```
Khởi tạo quần thể P (ngẫu nhiên hoặc có seeding)
    │
    ▼
[Lặp qua các thế hệ g = 0 → G-1]
    │
    ├─► Đánh giá: tính fitness(c) cho mọi c ∈ P
    │
    ├─► Sắp xếp P theo fitness giảm dần
    │
    ├─► Tạo thế hệ mới P':
    │       ├── Clone elite (top 10%)                → bảo toàn tốt nhất
    │       ├── TournamentSelect × 2 → Crossover     → kế thừa từ tốt
    │       └── Mutate + Immigrate                   → khám phá mới
    │
    └─► P ← P'
    │
    ▼
Trả về 5 specialist bot từ quần thể cuối
```

> **[HÌNH 2.2 — Vòng Lặp Tiến Hóa GA]** *Sơ đồ flowchart minh họa vòng lặp qua các thế hệ: Khởi tạo → Đánh giá fitness → Sắp xếp → Tạo thế hệ mới (Elite clone + Tournament Select → Crossover + Mutate + Immigrate) → Kiểm tra điều kiện dừng → Lặp lại. Làm rõ hơn sơ đồ ASCII bên trên.*

Sức mạnh của GA nằm ở sự phân công rõ ràng giữa ba toán tử này: **chọn lọc** tập trung vào khai thác (exploit) những gì đã biết là tốt; **lai ghép** tái tổ hợp các phần tốt từ nhiều nghiệm; **đột biến** khám phá (explore) những vùng chưa từng thử. Sự cân bằng giữa exploit và explore là yếu tố quyết định hiệu quả của GA.

**Chọn lọc — Tournament Selection:**

Để chọn cha mẹ cho vòng lai ghép, đề tài sử dụng **Tournament Selection với k = 3**: chọn ngẫu nhiên 3 cá thể từ quần thể, lấy cá thể có fitness cao nhất trong nhóm đó làm cha/mẹ:

```csharp
// GATrainer.cs — TournamentSelect()
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
```

Lý do chọn Tournament thay vì Roulette Wheel (một phương pháp phổ biến khác) liên quan đến vấn đề **superindividual**: nếu dùng Roulette Wheel — xác suất chọn tỉ lệ thuận với fitness — khi một chromosome có fitness vượt trội, nó sẽ chiếm phần lớn "diện tích bánh xe" và gần như mọi cặp cha mẹ đều có nó. Trong vài thế hệ, quần thể trở thành bản sao của cá thể đó, diversity sụt giảm đột ngột và GA mất khả năng khám phá. Tournament Selection tránh vấn đề này vì xác suất được chọn phụ thuộc vào *hạng tương đối* trong tournament, không phải giá trị fitness tuyệt đối: dù một chromosome có fitness gấp 10 lần đối thủ yếu nhất, nó chỉ thắng tournament nếu được bốc cùng với ít nhất một đối thủ trong batch đó. Với k = 3, đây là sự cân bằng đã được kiểm chứng trong nhiều nghiên cứu: đủ áp lực để hội tụ, đủ thưa để không loại sạch minority.

**Lai ghép — 2-Point Crossover:**

Crossover mô phỏng quá trình trao đổi vật chất di truyền giữa hai nhiễm sắc thể khi sinh sản. Mục tiêu là tạo ra con cái kế thừa "điểm mạnh" của cả cha lẫn mẹ, hy vọng rằng hai bộ phận tốt kết hợp lại sẽ tạo ra một cá thể tốt hơn cả hai. Đây là "lý thuyết building block" mà Holland đề xuất: các chuỗi gene ngắn mang lại lợi ích (gọi là *schemata*) được bảo toàn và kết hợp qua crossover.

Đề tài sử dụng **2-point crossover**: chọn ngẫu nhiên hai điểm cắt `pt1 ≤ pt2`, đoạn giữa [pt1, pt2) lấy từ cha B, hai đầu còn lại lấy từ cha A:

```
Cha A: [a₀ a₁ a₂ a₃ | a₄ a₅ a₆ a₇ a₈ | a₉ a₁₀ ... a₃₆]
Cha B: [b₀ b₁ b₂ b₃ | b₄ b₅ b₆ b₇ b₈ | b₉ b₁₀ ... b₃₆]
                  pt1=4            pt2=9
Con:   [a₀ a₁ a₂ a₃ | b₄ b₅ b₆ b₇ b₈ | a₉ a₁₀ ... a₃₆]
```

```csharp
// GATrainer.cs — CrossoverAndMutate()
int pt1 = Random.Range(0, GeneCount);
int pt2 = Random.Range(0, GeneCount);
if (pt1 > pt2) { int tmp = pt1; pt1 = pt2; pt2 = tmp; }

for (int i = 0; i < GeneCount; i++)
    child.genes[i] = (i >= pt1 && i < pt2) ? b.genes[i] : a.genes[i];
```

Tại sao 2-point thay vì uniform crossover (mỗi gene chọn ngẫu nhiên 50/50 từ cha hoặc mẹ)? Lý do nằm ở khái niệm **epistasis** — sự phụ thuộc lẫn nhau giữa các gene: khi giá trị của gene này ảnh hưởng đến ý nghĩa của gene kia, chúng cần được "di truyền cùng nhau" để giữ nguyên tính cộng tác. Trong chromosome 37 gene của dự án, 9 nhóm gene được thiết kế để mỗi nhóm là một "đơn vị ngữ nghĩa" — ví dụ, gene[18], [19], [20] cùng kiểm soát tribe synergy, và một bot hiệu quả cần chúng nhất quán với nhau (không thể vừa ưu tiên Babylon [18] cao vừa ưu tiên Niles [20] cao vì hai chiến lược này mâu thuẫn). Uniform crossover sẽ xé nhỏ các nhóm này bừa bãi, tạo ra con cái "lộn xộn" về mặt chiến lược. 2-point crossover, với xác suất cao, giữ nguyên các đoạn gene liền kề trong cùng nhóm — bảo toàn epistasis tốt hơn mà không cần thiết kế phức tạp hơn.

**Đột biến — Gaussian Noise với Box-Muller:**

Nếu crossover chỉ tái tổ hợp những gì đã có trong quần thể, thì đột biến là cơ chế duy nhất tạo ra **vật liệu di truyền hoàn toàn mới** — những giá trị gene chưa từng tồn tại trong bất kỳ cá thể nào trước đó. Không có đột biến, quá trình tìm kiếm bị giới hạn trong "bao lồi" (convex hull) của quần thể ban đầu và không thể thoát ra ngoài. Đây là lý do mutation rate không thể bằng 0.

Tuy nhiên, đột biến quá mạnh cũng có hại: nếu mỗi gene đều bị thay bằng giá trị ngẫu nhiên hoàn toàn, quá trình tiến hóa trở thành random walk — không còn giữ lại được những gì đã học từ thế hệ trước. Thực nghiệm nhiều thập kỷ cho thấy mutation rate thường nằm trong khoảng 0.5%–2% cho mỗi gene khi dùng binary encoding, và thấp hơn nhưng với biên độ lớn hơn khi dùng real-valued encoding.

Thay vì thay thế gene bằng giá trị ngẫu nhiên hoàn toàn, đề tài dùng **Gaussian mutation**: cộng thêm nhiễu từ phân phối chuẩn N(0, σ) vào giá trị gene hiện tại. Điều này tạo ra những thay đổi *nhỏ xung quanh giá trị hiện tại* — thay đổi nhỏ phổ biến hơn thay đổi lớn, phản ánh đúng trực giác "fine-tuning": nếu gene[0] = 0.8 đang cho kết quả tốt, có nhiều khả năng 0.75 hoặc 0.85 còn tốt hơn, chứ không phải 0.1. Đây là lý do Gaussian mutation hội tụ mượt mà hơn Uniform mutation.

Để lấy mẫu từ phân phối chuẩn trong môi trường không có thư viện thống kê (Unity C#), đề tài dùng **biến đổi Box-Muller** — phương pháp biến đổi hai biến đều U(0,1) thành một biến chuẩn N(0,1):

```csharp
// GATrainer.cs — bên trong CrossoverAndMutate()
if (Random.value < mutationRate)   // mutationRate = 0.08 → 8% mỗi gene
{
    float u1 = Mathf.Max(1e-6f, Random.value);   // tránh ln(0)
    float z  = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * Random.value);
    child.genes[i] += z * mutationMag;            // mutationMag = 0.12 → σ = 0.12
}
child.genes[i] = Mathf.Clamp01(child.genes[i]);  // giữ trong [0, 1]
```

> **[HÌNH 2.3 — Gaussian Mutation]** *Biểu đồ phân phối chuẩn N(0, 0.12) minh họa phạm vi thay đổi của một gene sau mutation: phần lớn thay đổi nằm trong ±0.24 (2σ), xác suất thay đổi lớn hơn giảm nhanh theo đuôi phân phối.*

Với `mutationRate = 0.08` và 37 gene, mỗi chromosome khi sinh ra trung bình có `37 × 0.08 ≈ 3` gene bị đột biến. Với `mutationMag = 0.12`, phần lớn đột biến thay đổi giá trị gene trong khoảng ±0.24 (2σ) — đủ để khám phá lân cận, không đủ để phá hỏng hoàn toàn cấu trúc gene đã có.

---

### 2.1.6 Elitism — Bảo Toàn Tri Thức Qua Thế Hệ

Crossover và mutation, dù cần thiết cho khám phá, đều có thể làm hỏng những chromosome đang hoạt động tốt. Nếu chromosome tốt nhất thế hệ hiện tại bị đưa vào crossover và nhận phần gene kém từ cha mẹ thứ hai, fitness của nó có thể giảm — và thế hệ sau không còn cá thể nào tốt bằng thế hệ trước. Đây là vấn đề **fitness regression**.

**Elitism** giải quyết vấn đề này bằng cách đơn giản: sao chép nguyên vẹn một số cá thể tốt nhất từ thế hệ hiện tại vào thế hệ tiếp theo, không qua bất kỳ toán tử nào. Điều này đảm bảo tính chất toán học quan trọng: **fitness tốt nhất của quần thể không bao giờ giảm theo thế hệ** — GA với elitism là một thuật toán *monotonically improving* theo nghĩa đó.

Đề tài cài đặt elitism theo hai cấp độ. Cấp độ đầu tiên là **elitism toàn cục**: clone top `max(3, round(Lerp(populationSize/18, populationSize/8, progress)))` chromosome có fitness cao nhất — khoảng 7 cá thể ở đầu training và ~15 ở cuối (tăng dần để bảo toàn nhiều nghiệm tốt hơn khi quần thể đã trưởng thành). Cấp độ thứ hai là **elitism theo archetype**: mỗi thế hệ bổ sung thêm top 2 Babylon, top 2 Niles, top 2 Summoner score, top 2 Resilient score vào thế hệ sau — đảm bảo từng dòng phong cách chơi luôn có đại diện tốt nhất của mình được bảo toàn, dù fitness tổng của nó thấp hơn nhóm khác ở thời điểm hiện tại:

```csharp
// GATrainer.cs — BreedNextGen()
int eliteCount = Mathf.Max(3, Mathf.RoundToInt(
    Mathf.Lerp(populationSize / 18f, populationSize / 8f, progress)));
AddTopClones(nextGen, population, c => true, eliteCount);              // global elite
AddTopClones(nextGen, population, IsBabylon, 2);                       // Babylon elite
AddTopClones(nextGen, population, IsNile, 2);                          // Niles elite
AddTopClones(nextGen, population.OrderByDescending(SummonerScore), 2); // Summoner
AddTopClones(nextGen, population.OrderByDescending(ResilientScore), 2);// Resilient
```

Thiết kế elitism theo archetype này phản ánh một mục tiêu thiết kế quan trọng của dự án: không chỉ tìm một bot "tốt nhất", mà tìm ra **nhiều phong cách chơi tốt nhất**. Một bot Babylon giỏi nhất có thể không có fitness tổng cao bằng một generalist, nhưng nó có giá trị riêng khi cần đa dạng hóa độ khó game.

> **[HÌNH 2.4 — Elitism và Fitness Không Giảm Theo Thế Hệ]** *Đồ thị fitness tốt nhất (best fitness) qua các thế hệ: đường best fitness luôn không giảm (monotonically non-decreasing) nhờ elitism, trong khi đường average fitness có thể dao động. So sánh với trường hợp không có elitism để làm rõ tác dụng.*

---

### 2.1.7 Duy Trì Đa Dạng Di Truyền — Chống Hội Tụ Sớm

Vấn đề nghiêm trọng nhất mà GA phải đối mặt là **premature convergence** (hội tụ sớm): toàn bộ quần thể co dần về cùng một vùng hẹp trong không gian tìm kiếm, mất đi sự đa dạng và không còn khả năng cải thiện dù chưa tìm được nghiệm tối ưu. Dấu hiệu nhận biết rõ nhất là độ lệch chuẩn fitness (std_dev) tiến về 0 — khi tất cả cá thể gần giống nhau, fitness của chúng cũng gần bằng nhau.

Premature convergence có thể xảy ra do nhiều nguyên nhân cộng hưởng: quần thể ban đầu quá nhỏ và thiếu đa dạng, áp lực chọn lọc quá cao (tournament size k lớn), hay đơn giản là sự may mắn khiến một vài cá thể tương tự nhau chiếm ưu thế sớm. Trong GA với quần thể 30–120 cá thể, đây là nguy cơ thực sự — đặc biệt khi không gian tìm kiếm rộng (37 chiều) và fitness noise cao (kết quả trận đấu có ngẫu nhiên).

Để chống lại premature convergence, đề tài áp dụng đồng thời hai chiến lược bổ trợ nhau — mỗi chiến lược giải quyết một nguyên nhân khác nhau của vấn đề.

Chiến lược đầu tiên là **Island Model** kết hợp với khởi tạo có seeding. Thay vì tạo quần thể ngẫu nhiên hoàn toàn, quần thể được chia thành 5 sub-population (nhóm) từ đầu, mỗi nhóm được khởi tạo với định hướng chiến lược khác nhau: Babylon, Niles, Summoner, Resilient và Random. Các nhóm này không tách biệt hoàn toàn — chúng vẫn chia sẻ breeding pool chung và crossover xảy ra liên nhóm — nhưng cơ chế elitism per-archetype đảm bảo mỗi nhóm luôn duy trì ít nhất 2 đại diện tốt nhất qua mọi thế hệ. Kết quả là quần thể duy trì được sự đa dạng chiến lược ngay cả khi áp lực chọn lọc làm thu hẹp diversity tổng thể.

Chiến lược thứ hai là **immigration**: mỗi thế hệ, `immigrantRate = 12%` cá thể trong thế hệ mới là các chromosome seeded được tạo mới hoàn toàn (không phải con của bất kỳ cá thể cũ nào). Cơ chế này liên tục "bơm" vật liệu di truyền mới vào quần thể, ngăn chặn hiện tượng *genetic drift* — sự mất đa dạng do ngẫu nhiên thuần túy dù không có áp lực chọn lọc. Hơn nữa, nếu hệ thống phát hiện một bộ tộc bị underrepresented, số immigrant cho bộ tộc đó được tăng theo tỉ lệ kích thước quần thể — không hardcoded — để cơ chế có hiệu lực tương đương ở mọi quy mô:

```csharp
if (pctBabylon < 12f) immigrantCount += Mathf.RoundToInt(popSize * 0.06f);
if (pctNiles   < 12f) immigrantCount += Mathf.RoundToInt(popSize * 0.08f);
if (pctOther   < 10f) immigrantCount += Mathf.RoundToInt(popSize * 0.07f);
```

Cuối cùng, hệ thống dừng sớm khi phát hiện quần thể không còn cải thiện — nhưng thay vì theo dõi chỉ std_dev (đo sự đa dạng), hệ thống theo dõi `progressScore` — một chỉ số tổng hợp bao gồm best fitness, trung bình EMA, sức mạnh late-game và chất lượng tay bài:

```csharp
const int   PLATEAU_PATIENCE = 28;
const float PLATEAU_EPS      = 150f;
int minStopGen = Mathf.RoundToInt(generations * 0.75f);  // không dừng trước 75% gen

float progressScore = bestEver + avgEma * 0.45f + avgLate * 0.08f + avgCard * 0.035f;

if (progressScore - prevProgressScore < PLATEAU_EPS)
    plateauCount++;
else
    plateauCount = 0;

if (plateauCount >= PLATEAU_PATIENCE && g >= minStopGen)
    break;
```

Ngưỡng `minStopGen` là cải tiến quan trọng: dù plateau xảy ra sớm (ví dụ do warm-start đã cung cấp chromosome tốt từ đầu), training vẫn chạy ít nhất 75% số thế hệ — đủ thời gian để avg fitness cải thiện toàn quần thể, không chỉ tìm best chromosome đơn lẻ.

Hai chiến lược này bổ trợ nhau theo cách rõ ràng: Island Model ngăn hội tụ *về mặt cấu trúc* — bằng cách giữ các dòng gene riêng biệt tồn tại song song; immigration ngăn hội tụ *về mặt động lực* — bằng cách liên tục bơm vật liệu mới vào. Thiếu Island Model, immigration vẫn có thể bị áp lực selection đẩy ra khỏi quần thể trước khi kịp đóng góp. Thiếu immigration, Island Model chỉ duy trì được diversity ban đầu mà không có khả năng phục hồi khi một archetype bị drift.

> **[HÌNH 2.5 — Premature Convergence vs. Healthy Evolution]** *Hai đồ thị song song: (trái) trường hợp premature convergence — std_dev giảm về gần 0 sớm trong khi best fitness vẫn thấp; (phải) trường hợp healthy evolution với immigration — std_dev duy trì ổn định và best fitness tiếp tục tăng qua nhiều thế hệ. Đường std_dev (xanh), best fitness (đỏ), avg fitness (cam).*

---

### 2.1.8 Kết Quả Cuối — Chọn Lọc 5 Bot Chuyên Biệt

Phần lớn ứng dụng GA kết thúc bằng việc trả về cá thể có fitness cao nhất trong quần thể cuối. Tuy nhiên, mục tiêu thiết kế của đề tài không phải tìm "một bot tốt nhất" mà tìm "5 bot chuyên biệt khác nhau" để tạo ra sự đa dạng trong trải nghiệm game. Điều này đặt ra yêu cầu bổ sung: các bot được chọn không chỉ cần tốt mà còn cần đủ khác nhau về cấu trúc gene — tức là về phong cách chơi.

Để đo sự khác biệt giữa hai chromosome, đề tài dùng **khoảng cách Euclidean chuẩn hóa** trong không gian 37 chiều:

```
d(a, b) = √( Σᵢ (aᵢ - bᵢ)² / GeneCount )
```

Chuẩn hóa bằng GeneCount đảm bảo khoảng cách nằm trong [0, 1] bất kể số lượng gene. Hai bot chỉ được chọn cùng một lúc nếu `d(a, b) ≥ 0.18` — nghĩa là trung bình mỗi gene khác nhau ít nhất 0.18 đơn vị, một ngưỡng đủ để đảm bảo sự khác biệt chiến lược thực chất chứ không chỉ là nhiễu ngẫu nhiên.

Quá trình chọn diễn ra theo thứ tự ưu tiên: trước tiên chọn hardBot (cá thể có fitness cao nhất tuyệt đối), sau đó lần lượt chọn babylonBot (fitness cao nhất trong nhóm Babylon và cách xa các bot đã chọn), nileBot, summonerBot (theo SummonerScore đặc biệt), và resilientBot (theo ResilientScore). Kết quả là 5 chromosome đại diện cho 5 góc khác nhau trong không gian gene — 5 "cá tính chiến lược" thực sự phân biệt nhau.

> **[HÌNH 2.6 — Radar Chart So Sánh 5 Bot Archetype]** *Biểu đồ radar (spider chart) 5 trục đại diện cho 5 nhóm gene chính (Chỉ số cơ bản, Tribe, Reroll, Spell, Trigger weights); mỗi bot một đường màu khác nhau — hardBot (đỏ), babylonBot (vàng), nileBot (xanh lam), summonerBot (tím), resilientBot (xanh lá). Thể hiện profile chiến lược khác biệt giữa các bot.*

---

### 2.1.9 Tổng Hợp — GA Trong Bức Tranh Tổng Thể

Nhìn lại toàn bộ phần này, có thể thấy rằng Genetic Algorithm không phải là một thuật toán đơn lẻ mà là một **framework** — một khung tư duy cho phép đặt bài toán tối ưu hóa vào ngôn ngữ của tiến hóa tự nhiên. Sức mạnh của nó không đến từ bất kỳ toán tử đơn lẻ nào, mà từ sự phối hợp: selection khai thác kiến thức đã có, crossover tái tổ hợp building blocks, mutation khám phá vùng mới, elitism bảo toàn thành quả tốt nhất, và diversity mechanisms giữ cho quá trình không sa lầy quá sớm.

Đặc biệt phù hợp với bài toán Auto Chess AI vì ba lý do cốt lõi. Một là **không cần gradient** — fitness là kết quả trận đấu, không có đạo hàm. Hai là **tự nhiên hỗ trợ đa nghiệm** — cùng một lần training có thể thu được nhiều archetype khác nhau nhờ island model. Ba là **tài nguyên vừa phải** — với thiết kế headless simulation, production training hoàn thành trong 20 phút trên máy tính cá nhân thông thường.

Hiểu được GA ở mức này — không chỉ "thuật toán là gì" mà "tại sao nó phù hợp với bài toán cụ thể này và những cơ chế nào là cần thiết" — là nền tảng để đọc Chương 5 một cách có chiều sâu. Trước đó, Mục 2.2 đến 2.4 đặt nền tảng kỹ thuật còn lại: Unity Engine là môi trường chạy, TTE engine là ngôn ngữ mô tả game state, và lý thuyết kinh tế là bối cảnh để hiểu tại sao chromosome cần đến 14 gene chỉ để mã hóa hành vi kinh tế của bot.

---

*[Tiếp theo: Mục 2.2 — Lập trình game với Unity Engine]*
