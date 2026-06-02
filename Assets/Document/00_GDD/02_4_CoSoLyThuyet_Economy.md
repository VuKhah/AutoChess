## 2.4 Lý Thuyết Kinh Tế Trong Auto Chess

### 2.4.1 Bài Toán Quyết Định Theo Tuần Tự Dưới Sự Không Chắc Chắn

Mỗi lượt trong game Auto Chess, người chơi đối mặt với một bài toán kinh tế có cấu trúc chặt chẽ: *với một ngân sách coin cố định, đưa ra tập quyết định tối ưu nhất trong một shop ngẫu nhiên, nhằm xây dựng đội hình mạnh nhất có thể cho cả trận đấu hiện tại lẫn các lượt về sau*. Thoạt nhìn đây có vẻ là một bài toán tối ưu hóa tham lam (greedy) đơn giản — cứ mua lá bài điểm cao nhất mỗi lượt. Nhưng tính khó của bài toán này nằm ở chỗ **các quyết định hiện tại có hậu quả trì hoãn**: coin tiêu lượt này là coin không có ở lượt sau, unit mua lượt này có thể cản trở việc merge lượt sau, và lá bài xuất hiện trong shop lượt này có thể sẽ không bao giờ xuất hiện lại.

Về mặt lý thuyết, đây là một dạng **bài toán quyết định tuần tự hữu hạn chân trời dưới sự không chắc chắn** (finite-horizon sequential decision problem under uncertainty). Mỗi lượt là một period với ba phần: quan sát trạng thái (board hiện tại, số coin, nội dung shop), thực hiện hành động (mua/bán/reroll/freeze), và chuyển sang trạng thái mới. Sự không chắc chắn đến từ tính ngẫu nhiên của shop: không biết lá bài nào sẽ xuất hiện lượt sau. Giải bài toán này một cách tối ưu đòi hỏi biết trước toàn bộ chuỗi shop trong tương lai — điều không thể.

Tuy nhiên, nhận biết bài toán có cấu trúc này là điều kiện để xây dựng một heuristic tốt. Phần còn lại của mục 2.4 phân tích từng khía cạnh của cấu trúc kinh tế đó, từ thiết kế thu nhập đến bài toán tempo-economy, và cuối cùng là cách GA encode một chiến lược kinh tế hoàn chỉnh trong 8 gene.

---

### 2.4.2 Cấu Trúc Thu Nhập — Thiết Kế Dòng Tiền

Điểm khởi đầu của mọi phân tích kinh tế là hiểu dòng tiền (cash flow). Đầu mỗi lượt, tài khoản coin được tính lại theo công thức:

```
CurrentCoin = 10 + bonusNextTurn + permanentIncomeBonus
bonusNextTurn ← 0   (đặt lại sau khi cộng)
```

Ba thành phần này biểu diễn ba nguồn thu nhập khác nhau về bản chất. **Base income = 10** là cố định, không tăng theo thời gian (khác với một số Auto Chess khác có cơ chế "streaks" hay "interest"). Đây là quyết định thiết kế có chủ ý: thu nhập cố định đặt mọi người chơi trên cùng một nền tảng, loại bỏ lợi thế "snowball" từ chiến lược tích lũy lãi suất. **`bonusNextTurn`** là thu nhập trì hoãn một lượt — được tích lũy từ các spell kinh tế ("nhận 3 coin đầu lượt sau") và bị xóa sau khi áp dụng. **`permanentIncomeBonus`** là thu nhập vĩnh viễn tăng thêm — một khi đã có thì cộng mỗi lượt mãi mãi. Sự phân biệt giữa bonus-one-time và bonus-permanent tạo ra hai loại spell kinh tế với độ rủi ro và phần thưởng khác nhau, và hai gene tương ứng trong chromosome (`gene[16]` cho GainCoin và `gene[31]` nhân thêm cho economy spells).

