## 5.3 BotAgent — Bộ Não Quyết Định

### 5.3.1 Vai Trò Và Thiết Kế Tổng Thể

`BotAgent` là lớp dịch ngôn ngữ của chromosome sang hành động trong game. Nếu chromosome là một tập trọng số trừu tượng, BotAgent là engine thực thi — nó biết *cách chơi game* và dùng trọng số từ chromosome để quyết định chơi *như thế nào*. Toàn bộ hành vi của bot trong một lượt chuẩn bị được gói trong một hàm duy nhất — `DecidePrepPhase()` — được gọi bởi `GameSimulator` mà không cần tương tác người dùng, coroutine hay Unity API.

Bên trong, hàm thực hiện tuần tự **bảy phase** theo thứ tự ưu tiên logic: reroll trước khi mua (để có cơ hội mua tốt hơn), mua trước khi bán (để biết có cần slot không), merge sau khi mua, sắp xếp sau khi merge, freeze sau cùng.

> **[HÌNH 5.4 — Flowchart 7-Phase DecidePrepPhase]** *Sơ đồ luồng 7 bước từ trên xuống: RerollPhase → BuyUnitsPhase → BuySpellsPhase → ProactiveSellPhase → TryMerge → RepositionPhase → FreezePhase. Mỗi phase một hộp màu, kèm điều kiện kích hoạt (gene threshold) và output chính.*

---

### 5.3.2 Hàm Đánh Giá Card — Cầu Nối Chromosome Và Game State

Hàm `Evaluate(CardDefinition c)` là công thức cốt lõi kết nối 37 gene với game state, được gọi trong hầu hết mọi quyết định mua:

```
S(c) = baseATK × genes[0]
     + baseHP  × genes[1]
     + (tier − 1) × genes[2] × 5

     + [hasTaunt]     × genes[4] × 10
     + [hasReborn]    × genes[5] × 12
     + [hasSafeguard] × genes[6] × 8

     + Σ_ability ( TriggerWeight(trigger) × EffectWeight(effect) × 10
                   × context_scale(trigger, sameTribeCount)
                   + [isEscalating] × TW × EW × 3 )

     + sameTribeCount × SynergyWeight(tribe) × 4

     + copies_on_board × genes[21] × (copies == 2 ? 16 : 8)

     + [hasTaunt] × emptyFrontlineSlots × genes[22] × 2

     ÷ cost × (1 + genes[3])
```

`context_scale` điều chỉnh trigger phụ thuộc đồng minh: `OnAllyDeath/Summon/Reborn` được nhân với `Clamp01(sameTribeCount / 2f)` — đảm bảo bot học rằng card với trigger OnAllyDeath chỉ có giá trị khi đội hình đủ đông. Công thức kết thúc bằng `÷ cost × (1 + genes[3])` để chuẩn hóa value/coin; `genes[3]` cho phép GA tinh chỉnh mức độ ưu tiên hiệu quả chi phí giữa các archetype.

---

### 5.3.3 Bảy Phase — Tổng Hợp Hành Vi Và Gene

| Phase | Hành vi chính | Gene chi phối |
|:-----:|---------------|:-------------:|
| **1 — Reroll** | Reroll nếu shop kém hơn board × genes[24]; tối đa genes[25]×3+1 lần; không reroll nếu coin dưới ngưỡng genes[26] | [24] threshold, [25] max lần, [26] coin dự phòng |
| **2 — Buy Units** | Greedy: mua card cao điểm nhất vượt genes[23]×3; bán unit yếu nhất để nhường slot nếu lợi nhuận đủ rõ | [23] ngưỡng mua |
| **3 — Buy Spells** | `EvaluateSpell()` riêng theo từng EffectType (xem bảng dưới); chỉ mua nếu điểm chuẩn hóa ≥ genes[28]×3 | [28] ngưỡng spell + gene effect tương ứng |
| **4 — Proactive Sell** | Bán unit có điểm dưới genes[27]×3 để giải phóng slot/coin; không bán unit token tạm thời | [27] ngưỡng bán chủ động |
| **5 — Merge** | Ghép 3 bản sao lv0→lv1, 2 bản sao lv1→lv2; giữ bản sao có tổng bonus lớn nhất | — (logic tất định) |
| **6 — Reposition** | Frontline: unit Taunt và OnDeath; Backline: unit Aura/support; sắp xếp qua PositionScore kết hợp `EvaluateInstance` + gene điều chỉnh | [22] Taunt, [17] eGiveBuff, [8] tOnDeath |
| **7 — Freeze** | Freeze shop nếu bot không aggressive reroll (1−genes[24] ≥ 0.35) VÀ có card muốn mua nhưng chưa đủ tiền | [24] (nghịch đảo) + [23] ngưỡng |

