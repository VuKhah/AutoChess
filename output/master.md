---
lang: vi
toc: true
toc-depth: 3
---



# MỞ ĐẦU: GIỚI THIỆU ĐỀ TÀI

---

## 1. Lý Do Chọn Đề Tài

Trong thập kỷ qua, sự giao thoa giữa trí tuệ nhân tạo và thiết kế game đã tạo ra những bước đột phá đáng chú ý. Từ DeepMind's AlphaStar chinh phục StarCraft II (2019) đến OpenAI Five vô địch Dota 2, máy móc ngày càng chứng tỏ khả năng làm chủ các trò chơi chiến lược phức tạp vốn từng được xem là đặc quyền của con người. Tuy nhiên, phần lớn các thành tựu đó đều dựa trên kỹ thuật Học Tăng cường (Reinforcement Learning) — một phương pháp đòi hỏi tài nguyên tính toán khổng lồ và dữ liệu huấn luyện cực kỳ lớn, nằm ngoài tầm tiếp cận của các nhóm nghiên cứu nhỏ hay sinh viên ngành Công nghệ Thông tin.

Bên cạnh đó, thể loại **Auto Chess** — dòng game chiến thuật theo lượt nơi người chơi xây dựng đội hình và để chúng tự động chiến đấu — đang là một trong những thể loại phát triển nhanh nhất trong ngành game hiện đại. Ra đời từ một bản mod của Dota 2 vào đầu năm 2019, Auto Chess nhanh chóng truyền cảm hứng cho hàng loạt tựa game thương mại lớn: *Teamfight Tactics* (Riot Games), *Hearthstone Battlegrounds* (Blizzard) và *Dota Underlords* (Valve).

> **[HÌNH 0.1 — Thể Loại Auto Chess Thương Mại]** *Ảnh chụp màn hình của Teamfight Tactics, Hearthstone Battlegrounds và Dota Underlords đặt cạnh nhau, minh họa sự phổ biến và đa dạng của thể loại Auto Chess.*

Sức hút của thể loại này đến từ sự kết hợp độc đáo giữa chiến lược dài hạn, quản lý kinh tế và yếu tố ngẫu nhiên có kiểm soát — tạo ra một không gian trạng thái cực kỳ phong phú và đầy thử thách cho cả con người lẫn máy.

Chính sự phong phú đó cũng là lý do mà việc xây dựng AI cho Auto Chess trở thành một bài toán nghiên cứu thú vị nhưng chưa được khai thác sâu ở cấp độ học thuật Việt Nam. Hầu hết các bot AI trong game thương mại hiện tại đều sử dụng **rule-based logic** — tức là tập hợp các quy tắc if-else cứng do lập trình viên viết tay — phương pháp này nhanh nhưng thiếu linh hoạt và không thể tự thích nghi hay học từ kinh nghiệm.

Đề tài này được hình thành từ câu hỏi: *Liệu có thể xây dựng một hệ thống AI cho game Auto Chess mà không cần viết một dòng quy tắc cứng nào, chỉ dựa thuần túy vào quá trình tiến hóa tự nhiên?* Câu trả lời chúng tôi đề xuất là **Thuật toán Di truyền (Genetic Algorithm — GA)** — một phương pháp tối ưu hóa lấy cảm hứng từ sinh học tiến hóa, nhẹ về tài nguyên tính toán, không cần dữ liệu gán nhãn, và đặc biệt phù hợp với bài toán tìm kiếm chiến lược trong không gian tham số liên tục.

---

## 2. Mục Tiêu Đề Tài

Đề tài đặt ra ba mục tiêu cụ thể, độc lập nhưng bổ trợ cho nhau:

**Mục tiêu 1 — Xây dựng game Auto Chess hoàn chỉnh:**
Thiết kế và lập trình một tựa game Auto Chess đầy đủ tính năng trên nền tảng Unity Engine, bao gồm: hệ thống combat tự động, kinh tế shop, cơ chế merge (ghép bài nâng cấp), khả năng đặc biệt của từng đơn vị (ability), và ba bộ tộc (tribe) với hiệu ứng cộng hưởng riêng biệt. Game phải đạt mức *playable* — tức là một người chơi bình thường có thể ngồi xuống, hiểu luật và trải nghiệm đầy đủ vòng lặp gameplay mà không cần hướng dẫn ngoài.

**Mục tiêu 2 — Thiết kế hệ thống AI thuần túy bằng GA:**
Biểu diễn toàn bộ "tư duy chiến lược" của một bot AI dưới dạng một Nhiễm sắc thể (Chromosome) gồm 37 gene thực số, không có bất kỳ logic if-else nào được hardcode. Mọi quyết định của bot — từ việc đánh giá giá trị một lá bài, quyết định khi nào nên reroll shop, ưu tiên bộ tộc nào, sắp xếp đội hình ra sao — đều phải có thể truy xuất ngược về một hoặc nhiều gene cụ thể trong chromosome.

**Mục tiêu 3 — Huấn luyện ra nhiều archetype bot khác biệt:**
Không chỉ tìm ra một bot "tốt nhất" duy nhất, hệ thống GA phải có khả năng tiến hóa ra nhiều phong cách chơi (playstyle) khác nhau — bot tấn công mạnh mẽ, bot phòng thủ kiên cường, bot triệu hồi liên hoàn — để game có chiều sâu chiến lược và tính tái chơi cao.

---

## 3. Phạm Vi Đề Tài

Để đảm bảo tính khả thi và chiều sâu kỹ thuật, đề tài được giới hạn trong phạm vi sau:

**Về nền tảng kỹ thuật:**
- Ngôn ngữ lập trình: C# trên Unity Engine 2022+
- Không sử dụng Unity ML-Agents, TensorFlow, PyTorch hay bất kỳ framework học máy nào
- Toàn bộ hệ thống AI được viết từ đầu (from scratch) trong C# thuần

**Về thiết kế game:**
- Số lượng lá bài: 68 card (bao gồm unit và spell)
- Ba bộ tộc: Babylon, Olympus, Niles — mỗi bộ tộc có hiệu ứng cộng hưởng riêng
- Chế độ chơi: người chơi đơn (PvE — player vs. AI bot), không có multiplayer
- Điều kiện thắng: đạt 10 điểm cup; điều kiện thua: HP về 0

**Về hệ thống AI:**
- Chromosome: mảng 37 gene thực số, mỗi giá trị trong đoạn [0, 1]
- Training hoàn toàn headless — không cần mở Unity Editor hay scene game
- Kết quả training: 5 bot chuyên biệt với gene profile khác nhau, lưu trong file JSON
- Thời gian training: từ 2 phút (quick mode) đến 20 phút (production mode)

**Ngoài phạm vi:**
- Chế độ PvP online hoặc local multiplayer
- Neural Network hay Reinforcement Learning
- Cân bằng game theo kiểu competitive (esports-level balancing)
- Mobile hoặc WebGL build

---

## 4. Phương Pháp Nghiên Cứu

Đề tài kết hợp ba phương pháp song song xuyên suốt quá trình thực hiện:

**Nghiên cứu lý thuyết:**
Tổng hợp kiến thức về Thuật toán Di truyền từ các tài liệu nền tảng (Holland 1975, Goldberg 1989) và các ứng dụng hiện đại trong game AI. Phân tích thiết kế của các tựa Auto Chess thương mại (Teamfight Tactics, Hearthstone Battlegrounds) để xác định các cơ chế cốt lõi cần tái hiện.

**Thiết kế và lập trình iterative:**
Áp dụng chu trình phát triển lặp (iterative development): thiết kế một tính năng nhỏ → lập trình → kiểm thử → tinh chỉnh → chuyển sang tính năng tiếp theo. Toàn bộ logic game được kiểm chứng bằng cách chạy thực tế, không phụ thuộc vào test framework tự động.

**Thực nghiệm và đánh giá:**
Chạy các lần training với tham số khác nhau, đo độ hội tụ (convergence) của fitness qua các thế hệ, so sánh win rate giữa các bot được train theo chiến lược khác nhau, và đánh giá chất lượng gameplay thông qua trải nghiệm thực tế.

---

## 5. Đóng Góp Của Đề Tài

Mặc dù đây là một đề tài ở cấp độ tiểu luận chuyên ngành, chúng tôi cho rằng dự án có một số đóng góp kỹ thuật đáng ghi nhận:

**Về thiết kế hệ thống ability (TTE Engine):**
Toàn bộ khả năng đặc biệt của 68 lá bài được biểu diễn theo mô hình **Trigger → Target → Effect** với 14 loại trigger, 12 loại target và 13 loại effect — cho phép tạo ra vô số tổ hợp khả năng phức tạp mà không cần viết code riêng cho từng lá bài. Đây là kiến trúc data-driven thực sự: thêm một lá bài mới chỉ cần sửa file JSON, không cần chạm vào code.

**Về thiết kế Chromosome 37-gene:**
Chromosome được thiết kế để bao phủ toàn diện không gian quyết định của một người chơi Auto Chess: từ đánh giá chỉ số thuần túy (ATK, HP), đánh giá khả năng đặc biệt (trigger/effect weights), nhận diện cộng hưởng bộ tộc, quản lý kinh tế (reroll threshold, coin reserve), đến hành vi vĩ mô (spell priority, board positioning). Đây là một trong những chromosome có độ bao phủ hành vi cao nhất được ứng dụng vào thể loại game này theo hiểu biết của nhóm nghiên cứu.

**Về kiến trúc headless training:**
Hệ thống AI được thiết kế hoàn toàn độc lập với Unity MonoBehaviour, cho phép chạy hàng nghìn trận đấu mô phỏng trong batch mode mà không cần khởi động scene hay giao diện đồ họa. Điều này giúp rút ngắn thời gian training từ vài giờ xuống còn vài phút, ngay trên máy tính cá nhân thông thường.

---

## 6. Cấu Trúc Báo Cáo

Tiểu luận được tổ chức thành sáu chương chính, logic nối tiếp nhau theo hướng từ lý thuyết đến thực hành:

**Chương 1 — Tổng quan lĩnh vực:**
Trình bày lịch sử và đặc trưng của thể loại Auto Chess, tổng quan các hướng tiếp cận AI trong game chiến lược, và các công trình liên quan đến GA trong game AI.

**Chương 2 — Cơ sở lý thuyết:**
Trình bày nền tảng toán học và thuật toán của Genetic Algorithm (quần thể, chọn lọc, lai ghép, đột biến, hội tụ), kiến trúc Unity Engine và mô hình lập trình game, thiết kế hệ thống ability theo mô hình TTE, và lý thuyết kinh tế trong Auto Chess.

**Chương 3 — Thiết kế game (Game Design Document):**
Mô tả chi tiết toàn bộ gameplay: vòng lặp cốt lõi, hệ thống lá bài, cơ chế merge, shop, kinh tế, chiến đấu và cân bằng. Chương này đóng vai trò tài liệu thiết kế đầy đủ cho phần game.

**Chương 4 — Kiến trúc hệ thống kỹ thuật:**
Phân tích kiến trúc phần mềm: Data Layer (JSON-driven), Ability Engine (TTE), Combat Engine (Death Stack), Manager Layer, UI và cơ chế headless training.

**Chương 5 — Thuật toán Di truyền và hệ thống AI:**
Đây là chương trọng tâm. Trình bày thiết kế Chromosome 37-gene, BotAgent với 7-phase decision pipeline, GameSimulator, vòng lặp GATrainer, kết quả training và so sánh phong cách chơi giữa 5 bot chuyên biệt.

**Chương 6 — Kết quả và đánh giá:**
Demo game, đánh giá tính hoàn chỉnh của hệ thống mechanic, hiệu năng, chất lượng AI, và thảo luận về hạn chế cùng hướng cải thiện.

**Kết luận** tổng kết đóng góp và phác thảo hướng phát triển tương lai cho dự án.

---

*Toàn bộ mã nguồn của dự án được phát triển và lưu trữ trong repository riêng. Các số liệu, tham số kỹ thuật và kết quả thực nghiệm được trình bày trong báo cáo này đều được trích xuất trực tiếp từ source code và log training, không có sự ước lượng hay làm tròn tùy tiện.*

\newpage

# CHƯƠNG 1: TỔNG QUAN LĨNH VỰC

---

## 1.1 Lịch Sử Và Tổng Quan Thể Loại Auto Chess

### 1.1.1 Nguồn Gốc — Từ Bản Mod Đến Thể Loại Độc Lập

Vào tháng 1 năm 2019, một nhóm nhỏ nhà phát triển người Trung Quốc mang tên **Drodo Studio** phát hành một bản mod miễn phí cho tựa game *Dota 2* với cái tên đơn giản: *Dota Auto Chess*. Trong vòng vài tuần, bản mod này thu hút hơn một triệu người chơi — một con số chưa từng có trong lịch sử các bản mod game. Đỉnh điểm đồng thời (peak concurrent users) vượt 280.000 người, gần bằng chính tựa game Dota 2 gốc tại cùng thời điểm. Điều khiến sự bùng nổ này đáng kinh ngạc là nó xảy ra hoàn toàn không có marketing, không có trailer, không có kênh quảng bá chính thức — chỉ là truyền miệng trong cộng đồng game thủ.

Thành công vượt sức tưởng tượng đó không thoát khỏi tầm nhìn của các hãng game lớn. Trong vòng sáu tháng, ba tựa game thương mại được phát triển dựa trên cùng khái niệm lần lượt ra đời: **Teamfight Tactics** (Riot Games, tháng 6/2019), **Dota Underlords** (Valve, tháng 6/2019) và **Hearthstone Battlegrounds** (Blizzard, tháng 11/2019). Các tựa game này không đơn thuần là bản sao — chúng tái diễn giải cơ chế cốt lõi của Auto Chess qua lăng kính universe riêng của mình, bổ sung thêm chiều sâu và hoàn thiện trải nghiệm ở nhiều khía cạnh. Đến năm 2020, **Auto Chess** đã được công nhận là một **thể loại game độc lập** (standalone genre), không còn được xem là nhánh phụ của bất kỳ thể loại nào.

> **[HÌNH 1.1 — Timeline Hình Thành Thể Loại Auto Chess]** *Đường thời gian từ tháng 1/2019 (Dota Auto Chess mod) đến cuối 2019 (Teamfight Tactics, Underlords, Hearthstone Battlegrounds), kèm số liệu người chơi đỉnh điểm của từng tựa game. Thể hiện tốc độ bùng nổ của thể loại.*

Câu hỏi đặt ra là: *điều gì trong Dota Auto Chess đã tạo ra sức hút lớn đến vậy?* Nhìn lại, bản mod đã chạm được vào một khoảng trống tâm lý đặc biệt trong cộng đồng game thủ: sự kết hợp giữa **chiều sâu chiến lược** của game chiến thuật (xây dựng đội hình, quản lý nguồn lực, nhận diện synergy) và **tốc độ vừa phải** không yêu cầu phản xạ tay hay kỹ năng micro-management như RTS. Người chơi có thể dừng lại, suy nghĩ, tham khảo, và vẫn hoàn toàn cạnh tranh được — một điều không thể trong StarCraft hay Dota 2 truyền thống.

---

### 1.1.2 Đặc Trưng Cốt Lõi Của Thể Loại

Dù mỗi tựa Auto Chess có universe và cơ chế riêng, tất cả đều chia sẻ một bộ **đặc trưng cốt lõi** nhất định — những đặc trưng định nghĩa thể loại và phân biệt nó với mọi thứ trước đó:

**Draft và chuẩn bị (Preparation Phase):** Người chơi không trực tiếp tạo ra đội hình từ đầu mà phải *chọn lọc* từ một pool bài ngẫu nhiên mỗi lượt (shop). Đây là sự kết hợp giữa yếu tố ngẫu nhiên (shop bài gì) và quyết định chiến lược (mua cái gì, giữ lại cái gì, bán cái gì). Không có ván nào giống ván nào vì pool ngẫu nhiên đảm bảo đội hình tối ưu không bao giờ cố định.

**Triển khai và sắp xếp (Placement):** Sau khi mua bài, người chơi sắp xếp chúng lên sân. Vị trí của từng unit không phải ngẫu nhiên — nó ảnh hưởng đến ai tấn công ai, unit nào bị tiêu diệt trước, ability nào có thể trigger đúng lúc. Đây là tầng chiến thuật vi mô nằm trong chiến lược vĩ mô của toàn ván đấu.

**Chiến đấu tự động (Auto-Combat):** Khi hai đội đối mặt nhau, người chơi không can thiệp được gì nữa — các unit tự tìm mục tiêu và chiến đấu theo quy tắc đã được lập trình. Kết quả phụ thuộc hoàn toàn vào chất lượng chuẩn bị, không vào tốc độ bấm hay phản xạ. Đây là điểm phân biệt cơ bản nhất của thể loại.

**Quản lý kinh tế (Economy Management):** Mỗi lượt người chơi nhận một lượng tiền cố định và phải quyết định: chi tiêu hết để có đội hình mạnh nhất ngay lập tức (tempo), hay giữ lại để có nhiều lựa chọn hơn ở lượt sau (economy)? Cân bằng giữa hai cực này là thách thức kinh tế liên tục xuyên suốt ván đấu.

**Cộng hưởng tổ hợp (Synergy Building):** Các unit không mạnh đơn lẻ theo đường thẳng — chúng tạo ra giá trị *bổ sung* khi kết hợp với nhau theo những cách nhất định (cùng bộ tộc, cùng class, hoặc ability bổ trợ lẫn nhau). Nhận ra và khai thác các tổ hợp synergy là kỹ năng chiều sâu của thể loại.

**Tiến triển theo thời gian (Progression):** Qua các lượt, pool bài trở nên phong phú hơn, unit mạnh hơn có thể xuất hiện, cơ chế merge/upgrade cho phép đội hình phát triển về chất lượng. Có một "arc" cho mỗi ván đấu — early game, mid game, late game — với chiến lược tối ưu khác nhau ở từng giai đoạn.

---

### 1.1.3 Phân Biệt Với Các Thể Loại Lân Cận

Auto Chess dễ bị nhầm lẫn với các thể loại game chiến lược khác vì nó mượn yếu tố từ nhiều nguồn. Sự phân biệt rõ ràng giúp hiểu chính xác bài toán thiết kế và AI mà đề tài này đang giải quyết:

| Tiêu chí | Auto Chess | Card Game (Hearthstone) | Tower Defense | RTS (StarCraft) |
|----------|-----------|------------------------|---------------|-----------------|
| Điều khiển trong combat | Không | Có (chọn target) | Không (tower tự bắn) | Có (micro-management) |
| Yếu tố ngẫu nhiên | Shop ngẫu nhiên | Rút bài ngẫu nhiên | Ít | Ít |
| Kỹ năng tay | Không cần | Không cần | Không cần | Rất cao |
| Quản lý kinh tế | Coin mỗi lượt | Mana theo lượt | Coin theo thời gian | Mineral/Vespene |
| Chiều sâu chiến lược | Cao (synergy) | Cao (deck building) | Trung bình | Rất cao (build order) |
| Thời gian mỗi ván | 15–30 phút | 10–20 phút | 20–40 phút | 10–60 phút |

> **[HÌNH 1.2 — So Sánh Auto Chess Với Các Thể Loại Lân Cận]** *Biểu đồ radar (spider chart) 5 trục: Độ phức tạp chiến lược, Yêu cầu kỹ năng tay, Yếu tố may mắn, Quản lý kinh tế, Chiều sâu combo. Bốn đường màu cho Auto Chess, Card Game, Tower Defense, RTS. Thể hiện vị trí độc đáo của Auto Chess.*

**So với Card Game:** Trong Hearthstone hay Magic: The Gathering, người chơi xây dựng deck trước ván đấu và sử dụng bài trong ván. Auto Chess không có khái niệm "deck" — đội hình được xây dựng *trong* ván từ pool ngẫu nhiên. Điều này làm cho mỗi ván Auto Chess là một trải nghiệm mới hoàn toàn, trong khi Card Game có xu hướng lặp lại chiến lược của deck qua nhiều ván.

**So với Tower Defense:** Cả hai đều có chiến đấu tự động, nhưng Tower Defense là chiến đấu một chiều — làn sóng địch đến theo đường cố định, người chơi đặt tower để ngăn chặn. Auto Chess là đối xứng — hai đội hình tương đương nhau đối mặt trên cùng sân, và ai thắng phụ thuộc vào chất lượng đội hình *tương đối*, không phải số lượng tower hay tốc độ xây dựng.

**So với RTS:** RTS đòi hỏi micro-management trong thời gian thực — người chơi phải ra lệnh cho từng unit, quản lý nhiều nhóm quân cùng lúc, xây dựng cơ sở kinh tế song song với chiến đấu. Auto Chess loại bỏ hoàn toàn yếu tố thời gian thực — mọi quyết định đều được đưa ra trong thời gian không giới hạn trong Shop Phase, và combat hoàn toàn tự động. Kết quả là Auto Chess thử thách khả năng *phán đoán và lập kế hoạch* thay vì *tốc độ thực thi*.

Sự khác biệt này có ý nghĩa trực tiếp đến bài toán AI: khác với RTS nơi AI cần phản xạ nhanh và micro-management liên tục, AI cho Auto Chess chỉ cần đưa ra *một tập quyết định tốt* trong mỗi Shop Phase — một không gian quyết định rời rạc, hữu hạn, và không phụ thuộc vào thời gian thực. Đây là lý do GA — một thuật toán batch optimization không có khái niệm "thời gian thực" — là lựa chọn phù hợp tự nhiên.

---

## 1.2 AI Trong Game Chiến Lược

### 1.2.1 Phương Pháp Truyền Thống — Rule-Based Và Cây Quyết Định

Trong nhiều thập kỷ, **rule-based AI** (AI dựa trên quy tắc) là cách tiếp cận phổ biến nhất để xây dựng đối thủ trong game chiến lược. Ý tưởng cơ bản là đơn giản: nhà thiết kế viết tay một tập hợp các quy tắc if-else mô tả hành vi của AI trong từng tình huống. Một bot đơn giản có thể trông như sau:

```
Nếu (coin >= 3 VÀ có unit tốt trong shop):
    → Mua unit
Ngược lại nếu (shop tier == 1 VÀ có unit trùng trong shop):
    → Mua để merge
Ngược lại:
    → Tiết kiệm coin
```

Rule-based AI có ưu điểm rõ ràng về mặt kỹ thuật: dễ cài đặt, dễ debug, hành vi hoàn toàn dự đoán được và có thể giải thích. Trong nhiều thể loại game, nó hoạt động tốt — ví dụ, AI trong game platformer hay shooter đơn giản cần có script không quá phức tạp để vẫn tạo ra trải nghiệm thú vị.

Tuy nhiên, rule-based AI có giới hạn cơ bản: **quy tắc là cứng nhắc**. Người viết phải dự đoán trước mọi tình huống có thể xảy ra, và số lượng tình huống trong game chiến lược phức tạp tăng theo hàm mũ. Một bot Auto Chess cần xử lý: hàng chục unit trong shop, hàng trăm tổ hợp synergy, trạng thái kinh tế biến đổi, đội hình đối thủ thay đổi theo lượt, và tất cả những yếu tố này cần được cân nhắc *cùng lúc*. Viết quy tắc đủ bao phủ cho không gian đó là bất khả thi về mặt thực tiễn, và ngay cả khi viết được, bot sẽ dễ đoán và dễ khai thác bởi người chơi có kinh nghiệm.

**Decision Tree** (cây quyết định) là bước cải tiến so với rule-based thuần túy: thay vì if-else tuyến tính, quyết định được tổ chức thành cây nhị phân với các nút kiểm tra điều kiện và lá là hành động. Cây quyết định dễ đọc hơn và có thể được *học* từ dữ liệu (ID3, C4.5, Random Forest), nhưng vẫn bị giới hạn ở tính rời rạc — mỗi nút phải đặt một ngưỡng cứng ("HP > 50 hay không?"), trong khi thực tế chiến lược tối ưu thường là một hàm liên tục, mượt mà của nhiều biến số.

**Minimax với Alpha-Beta Pruning** — thuật toán kinh điển cho chess, Go, và checkers — tìm nước đi tối ưu bằng cách xây dựng cây tất cả các trạng thái có thể và chọn nhánh tối ưu theo chiến lược minimax. Thuật toán này hoạt động xuất sắc trong các game có không gian trạng thái hữu hạn, hai người chơi, thông tin đầy đủ và kết quả nhị phân (thắng/thua). Auto Chess vi phạm cả bốn điều kiện đó: không gian trạng thái gần như vô hạn (tổ hợp đội hình, shop ngẫu nhiên), không gian hành động liên tục, thông tin không đầy đủ (không biết đối thủ sẽ mua gì), và kết quả không nhị phân ngay sau mỗi bước.

---

### 1.2.2 Xu Hướng Hiện Đại — Reinforcement Learning

Cuộc cách mạng thực sự trong AI game đến vào nửa sau thập niên 2010 với sự trỗi dậy của **Reinforcement Learning** (RL) kết hợp Deep Neural Network, mà cộng đồng thường gọi là **Deep RL**. Thay vì quy tắc hay cây, RL học cách hành xử qua *kinh nghiệm tương tác*: agent thực hiện hành động trong môi trường, nhận reward từ kết quả, và cập nhật policy (chính sách hành động) để tối đa hóa tổng reward tích lũy theo thời gian.

Thành công đỉnh cao của hướng tiếp cận này được đánh dấu bởi hai dự án của DeepMind và OpenAI:

**AlphaGo và AlphaZero (DeepMind, 2016–2018):** Lần đầu tiên trong lịch sử, AI đánh bại kiện tướng cờ vây thế giới — điều mà giới khoa học từng cho là sẽ không xảy ra trong hàng thập kỷ. AlphaZero sau đó tự học từ đầu (không cần dữ liệu ván cờ của con người) và đạt siêu nhân cho ba game cờ bảng: Go, Chess và Shogi.

**OpenAI Five (OpenAI, 2019):** Hệ thống gồm 5 agent RL tập hợp thành đội, học chơi Dota 2 ở cấp độ professional và đánh bại đội tuyển thế giới OG tại trận đấu lịch sử. Để đạt được điều này, hệ thống đã chơi khoảng 180 năm gameplay *mỗi ngày* thông qua self-play song song trên hàng nghìn CPU.

**AlphaStar (DeepMind, 2019):** Thắng kiện tướng chuyên nghiệp trong StarCraft II — một game RTS thời gian thực đòi hỏi micro-management cực kỳ phức tạp.

Những thành tựu này đặt ra một tiêu chuẩn mới và cũng đặt ra câu hỏi tự nhiên: *liệu RL có thể áp dụng cho Auto Chess không?*

---

### 1.2.3 Hạn Chế Của Reinforcement Learning Trong Auto Chess

Mặc dù những thành công nêu trên rất ấn tượng, RL trong dạng thức hiện tại gặp phải những khó khăn cơ bản khi áp dụng vào thể loại Auto Chess — không phải vì lý do ngẫu nhiên, mà vì bản chất cấu trúc của bài toán.

**Thứ nhất — không gian trạng thái và hành động quá lớn:**

Trong Go hay Chess, mỗi nước đi là một thao tác rõ ràng trên bàn cờ. Trong Auto Chess, một "hành động" của người chơi trong một lượt là một *tập hợp* các quyết định phụ thuộc nhau: mua bài nào, sắp xếp đội hình ra sao, có reroll không, có freeze không, có bán unit nào không. Không gian hành động tổ hợp này tăng theo hàm mũ theo số lượng unit trong shop và sân. Cụ thể, với 7 ô shop và 7 ô sân, số cấu hình đội hình có thể lên tới hàng triệu trên mỗi lượt.

**Thứ hai — reward thưa (sparse reward):**

Một trong những thách thức lớn nhất của RL là thiết kế reward signal đủ dày để agent học được. Nếu chỉ thưởng khi thắng ván (sparse reward), agent phải trải qua hàng nghìn ván đấu trước khi nhận được đủ tín hiệu để học một hành vi đơn giản. Trong Auto Chess, kết quả của một quyết định cụ thể (mua unit X ở lượt 3) có thể không thể hiện rõ ràng cho đến lượt 10 hoặc hơn — chuỗi nhân quả quá dài và bị pha loãng bởi nhiều yếu tố ngẫu nhiên trung gian.

**Thứ ba — chi phí tính toán:**

OpenAI Five cần tương đương 180 năm gameplay *mỗi ngày* để học Dota 2 — một con số chỉ khả thi với hạ tầng phần cứng trị giá hàng triệu đô la. AlphaStar sử dụng TPU pod của Google với chi phí ước tính hàng chục nghìn đô mỗi lần training. Với một đề tài nghiên cứu sinh viên chạy trên máy tính cá nhân, RL ở quy mô như vậy là hoàn toàn ngoài tầm với.