Về phía chi tiêu, có ba kênh cơ bản: **mua unit** (cost 1–3 tùy tier), **reroll** (cố định 1 coin), và **mua spell** (cost 1–3). Bán unit luôn hoàn lại đúng 1 coin bất kể giá mua ban đầu — đây là một đơn giản hóa thiết kế so với nhiều game trong thể loại, loại bỏ sự phức tạp của "sunk cost tracking" và đặt focus vào quyết định forward-looking: đội hình tốt nhất từ đây về sau là gì, không phải đội hình nào tôi đã trả tiền nhiều nhất.

---

### 2.4.3 Sự Leo Thang Tier — Cơ Chế Tăng Chất Lượng Miễn Phí

Một khác biệt quan trọng giữa thiết kế kinh tế của dự án này so với nhiều Auto Chess thương mại là **tier progression tự động**. Không có khái niệm "mua cấp độ" (level up) hay "mua XP" — shop tier tăng tự động theo công thức:

```
shopTier = clamp( ⌊(currentTurn + 1) / 2⌋, 1, 6 )
```

| Lượt | Shop Tier |
|------|-----------|
| 1–2  | Tier 1    |
| 3–4  | Tier 2    |
| 5–6  | Tier 3    |
| 7–8  | Tier 4    |
| 9–10 | Tier 5    |
| 11+  | Tier 6    |

Tiến triển này có ý nghĩa kinh tế sâu: người chơi không cần trả coin để truy cập unit mạnh hơn — họ chỉ cần sống sót đủ lâu. Điều này tạo ra một **áp lực tích cực về thời gian**: mỗi lượt trôi qua, pool unit không chỉ mạnh hơn mà còn đa dạng hơn (nhiều tier được mix vào). Bảng drop rate thay đổi theo shopTier:

| Shop Tier | T1  | T2  | T3  | T4  | T5  | T6  |
|-----------|-----|-----|-----|-----|-----|-----|
| 1         | 100%| 0%  | 0%  | 0%  | 0%  | 0%  |
| 3         | 50% | 35% | 15% | 0%  | 0%  | 0%  |
| 5         | 15% | 25% | 35% | 15% | 10% | 0%  |
| 6         | 10% | 15% | 20% | 25% | 20% | 10% |

Bảng này được thiết kế theo phân phối trọng số (weighted distribution) thay vì cutoff cứng: ở tier 5, unit tier 1 vẫn có thể xuất hiện với xác suất 15%, đảm bảo người chơi vẫn có thể tìm được bản sao để merge unit cũ dù game đã ở giai đoạn muộn. Cơ chế fallback trong `CardDatabase.GetRandomUnitShop()` xử lý trường hợp không có unit nào tại tier rolled — thay vì trả về rỗng, nó tự động lấy unit ở tier thấp hơn gần nhất, đảm bảo shop không bao giờ có ô trống do thiếu pool.

> **[HÌNH 2.14 — Drop Rate Theo Shop Tier]** *Biểu đồ thanh xếp chồng (stacked bar chart): trục hoành là Shop Tier 1–6, trục tung là tỉ lệ %, mỗi phân khúc màu là một card tier. Thể hiện sự dịch chuyển từ 100% tier 1 ở shop level 1 đến phân phối đầy đủ ở shop level 6.*

---

### 2.4.4 Đánh Đổi Tempo — Economy: Căng Thẳng Cốt Lõi

Căng thẳng kinh tế cơ bản nhất trong Auto Chess là sự đánh đổi giữa **tempo** và **economy**.

**Tempo** là sức mạnh đội hình *ngay bây giờ* — số coin bỏ ra để có unit tốt nhất có thể cho trận đấu lượt này. Mỗi coin chi tiêu vào unit là một đầu tư vào xác suất thắng lượt hiện tại. Chiến lược tempo cao đặt cược rằng thắng sớm tích lũy đủ cups trước khi đối thủ xây dựng đội hình mạnh hơn.