**Hàm EvaluateSpell() theo EffectType:**

| Loại spell | Công thức điểm | Gene |
|:--:|---|:--:|
| BuffStats permanent | `(val1×g[0] + val2×g[1]) × 2.5` | [0], [1] |
| GainCoin | `val × g[16] × g[31]` | [16], [31] |
| GainIncome permanent | `val × g[16] × g[31] × 12` | [16], [31] |
| GetRandomUnit / StealFromShop | `g[2] × 6 / 7` | [2] |
| UpgradeTierUnit | `g[2] × 12` | [2] |
| GiveDoubleAtkAndSafeguard | `g[0]×8 + g[6]×8` | [0], [6] |
| GiveEndTurnBuff | `(val1×g[0]+val2×g[1]) × g[11] × g[31] × 3` | [0],[1],[11],[31] |
| LoseLife / TransferStats | −25 / −8 (phạt cứng) | — |

Hệ số 12 cho GainIncome permanent phản ánh giá trị tích lũy thực: "+1 coin/lượt vĩnh viễn" tích lũy tối đa 20 coin qua 20 lượt — hệ số này còn thận trọng so với giá trị lý thuyết.

**Một điểm mù có chủ đích:** Bảng trên liệt kê tất cả các nhánh `EffectType` mà `EvaluateSpell()` thực sự xử lý — và đáng chú ý, `ConditionalCoinGain` (cơ chế nền của *Wager*, đã nêu ở mục 3.5.2: "nếu thắng trận kế tiếp → nhận 3 coin") không nằm trong số đó. Spell này rơi vào nhánh `default` của switch-statement và nhận điểm 0 một cách vô điều kiện — không chỉ với một bot cụ thể, mà với *mọi* chromosome trong quần thể, bất kể gene nào. Nói cách khác, đây không phải một tham số chưa được tinh chỉnh tốt; đó là một loại giá trị mà kiến trúc đánh giá hiện tại *về nguyên tắc* không thể biểu diễn.

Gốc rễ nằm ở chính giả định nền tảng của `EvaluateSpell()`: mỗi lá bài được chấm điểm như một đại lượng *tĩnh, độc lập theo ngữ cảnh* — một con số duy nhất tại thời điểm cân nhắc mua. Giả định đó đúng với phần lớn 21 spell (buff vĩnh viễn, tuyển quân, đổi coin tức thì đều có giá trị xác định ngay khi mua), nhưng thất bại đúng vào nhóm spell mà giá trị thật phụ thuộc vào một sự kiện *ngẫu nhiên và xảy ra trong tương lai* — kết quả trận đấu kế tiếp, một biến mà tại thời điểm đánh giá chưa hề tồn tại. Để gán điểm đúng cho `Wager`, hệ thống sẽ cần ước lượng xác suất thắng trận tiếp theo (một đại lượng phụ thuộc trạng thái bàn cờ, không phải thuộc tính cố định của lá bài) rồi nhân với phần thưởng kỳ vọng — một dạng suy luận hoàn toàn khác về bản chất so với phép tra cứu hệ số tuyến tính mà `EvaluateSpell()` được xây dựng để thực hiện.