**Thứ tư — khả năng diễn giải (interpretability):**

Một bot RL học bởi neural network với hàng triệu trọng số là một **hộp đen** — không thể biết tại sao nó đưa ra quyết định nào. Điều này không chỉ là vấn đề học thuật: trong ngữ cảnh nghiên cứu, khả năng phân tích "bot này quyết định mua Babylon unit vì gene[18] của nó có giá trị cao" là một đóng góp khoa học có giá trị mà RL không thể cung cấp.

Những hạn chế này không phủ nhận tiềm năng của RL — chúng chỉ đặt ra câu hỏi về **tính phù hợp** của RL cho bài toán cụ thể này, trong ngữ cảnh tài nguyên cụ thể này. Và đó là không gian mà Genetic Algorithm bước vào.

---

## 1.3 Genetic Algorithm Trong Game AI

### 1.3.1 Lịch Sử Ứng Dụng — Từ Hành Vi NPC Đến Balancing

Genetic Algorithm không phải là công nghệ mới trong game AI — nó đã được thử nghiệm ở nhiều hướng khác nhau từ đầu những năm 2000. Điểm qua các mốc quan trọng cho thấy hướng tiếp cận này đã được kiểm chứng ở nhiều khía cạnh:

**Tiến hóa hành vi NPC:** Một trong những ứng dụng sớm nhất là tiến hóa hành vi của Non-Player Character (NPC). Thay vì lập trình hành vi cứng, nhà nghiên cứu định nghĩa một tập tham số hành vi (tốc độ di chuyển, ngưỡng aggro, chiến lược tấn công/phòng thủ) và dùng GA để tìm bộ tham số tạo ra NPC thú vị nhất theo một hàm fitness định nghĩa sẵn. Kết quả thường vượt trội so với behavior được thiết kế tay về tính đa dạng và khả năng thích nghi.

**NEAT — NeuroEvolution of Augmenting Topologies:** Năm 2002, Kenneth Stanley và Risto Miikkulainen giới thiệu **NEAT**, một framework tiến hóa không chỉ trọng số của neural network mà cả *cấu trúc* (topology) của nó. NEAT nổi tiếng nhất qua ứng dụng *MarI/O* — một agent NEAT học chơi Super Mario Bros từ đầu chỉ qua quan sát pixel, không có bất kỳ kiến thức lập trình sẵn nào về game. NEAT thể hiện rằng GA có thể tìm kiếm trong không gian giải pháp cực kỳ phức tạp (cấu trúc mạng nơ-ron) khi không gian đó được encode đúng cách.

**Game Balancing qua GA:** Một ứng dụng thực tiễn quan trọng khác là cân bằng game tự động. Thiết kế thủ công 68 lá bài với 14 trigger, 12 target và 13 effect như trong dự án này, rồi điều chỉnh tay từng giá trị số để không có chiến lược "ăn tất cả", là công việc cực kỳ tốn kém về nhân lực và thời gian. Các nghiên cứu đã sử dụng GA để tự động tìm kiếm bộ tham số cân bằng nhất theo các tiêu chí đo lường như entropy chiến lược (tất cả chiến lược đều có tỉ lệ thắng gần bằng nhau) hay diversity of winning strategies. Đây là hướng nghiên cứu đang được các hãng game AAA quan tâm.

**Evolving Bot Personalities:** Một số nghiên cứu tập trung không phải vào việc tạo ra bot *mạnh nhất* mà bot *thú vị nhất* hoặc *đa dạng nhất*. Ý tưởng là dùng GA với fitness function khác nhau cho từng "nhân cách" bot — aggressive, defensive, economic, chaotic — thu được một tập bot đa dạng hơn nhiều so với tối ưu hóa đơn thuần theo win rate. Đây chính là hướng mà thiết kế 5-bot-archetype của dự án này theo đuổi.

> **[HÌNH 1.3 — Lịch Sử Ứng Dụng GA Trong Game AI]** *Timeline ngang từ 2002 đến nay: NEAT (2002), Mario AI Competition (2009), Procedural Content Generation (2010s), Game Balancing via GA (2015+), đề tài này (2025). Mỗi mốc kèm biểu tượng game/ứng dụng tiêu biểu.*

---

### 1.3.2 Tại Sao GA Phù Hợp Cho Bài Toán Này

Sau khi đã phân tích hạn chế của RL (mục 1.2.3) và lịch sử GA (1.3.1), có thể thấy rõ lý do GA là lựa chọn phù hợp — không phải vì GA "tốt hơn RL" theo nghĩa tuyệt đối, mà vì bài toán cụ thể này có những đặc điểm ăn khớp với thế mạnh của GA:

**Không cần gradient, không cần differentiability:** Fitness của một chromosome là kết quả trận đấu mô phỏng — một hàm số nguyên không có đạo hàm và không liên tục. GA không yêu cầu bất kỳ tính chất giải tích nào của hàm fitness — nó chỉ cần so sánh được hai giá trị fitness với nhau. Đây là ưu điểm quyết định so với mọi phương pháp dựa trên gradient.

**Tự nhiên hỗ trợ đa nghiệm song song:** Mục tiêu của dự án không phải tìm một bot "tốt nhất" mà tìm nhiều phong cách chơi phân biệt. GA duy trì quần thể nhiều cá thể đồng thời — với cơ chế island model và elitism per-archetype, một lần training có thể thu được 5 bot chuyên biệt mà không cần 5 lần training riêng biệt.

**Chromosome cho interpretability:** Bởi vì mỗi gene mang ý nghĩa cụ thể (gene[18] = ưu tiên Babylon, gene[24] = ngưỡng reroll...), kết quả training không chỉ là một bot — mà là một **mô tả chiến lược có thể đọc và phân tích**. Sau training, ta có thể nhìn vào bộ gene của babylonBot và hardBot rồi giải thích *tại sao* chúng chơi khác nhau. Đây là đóng góp học thuật thực chất mà RL không thể cung cấp trong ngữ cảnh này.

**Tài nguyên vừa phải:** Toàn bộ logic game được thiết kế headless (không cần Unity Editor, không cần GPU), cho phép chạy hàng nghìn trận đấu mô phỏng trên CPU thông thường. Production training hoàn thành trong 20–30 phút — so sánh với hàng trăm giờ GPU cần thiết cho Deep RL ở quy mô tương tự.

Bảng dưới đây tóm tắt sự so sánh:

| Tiêu chí | Rule-Based | Decision Tree | Deep RL | Genetic Algorithm |
|----------|-----------|---------------|---------|-------------------|
| Cần gradient | Không | Không | Có | Không |
| Interpretable | Cao | Cao | Thấp | Cao |
| Đa phong cách | Không | Không | Khó | Tự nhiên |
| Tài nguyên tính toán | Thấp | Thấp | Rất cao | Thấp–Trung bình |
| Thích nghi với game phức tạp | Thấp | Trung bình | Cao | Cao |
| Thời gian thiết kế | Cao (viết tay) | Trung bình | Thấp | Thấp |

> **[HÌNH 1.4 — So Sánh Các Phương Pháp AI]** *Bảng so sánh trực quan hóa bằng biểu đồ màu sắc (xanh = tốt, đỏ = kém): Rule-based, Decision Tree, Deep RL và GA theo 6 tiêu chí nêu trên. Thể hiện rõ GA lấp đầy khoảng trống mà RL để lại trong ngữ cảnh tài nguyên hạn chế.*

---

## 1.4 Các Công Trình Liên Quan

### 1.4.1 Nghiên Cứu AI Trong Auto Chess Và TFT

Do thể loại Auto Chess còn tương đối mới (từ 2019), các nghiên cứu học thuật thuần túy về AI trong thể loại này còn khá hạn chế so với các game cổ điển như Chess hay Go. Tuy nhiên, đã có một số hướng tiếp cận đáng chú ý:

**Hướng rule-based và heuristic:** Phần lớn bot AI trong các Auto Chess thương mại (PvE bots trong TFT Practice Tool, bots trong Hearthstone Battlegrounds) sử dụng rule-based hoặc scripted behavior do team phát triển viết tay. Các bot này đủ để làm đối thủ thực hành cho người mới nhưng nhanh chóng bị người chơi có kinh nghiệm "đọc vị" vì hành vi có thể đoán trước. Đây chính là khoảng trống mà hướng tiếp cận học máy hướng đến.

**Hướng Monte Carlo Tree Search (MCTS):** MCTS — thuật toán từng được dùng thành công trong Go và chess — đã được nghiên cứu cho các game có yếu tố ngẫu nhiên. Tuy nhiên, trong Auto Chess, độ sâu của cây tìm kiếm kết hợp với không gian hành động rộng làm cho MCTS kém hiệu quả về mặt thời gian: với mỗi nút của cây là một "lượt shop" với hàng trăm tổ hợp hành động có thể, cây mở rộng quá nhanh để có thể tìm kiếm đủ sâu trong thời gian thực.

**Hướng học từ dữ liệu người chơi:** Một số nghiên cứu khai thác log dữ liệu ván đấu của người chơi thực (từ API của Riot Games cho TFT) để học pattern chiến thắng. Tuy nhiên hướng này phụ thuộc vào việc có thể truy cập dữ liệu lớn từ tựa game thương mại — điều không khả thi cho một dự án nghiên cứu độc lập.

Đề tài này không dựa trên bất kỳ hướng nào nêu trên mà chọn một hướng ít được khai thác hơn: **tiến hóa chromosome biểu diễn chiến lược**. Thay vì học từ dữ liệu có sẵn hay tìm kiếm trong không gian game state, hệ thống định nghĩa trước không gian tham số chiến lược (37 gene) rồi dùng GA để khám phá không gian đó.

---

### 1.4.2 Unity ML-Agents Và Lý Do Không Áp Dụng

**Unity ML-Agents** là framework chính thức của Unity Technologies, phát hành năm 2017 và liên tục phát triển, cho phép huấn luyện agent AI trong môi trường Unity bằng các thuật toán RL hiện đại (PPO, SAC, MA-POCA cho multi-agent). Framework này được tích hợp sâu vào Unity Editor, hỗ trợ observation vector từ game state, training trên GPU qua Python backend, và export model dưới dạng ONNX để deploy trực tiếp trong game.

Từ góc độ kỹ thuật, ML-Agents là giải pháp hoàn thiện và được hỗ trợ tốt. Tuy nhiên, có ba lý do cụ thể khiến nó không phù hợp với đề tài này:

**Lý do 1 — Mục tiêu là interpretability, không phải raw performance:** ML-Agents tạo ra neural network với hàng chục nghìn trọng số — không thể đọc hay phân tích chiến lược từ các trọng số đó. Mục tiêu của đề tài là không chỉ tạo ra bot tốt mà còn **hiểu được** tại sao bot chơi như vậy. Chromosome 37 gene cung cấp điều đó; neural network thì không.

**Lý do 2 — Tài nguyên huấn luyện:** ML-Agents với PPO cần hàng triệu bước để hội tụ cho bài toán có không gian trạng thái lớn như Auto Chess. Điều này đòi hỏi GPU và thời gian training tính bằng giờ đến ngày — không phù hợp với phạm vi dự án. GA với headless simulation hoàn thành trong 20–30 phút trên CPU thông thường.

**Lý do 3 — Thiết kế observation và action space:** Để dùng ML-Agents, cần encode toàn bộ trạng thái game thành observation vector và toàn bộ action space thành action mask — một công việc thiết kế engineering đáng kể và rất nhạy cảm với các quyết định về representation. Mỗi thay đổi nhỏ trong thiết kế game (thêm trigger type mới, thêm bộ tộc) đòi hỏi cập nhật toàn bộ observation/action schema và có thể làm vô hiệu model đã train. GA với chromosome real-valued không có vấn đề này — chromosome chỉ cần mở rộng thêm gene, không cần thiết kế lại từ đầu.

Điều này không có nghĩa ML-Agents là lựa chọn sai trong mọi ngữ cảnh — với đội ngũ lớn hơn, tài nguyên phần cứng đầy đủ và mục tiêu tập trung vào raw performance, ML-Agents sẽ là lựa chọn tốt hơn GA. Nhưng cho đề tài học thuật ở cấp tiểu luận chuyên ngành, GA cung cấp sự cân bằng tốt hơn giữa độ phức tạp triển khai, tài nguyên cần thiết, và giá trị học thuật thu được.

---

### 1.4.3 Nền Tảng Kỹ Thuật — Headless Simulation

Một thách thức kỹ thuật chung cho mọi hướng tiếp cận AI trong game Unity là: làm thế nào để chạy hàng nghìn trận đấu mô phỏng mà không cần mở Unity Editor hay khởi động scene? Vấn đề này thường được gọi là **headless simulation** — khả năng chạy logic game tách biệt khỏi rendering và UI.

Có ba cách tiếp cận phổ biến:

**Unity Batch Mode:** Unity cung cấp flag `-batchmode` để chạy build mà không mở cửa sổ. Tuy nhiên, batch mode vẫn yêu cầu toàn bộ Unity runtime được tải và scene được khởi tạo — không phù hợp cho việc chạy hàng nghìn vòng lặp training trong một tiến trình đơn lẻ vì overhead quá lớn.

**Tách logic thành Plain C#:** Đây là cách tiếp cận của đề tài: mọi lớp liên quan đến AI và combat logic được viết thuần C# không kế thừa MonoBehaviour, có thể khởi tạo bằng `new` và chạy trực tiếp trong Unity Editor mà không cần scene. `GATrainer` (MonoBehaviour) gọi `GameSimulator` (plain C#) hàng nghìn lần trong vòng lặp training — không cần scene mới, không cần render frame.

**External simulation framework:** Một số dự án tách hoàn toàn logic game ra khỏi Unity, viết lại bằng Python hay C++ thuần để tăng tốc độ simulation. Cách này cho hiệu năng cao nhất nhưng đòi hỏi duy trì đồng thời hai codebase (game Unity và simulation engine bên ngoài) — rủi ro về sự không nhất quán logic.

Đề tài chọn cách thứ hai vì lý do kỹ thuật rõ ràng: cùng một `CombatResolver.cs` được dùng cả trong gameplay thực (gọi qua Coroutine trong GameManager) lẫn trong headless training (gọi trực tiếp trong GameSimulator). Không có nguy cơ divergence giữa "logic game thật" và "logic training" — đây là tính chất quan trọng bậc nhất khi đánh giá tính hợp lệ của kết quả training.

---

## 1.5 Định Vị Đề Tài

### 1.5.1 Khoảng Trống Nghiên Cứu

Nhìn lại bức tranh tổng thể từ ba mục trước, có thể xác định rõ **khoảng trống** mà đề tài này lấp đầy:

- Thể loại Auto Chess đang phát triển nhanh nhưng **chưa có nghiên cứu học thuật có hệ thống** về AI cho thể loại này ở cấp độ tiếng Việt.
- Các bot trong Auto Chess thương mại đều dùng **rule-based logic**, không có khả năng tự học hay phát triển chiến lược mới.
- Deep RL — cách tiếp cận mạnh nhất hiện tại cho game AI phức tạp — **không khả thi** về mặt tài nguyên và không cung cấp interpretability cần thiết cho nghiên cứu học thuật ở cấp độ này.
- **GA đã được chứng minh** là phù hợp cho bài toán tối ưu hóa chiến lược trong game, nhưng chưa được áp dụng cụ thể cho Auto Chess với thiết kế chromosome bao phủ đầy đủ không gian quyết định.

---

### 1.5.2 Đóng Góp Cụ Thể Của Đề Tài

Từ khoảng trống trên, đề tài đóng góp ba thứ có giá trị độc lập với nhau:

**Đóng góp 1 — Chromosome 37 gene bao phủ toàn diện không gian quyết định:**

Không có nghiên cứu nào (theo hiểu biết của tác giả) thiết kế chromosome GA bao phủ đồng thời: đánh giá chỉ số thuần túy, đánh giá ability theo trigger/effect type, nhận diện tribe synergy, hành vi kinh tế (reroll, freeze, sell), và hành vi spell — trong một vector duy nhất và nhất quán. Mỗi gene có ý nghĩa cụ thể, cho phép phân tích kết quả training một cách có chiều sâu.

**Đóng góp 2 — TTE Ability Engine cho phép data-driven card design:**

Mô hình Trigger → Target → Effect với 14×12×13 tổ hợp cơ bản, kết hợp hệ thống modifier phong phú, cho phép thiết kế 68 lá bài với hành vi phức tạp mà không cần viết code riêng cho từng lá. Đây là đóng góp về kiến trúc phần mềm có giá trị độc lập với hệ thống AI — bất kỳ dự án card game nào cũng có thể tái sử dụng thiết kế này.

**Đóng góp 3 — Hệ thống training headless hoàn chỉnh, tái hiện được:**

Toàn bộ pipeline training — từ khởi tạo quần thể, mô phỏng trận đấu, đến lưu kết quả dưới dạng JSON — có thể chạy trên máy tính cá nhân thông thường trong 20–30 phút và tái hiện được (reproducible) vì không phụ thuộc vào cloud hay GPU. Đây là điều kiện quan trọng để kết quả thực nghiệm có thể được kiểm chứng độc lập.

> **[HÌNH 1.5 — Sơ Đồ Định Vị Đề Tài]** *Ma trận 2×2: trục hoành là "Tài nguyên cần thiết" (thấp→cao), trục tung là "Interpretability" (thấp→cao). Bốn ô: Rule-based (tài nguyên thấp, interpretability cao), Deep RL (tài nguyên cao, interpretability thấp), Decision Tree (thấp/cao), GA — vị trí của đề tài (tài nguyên thấp-trung bình, interpretability cao). Đề tài được tô màu nổi bật.*

---

### 1.5.3 Phạm Vi Và Không Phải Mục Tiêu

Để tránh hiểu nhầm về phạm vi, cần nêu rõ những gì đề tài **không** nhắm đến:

Đề tài không nhằm tạo ra bot AI mạnh nhất có thể cho thể loại Auto Chess — nếu mục tiêu là hiệu năng thuần túy, Deep RL với đủ tài nguyên sẽ thắng GA. Mục tiêu là tạo ra hệ thống AI có thể **được phân tích và giải thích**, phù hợp với ngữ cảnh học thuật.

Đề tài không so sánh với bot của Teamfight Tactics hay Hearthstone Battlegrounds — không có quyền truy cập vào codebase và phương pháp huấn luyện của các tựa game thương mại đó.

Đề tài không giải quyết bài toán AI cho PvP multi-player — game hiện tại chỉ hỗ trợ PvE (người chơi đơn đối đầu với bot). Mở rộng sang PvP sẽ đòi hỏi thiết kế lại đáng kể cả phần game lẫn phần AI.

Cuối cùng, kết quả training phụ thuộc vào thiết kế game cụ thể của dự án — một tập 68 lá bài với hệ thống ability riêng. Chromosome và fitness function không thể trực tiếp chuyển giao sang game khác mà không cần điều chỉnh, vì chúng được thiết kế để phản ánh không gian quyết định của game này.

---

Với bức tranh tổng quan lĩnh vực đã được thiết lập — lịch sử thể loại, các hướng tiếp cận AI và lý do chọn GA, các công trình liên quan và định vị rõ ràng — các chương tiếp theo có thể đi thẳng vào nền tảng lý thuyết và thiết kế kỹ thuật mà không cần giải thích ngữ cảnh thêm nữa.

---

*[Tiếp theo: Chương 2 — Cơ Sở Lý Thuyết]*

\newpage

# CHƯƠNG 2: CƠ SỞ LÝ THUYẾT

---

## 2.1 Thuật Toán Di Truyền (Genetic Algorithm)

### 2.1.1 Bài Toán Tối Ưu Và Tại Sao Chọn GA

Bài toán tối ưu hóa tổng quát: `x* = argmax f(x), x ∈ S ⊆ ℝⁿ`. Khi f không liên tục, không có đạo hàm, hay chứa vô số cực tiểu địa phương, các phương pháp giải tích cổ điển thất bại. Bài toán thiết kế AI cho game Auto Chess là trường hợp điển hình: hàm mục tiêu là tỉ lệ thắng của bot — không có công thức, không có gradient, chỉ có thể đo bằng cách *cho bot chơi thật*.

GA thuộc họ thuật toán tối ưu hóa theo quần thể: thay vì đi theo một đường từ một điểm, GA duy trì đồng thời một *tập hợp* nhiều nghiệm, khám phá nhiều vùng trong không gian song song. Ba lý do cốt lõi chọn GA cho đề tài: (1) không cần gradient — fitness là kết quả trận đấu; (2) tự nhiên hỗ trợ đa nghiệm — cùng một lần training thu được nhiều archetype nhờ island model; (3) khả thi trên phần cứng cá nhân — production training hoàn thành trong ~20 phút.

---

### 2.1.2 Biểu Diễn Nghiệm — Real-Valued Chromosome

Đề tài dùng **real-valued encoding**: mỗi chromosome là mảng 37 số thực trong [0,1]:

```csharp
// Chromosome.cs
public const int GeneCount = 37;
public float[] genes = new float[GeneCount];
```

Lý do: bài toán cần ước lượng *mức độ ưu tiên* — "bot này coi trọng ATK bao nhiêu?" là câu hỏi về mức độ liên tục, không phải có/không. Binary encoding đánh mất sắc thái đó; real-valued giữ nguyên. Chuẩn hóa về [0,1] cho phép gene có thể so sánh và kết hợp nhất quán, đơn giản hóa mutation (luôn clamp về [0,1]).

37 gene phân thành 9 nhóm chức năng:

| Nhóm | Gene | Chức năng |
|------|------|-----------|
| 1 | [0–3]   | Đánh giá chỉ số cơ bản (ATK, HP, Tier, Cost) |
| 2 | [4–6]   | Giá trị passive keyword (Taunt, Reborn, Safeguard) |
| 3 | [7–12]  | Trọng số loại trigger (StartOfBattle, OnDeath...) |
| 4 | [13–17] | Trọng số loại effect (AddStats, Summon, DealDmg...) |
| 5 | [18–20] | Ưu tiên bộ tộc (Babylon, Olympus, Niles) |
| 6 | [21–23] | Ngữ cảnh board (Merge, Frontline, Threshold) |
| 7 | [24–27] | Hành vi reroll (Threshold, Max, Reserve, Sell) |
| 8 | [28–31] | Hành vi spell (Threshold, Target, Economy) |
| 9 | [32–36] | Trigger con độc lập (Aura, OnSell, OnAllyGroup...) |

Sự phân nhóm giúp đọc "cá tính chiến lược" của bot từ chromosome — điều Neural Network không làm được (interpretability).

> **[HÌNH 2.1 — Cấu trúc Chromosome 37 Gene]** *Biểu đồ thanh ngang phân nhóm 9 cụm gene theo màu sắc.*

---

### 2.1.3 Hàm Fitness

Fitness đo qua điểm tích lũy từ các trận đấu mô phỏng:

```
score(trận) = {120 thắng / 70 hòa / 35 thua}
            + hpA × 6 − hpB × 3
            + (nếu thắng) (MaxTurns − turns) × 2

fitness(c) = Σᵢ score(trận i),   i ∈ [0, matchesPerChrom)
```

Điểm hòa = 70 (không phải 0) — giữ lại chiến lược phòng thủ bền bỉ. Ba thành phần encode đồng thời: thắng/thua, biên HP, và tốc độ thắng — tránh bot chỉ tối ưu một chiều.

Fitness noise (kết quả trận có ngẫu nhiên do shop roll) được giảm thiểu bằng cách chạy đủ nhiều trận: 5 trận/chromosome (quick) hoặc 20 trận (production).

---

### 2.1.4 Vòng Lặp Tiến Hóa

```
Khởi tạo quần thể P (ngẫu nhiên + seeding 5 archetype)
    │
    ▼
[Lặp g = 0 → G-1]
    ├─► Đánh giá fitness(c) cho mọi c ∈ P
    ├─► Sắp xếp P theo fitness giảm dần
    ├─► Tạo thế hệ mới P':
    │       ├── Clone elite (top 10%)            → bảo toàn tốt nhất
    │       ├── TournamentSelect × 2 → Crossover → kế thừa từ tốt
    │       └── Mutate + Immigrate               → khám phá mới
    └─► P ← P'
    ▼
Trả về 5 specialist bot từ quần thể cuối
```

> **[HÌNH 2.2 — Vòng Lặp Tiến Hóa GA]** *Flowchart minh họa vòng lặp qua các thế hệ.*

**Tournament Selection (k=3):** Chọn ngẫu nhiên 3 cá thể, lấy fitness cao nhất làm cha/mẹ. Ưu điểm so với Roulette Wheel: tránh superindividual — cá thể vượt trội không độc chiếm breeding pool, duy trì diversity.

```csharp
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

**2-Point Crossover:** Hai điểm cắt ngẫu nhiên; đoạn giữa từ cha B, hai đầu từ cha A. Bảo toàn epistasis (gene liền kề trong cùng nhóm chức năng) tốt hơn uniform crossover.

```
Con: [a₀..a₃ | b₄..b₈ | a₉..a₃₆]   (pt1=4, pt2=9)
```

**Gaussian Mutation (Box-Muller):** Cộng nhiễu N(0, σ) vào giá trị gene hiện tại — thay đổi nhỏ phổ biến hơn thay đổi lớn, hội tụ mượt hơn uniform mutation.

```csharp
if (Random.value < mutationRate)   // mutationRate = 0.08
{
    float u1 = Mathf.Max(1e-6f, Random.value);
    float z  = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * Random.value);
    child.genes[i] += z * mutationMag;   // mutationMag = 0.12
}
child.genes[i] = Mathf.Clamp01(child.genes[i]);
```

> **[HÌNH 2.3 — Gaussian Mutation]** *Phân phối chuẩn N(0, 0.12): phần lớn thay đổi trong ±0.24 (2σ).*

---

### 2.1.5 Elitism Và Duy Trì Đa Dạng

**Elitism hai cấp:** Clone top `max(3, Lerp(N/18, N/8, progress))` chromosome (tăng dần cuối training) + top 2 mỗi archetype (Babylon, Niles, Summoner, Resilient) — đảm bảo fitness tốt nhất không bao giờ giảm và mỗi phong cách chơi luôn có đại diện.

```csharp
AddTopClones(nextGen, population, c => true,  eliteCount);
AddTopClones(nextGen, population, IsBabylon,  2);
AddTopClones(nextGen, population, IsNile,     2);
```

> **[HÌNH 2.4 — Elitism và Fitness Không Giảm]** *Best fitness monotonically non-decreasing nhờ elitism.*

**Chống premature convergence** bằng hai chiến lược: (1) **Island Model** — 5 sub-population với seeding theo archetype, elitism per-archetype duy trì diversity chiến lược; (2) **Immigration** — 12% cá thể mỗi thế hệ là chromosome seeded mới hoàn toàn, bơm vật liệu di truyền mới vào quần thể. Nếu một tribe < 10% quần thể, +2 immigrant cho tribe đó.

Dừng sớm khi phát hiện plateau:

```csharp
const int PLATEAU_PATIENCE = 15;
const float PLATEAU_EPS = 0.5f;
if (Mathf.Abs(stdDev - prevStdDev) < PLATEAU_EPS) plateauCount++;
else plateauCount = 0;
if (plateauCount >= PLATEAU_PATIENCE) break;
```

> **[HÌNH 2.5 — Premature Convergence vs. Healthy Evolution]** *std_dev (xanh), best fitness (đỏ), avg fitness (cam) qua các thế hệ.*

---

### 2.1.6 Kết Quả — 5 Bot Chuyên Biệt

Thay vì trả về một bot "tốt nhất", GA trả về 5 bot đủ khác nhau về cấu trúc gene (khoảng cách Euclidean chuẩn hóa `d(a,b) ≥ 0.18`) đại diện cho 5 phong cách chơi: hardBot (fitness cao nhất), babylonBot, nileBot, summonerBot, resilientBot.

> **[HÌNH 2.6 — Radar Chart So Sánh 5 Bot Archetype]** *Spider chart 5 trục gene chính; mỗi bot một đường màu khác nhau.*

---

*[Tiếp theo: Mục 2.2 — Lập trình game với Unity Engine]*

## 2.2 Lập Trình Game Với Unity Engine

### 2.2.1 Component-Entity Model

Unity tổ chức thực thể theo **Component-Entity**: mọi object là một **GameObject** rỗng, tính năng được thêm bằng cách gắn **Component** (lớp kế thừa `MonoBehaviour`). Một lá bài không phải là một lớp "God Card" khổng lồ mà là một GameObject với `CardUI`, `CardDraggable`, `CardVisuals`, `CardSlot` — mỗi component phát triển và tái sử dụng độc lập.

Quy ước lifecycle trong dự án: **Singleton tự đăng ký trong `Awake()`**, các component khác **dùng Singleton trong `Start()`** — đảm bảo thứ tự khởi tạo đúng dù Unity không cam kết thứ tự `Awake()` giữa các object.

> **[HÌNH 2.7 — Component-Entity Model trong Unity]** *Sơ đồ GameObject "Card" với bốn Component: CardUI, CardDraggable, CardVisuals, CardSlot.*

---

### 2.2.2 Singleton — Quản Lý Trạng Thái Toàn Cục

Dự án có đúng 5 Singleton tương ứng 5 domain: `GameManager`, `CardDatabase`, `AIManager`, `UIManager`, `AudioManager`. Hai biến thể:

```csharp
// GameManager.cs — gán trực tiếp (luôn chỉ có 1 instance)
public static GameManager Instance;
private void Awake() { Instance = this; }
```

```csharp
// CardDatabase.cs — an toàn cho DontDestroyOnLoad
private void Awake()
{
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
}
```

---

### 2.2.3 Partial Class — Tổ Chức Lớp Phức Tạp

`GameManager` chia thành 4 file theo domain, dùng từ khóa `partial`:

```
GameManager.cs        →  Core state: HP, coins, board, Singleton, ExecuteNextTurn
GameManager.Shop.cs   →  RefreshShop, BuyCard, Roll, Lock, MergeHints
GameManager.Combat.cs →  StartCombatPhase, CombatSequence, VisualizeAction
GameManager.Board.cs  →  SyncBoards, Snapshot, Restore, SpawnUI
```

```csharp
public partial class GameManager : MonoBehaviour { ... }   // GameManager.cs
public partial class GameManager { ... }                   // GameManager.Combat.cs
```

> **[HÌNH 2.8 — Partial Class GameManager]** *Sơ đồ cây: 4 file riêng cùng tạo thành 1 class.*

---

### 2.2.4 Coroutine — Tách Tính Toán Và Trình Diễn

`CombatSequence()` minh họa quyết định kiến trúc quan trọng nhất: **tính toán hoàn toàn đồng bộ, trình diễn bất đồng bộ**:

```csharp
private IEnumerator CombatSequence()
{
    TurnRecord combatLog = new TurnRecord();
    resolver.ResolveTurn(playerBoard, enemyBoard, combatLog);   // tính toán toàn bộ ngay lập tức

    foreach (var action in combatLog.actions)
    {
        yield return StartCoroutine(VisualizeAction(action));   // phát lại từng action
        yield return new WaitForSeconds(0.1f);
    }
    CheckVictoryConditions();
}
```

Logic hoàn tất trước khi animation bắt đầu — cùng `resolver.ResolveTurn()` chạy được trong headless training mà không cần viết lại.

---

### 2.2.5 Ranh Giới MonoBehaviour / Plain C#

Đây là điều kiện tiên quyết cho GA training: lớp kế thừa `MonoBehaviour` không thể khởi tạo bằng `new` — cần scene, cần Unity runtime. Plain C# class thì có thể:

```csharp
// Có thể chạy trong training loop — không cần scene:
BotAgent botA     = new BotAgent(chromosome);
CombatResolver res = new CombatResolver();
GameSimulator sim  = new GameSimulator();