**Economy** là tiền dự trữ cho *tương lai* — giữ lại coin từ lượt này để có thể reroll nhiều hơn hay mua unit tier cao hơn ở lượt sau. Chiến lược economy tin rằng hy sinh một vài trận thua sớm (mất 1 HP mỗi lần) đổi lấy đội hình tốt hơn về dài hạn là phép trao đổi có lợi.

Bài toán tối ưu hóa thực sự là tìm **điểm cân bằng** giữa hai cực này, và điểm cân bằng đó thay đổi theo ngữ cảnh: nếu HP còn 7 (đầu game), hy sinh tempo để tích economy là hợp lý; nếu HP còn 1 (bờ vực thua), tempo phải được tối đa hóa ngay lập tức. Đây là lý do tại sao không có một chiến lược kinh tế nào "đúng tuyệt đối" — nó phụ thuộc vào trạng thái hiện tại của ván đấu.

> **[HÌNH 2.15 — Đánh Đổi Tempo và Economy]** *Biểu đồ 2D: trục hoành là "mức độ chi tiêu sớm" (tempo), trục tung là "xác suất thắng dự kiến". Hai đường cong cho hai ngữ cảnh khác nhau: HP cao (đầu game) và HP thấp (cuối game), thể hiện điểm tối ưu dịch chuyển theo tình huống.*

---

### 2.4.5 Reroll và Freeze — Hai Công Cụ Kiểm Soát Ngẫu Nhiên

Shop ngẫu nhiên tạo ra sự không chắc chắn, nhưng hai cơ chế cho phép người chơi *điều tiết* sự ngẫu nhiên đó: **reroll** và **freeze**.

**Reroll** (chi 1 coin, xáo lại 5 ô shop) là công cụ tìm kiếm: tôi không muốn những gì đang có, hãy thử may mắn với pool mới. Giá trị kỳ vọng của một reroll là: `E[improvement] - cost = E[better_card_score] - 1`. Nếu pool hiện tại đã chứa unit tốt hơn unit kém nhất trên board, kỳ vọng là dương và nên reroll. Tuy nhiên, reroll có chi phí ẩn: nếu sau khi reroll vẫn không tìm được gì tốt hơn, bạn đã tiêu 1 coin mà không có gì đổi lại, làm giảm khả năng mua unit ở lượt này.

Trong BotAgent, logic reroll được điều khiển bởi ba gene phối hợp:

```
Điều kiện reroll: bestShopScore < gene[24] × bestBoardScore
Số lần tối đa:    floor(gene[25] × 3) + 1   →  [1..4 lần/lượt]
Coin dự trữ:      floor(gene[26] × 4)        →  [0..4 coin giữ lại]
```

Gene[24] (`wRerollThresh`) điều chỉnh "ngưỡng bất mãn": bot có gene[24] cao sẽ reroll ngay khi shop chỉ hơi kém hơn board hiện tại (reroll aggressively); bot có gene[24] thấp chỉ reroll khi shop thực sự tệ (reroll conservatively). Gene[26] (`wRerollKeep`) là buffer an toàn — bot không reroll nếu số coin còn lại sau reroll sẽ thấp hơn ngưỡng này, tránh rơi vào tình trạng không đủ tiền mua bất cứ thứ gì dù đã tìm được unit tốt.

**Freeze** (chi 0 coin, giữ nguyên shop sang lượt sau) là công cụ đối ngược: tôi thích những gì đang thấy nhưng chưa đủ tiền để mua. Thay vì để shop bị xáo ngẫu nhiên đầu lượt sau, freeze lock lại nội dung shop. Bot quyết định freeze dựa trên sự kết hợp của hai điều kiện: bot không thích reroll nhiều (`1 - gene[24] ≥ 0.35`) *và* có ít nhất một unit trong shop đủ hấp dẫn nhưng chưa đủ tiền mua (`score ≥ gene[23] × 3` mà `cost > currentCoin`). Đây là biểu hiện của chiến lược economy: hy sinh cơ hội xem shop mới để chắc chắn giữ được cơ hội hiện tại.

---