Hệ quả đối với quá trình tiến hóa rất rõ ràng: gene không có cách nào "học" được xu hướng chấp nhận hay né tránh rủi ro đối với loại spell này, vì tín hiệu mà chọn lọc tự nhiên dựa vào — điểm số do `EvaluateSpell()` trả về — hoàn toàn không đổi (luôn bằng 0) bất kể chromosome biểu diễn thiên hướng nào. Đây là một ranh giới đáng được ghi nhận một cách tường minh hơn là che giấu: nó không làm hệ thống "sai" theo nghĩa có lỗi cần sửa, mà cho thấy một giới hạn cố hữu của lựa chọn kiến trúc "chấm điểm tĩnh trên từng lá bài riêng lẻ" — một sự đánh đổi giữa tính đơn giản, có thể kiểm chứng của mô hình đánh giá, và khả năng biểu diễn các loại giá trị có bản chất xác suất, phụ thuộc ngữ cảnh động. Việc nhận diện đúng *loại* giới hạn này — kiến trúc không biểu diễn được, chứ không phải tham số chưa tối ưu — là bước cần thiết để biết nên cải tiến ở đâu trong các vòng lặp thiết kế tiếp theo.

---

## 5.4 GameSimulator — Môi Trường Huấn Luyện

### 5.4.1 Vai Trò

`GameSimulator` là cầu nối giữa BotAgent và GATrainer: cung cấp môi trường đánh giá chromosome qua các trận đấu hoàn chỉnh dùng đúng cùng `CombatResolver`, `CardDatabase`, và logic shop-tier progression như game thật. Thiết kế plain C# (không MonoBehaviour) cho phép khởi tạo tự do mà không cần scene.

---

### 5.4.2 Vòng Lặp 20 Lượt

Mỗi trận chạy tối đa 20 lượt. Shop tier tự động tăng theo công thức `clamp((turn+2)/2, 1, 6)` — cả hai bot đối đầu ở cùng tier, đảm bảo training công bằng. Luồng mỗi lượt:

```
for turn in 0..19:
    shopTier = clamp((turn+2)/2, 1, 6)
    shopA = frozenShop[A] nếu A đã freeze, ngược lại GetRandomShop(tier)
    shopB = frozenShop[B] nếu B đã freeze, ngược lại GetRandomShop(tier)

    botA.DecidePrepPhase(shopA, tier)
    botB.DecidePrepPhase(shopB, tier)

    resolver.ResolveTurn(boardA, boardB)

    if boardA dead: hpA--
    if boardB dead: hpB--
    if hpA<=0 or hpB<=0: break

    botA/B.EndCombatPhase()      // reset board, xóa unit chết
    botA/B.TriggerEndTurnShop()  // áp buff tích lũy lượt tiếp
```

Nếu bot đã freeze shop lượt trước, GameSimulator dùng shop đã lưu — đảm bảo gene freeze (genes[24]) có hiệu lực thực sự và GA có lý do để tiến hóa hành vi này.

---

### 5.4.3 Hàm Fitness — Một Quá Trình Thiết Kế Lặp

Trong toàn bộ hệ thống AI, định nghĩa "chơi tốt" bằng một con số duy nhất là quyết định khó nhất — khó hơn cả việc viết vòng lặp tiến hóa hay cài đặt selection. Hàm fitness hiện tại không phải là phiên bản đầu tiên; nó là kết quả của ít nhất một vòng quan sát–giả thuyết–chỉnh sửa, và chính quá trình đó minh họa rõ vì sao thiết kế hàm fitness thường được xem là phần khó nhất, dễ sai nhất của bất kỳ ứng dụng GA nào lên một bài toán mới.

**Phiên bản đầu — chỉ thắng/thua:** Bản fitness sơ khai phản ánh đúng và chỉ đúng định nghĩa thắng cuộc của trò chơi: thắng được 10 điểm, hòa được 2, thua không được gì. Về mặt lý thuyết, đây là tín hiệu "trung thực" nhất có thể — nó không thêm bất kỳ giả định nào vượt ra ngoài luật chơi. Nhưng kết quả huấn luyện cho thấy ngay vấn đề: quần thể hội tụ rất nhanh về một chiến lược duy nhất — dồn toàn bộ coin vào lượt đầu để tạo một đội hình áp đảo ở giai đoạn sớm ("rush"). Chiến lược ấy thắng nhiều trận ngắn trước những đối thủ cũng yếu early-game, nhưng gần như luôn thua trước bất kỳ đối thủ nào biết tích lũy kinh tế và scale dần về sau. Nguyên nhân không nằm ở thuật toán tiến hóa — nó nằm ở chỗ tín hiệu chỉ phân biệt được *có thắng hay không*, mà không phân biệt được *thắng bằng cách nào*; quá trình chọn lọc, vì vậy, khen thưởng một chiến lược một chiều như thể nó là một chiến lược toàn diện.