// KHÔNG thể dùng ngoài scene:
GameManager gm = new GameManager();   // MonoBehaviour — COMPILE ERROR
```

Toàn bộ tầng AI và combat (`Chromosome`, `BotAgent`, `CombatResolver`, `GameSimulator`, `EconomyManager`, `CardInstance`) là plain C#. `GATrainer` (MonoBehaviour) gọi `sim.EvaluateMatch()` hàng nghìn lần mà không cần tạo scene mới:

```csharp
// GameSimulator.cs — không có Unity API nào
public class GameSimulator
{
    private CombatResolver resolver = new CombatResolver();

    public MatchResult EvaluateMatch(BotAgent botA, BotAgent botB)
    {
        int hpA = 7, hpB = 7;
        for (int i = 0; i < 20; i++) { /* shop, decision, combat, check HP */ }
        return new MatchResult { ... };
    }
}
```

> **[HÌNH 2.9 — Ranh Giới MonoBehaviour và Plain C#]** *Sơ đồ hai tầng: tầng Unity-dependent (GameManager, CardDatabase, UIManager) trên; tầng headless-compatible (Chromosome, BotAgent, CombatResolver, GameSimulator) dưới. Mũi tên một chiều từ trên xuống.*

---

### 2.2.6 Data-Driven: JSON Và Resources

Toàn bộ 68 lá bài lưu trong `CardsData.json`, nạp vào runtime:

```csharp
// CardDatabase.cs
TextAsset jsonFile = Resources.Load<TextAsset>("CardsData");
CardDataWrapper data = JsonUtility.FromJson<CardDataWrapper>(jsonFile.text);
```

`Chromosome` được đánh dấu `[System.Serializable]` để `JsonUtility` serialize được `AILibrary` (chứa các Chromosome) sang `AI_Library.json` không cần thư viện ngoài:

```csharp
[System.Serializable]
public class Chromosome
{
    public const int GeneCount = 37;
    public float[] genes = new float[GeneCount];
    public float fitness = 0f;
}
```

`AI_Library.json` là file duy nhất chứa "trí tuệ" của 5 bot sau training — có thể đọc bằng text editor, commit vào version control, nạp lại không cần công cụ đặc biệt.

---

*[Tiếp theo: Mục 2.3 — Mô hình Trigger → Target → Effect]*

## 2.3 Mô Hình Trigger → Target → Effect (TTE)

### 2.3.1 Kiến Trúc Data-Driven Cho 68 Lá Bài

Thay vì viết 68 lớp C# riêng, mọi ability được biểu diễn bằng struct `AbilityData` với ba trường độc lập:

```csharp
[System.Serializable]
public class AbilityData
{
    public TriggerType trigger;    // KHI NÀO kích hoạt?
    public TargetType  target;     // NHẮM VÀO AI?
    public EffectType  effect;     // LÀM GÌ?
    public int effectValue1;
    public int effectValue2;
    // ... modifier flags
}
```

Với **14 trigger × 12 target × 13 effect = 2.184 tổ hợp cơ bản**, cộng các modifier (`isPermanent`, `triggerLimit`, `conditionCount`, `isEscalating`...) tạo ra hàng chục nghìn hành vi — tất cả từ JSON, không cần thêm một dòng C#.

> **[HÌNH 2.10 — Không Gian Tổ Hợp TTE]** *Hình khối lập phương 3D: Trigger (14) × Target (12) × Effect (13). Một số ô được gán nhãn ví dụ cụ thể.*

---

### 2.3.2 Luồng Thực Thi TriggerAbility()

```
TriggerAbility(triggerContext, source, directEnemy, allyBoard, enemyBoard)
    │
    ├─ [1] source có abilities không? → thoát ngay nếu không
    │
    ├─ [2] Với mỗi ability:
    │       ├─ ability.trigger == triggerContext?     → bỏ qua nếu không
    │       ├─ Đã đạt triggerLimit?                  → bỏ qua nếu rồi
    │       ├─ conditionCount: đây có phải lần N?    → bỏ qua nếu không
    │       ├─ subjectTribe filter hợp lệ?           → bỏ qua nếu không
    │       ├─ isConsume trong shop phase?           → block nếu cần
    │       │
    │       ├─ FindTargets() → List<CardInstance>
    │       │
    │       ├─ triggerLimit > 0 VÀ targets rỗng?    → không đếm lần kích hoạt
    │       │
    │       └─ Với mỗi target: ApplyGrowth() hoặc ExecuteEffect()
    │
    └─ isEscalating? → tăng escalationBonus cho lần sau
```

Chi tiết quan trọng: `triggerLimit` chỉ đếm khi **có mục tiêu hợp lệ** — tránh Sekhmet thứ hai "lãng phí" lần sử dụng khi target đã bị nuốt bởi Sekhmet đầu tiên.

> **[HÌNH 2.11 — Luồng TriggerAbility()]** *Flowchart: trigger match → limit/condition check → FindTargets → ExecuteEffect. Nhánh skip màu xám, nhánh thực thi màu xanh.*

---

### 2.3.3 14 Trigger Types — Bốn Nhóm

| Nhóm | Trigger | Mô tả ngắn |
|------|---------|------------|
| **Combat cá nhân** | `OnAttack` | Sau khi unit tấn công và còn sống |
| | `OnTakeDamage` | Khi nhận damage và còn sống |
| | `OnDeath` | Khi chết (qua death stack) |
| | `StartOfBattle` | Một lần đầu trận |
| **Shop phase** | `OnDeploy` | Khi kéo từ tay lên sân |
| | `OnSell` | Khi bị bán |
| | `EndTurnShop` | Đầu lượt tiếp theo với mọi unit trên sân |
| **Phản ứng đồng minh** | `OnAllyDeath` | Khi đồng minh (cùng tribe) chết |
| | `OnAllySummon` | Khi đồng minh được triệu hồi trong combat |
| | `OnAllyReborn` | Khi đồng minh hồi sinh |
| | `OnAllyDeploy` | Khi đồng minh deploy trong shop phase |
| | `OnAllySell` | Khi đồng minh bị bán |
| **Đặc biệt** | `Aura` | Đầu trận → `AllAlliesExceptSelf` (passive buff) |
| | `OnStatGain` | Khi nhận chỉ số vĩnh viễn |

`OnStatGain` tạo chain reaction nguy hiểm. Guard flag ngăn đệ quy vô hạn:

```csharp
private bool _firingOnStatGain = false;

if (!_firingOnStatGain)
{
    _firingOnStatGain = true;
    try { TriggerAbility(TriggerType.OnStatGain, target, ...); }
    finally { _firingOnStatGain = false; }
}
```

> **[HÌNH 2.12 — Phân Loại 14 Trigger Types]** *Bảng 4 nhóm màu sắc với ví dụ card tiêu biểu mỗi trigger.*

---

### 2.3.4 Effects Đáng Chú Ý

**Growth (`StartOfBattle` + `AddStats`):** Tích lũy vào `growthBonus` thay vì chỉ `currentStats` — tồn tại xuyên nhiều lượt:

```csharp
target.growthATKBonus += atkGain;
target.growthHPBonus  += hpGain;
target.currentATK     += atkGain;
target.currentHP      += hpGain;
// ResetStats(): currentATK = baseATK × tier + 0.7 × (growthBonus + permanentBonus)
```

**`Destroy + isConsume`:** Nuốt unit đồng minh, lưu `cardID` vào `consumedCardIDs`; `SummonConsumed` giải phóng tất cả khi source chết. Edge case: nếu target còn Reborn chưa dùng, delay ghi nhận consume đến sau khi Reborn resolve:

```csharp
bool skipForPendingReborn = _isCombatPhase && target.isReborn && !target.hasRebornUsed;
if (!skipForPendingReborn)
    source.consumedCardIDs.Add(target.Data.cardID);