### 2.4.6 Kinh Tế Merge — Đầu Tư Dài Hạn Cao Rủi Ro

Merge là hành động kinh tế dài hạn nhất trong game: tích lũy 3 bản sao của cùng một unit (`cardID` và `mergeLevel` giống nhau) để tạo ra một unit mạnh hơn nhiều. Sau merge, unit "nâng cấp" (`mergeLevel++`) và chỉ số được tính lại:

```
tier = mergeLevel + 1
currentATK = round( baseATK × tier  +  0.7 × (growthATKBonus + permanentATKBonus) )
maxHP      = round( baseHP  × tier  +  0.7 × (growthHPBonus  + permanentHPBonus) )
```

Với một unit thường (mergeLevel = 0, tier = 1), chỉ số chính xác là base. Sau merge một lần (mergeLevel = 1, tier = 2): ATK và HP tăng gấp đôi. Sau merge hai lần (mergeLevel = 2, tier = 3): gấp ba. Ngoài ra, hệ số `0.7` áp dụng cho growth và permanent bonus có nghĩa là mọi buff tích lũy qua các lượt cũng được tăng cường — unit đã tích lũy nhiều growth sẽ trở nên cực kỳ mạnh sau merge.

Chi phí kinh tế của merge không chỉ là coin để mua 3 bản sao mà còn là **chi phí cơ hội**: 3 slot trong shop và board được dùng cho cùng một card ID thay vì 3 card khác nhau, và 3 unit cost 1–3 coin mỗi cái thay vì một unit tier cao hơn. Quyết định "có nên theo đuổi merge không" phụ thuộc vào xác suất tìm được bản sao thứ 3 trong pool, giá trị của unit đó khi được merge so với các unit tier cao hơn, và số lượt còn lại để tận dụng sức mạnh sau merge. Gene[21] (`wMerge`) trong chromosome mã hóa heuristic này: bot sẽ đánh giá điểm của một unit trong shop cao hơn đáng kể nếu đã có bản sao tương ứng trên board, kích thích chiến lược merge:

```
mergeBonus = copies × gene[21] × (copies == 2 ? 16 : 8)
```

Nếu đã có 2 bản sao (chỉ thiếu 1 nữa để merge), bonus tăng gấp đôi — phản ánh tính cấp thiết của việc hoàn thành bộ 3 so với việc mới bắt đầu tích lũy.

---

### 2.4.7 Sell — Tái Phân Bổ Nguồn Lực

Bán unit (`Sell`) là cơ chế tái phân bổ nguồn lực: loại bỏ một unit kém hơn để lấy lại coin và slot cho unit tốt hơn. Mọi unit khi bán đều trả lại đúng 1 coin bất kể giá mua ban đầu hay mergeLevel — thiết kế này không cần người chơi theo dõi "đã bỏ ra bao nhiêu cho unit này", mà chỉ cần hỏi: *unit mới có đáng hơn unit cũ cộng với 1 coin không*?

BotAgent có hai thời điểm bán unit. **Reactive sell** xảy ra trong Phase 2 (BuyUnitsPhase) khi board đã đầy: nếu muốn mua unit mới nhưng không còn slot, bot so sánh unit mới với unit yếu nhất trên board. Unit mới chỉ được mua nếu điểm của nó cao hơn đáng kể:

```
score(new) > worstBoardScore × (1.5 + gene[23])
```

Hệ số `1.5 + gene[23]` (luôn > 1.5) tạo ra một "ngưỡng trao đổi có lợi rõ ràng" — bot không thay thế unit cũ trừ khi unit mới thực sự tốt hơn nhiều, tránh churning (bán đi mua lại liên tục mà không tạo ra giá trị thực).

**Proactive sell** xảy ra trong Phase 4: ngay cả khi board chưa đầy, bot chủ động bán những unit quá kém:

```
if (EvaluateInstance(unit) < gene[27] × 3)  →  Sell
```