**Phiên bản hiện tại — sáu tín hiệu cộng dồn, có trọng số:**

```
ScoreFromA(result, hpA, hpB, turns, lateScoreA, lateScoreB, cardScoreA, cardScoreB):
    base  = 300 nếu thắng | 100 nếu hòa | 10 nếu thua
    score = base + hpA×8 − hpB×4
    score += max(0, lateScoreA)×0.06 + clamp(lateScoreA − lateScoreB, ±300)×0.04
    score += max(0, cardScoreA)×0.035 + clamp(cardScoreA − cardScoreB, ±250)×0.025
    nếu turns ≥ 12: score += (turns − 11) × 2
    return max(1, score)
```

`lateScoreA`/`cardScoreA` (và cặp tương ứng của đối thủ) là hai đại lượng được dồn tích *trong suốt trận đấu* — chỉ bắt đầu được cộng dồn từ lượt 9 trở đi, với trọng số tăng dần tuyến tính cho đến lượt cuối (lượt càng về sau, đóng góp của trạng thái bàn cờ tại thời điểm đó vào tổng càng lớn). `lateScore` đo "sức mạnh hiện diện trên bàn" tại mỗi thời điểm lấy mẫu (tổng ATK/HP, tier, cấp merge, các từ khóa Taunt/Reborn/Safeguard…); `cardScore` đo "chất lượng khoản đầu tư vào đội hình" (tier, giá mua, cấp merge của từng lá, cùng một điểm chất lượng riêng cho từng kỹ năng đang sở hữu). So với phiên bản ba-thành-phần ban đầu của vòng lặp tinh chỉnh này, công thức hiện hành đã mở rộng thành ba *nhóm* tín hiệu: **kết quả trận đấu** (nền 300/100/10 — vẫn là tín hiệu áp đảo, chiếm phần lớn biên độ điểm), **biên độ HP tức thời** (`hpA×8 − hpB×4`, vẫn giữ nguyên tính bất đối xứng "tự bảo tồn" của thiết kế ban đầu — trọng số giữ mạng cao gấp đôi trọng số gây thiệt hại), và **một cặp tín hiệu được bổ sung về sau — sức mạnh bàn cờ và chất lượng đội hình ở nửa sau trận đấu**, mỗi loại được đo song song ở dạng tuyệt đối (thưởng cho việc "xây dựng tốt", không phụ thuộc đối thủ ra sao) lẫn dạng tương đối (thưởng thêm cho việc "xây dựng tốt hơn đối thủ"). Cặp tín hiệu bổ sung này ra đời chính xác để lấp khoảng trống mà phiên bản trước để lại — phiên bản chỉ nhìn vào *kết quả cuối cùng* của một trận đấu sẽ không thể phân biệt một chiến thắng đến từ một đội hình được đầu tư bài bản qua nhiều lượt với một chiến thắng may mắn trước một đối thủ yếu hơn; bằng cách lấy mẫu trạng thái bàn cờ tại nhiều thời điểm trong nửa sau trận — đúng giai đoạn mà sự khác biệt giữa "rush" và "scale" bộc lộ rõ nhất — hàm fitness có thêm một kênh quan sát độc lập với thắng/thua để ghi nhận *cách* một bot đi đến kết quả đó.

> **[HÌNH 5.5 — Sự Mở Rộng Của Hàm Fitness Qua Các Vòng Tinh Chỉnh]** *Hai sơ đồ đặt cạnh nhau: (trái) phiên bản chỉ-thắng-thua — phân phối điểm rời rạc ba giá trị (10/2/0), mũi tên dẫn tới "quần thể hội tụ về chiến lược rush một chiều"; (phải) phiên bản hiện hành — sơ đồ cộng dồn sáu tín hiệu theo ba nhóm (kết quả / biên độ HP / sức mạnh bàn cờ-chất lượng đội hình cuối trận), mũi tên dẫn tới "áp lực chọn lọc đa chiều hơn, phân biệt được *cách* một bot thắng chứ không chỉ *có* thắng hay không".*