```

**`ScaleTargetStats`:** Nhân chỉ số target với `mergeLevel + 1`. Dùng cho Osiris — `OnAllyReborn` → nhân đôi/ba/tư chỉ số unit hồi sinh.

**Modifier table:**

| Modifier | Cơ chế |
|----------|--------|
| `conditionCount` | Chỉ kích hoạt lần N, 2N, 3N... |
| `isEscalating` | effectValue tăng +1 mỗi lần kích hoạt |
| `isScaledTriggerLimit` | Giới hạn = `triggerLimit × (mergeLevel + 1)` |
| `globalTribeBuff` | Áp dụng cho tất cả unit cùng tribe kể cả trong tay/shop/tương lai |

---

### 2.3.5 Ordering Và Reentrancy

**BroadcastAllyEvent snapshot:** Tránh unit mới được summon trong khi broadcast nhận event "xảy ra trước khi nó tồn tại":

```csharp
public void BroadcastAllyEvent(TriggerType context, CardInstance subject,
    List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
{
    var snapshot = new List<CardInstance>(allyBoard);   // chụp trước
    foreach (var unit in snapshot)
    {
        if (unit == null || unit.IsDead || unit == subject) continue;
        TriggerAbility(context, unit, subject, allyBoard, enemyBoard);
    }
}
```

**Pending summon queue:** Summon đầu tiên trong batch thực hiện ngay; các summon còn lại xếp vào `_pendingSummons`. `FlushDeathStack()` gọi `ProcessNextPendingSummon()` sau mỗi death event — **hậu quả của một cái chết được resolve hoàn toàn trước khi unit tiếp theo xuất hiện**:

```csharp
CardInstance first = SummonUnit(ability.summonCardID, allyBoard);
TriggerAbility(TriggerType.OnDeploy, first, null, allyBoard, enemyBoard);
BroadcastAllyEvent(TriggerType.OnAllySummon, first, allyBoard, enemyBoard);

for (int s = 1; s < summonCount; s++)
    _pendingSummons.Enqueue(new PendingSummonEntry { cardID = ..., allyBoard = ..., enemyBoard = ... });
```

> **[HÌNH 2.13 — Pending Summon Queue và Death Stack]** *Sequence diagram: unit A chết → FlushDeathStack → TriggerOnDeath → Summon B (ngay) + Enqueue C → BroadcastOnAllyDeath → CleanupBoard → ProcessNextPendingSummon pop C.*

---

### 2.3.6 Phase-Awareness Và Kết Nối Với AI

**Phase-awareness:** Cùng một ability hành xử khác nhau theo phase — không cần hai ability riêng:

```csharp
bool isShopPhase = GameManager.Instance != null && !GameManager.Instance.isCombatActive;

if (isShopPhase)
    GameManager.Instance.AddUnitToHand(toAdd);       // vào Hand
else
    CardInstance first = SummonUnit(summonCardID, allyBoard);  // vào Board

// Sekhmet chỉ nuốt trong combat
if (ability.trigger == TriggerType.OnAllySummon && ability.isConsume && !_isCombatPhase) continue;
```

**Kết nối với AI:** TTE cho phép bot đánh giá mọi lá bài bằng cùng một công thức — vì mọi ability có trigger và effect rõ ràng dưới dạng enum:

```
abilityScore = Σ TriggerWeight(ability.trigger) × EffectWeight(ability.effect) × 10
```

Gene[7–12] và [32–36] là trigger weights; gene[13–17] là effect weights. Bot có `gene[8]` (tOnDeath) cao sẽ đánh giá cao mọi unit trigger OnDeath — dù đó là Horus, Sekhmet, hay Osiris. **TTE định nghĩa ngôn ngữ của lá bài; Chromosome học cách đánh giá ngôn ngữ đó**.

---

*[Tiếp theo: Mục 2.4 — Lý thuyết kinh tế trong Auto Chess]*

## 2.4 Lý Thuyết Kinh Tế Trong Auto Chess

### 2.4.1 Cấu Trúc Bài Toán

Mỗi lượt trong Auto Chess là một **bài toán quyết định tuần tự dưới sự không chắc chắn**: quan sát trạng thái (board, coin, shop) → thực hiện hành động (mua/bán/reroll/freeze) → chuyển trạng thái. Tối ưu hóa đòi hỏi biết trước toàn bộ chuỗi shop tương lai — điều không thể. Phần còn lại của mục 2.4 phân tích từng khía cạnh để xây dựng heuristic tốt, và cuối cùng là cách GA encode chiến lược kinh tế trong 8 gene.

---

### 2.4.2 Dòng Tiền Và Thu Nhập

```
CurrentCoin = 10 + bonusNextTurn + permanentIncomeBonus
bonusNextTurn ← 0   (đặt lại sau khi cộng)
```

Ba thành phần: `base = 10` (cố định, không snowball), `bonusNextTurn` (delayed income từ spell, dùng một lần), `permanentIncomeBonus` (vĩnh viễn, cộng mỗi lượt). Kênh chi tiêu: mua unit (1–3 coin), reroll (1 coin), mua spell (1–3 coin). Bán unit luôn trả lại đúng 1 coin bất kể giá mua — loại bỏ "sunk cost tracking".

---

### 2.4.3 Tier Progression Và Drop Rate

Shop tier tăng tự động: `shopTier = clamp(⌊(turn + 1) / 2⌋, 1, 6)`. Không cần trả coin để truy cập unit mạnh hơn — chỉ cần sống sót đủ lâu.

| Shop Tier | T1  | T2  | T3  | T4  | T5  | T6  |
|-----------|-----|-----|-----|-----|-----|-----|
| 1         | 100%| 0%  | 0%  | 0%  | 0%  | 0%  |
| 3         | 50% | 35% | 15% | 0%  | 0%  | 0%  |
| 5         | 15% | 25% | 35% | 15% | 10% | 0%  |
| 6         | 10% | 15% | 20% | 25% | 20% | 10% |

Weighted distribution (không phải cutoff cứng) đảm bảo bản sao unit cũ vẫn có thể xuất hiện ở giai đoạn muộn. Fallback: nếu không có unit tại tier rolled, `GetRandomUnitShop()` lấy tier thấp hơn gần nhất — shop không bao giờ có ô trống.

> **[HÌNH 2.14 — Drop Rate Theo Shop Tier]** *Stacked bar chart: Shop Tier 1–6 trên trục hoành, phân phối % mỗi card tier.*

---

### 2.4.4 Đánh Đổi Tempo — Economy

**Tempo** = sức mạnh đội hình ngay bây giờ. **Economy** = tiền dự trữ cho tương lai. Không có điểm cân bằng tuyệt đối: HP = 7 (đầu game) → hy sinh tempo hợp lý; HP = 1 (bờ vực thua) → tempo phải tối đa hóa ngay.

> **[HÌNH 2.15 — Đánh Đổi Tempo và Economy]** *Biểu đồ 2D: trục hoành = mức độ chi tiêu sớm, trục tung = xác suất thắng. Hai đường cong HP cao và HP thấp — điểm tối ưu dịch chuyển theo ngữ cảnh.*

---

### 2.4.5 Reroll, Freeze Và Merge Logic

**Reroll** — ba gene phối hợp:
```
Điều kiện:   bestShopScore < gene[24] × bestBoardScore
Số lần max:  floor(gene[25] × 3) + 1   →  [1..4 lần/lượt]
Buffer coin: floor(gene[26] × 4)        →  [0..4 coin giữ lại]
```

**Freeze:** Bot giữ nguyên shop khi `(1 - gene[24] ≥ 0.35)` *và* có unit hấp dẫn nhưng chưa đủ tiền (`score ≥ gene[23] × 3` mà `cost > currentCoin`).

**Merge bonus** khi đánh giá unit trong shop:
```
mergeBonus = copies × gene[21] × (copies == 2 ? 16 : 8)
```
Thiếu 1 bản sao cuối (copies == 2) → bonus gấp đôi, phản ánh tính cấp thiết hoàn thành bộ 3.

**Sell logic:**
- *Reactive*: `score(new) > worstBoardScore × (1.5 + gene[23])` → thay thế unit yếu nhất
- *Proactive*: `EvaluateInstance(unit) < gene[27] × 3` → bán chủ động nếu unit quá kém

---

### 2.4.6 Đánh Giá Spell

Spell được đánh giá qua `EvaluateSpell()` dùng 4 gene riêng (gene[28–31]), không so sánh trực tiếp với unit. Ví dụ `GainIncome` (permanent income):

```
score = value × gene[16] × gene[31] × (isPermanent ? 12 : 1.5)
```

Hệ số 12 vs. 1.5: với 20 lượt tối đa, mỗi coin thêm mỗi lượt tích lũy 20 coin. Spell rủi ro cao có điểm âm cứng:

```csharp
case 14: // LoseLife   → return -25f;
case 15: // TransferStats → return -8f;
```

---

### 2.4.7 Hàm Fitness Như Mô Hình Kinh Tế

```csharp
// GameSimulator.ScoreFromA()
float score = result > 0 ? 120f : result == 0 ? 70f : 35f;
score += hpA * 6f;
score -= hpB * 3f;
if (result > 0)
    score += (MaxTurns - turns) * 2f;
return Mathf.Max(1f, score);
```

Ba thành phần encode ba mục tiêu kinh tế: (1) kết quả tuyệt đối (hòa = 70, không phải 0 — giữ lại chiến lược phòng thủ); (2) biên thắng `hpA×6 − hpB×3` (hệ số không đối xứng khuyến khích giữ mình sống hơn chỉ tấn công); (3) tốc độ `(MaxTurns − turns) × 2` — ngầm thưởng cho chiến lược tempo.

Phạm vi: thắng sớm HP cao ≈ 200; thua nhanh HP cạn ≈ 14 — đủ rộng phân biệt chiến lược. Đây là **reward shaping**: behavior tốt nổi lên tự nhiên từ fitness, không cần hard-code.

> **[HÌNH 2.16 — Cấu Trúc Hàm Fitness]** *Thanh dọc chia nhỏ: phần thắng/thua, phần biên (hpA−hpB), phần tốc độ. Ví dụ số: thắng nhanh vs. thắng muộn.*

---

### 2.4.8 8 Gene Kinh Tế

| Gene | Tên | Câu hỏi kinh tế |
|------|-----|-----------------|
| [23] | wSaveThreshold | Điểm tối thiểu để lá bài "đáng mua"? |
| [24] | wRerollThresh | Khi nào shop đủ tệ để đáng trả 1 coin? |
| [25] | wRerollMax | Tối đa bao nhiêu lần reroll mỗi lượt? |
| [26] | wRerollKeep | Giữ lại bao nhiêu coin như buffer an toàn? |
| [27] | wProactiveSell | Khi nào unit đủ kém để đáng bán chủ động? |
| [28] | wSpellThresh | Điểm tối thiểu để spell "đáng mua"? |
| [29] | wSpellOnStrong | Buff unit mạnh nhất hay phân bổ đều? |
| [31] | wSpellEconomy | Coi trọng công cụ tạo coin bao nhiêu? |

GA không được lập trình để "hiểu kinh tế" — nó chỉ tìm bộ tám số thực trong [0,1]⁸ cho fitness cao nhất. Vì fitness phản ánh đúng "chơi tốt", chromosome hội tụ về chính sách kinh tế hợp lý. Đây là sức mạnh và giới hạn của GA: tìm được giải pháp tốt mà không hiểu tại sao nó tốt.

---

*[Kết thúc Chương 2 — Tiếp theo: Chương 3 — Thiết Kế Game (GDD)]*

\newpage

# CHƯƠNG 3: THIẾT KẾ GAME — GAME DESIGN DOCUMENT

---

## 3.1 Tầm Nhìn Và Concept

**Tên tựa game:** AutoChess: Pantheon *(working title)*  
**Thể loại:** Auto Battler / Auto Chess  
**Nền tảng:** PC (Windows), Unity 2022+, C#  
**Chế độ chơi:** Single-player PvE, tối đa 20 lượt  
**Trạng thái:** Prototype hoàn chỉnh

Người chơi không điều khiển chiến đấu trực tiếp — họ đưa ra quyết định *trước* trận: mua bài nào, sắp xếp đội hình, khi nào tiết kiệm/đầu tư. Sau pha chuẩn bị, hai đội tự động giao chiến; kết quả phụ thuộc hoàn toàn vào chất lượng chiến lược.

---

### 3.1.1 Ba Bộ Tộc — Ba Triết Lý Chơi

| Bộ tộc | Nguồn gốc | Cơ chế cốt lõi | Phong cách chơi |
|--------|-----------|----------------|-----------------|
| **Babylon** | Thần thoại Lưỡng Hà | Buff qua deploy/sell, hấp thụ chỉ số | Accumulation — snowball dài hạn |
| **Olympus** | Thần thoại Hy Lạp | ATK synergy qua combat events | Aggression — áp đảo sớm |
| **Niles** | Thần thoại Ai Cập | Chu trình chết–tái sinh, OnAllyDeath chain | Reaction — mạnh bất ngờ qua chuỗi trigger |

Người chơi có thể đi theo một bộ tộc, kết hợp hai, hoặc xây đội hình hybrid — đây là chiều sâu chiến thuật mà thiết kế hướng tới.

> **[HÌNH 3.1 — Ba Bộ Tộc Và Triết Lý Chơi]** *Infographic ba cột: Babylon (Accumulation), Olympus (Aggression), Niles (Death & Rebirth). Mỗi cột kèm từ khóa chiến lược và unit tiêu biểu.*

Core fantasy: *"Tôi là chiến lược gia sắp đặt để các thần linh cổ đại chiến đấu theo ý muốn của tôi."* Niềm vui đến từ *sự tiên liệu* — combo Anubis → Reborn → Osiris được lên kế hoạch ba lượt trước là khoảnh khắc đặc trưng nhất của game.

---

### 3.1.2 Đặc Trưng Phân Biệt

**TTE Engine data-driven:** Toàn bộ 68 lá bài định nghĩa qua JSON (trigger/target/effect), không có code C# riêng cho bất kỳ lá bài nào. Thiết kế viên có thể thêm card mới chỉ bằng sửa JSON, không cần lập trình viên.

**AI được sinh ra, không được lập trình:** Mọi hành vi bot — đánh giá card, quyết định reroll, ưu tiên tribe, bán unit — phát sinh từ 37 số thực được Genetic Algorithm tìm kiếm qua hàng chục nghìn trận mô phỏng. Không có một dòng if-else quy tắc nào được viết tay.

**Ba cấp độ với phong cách chơi thực sự khác nhau:** Easy/Medium/Hard tương ứng với các archetype bot được train riêng biệt (babylonBot, nileBot, resilientBot…) — không chỉ khó hơn mà còn chơi *khác hơn*.

> **[HÌNH 3.2 — Screenshot Màn Hình Chọn Độ Khó]** *Ảnh chụp UI chọn Easy / Medium / Hard với mô tả từng cấp độ.*

---

*[Tiếp theo: Mục 3.2 — Vòng Lặp Gameplay Cốt Lõi (Core Loop)]*

## 3.2 Vòng Lặp Gameplay Cốt Lõi (Core Loop)

### 3.2.1 Cấu Trúc Một Ván Đấu

Người chơi bắt đầu với 7 HP, 0 cup. Mục tiêu: tích đủ 10 cup (mỗi trận thắng +1) trước khi HP về 0, trong tối đa 20 lượt.

```
┌──────────────────────────────────────────────────────────────────┐
│                    VÒNG LẶP MỘT VÁN ĐẤU                         │
│                                                                  │
│   [BẮT ĐẦU] HP = 7, Cup = 0, Lượt = 1                           │
│       │                                                          │
│       ▼                                                          │
│   ╔═══════════════╗                                              │
│   ║  SHOP PHASE   ║  ← nhận 10 coin, shop mới, quyết định       │
│   ╚═══════╤═══════╝                                              │
│           │  [Nhấn Fight]                                        │
│           ▼                                                      │
│   ╔═══════════════╗                                              │
│   ║ COMBAT PHASE  ║  ← chiến đấu tự động, tối đa 50 bước        │
│   ╚═══════╤═══════╝                                              │
│           │                                                      │
│     ┌─────┴──────┐                                               │
│  Thắng?      Thua?      Hòa?                                     │
│  Cup +1      HP −1     (không đổi)                               │
│     │          │           │                                     │
│     └────┬─────┴───────────┘                                     │
│          ▼                                                       │
│   ┌──────────────────────────────────────┐                       │
│   │ Cup ≥ 10? → Thắng ván                │                       │
│   │ HP ≤ 0?  → Thua ván                  │                       │
│   │ Lượt > 20? → Thắng ván (vượt thời hạn)                      │
│   │ Còn lại  → Lượt + 1, quay lại Shop  │                       │
│   └──────────────────────────────────────┘                       │
└──────────────────────────────────────────────────────────────────┘
```

> **[HÌNH 3.3 — Sơ Đồ Vòng Lặp Một Ván Đấu]** *Flowchart hoàn chỉnh dựa trên sơ đồ ASCII bên trên, màu phân biệt Shop Phase (xanh lá), Combat Phase (đỏ), điều kiện kết thúc (vàng).*

---

### 3.2.2 Pha Chuẩn Bị — Shop Phase

Đầu mỗi lượt, người chơi nhận coin theo công thức:

```
CurrentCoin = 10 + bonusNextTurn + permanentIncomeBonus
```

Thu nhập cơ sở **10 coin/lượt** cố định — không có streak bonus hay interest. Shop tự động làm mới 7 lá (5 unit + 2 spell) theo shop tier tăng tự động:

```
shopTier = clamp( ⌊(currentTurn + 1) / 2⌋, 1, 6 )
```

| Lượt | Shop Tier | Ý nghĩa |
|------|-----------|---------|
| 1–2  | Tier 1    | Unit tier thấp, chi phí 1 coin |
| 3–4  | Tier 2    | Bắt đầu thấy unit tier 2 |
| 5–6  | Tier 3    | Pool đa dạng, unit tier 3 phổ biến |
| 7–8  | Tier 4    | Unit mạnh bắt đầu xuất hiện |
| 9–10 | Tier 5    | Gần đến tier 5–6 |
| 11+  | Tier 6    | Pool đầy đủ, mọi tier |

Tier shop tăng tự động (không tốn coin) — khác biệt so với TFT/Hearthstone Battlegrounds, đặt focus hoàn toàn vào quyết định *mua gì* thay vì *lên level khi nào*.

**7 hành động trong Shop Phase:**

1. **Mua + đặt unit lên sân** — trigger `OnDeploy`, nhận tribe synergy ngay
2. **Giữ unit trong tay** — chờ merge, vẫn nhận global tribe buff
3. **Bán unit → +1 coin** bất kể giá mua hay mergeLevel (loại bỏ sunk cost)
4. **Reroll → 1 coin** — lấy 7 lá mới, hủy freeze
5. **Freeze → 0 coin** — giữ shop sang lượt sau, chỉ điền ô trống
6. **Mua + dùng spell** — tác động ngay, biến mất
7. **Nhấn Fight** — trigger `EndTurnShop` toàn sân trước combat

> **[HÌNH 3.4 — Giao Diện Shop Phase]** *Ảnh chụp màn hình Shop Phase: 7 ô shop, sân 7 slot, hand, HUD với HP/Cup/Coin, nút Reroll/Lock/BẮT ĐẦU.*

---

### 3.2.3 Pha Chiến Đấu — Combat Phase

**Kiến trúc then chốt — tách biệt tính toán và trình diễn:** `ResolveTurn()` tính toán hoàn toàn đồng bộ, ghi vào `combatLog`. Sau đó các action được phát lại theo trình tự (0.1s/action, 5–15s tổng) cho người chơi theo dõi. Cùng `ResolveTurn()` được dùng trong headless simulation của GA training.

Bên trong `ResolveTurn()`:
- *Giai đoạn Setup:* Tất cả unit trigger `StartOfBattle` và `Aura`
- *Battle Loop (tối đa 50 round):* Hàng đợi tấn công theo slot tăng dần (enemy trước, player sau cùng slot). `FindTarget()` ưu tiên: Taunt > Frontline > Backline. Sau mỗi đòn, `FlushDeathStack()` xử lý `OnDeath`/Reborn/Summon chain ngay lập tức.
- *Kết thúc:* Một phía bị xóa sổ → kết quả; sau 50 round cả hai còn unit → hòa.

> **[HÌNH 3.5 — Giao Diện Combat Phase]** *Ảnh chụp Combat Phase: sân người chơi (dưới) và sân đối thủ (trên) đối mặt, HUD hiển thị Cup thay Coin.*

---

### 3.2.4 Kết Quả Và Điều Kiện Kết Thúc

```
Thắng trận:  player còn unit, enemy bị xóa  →  Cup +1
Thua trận:   player bị xóa, enemy còn unit  →  HP  −1
Hòa:         cả hai còn unit sau 50 round   →  không đổi gì
```

| Điều kiện   | Khởi đầu | Ngưỡng kết thúc ván | Thay đổi/lượt |
|-------------|---------|---------------------|---------------|
| playerCups  | 0       | ≥ 10 → thắng        | +1 khi thắng  |
| playerHP    | 7       | ≤ 0 → thua          | −1 khi thua   |
| currentTurn | 1       | > 20 → thắng        | +1 mỗi lượt   |

Hòa không trừ HP — cả hai bên đều chơi tốt khi duy trì đủ lâu qua 50 round. Sau combat, sân được khôi phục về snapshot trước khi nhấn Fight — các unit triệu hồi trong battle biến mất, đội hình trở về đúng trạng thái ban đầu.

---

*[Tiếp theo: Mục 3.3 — Hệ Thống Bài (Card System)]*

## 3.3 Hệ Thống Bài (Card System)

Bài là đơn vị nội dung cơ bản của game. Sức mạnh của một lá bài không đến từ một thuộc tính duy nhất mà từ sự cộng hưởng của nhiều lớp: chỉ số cơ bản, tribe synergy, passive keywords, ability, và mức merge tích lũy.

---

### 3.3.1 Hai Loại Bài — Unit và Spell

**Unit card (47 lá):** Đặt lên sân, tham chiến. Thuộc tính định nghĩa trong JSON:

| Thuộc tính | Kiểu | Ý nghĩa |
|------------|------|---------|
| `cardID` | string | Khóa định danh duy nhất |
| `tribe` | enum | None / Babylon / Olympus / Niles |
| `baseATK` / `baseHP` | int | Chỉ số trước buff |
| `cost` | int | Coin mua trong shop |
| `tier` | int 1–6 | Cấp độ trong pool |
| `hasTaunt` / `hasReborn` / `hasSafeguard` | bool | Passive keywords |
| `abilities[]` | AbilityData[] | Kỹ năng theo mô hình TTE |
| `isToken` | bool | Không xuất hiện trong shop |

> **[HÌNH 3.6 — Giải Phẫu Lá Bài Unit]** *Ảnh chụp lá bài unit (ví dụ: Gilgamesh) với 8 thành phần chú thích: Tên, Tier icon, Character art, Ability icon + keywords, ATK, HP, Merge stars, Description text.*

**Spell card (21 lá):** Không có ATK/HP, không chiếm slot sân, mua xong dùng ngay rồi biến mất. Mỗi lượt shop có 2 ô spell — tạo quyết định thường xuyên: spell hay reroll/mua unit?

---

### 3.3.2 Hệ Thống Bộ Tộc (Tribe Synergy)

Synergy trong game là *emergent* (tự nổi lên), không phải *threshold* (ngưỡng cứng như TFT). Nhiều unit cùng bộ tộc → nhiều trigger cùng bộ tộc xảy ra → buff tích lũy nhiều hơn. Sức mạnh tribe tỉ lệ thuận *mềm* với số lượng unit.

**Babylon — cộng sinh qua vòng kinh tế:** Buff lẫn nhau khi có sự kiện deploy/sell.
- *Utu* (Tier 6): đồng minh Babylon deploy → toàn Babylon +2/+2 vĩnh viễn
- *Ashur* (Tier 5): bất kỳ đồng minh bị bán → toàn đội +1/+2 vĩnh viễn
- *Lamashtu/Uridimmu*: buff ATK/HP Babylon mỗi khi đồng minh Babylon bị bán

Chiến lược điển hình: *snowball* dài hạn — yếu đầu game, khó chặn ở late-game sau nhiều lượt trigger.

**Niles — cộng sinh qua cái chết và tái sinh:** Buff lẫn nhau qua summon/reborn/death.
- *Anubis* (Tier 6): đồng minh chết → ban Reborn cho đồng minh HP thấp nhất (max 2 lần)
- *Osiris* (Tier 6): đồng minh hồi sinh → nhân đôi/ba/bốn chỉ số unit đó
- *Sobek* (Tier 5): đồng minh summon/reborn → bản thân +1/+2 vĩnh viễn
- *Thoth* (Tier 5, Reborn): khi bị tiêu diệt → toàn bộ Niles nhận vĩnh viễn +2 ATK (kể cả unit mua sau, qua `globalTribeBuff`)

Chiến lược điển hình: *chaos resilience* — đội hình dễ chết nhưng mỗi cái chết kích hoạt chuỗi buff và hồi sinh.

**Olympus — tấn công thuần túy *(thiết kế dự kiến)*:** ATK synergy qua combat events. *Lưu ý: phiên bản hiện tại chưa có unit Olympus; gene[19] (sOlympus) được giữ trong kiến trúc để mở rộng về sau.*

> **[HÌNH 3.7 — So Sánh Ba Triết Lý Tribe Synergy]** *Bảng 3 cột: Babylon/Niles/Olympus theo trigger chính, giai đoạn mạnh nhất, rủi ro, unit tiêu biểu.*

---

### 3.3.3 Passive Keywords

Ba keyword tồn tại từ đầu đến hết combat trừ khi bị tiêu thụ.

**Taunt — buộc phải bị nhắm:**

```
FindTarget():
  1. Có unit isTaunt = true trên sân địch? → tấn công unit đó (ưu tiên tuyệt đối)
  2. Không có Taunt → tấn công unit sống ở slot nhỏ nhất (frontline)
```

Kiểm soát hoàn toàn luồng damage. Taunt có thể được *grant* qua GiveBuff — tạo ra "bẫy" buộc địch phải đánh.

**Reborn — hồi sinh một lần:**

```csharp
// CardInstance.ReviveDefault()
currentHP = 1;
isReborn  = false;   // đã dùng, không Reborn lần nữa
hasRebornUsed = true;
```

Unit vẫn kích hoạt `OnDeath` *trước khi* Reborn phục hồi — combo Horus (OnDeath: buff toàn đội) + Reborn = buff đội hai lần trong một ván chiến.

**Safeguard — chặn một đòn:**

```
ExecuteClash:
  if (defender.safeguardActive):
    dmgToDefender = 0       // chặn hoàn toàn
    safeguardActive = false // tiêu thụ
```

Tương tác giữa ba keyword: **Taunt + Reborn** = địch tốn 2 round để hạ một unit; **Reborn + OnDeath** = "chết" một lần hợp lệ để kích hoạt trigger rồi sống lại.

---

### 3.3.4 Hệ Thống Merge — Đầu Tư Dài Hạn

```
mergeLevel 0→1 (1 sao → 2 sao): cần 3 bản sao
mergeLevel 1→2 (2 sao → 3 sao): cần 2 bản sao
```

**Công thức chỉ số sau merge:**

```
currentATK = round( baseATK × tier  +  0.7 × (growthATKBonus + permanentATKBonus) )
             + tempSpellATKBonus + globalPermATKBonus

maxHP      = round( baseHP × tier   +  0.7 × (growthHPBonus  + permanentHPBonus)  )
             + tempSpellHPBonus + globalPermHPBonus
```

Tier = mergeLevel + 1: 2 sao → chỉ số nhân đôi, 3 sao → nhân ba. Hệ số `0.7` cho growth/permanent bonus nghĩa là buff tích lũy qua nhiều lượt cũng được nhân theo merge.

Ví dụ — *Kingu* (Babylon Tier 2, ATK 2, HP 2, Taunt + Reborn, không ability):
- Nhìn thuần túy: unit yếu nhất có thể
- Thực tế: deploy → trigger Utu buff toàn Babylon; Taunt + Reborn buộc địch tốn 2 round; 3 sao = tank frontline với Babylon chain trigger x3
- Đây là ví dụ điển hình: lá bài "đơn giản" trở nên quan trọng trong ngữ cảnh đúng

Khi thiếu 1 bản sao để merge, lá bài trong shop nhấp nháy (Merge Hint) — UX tránh bỏ lỡ cơ hội merge.

---

*[Tiếp theo: Mục 3.4 — Hệ Thống Shop]*

## 3.4 Hệ Thống Shop

Shop là giao diện giữa người chơi và sự ngẫu nhiên — bộ lọc kiểm soát những lá bài nào có thể xuất hiện, theo tỉ lệ nào.

---

### 3.4.1 Kiến Trúc Pool Và Weighted Random

Toàn bộ 47 unit card (không gồm token) tạo thành một pool đơn, chia tầng theo `tier`. Mỗi lượt, 5 ô unit và 2 ô spell được roll **độc lập** — cùng card có thể xuất hiện nhiều lần trong cùng một shop, cho phép hoàn thành bộ merge mà không cần reroll nhiều lần.

Cơ chế trung tâm là `RollTier(shopLevel)` — weighted random theo bảng 6×6:

| Shop Level | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 | Tier 6 |
|:----------:|:------:|:------:|:------:|:------:|:------:|:------:|
| 1          | 100%   | 0%     | 0%     | 0%     | 0%     | 0%     |
| 2          | 70%    | 30%    | 0%     | 0%     | 0%     | 0%     |
| 3          | 50%    | 35%    | 15%    | 0%     | 0%     | 0%     |
| 4          | 25%    | 40%    | 25%    | 10%    | 0%     | 0%     |
| 5          | 15%    | 25%    | 35%    | 15%    | 10%    | 0%     |
| 6          | 10%    | 15%    | 20%    | 25%    | 20%    | 10%    |

```csharp
int roll = Random.Range(0, 100);
int cumulative = 0;
for (int tierIndex = 0; tierIndex < 6; tierIndex++) {
    cumulative += shopDropRates[levelIndex, tierIndex];
    if (roll < cumulative) return tierIndex + 1;
}
```

Sau khi có tier, hệ thống lọc pool theo `tier == rolledTier && !isToken`, rồi chọn ngẫu nhiên đều. Nếu pool trống: fallback về tier ≤ rolledTier (unit) hoặc bất kỳ spell nào — đảm bảo shop luôn đủ 7 lá.

**Đọc bảng:** Tier 1 vẫn còn 10% ở shop level 6 — người chơi có thể tìm bản sao merge unit thấp tier ở late-game. Tier 6 chỉ xuất hiện 10% ở shop level tối đa — phần thưởng hiếm cho người chơi sống sót đủ lâu.

---

### 3.4.2 Đóng Băng Shop (Freeze)

Freeze tốn **0 coin** nhưng có chi phí ẩn: từ bỏ quyền xem 7 lá mới đầu lượt tiếp theo.

```
Freeze trước khi nhấn Fight
  → Đầu lượt tiếp theo: FillEmptyShopSlots()
       • Ô đã mua (trống) → điền card mới theo shopTier hiện tại
       • Ô chưa mua (còn card) → giữ nguyên
  → isShopFrozen = false  (reset sau một lượt)
```

Nên freeze khi thấy lá bài cần thiết nhưng chưa đủ coin; không nên freeze khi shop hiện tại không có gì đặc biệt. Reroll trong lượt hiện tại hủy freeze — không thể vừa freeze vừa reroll.

---

### 3.4.3 Global Tribe Buff

Mỗi `CardInstance` mới tạo trong shop tự động nhận toàn bộ global tribe buff đã tích lũy:

```csharp
// GameManager.Shop.cs — CreateCardInSlot()
CardInstance instance = new CardInstance(data, 0);
ApplyGlobalPermBuffToNewUnit(instance);  // áp ngay khi tạo
```

Nếu Thoth đã kích hoạt buff +2 ATK toàn Niles, mọi unit Niles *xuất hiện trong shop sau đó* đã có sẵn +2 ATK — người chơi thấy chỉ số thực tế, không cần tính thêm. Thoth chết sớm vẫn có giá trị: nó khóa +2 ATK vào mọi Niles unit sẽ mua cho đến hết ván.

---

*[Tiếp theo: Mục 3.5 — Hệ Thống Kinh Tế]*

## 3.5 Hệ Thống Kinh Tế

### 3.5.1 Thu Nhập Và Chi Tiêu

Mỗi lượt, người chơi nhận coin theo ba thành phần độc lập:

```
CurrentCoin = 10  +  bonusNextTurn  +  permanentIncomeBonus
bonusNextTurn ← 0 sau khi áp dụng
```

- **Base 10:** Cố định, không có streak bonus hay interest. Mọi quyết định kinh tế tập trung vào chi tiêu, không tích lũy.
- **bonusNextTurn:** Thưởng trì hoãn từ spell (ví dụ *Wager* +3 coin nếu thắng trận tiếp theo).
- **permanentIncomeBonus:** Cộng dồn suốt ván từ spell đặc biệt (ví dụ *Caishen's Knock* +1/lượt, mua ở lượt 5 → hòa vốn lượt 7, sinh lãi từ lượt 8).

Coin rời tài khoản qua bốn kênh:

| Kênh | Chi phí | Giá trị nhận được | Loại đầu tư |
|------|---------|-------------------|-------------|
| Mua unit | = `card.cost` | Unit lên sân/tay | Sức mạnh tức thời / merge tiến độ |
| Mua spell | = `spell.cost` | Hiệu ứng tức thì | Tùy loại spell |
| Reroll | 1 | 7 lá bài mới | Thông tin và cơ hội |
| Bán unit | −1 (nhận) | +1 coin + giải phóng slot | Tái phân bổ nguồn lực |

---

### 3.5.2 Phân Loại Spell Theo Chức Năng Kinh Tế

21 spell phục vụ sáu chức năng kinh tế khác nhau:

**Nhóm 1 — Buff Chỉ Số Trực Tiếp**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Sharpen Blade | 1 | 1 | +3/+0 vĩnh viễn cho 1 đồng minh |
| Plated Armor | 1 | 1 | +0/+3 + Taunt vĩnh viễn cho 1 đồng minh |
| Let's Feast | 1 | 2 | +0/+2 cho 3 đồng minh trên board (trận này) |
| Balance Stance | 1 | 2 | +3/+3 vĩnh viễn cho 1 đồng minh |
| Strengthen Bond | 2 | 1 | +1/+1 vĩnh viễn cho toàn bộ đồng minh |
| Divine Inspiration | 3 | 2 | ATK+HP = shop tier hiện tại cho 1 đồng minh |
| Olympic Flame | 4 | 3 | Nhân đôi ATK + Safeguard cho 1 đồng minh |
| Change of Heart | 2 | 2 | Taunt → xóa Taunt +3/+0 \| không Taunt → +Taunt +0/+5 |

**Nhóm 2 — Tuyển Quân (Recruiting)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Quick Recruit | 1 | 2 | Nhận 1 unit ngẫu nhiên Tier 1 vào Hand |
| Sanctum Heist | 1 | 2 | Lấy 1 unit ngẫu nhiên từ Shop hiện tại vào Hand |
| Tailored Recruit | 2 | 3 | Chọn 1 đồng minh → nhận 1 unit cùng tộc khác |
| Military Support | 5 | 5 | Nhận 3 unit ngẫu nhiên Tier 5 vào Hand |

**Nhóm 3 — Kinh Tế Coin**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Ancient Coin | 1 | 1 | Nhận 1 coin ngay |
| Trader's Trick | 2 | 1 | +2 coin lượt này |
| Caishen's Knock | 2 | 2 | +1 thu nhập vĩnh viễn/lượt |

**Nhóm 4 — Nâng Cấp Unit**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Rising Spirit | 3 | 2 | Grant EndTurnShop: +2/+2 mỗi lượt cho 1 đồng minh |
| Ritual of the Realm | 3 | 3 | Nâng 1 unit cùng tộc lên +1 merge level |

**Nhóm 5 — Tái Phân Bổ**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Gate of Destruction | 4 | 3 | Loại bỏ 1 đồng minh → toàn chỉ số chuyển cho 1 đồng minh ngẫu nhiên |

**Nhóm 6 — Rủi Ro Cao**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Devil's Deal | 4 | 1 | Mất 1 Mạng → nhận 6 coin ngay |
| Wager | 2 | 1 | Nếu thắng trận kế tiếp → nhận 3 coin |

*Wager* — expected value: `E = 4p − 2` (dương khi p > 0.5). Nhóm này được BotAgent gán điểm âm cứng trừ khi `genes[16]` (eGainCoin) và `genes[31]` (wSpellEconomy) rất cao.

**Permanent vs one-time:** Cùng coin, permanent luôn đáng hơn vì tồn tại qua mọi lượt còn lại. *Balance Stance* (2 coin → +3/+3 vĩnh viễn) vs *Let's Feast* (2 coin → +0/+2 cho 3 unit trận này). Với 15 lượt còn lại, Balance Stance tương đương 15× giá trị chiến đấu từ cùng chi phí.

---

### 3.5.3 Sơ Đồ Lưu Thông Coin

```
┌──────────────────────────────────────────────────────┐
│                  NGUỒN THU NHẬP                       │
│                                                      │
│  Base 10/lượt ──┐                                    │
│  bonusNextTurn ─┼──► CurrentCoin                     │
│  permanentBonus─┘                                    │
│  Bán unit (+1) ─────► CurrentCoin                    │
│  Thắng Wager ───────► CurrentCoin                    │
│                                                      │
├──────────────────────────────────────────────────────┤
│                  KÊNH CHI TIÊU                        │
│                                                      │
│  Mua unit ◄──── CurrentCoin ────► Mua spell          │
│  Reroll (1) ◄────────────────────► (giữ = không chi) │
│                                                      │
├──────────────────────────────────────────────────────┤
│              GIÁ TRỊ TẠO RA (Output)                 │
│                                                      │
│  Unit trên sân → Combat power → Cup / HP              │
│  Spell stat    → Combat power (permanent/one-time)   │
│  Spell recruit → Unit mới (→ merge tiến độ)          │
│  Spell economy → Nhiều coin lượt sau (tái đầu tư)    │
│  Reroll info   → Cơ hội tìm unit/merge bản sao       │
└──────────────────────────────────────────────────────┘
```

Không có "cách chơi kinh tế sai" hoàn toàn — mỗi kênh tạo ra giá trị theo cách khác nhau. Câu hỏi là "loại giá trị nào cần nhất lúc này?" — đó là bài toán mà Chromosome trong Chương 5 học cách trả lời qua hàng nghìn trận mô phỏng.

---

*[Tiếp theo: Mục 3.6 — Hệ Thống Chiến Đấu]*

## 3.6 Hệ Thống Chiến Đấu (Combat System)

### 3.6.1 Bố Cục Sân — Frontline Và Backline

Sân chiến đấu có **7 slot** (0–6), chia thành hai vùng:

```
 ┌───┬───┬───┬───┬───┬───┬───┐
 │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │  ← slot index
 └───┴───┴───┴───┴───┴───┴───┘
  ◄─── FRONTLINE ───►◄─ BACK ─►
     slot 0–3 (4 ô)   slot 4–6 (3 ô)
```

> **[HÌNH 3.8 — Sơ Đồ Sân Chiến Đấu 7 Slot]** *Sân 7 slot với hai vùng màu khác nhau: Frontline (slot 0–3) và Backline (slot 4–6). Mũi tên minh họa hướng tấn công: địch nhắm frontline trước, chỉ vào backline khi frontline trống.*

**Frontline (0–3):** Luôn lộ ra trước đòn địch.  
**Backline (4–6):** Chỉ bị nhắm khi toàn bộ frontline đã bị tiêu diệt (kể cả sau Reborn và unit được triệu hồi vào frontline trong combat).

Phân chia 4:3 là có chủ ý: phần lớn đội hình ở frontline, backline là vị trí đặc quyền cho unit có chỉ số thấp nhưng ability quan trọng (Thoth, Osiris…).

---

### 3.6.2 Thứ Tự Tấn Công — Attack Queue

Mỗi round, **tất cả unit còn sống** tấn công một lần theo thứ tự slot tăng dần, enemy trước player ở cùng slot:

```
Slot 0 (enemy) → Slot 0 (player) → Slot 1 (enemy) → Slot 1 (player) → ... → Slot 6
(Slot trống hoặc ATK = 0 → bỏ qua)
```

> **[HÌNH 3.9 — Thứ Tự Tấn Công Trong Một Round]** *Sơ đồ hai sân đối mặt, số thứ tự 1–14 đánh theo hàng đợi. Slot trống gạch chéo xám.*

Thứ tự tấn công là **động**: unit mới triệu hồi hoặc hồi sinh trong round được chèn vào đúng vị trí slot trong phần hàng đợi chưa xử lý — bảo toàn thứ tự nhất quán dù board thay đổi liên tục.

---

### 3.6.3 Cơ Chế Va Chạm — Clash Đồng Thời

Mỗi lần tấn công là một **Clash** — attacker và defender nhận damage đồng thời:

```
Clash(attacker A, defender D):
  damage_to_D = A.currentATK
  damage_to_A = D.currentATK
  [kiểm tra Safeguard — nếu có, block 1 phía]
  D.currentHP -= damage_to_D
  A.currentHP -= damage_to_A
```

Không có miss hay critical — mọi đòn trúng, chỉ Safeguard block. Combat hoàn toàn deterministic từ trạng thái hai sân.

Triggers phụ sau clash: `OnAttack` chỉ kích hoạt nếu attacker **còn sống** sau đòn; `OnTakeDamage` chỉ kích hoạt nếu unit nhận damage **còn sống** sau khi nhận.

---

### 3.6.4 Chọn Mục Tiêu — Ba Tầng Ưu Tiên

```
Tầng 1 — TAUNT (ưu tiên tuyệt đối)
  Có unit isTaunt = true → bắt buộc tấn công unit Taunt gần nhất
  → Bypass hoàn toàn frontline/backline

Tầng 2 — FRONTLINE
  Không có Taunt → tấn công unit sống ở frontline (slot 0–3) gần nhất

Tầng 3 — BACKLINE (chỉ khi frontline đã trống)
  Tấn công unit sống ở backline (slot 4–6) gần nhất
```

"Gần nhất" = khoảng cách slot tuyệt đối (`|attackerSlot − targetSlot|`). Taunt bypass frontline — một unit Taunt ở backline vẫn bị nhắm dù toàn bộ frontline còn sống.

---

### 3.6.5 Xử Lý Cái Chết — Death Stack

Cái chết đi qua **Death Stack** (LIFO) — đảm bảo mọi chuỗi phản ứng được resolve hoàn toàn trước khi combat tiếp tục:

```
Sau mỗi Clash:
1. Scan board: unit nào HP ≤ 0 → đẩy vào death stack (RegisterDeath)

2. FlushDeathStack: lặp cho đến khi stack rỗng VÀ không còn pending summon:
   a. Pop victim từ stack
   b. TriggerAbility(OnDeath, victim)
   c. Scan board tìm death mới
   d. Broadcast OnAllyDeath cho đồng minh cùng tộc còn sống
   e. Scan lại
   f. CleanupBoard:
      - isReborn và chưa dùng? → ReviveDefault (HP=1, isReborn=false)
        → Broadcast OnAllyReborn + OnAllySummon
      - Không có Reborn → xóa khỏi slot (null)
   g. Nếu stack rỗng VÀ có pending summon: pop một summon, xử lý
   h. Lặp lại nếu còn death mới hoặc pending summon

3. Insert unit mới (summon/reborn) vào đúng vị trí queue
```

> **[HÌNH 3.10 — Quy Trình Death Stack (FlushDeathStack)]** *Flowchart vòng lặp FlushDeathStack với nhánh Reborn (xanh) và không Reborn (xám).*

LIFO đảm bảo "chuỗi con" hoàn chỉnh trước khi chuỗi cha tiếp tục. Giới hạn 500 vòng lặp bảo vệ chống loop vô hạn lý thuyết.

---

### 3.6.6 Reborn Trong Combat

Reborn không phải "không chết" — là "chết rồi sống lại". Unit Reborn vẫn trigger `OnDeath` trước khi hồi sinh:

```
Unit bị HP về 0 (có Reborn):
  → Đưa vào death stack → OnDeath fires (Horus buff toàn đội)
  → CleanupBoard: ReviveDefault → HP=1, isReborn=false
  → Broadcast OnAllyReborn → Osiris nhân đôi/ba chỉ số
  → Broadcast OnAllySummon → Sobek +1/+2
  → Unit vào lại attack queue với HP=1
```

> **[HÌNH 3.11 — Chuỗi Trigger Khi Reborn]** *Timeline ngang: HP về 0 → OnDeath fires → ReviveDefault HP=1 → OnAllyReborn (Osiris) → OnAllySummon (Sobek) → unit vào lại queue.*

Unit hồi sinh với HP=1 — một đòn bất kỳ đủ để hạ lần thứ hai. Đây là counterplay rõ ràng: giữ unit ATK đủ để one-shot HP=1 sau Reborn.

---

### 3.6.7 Hòa Và Tổng Hợp

Sau **50 round**, nếu cả hai phía vẫn còn unit sống: kết quả **hòa** — không ai mất HP. Giới hạn 50 được chọn để rất hiếm khi đạt được trong gameplay bình thường (trận đầy đủ 7v7 thường kết thúc trong 10–20 round).

Toàn bộ hệ thống chiến đấu xây dựng trên một nguyên tắc nhất quán: **mọi mechanic tạo ra quyết định** cho người chơi trong Shop Phase — bố cục sân, thứ tự tấn công, target selection, Clash đồng thời, Death Stack LIFO, Reborn timing. Không có may mắn trong combat (không miss, không critical) — kết quả hoàn toàn deterministic từ trạng thái hai sân. Người chơi học được pattern và đưa ra quyết định shop tốt hơn qua nhiều ván.

---

*[Tiếp theo: Mục 3.7 — Cân Bằng Game (Balancing)]*

## 3.7 Cân Bằng Game (Balancing)

Cân bằng không có nghĩa là mọi lá bài có sức mạnh bằng nhau — nó có nghĩa là độ mạnh yếu được phân bổ theo chiều sâu chiến lược, không theo chiều "tốt hơn hoàn toàn".

---

### 3.7.1 Phân Phối Bài Theo Tier

**Unit cards (47 lá):**

| Tier | Babylon | Niles | Tổng unit | Tỉ lệ |
|:----:|:-------:|:-----:|:---------:|:-----:|
| 6    | 3       | 3     | 6         | 12.8% |
| 5    | 4       | 4     | 8         | 17.0% |
| 4    | 6       | 4     | 10        | 21.3% |
| 3    | 3       | 4     | 7         | 14.9% |
| 2    | 3       | 4     | 7         | 14.9% |
| 1    | 4       | 5     | 9         | 19.1% |

**Spell cards (21 lá):**

| Tier | Spell | Ghi chú |
|:----:|:-----:|:--------|
| 1    | 8     | 7 non-token + 1 token (Tinh Hoa Hợp Nhất) |
| 2    | 6     | Bao gồm spell kinh tế chính (Caishen's Knock) |
| 3    | 3     | Rising Spirit, Ritual, Divine Inspiration |
| 4    | 3     | Olympic Flame, Gate of Destruction, Devil's Deal |
| 5    | 1     | Military Support — spell đắt nhất (5 coin) |

Tier 4 đông nhất (21.3%) — vùng mid-game phong phú, trùng với giai đoạn người chơi định hình đội hình cuối (shop level 4–5, drop rate 25–40%). Tier 5–6 ít unit có chủ ý: pool nhỏ → xác suất ra đúng unit mong muốn cao hơn khi tier đủ cao để roll vào.

> **[HÌNH 3.12 — Phân Phối 68 Lá Bài Theo Tier]** *Biểu đồ cột đôi: unit (47 lá, phân tầng Babylon/Niles) và spell (21 lá). Trục hoành Tier 1–6, tổng số mỗi tier ghi trên đầu cột.*

---

### 3.7.2 Triết Lý Chỉ Số — Stat Không Nói Lên Tất Cả

Tier của unit xác định *khi nào* nó mạnh nhất, không phải *tại sao* nó mạnh. Nhiều unit tier cao có base stat thấp — sức mạnh đến từ ability:

| Unit | Tribe | Tier | ATK | HP | Nguồn sức mạnh thực sự |
|------|-------|:----:|:---:|:--:|------------------------|
| Osiris | Niles | 6 | 3 | 3 | ScaleTargetStats khi OnAllyReborn — nhân đôi/ba/bốn chỉ số |
| Anubis | Niles | 6 | 5 | 5 | Grant Reborn mỗi khi đồng minh chết (≤2/4/6 lần) |
| Sobek | Niles | 5 | 2 | 2 | +1/+2 vĩnh viễn mỗi lần đồng minh summon/reborn |
| Enki | Babylon | 5 | 2 | 15 | Taunt + phản dame 5/10/15 khi bị đánh |
| Humbaba | Babylon | 4 | 7 | 1 | Nhận 2 coin mỗi khi tấn công |
| Pazuzu | Babylon | 3 | 1 | 2 | Nhận 2 coin khi deploy |

*Osiris (3/3 tier 6)* là ví dụ cực đoan: raw stat bằng unit tier 2, nhưng mỗi Reborn đồng minh → nhân đôi/ba chỉ số unit đó ngay lập tức. Sức mạnh đến từ hệ sinh thái xung quanh, không từ bản thân.

---

### 3.7.3 Combo Mạnh Và Lever Kiềm Chế

**Combo 1 — Taunt + Reborn (Kingu):**
Kingu (tier 2, 2/2, Taunt + Reborn) buộc địch tốn 2 round để hạ một unit tier 2. Tự cân bằng bẩm sinh: ở late-game khi địch có ATK 15+, 1 HP sau Reborn bị hạ trong đúng một đòn tiếp theo. Mạnh ở early-mid, tự yếu dần ở late — không cần điều chỉnh số liệu.

**Combo 2 — Anubis + Reborn + Osiris (Niles Reborn Chain):**
```
Đồng minh chết → Anubis: Grant Reborn cho LowestHealthAlly
→ Ally đó chết → Reborn (HP=1)
→ Osiris: ScaleTargetStats(x2/x3/x4) lên unit vừa hồi sinh
→ Unit từ trung bình → cực mạnh trong 1 round
```
Hai lever cân bằng: (1) `triggerLimit = 2 + isScaledTriggerLimit` — Anubis chỉ grant Reborn tối đa 2 lần/trận ở merge level 0; (2) unit được scale bắt đầu với đúng 1 HP — một đòn bất kỳ đủ xử lý.

**Combo 3 — Gilgamesh + Buff Chain (Babylon Snowball):**
Gilgamesh `OnStatGain`: mỗi lần nhận permanent buff từ *bên ngoài* → tự cộng thêm +2/+1 vĩnh viễn. Flag `_firingOnStatGain = true` ngăn đệ quy vô hạn. `keepRatio = 0.7` trong công thức merge giảm hiệu quả tích lũy: dù có 50 permanent ATK bonus, thực tế chỉ nhận 35 — snowball không vô hạn mà vẫn đủ để cảm giác đáng đầu tư.

---

### 3.7.4 Quy Trình Cân Bằng — Lặp Qua Thực Nghiệm

**Vòng lặp:** Thiết kế → Test (GA training + playtest) → Quan sát → Điều chỉnh.

GA training là công cụ *stress test*: nếu tất cả 5 specialist bots hội tụ về cùng một chiến lược, đó là dấu hiệu một tribe đang quá mạnh. Fitness convergence curve của từng archetype bot cho biết chiến lược nào không thể cạnh tranh.

| Vấn đề quan sát | Lever điều chỉnh |
|-----------------|-----------------|
| Unit quá mạnh so với tier | Giảm base stat hoặc thêm triggerLimit |
| Combo kích hoạt quá nhiều lần | Giảm triggerLimit hoặc thêm conditionCount |
| Tribe quá yếu — bot không chọn | Tăng effectValue hoặc bỏ bớt điều kiện kích hoạt |
| Snowball quá nhanh | Giảm keepRatio hoặc thêm điều kiện subjectTribe |
| Merge không đáng đầu tư | Thêm isScaledTriggerLimit để reward merge |

*Ví dụ cụ thể — Sekhmet:* Thiết kế ban đầu không giới hạn lần nuốt unit. Kết quả test: Sekhmet nuốt toàn bộ 6 đồng minh → chỉ số của toàn đội. Điều chỉnh: thêm `triggerLimit = 2 + isScaledTriggerLimit`. Kết quả: tối đa 2 lần nuốt ở merge level 0 — đủ identity riêng, không phá vỡ toàn đội hình.

Giới hạn: GA tối ưu hóa cho fitness function, không cho trải nghiệm người chơi. Điều chỉnh cuối cùng kết hợp cả bot training (khách quan) và playtest thực tế (cảm giác gameplay).

---

### 3.7.5 Asymmetry Có Chủ Đích — Babylon Vs. Niles

**Babylon** là bộ tộc *accumulation*: tích lũy chậm, phụ thuộc vào **thời gian** và **kích thước đội hình**, mạnh khi trận kéo dài.

**Niles** là bộ tộc *reaction*: mạnh lên bất ngờ qua chuỗi tử-sinh, phụ thuộc vào **đúng tổ hợp unit**, không cần thời gian dài để scale.

Asymmetry tạo ra counterplay tự nhiên: Babylon buff tích lũy không bị ảnh hưởng bởi combat outcomes — dù thua một trận, buff vẫn còn đó. Niles Reborn chain làm Babylon khó tiêu diệt toàn bộ Niles dễ dàng. Chính sự không đối xứng — chứ không phải cân bằng đối xứng hoàn toàn — là nguồn gốc của chiều sâu chiến thuật.

---

*[Tiếp theo: Mục 3.8 — Thiết Kế UI/UX]*

## 3.8 Thiết Kế Giao Diện Và Trải Nghiệm Người Dùng (UI/UX)

Nguyên tắc xuyên suốt: **mỗi phần tử giao diện phải phục vụ một quyết định gameplay cụ thể** — không có thông tin thừa, không có bước tương tác không cần thiết.

---

### 3.8.1 Hai Trạng Thái Giao Diện

Toàn bộ giao diện tồn tại trong hai trạng thái phân biệt hoàn toàn:

```
SHOP PHASE                          COMBAT PHASE
─────────────────────────           ─────────────────────────
shopPanel: Active        ←──── ────→ shopPanel: Hidden
handPanel: Active                    handPanel: Hidden
enemyBoardPanel: Hidden             enemyBoardPanel: Active
Roll/Lock buttons: Hiện             Roll/Lock buttons: Ẩn
Nút: "BẮT ĐẦU"                     Nút: "LƯỢT TIẾP"
Background: shopSprite              Background: combatSprite
Resource icon: Coin                 Resource icon: Cup
ShopTierPanel: Hiện                 ShopTierPanel: Ẩn
```

> **[HÌNH 3.13 — So Sánh Hai Trạng Thái Giao Diện]** *Hai ảnh chụp màn hình đặt cạnh nhau: (trái) Shop Phase, (phải) Combat Phase. Các thành phần thay đổi được khoanh vòng và nối bằng mũi tên.*

Background và nhạc nền thay đổi đồng thời (`PlayPrepBGM()` / `PlayCombatBGM()`) — tín hiệu trực quan mạnh nhất về phase hiện tại. HUD hiển thị liên tục: HP (số nguyên), Cup (tiến độ thắng), và resource widget thay đổi icon theo phase (coin trong shop, cup trong combat). Khi có `permanentIncomeBonus > 0`, hiển thị dạng `"12G (+2)"`.

---

### 3.8.2 Giải Phẫu Một Lá Bài — Tám Lớp Thông Tin

```
┌─────────────────────────────┐
│ [NAME]         [TIER ICON]  │ ← tên + tier pool
│                             │
│       [CHARACTER ART]       │ ← nhận diện visual nhanh
│                             │
│ [ABILITY ICON] [KEYWORDS]   │ ← TTE effect + Taunt/Reborn/Safeguard icons
│                             │
│ [ATK]              [HP]     │ ← stats (màu HP đỏ khi bị thương)
│                             │
│ [★ ★ ☆]                    │ ← merge stars (1-3)
│                             │
│ [description text]          │ ← mô tả ability
└─────────────────────────────┘
  ↑ Frame blink vàng = merge hint
```

> **[HÌNH 3.14 — Giải Phẫu Giao Diện Lá Bài]** *Ảnh chụp lá bài unit trong game với 8 thành phần chú thích. Frame blink vàng khi có merge hint.*

Stat Punch Animation khi ATK/HP thay đổi: tăng → số phóng to 1.45× rồi thu lại trong 0.22s, flash xanh lá; giảm → flash đỏ fade về màu gốc trong 0.28s.

---

### 3.8.3 Drag And Drop

Toàn bộ tương tác Shop Phase thực hiện bằng drag and drop — không có menu bối cảnh hay nút mua riêng:

| Destination | Source | Hành vi |
|-------------|--------|---------|
| PlayerBoard (ô trống) | Shop | Mua + Deploy (trừ coin, trigger OnDeploy) |
| PlayerBoard (ô trống) | Hand | Di chuyển + Deploy (không trừ coin) |
| PlayerBoard (ô trống) | PlayerBoard | Đổi chỗ trong board |
| Hand (ô trống) | Shop | Mua vào tay (không Deploy) |
| SellZone | Board/Hand | Bán (+1 coin, trigger OnSell) |
| Unit slot | Spell từ shop | Mua + Cast spell lên unit đó |

SellZone (nền đỏ, chữ "BÁN +1G") chỉ xuất hiện khi đang kéo card từ Board/Hand — contextual UI, ẩn ngay khi thả. Spell cần target được kéo đặt trực tiếp lên unit mục tiêu — cử chỉ kéo-thả là hành động chọn target.

---

### 3.8.4 Phản Hồi Merge

Merge là khoảnh khắc đáng nhấn mạnh — ba lớp phản hồi đồng thời:

1. **BurstAnimation:** Lá bài sau merge phát hiệu ứng bùng phát
2. **StarUp sound effect:** Âm thanh riêng biệt cho khoảnh khắc lên sao
3. **Token "Tinh Hoa Hợp Nhất":** `S_00` tự xuất hiện trong Hand — phần thưởng gameplay thực sự, không chỉ visual

Merge Hint blink (frame nhấp nháy vàng khi thiếu 1 bản sao):

```csharp
float t = (Mathf.Sin(Time.unscaledTime * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
frameBackground.color = Color.Lerp(originalColor, blinkColor, t);
```

`blinkSpeed = 4f` Hz, `unscaledTime` đảm bảo blink không bị ảnh hưởng bởi `Time.timeScale`.

---

### 3.8.5 Ngôn Ngữ Màu Sắc

| Màu | Ý nghĩa | Nơi dùng |
|-----|---------|----------|
| Xanh lá (`#40FF58`) | Buff / tăng stat | Stat punch tăng, SynergyNiles flash |
| Đỏ (`#FF2626`) | Debuff / giảm stat / nguy hiểm | Stat punch giảm, HP damaged, SellZone |
| Xanh dương (`#4D99FF`) | Trạng thái / thông tin | Status effect flash |
| Vàng kim (`#FFD90D`) | Babylon synergy / merge hint | Babylon buff flash, blink frame |
| Cyan (`#1AD9FF`) | Olympus synergy / freeze active | Olympus buff flash, Lock button active |
| Lục (`#1AFF73`) | Niles synergy | Niles buff flash |
| Trắng | Bình thường | HP đầy, stat gốc |

Hệ màu nhất quán cho phép đọc trạng thái combat mà không cần đọc text. DifficultyPanel (Easy/Medium/Hard) và EndGamePanel (Thắng/Thua + Restart) hoàn thiện luồng trải nghiệm tối giản: chọn độ khó → chơi → nhận kết quả → restart.

---

*[Kết thúc Chương 3 — Thiết Kế Game (GDD)]*
*[Tiếp theo: Chương 4 — Kiến Trúc Hệ Thống Kỹ Thuật]*

\newpage

# CHƯƠNG 4: KIẾN TRÚC HỆ THỐNG KỸ THUẬT

## 4.1 Ba Ràng Buộc Kiến Trúc

Dự án đặt ra ba yêu cầu cứng — vi phạm bất kỳ yêu cầu nào đều khiến dự án không thể hoàn thành:

- **Data-driven:** Mọi tham số nội dung game (chỉ số lá bài, giá trị kỹ năng, drop rate) phải định nghĩa trong JSON, không cứng hóa trong code. Mục tiêu: điều chỉnh balance mà không cần recompile.
- **Headless-compatible:** GA training đòi hỏi hàng trăm nghìn trận đấu mô phỏng. Toàn bộ phần tính toán (combat, quyết định mua bài, đánh giá board) phải chạy được không cần MonoBehaviour, không cần scene, không cần màn hình.
- **Phụ thuộc một chiều:** Thành phần cấp dưới (core engine, data) không được biết đến thành phần cấp trên (manager, UI). AI system có thể dùng combat engine mà không biết UI tồn tại.

---

## 4.2 Kiến Trúc Phân Tầng

```
┌─────────────────────────────────────────────────────────┐
│                    TẦNG UI                              │
│   CardUI · CardDraggable · CardSlot · UIManager         │
│   (MonoBehaviour — chỉ hiển thị, không chứa logic game) │
├─────────────────────────────────────────────────────────┤
│                  TẦNG MANAGER                           │
│   GameManager (partial) · CardDatabase · EconomyManager  │
│   AIManager · AudioManager                              │
│   (Singleton MonoBehaviour — điều phối game state)      │
├─────────────────────────────────────────────────────────┤
│                 TẦNG CORE ENGINE                        │
│   AbilityEngine · CombatResolver · CardInstance         │
│   (Plain C# — không phụ thuộc Unity API)               │
├─────────────────────────────────────────────────────────┤
│                   TẦNG DATA                             │
│   CardDefinition · AbilityData · AI_Library.json        │
│   CardsData.json · CardsSpells.json                     │
│   (Dữ liệu thuần — serializable, không có logic)        │
└─────────────────────────────────────────────────────────┘
                          ↕ (song song, không phụ thuộc)
        ┌──────────────────────────────────────┐
        │            TẦNG AI                   │
        │  BotAgent · GameSimulator · GATrainer │
        │  (Plain C# — dùng Core Engine trực tiếp, │
        │   bỏ qua hoàn toàn Manager và UI)    │
        └──────────────────────────────────────┘
```

Vị trí của tầng AI là điểm then chốt: nó nằm ngoài tháp bốn tầng, kết nối trực tiếp xuống Core Engine mà không đi qua Manager hay UI — hiện thân kiến trúc của yêu cầu headless.

Ranh giới cứng nhất là phân chia **MonoBehaviour** và **Plain C#**: bất kỳ lớp nào có logic thuần (tính toán, quyết định game, xử lý dữ liệu) đều là plain C# và có thể khởi tạo tự do bằng `new ClassName()`. Chỉ lớp thực sự cần Unity lifecycle (`Awake`, `Start`, `Update`, input event) mới kế thừa `MonoBehaviour`. Ranh giới này là điều kiện vật lý để AI training tồn tại.

> **[HÌNH 4.1 — Kiến Trúc Phân Tầng]** *Sơ đồ 4 tầng xếp dọc (Data ở đáy → Core Engine → Manager → UI), AI System nằm ngoài sang phải kết nối thẳng xuống Core Engine. Mũi tên một chiều từ trên xuống. Màu xanh lá: plain C#; màu vàng: MonoBehaviour; màu xám: Data.*

---

## 4.3 Tầng Data

### 4.3.1 CardDefinition Và CardInstance

Tầng Data tách **định nghĩa** (những gì lá bài *là*) khỏi **trạng thái** (những gì lá bài *đang là* khi chơi). `CardDefinition` là template bất biến được đọc từ `CardsData.json` và tồn tại duy nhất trong `CardDatabase` — mọi bản sao "Marduk" trong game đọc từ cùng một template. `CardInstance` là thực thể runtime với chỉ số hiện tại, lịch sử buff, và mức merge riêng.

`CardDefinition` giữ ba passive keyword (Taunt, Reborn, Safeguard) là boolean trực tiếp thay vì đưa vào hệ thống TTE — vì chúng là trạng thái thụ động ảnh hưởng đến cơ chế engine (xác định mục tiêu, xử lý cái chết), không phải kỹ năng kích hoạt theo sự kiện. Phương thức `Clone()` tạo bản sao độc lập của `abilities` list khi tạo instance — tránh lỗi thiết kế khi spell "AddAbility" thêm kỹ năng cho một unit cụ thể làm ảnh hưởng toàn bộ unit cùng loại trong tương lai.

Chỉ số của `CardInstance` được quản lý theo bốn lớp bonus với vòng đời khác nhau:

```
currentATK = baseATK × tier
           + 0.7 × (permanentATKBonus + growthATKBonus)   ← tích lũy lâu dài
           + tempSpellATKBonus                             ← tồn tại 1 combat rồi xóa
           + globalPermATKBonus                            ← từ tribe buff toàn cục
```

Hệ số `0.7` (keepRatio) là quyết định balance có chủ ý: buff tích lũy nhiều lượt chỉ mang 70% giá trị vào chỉ số thực, tránh tình trạng phi thực tế khi một unit nhận liên tiếp nhiều buff nhỏ. `globalPermATKBonus` phục vụ kỹ năng như Thoth — buff vĩnh viễn *toàn bộ* unit cùng bộ tộc, kể cả unit chưa được mua; mỗi khi unit mới được tạo ra, `ApplyGlobalPermBuffToNewUnit()` áp tổng buff đã tích lũy.

> **[HÌNH 4.2 — Mối Quan Hệ CardDefinition / CardInstance]** *Một CardDefinition "Marduk" với ba mũi tên ra đến ba CardInstance khác nhau (board player, board bot, shop). Mỗi instance có chỉ số khác nhau. Dưới đó: công thức ResetStats() với 4 thành phần bonus.*

---

### 4.3.2 AbilityData

`AbilityData` mô tả một kỹ năng theo mô hình TTE: ba trường cốt lõi (`trigger`, `target`, `effect`) cộng thêm các modifier định hình cách kỹ năng hoạt động — `isPermanent`, `triggerLimit`, `conditionCount` (skip N lần kích hoạt), `isEscalating` (giá trị tăng dần), `subjectTribe` (lọc trigger chỉ phản ứng với bộ tộc cụ thể). Anubis — "khi một đồng minh chết, ban Reborn cho đồng minh HP thấp nhất; tối đa 2 lần, tăng theo merge level" — được biểu diễn đầy đủ bởi một `AbilityData` không cần một dòng C# riêng.

---

## 4.4 Tầng Core Engine

### 4.4.1 AbilityEngine

`AbilityEngine` diễn giải và thực thi `AbilityData`: nhận sự kiện game (unit tấn công, đồng minh chết, lượt kết thúc), tìm kỹ năng phù hợp, chọn mục tiêu, thực thi hiệu ứng. Hai hàm chính phản ánh hai mô hình kích hoạt khác nhau: `TriggerAbility()` cho kích hoạt tự thân (Marduk chết → chính Marduk trigger `OnDeath`), và `BroadcastAllyEvent()` cho phản ứng đồng đội (Marduk chết → tất cả đồng minh nhận broadcast `OnAllyDeath`, Horus có cơ hội phản ứng).

Trước khi thực thi kỹ năng, engine kiểm tra tuần tự: trigger có khớp không; `triggerLimit` chưa vượt; `conditionCount` thỏa; `subjectTribe` hợp lệ; và guard đặc biệt ngăn Sekhmet ăn unit trong shop phase. Engine chỉ tăng `abilityTriggerCounts` khi *thực sự có mục tiêu hợp lệ* — giới hạn kích hoạt phản ánh số lần ability có tác dụng, không phải số lần sự kiện xảy ra.

---

### 4.4.2 Ba Edge Case Phức Tạp

**Reborn chain:** Unit chết → hồi sinh → `OnAllyReborn` kích hoạt → Summoner triệu hồi unit → `OnAllySummon` của Sekhmet kích hoạt → Sekhmet nuốt unit → nếu Sekhmet chết → `SummonConsumed` triệu hồi lại → thêm `OnAllySummon`... Chuỗi này không thể xử lý "ngay khi xảy ra" — đòi hỏi Death Stack (xem 4.4.3).

**SummonConsumed (Sekhmet):** `isConsume = true` trong `AbilityData` khi effect là `Destroy` có nghĩa unit bị hủy không biến mất — cardID được lưu vào `consumedCardIDs`. Khi Sekhmet chết, effect `SummonConsumed` đọc danh sách này và triệu hồi lại. Guard `_isCombatPhase` ngăn Sekhmet ăn unit trong shop phase.

**Gilgamesh OnStatGain:** Kỹ năng phản ứng mỗi khi nhận permanent stat bằng cách tự buff thêm — buff đó lại là permanent stat gain, tạo đệ quy vô hạn. Guard `_firingOnStatGain` ngăn `OnStatGain` kích hoạt lại chính nó trong cùng một chuỗi.

---

### 4.4.3 Death Stack

`CombatResolver` là engine chiến đấu hoàn toàn plain C# — không MonoBehaviour, không Unity API — cho phép `GameSimulator` gọi hàng nghìn lần/phút trong training.

Vấn đề cốt lõi: khi một unit chết trong lúc đang xử lý turn của unit khác, xử lý ngay lập tức dẫn đến modify collection trong khi đang iterate và missed events trong chain death. Giải pháp là **Death Stack** — cấu trúc dữ liệu thu gom mọi "cái chết đang chờ xử lý". Khi unit có `currentHP ≤ 0`, nó được đưa vào stack (với `onDeathProcessed = true` để tránh đưa vào hai lần) thay vì xử lý ngay. Sau mỗi đòn đánh, `FlushDeathStack()` giải quyết tất cả:

```
FlushDeathStack():
  while stack không rỗng:
    pop (victim, killer, victimBoard, killerBoard)

    if victim.isReborn và chưa dùng Reborn:
        victim.ReviveDefault()     → hồi sinh 1 HP
        BroadcastAllyEvent(OnAllyReborn, victim)
        ProcessNextPendingSummon()
    else:
        xóa victim khỏi board
        TriggerAbility(OnDeath, victim, killer)
        BroadcastAllyEvent(OnAllyDeath, victim)
        ProcessNextPendingSummon()
```

Chọn Stack (LIFO) thay vì Queue (FIFO) là chủ ý: khi chain death tạo ra thêm death events, cái chết mới nhất cần được resolve hoàn toàn trước khi quay lại chain trước — thứ tự xử lý nhất quán và có thể dự đoán.

> **[HÌNH 4.3 — Death Stack Flow]** *Sơ đồ luồng: "unit HP≤0" → "đưa vào deathStack" → vòng lặp "stack không rỗng?" → nhánh "isReborn?" (có: Revive → OnAllyReborn → ProcessSummon; không: xóa board → OnDeath → OnAllyDeath → ProcessSummon). Màu đỏ cho xóa thật, xanh lam cho Reborn.*

---

### 4.4.4 Thứ Tự Tấn Công Và Pre-Combat Snapshot

**Thứ tự tấn công động:** Thứ tự được rebuild theo ATK ở đầu *mỗi sub-turn* (không phải một lần cho cả trận) để unit mới được triệu hồi giữa chừng tự động có vị trí đúng trong queue tấn công. Chi phí O(n log n) mỗi sub-turn đổi lại đảm bảo đúng đắn trong mọi trường hợp.

**Pre-combat snapshot:** Trước mỗi combat, `GameManager` lưu snapshot board người chơi (slot index, unit, và `consumedCardIDs` tại thời điểm đó). Sau combat, board được restore theo snapshot — unit chết và unit token tạm thời (`isBattleSpawned`) bị xóa, buff tạm thời bị reset, buff vĩnh viễn được giữ. `consumedCardIDs` của Sekhmet cũng được restore để ngăn Sekhmet "nhớ" đã nuốt unit tạm thời không còn tồn tại.

---

## 4.5 Tầng Manager

### 4.5.1 GameManager — Partial Class

`GameManager` được chia thành bốn file `partial class` với trách nhiệm không chồng chéo:

- `GameManager.cs` — trạng thái cốt lõi: HP người chơi, turn counter, bot đối thủ, vòng lặp `ExecuteNextTurn()`
- `GameManager.Shop.cs` — logic shop: refresh, tier, reroll, freeze, điền slot trống
- `GameManager.Combat.cs` — khởi động/kết thúc combat: snapshot, gọi `CombatResolver`, restore board, visualize `TurnRecord`
- `GameManager.Board.cs` — board UI: tạo card prefab, xử lý kéo-thả, merge hints

---

### 4.5.2 CardDatabase — Drop Rate Theo Shop Tier

`CardDatabase` đọc toàn bộ định nghĩa card từ JSON và cung cấp shop ngẫu nhiên theo tier. Phân phối drop rate tạo cảm giác progression — người chơi xây nền tảng bằng unit rẻ sớm game, dần nâng cấp khi shop chất lượng hơn:

| Shop Tier | Tier 1 (pool 1–2) | Tier 2 (pool 3–4) | Tier 3 (pool 5–6) |
|:---------:|:-----------------:|:-----------------:|:-----------------:|
| 1 | 100% | — | — |
| 2 | 70% | 30% | — |
| 3 | 40% | 45% | 15% |
| 4 | 25% | 40% | 35% |
| 5 | 15% | 30% | 55% |
| 6 | 10% | 20% | 70% |

Card có `isToken = true` bị lọc khỏi pool shop — chỉ xuất hiện qua ability `Summon`.

---

### 4.5.3 EconomyManager

Mỗi lượt người chơi (và bot) nhận đúng **10 coin cố định** cộng bonus từ spell. Không có interest, không có streak bonus. Quyết định này có chủ ý: với AI học qua 37 gene, economy phức tạp thêm quá nhiều biến số vào fitness landscape làm chậm convergence — GA cần tập trung học "mua gì, khi nào reroll, khi nào bán", không phải tối ưu đường cong interest.

---

## 4.6 Tầng UI

### 4.6.1 Chuỗi CardUI → CardDraggable → CardSlot

Mỗi lá bài có ba Component phối hợp theo chuỗi trách nhiệm rõ ràng: **CardUI** chỉ render (đọc `CardInstance`, hiển thị chỉ số — không biết drag hay slot hợp lệ). **CardDraggable** chỉ xử lý vật lý kéo-thả (detach card, theo dõi con trỏ, thông báo slot đích — không quyết định điều gì xảy ra với card). **CardSlot** xử lý drop event và delegate toàn bộ game logic về `GameManager` — mua card nếu từ shop sang board, swap nếu board sang board, trả về tay nếu board sang hand. Không có game logic nào nằm trong tầng UI.

---

### 4.6.2 TurnRecord — Cầu Nối Headless Và Visual

`CombatResolver.ResolveTurn()` là headless và tức thì — toàn bộ combat hoàn thành trong milliseconds không cần render. Để người chơi xem diễn biến, `CombatResolver` nhận một `TurnRecord log` parameter và ghi lại mọi sự kiện (đòn đánh, cái chết, buff, summon) theo đúng thứ tự. UI layer đọc danh sách này và animate từng action với delay nhỏ. Trong AI training, `GameSimulator` truyền `null` thay vì `TurnRecord` — cùng `CombatResolver` phục vụ cả hai mục đích mà không cần branch code.

---

## 4.7 Headless Compatibility

### 4.7.1 Ranh Giới Vật Lý

`GameSimulator` và tầng AI hoàn toàn là plain C#: `CombatResolver`, `AbilityEngine`, `EconomyManager` không có một import `UnityEngine` nào ngoài `Mathf.Clamp` (có thể thay bằng `System.Math.Clamp`). `CardDatabase.Instance` là singleton có thể populate trong Editor script không cần scene.

---

### 4.7.2 Nullable GameManager

Khi `AbilityEngine` cần cấp coin (`GainCoin` effect), trong game thực nó gọi `GameManager.Instance.AddCoin()`. Trong training headless, `GameManager` không tồn tại. Giải pháp là null-conditional operator:

```csharp
GameManager.Instance?.AddCoin(ability.effectValue1);
```

Toán tử `?.` biến lời gọi thành no-op khi `Instance` là null — không exception, không branch code. Economy của bot trong training được `BotAgent.economy` (một `EconomyManager` độc lập) quản lý hoàn toàn trong sandbox, không qua global state.

---

### 4.7.3 Pipeline Training Khép Kín

Toàn bộ quy trình training chạy qua một PowerShell script: Unity được gọi với `-batchmode -nographics`, thực thi `AITrainingBatch.RunTraining()` — khởi tạo `GATrainer`, chạy vòng lặp tiến hóa đồng bộ, lưu kết quả vào `AI_Library.json`. Khi game khởi động lần sau, `AIManager` đọc file và nạp 5 chromosome vào 5 `BotAgent`.

Vòng lặp khép kín: `train_ai.ps1` → `GATrainer` → `GameSimulator` × 432.000 trận → `AI_Library.json` → `AIManager` → `BotAgent` trong game. Mỗi bước độc lập và có thể chạy lại từ bất kỳ điểm nào.

> **[HÌNH 4.4 — Pipeline Headless Training]** *Sơ đồ ngang 6 bước: train_ai.ps1 → Unity -batchmode → AITrainingBatch → [GATrainer → GameSimulator × 432.000 trận] → AI_Library.json → AIManager nạp vào game. Phần headless bao trong hộp xám; phần game runtime tô xanh nhạt.*

---

## 4.8 Tổng Hợp

Mọi quyết định kiến trúc quan trọng đều xuất phát từ ba ràng buộc ở mục 4.1:

**Data-driven** → `CardDefinition`/`AbilityData` là pure data class, `CardDatabase` đọc JSON, toàn bộ content thay đổi không cần recompile.

**Headless-compatible** → ranh giới cứng MonoBehaviour/Plain C#, nullable `GameManager.Instance?.` thay vì direct call, `TurnRecord` như cầu nối giữa headless engine và visual layer.

**Phụ thuộc một chiều** → `AbilityEngine` không import `GameManager`, `GameSimulator` không biết `UIManager` tồn tại, mọi game logic trong `CardSlot.OnDrop()` đều delegate về `GameManager`.

Kết quả: cùng một codebase chạy 432.000 trận đấu mô phỏng trong 20–30 phút và cung cấp trải nghiệm chơi game hoàn chỉnh với UI tương tác — không cần điều chỉnh runtime nào giữa hai chế độ.

---

*[Kết thúc Chương 4 — Tiếp theo: Chương 5 — Thuật Toán Di Truyền Và Hệ Thống AI]*

\newpage

# CHƯƠNG 5: THUẬT TOÁN DI TRUYỀN VÀ HỆ THỐNG AI

---

## 5.1 Tổng Quan Hệ Thống AI

### 5.1.1 Mục Tiêu Thiết Kế

Ba yêu cầu được xác định ngay từ đầu khi thiết kế hệ thống AI:

- **Không có quy tắc cứng:** Mọi hành vi của bot — đánh giá card, quyết định reroll, ưu tiên bộ tộc, bán unit — phải phát sinh từ quá trình học, không phải từ if-else viết tay. Nếu một hành vi không truy xuất được về gene cụ thể, nó không tồn tại trong hệ thống.
- **Nhiều phong cách chơi:** Người chơi cần đối mặt với đối thủ chơi *khác nhau* — bot tích lũy Babylon, bot dựa vào chain hồi sinh Niles, bot phòng thủ thuần túy. Hệ thống training phải tự nhiên tạo ra diversity, không hội tụ về một chiến lược duy nhất.
- **Khả thi trên phần cứng cá nhân:** Training hoàn thành trong vài chục phút trên máy thông thường, không cần GPU hay training tính bằng giờ.

Ba yêu cầu này cùng trỏ về một kiến trúc: Genetic Algorithm với chromosome real-valued, đánh giá qua headless simulation, và island model để duy trì diversity.

---

### 5.1.2 Kiến Trúc Tổng Thể — Bốn Thành Phần

```
┌─────────────────────────────────────────────────────────┐
│                    KIẾN TRÚC HỆ THỐNG AI                │
│                                                         │
│  ┌─────────────┐   "bộ não"    ┌──────────────────┐    │
│  │ Chromosome  │ ────────────► │    BotAgent       │    │
│  │  37 genes   │               │ DecidePrepPhase() │    │
│  └─────────────┘               └────────┬─────────┘    │
│                                          │ board state  │
│  ┌──────────────────────────────────────▼──────────┐   │
│  │               GameSimulator                      │   │
│  │   EvaluateMatch(botA, botB) → MatchResult       │   │
│  │   20 lượt × [DecidePrepPhase + ResolveTurn]     │   │
│  └───────────────────────┬──────────────────────────┘  │
│                           │ fitness scores              │
│  ┌────────────────────────▼──────────────────────────┐ │
│  │                   GATrainer                        │ │
│  │   Init → Evaluate → Select → Crossover → Mutate   │ │
│  │   → 5 specialist bots → AI_Library.json           │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

**Chromosome** — mảng 37 số thực [0,1], biểu diễn chiến lược chơi, không chứa logic.  
**BotAgent** — engine thực thi: nhận chromosome làm "bộ não", dùng gene để quyết định trong mỗi lượt shop.  
**GameSimulator** — môi trường đánh giá: chạy trận đấu 20 lượt giữa hai BotAgent, trả về điểm fitness.  
**GATrainer** — vòng lặp tiến hóa: điều phối khởi tạo, đánh giá, chọn lọc, lai ghép, đột biến qua nhiều thế hệ.

> **[HÌNH 5.1 — Kiến Trúc Tổng Thể Hệ Thống AI]** *Sơ đồ bốn thành phần với mũi tên luồng dữ liệu: Chromosome → BotAgent → GameSimulator → GATrainer → AI_Library.json. Mỗi thành phần một màu, kèm class name C# tương ứng.*

---

### 5.1.3 Kết Quả — Năm Bot Chuyên Biệt

| Bot | Đặc trưng gene | Phong cách chơi |
|-----|---------------|-----------------|
| **hardBot** | Fitness tổng cao nhất (Hall of Fame) | Generalist — cân bằng, không có điểm yếu rõ ràng |
| **babylonBot** | `genes[18]` (sBabylon) dominant | Tích lũy buff qua deploy/sell, snowball dài hạn |
| **nileBot** | `genes[20]` (sNiles) dominant | Chuỗi chết-hồi sinh, tích lũy OnAllyDeath |
| **summonerBot** | `genes[14]` (eSummon), `[5]` (wReborn), `[8]` (tOnDeath) cao | Summon/consume chain, giữ shell units |
| **resilientBot** | `genes[1]` (wHP), `[4]` (wTaunt), `[5]` (wReborn), `[6]` (wSafeguard) cao | Phòng thủ bền bỉ, hút damage, phản đòn |

Năm bot vừa tạo đa dạng trải nghiệm gameplay, vừa là bằng chứng GA với island model khám phá được nhiều vùng khác nhau trong không gian gene thay vì hội tụ về một điểm duy nhất.

---

## 5.2 Thiết Kế Chromosome — 37 Gene

Mỗi gene phải ánh xạ chính xác đến một **chiều quyết định** cụ thể trong `DecidePrepPhase`, đảm bảo không gian gene và không gian chiến lược tương đương nhau. Bảng dưới tổng hợp 37 gene theo 9 nhóm chức năng:

| Nhóm | Gene | Tên | Phạm vi | Chức năng |
|:----:|:----:|-----|:-------:|-----------|
| **1** | 0 | wATK | [0,1] | Trọng số `baseATK` khi chấm điểm card |
| | 1 | wHP | [0,1] | Trọng số `baseHP` |
| | 2 | wTierBonus | [0,1] | Bonus tier: `(tier−1) × gene × 5` |
| | 3 | wCostEff | [0,1] | Hệ số hiệu quả chi phí: `÷ cost × (1+gene)` |
| **2** | 4 | wTaunt | [0,1] | Giá trị keyword Taunt: `+ gene × 10` |
| | 5 | wReborn | [0,1] | Giá trị keyword Reborn: `+ gene × 12` |
| | 6 | wSafeguard | [0,1] | Giá trị keyword Safeguard: `+ gene × 8` |
| **3** | 7 | tStartBattle | [0,1] | TriggerWeight cho `StartOfBattle` |
| | 8 | tOnDeath | [0,1] | TriggerWeight cho `OnDeath` |
| | 9 | tOnAttack | [0,1] | TriggerWeight cho `OnAttack` |
| | 10 | tOnTakeDmg | [0,1] | TriggerWeight cho `OnTakeDamage` |
| | 11 | tEndTurnShop | [0,1] | TriggerWeight cho `EndTurnShop` |
| | 12 | tOnDeploy | [0,1] | TriggerWeight cho `OnDeploy` |
| **4** | 13 | eAddStats | [0,1] | EffectWeight cho `AddStats`, `AbsorbStats`, `ScaleTargetStats` |
| | 14 | eSummon | [0,1] | EffectWeight cho `Summon`, `SummonConsumed` |
| | 15 | eDealDmg | [0,1] | EffectWeight cho `DealDamage`, `Destroy` |
| | 16 | eGainCoin | [0,1] | EffectWeight cho `GainCoin` và spell kinh tế |
| | 17 | eGiveBuff | [0,1] | EffectWeight cho `GiveBuff`, `Reborn` effect |
| **5** | 18 | sBabylon | [0,1] | Bonus mỗi đồng minh Babylon trên sân |
| | 19 | sOlympus | [0,1] | Bonus mỗi đồng minh Olympus trên sân |
| | 20 | sNiles | [0,1] | Bonus mỗi đồng minh Niles trên sân |
| **6** | 21 | wMerge | [0,1] | Bonus per bản sao cùng cardID: `copies × gene × (copies==2 ? 16 : 8)` |
| | 22 | wFrontline | [0,1] | Bonus mua Taunt khi frontline trống: `emptyFront × gene × 2` |
| | 23 | wSaveThreshold | [0,1] | Ngưỡng tối thiểu để mua: bỏ qua card nếu `score < gene × 3` |
| **7** | 24 | wRerollThresh | [0,1] | Reroll nếu `bestShop < gene × bestBoard` |
| | 25 | wRerollMax | [0,1] | Số lần reroll tối đa: `floor(gene×3)+1` → [1..4 lần] |
| | 26 | wRerollKeep | [0,1] | Coin dự phòng trước khi reroll: `floor(gene×4)` → [0..4] |
| | 27 | wProactiveSell | [0,1] | Bán unit có `score < gene × 3` dù board chưa đầy |
| **8** | 28 | wSpellThresh | [0,1] | Ngưỡng mua spell: `spellScore/cost > gene × 3` |
| | 29 | wSpellOnStrong | [0,1] | Ưu tiên cast spell lên unit `EvaluateInstance` cao nhất |
| | 30 | wSpellOnMerged | [0,1] | Ưu tiên cast spell lên unit đã merge nhiều nhất |
| | 31 | wSpellEconomy | [0,1] | Nhân thêm cho GainCoin / GainIncome trong `EvaluateSpell` |
| **9** | 32 | tAura | [0,1] | TriggerWeight cho `Aura` |
| | 33 | tOnSell | [0,1] | TriggerWeight cho `OnSell` |
| | 34 | tOnAllyGroup | [0,1] | TriggerWeight cho `OnAllyDeath`, `OnAllySummon`, `OnAllyReborn` |
| | 35 | tOnAllyDeploy | [0,1] | TriggerWeight cho `OnAllyDeploy` |
| | 36 | tOnAllySell | [0,1] | TriggerWeight cho `OnAllySell` |

> **[HÌNH 5.2 — Biểu Đồ 37 Gene Phân Nhóm Màu Sắc]** *Hình thanh ngang 37 ô, mỗi nhóm một màu nền. Mỗi ô ghi chỉ số gene và tên viết tắt.*

---

### 5.2.1 Nguyên Tắc Thiết Kế Gene

**Tính tổ hợp giữa nhóm 3 và nhóm 4.** Điểm ability của mỗi card được tính qua tích `TriggerWeight(trigger) × EffectWeight(effect) × 10`. Nhân trực tiếp hai gene từ hai nhóm khác nhau tạo ra đặc tính cộng hưởng: bot có cả `genes[8]` (tOnDeath) lẫn `genes[14]` (eSummon) cao sẽ đặc biệt đánh giá cao card dạng *khi chết triệu hồi* — chính là archetype summonerBot. Không cần gene riêng cho từng tổ hợp trigger-effect; `6 × 5 = 30` kết hợp xuất hiện tự nhiên từ 11 gene.

**Context-dependency trong nhóm 5.** Ba gene tribe (18–20) không đánh giá card trong chân không mà đánh giá *trong ngữ cảnh board*: mỗi đồng minh cùng bộ tộc cộng thêm `4 × genes[tribe]` điểm cho card đang xét. Với `genes[18] = 0.9` và 3 unit Babylon trên sân, card Babylon tiếp theo nhận thêm 10.8 điểm — đủ để vượt qua card non-Babylon có chỉ số tốt hơn. Cơ chế này đảm bảo bot tự nhiên theo đuổi chiến lược bộ tộc, không cần rule cứng.

**Tính nhất quán chiến lược qua các nhóm.** Chromosome không thể chứa mâu thuẫn nội tại giữa các gene: bot aggressive reroll (`genes[24]` cao) bị gene[24] ngầm ngăn freeze (`FreezePhase` dùng `1 − genes[24]`), nên không thể vừa reroll mạnh vừa freeze shop. summonerBot cần `genes[27]` (proactive sell) thấp để giữ shell units — và seed khởi tạo phản ánh đúng điều đó.

**Nhóm 9 — bài học thiết kế lặp.** Thiết kế ban đầu chỉ có 32 gene: các trigger như Aura, OnSell, OnAllyDeath dùng chung gene với trigger cha kèm hệ số cứng (`Aura = genes[7] × 0.6`). Vấn đề: GA chỉ học được trọng số trigger cha, không học được giá trị *thực tế* của trigger con. Nếu Aura mạnh hơn StartOfBattle trong thực tế game, hệ số cứng 0.6 luôn đánh giá thấp nó — GA không thể sửa sai này. Giải pháp ở lần mở rộng thứ hai: tách 5 trigger con thành gene độc lập (genes[32–36]), cho GA hoàn toàn tự do xác định giá trị. Kết quả sau training xác nhận: summonerBot hội tụ về `genes[34]` (tOnAllyGroup) rất cao — phản ánh đúng rằng OnAllyDeath/Summon/Reborn chain quan trọng hơn OnDeploy nhiều với archetype này.

---

### 5.2.2 Tính Đầy Đủ Của Không Gian Gene

Mỗi quyết định quan trọng trong `DecidePrepPhase` đều có ít nhất một gene chi phối:

| Quyết định | Gene chi phối |
|-----------|---------------|
| Đánh giá chỉ số unit | [0] wATK, [1] wHP, [2] wTierBonus |
| Cân nhắc giá/giá trị | [3] wCostEff |
| Mua unit có keyword | [4] wTaunt, [5] wReborn, [6] wSafeguard |
| Đánh giá ability (trigger × effect) | [7–12] Trigger + [13–17] Effect + [32–36] Trigger con |
| Ưu tiên bộ tộc (context-aware) | [18] sBabylon, [19] sOlympus, [20] sNiles |
| Theo đuổi merge | [21] wMerge |
| Quản lý frontline | [22] wFrontline |
| Ngưỡng mua tối thiểu | [23] wSaveThreshold |
| Quyết định reroll | [24] wRerollThresh, [25] wRerollMax, [26] wRerollKeep |
| Bán unit kém | [27] wProactiveSell |
| Mua và target spell | [28] wSpellThresh, [29] wSpellOnStrong, [30] wSpellOnMerged, [31] wSpellEconomy |
| Vị trí đặt unit | [8] tOnDeath→frontline, [17] eGiveBuff→backline, [22] Taunt→frontline |

Không có quyết định chiến lược quan trọng nào bị bỏ sót. Đây là điều kiện cần để GA tiến hóa ra các phong cách chơi đa dạng: vì không gian gene đủ rộng để biểu diễn nhiều chiến lược khác nhau, quá trình chọn lọc không bị ép vào một hướng cứng nào.

> **[HÌNH 5.3 — Bảng 37 Gene Với Giá Trị 5 Bot]** *Bảng 37 hàng phân nhóm màu sắc (9 nhóm). Phía dưới: biểu đồ cột thể hiện giá trị gene của hardBot, babylonBot, nileBot, summonerBot, resilientBot (5 đường màu).*

---

*[Tiếp theo: Mục 5.3 — BotAgent: Bộ Não Quyết Định]*

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

### 5.4.3 Hàm Fitness

```
ScoreFromA(result, hpA, hpB, turns):
    base  = 120 nếu thắng | 70 nếu hòa | 35 nếu thua
    score = base + hpA×6 − hpB×3
    nếu thắng: score += (20 − turns) × 2
    return max(1, score)
```

Ba thành phần encode ba khía cạnh của "chơi tốt": kết quả nhị phân (120/70/35), biên thắng (`hpA×6 − hpB×3` với hệ số không cân xứng khuyến khích tự bảo tồn), và tốc độ thắng (thưởng chiến lược tempo). Phạm vi thực tế: 14 (thua nặng) → 200 (thắng sớm, HP cao) — đủ rộng để phân biệt chiến lược.

> **[HÌNH 5.5 — Hàm Fitness ScoreFromA]** *Biểu đồ thanh ba kịch bản: Thắng sớm (HP cao, ít lượt), thắng muộn (HP vừa, nhiều lượt), thua sát (HP gần 0, địch HP gần 0). Mỗi thanh chia 3 phần màu: kết quả nhị phân, biên thắng, tốc độ.*

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

\newpage

# CHƯƠNG 6: KẾT QUẢ VÀ ĐÁNH GIÁ

## 6.1 Nhìn Lại Mục Tiêu Và Cách Đo Thành Công

Trước khi đánh giá kết quả, cần nhắc lại ba mục tiêu ban đầu của dự án: xây dựng một game Auto Chess hoàn chỉnh và có thể chơi được; thiết kế một hệ thống AI bot học qua Genetic Algorithm mà không dùng rule-based hay Neural Network; và huấn luyện ra nhiều phong cách chơi khác nhau để tạo trải nghiệm đa dạng cho người chơi.

Ba mục tiêu này không phải cùng loại — mục tiêu đầu tiên có thể đánh giá bằng chức năng (game chạy được, mechanic hoạt động đúng), mục tiêu thứ hai bằng kết quả thực nghiệm (bot có học được chiến lược tốt hơn ngẫu nhiên không), và mục tiêu thứ ba bằng tính chất định tính (các bot có thực sự chơi khác nhau không). Chương này đi qua từng chiều đánh giá, dùng dữ liệu cụ thể nơi có thể, và thẳng thắn về những điểm còn hạn chế.

---

## 6.2 Demo Hệ Thống — Game Trong Trạng Thái Hoạt Động

### 6.2.1 Luồng Chơi Từ Đầu Đến Cuối

Người chơi bắt đầu từ màn hình chọn độ khó — một lựa chọn đơn giản giữa Easy, Medium và Hard, mỗi mức tương ứng với cách AIManager phân bổ bot đối thủ theo vòng lặp. Sau khi chọn, game bắt đầu ngay ở lượt đầu tiên với 10 coin và một shop tier 1.

> **[HÌNH 6.1 — Màn Hình Shop Phase (Mid-Game)]** *Screenshot gameplay: 5 slot shop phía dưới (4 unit + 1 spell), board 7 slot ở giữa với các unit của người chơi đang được sắp xếp, HUD trên cùng hiển thị HP=5, Coin=7, Turn=8, Cups=3. Sidebar bên phải hiển thị thông tin unit đang được chọn (CardDetailPanel).*

Shop phase mỗi lượt cho người chơi tất cả thời gian cần thiết để: đọc shop, kéo thả unit lên bàn, bán unit yếu, cast spell, reroll nếu muốn, và freeze shop cho lượt sau. Đây là phase player-driven hoàn toàn — không có time limit, không có áp lực thời gian thực. Khi người chơi sẵn sàng, bấm nút "Chiến đấu" để chuyển sang combat phase.

Combat diễn ra tự động: hai board đối đầu nhau, unit tấn công theo thứ tự ATK, kỹ năng kích hoạt theo trigger tương ứng. Kết quả sau mỗi lượt combat được tổng kết — bên thắng không mất HP, bên thua mất 1 HP. Sau 20 lượt, bên còn HP thắng; bên mất hết HP trước thua ngay lập tức.

> **[HÌNH 6.2 — Combat Phase]** *Screenshot: board hai bên đang trong trạng thái chiến đấu, hiển thị các unit với HP bar, các indicator Taunt/Reborn/Safeguard. Board bot bên phải. Một unit đang trong animation nhận damage (flash đỏ). Lượt số 3/20 (shop turn counter) đang diễn ra combat.*

---

## 6.3 Đánh Giá Hệ Thống Game

### 6.3.1 Tính Hoàn Chỉnh Của Mechanic

Tiêu chí đánh giá đầu tiên và quan trọng nhất là: các mechanic đã thiết kế có hoạt động đúng như ý định không? Đây không phải câu hỏi mơ hồ — mỗi mechanic có một kết quả mong đợi cụ thể có thể kiểm tra được.

**Hệ thống TTE với 14 trigger và 13 effect** được xác nhận qua quá trình test từng lá bài trong bộ 68 card. Ba nhóm trigger đáng chú ý nhất về mặt kỹ thuật — OnDeath chain, OnAllyDeath broadcast, và EndTurnShop accumulation — đều hoạt động đúng thứ tự xử lý như thiết kế. Đặc biệt, Death Stack đảm bảo rằng khi nhiều unit chết cùng một turn, thứ tự kích hoạt OnDeath là nhất quán và không phụ thuộc vào thứ tự slot trên bàn.

Hai mechanic phức tạp nhất đã được xác nhận hoạt động đúng:

*Reborn chain*: Unit có Reborn chết → hồi sinh với 1 HP → broadcast OnAllyReborn tới đồng minh → nếu có Summoner phản ứng với OnAllyReborn bằng cách triệu hồi, summon mới được xử lý ngay trong death flush, không bị mất. Unit đã dùng Reborn (`hasRebornUsed = true`) không hồi sinh lần thứ hai — mechanic đúng với thiết kế.

*SummonConsumed (Sekhmet mechanic)*: Sekhmet nuốt unit đồng minh được triệu hồi trong combat → lưu cardID → khi Sekhmet chết, triệu hồi lại tất cả unit đã bị nuốt. Guard `_isCombatPhase` ngăn Sekhmet ăn unit trong shop phase. Restore snapshot sau combat đảm bảo `consumedCardIDs` không bị reset không đúng lúc.

**Hệ thống Merge** hoạt động đúng với quy tắc bất đối xứng: 3 bản sao lv0 thành lv1, chỉ cần 2 bản sao lv1 thành lv2. Hint system (highlight khi có ≥2 bản sao trên board/hand) hoạt động trong cả hai chiều — board và hand. Khi merge, unit giữ lại là unit có tổng bonus vĩnh viễn cao nhất, đảm bảo không lãng phí buff đã tích lũy.

**Tribe Synergy** — Babylon buff HP vĩnh viễn qua trigger deploy/sell của unit cùng tộc; Niles buff HP và ATK qua chain summon/reborn/death; Olympus buff ATK qua combat events — đều hoạt động và quan trọng hơn, buff *future units* mua sau khi synergy kích hoạt. Cơ chế `globalTribeBuff` và `ApplyGlobalPermBuffToNewUnit()` đảm bảo không có unit Babylon nào thoát khỏi buff dù được mua khi nào trong game.

**Shop tier progression và drop rate** tuân đúng bảng thiết kế. Ở lượt 1 chỉ thấy tier 1, từ lượt 5 trở đi bắt đầu thấy tier 3, và về cuối game tier 3 chiếm đa số shop — cảm giác progression mà người thiết kế muốn.

---

### 6.3.2 Hiệu Năng Kỹ Thuật

Hai chỉ số hiệu năng quan trọng nhất là tốc độ combat simulation và tốc độ training.

**Combat simulation** trong `GameSimulator` — một trận đấu hoàn chỉnh 20 lượt bao gồm cả shop phase decision và combat resolution — chạy trong khoảng vài milliseconds trên máy tính phát triển (Windows 10, CPU 8 nhân, không GPU). Đây là kết quả trực tiếp của việc thiết kế headless: không có render, không có allocation Unity object, không có coroutine overhead. Với tốc độ này, 432.000 trận đấu của production training hoàn thành trong 20–30 phút — khả thi hoàn toàn trên phần cứng cá nhân.

**Training convergence** xảy ra nhanh hơn dự kiến. Best fitness đạt đỉnh ở thế hệ thứ 6, nhưng average fitness và composite score của specialist bots tiếp tục cải thiện suốt 179 thế hệ — điều này có nghĩa là phần lớn thời gian training đang "nâng chất lượng trung bình" thay vì tìm nghiệm tốt hơn cho hardBot. Hành vi này là mong đợi và hợp lý: một khi không gian gene đã tìm được chromosome xuất sắc, GA chuyển sang vai trò nâng chất toàn quần thể — đảm bảo babylonBot và nileBot cũng là những chromosome mạnh, không chỉ là những chromosome "khác biệt gene nhưng chơi kém".

---

## 6.4 Đánh Giá Hệ Thống AI

### 6.4.1 Kết Quả Training — Dữ Liệu Từ 179 Thế Hệ

Lần training được ghi nhận trong báo cáo này sử dụng production mode với 120 cá thể, tối đa 180 thế hệ, và 20 trận đấu mỗi chromosome mỗi thế hệ. Dữ liệu được đọc từ file `training_20260601_213435.csv` — log thực tế từ lần chạy training trên máy phát triển.

Toàn bộ quá trình 179 thế hệ chia thành ba giai đoạn rõ nét, mỗi giai đoạn phản ánh một pha khác nhau của quá trình tiến hóa:

**Giai đoạn 1 — Hội tụ nhanh (Thế hệ 0–10):** Best fitness đạt đỉnh 4.764 điểm ngay ở thế hệ thứ 6. Trong 10 thế hệ đầu, độ lệch chuẩn giảm từ 952 xuống 663 (giảm 30%), phản ánh quần thể đang nhanh chóng loại bỏ những chromosome yếu. Điều đáng chú ý là chromosome tốt nhất được tìm thấy rất sớm — với quần thể 120 cá thể và khởi tạo seeded có chủ đích, GA không cần nhiều thế hệ để tìm được vùng gene tốt.

| Thế hệ | Best | Avg | Std Dev | Babylon% | Niles% | Other% |
|:------:|:----:|:---:|:-------:|:--------:|:------:|:------:|
| 0 | 4.727 | 2.871 | 952 | 42,5% | 40,8% | 16,7% |
| 6 | **4.764** | 3.033 | 717 | 34,2% | 32,5% | 33,3% |
| 10 | 4.764 | 3.084 | 663 | 35,0% | 36,7% | 28,3% |

**Giai đoạn 2 — Niles Domination (Thế hệ 33–65):** Một hiện tượng đáng phân tích xảy ra: chromosome thuộc nhóm Niles bắt đầu chiếm ưu thế ngày càng mạnh. Đỉnh điểm ở thế hệ 45, Niles chiếm tới 70,0% quần thể — Babylon chỉ còn 26,7% và Other gần như biến mất (3,3%). Đây không phải do immigration thất bại, mà do chromosome Niles *thực sự đang thắng nhiều hơn* trong đấu trường quần thể. Điều này tiết lộ một insight về game balance mà chỉ training AI mới làm lộ ra: cơ chế Reborn + OnAllyDeath chain của tribe Niles tạo ra sức mạnh combat tự nhiên cao hơn, đặc biệt trong những trận đấu kéo dài nhiều lượt.

| Thế hệ | Niles% | Babylon% | Other% | Ghi chú |
|:------:|:------:|:--------:|:------:|---------|
| 33 | 46,7% | 42,5% | 10,8% | Niles bắt đầu vượt |
| 40 | 66,7% | 28,3% | 5,0% | Accelerated growth |
| 45 | **70,0%** | 26,7% | 3,3% | Đỉnh Niles domination |
| 65 | 48,3% | 40,0% | 11,7% | Diversity phục hồi |

**Giai đoạn 3 — Ổn định và Specialist Refinement (Thế hệ 66–179):** Best fitness giữ nguyên 4.764, nhưng composite score của summonerBot và resilientBot tiếp tục cải thiện trong suốt giai đoạn này. Average fitness dao động quanh 2.970–3.100, std_dev ổn định trong khoảng 480–650 — không về 0, là dấu hiệu Island Model và immigration đã thành công giữ diversity mà không để premature convergence.

> **[HÌNH 6.3 — Đường Cong Fitness Qua 179 Thế Hệ]** *Biểu đồ đường: trục hoành thế hệ 0–179, trục tung fitness 0–5000. Ba đường: Best (đỏ — plateau tại 4764 từ gen 6), Avg (xanh lam — tăng dần rồi dao động quanh 3000), Std Dev (vàng nhạt — trục phụ bên phải, giảm từ 952 xuống ~540). Đánh dấu vùng "Niles Domination" (gen 33–65) bằng vùng tô xám nhạt.*

---

### 6.4.2 Năm Bot Chuyên Biệt — Từ Số Liệu Đến Hành Vi

Kết thúc training, 5 bot được chọn với kết quả định lượng như sau:

| Bot | Fitness | Điểm đặc trưng | Cơ sở chọn |
|-----|:-------:|----------------|------------|
| **hardBot** | 4.764 | Cao nhất tuyệt đối | Hall of Fame (fitness cao nhất *bao giờ*) |
| **babylonBot** | 4.764 | genes[18] Babylon cao nhất | Best Babylon, cách xa hardBot ≥ 0,18 |
| **nileBot** | 4.727 | genes[20] Niles cao nhất | Best Niles, cách xa 2 bot trước |
| **summonerBot** | — | SummonerScore = **8,56** | Viable (≥80% avg), score cao nhất |
| **resilientBot** | — | ResilientScore = **6,13** | Viable (≥80% avg), score cao nhất |

Điều đáng chú ý nhất trong bảng trên: babylonBot có cùng fitness tuyệt đối với hardBot (4.764). Cả hai đều là chromosome xuất sắc theo thước đo fitness thô — chúng được phân biệt không phải bởi chất lượng mà bởi profile gene khác nhau. Đây là mục tiêu thiết kế được thực hiện: không phải tìm bot tốt nhất duy nhất, mà tìm nhiều bot *cùng đẳng cấp nhưng chơi khác nhau*.

nileBot có fitness thấp hơn nhẹ (4.727 so với 4.764), nhưng con số này không có nghĩa là nileBot yếu hơn — nó có thể phản ánh rằng chromosome Niles tốt nhất đã được phân loại là babylonBot hay hardBot (cả hai có thể có tribe gene Niles đủ cao để qualify). Trong thực tế gameplay, nileBot chiến đấu ngang ngửa với hai bot kia nhờ sức mạnh tự nhiên của tribe Niles đã được ghi nhận trong Giai đoạn 2.

SummonerBot và resilientBot không được đánh giá bằng fitness thô vì đó không phải tiêu chí thiết kế của chúng. SummonerScore tổng hợp từ `genes[14]` (eSummon), `genes[5]` (wReborn), `genes[8]` (tOnDeath) và `genes[34]` (tOnAllyGroup) — những gene đặc trưng cho playstyle triệu hồi chuỗi. Score này tăng từ 7,77 ở thế hệ 0 lên 8,56 ở thế hệ 131 (+10,2%), phản ánh GA đang tiến hóa ra chromosome có profile summoner ngày càng đặc trưng hơn. Tương tự, ResilientScore tăng từ 5,59 lên 6,13 (+9,7%).

> **[HÌNH 6.4 — Radar Chart 5 Bot Được Chọn]** *Biểu đồ radar 8 trục (rút gọn từ 9 nhóm gene): Stat Weight (genes 0–1), Keywords (4–6), Trigger (7–12), Effect (13–17), Tribe (18–20), Board Context (21–23), Reroll (24–27), Spell (28–31). Giá trị mỗi trục là trung bình nhóm gene tương ứng. Năm đường màu khác nhau cho 5 bot. hardBot gần đều tất cả các trục; babylonBot spike ở Tribe; summonerBot spike ở Trigger và Effect; resilientBot spike ở Keywords.*

---

### 6.4.3 Phân Tích Diversity — Island Model Có Hoạt Động Không?

Một câu hỏi quan trọng khi đánh giá GA là: cơ chế đa dạng hóa (Island Model + Immigration) có thực sự ngăn premature convergence không? Dữ liệu training cung cấp câu trả lời rõ ràng.

Nếu premature convergence xảy ra, std_dev sẽ giảm về gần 0 và quần thể sẽ trở thành một tập clone của chromosome tốt nhất. Điều đó không xảy ra: std_dev cuối training (~540 ở thế hệ 179) so với đầu training (952 ở thế hệ 0) chỉ giảm 43% — quần thể hội tụ có chủ đích nhưng vẫn giữ được sự đa dạng đáng kể.

Quan trọng hơn, Babylon không bao giờ biến mất khỏi quần thể ngay cả trong giai đoạn Niles chiếm 70%. Đây là trực tiếp kết quả của cơ chế elitism per-archetype: top-2 Babylon chromosome được bảo toàn mỗi thế hệ bất kể fitness tương đối của chúng. Không có cơ chế này, Babylon có thể đã bị "tiến hóa ra khỏi" quần thể ở thế hệ 40–65 — và kết quả cuối training sẽ không có babylonBot đủ tiêu chuẩn để chọn.

Biểu đồ phân phối tribe qua 179 thế hệ có hình dạng của một cuộc "đấu tranh cân bằng": Niles vươn lên dominate, Immigration + Elitism kéo Babylon trở lại, rồi cân bằng mới hình thành — không phải một chiến thắng tuyệt đối của bất kỳ tribe nào. Đây chính là hành vi mong muốn của Island Model: không hội tụ về một archetype, mà duy trì đa dạng trong khi vẫn cải thiện chất lượng tổng thể.

---

### 6.4.4 Bot Có Thực Sự Chơi Khác Nhau Không?

Dữ liệu gene định lượng được trình bày ở trên, nhưng câu hỏi quan trọng hơn là định tính: khi người chơi đối mặt với babylonBot so với summonerBot, có cảm nhận thấy sự khác biệt không?

Quan sát từ playtest cho thấy có. babylonBot có xu hướng tích lũy chậm nhưng chắc — mỗi lượt đều cố gắng có đủ 3 unit Babylon trên bàn để kích hoạt tribe synergy, rồi để HP tích lũy vĩnh viễn qua nhiều lượt biến đội hình thành một khối "tường thành" chỉ số cao. Người chơi đối đầu babylonBot sẽ thấy board đối thủ tương đối ổn định (ít reroll, ít trao đổi unit) nhưng ngày càng khó xuyên phá.

summonerBot chơi hoàn toàn khác: đội hình thường nhỏ hơn về số lượng nhưng liên tục "đẻ ra" thêm unit trong combat thông qua Summon chain. Người chơi đối đầu summonerBot sẽ thấy những combat có vẻ tất thắng ở lượt đầu bỗng nhiên đảo ngược khi một loạt Mummy hay Zombie xuất hiện từ deathrattle chain. Playstyle này khó đọc và khó counter hơn babylonBot.

resilientBot ngược lại — board thường có nhiều unit Taunt + Reborn, tạo ra những combat kéo dài bởi vì phía đối thủ không thể clear board trong 20 lượt. Win condition của resilientBot không phải burst damage mà là outlast — trong nhiều trận đấu, nó hòa nhiều hơn thắng, nhưng hòa liên tiếp cũng có nghĩa là đối thủ không tích lũy được cups để thắng.

Sự khác biệt playstyle giữa 5 bot không phải ngẫu nhiên — nó là kết quả trực tiếp của chromosome encoding: các gene liên quan đến tribe synergy, trigger weights, và reroll behavior khác nhau giữa 5 bot tạo ra những quyết định mua bài và chiến lược khác nhau. GA không được "nói" phải tạo ra 5 phong cách chơi — nó tự tìm ra rằng đây là cách tồn tại trong quần thể có Island Model.

---

## 6.5 Hạn Chế Và Điểm Chưa Hoàn Thiện

Một đánh giá trung thực đòi hỏi nhìn nhận rõ những gì dự án chưa đạt được, không chỉ những gì đã làm được. Có bốn hạn chế đáng kể.

**Hạn chế 1 — Game chỉ là PvE, không có PvP thực sự.** Trong Auto Chess thương mại, trải nghiệm cốt lõi là đấu với nhiều người chơi thật cùng lúc trong một lobby — cạnh tranh không chỉ về combat mà còn về draft (tranh nhau card từ pool chung). Hệ thống này chỉ có một người chơi đối đầu vòng tròn với 5 bot — thiếu yếu tố social và competitive của thể loại. Đây là hạn chế có chủ ý (scope của dự án đặt rõ là single-player), nhưng nó ảnh hưởng đến tính đại diện của game với thể loại Auto Chess.

**Hạn chế 2 — Chromosome real-valued có thể bị stuck trong local optima.** 37 gene thực trong đoạn [0, 1] tạo ra không gian liên tục 37 chiều — về lý thuyết GA có thể tìm nghiệm toàn cục, nhưng thực tế với training 20–30 phút, không có gì đảm bảo nghiệm tìm được không phải local optima. Best fitness đạt đỉnh ở thế hệ 6 và không cải thiện sau đó là dấu hiệu có thể của một local optimum plateau — hoặc cũng có thể là true global optimum với thiết kế game hiện tại. Không có cách phân biệt hai khả năng này mà không có baseline so sánh từ nhiều lần training độc lập.

**Hạn chế 3 — Combat không có visual animation.** Người chơi hiện tại xem combat qua cơ chế "replay từ TurnRecord" — các stat number nhảy số, flash màu đỏ/xanh, unit biến mất khi chết. Đây là thiếu hụt visual đáng kể so với kỳ vọng của người chơi game card hiện đại, nơi combat thường có animation di chuyển, projectile, và hiệu ứng kỹ năng. Giới hạn này xuất phát từ ưu tiên thiết kế: headless combat engine không có khái niệm "vị trí vật lý" hay "animation state" — chỉ có slot index và chỉ số. Để có animation đầy đủ đòi hỏi refactor đáng kể ở cả combat engine lẫn UI layer.

**Hạn chế 4 — Tribe Olympus là dead gene.** Gene `genes[19]` (sOlympus — weight cho tribe Olympus) không có ý nghĩa thực tế vì không có unit nào có `tribe = Olympus` trong bộ 68 card hiện tại. Bot học được rằng gene này vô dụng và đặt giá trị thấp, nhưng đây là lãng phí một gene trong chromosome — không gian tìm kiếm bị "ô nhiễm" bởi một chiều không có tác dụng. Trong phiên bản phát triển tiếp theo, hoặc Olympus cần được implement như một tribe thực sự với unit đầy đủ, hoặc gene cần được loại bỏ khỏi chromosome.

---

## 6.6 Bài Học Kinh Nghiệm

Ba bài học từ dự án có giá trị vượt ra ngoài phạm vi của game cụ thể này.

### 6.6.1 Tại Sao GA Thay Vì MCTS Hay Reinforcement Learning?

Câu hỏi này thường được đặt ra khi nghe về AI cho game chiến lược. MCTS (Monte Carlo Tree Search) — nền tảng của AlphaGo — hoạt động tốt khi không gian trạng thái có cấu trúc cây rõ ràng và horizon ngắn. Auto Chess có horizon quá dài (20 lượt × nhiều quyết định/lượt), không gian trạng thái không có cấu trúc cây rõ ràng (ngẫu nhiên từ shop roll), và không có adversarial opponent tĩnh — MCTS không phù hợp.

Reinforcement Learning (RL) — nền tảng của AlphaStar, OpenAI Five — về lý thuyết có thể học bất kỳ game nào, nhưng đòi hỏi hàng triệu đến hàng tỷ environment steps và thường cần GPU training. Quan trọng hơn, RL reward signal trong Auto Chess rất sparse: mỗi "episode" (một game đầy đủ) chỉ cho một reward signal cuối cùng (thắng/thua), và trong 20 lượt không có intermediate signal dày đặc nào. Sparse reward kéo dài thời gian học của RL đáng kể.

GA phù hợp với ba lý do cụ thể cho bài toán này: (1) không cần gradient — toàn bộ "học" diễn ra qua selection pressure mà không cần differentiable reward; (2) khả thi trên CPU phần cứng cá nhân — mỗi simulation nhanh và stateless; (3) tự nhiên tạo ra diversity — Island Model giữ nhiều archetype song song thay vì hội tụ về một policy duy nhất.

Điều này không có nghĩa GA là lựa chọn tốt nhất về mặt tuyệt đối — mà là lựa chọn tốt nhất trong điều kiện ràng buộc cụ thể: phần cứng cá nhân, training dưới 1 giờ, và yêu cầu nhiều phong cách chơi khác nhau.

### 6.6.2 Khó Khăn Khi Thiết Kế Fitness Function

Bài học quan trọng nhất trong quá trình phát triển không đến từ code mà đến từ một câu hỏi tưởng chừng đơn giản: làm thế nào đo "chơi tốt"?

Phiên bản fitness đầu tiên chỉ dùng win/lose: thắng được 1 điểm, thua được 0. Kết quả là GA converge rất nhanh về một chiến lược — rush tất cả coin vào lượt đầu để có board đủ mạnh early game. Bot thắng nhiều trận ngắn nhưng thường thua khi gặp bot chơi economy. Fitness chỉ thưởng thắng mà không phân biệt "thắng như thế nào" tạo ra bot one-dimensional.

Phiên bản fitness hiện tại thưởng ba thành phần: kết quả trận (120/70/35 cho thắng/hòa/thua), biên thắng (`hpA×6 − hpB×3`, phân biệt thắng áp đảo với thắng cận), và tốc độ thắng (`(20−turns)×2`, thưởng kết thúc nhanh). Công thức ba thành phần này tạo ra áp lực fitness phong phú hơn và khuyến khích bot học nhiều chiều của "chơi tốt" cùng lúc.

Tuy nhiên, ngay cả công thức này vẫn có mặt hạn chế: nó thưởng tốc độ thắng (tempo) nhưng không thưởng tường minh cho economy hay late-game scaling. Bot học được một mức cân bằng tempo-economy, nhưng không nhất thiết là mức tối ưu cho game này cụ thể. Thiết kế fitness function tốt đòi hỏi iterate qua nhiều phiên bản và quan sát hành vi kết quả — không có công thức đúng từ đầu.

### 6.6.3 Island Model — Đầu Tư Complexity Có Giá Trị

Khi nhìn lại, cơ chế Island Model (5 sub-population seeded + elitism per-archetype + immigration) là phần tốn nhiều công implement nhất nhưng cũng là phần tạo ra giá trị lớn nhất.

Không có Island Model, GA gần như chắc chắn sẽ hội tụ về một hoặc hai archetype tốt nhất và loại bỏ phần còn lại. Giai đoạn Niles Domination (thế hệ 33–65) là ví dụ cụ thể: nếu không có elitism per-archetype bảo vệ Babylon, tribe này có thể biến mất hoàn toàn khỏi quần thể. Mà không có Babylon trong quần thể, babylonBot cuối cùng sẽ không tồn tại — hoặc tồn tại nhưng chỉ là hardBot với một vài gene Babylon, không phải một specialist thực sự.

Bài học tổng quát: trong các bài toán tối ưu đòi hỏi nhiều nghiệm *đa dạng* (không chỉ nghiệm tốt nhất đơn lẻ), đầu tư vào cơ chế diversity preservation là cần thiết — không phải tùy chọn. GA standard không tự nhiên tạo ra diversity; phải được thiết kế có chủ đích.

---

*[Kết thúc Chương 6 — Tiếp theo: Kết Luận]*

\newpage

# KẾT LUẬN

## 1. Nhìn Lại Hành Trình

Tiểu luận này bắt đầu từ một câu hỏi: liệu Genetic Algorithm — một thuật toán lấy cảm hứng từ sinh học tiến hóa — có thể tạo ra AI bot chơi Auto Chess theo nhiều phong cách khác nhau, chỉ bằng phần cứng cá nhân, không cần rule-based hay Neural Network? Sau toàn bộ quá trình thiết kế, lập trình, thực nghiệm và phân tích, câu trả lời là có — nhưng đi kèm với nhiều chi tiết quan trọng mà câu trả lời đơn giản đó không truyền tải được.

Dự án đã xây dựng một game Auto Chess hoàn chỉnh từ đầu: 68 lá bài với hàng trăm kỹ năng khác nhau được mô tả bằng dữ liệu JSON thuần túy; một combat engine headless có thể giải quyết một trận đấu trong vài milliseconds; một vòng lặp kinh tế và shop đủ sâu để tạo ra quyết định thú vị mỗi lượt; và một hệ thống GA training ra 5 bot chuyên biệt với profile chiến lược phân biệt. Tất cả chạy trong cùng một codebase, phục vụ cả game thật lẫn headless training mà không cần điều chỉnh runtime nào.

Nhưng điều đáng nhớ hơn kết quả là những gì dự án dạy về *cách* xây dựng hệ thống phức tạp — và chương Kết luận này là nơi để tổng hợp những bài học đó.

---

## 2. Những Đóng Góp Cụ Thể

Đặt dự án vào bối cảnh rộng hơn, có ba đóng góp kỹ thuật có giá trị tham khảo vượt ra ngoài game cụ thể này.

**Thứ nhất: Chromosome 37-gene kiểm soát hành vi bot ở mức chi tiết chưa từng thấy trong các nghiên cứu công bố về GA trong Auto Chess.** Hầu hết các công trình liên quan dùng GA để tối ưu hóa một khía cạnh nhất định — lựa chọn đội hình, quyết định reroll, hay ưu tiên mua. Chromosome 37-gene trong dự án này kiểm soát toàn bộ spectrum quyết định của bot: đánh giá chỉ số lá bài (genes 0–3), trọng số passive keyword (4–6), trọng số từng loại trigger và effect trong hệ thống TTE (7–17), ưu tiên tribe (18–20), nhận diện ngữ cảnh board (21–23), hành vi reroll và tiết kiệm (24–27), chiến lược spell (28–31), và cuối cùng là các sub-weight cho trigger phức hợp (32–36). Không có quyết định nào của bot có thể được truy xuất ngược về rule hardcode — mọi hành vi đều có "nguồn gốc gene".

**Thứ hai: TTE engine (Trigger → Target → Effect) như một kiến trúc data-driven cho game ability.** Mô hình ba chiều này cho phép 68 lá bài với hàng trăm kỹ năng được định nghĩa hoàn toàn trong JSON, không cần một dòng code C# riêng cho từng lá bài. Tính tổ hợp của TTE (14 trigger × 12 target × 13 effect = 2.184 tổ hợp cơ bản, thêm vào các modifier như `isEscalating`, `triggerLimit`, `conditionCount`) tạo ra không gian thiết kế nội dung rộng lớn mà vẫn hoàn toàn có thể kiểm soát bởi data. Kiến trúc này không phải mới trong game development, nhưng việc tích hợp nó với hệ thống AI — nơi `BotAgent` dùng trọng số trigger/effect trong chromosome để đánh giá lá bài — là một kết hợp ít được ghi nhận trong tài liệu.

**Thứ ba: Pipeline headless training khép kín trên phần cứng cá nhân.** Toàn bộ chuỗi từ `train_ai.ps1` → `GATrainer` → `GameSimulator` (432.000 trận đấu) → `AI_Library.json` → game runtime hoàn thành trong 20–30 phút mà không cần GPU, không cần cloud, không cần bất kỳ thao tác thủ công nào. Đây là kết quả của quyết định kiến trúc nhất quán từ đầu: ranh giới cứng giữa plain C# (Core Engine, AI) và MonoBehaviour (Manager, UI), với toàn bộ game logic tồn tại độc lập với Unity runtime.

---

## 3. Điều GA Dạy Về Game Balance

Một kết quả ngoài dự kiến của quá trình training là những gì GA tiết lộ về balance của game — những điều mà thiết kế tay không thể thấy được.

Giai đoạn "Niles Domination" ở thế hệ 33–65, khi tribe Niles chiếm tới 70% quần thể, không phải do thuật toán lỗi — đó là GA đang nói rằng cơ chế Reborn + OnAllyDeath chain của Niles có lợi thế tự nhiên trong combat format 20 lượt của game này. Không có playtest hay lý thuyết nào chỉ ra điều đó một cách thuyết phục; chỉ sau khi để AI "chơi thật" hàng trăm nghìn trận mới xuất hiện pattern rõ ràng đến vậy.

Điều này mở ra một ứng dụng thú vị của GA training ngoài mục tiêu AI ban đầu: dùng distribution của quần thể evolved như một công cụ *phát hiện balance vấn đề*. Nếu một tribe hay một chiến lược chiếm lĩnh quần thể sau training, đó là tín hiệu để điều tra. Nếu quần thể ổn định với diversity tốt, đó là dấu hiệu các chiến lược đang cân bằng nhau.

Insight này có giá trị thực tiễn: trong development cycle thực tế, có thể chạy một lần training ngắn (30–40 thế hệ, vài phút) sau mỗi thay đổi balance để kiểm tra xem distribution quần thể có bị lệch không — một quy trình testing AI-driven cho game balance.

---

## 4. Hướng Phát Triển Tương Lai

Dự án hiện tại là một nền tảng hoàn chỉnh nhưng còn nhiều hướng mở rộng có giá trị.

**Multiplayer PvP** là bước tự nhiên tiếp theo. Phần lớn kiến trúc đã headless-ready; thêm multiplayer đòi hỏi một server layer để đồng bộ board state giữa nhiều client, không phải refactor game logic. Cạnh tranh với người chơi thật — tranh nhau card từ pool chung, đọc được chiến lược của đối thủ qua board họ để lộ — sẽ mang lại chiều sâu gameplay mà PvE không thể tái tạo.

**Neural-GA (NEAT)** là bước tiến hóa tự nhiên của hệ thống AI. Thay vì chromosome real-valued mã hóa trọng số cố định, NEAT (NeuroEvolution of Augmenting Topologies) tiến hóa cả cấu trúc mạng neural lẫn trọng số. Với NEAT, bot không chỉ học "gene nào quan trọng hơn" mà học *cách kết hợp thông tin* từ nhiều nguồn — board state, coin hiện tại, turn số, lịch sử combat. Điều này cho phép biểu diễn những chiến lược phức tạp hơn như "thay đổi tactic tùy theo giai đoạn game" mà chromosome 37-gene không thể encode.

**Mở rộng nội dung** — thêm tribe Olympus với unit đầy đủ, nâng pool card lên 100+, thêm 2–3 mechanic mới — là hướng đơn giản nhất về mặt kỹ thuật nhờ data-driven architecture của TTE engine. Một tribe mới chỉ đòi hỏi thêm entry JSON và một gene mới trong chromosome; không cần thay đổi code engine.

**Replay system đầy đủ** là cải tiến UX quan trọng. `TurnRecord` đã có đầy đủ thông tin để replay từng lượt combat; cần thêm lưu trữ toàn bộ game history và UI scrubbing để người chơi có thể xem lại bất kỳ lượt nào — đặc biệt hữu ích khi học từ những trận thua.

**Mobile port** là mục tiêu thương mại hóa tự nhiên — thể loại Auto Chess vốn phù hợp với mobile (session ngắn, turn-based, không cần phản xạ nhanh). Unity cross-platform compilation về lý thuyết đơn giản; thách thức thực tế là UI adaptation cho màn hình nhỏ và touch input.

---

## 5. Kết Luận Cá Nhân

Nhìn lại toàn bộ dự án, điều tôi học được nhiều nhất không đến từ bất kỳ thuật toán hay pattern nào cụ thể — mà đến từ việc trải nghiệm trực tiếp rằng *kiến trúc tốt là điều kiện để mọi thứ khác có thể tồn tại*.

Hệ thống AI training chỉ có thể chạy headless vì tầng Core Engine được tách biệt khỏi Unity ngay từ đầu. Khả năng thêm lá bài mới mà không cần sửa code chỉ có thể xảy ra vì TTE engine được thiết kế data-driven ngay từ đầu. Khả năng debug một edge case trong combat chỉ liên quan đến `CombatResolver` mà không sợ ảnh hưởng đến UI chỉ có thể xảy ra vì ranh giới phụ thuộc được duy trì nghiêm ngặt ngay từ đầu. Trong cả ba trường hợp, "ngay từ đầu" là điều kiện then chốt — kiến trúc khó refactor về sau khi hệ thống đã phức tạp.

Genetic Algorithm, cụ thể hơn, dạy một bài học về sự khiêm tốn của người thiết kế: AI không học theo cách tôi nghĩ nó sẽ học, mà tìm ra những con đường trong không gian gene mà tôi không dự đoán trước. Giai đoạn Niles Domination, cách summonerBot học được rằng genes[27] (proactive sell) cần thấp để không bán đi unit "mồi" cho summon chain, cách resilientBot độc lập tìm ra tổ hợp Taunt + Reborn là phòng thủ tốt nhất — tất cả những hành vi đó phát sinh từ áp lực selection, không từ instruction của tôi. Đây là điều thú vị và hơi đáng sợ cùng lúc về evolutionary computation: hệ thống tìm ra những giải pháp mà người thiết kế không hình dung được, và đôi khi những giải pháp đó tiết lộ những điều về bài toán mà người thiết kế đã bỏ qua.

Cuối cùng, dự án này là bằng chứng rằng nghiên cứu AI trong game không cần hàng tỷ tham số hay data center để tạo ra kết quả thú vị. Với 37 con số, một fitness function được thiết kế cẩn thận, và 432.000 trận đấu mô phỏng trong 30 phút, Genetic Algorithm có thể tạo ra những bot chơi Auto Chess theo cách đủ đa dạng để người chơi thật nhận ra và cảm nhận sự khác biệt. Đôi khi, sự đơn giản có chủ ý là công cụ mạnh nhất.

---

*[Kết thúc Tiểu Luận — Tiếp theo: Phụ Lục]*

\newpage

# PHỤ LỤC

---

## Phụ lục A — Danh Sách Đầy Đủ Card

Tổng cộng **68 card** trong game, chia làm 3 nhóm: 43 unit card có thể mua, 4 unit token (chỉ xuất hiện qua ability), và 21 spell card (19 mua được + 2 token).

Tribe: **B** = Babylon (1), **O** = Olympus (2), **N** = Niles (3), **—** = không có.  
Keywords: **T** = Taunt, **R** = Reborn, **S** = Safeguard.

### A.1 Unit Cards — Babylon (22 non-token + 1 token)

| # | ID | Tên | Tier | ATK/HP | Cost | KW | Kỹ năng tóm tắt |
|:-:|:---|:----|:----:|:------:|:----:|:--:|:----------------|
| 1 | U_01_B | **Ereskigal** | 6 | 10/10 | 3 | | OnAllySell (chỉ Babylon, max 2×tier): Hấp thụ vĩnh viễn toàn bộ chỉ số unit bán |
| 2 | U_02_B | **Ninurta** | 6 | 9/9 | 3 | | OnAllyDeploy (mỗi 2 lần): Nhận 1 Ancient Coin vào hand |
| 3 | U_03_B | **Utu** | 6 | 8/8 | 3 | | OnAllyDeploy (Babylon): +2/+2 vĩnh viễn cho toàn bộ đồng minh Babylon |
| 4 | U_04_B | **Ki** | 5 | 6/6 | 3 | | OnAllyDeploy (Babylon, mỗi 2 lần, max 1×tier): +3/+3 vĩnh viễn cho đồng minh yếu nhất |
| 5 | U_05_B | **Nanna** | 5 | 6/6 | 3 | | OnSell: Trao toàn bộ chỉ số bản thân vĩnh viễn cho 2 đồng minh ngẫu nhiên |
| 6 | U_06_B | **Ashur** | 5 | 6/6 | 3 | | OnAllySell: +1/+2 vĩnh viễn cho toàn bộ đồng minh |
| 7 | U_07_B | **Enki** | 5 | 2/15 | 3 | T | OnTakeDamage: Gây 5/10/15 sát thương lên kẻ địch trực tiếp |
| 8 | U_08_B | **Gugalana** | 4 | 10/10 | 3 | T | OnDeploy: +10/+10 vĩnh viễn cho bản thân |
| 9 | U_09_B | **Umu Drabutu** | 4 | 5/5 | 3 | | OnDeploy (1 lần): Nhận 2 unit Babylon ngẫu nhiên (≤tier shop) vào hand |
| 10 | U_10_B | **Enkidu** | 4 | 5/5 | 3 | | OnAllyDeploy (Babylon): +1/+2 vĩnh viễn cho đồng minh bên phải |
| 11 | U_11_B | **Gilgamesh** | 4 | 8/8 | 3 | | OnStatGain: +2/+1 vĩnh viễn cho bản thân |
| 12 | U_12_B | **Humbaba** | 4 | 7/1 | 3 | | OnAttack: Nhận 2 coin |
| 13 | U_13_B | **Asag** | 4 | 8/8 | 3 | | OnDeploy: Hấp thụ và nuốt đồng minh Babylon bên trái + bên phải |
| 14 | U_14_B | **Usumgallu** | 3 | 3/3 | 3 | T | OnTakeDamage: +1/+1 cho toàn bộ đồng minh Babylon (trận này) |
| 15 | U_15_B | **Lamashtu** | 3 | 1/5 | 3 | | OnAllySell: +1/+0 vĩnh viễn cho toàn bộ đồng minh Babylon |
| 16 | U_16_B | **Pazuzu** | 3 | 1/2 | 3 | | OnDeploy: Nhận 2 coin |
| 17 | U_17_B | **Kingu** | 2 | 2/2 | 3 | T R | *(không có ability — passive Taunt + Reborn)* |
| 18 | U_18_B | **Uridimmu** | 2 | 5/1 | 3 | | OnAllySell: +0/+1 vĩnh viễn cho toàn bộ đồng minh Babylon |
| 19 | U_19_B | **Ugallu** | 2 | 3/3 | 3 | | OnSell: Nhận 1 unit Babylon ngẫu nhiên (≤tier 2) vào hand |
| 20 | U_20_B | **Kusarikku** | 1 | 3/3 | 3 | | OnAllySell: +1/+1 vĩnh viễn cho bản thân |
| 21 | U_21_B | **Babilonian Soldier** | 1 | 2/3 | 3 | | OnAllyDeploy (Babylon): +0/+1 vĩnh viễn cho bản thân |
| 22 | U_22_B | **Sumerian Scholar** | 1 | 2/3 | 3 | | OnDeploy: Nhận 1 Ancient Coin vào hand |
| — | U_23_B | *Galla* *(token)* | 1 | 2/2 | — | | *(token Babylon — không có ability)* |

### A.2 Unit Cards — Niles (21 non-token + 3 token)

| # | ID | Tên | Tier | ATK/HP | Cost | KW | Kỹ năng tóm tắt |
|:-:|:---|:----|:----:|:------:|:----:|:--:|:----------------|
| 23 | U_01_N | **Anubis** | 6 | 5/5 | 3 | | OnAllyDeath (bất kỳ, max 2): Ban Reborn cho đồng minh HP thấp nhất |
| 24 | U_02_N | **Sekhmet** | 6 | 9/3 | 3 | | OnAllySummon (bất kỳ, max 2): Nuốt unit đó. OnDeath: Triệu hồi lại tất cả đã nuốt |
| 25 | U_03_N | **Osiris** | 6 | 3/3 | 3 | | OnAllyReborn (bất kỳ): Nhân đôi/ba/bốn chỉ số unit đó (×merge+1) |
| 26 | U_04_N | **Sobek** | 5 | 2/2 | 3 | | OnAllySummon/OnAllyReborn (bất kỳ): +1/+2 vĩnh viễn cho bản thân |
| 27 | U_05_N | **Bastet** | 5 | 4/8 | 3 | | OnAllySummon (mỗi 2 lần): +1/+1 cho tất cả Niles (tăng dần, trận này) |
| 28 | U_06_N | **Thoth** | 5 | 8/5 | 3 | R | OnDeath: +2/+0 vĩnh viễn toàn cầu cho *mọi* unit Niles (board/hand/shop/future) |
| 29 | U_07_N | **Isis** | 5 | 2/9 | 3 | | OnAllyReborn: +3/+3 vĩnh viễn cho toàn bộ đồng minh |
| 30 | U_08_N | **Sepopard** | 4 | 5/2 | 3 | | OnAllySummon: +4/+4 cho unit vừa được triệu hồi (trận này) |
| 31 | U_09_N | **Nefertem** | 4 | 4/2 | 3 | | OnDeath: Ban Reborn cho 1 đồng minh Niles ngẫu nhiên |
| 32 | U_10_N | **Sand Golem** | 4 | 6/2 | 3 | | OnDeath: Triệu hồi 2 Niles Warrior |
| 33 | U_11_N | **Upamaki** | 4 | 5/2 | 3 | | OnDeploy: Nuốt đồng minh Niles bên trái. OnDeath: Triệu hồi lại unit đã nuốt |
| 34 | U_12_N | **Cat Archer** | 3 | 3/1 | 3 | | StartOfBattle: Kích hoạt kỹ năng của đồng minh bên trái |
| 35 | U_13_N | **Niles Cultist** | 3 | 3/1 | 3 | | OnAllyDeath (mỗi 3 lần): +1/+0 vĩnh viễn cho toàn bộ đồng minh Niles |
| 36 | U_14_N | **Scorpion** | 3 | 1/1 | 3 | | OnAttack: Gây thêm 5 sát thương lên kẻ địch trực tiếp |
| 37 | U_15_N | **Ammit** | 3 | 2/2 | 3 | | OnAllyDeath: +1/+1 vĩnh viễn cho bản thân |
| 38 | U_16_N | **Hyena Thief** | 2 | 3/1 | 3 | | OnDeploy: Nhận 1 Ancient Coin vào hand |
| 39 | U_17_N | **Babi** | 2 | 1/1 | 3 | R | Aura: +2/+0 vĩnh viễn cho 1 đồng minh ngẫu nhiên (mỗi đầu trận) |
| 40 | U_18_N | **Niles Commander** | 2 | 4/4 | 3 | | OnAllySummon/OnAllyReborn (bất kỳ): +1/+1 cho bản thân (trận này) |
| 41 | U_19_N | **Tawaret** | 2 | 0/1 | 3 | | OnDeath: Triệu hồi 2 Medjed |
| 42 | U_20_N | **Niles Warrior** | 1 | 2/1 | 3 | | OnDeath: Triệu hồi 1 Mummy |
| 43 | U_21_N | **Griffin** | 1 | 2/1 | 3 | T R | *(không có ability — passive Taunt + Reborn)* |
| — | U_22_N | *Mummy* *(token)* | 1 | 1/1 | — | | *(triệu hồi khi Niles Warrior chết)* |
| — | U_23_N | *Medjed* *(token)* | 1 | 3/2 | — | | *(triệu hồi khi Tawaret chết)* |
| — | U_24_N | *Dark Medjed* *(token)* | 1 | 3/2 | — | | *(token triệu hồi)* |

### A.3 Spell Cards (19 non-token + 2 token)

| # | ID | Tên | Tribe | Tier | Cost | Mô tả |
|:-:|:---|:----|:-----:|:----:|:----:|:------|
| 1 | S_02 | **Sharpen Blade** | — | 1 | 1 | +3/+0 vĩnh viễn cho 1 đồng minh |
| 2 | S_03 | **Plated Armor** | — | 1 | 1 | +0/+3 + Taunt vĩnh viễn cho 1 đồng minh |
| 3 | S_04 | **Quick Recruit** | — | 1 | 2 | Nhận 1 unit ngẫu nhiên tier 1 vào hand |
| 4 | S_05 | **Sanctum Heist** | — | 1 | 2 | Lấy 1 unit ngẫu nhiên từ shop hiện tại |
| 5 | S_06 | **Let's Feast** | — | 1 | 2 | +0/+2 cho 3 đồng minh ngẫu nhiên đang trên bàn (trận này) |
| 6 | S_07 | **Balance Stance** | — | 1 | 2 | +3/+3 vĩnh viễn cho 1 đồng minh |
| 7 | S_08 | **Trader's Trick** | — | 2 | 1 | +2 coin thu nhập đến hết lượt này |
| 8 | S_17 | **Wager** | — | 2 | 1 | Nếu thắng trận kế tiếp: nhận 3 coin |
| 9 | S_13 | **Strengthen Bond** | B | 2 | 1 | +1/+1 vĩnh viễn cho toàn bộ đồng minh |
| 10 | S_12 | **Caishen's Knock** | B | 2 | 2 | +1 coin thu nhập cố định mỗi lượt (vĩnh viễn) |
| 11 | S_09 | **Tailored Recruit** | B | 2 | 3 | Chọn 1 unit, nhận 1 unit khác cùng tribe vào hand |
| 12 | S_14 | **Rising Spirit** | B | 3 | 2 | Cho 1 đồng minh ability: cuối lượt shop nhận +2/+2 |
| 13 | S_18 | **Ritual of the Realm** | O | 3 | 3 | Nâng 1 unit cùng tribe lên +1 tier (upgrade) |
| 14 | S_19 | **Olympic Flame** | O | 4 | 3 | Nhân đôi ATK + ban Safeguard cho 1 đồng minh |
| 15 | S_16 | **Gate of Destruction** | B | 4 | 3 | Loại bỏ 1 đồng minh, chuyển chỉ số của nó sang 1 đồng minh ngẫu nhiên |
| 16 | S_10 | **Devil's Deal** | O | 4 | 1 | Mất 1 HP, nhận 6 coin |
| 17 | S_20 | **Change of Heart** | N | 2 | 2 | Toggle Taunt: nếu chưa có → +0/+5 + Taunt; nếu có → +3/+0, xóa Taunt |
| 18 | S_11 | **Divine Inspiration** | N | 3 | 2 | Ban cho 1 đồng minh ATK và HP bằng shop tier hiện tại |
| 19 | S_15 | **Military Support** | B | 5 | 5 | Nhận 3 unit ngẫu nhiên tier 5 vào hand |
| — | S_01 | *Ancient Coin* *(token)* | — | 1 | — | +1 coin khi dùng |
| — | S_00 | *Tinh Hoa Hợp Nhất* *(token)* | — | — | — | Phần thưởng merge (nội bộ game) |

---

## Phụ lục B — Bảng Định Nghĩa 37 Gene

Mỗi gene là một số thực trong đoạn [0, 1]. Giá trị cao = hành vi tương ứng được ưu tiên hơn.

| Gene | Ký hiệu | Nhóm | Ý nghĩa |
|:----:|:--------|:----:|:--------|
| 0 | wATK | Stat | Trọng số ATK khi đánh giá lá bài |
| 1 | wHP | Stat | Trọng số HP |
| 2 | wTierBonus | Stat | Bonus mỗi tier vượt quá 1 |
| 3 | wCostEff | Stat | Mức độ nhấn mạnh hiệu quả chi phí (stat/coin) |
| 4 | wTaunt | Keyword | Giá trị của passive Taunt |
| 5 | wReborn | Keyword | Giá trị của passive Reborn |
| 6 | wSafeguard | Keyword | Giá trị của passive Safeguard |
| 7 | tAura_old | Trigger | *(Kế thừa — hiện dùng gene[32])* |
| 8 | tOnDeath | Trigger | Trọng số trigger OnDeath (deathrattle) |
| 9 | tOnAllyDeath | Trigger | Trọng số trigger OnAllyDeath |
| 10 | tOnTakeDmg | Trigger | Trọng số trigger OnTakeDamage |
| 11 | tEndTurnShop | Trigger | Trọng số trigger EndTurnShop |
| 12 | tOnAttack | Trigger | Trọng số trigger OnAttack |
| 13 | eAddStats | Effect | Trọng số effect AddStats |
| 14 | eSummon | Effect | Trọng số effect Summon |
| 15 | eDealDmg | Effect | Trọng số effect DealDamage |
| 16 | eGainCoin | Effect | Trọng số effect GainCoin |
| 17 | eGiveBuff | Effect | Trọng số effect GiveBuff / support |
| 18 | sBabylon | Tribe | Ưu tiên tribe Babylon |
| 19 | sOlympus | Tribe | Ưu tiên tribe Olympus *(dead gene — không có unit Olympus)* |
| 20 | sNiles | Tribe | Ưu tiên tribe Niles |
| 21 | wMergeBonus | Board | Thưởng cho lá bài có bản sao trên board (gần merge) |
| 22 | wTauntFront | Board | Ưu tiên đặt Taunt unit vào frontline |
| 23 | wBuyThreshold | Board | Ngưỡng điểm tối thiểu để mua lá bài |
| 24 | wRerollThresh | Reroll | Ngưỡng chất lượng shop để kích hoạt reroll |
| 25 | wMaxRerolls | Reroll | Số lần reroll tối đa mỗi lượt (nhân 3, làm tròn) |
| 26 | wCoinReserve | Reroll | Coin dự phòng tối thiểu trước khi reroll |
| 27 | wProactiveSell | Reroll | Ngưỡng điểm để bán chủ động unit yếu |
| 28 | wSpellThresh | Spell | Ngưỡng điểm tối thiểu để mua spell |
| 29 | wSpellStrong | Spell | Ưu tiên cast spell lên unit mạnh nhất |
| 30 | wSpellMerge | Spell | Ưu tiên cast spell lên unit gần merge |
| 31 | wEconWeight | Spell | Trọng số giá trị kinh tế (GainCoin, Income) |
| 32 | tAura | Trigger Con | Trọng số trigger Aura (tách khỏi gene[7]) |
| 33 | tOnSell | Trigger Con | Trọng số trigger OnSell |
| 34 | tOnAllyGroup | Trigger Con | Trọng số OnAllyDeath / OnAllySummon / OnAllyReborn |
| 35 | tOnAllyDeploy | Trigger Con | Trọng số trigger OnAllyDeploy |
| 36 | tOnAllySell | Trigger Con | Trọng số trigger OnAllySell |

---

## Phụ lục C — Giá Trị Gene Của 5 Bot Trained

Giá trị từ `AI_Library.json` sau lần production training (120 pop × 180 gen). Các giá trị **đậm** là gene đặc trưng nổi bật (≥ 0.85 hoặc ≤ 0.05) thể hiện playstyle của bot.

| Gene | Ký hiệu | hardBot | babylonBot | nileBot | summonerBot | resilientBot |
|:----:|:--------|:-------:|:----------:|:-------:|:-----------:|:------------:|
| 0 | wATK | 0.040 | 0.040 | **0.000** | 0.289 | **0.000** |
| 1 | wHP | **0.863** | **0.991** | **0.863** | **0.851** | **0.863** |
| 2 | wTierBonus | 0.487 | 0.392 | 0.487 | 0.642 | 0.487 |
| 3 | wCostEff | 0.075 | 0.123 | 0.177 | 0.491 | **0.035** |
| 4 | wTaunt | 0.260 | 0.241 | **0.878** | 0.188 | 0.259 |
| 5 | wReborn | **0.926** | **0.908** | 0.761 | 0.785 | **1.000** |
| 6 | wSafeguard | 0.353 | 0.558 | 0.114 | 0.180 | 0.558 |
| 7 | tAura_old | 0.360 | 0.360 | 0.401 | 0.091 | 0.324 |
| 8 | tOnDeath | **1.000** | **1.000** | 0.746 | 0.545 | **1.000** |
| 9 | tOnAllyDeath | **0.000** | **0.000** | **0.983** | **0.812** | **0.000** |
| 10 | tOnTakeDmg | 0.720 | 0.710 | 0.710 | 0.070 | 0.693 |
| 11 | tEndTurnShop | 0.692 | 0.692 | 0.323 | 0.219 | **0.947** |
| 12 | tOnAttack | 0.594 | 0.578 | 0.174 | 0.553 | 0.540 |
| 13 | eAddStats | 0.622 | **0.060** | 0.792 | 0.394 | 0.221 |
| 14 | eSummon | **0.937** | 0.234 | **0.958** | 0.480 | 0.741 |
| 15 | eDealDmg | **0.904** | 0.610 | **0.907** | 0.686 | 0.772 |
| 16 | eGainCoin | **0.992** | **0.942** | 0.265 | **0.047** | 0.158 |
| 17 | eGiveBuff | **0.882** | **0.846** | 0.594 | **0.940** | 0.288 |
| 18 | sBabylon | 0.478 | **0.559** | 0.379 | 0.462 | 0.478 |
| 19 | sOlympus | 0.144 | **0.018** | 0.313 | **0.841** | 0.127 |
| 20 | sNiles | 0.360 | 0.402 | **0.946** | 0.441 | 0.357 |
| 21 | wMergeBonus | 0.500 | 0.471 | 0.191 | 0.095 | 0.405 |
| 22 | wTauntFront | 0.452 | 0.283 | 0.065 | 0.419 | 0.452 |
| 23 | wBuyThreshold | **0.040** | **0.021** | 0.382 | **0.731** | 0.158 |
| 24 | wRerollThresh | **0.940** | 0.741 | **0.887** | **0.975** | **0.986** |
| 25 | wMaxRerolls | 0.207 | 0.122 | 0.172 | 0.197 | 0.207 |
| 26 | wCoinReserve | **0.047** | 0.075 | 0.107 | **0.015** | 0.057 |
| 27 | wProactiveSell | 0.156 | **0.036** | 0.125 | **0.826** | **0.000** |
| 28 | wSpellThresh | 0.082 | 0.067 | **0.022** | 0.294 | 0.227 |
| 29 | wSpellStrong | 0.606 | 0.401 | 0.606 | **0.907** | 0.532 |
| 30 | wSpellMerge | 0.692 | 0.359 | 0.453 | 0.184 | 0.453 |
| 31 | wEconWeight | 0.744 | **0.958** | **0.875** | **0.891** | **0.875** |
| 32 | tAura | 0.346 | 0.559 | 0.491 | **0.917** | 0.491 |
| 33 | tOnSell | 0.230 | **0.018** | 0.230 | **0.805** | 0.230 |
| 34 | tOnAllyGroup | 0.533 | 0.526 | **1.000** | 0.129 | **1.000** |
| 35 | tOnAllyDeploy | **0.000** | **0.000** | **1.000** | **0.629** | **1.000** |
| 36 | tOnAllySell | 0.143 | 0.125 | 0.143 | 0.546 | 0.143 |
| | **Fitness** | **14400** | **11597** | **9169** | **7577** | **7436** |

*Lưu ý: Fitness trong bảng là tổng tích lũy qua 20 trận × nhiều thế hệ, không so sánh trực tiếp với fitness thô (max 4764/trận) trong phần phân tích.*

**Đọc bảng:** Ba cột đặc trưng rõ nhất:
- **nileBot** và **resilientBot**: genes[34–35] đều bằng 1.0, phản ánh ưu tiên cực cao cho OnAllyGroup và OnAllyDeploy triggers — cơ sở của playstyle triệu hồi và phòng thủ.
- **resilientBot**: genes[5] = 1.0 (Reborn max) + genes[8] = 1.0 (OnDeath max) + genes[27] = 0.0 (không bao giờ bán chủ động) — phòng thủ tuyệt đối.
- **babylonBot**: genes[1] = 0.991 (HP max), genes[31] = 0.958 (Economy max), genes[13] = 0.060 (AddStats thấp bất ngờ — ưu tiên spell economy hơn buff thủ công).

---

## Phụ lục D — Code Snippets Quan Trọng

### D.1 Hàm Đánh Giá Lá Bài — BotAgent.Evaluate()

Hàm cốt lõi kết nối chromosome với game state, được gọi trong mọi quyết định mua:

```csharp
private float Evaluate(CardDefinition c)
{
    float score = c.baseATK * brain.genes[0]
                + c.baseHP  * brain.genes[1]
                + (c.tier - 1) * brain.genes[2] * 5f

                + (c.hasTaunt     ? brain.genes[4] * 10f : 0)
                + (c.hasReborn    ? brain.genes[5] * 12f : 0)
                + (c.hasSafeguard ? brain.genes[6] * 8f  : 0);

    if (c.abilities != null)
    {
        int sameTribeCount = board.Count(u => u != null
                             && u.Data.tribe == c.tribe && !u.IsDead);
        foreach (var ability in c.abilities)
        {
            float tw = TriggerWeight(ability.trigger);
            float ew = EffectWeight(ability.effect);
            float ctx = (ability.trigger == TriggerType.OnAllyDeath
                      || ability.trigger == TriggerType.OnAllySummon
                      || ability.trigger == TriggerType.OnAllyReborn)
                      ? Mathf.Clamp01(sameTribeCount / 2f) : 1f;

            score += tw * ew * 10f * ctx;
            if (ability.isEscalating) score += tw * ew * 3f;
        }
    }

    score += sameTribeCount * SynergyWeight(c.tribe) * 4f;
    score += copiesOnBoard  * brain.genes[21] * (copies == 2 ? 16f : 8f);
    score += (c.hasTaunt ? emptyFront * brain.genes[22] * 2f : 0);

    return score / Mathf.Max(1, c.cost) * (1 + brain.genes[3]);
}
```

### D.2 Hàm Fitness Một Trận — GameSimulator.ScoreFromA()

Ba thành phần fitness thưởng: kết quả nhị phân, biên thắng, và tốc độ:

```csharp
private static float ScoreFromA(int result, int hpA, int hpB, int turns)
{
    float score = result > 0 ? 120f    // thắng
                : result == 0 ? 70f    // hòa
                : 35f;                 // thua

    score += hpA * 6f;    // thưởng HP còn lại
    score -= hpB * 3f;    // phạt HP đối thủ còn lại

    if (result > 0)
        score += (MaxTurns - turns) * 2f;   // thưởng tốc độ thắng

    return Mathf.Max(1f, score);
}
```

### D.3 Tournament Selection Thích Nghi — GATrainer

Kích thước tournament tăng theo tiến trình để tăng áp lực chọn lọc:

```csharp
private static int CurrentTournamentSize(float progress)
{
    if (progress < 0.35f) return 3;   // early: diversity cao
    if (progress < 0.75f) return 4;   // mid
    return 5;                          // late: hội tụ nhanh
}

private Chromosome TournamentSelect(List<Chromosome> pop, float progress)
{
    int k = CurrentTournamentSize(progress);
    var contestants = Enumerable.Range(0, k)
        .Select(_ => pop[Random.Range(0, pop.Count)])
        .ToList();
    return contestants.MaxBy(c => c.fitness);
}
```

### D.4 Elitism Bốn Tầng — GATrainer

Bảo toàn top global và top mỗi archetype riêng:

```csharp
// Tầng 1: Elite tổng
AddTopClones(nextGen, population, c => true, eliteCount);

// Tầng 2–4: Elite theo archetype
AddTopClones(nextGen, population, IsBabylon,   2);
AddTopClones(nextGen, population, IsNiles,     2);
AddTopClones(nextGen, population.OrderByDescending(SummonerScore),   2);
AddTopClones(nextGen, population.OrderByDescending(ResilientScore),  2);
```

### D.5 Death Stack — CombatResolver.FlushDeathStack()

Xử lý cái chết tuần tự LIFO, đảm bảo chain death không bị interleave:

```csharp
private void FlushDeathStack(List<CardInstance> pBoard, List<CardInstance> eBoard)
{
    while (deathStack.Count > 0)
    {
        var evt = deathStack.Pop();
        var (victim, killer, vBoard, kBoard) = evt;

        if (victim.isReborn && !victim.hasRebornUsed)
        {
            victim.ReviveDefault();
            engine.BroadcastAllyEvent(TriggerType.OnAllyReborn, victim, vBoard, kBoard);
        }
        else
        {
            int slot = vBoard.IndexOf(victim);
            if (slot >= 0) vBoard[slot] = null;

            engine.TriggerAbility(TriggerType.OnDeath, victim, killer, vBoard, kBoard);
            engine.BroadcastAllyEvent(TriggerType.OnAllyDeath, victim, vBoard, kBoard);
        }

        while (engine.HasPendingSummons)
            engine.ProcessNextPendingSummon();
    }
}
```

---

## Phụ lục E — Tài Liệu Tham Khảo

### Sách và Giáo Trình

[1] Mitchell, M. (1998). *An Introduction to Genetic Algorithms*. MIT Press. — Nền tảng lý thuyết GA: chromosome encoding, selection, crossover, mutation, convergence.

[2] Goldberg, D. E. (1989). *Genetic Algorithms in Search, Optimization and Machine Learning*. Addison-Wesley. — Tác phẩm kinh điển về GA, bao gồm Schema Theorem và Building Block Hypothesis.

[3] Russell, S., & Norvig, P. (2020). *Artificial Intelligence: A Modern Approach* (4th ed.). Pearson. — Chương về Evolutionary Computation và Local Search.

### Bài Báo Nghiên Cứu

[4] Stanley, K. O., & Miikkulainen, R. (2002). Evolving Neural Networks through Augmenting Topologies. *Evolutionary Computation*, 10(2), 99–127. — NEAT: nền tảng cho Neural-GA, được đề cập trong hướng phát triển tương lai.

[5] Whiteson, S., & Stone, P. (2006). Evolutionary Function Approximation for Reinforcement Learning. *Journal of Machine Learning Research*, 7, 877–917. — So sánh GA và RL trong game AI.

[6] Lucas, S. M., & Kendall, G. (2006). Evolutionary Computation and Games. *IEEE Computational Intelligence Magazine*, 1(1), 10–18. — Tổng quan ứng dụng GA trong game design và NPC AI.

[7] Perez, D., Samothrakis, S., Lucas, S., & Rohlfshagen, P. (2013). Rolling Horizon Evolution versus Tree Search for Navigation in Single-Player Real-Time Games. *Proceedings of GECCO 2013*. — So sánh MCTS và evolutionary methods trong game AI.

### Tài Liệu Kỹ Thuật

[8] Unity Technologies. (2024). *Unity Documentation: Manual & Scripting Reference*. unity.com/docs — MonoBehaviour lifecycle, partial class, batch mode, JsonUtility.

[9] Microsoft. (2024). *C# Language Specification 11.0*. docs.microsoft.com — Partial class, null-conditional operator, LINQ, generic collections.

[10] Garfield, R. (1994). *Magic: The Gathering* — Game Design Document. Wizards of the Coast. — Thiết kế trigger-based card ability, ảnh hưởng đến mô hình TTE.

### Nguồn Tham Khảo Về Auto Chess

[11] Drodo Studio. (2019). *Dota Auto Chess* — Dota 2 Workshop. — Game khởi nguồn thể loại Auto Chess, tham khảo cơ chế draft, merge, và economy.

[12] Riot Games. (2019). *Teamfight Tactics* — Game mechanics documentation. — Tham khảo drop rate, shop tier progression, và synergy system.

[13] Blizzard Entertainment. (2020). *Hearthstone Battlegrounds* — Official blog posts on game design. — Tham khảo deathrattle chain và summon mechanic.

### Công Cụ Phát Triển

[14] JetBrains. (2024). *Rider IDE 2024* — IDE cho Unity C# development, hỗ trợ partial class navigation.

[15] Git / GitHub. — Version control, branch management, commit history tham chiếu trong báo cáo.

---

*[Kết thúc Phụ lục — Hết tiểu luận]*