Gene[27] (`wProactiveSell`) là ngưỡng chịu đựng: bot có gene[27] cao sẽ dễ dàng bán unit đang có để giải phóng slot và coin; bot có gene[27] thấp (≤ 0.05 tức là gần 0) không bao giờ bán chủ động, thích giữ đội hình đầy đủ dù một số unit yếu. Điểm thú vị là summonerBot — archetype phụ thuộc vào việc giữ lại "shell units" để SummonConsumed — được seed với `gene[27] ∈ [0.00, 0.15]` rất thấp, phản ánh đúng hành vi cần thiết: không bán unit ngay cả khi chúng có vẻ yếu, vì chúng đóng vai trò "container" trong chuỗi triệu hồi.

---

### 2.4.8 Kinh Tế Spell — Tầng Meta-Economy

Spell là loại card không chiến đấu, tác động lên unit hoặc nền kinh tế thay vì tham gia combat trực tiếp. Trong bức tranh kinh tế tổng thể, spell tạo ra một tầng thứ hai — *meta-economy*: thay vì chỉ mua unit để mạnh hơn, có thể đầu tư vào công cụ làm cho quá trình mua unit sau này hiệu quả hơn.

Các spell kinh tế trực tiếp (`GainCoin`, `GainIncome`) tạo ra coin ngay lập tức hoặc tăng thu nhập lâu dài, cho phép bot có nhiều coin hơn mỗi lượt. Spell gián tiếp như `GetRandomUnit`, `StealFromShop`, `GetUnitAtNextTier` không tạo ra coin nhưng mang lại unit tốt hơn mức giá thị trường thông thường — đây là "arbitrage": mua thứ trị giá 3 coin bằng cách dùng spell giá 2 coin. Spell ngoại lệ như `LoseLife` (mất HP để đổi lấy lợi ích khác) hay `TransferStats` (hy sinh một unit để buff unit khác) là những công cụ rủi ro cao, được phản ánh bằng điểm âm cứng trong `EvaluateSpell()`:

```csharp
case 14: // LoseLife
    return -25f;   // phạt nặng — không bao giờ mua trừ khi cực kỳ cần
case 15: // TransferStats
    return -8f;    // rủi ro — phạt trung bình
```

BotAgent đánh giá spell qua hàm `EvaluateSpell()` — một hàm tính điểm riêng biệt với `Evaluate()` dành cho unit, sử dụng 4 gene riêng (gene[28–31]). Điều này quan trọng vì spell và unit không so sánh được trực tiếp: spell không có ATK hay HP, không có tribe, không merge được. Thay vào đó, từng loại spell được đánh giá theo logic riêng nhân với các gene liên quan. Ví dụ `GainIncome` — spell tăng thu nhập vĩnh viễn — được tính:

```
score = value × gene[16] × gene[31] × (isPermanent ? 12 : 1.5)
```

Hệ số 12 cho permanent income (vs 1.5 cho one-time) phản ánh giá trị tương lai: với 20 lượt tối đa, mỗi coin thu nhập thêm mỗi lượt tích lũy lên đến 20 coin, xứng đáng đánh giá cao hơn nhiều so với nhận 1 coin ngay lập tức.

---

### 2.4.9 Hàm Fitness Như Mô Hình Kinh Tế Hoàn Chỉnh

Một điểm thú vị ít được chú ý là **hàm fitness trong GameSimulator cũng là một mô hình kinh tế** — nó không chỉ đo "thắng hay thua" mà đo "thắng như thế nào":

```csharp
// GameSimulator.ScoreFromA()
float score = result > 0 ? 100f : result == 0 ? 25f : 0f;
score += (hpA - hpB) * 4f;
if (result > 0)
    score += (MaxTurns - turns) * 2f;   // thắng nhanh hơn → điểm cao hơn
```