**Giới hạn còn lại — vì sao một hàm fitness không bao giờ thực sự "hoàn chỉnh":** Ngay cả phiên bản sáu tín hiệu cũng chưa phải điểm dừng — nó chỉ đơn giản là chuyển câu hỏi "thiếu tín hiệu gì" sang một câu hỏi tinh vi hơn: "các tín hiệu đã có nên được cân với nhau theo tỉ lệ nào?" Quan sát kỹ các hệ số sẽ thấy một sự bất đối xứng có chủ đích: cặp tín hiệu sức mạnh bàn cờ/chất lượng đội hình mang hệ số rất nhỏ (0.06/0.04/0.035/0.025) so với nền kết quả trận đấu (300/100/10) — nghĩa là, dù được thiết kế để "nhìn thấy" quá trình xây dựng đội hình, chúng trên thực tế chỉ đóng vai trò *phân định tinh* giữa những chiến lược đã rõ thắng/thua, chứ không đủ sức lấn át tín hiệu kết quả để tự mình định hướng quần thể theo một triết lý "scale" cụ thể. Đây là một lựa chọn có chủ đích — nếu các tín hiệu phụ được cân nặng hơn, chúng có nguy cơ tạo ra một dạng hội tụ một chiều khác (ví dụ: quần thể chỉ học cách "trông có vẻ mạnh" mà quên mất mục tiêu là thắng) — nhưng cũng đồng nghĩa rằng không có cơ sở lý thuyết nào đảm bảo tỉ lệ 0.06/0.04/0.035/0.025 hiện tại là điểm cân bằng đúng cho trục đánh đổi tempo–economy đã trình bày như lý thuyết tổng quát ở mục 2.4.2 — có thể tồn tại một bộ trọng số khác giúp quần thể khám phá được những điểm cân bằng tốt hơn, mà cấu hình hiện tại đơn giản là chưa "nhìn thấy". Đây không phải một lỗi có thể vá một lần là xong; nó là đặc điểm cố hữu của bài toán: mỗi lần thêm một tín hiệu mới để lấp một khoảng trống quan sát được, người thiết kế lại tạo ra một tham số cân bằng mới — và do đó một câu hỏi mới không có điểm dừng tự nhiên — *tỉ lệ nào giữa các tín hiệu mới phản ánh đúng nhất "chơi giỏi"?* Thiết kế hàm fitness, vì vậy, không phải một bước "định nghĩa rồi xong" ở đầu dự án, mà là một vòng lặp quan sát hành vi → đặt giả thuyết về nguyên nhân → chỉnh sửa tín hiệu (thêm tín hiệu mới hoặc cân lại trọng số cũ) → quan sát lại — lặp lại cho đến khi hành vi quan sát được đủ gần với kỳ vọng thiết kế, dù "đủ gần" đến đâu cũng không bao giờ là "xong".

---

## 5.5 GATrainer — Vòng Lặp Tiến Hóa

### 5.5.1 Tham Số Huấn Luyện

| Tham số | Quick Mode | Production Mode | Ý nghĩa |
|---------|:----------:|:---------------:|---------|
| `populationSize` | 30 | 120 | Số chromosome |
| `generations` | 40 | 180 | Số thế hệ tối đa |
| `matchesPerChrom` | 5 | 20 | Trận/chromosome/thế hệ |
| `mutationRate` | 0.10 | 0.10→0.035 | Xác suất mutation (thích nghi) |
| `mutationMag` | 0.12 | 0.12→0.035 | Biên độ Gaussian σ (thích nghi) |
| `immigrantRate` | 0.12 | 0.12→0.04 | Tỉ lệ chromosome mới/thế hệ |
| `minLibraryDistance` | 0.18 | 0.18 | Khoảng cách Euclidean tối thiểu giữa specialist |

Quick mode hoàn thành trong ~2 phút (kiểm tra logic). Production mode ~20–30 phút cho kết quả tích hợp game.

---

### 5.5.2 Khởi Tạo — 5 Sub-Population Seeded

Quần thể chia thành 5 nhóm bằng nhau, mỗi nhóm được định hướng bằng seed gene đặc trưng: nhóm 0 seed genes[18] (sBabylon) cao; nhóm 1 seed genes[20] (sNiles) cao; nhóm 2 seed genes[14]+[5]+[8] cho Summoner; nhóm 3 seed genes[1]+[4]+[5]+[6] cho Resilient; nhóm 4 hoàn toàn ngẫu nhiên. Phần gene còn lại của mỗi nhóm vẫn ngẫu nhiên — GA tự tìm giá trị tối ưu. Seeding giải quyết vấn đề thực tiễn: xác suất GA ngẫu nhiên tìm ra chromosome Summoner (đòi hỏi nhiều gene cụ thể đồng thời cao) là cực thấp trong không gian 37 chiều với thời gian training hạn chế.

---

### 5.5.3 Đánh Giá Fitness — Self-Play + Benchmark

Mỗi chromosome được đánh giá qua hai tập: **20 trận self-play** với đối thủ ngẫu nhiên từ quần thể (trọng số 1.0×) và **benchmark** với 10 chromosome seeded cố định, được làm mới mỗi 30 thế hệ (trọng số 0.5×). Benchmark cố định cho phép so sánh fitness ổn định qua các thế hệ, không phụ thuộc hoàn toàn vào chất lượng quần thể hiện tại.

---

### 5.5.4 Chọn Lọc — Tournament Thích Nghi

Tournament size tăng theo `progress` (0.0→1.0): k=3 (early, áp lực thấp, diversity cao) → k=4 (mid) → k=5 (late, áp lực cao, hội tụ nhanh). Kỹ thuật *annealing selection pressure* này cân bằng exploration và exploitation theo thời gian tự động.

---

### 5.5.5 Đột Biến Thích Nghi Và Clone Tinh Chỉnh

`mutationRate` và `mutationMag` giảm từ 10%/σ=0.12 xuống 3.5%/σ=0.035 theo đường cong SmoothStep — đầu training khám phá rộng, cuối training fine-tune. Ngoài crossover thông thường, giai đoạn mid-late còn tạo thêm các *refinement clone*: bản sao của elite chromosome với mutation rate và magnitude giảm thêm 35–55%, tạo ra biến thể tinh chỉnh quanh nghiệm đang tốt mà không trộn genetic với chromosome khác.

---

### 5.5.6 Elitism — Bốn Tầng Bảo Toàn

Mỗi thế hệ bảo toàn tối thiểu `eliteCount + 8` cá thể qua bốn tầng: (1) top global theo fitness tổng, (2) top-2 chromosome Babylon, (3) top-2 Niles, (4) top-2 Summoner + top-2 Resilient theo composite score. Tầng 2–4 đảm bảo mỗi archetype luôn có đại diện tốt nhất được bảo toàn — dù babylonBot có thể không vào top global elite, nó chắc chắn không bị xóa. `eliteCount` tăng từ ~7 (early) lên ~15 (late): đầu training dành chỗ cho crossover đa dạng, cuối training bảo toàn nhiều nghiệm tốt hơn.

---

### 5.5.7 Immigration — Chống Premature Convergence

Immigrant rate giảm từ 12%→4% theo progress. Ngoài lịch trình cố định, hệ thống theo dõi tỉ lệ tribe trong quần thể và bơm thêm chromosome khi Babylon < 12% hoặc Niles < 12% hoặc "Other" < 8% — đảm bảo babylonBot và nileBot luôn có nguyên liệu di truyền để được chọn cuối training.

---

### 5.5.8 Dừng Sớm — Plateau Detection

Dừng sớm khi `std_dev` của fitness quần thể thay đổi dưới 0.5 trong 15 thế hệ liên tiếp. Theo dõi `std_dev` (thay vì best fitness) phát hiện hội tụ thực sự: best có thể không đổi nhưng avg vẫn đang cải thiện, còn khi `std_dev` ổn định, toàn quần thể thực sự đã không còn tiến bộ.

---

### 5.5.9 Chọn 5 Bot Cuối — Diversity-Aware

5 bot được chọn theo thứ tự: (1) **hardBot** = chromosome Hall of Fame (fitness cao nhất bao giờ); (2) **babylonBot** = Babylon fitness cao nhất, GeneDistance ≥ 0.18 so với hardBot; (3) **nileBot** = Niles fitness cao nhất, cách xa cả hai bot trước; (4) **summonerBot** = SummonerScore cao nhất trong `viable` (fitness ≥ 80% avg); (5) **resilientBot** = ResilientScore cao nhất trong viable. Nếu không tìm được candidate đủ xa, `DiversityBonus` (khoảng cách tối thiểu × 100) được cộng vào score — thưởng cho chromosome "khác biệt nhất" ngay cả khi không phải tốt nhất về fitness thuần.