Ba thành phần của công thức này thực chất encode ba mục tiêu kinh tế khác nhau. Thành phần đầu (`100f` khi thắng) là reward tuyệt đối cho chiến thắng. Thành phần `(hpA - hpB) × 4` là reward tương đối cho **biên chiến thắng** — thắng với HP còn nhiều hơn đối thủ nghĩa là chiến lược đang hoạt động hiệu quả, không chỉ may mắn. Thành phần `(MaxTurns - turns) × 2` là reward cho **tốc độ** — thắng ở lượt 8 được nhiều điểm hơn thắng ở lượt 18.

Thành phần thứ ba ngầm khuyến khích chiến lược **tempo**: chi tiêu coin sớm để xây dựng đội hình mạnh từ đầu, từ đó áp đảo đối thủ và kết thúc ván đấu nhanh. Một bot tiết kiệm quá mức (economy-heavy) có thể thắng nhưng thắng muộn, nhận điểm thấp hơn một bot balanced. Qua nhiều thế hệ evolution, áp lực fitness này "dạy" chromosome học được điểm cân bằng tempo-economy mà không cần người thiết kế chỉ định cụ thể — đây là một ví dụ điển hình về **reward shaping**: thiết kế hàm fitness cẩn thận để behavior mong muốn nổi lên tự nhiên từ quá trình tối ưu hóa.

> **[HÌNH 2.16 — Cấu Trúc Hàm Fitness]** *Biểu đồ tròn hoặc thanh dọc chia nhỏ điểm fitness: phần thắng/thua (100/0/25 điểm), phần biên thắng (hpA−hpB × 4), phần tốc độ (MaxTurns−turns × 2). Kèm ví dụ số cụ thể cho hai kịch bản: thắng nhanh vs. thắng muộn.*

---

### 2.4.10 Tổng Hợp — Kinh Tế Học Như Ngôn Ngữ Của Chromosome

Nhìn lại 8 gene kinh tế trong chromosome (gene[23–31]) qua lăng kính của toàn bộ mục này, có thể thấy mỗi gene thực chất là một tham số của một policy kinh tế cụ thể:

| Gene | Tên | Câu hỏi kinh tế mà nó trả lời |
|------|-----|---------------------------------|
| [23] | wSaveThreshold | Điểm tối thiểu để một lá bài "đáng mua"? |
| [24] | wRerollThresh | Khi nào shop đủ tệ để đáng trả 1 coin xem cái khác? |
| [25] | wRerollMax | Tối đa bao nhiêu lần "thử vận" mỗi lượt? |
| [26] | wRerollKeep | Giữ lại bao nhiêu coin như buffer an toàn? |
| [27] | wProactiveSell | Khi nào unit kém đến mức đáng bán để lấy slot và 1 coin? |
| [28] | wSpellThresh | Điểm tối thiểu để spell "đáng mua"? |
| [29] | wSpellOnStrong | Ưu tiên buff unit mạnh nhất hay phân bổ đều? |
| [31] | wSpellEconomy | Coi trọng công cụ tạo coin bao nhiêu? |

GA không được lập trình để hiểu "đây là bài toán kinh tế" hay "cần cân bằng tempo và economy". Nó chỉ đơn giản tìm kiếm bộ tám số thực trong [0,1]⁸ sao cho fitness cao nhất. Nhưng vì hàm fitness được thiết kế để phản ánh đúng "chơi tốt" trong game Auto Chess — thắng nhanh, thắng chắc, không thua do quản lý coin kém — chromosome tốt nhất sẽ tự nhiên hội tụ về một chính sách kinh tế hợp lý. Đây là sức mạnh và cũng là giới hạn của GA: nó tìm được giải pháp tốt mà không hiểu tại sao nó tốt.

Với nền tảng lý thuyết từ bốn mục trong Chương 2 — Genetic Algorithm, Unity Engine, TTE pattern, và kinh tế học Auto Chess — các chương tiếp theo có thể bước vào mô tả thiết kế thực tế của dự án với đầy đủ ngữ cảnh để giải thích mọi quyết định kỹ thuật và thiết kế.

---

*[Kết thúc Chương 2 — Tiếp theo: Chương 3 — Thiết Kế Game (Game Design Document)]*