---

## 5.6 AILibrary — Lưu Trữ Và Nạp Kết Quả

Kết quả training được lưu vào `Assets/Resources/AI_Library.json` với cấu trúc:

```json
{
  "hardBot":     { "genes": [0.71, 0.68, ...], "fitness": 4764.0 },
  "babylonBot":  { "genes": [0.12, 0.45, ..., 0.91, 0.04, 0.08, ...], "fitness": 4764.0 },
  "nileBot":     { "genes": [0.38, 0.72, ..., 0.07, 0.03, 0.88, ...], "fitness": 4727.0 },
  "summonerBot": { "genes": [0.20, 0.35, ..., 0.94, 0.85, ...], "fitness": 3892.0 },
  "resilientBot":{ "genes": [0.14, 0.89, 0.31, ...], "fitness": 3645.0 }
}
```

`AIManager` đọc file khi game khởi động. Chỉ yêu cầu `hardBot` hợp lệ (có đủ 37 gene) — bốn specialist là optional, fallback về hardBot nếu training không tìm được bot đủ xa. Tính minh bạch của JSON cho phép xác nhận thủ công profile gene của từng bot mà không cần chạy lại training.

---

## 5.7 Kết Quả Thực Nghiệm

### 5.7.1 Thiết Lập Thí Nghiệm

Lần training được ghi nhận sử dụng production mode trên máy tính cá nhân (Windows 10, CPU 8 nhân):

| Tham số | Giá trị |
|---------|---------|
| Population size | 120 |
| Generations (max) | 180 |
| Matches per chromosome | 20 |
| Mutation rate (ban đầu → cuối) | 10% → 3.5% |
| Mutation magnitude | σ=0.12 → 0.035 |
| Immigrant rate | 12% → 4% |
| Tournament size | k=3 → 4 → 5 |

Tổng số trận đấu simulation tối thiểu: `120 × 20 × 180 = 432.000 trận`. Thực tế cao hơn do benchmark opponents (thêm 10 trận × 120 cá thể/thế hệ).

---

### 5.7.2 Đường Cong Hội Tụ Fitness

Dữ liệu CSV log training thể hiện ba giai đoạn rõ ràng:

**Giai đoạn 1 — Khám phá (Gen 0–10): Hội tụ nhanh**

| Gen | Best | Avg | Std Dev | Babylon% | Niles% |
|:---:|:----:|:---:|:-------:|:--------:|:------:|
| 0 | 4727 | 2871 | 952 | 42.5% | 40.8% |
| 6 | **4764** | 3033 | 717 | 34.2% | 32.5% |
| 10 | 4764 | 3084 | 663 | 35.0% | 36.7% |

Best fitness đạt đỉnh 4764 ngay ở thế hệ 6 — Hall of Fame xác định được chromosome tốt nhất rất sớm. Std_dev giảm từ 952 xuống 663 (−30%), quần thể hội tụ về vùng fitness cao hơn nhưng vẫn còn đa dạng.

**Giai đoạn 2 — Tinh chỉnh (Gen 11–70): Avg tăng ổn định**

Best fitness giữ nguyên 4764 nhưng avg liên tục tăng từ ~3033 lên ~3100. `pct_other` (non-Babylon, non-Niles) tăng mạnh ở gen 20–45 (từ 19% lên đến 48%), phản ánh quần thể khám phá chiến lược generalist sau khi đã xác định xong hai extreme. Ở gen 40–45, Niles chiếm 66–70% quần thể — dấu hiệu Niles có lợi thế tự nhiên trong game.

**Giai đoạn 3 — Ổn định (Gen 71–179): Duy trì diversity**

Avg dao động quanh 2970–3100, std_dev quanh 480–650. Babylon và Niles luôn được duy trì trên 10% nhờ immigrant injection.

> **[HÌNH 5.6 — Đường Cong Fitness Qua 180 Thế Hệ]** *Biểu đồ đường: trục hoành thế hệ (0–179), trục tung điểm fitness. Ba đường: Best (đỏ đậm — plateau tại 4764 từ gen 6), Avg (xanh lam — tăng dần), Worst (xám). Trục phụ: Std Dev (vàng — giảm dần). Đánh dấu gen 6 và vùng 20–45 (pct_other spike).*

---

### 5.7.3 Diversity — Phân Phối Bộ Tộc Qua Các Thế Hệ

> **[HÌNH 5.7 — Phân Phối Tribe Qua Các Thế Hệ]** *Area chart xếp chồng: trục hoành gen 0–179, trục tung 0–100%. Ba vùng: Babylon (vàng), Niles (xanh lam), Other (xám). Thể hiện sự dao động nhưng không có bộ tộc nào bị tuyệt chủng.*

- **Babylon** dao động 6.7%–53.3%, trung bình ~30%. Không bao giờ xuống 0 nhờ elitism + immigration.
- **Niles** dao động 22.5%–70.0%, trung bình ~43%. Có xu hướng chiếm ưu thế ở mid training (gen 35–60).
- **Other** (generalist/Olympus/mixed): dao động 3.3%–68.3%. Giá trị thấp ở gen 33–47 là dấu hiệu immigration đã kích hoạt và bổ sung chromosome mới.

Std_dev cuối (~500–565) so với ban đầu (952) — giảm ~41% — cho thấy quần thể hội tụ có chủ ý mà không bị premature convergence (nếu premature, std_dev sẽ tiến về 0).

---

### 5.7.4 Profile Gene Của 5 Bot Được Chọn

| Chỉ số | hardBot | babylonBot | nileBot | summonerBot | resilientBot |
|--------|:-------:|:----------:|:-------:|:-----------:|:------------:|
| Fitness cuối | **4764** | **4764** | 4727 | — | — |
| Best score (cumulative) | 4764 | 4764 | 4764 | **8.56** | **6.13** |
| Cải thiện so với gen 0 | +37 | +37 | +0 | +0.79 (10.2%) | +0.54 (9.7%) |

*SummonerScore và ResilientScore là composite metrics, không so sánh trực tiếp với fitness thô.*

hardBot và babylonBot đều đạt fitness 4764 — được phân biệt không phải bởi chất lượng tổng thể mà bởi profile gene khác nhau (GeneDistance ≥ 0.18). Đây chính là mục tiêu thiết kế: không phải tìm bot tốt nhất duy nhất, mà tìm nhiều bot giỏi theo cách khác nhau.

> **[HÌNH 5.8 — Radar Chart 5 Bot Được Chọn]** *Biểu đồ radar 8 trục (rút gọn từ 9 nhóm gene): Stat, Keywords, Trigger, Effect, Tribe, Board, Reroll, Spell. Năm đường màu cho 5 bot. Thể hiện profile chiến lược khác biệt.*

---

### 5.7.5 Thảo Luận — Điều Gì Training Học Được?

**Niles có lợi thế tự nhiên trong game này:** Niles chiếm tỉ lệ quần thể cao hơn Babylon (~43% vs ~30%), phản ánh cơ chế Reborn + OnAllyDeath chain tạo ra sức mạnh combat tự nhiên hơn — đội hình tái sinh liên tục khó bị tiêu diệt. Đây là insight về game balance không thể thu được chỉ từ thiết kế tay.

**Best fitness hội tụ rất sớm, avg tiếp tục cải thiện:** Best không tăng sau gen 6, nhưng avg tăng từ 2871 → ~3050 (+6.2%). GA làm đúng vai trò: không chỉ tìm ra cá thể tốt nhất mà *nâng cao chất lượng trung bình toàn quần thể*.

**Std Dev giảm lành mạnh, không về 0:** Std dev giảm ~41% nhưng không tiến về 0 — island model + immigration thành công ngăn premature convergence: quần thể hội tụ về vùng fitness tốt nhưng vẫn đủ đa dạng để chọn được 5 specialist khác nhau.

**summonerBot và resilientBot tiến hóa theo thế hệ:** `best_summoner` tăng 10.2% và `best_resilient` tăng 9.7% qua 180 thế hệ — kết quả trực tiếp của seeded sub-population và elitism per-archetype.

---

*[Kết thúc Chương 5 — Tiếp theo: Chương 6 — Kết Quả Và Đánh Giá]*
