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

Một bot RL học bởi neural network với hàng triệu trọng số là một **hộp đen** — không thể biết tại sao nó đưa ra quyết định nào. Điều này không chỉ là vấn đề học thuật: trong ngữ cảnh nghiên cứu, khả năng phân tích "bot này ưu tiên một loại chiến lược cụ thể vì một tham số tương ứng trong biểu diễn nội tại của nó có giá trị cao" là một đóng góp khoa học có giá trị mà RL không thể cung cấp (cách biểu diễn cụ thể được trình bày ở Chương 5).

Những hạn chế này không phủ nhận tiềm năng của RL — chúng chỉ đặt ra câu hỏi về **tính phù hợp** của RL cho bài toán cụ thể này, trong ngữ cảnh tài nguyên cụ thể này. Và đó là không gian mà Genetic Algorithm bước vào.

---

## 1.3 Genetic Algorithm Trong Game AI

### 1.3.1 Lịch Sử Ứng Dụng — Từ Hành Vi NPC Đến Balancing

Genetic Algorithm không phải là công nghệ mới trong game AI — nó đã được thử nghiệm ở nhiều hướng khác nhau từ đầu những năm 2000. Điểm qua các mốc quan trọng cho thấy hướng tiếp cận này đã được kiểm chứng ở nhiều khía cạnh:

**Tiến hóa hành vi NPC:** Một trong những ứng dụng sớm nhất là tiến hóa hành vi của Non-Player Character (NPC). Thay vì lập trình hành vi cứng, nhà nghiên cứu định nghĩa một tập tham số hành vi (tốc độ di chuyển, ngưỡng aggro, chiến lược tấn công/phòng thủ) và dùng GA để tìm bộ tham số tạo ra NPC thú vị nhất theo một hàm fitness định nghĩa sẵn. Kết quả thường vượt trội so với behavior được thiết kế tay về tính đa dạng và khả năng thích nghi.

**NEAT — NeuroEvolution of Augmenting Topologies:** Năm 2002, Kenneth Stanley và Risto Miikkulainen giới thiệu **NEAT**, một framework tiến hóa không chỉ trọng số của neural network mà cả *cấu trúc* (topology) của nó. NEAT nổi tiếng nhất qua ứng dụng *MarI/O* — một agent NEAT học chơi Super Mario Bros từ đầu chỉ qua quan sát pixel, không có bất kỳ kiến thức lập trình sẵn nào về game. NEAT thể hiện rằng GA có thể tìm kiếm trong không gian giải pháp cực kỳ phức tạp (cấu trúc mạng nơ-ron) khi không gian đó được encode đúng cách.

**Game Balancing qua GA:** Một ứng dụng thực tiễn quan trọng khác là cân bằng game tự động. Thiết kế thủ công một bộ bài với hàng chục loại kỹ năng tương tác lẫn nhau — như trong dự án này (xem Chương 3) — rồi điều chỉnh tay từng giá trị số để không có chiến lược "ăn tất cả", là công việc cực kỳ tốn kém về nhân lực và thời gian. Các nghiên cứu đã sử dụng GA để tự động tìm kiếm bộ tham số cân bằng nhất theo các tiêu chí đo lường như entropy chiến lược (tất cả chiến lược đều có tỉ lệ thắng gần bằng nhau) hay diversity of winning strategies. Đây là hướng nghiên cứu đang được các hãng game AAA quan tâm.

**Evolving Bot Personalities:** Một số nghiên cứu tập trung không phải vào việc tạo ra bot *mạnh nhất* mà bot *thú vị nhất* hoặc *đa dạng nhất*. Ý tưởng là dùng GA với fitness function khác nhau cho từng "nhân cách" bot — aggressive, defensive, economic, chaotic — thu được một tập bot đa dạng hơn nhiều so với tối ưu hóa đơn thuần theo win rate. Đây chính là hướng mà đề tài này theo đuổi — cách hiện thực hóa cụ thể ý tưởng "nhiều nhân cách bot" được trình bày ở Chương 5.

> **[HÌNH 1.3 — Lịch Sử Ứng Dụng GA Trong Game AI]** *Timeline ngang từ 2002 đến nay: NEAT (2002), Mario AI Competition (2009), Procedural Content Generation (2010s), Game Balancing via GA (2015+), đề tài này (2025). Mỗi mốc kèm biểu tượng game/ứng dụng tiêu biểu.*

---

### 1.3.2 Tại Sao GA Phù Hợp Cho Bài Toán Này

Sau khi đã phân tích hạn chế của RL (mục 1.2.3) và lịch sử GA (1.3.1), có thể thấy rõ lý do GA là lựa chọn phù hợp — không phải vì GA "tốt hơn RL" theo nghĩa tuyệt đối, mà vì bài toán cụ thể này có những đặc điểm ăn khớp với thế mạnh của GA:

**Không cần gradient, không cần differentiability:** Fitness của một chromosome là kết quả trận đấu mô phỏng — một hàm số nguyên không có đạo hàm và không liên tục. GA không yêu cầu bất kỳ tính chất giải tích nào của hàm fitness — nó chỉ cần so sánh được hai giá trị fitness với nhau. Đây là ưu điểm quyết định so với mọi phương pháp dựa trên gradient.

**Tự nhiên hỗ trợ đa nghiệm song song:** Mục tiêu của dự án không phải tìm một bot "tốt nhất" mà tìm nhiều phong cách chơi phân biệt. GA duy trì cả một quần thể cá thể đồng thời thay vì một nghiệm đơn lẻ — đặc tính này cho phép một lần chạy thu được nhiều phong cách chiến lược phân biệt cùng lúc, thay vì phải lặp lại toàn bộ quá trình huấn luyện riêng cho từng phong cách (cơ chế cụ thể được trình bày ở Chương 5).

**Chromosome cho interpretability:** Bởi vì mỗi thành phần trong biểu diễn chiến lược mang một ý nghĩa cụ thể — mức độ ưu tiên cho một loại unit, ngưỡng cho một quyết định kinh tế... — kết quả training không chỉ là một bot, mà là một **mô tả chiến lược có thể đọc và phân tích**. Sau training, ta có thể đặt cạnh nhau hai bot chơi khác nhau và lý giải được sự khác biệt đó bắt nguồn từ đâu trong biểu diễn nội tại của chúng (minh họa cụ thể ở Chương 5). Đây là đóng góp học thuật thực chất mà RL không thể cung cấp trong ngữ cảnh này.

**Tài nguyên vừa phải:** Toàn bộ logic game được thiết kế headless (không cần Unity Editor, không cần GPU), cho phép chạy hàng nghìn trận đấu mô phỏng trên CPU thông thường trong một khoảng thời gian ngắn (số liệu cụ thể ở Chương 4–5) — so với hàng trăm giờ GPU cần thiết cho Deep RL ở quy mô tương tự.

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

Đề tài này không dựa trên bất kỳ hướng nào nêu trên mà chọn một hướng ít được khai thác hơn: **tiến hóa chromosome biểu diễn chiến lược**. Thay vì học từ dữ liệu có sẵn hay tìm kiếm trong không gian game state, hệ thống định nghĩa trước một không gian tham số chiến lược có cấu trúc rồi dùng GA để khám phá không gian đó (cấu trúc cụ thể của không gian này là nội dung trung tâm của Chương 5).

---

### 1.4.2 Unity ML-Agents Và Lý Do Không Áp Dụng

**Unity ML-Agents** là framework chính thức của Unity Technologies, phát hành năm 2017 và liên tục phát triển, cho phép huấn luyện agent AI trong môi trường Unity bằng các thuật toán RL hiện đại (PPO, SAC, MA-POCA cho multi-agent). Framework này được tích hợp sâu vào Unity Editor, hỗ trợ observation vector từ game state, training trên GPU qua Python backend, và export model dưới dạng ONNX để deploy trực tiếp trong game.

Từ góc độ kỹ thuật, ML-Agents là giải pháp hoàn thiện và được hỗ trợ tốt. Tuy nhiên, có ba lý do cụ thể khiến nó không phù hợp với đề tài này:

**Lý do 1 — Mục tiêu là interpretability, không phải raw performance:** ML-Agents tạo ra neural network với hàng chục nghìn trọng số — không thể đọc hay phân tích chiến lược từ các trọng số đó. Mục tiêu của đề tài là không chỉ tạo ra bot tốt mà còn **hiểu được** tại sao bot chơi như vậy. Một biểu diễn chromosome có cấu trúc, nơi mỗi thành phần mang ý nghĩa tường minh (Chương 5), cung cấp điều đó; neural network thì không.

**Lý do 2 — Tài nguyên huấn luyện:** ML-Agents với PPO cần hàng triệu bước để hội tụ cho bài toán có không gian trạng thái lớn như Auto Chess. Điều này đòi hỏi GPU và thời gian training tính bằng giờ đến ngày — không phù hợp với phạm vi dự án. GA với headless simulation hoàn thành trong 20–30 phút trên CPU thông thường.

**Lý do 3 — Thiết kế observation và action space:** Để dùng ML-Agents, cần encode toàn bộ trạng thái game thành observation vector và toàn bộ action space thành action mask — một công việc thiết kế engineering đáng kể và rất nhạy cảm với các quyết định về representation. Mỗi thay đổi nhỏ trong thiết kế game (thêm trigger type mới, thêm bộ tộc) đòi hỏi cập nhật toàn bộ observation/action schema và có thể làm vô hiệu model đã train. GA với chromosome real-valued không có vấn đề này — chromosome chỉ cần mở rộng thêm gene, không cần thiết kế lại từ đầu.

Điều này không có nghĩa ML-Agents là lựa chọn sai trong mọi ngữ cảnh — với đội ngũ lớn hơn, tài nguyên phần cứng đầy đủ và mục tiêu tập trung vào raw performance, ML-Agents sẽ là lựa chọn tốt hơn GA. Nhưng cho đề tài học thuật ở cấp tiểu luận chuyên ngành, GA cung cấp sự cân bằng tốt hơn giữa độ phức tạp triển khai, tài nguyên cần thiết, và giá trị học thuật thu được.

---

### 1.4.3 Nền Tảng Kỹ Thuật — Headless Simulation

Một thách thức kỹ thuật chung cho mọi hướng tiếp cận AI trong game Unity là: làm thế nào để chạy hàng nghìn trận đấu mô phỏng mà không cần mở Unity Editor hay khởi động scene? Vấn đề này thường được gọi là **headless simulation** — khả năng chạy logic game tách biệt khỏi rendering và UI.

Có ba cách tiếp cận phổ biến:

**Unity Batch Mode:** Unity cung cấp flag `-batchmode` để chạy build mà không mở cửa sổ. Tuy nhiên, batch mode vẫn yêu cầu toàn bộ Unity runtime được tải và scene được khởi tạo — không phù hợp cho việc chạy hàng nghìn vòng lặp training trong một tiến trình đơn lẻ vì overhead quá lớn.

**Tách logic thành Plain C#:** Đây là cách tiếp cận của đề tài: các lớp liên quan đến AI và combat logic được viết thuần C# không kế thừa MonoBehaviour, có thể khởi tạo trực tiếp và chạy hàng nghìn lần trong vòng lặp huấn luyện mà không cần mở scene mới hay render khung hình.

**External simulation framework:** Một số dự án tách hoàn toàn logic game ra khỏi Unity, viết lại bằng Python hay C++ thuần để tăng tốc độ simulation. Cách này cho hiệu năng cao nhất nhưng đòi hỏi duy trì đồng thời hai codebase (game Unity và simulation engine bên ngoài) — rủi ro về sự không nhất quán logic.

Đề tài chọn cách thứ hai vì lý do kỹ thuật rõ ràng: cùng một bộ luật combat được dùng cả trong gameplay thực lẫn trong headless training, nên không có nguy cơ phân kỳ (divergence) giữa "logic game thật" và "logic training" — đây là tính chất quan trọng bậc nhất khi đánh giá tính hợp lệ của kết quả training. Cách ranh giới Plain C# / MonoBehaviour này được tổ chức cụ thể trong codebase là nội dung của Chương 4 (mục 4.7).

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

**Đóng góp 1 — Một biểu diễn chromosome bao phủ toàn diện không gian quyết định:**

Không có nghiên cứu nào (theo hiểu biết của tác giả) thiết kế chromosome GA bao phủ đồng thời: đánh giá chỉ số thuần túy, đánh giá ability theo trigger/effect type, nhận diện tribe synergy, hành vi kinh tế (reroll, freeze, sell), và hành vi spell — trong một vector duy nhất và nhất quán, nơi mỗi thành phần mang một ý nghĩa cụ thể, cho phép phân tích kết quả training một cách có chiều sâu (cấu trúc đầy đủ ở Chương 5, mục 5.2).

**Đóng góp 2 — TTE Ability Engine cho phép data-driven card design:**

Mô hình Trigger → Target → Effect, kết hợp hệ thống modifier phong phú, cho phép thiết kế một thư viện thẻ bài lớn với hành vi phức tạp mà không cần viết code riêng cho từng lá (quy mô và cấu trúc cụ thể ở Chương 2 mục 2.3 và Chương 3). Đây là đóng góp về kiến trúc phần mềm có giá trị độc lập với hệ thống AI — bất kỳ dự án card game nào cũng có thể tái sử dụng thiết kế này.

**Đóng góp 3 — Hệ thống training headless hoàn chỉnh, tái hiện được:**

Toàn bộ pipeline training — từ khởi tạo quần thể, mô phỏng trận đấu, đến lưu kết quả dưới dạng JSON — có thể chạy trên máy tính cá nhân thông thường trong một khoảng thời gian ngắn (chi tiết ở Chương 4–5) và tái hiện được (reproducible) vì không phụ thuộc vào cloud hay GPU. Đây là điều kiện quan trọng để kết quả thực nghiệm có thể được kiểm chứng độc lập.

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

### 2.1.1 Bài Toán Tối Ưu Hóa Trong Không Gian Tìm Kiếm Lớn

Nhiều bài toán thiết kế — từ tối ưu hóa kỹ thuật đến cân bằng độ khó game — đều có thể quy về dạng tổng quát: tìm `x* = argmax f(x), x ∈ S ⊆ ℝⁿ`. Khi `S` nhỏ và `f` khả vi, các phương pháp giải tích cổ điển (gradient descent, quy hoạch tuyến tính...) giải quyết hiệu quả. Nhưng có một lớp bài toán mà các phương pháp đó bất lực:

- **Không có biểu thức tường minh cho `f`** — chỉ có thể *đo* giá trị của nó bằng cách thử nghiệm (hàm hộp đen — black-box function)
- **`f` không liên tục hoặc có nhiễu** — kết quả đo dao động giữa các lần thử ngay cả với cùng một đầu vào
- **Không gian `S` chứa rất nhiều cực trị địa phương** — các phương pháp leo đồi (hill-climbing) dễ mắc kẹt ở lời giải tốt-cục-bộ

Bài toán "tìm một chiến lược chơi tốt cho một tác tử AI trong game chiến thuật" là một ví dụ điển hình thuộc lớp này: "độ tốt" của một chiến lược chỉ có thể đo bằng cách *cho nó chơi thật* và quan sát kết quả — không có công thức đóng, không có đạo hàm để đi theo.

**Thuật toán di truyền (Genetic Algorithm — GA)**, do J. Holland đề xuất và được hệ thống hóa bởi D. Goldberg, thuộc họ **thuật toán tối ưu hóa siêu khải (metaheuristic) dựa trên quần thể (population-based)**. Khác với các phương pháp đơn-điểm (đi theo một đường từ một lời giải khởi đầu), GA duy trì đồng thời một *tập hợp* nhiều lời giải, để chúng cạnh tranh, lai ghép và biến đổi qua nhiều thế hệ — mô phỏng nguyên lý chọn lọc tự nhiên ở cấp độ tính toán.

> **[HÌNH 2.1 — Tìm kiếm đơn-điểm và tìm kiếm theo quần thể]** *Hai sơ đồ minh họa trên cùng một bề mặt hàm mục tiêu nhiều cực trị: (trái) một điểm đi theo gradient và mắc kẹt ở cực trị địa phương; (phải) nhiều điểm xuất phát song song, trao đổi thông tin qua các thế hệ, dần hội tụ về vùng cực trị tốt hơn.*

Ba điều kiện khiến GA trở thành lựa chọn phù hợp cho lớp bài toán "tối ưu hóa hành vi của một tác tử trong môi trường mô phỏng": (1) không đòi hỏi đạo hàm — chỉ cần một hàm đánh giá "tốt đến đâu"; (2) bản chất song song theo quần thể cho phép thu được *nhiều lời giải khác biệt* trong cùng một lần chạy, thay vì chỉ một lời giải "tốt nhất duy nhất"; (3) chi phí tính toán phù hợp với hạ tầng vừa phải, không đòi hỏi cụm phần cứng chuyên dụng như phần lớn các phương pháp học sâu.

---

### 2.1.2 Cấu Trúc Chuẩn Của Một Thuật Toán Di Truyền

Một GA chuẩn gồm năm thành phần, mỗi thành phần có nhiều phương án thiết kế khác nhau tùy theo đặc thù bài toán:

**(a) Biểu diễn nghiệm (Encoding).** Mỗi lời giải ứng viên được mã hóa thành một **chromosome** — một chuỗi các **gene**. Ba lược đồ phổ biến:

| Lược đồ | Mô tả | Phù hợp khi |
|---|---|---|
| Nhị phân (binary) | Chuỗi bit 0/1 | Bài toán có bản chất rời rạc, tổ hợp |
| Số thực (real-valued) | Mảng số thực, thường chuẩn hóa về [0,1] hoặc [-1,1] | Tham số biến thiên liên tục, cần giữ "sắc thái" mức độ |
| Hoán vị (permutation) | Một hoán vị của tập phần tử | Bài toán định tuyến/lập lịch (TSP, scheduling) |

Lựa chọn lược đồ ảnh hưởng trực tiếp đến cách thiết kế các toán tử lai ghép/đột biến phía sau — đây là quyết định nền tảng đầu tiên khi áp dụng GA vào một bài toán cụ thể.

**(b) Hàm thích nghi (Fitness Function).** Hàm số gán cho mỗi chromosome một điểm số phản ánh "độ tốt" của lời giải mà nó biểu diễn. Đây là cầu nối *duy nhất* giữa bài toán thực tế và áp lực chọn lọc của thuật toán — thiết kế sai hàm fitness (không phản ánh đúng mục tiêu, hoặc dễ bị "khai thác" theo hướng không mong muốn) sẽ khiến quần thể hội tụ về lời giải "điểm số cao nhưng vô dụng". Đây được xem là một trong những khâu khó nhất khi đưa GA vào bài toán thực tế — thường được gọi là vấn đề **định hình phần thưởng (reward shaping)**.

**(c) Chọn lọc (Selection).** Cơ chế quyết định cá thể nào được quyền sinh sản, dựa trên fitness. Ba phương án phổ biến và đánh đổi của chúng:

| Phương pháp | Cơ chế | Đánh đổi |
|---|---|---|
| Roulette Wheel | Xác suất chọn tỉ lệ thuận với fitness | Dễ bị một cá thể vượt trội ("super-individual") độc chiếm, làm giảm đa dạng |
| Rank Selection | Xếp hạng rồi chọn theo hạng, không theo giá trị tuyệt đối | Giảm áp lực chọn lọc khi chênh lệch fitness giữa các cá thể quá lớn |
| Tournament Selection | Chọn ngẫu nhiên *k* cá thể, lấy cá thể tốt nhất trong nhóm làm cha/mẹ | Cân bằng được giữa áp lực chọn lọc và đa dạng bằng cách điều chỉnh *k* |

**(d) Lai ghép (Crossover).** Toán tử kết hợp vật liệu di truyền của hai cha/mẹ để tạo cá thể con — cơ chế chính giúp GA "kế thừa" đặc điểm tốt thay vì dò tìm lại từ đầu. Với encoding dạng chuỗi (nhị phân hoặc số thực), các biến thể thường gặp là **lai ghép một điểm**, **hai điểm** và **đồng nhất (uniform)** — khác nhau ở số lượng và vị trí các "điểm cắt" phân chia đoạn gene giữa hai cha/mẹ. Một tiêu chí thiết kế quan trọng là **bảo toàn epistasis**: nếu các gene liền kề trong chuỗi cùng quy định một nhóm đặc điểm có tương tác chặt với nhau, toán tử lai ghép nên hạn chế "cắt rời" chúng — nếu không, cá thể con dễ thừa hưởng các mảnh gene không tương thích lẫn nhau.

**(e) Đột biến (Mutation).** Toán tử thay đổi ngẫu nhiên một số gene với xác suất nhỏ — nguồn chính tạo ra vật liệu di truyền *hoàn toàn mới* mà phép lai ghép không thể tạo ra (lai ghép chỉ tổ hợp lại cái đã có sẵn trong quần thể). Với encoding số thực, hai cách tiếp cận phổ biến: **đột biến đều (uniform)** — thay gene bằng một giá trị ngẫu nhiên hoàn toàn mới, tạo bước nhảy lớn và tăng khả năng khám phá; và **đột biến nhiễu Gauss (Gaussian perturbation)** — cộng thêm vào giá trị hiện tại một lượng nhiễu lấy từ phân phối chuẩn N(0, σ), khiến các thay đổi nhỏ phổ biến hơn thay đổi lớn, giúp quá trình hội tụ "mượt" hơn.

> **[HÌNH 2.2 — Vòng lặp tiến hóa tổng quát]** *Flowchart chuẩn của một GA: Khởi tạo quần thể → Đánh giá fitness → Chọn lọc → Lai ghép → Đột biến → Thay thế thế hệ → kiểm tra điều kiện dừng → lặp lại.*

---

### 2.1.3 Hội Tụ Sớm Và Bài Toán Duy Trì Đa Dạng Quần Thể

Nhược điểm cố hữu của các thuật toán theo quần thể là **hội tụ sớm (premature convergence)**: nếu áp lực chọn lọc quá mạnh, quần thể nhanh chóng "đồng nhất hóa" quanh một lời giải tốt-cục-bộ và đánh mất khả năng khám phá các vùng khác của không gian tìm kiếm. Triệu chứng đặc trưng là độ lệch chuẩn (độ đa dạng) của quần thể giảm rất nhanh trong khi fitness trung bình chững lại ở một mức chưa tối ưu toàn cục.

Một số kỹ thuật kinh điển để đối phó với hiện tượng này:

- **Elitism** — luôn sao chép nguyên vẹn một số cá thể tốt nhất sang thế hệ kế tiếp, đảm bảo fitness tốt nhất không bao giờ giảm qua các thế hệ (tính đơn điệu — monotonicity)
- **Niching / Speciation** — chia quần thể thành các "ổ sinh thái" theo độ tương đồng, hạn chế cạnh tranh trực tiếp giữa các nhóm khác biệt để bảo tồn nhiều kiểu hình song song
- **Mô hình đảo (Island Model)** — chạy nhiều quần thể con song song, thỉnh thoảng cho cá thể di cư giữa chúng; mỗi "đảo" có thể hội tụ về một vùng lời giải khác nhau, kết quả cuối cùng là *một tập hợp* nhiều lời giải đa dạng thay vì một lời giải duy nhất
- **Người nhập cư ngẫu nhiên (Random Immigrants)** — định kỳ thay thế một phần quần thể bằng các cá thể khởi tạo hoàn toàn mới, "bơm" thêm vật liệu di truyền mới để chống bão hòa

> **[HÌNH 2.3 — Hội tụ sớm và tiến hóa lành mạnh]** *Hai bộ đường cong đối chiếu qua các thế hệ: độ lệch chuẩn (đa dạng) và best-fitness. Trường hợp hội tụ sớm: đa dạng giảm về gần 0 rất nhanh trong khi best-fitness còn ở mức thấp. Trường hợp lành mạnh: đa dạng giảm chậm và đều, fitness tăng ổn định theo thời gian.*

Việc *khi nào dừng* thuật toán cũng là một quyết định thiết kế đáng cân nhắc: dừng theo số thế hệ cố định (đơn giản nhưng có thể lãng phí hoặc chưa đủ tùy bài toán), hoặc dừng theo **phát hiện bão hòa (plateau detection)** — theo dõi sự biến thiên của độ lệch chuẩn quần thể qua các thế hệ liên tiếp, và dừng khi nó không còn thay đổi đáng kể trong một khoảng thời gian đủ dài.

Những khái niệm trên — encoding, fitness, các toán tử di truyền, và đặc biệt là bộ công cụ chống hội tụ sớm — chính là "bộ từ vựng nền tảng" sẽ được dùng xuyên suốt Chương 5 để mô tả *cách* hệ thống AI của đề tài được xây dựng, thử nghiệm và tinh chỉnh trong thực tế.

---

*[Tiếp theo: Mục 2.2 — Mô hình lập trình game hướng component và mô phỏng headless]*

## 2.2 Mô Hình Lập Trình Game Hướng Component Và Mô Phỏng Headless

### 2.2.1 Kiến Trúc Hướng Thành Phần (Entity – Component)

Phần lớn engine game hiện đại (Unity, Unreal, Godot...) tổ chức đối tượng trong thế giới game theo nguyên lý **composition over inheritance**: thay vì xây một cây phân cấp lớp kế thừa sâu (ví dụ `Card → MeleeCard → TauntMeleeCard → ...`) — vốn nhanh chóng trở nên cứng nhắc và khó mở rộng khi số lượng đặc điểm cần kết hợp tăng lên — mỗi đối tượng trong game (**Entity**) được xem như một "vỏ" gần như rỗng, và *hành vi* được thêm vào bằng cách gắn các **Component** độc lập, mỗi Component phụ trách đúng một khía cạnh (hiển thị, vật lý, tương tác, dữ liệu...).

Lợi ích chính của mô hình này: (1) mỗi Component có thể được phát triển, kiểm thử và tái sử dụng độc lập với phần còn lại; (2) một đối tượng có thể "lắp ráp" hành vi mới chỉ bằng cách thêm/bớt Component, không cần sửa đổi mã nguồn của các phần khác; (3) tránh được hiện tượng "lớp Chúa" (god class) — nơi một lớp duy nhất gánh quá nhiều trách nhiệm và trở thành điểm nghẽn của toàn bộ codebase.

> **[HÌNH 2.4 — Kế thừa sâu và Component hóa]** *Đối chiếu hai sơ đồ: (trái) cây kế thừa sâu dễ "nổ tổ hợp" lớp khi cần kết hợp nhiều đặc điểm khác nhau; (phải) một Entity lắp ráp từ nhiều Component độc lập, mỗi Component phụ trách một khía cạnh hành vi riêng biệt.*

Một hệ quả thiết kế quan trọng đi kèm mô hình này: vì các Component có thể được khởi tạo theo thứ tự không xác định trước, hệ thống cần một **quy ước rõ ràng cho trình tự khởi tạo** (chẳng hạn: tách bạch một giai đoạn "tự đăng ký dịch vụ" diễn ra sớm, và một giai đoạn "sử dụng các dịch vụ đã đăng ký" diễn ra sau) — nếu không, các Component dễ rơi vào tình huống phụ thuộc vào một dịch vụ chưa kịp khởi tạo.

---

### 2.2.2 Singleton — Mẫu Hình Quản Lý Trạng Thái Toàn Cục

**Singleton** là một mẫu thiết kế (design pattern) đảm bảo một lớp chỉ có đúng một thực thể tồn tại trong toàn bộ vòng đời chương trình, đồng thời cung cấp một điểm truy cập toàn cục đến thực thể đó. Trong lập trình game, mẫu hình này thường được áp dụng cho các đối tượng mang tính "trung tâm — nơi việc tồn tại nhiều bản sao đồng thời sẽ gây ra trạng thái mâu thuẫn (ví dụ: bộ điều phối luật chơi, bộ quản lý âm thanh, kho dữ liệu tĩnh dùng chung).

Đánh đổi cần cân nhắc khi sử dụng mẫu hình này: Singleton giúp truy cập tiện lợi từ bất kỳ đâu trong codebase, nhưng đồng thời tạo ra **phụ thuộc ẩn (hidden dependency)** — một đoạn mã có thể âm thầm dựa vào trạng thái toàn cục mà không khai báo tường minh trong giao diện của nó, gây khó khăn cho việc kiểm thử cô lập (unit testing) và việc lần theo luồng dữ liệu khi gỡ lỗi. Một biến thể thường gặp nhằm giảm thiểu rủi ro "tồn tại nhiều bản sao" — tình huống đặc biệt dễ xảy ra khi một đối tượng được giữ lại xuyên suốt nhiều màn chơi — là để bản thân Singleton tự phát hiện và loại bỏ các bản sao thừa ngay tại thời điểm khởi tạo, thay vì đặt niềm tin hoàn toàn vào việc lập trình viên tuân thủ quy ước một cách thủ công.

---

### 2.2.3 Tách Biệt Giữa Tính Toán Và Trình Diễn

Một quyết định kiến trúc có ảnh hưởng sâu rộng trong các game theo lượt (turn-based) hoặc có yếu tố mô phỏng là: **tách bạch lớp tính toán trạng thái (đồng bộ, tất định) khỏi lớp trình diễn (bất đồng bộ, trải dài theo thời gian thực)**. Nói cách khác: trước tiên tính toán *toàn bộ* kết quả của một lượt chơi một cách tức thời và đầy đủ, ghi lại thành một chuỗi sự kiện; sau đó mới "phát lại" tuần tự từng sự kiện cho người chơi quan sát bằng hoạt ảnh, âm thanh, hiệu ứng hình ảnh — thay vì để quá trình hoạt ảnh "điều khiển ngược" luồng tính toán.

> **[HÌNH 2.5 — Hai mô hình tổ chức vòng lặp game]** *Đối chiếu hai sơ đồ: (trái) tính toán và trình diễn đan xen trong cùng một vòng lặp — khó tách rời, khó kiểm thử độc lập; (phải) tính toán hoàn tất trọn vẹn trước, kết quả được lưu thành một "nhật ký hành động" và phát lại với tốc độ trình diễn riêng, độc lập với tốc độ tính toán.*

Lợi ích của cách tách biệt này vượt xa mục đích trình diễn đơn thuần — nó là điều kiện tiên quyết cho ba khả năng thường gặp ở các hệ thống game quy mô lớn: **hệ thống phát lại (replay)** — tái hiện một trận đấu từ nhật ký đã lưu; **kiến trúc server có thẩm quyền (server-authoritative)** trong game nhiều người chơi — nơi server đảm nhận toàn bộ tính toán còn client chỉ trình diễn; và đặc biệt quan trọng với phạm vi đề tài này — khả năng vận hành ở **chế độ không giao diện (headless / batch mode)**: cùng một lõi tính toán có thể được gọi lặp lại hàng nghìn lần liên tiếp mà không cần dựng cảnh hay kết xuất hình ảnh, một yêu cầu then chốt khi muốn dùng chính engine của game làm môi trường huấn luyện cho AI.

Hệ quả kỹ thuật đi kèm là cần phân định rõ **ranh giới giữa phần mã phụ thuộc vào runtime của engine** (cần một "khung cảnh" đang sống để hoạt động, không thể khởi tạo tùy ý ngoài ngữ cảnh đó) và **phần mã thuần logic, độc lập với engine** (có thể khởi tạo và thực thi ở bất kỳ đâu, kể cả bên ngoài một tiến trình game thực sự). Ranh giới này càng được vạch rõ ràng — và càng mang tính **một chiều** (lớp phụ thuộc engine được phép gọi vào lớp thuần logic, nhưng chiều ngược lại bị cấm) — thì phần lõi tính toán càng dễ được tái sử dụng cho các mục đích nằm ngoài một phiên chơi thông thường: huấn luyện AI hàng loạt, kiểm thử tự động, hay mô phỏng cân bằng game trên quy mô lớn.

---

### 2.2.4 Thiết Kế Hướng Dữ Liệu (Data-Driven Design)

**Thiết kế hướng dữ liệu** là triết lý tách nội dung (data — số liệu, cấu hình, định nghĩa hành vi) ra khỏi cỗ máy xử lý nó (engine — phần mã diễn giải và thực thi data đó tại runtime). Thay vì viết một đoạn mã chuyên biệt cho từng đối tượng nội dung (mỗi loại quái, mỗi lá bài, mỗi vũ khí riêng một lớp...), người thiết kế định nghĩa chúng dưới dạng **dữ liệu có cấu trúc** (thường ở định dạng văn bản như JSON/XML, hoặc các định dạng serialize riêng của engine), và một engine xử lý *chung* sẽ "đọc hiểu" cùng thực thi dựa trên dữ liệu đó.

Lợi ích chính của hướng tiếp cận này: (1) **mở rộng nội dung mà không cần biên dịch lại mã nguồn** — thêm một đối tượng nội dung mới đơn thuần là thêm một bản ghi dữ liệu; (2) **tách bạch vai trò** — người thiết kế nội dung không cần biết lập trình để tạo ra hành vi mới, miễn là nằm trong "ngôn ngữ" mà engine đã hỗ trợ; (3) **dễ kiểm tra, theo dõi phiên bản và chia sẻ** — dữ liệu dạng văn bản có thể đọc, so sánh và lưu trữ bằng các công cụ quản lý phiên bản mã nguồn thông thường, không cần công cụ chuyên dụng.

Đánh đổi đi kèm là sự **mất mát một phần an toàn kiểu (type safety)** và khả năng gỡ lỗi trực tiếp bằng công cụ của ngôn ngữ lập trình: lỗi nằm trong dữ liệu (ví dụ một tham chiếu trỏ đến định danh không tồn tại) thường chỉ lộ diện khi engine cố gắng diễn giải nó lúc chạy, chứ không phải tại thời điểm biên dịch. Vì vậy, mức độ "hướng dữ liệu" của một hệ thống luôn là một điểm cân bằng có chủ đích giữa *tính linh hoạt* và *tính an toàn* — không phải một thước đo mà "càng cao càng tốt".

---

*[Tiếp theo: Mục 2.3 — Mô hình thiết kế kỹ năng hướng sự kiện]*

## 2.3 Mô Hình Thiết Kế Kỹ Năng Hướng Sự Kiện (Event-Driven Ability Design)

### 2.3.1 Bài Toán: Mở Rộng Nội Dung Mà Không Bùng Nổ Tổ Hợp Mã Nguồn

Các thể loại game xoay quanh một "thư viện" nội dung lớn — game bài, nhập vai, chiến thuật theo lượt — đều đối mặt với cùng một bài toán cốt lõi: làm sao thiết kế **hàng chục đến hàng trăm kỹ năng/lá bài/vật phẩm**, mỗi cái mang một hành vi riêng biệt, mà không phải viết hàng trăm đoạn mã rời rạc cho từng cái?

Cách tiếp cận ngây thơ — mỗi kỹ năng là một lớp/đoạn mã riêng — vấp phải hai vấn đề khi quy mô nội dung tăng lên: **chi phí phát triển tuyến tính** (mỗi nội dung mới luôn đòi hỏi thêm mã mới, không tận dụng được "đòn bẩy" từ những gì đã xây dựng trước đó) và **chi phí bảo trì tăng phi tuyến** (càng nhiều đoạn mã độc lập tồn tại song song, càng khó đảm bảo chúng phối hợp đúng đắn khi xuất hiện cùng lúc trong một ván đấu).

---

### 2.3.2 Các Hướng Tiếp Cận Phổ Biến

Quan sát cách các game thuộc thể loại bài chiến thuật xử lý bài toán này, có thể nhận diện ba hướng chính — mỗi hướng đứng ở một điểm khác nhau trên trục đánh đổi giữa **biểu cảm** (mỗi kỹ năng có thể độc đáo đến đâu) và **khả năng mở rộng** (thêm một kỹ năng mới tốn bao nhiêu công sức):

| Hướng tiếp cận | Cơ chế | Biểu cảm | Khả năng mở rộng |
|---|---|---|---|
| Mã hóa cứng (hard-coded) | Mỗi kỹ năng là một đoạn mã/lớp riêng biệt | Rất cao — gần như không giới hạn | Thấp — mỗi kỹ năng mới đòi hỏi một lần lập trình |
| Ngôn ngữ kịch bản (scripting/DSL) | Kỹ năng được viết bằng một ngôn ngữ kịch bản nhúng, engine thông dịch và thực thi | Cao | Trung bình — vẫn phải "viết mã" cho từng kỹ năng, dù ở mức trừu tượng cao hơn |
| Khuôn mẫu hướng dữ liệu (data-driven templating) | Kỹ năng được lắp ráp từ một bộ "khối xây dựng" hữu hạn, định nghĩa sẵn trong engine | Bị giới hạn bởi tập khối xây dựng có sẵn | Rất cao — thêm một kỹ năng mới chỉ là tổ hợp dữ liệu, không cần viết mã |

Không hướng tiếp cận nào "đúng tuyệt đối" trong mọi hoàn cảnh — lựa chọn phụ thuộc vào quy mô nội dung dự kiến và nguồn lực của đội ngũ phát triển. Những game có thư viện nội dung rất lớn và cần liên tục mở rộng theo thời gian (game bài số hóa, auto-battler, các tựa game vận hành dài hạn...) thường có xu hướng nghiêng về hướng thứ ba — chấp nhận đánh đổi một phần độ độc đáo để đổi lấy tốc độ và quy mô mở rộng nội dung.

---

### 2.3.3 Khuôn Mẫu Trigger – Target – Effect

Trong số các khuôn mẫu hướng dữ liệu cho hệ thống kỹ năng, có một cấu trúc xuất hiện lặp đi lặp lại — dưới nhiều tên gọi khác nhau — ở rất nhiều tựa game thuộc thể loại bài chiến thuật: phân rã mỗi kỹ năng thành ba trục độc lập:

- **Trigger (Khi nào?)** — điều kiện kích hoạt: một sự kiện xảy ra trong ván đấu (bắt đầu trận, bị tấn công, một lá bài khác vừa chết...)
- **Target (Nhắm vào ai?)** — quy tắc chọn đối tượng chịu tác động (bản thân, một đồng minh ngẫu nhiên, kẻ địch yếu nhất, tất cả các đối tượng thỏa điều kiện...)
- **Effect (Làm gì?)** — hành động được thực thi lên (các) đối tượng đã chọn (cộng chỉ số, gây sát thương, triệu hồi, loại bỏ khỏi trận...)

> **[HÌNH 2.6 — Khuôn mẫu Trigger – Target – Effect]** *Sơ đồ ba trục độc lập; mỗi trục là một tập hữu hạn các lựa chọn được định nghĩa trước; một kỹ năng cụ thể tương ứng với một điểm trong không gian tổ hợp ba chiều này.*

Sức mạnh của khuôn mẫu này nằm ở **tính tổ hợp**: nếu có *m* loại trigger, *n* loại target và *k* loại effect, hệ thống có thể biểu diễn đến *m × n × k* hành vi cơ bản khác nhau chỉ từ ba "bộ từ vựng" hữu hạn — và con số này còn nhân lên đáng kể nếu cho phép gắn thêm các "bộ điều chỉnh" (modifier: giới hạn số lần kích hoạt, điều kiện theo chu kỳ, hiệu ứng tăng dần...) vào mỗi tổ hợp. Nói cách khác: chi phí thiết kế mang tính *cộng tính* (xây dựng từng bộ từ vựng một lần duy nhất), trong khi biểu cảm đạt được mang tính *nhân tính* — đây chính là cơ chế giúp một thư viện nội dung lớn "nở ra" từ một bộ quy tắc nhỏ và nhất quán.

Tuy nhiên, để khuôn mẫu này vận hành đúng đắn trong một môi trường có nhiều kỹ năng tương tác *đồng thời*, người thiết kế hệ thống buộc phải trả lời một số câu hỏi mang tính kiến trúc — những câu hỏi mà bản thân khuôn mẫu **không** tự giải quyết, mà chỉ "phơi bày" ra một cách rõ ràng hơn:

- **Thứ tự**: Khi nhiều kỹ năng cùng thỏa điều kiện kích hoạt tại cùng một thời điểm, chúng được thực thi theo trình tự nào — và trình tự đó có nhất quán, có thể dự đoán được không?
- **Tính đệ quy**: Một effect có thể tự gây ra trigger của chính nó hoặc của một kỹ năng khác đang chờ — làm thế nào để tránh các vòng lặp kích hoạt vô hạn?
- **Đối tượng động**: Nếu một effect làm xuất hiện hoặc loại bỏ đối tượng *ngay trong lúc* hệ thống đang duyệt qua danh sách đối tượng để xử lý sự kiện, điều gì sẽ xảy ra — đối tượng mới có "kịp" nhận sự kiện đang được phát đi hay không?

Đây chính là những "khoảng trống thiết kế" mà bất kỳ hệ thống cụ thể nào áp dụng khuôn mẫu Trigger–Target–Effect cũng buộc phải tự lấp đầy bằng các quy ước riêng của mình. Cách một dự án trả lời ba câu hỏi nêu trên — sẽ được trình bày chi tiết cùng các quyết định thực tế trong Chương 4 — phản ánh khá rõ nét triết lý kỹ thuật và mức độ trưởng thành trong tư duy thiết kế hệ thống của đội ngũ phát triển.

---

*[Tiếp theo: Mục 2.4 — Lý thuyết kinh tế trong game chiến thuật theo lượt]*

## 2.4 Lý Thuyết Kinh Tế Trong Game Chiến Thuật Theo Lượt

### 2.4.1 Bài Toán Quyết Định Tuần Tự Dưới Sự Không Chắc Chắn

Một lớp game chiến thuật theo lượt — đặc biệt là thể loại auto-battler/auto chess — đặt người chơi vào một chuỗi quyết định lặp lại qua từng lượt: quan sát trạng thái hiện tại (tài nguyên, đội hình, các lựa chọn ngẫu nhiên được cung cấp) → chọn một hành động → nhận trạng thái mới, thường kèm theo yếu tố ngẫu nhiên ở bước kế tiếp. Đây là một dạng **bài toán quyết định tuần tự dưới sự không chắc chắn (sequential decision-making under uncertainty)**: lựa chọn tối ưu tại một thời điểm phụ thuộc vào toàn bộ chuỗi trạng thái *tương lai* — điều không thể biết trước do yếu tố ngẫu nhiên xen vào ở mỗi bước.

Vì không thể "giải" bài toán này theo nghĩa tìm ra trước một chuỗi hành động tối ưu tuyệt đối, cách tiếp cận khả thi trong thực tế — cả với người chơi lẫn với một tác tử AI — là xây dựng các **luật ước lượng (heuristics)**: những quy tắc kinh nghiệm giúp đánh giá nhanh "hành động nào có khả năng tốt trong phần lớn tình huống", dựa trên một số khái niệm kinh tế nền tảng được trình bày dưới đây.

---

### 2.4.2 Đánh Đổi Tempo Và Economy

Một khái niệm nền tảng xuất hiện xuyên suốt các game chiến thuật — cả thời gian thực lẫn theo lượt — là sự đánh đổi giữa hai đại lượng:

- **Tempo** — sức mạnh và vị thế *ngay tại thời điểm hiện tại* (đội hình hiện có, khả năng thắng ở lượt sắp tới)
- **Economy** — nguồn lực được tích lũy, đầu tư cho *tương lai* (tài nguyên dự trữ, tốc độ tăng trưởng dài hạn)

Hai đại lượng này luôn ở thế đánh đổi lẫn nhau: chi tiêu ngay để mạnh lên tức thời thường đi kèm việc đánh mất tốc độ tăng trưởng dài hạn, và ngược lại, dồn lực cho tương lai đồng nghĩa chấp nhận yếu thế ở hiện tại. Không tồn tại một "điểm cân bằng tuyệt đối và cố định" — lựa chọn tối ưu phụ thuộc chặt vào *bối cảnh*: một người chơi đang ở thế dẫn đầu có đủ "sức chịu đựng" để theo đuổi một khoản đầu tư dài hạn và chờ nó phát huy tác dụng, trong khi một người chơi đang ở bờ vực thua cuộc buộc phải tối đa hóa sức mạnh tức thời — bởi không chắc còn "tương lai" để hưởng lợi từ khoản đầu tư đó.

> **[HÌNH 2.7 — Đánh đổi Tempo và Economy theo bối cảnh]** *Biểu đồ hai trục: trục hoành biểu diễn mức độ ưu tiên đầu tư dài hạn, trục tung biểu diễn xác suất sống sót/thắng cuộc. Hai đường cong tương ứng với hai bối cảnh khác nhau (đang dẫn đầu / đang nguy cấp) cho thấy điểm lựa chọn tối ưu dịch chuyển theo bối cảnh, không nằm cố định ở một vị trí.*

Vì một chiến lược "tốt" hay "tệ" không thể đánh giá bằng một con số tĩnh mà phải đặt trong *ngữ cảnh trạng thái* tại thời điểm ra quyết định, đây chính là một trong những lý do khiến cả việc thiết kế luật chơi cân bằng (sẽ trình bày ở Chương 3) lẫn việc huấn luyện một AI "biết tùy cơ ứng biến" (Chương 5) đều khó hơn nhiều so với vẻ ngoài đơn giản của chúng.

---

### 2.4.3 Khảo Sát Cơ Chế Kinh Tế Trong Các Game Auto Chess Tiêu Biểu

Thể loại auto chess — định hình qua *Dota Auto Chess* và được phổ biến rộng rãi qua *Teamfight Tactics* (Riot Games), *Dota Underlords* (Valve) hay *Battlegrounds* (Hearthstone, Blizzard) — đã dần hình thành một "không gian thiết kế kinh tế" chung, với một số trục biến thiên mà hầu hết các tựa game trong thể loại đều phải đưa ra lựa chọn riêng:

| Trục thiết kế | Câu hỏi cốt lõi | Ví dụ biến thể thường gặp |
|---|---|---|
| Thu nhập | Tăng tuyến tính theo lượt, hay có cơ chế "lãi kép" theo số dư? | Thu nhập cố định mỗi lượt, hoặc thưởng thêm tỉ lệ với tài nguyên đang nắm giữ |
| Lên cấp / mở khóa | Người chơi trả phí trực tiếp để mạnh lên, hay năng lực tăng tự động theo thời gian? | Mua cấp độ bằng tài nguyên tích lũy, hoặc cấp độ tăng dần tự động theo số lượt trôi qua |
| Quay ngẫu nhiên (reroll) | Có tốn phí hay miễn phí? Phí cố định hay tăng dần theo số lần dùng? | Trả một khoản phí cố định mỗi lần làm mới các lựa chọn được hiển thị |
| Hoàn vốn khi loại bỏ | Hoàn lại đúng giá đã mua, một phần, hay theo một công thức khác? | Cơ chế hoàn vốn ảnh hưởng trực tiếp đến mức độ "thử rồi sửa sai" mà người chơi dám chấp nhận |

Cách mỗi tựa game "vặn" các trục thiết kế này theo những hướng khác nhau tạo ra những bản sắc chiến lược hoàn toàn riêng biệt — chẳng hạn, một cơ chế lãi kép mạnh sẽ khuyến khích lối chơi tích lũy dài hơi và trừng phạt việc tiêu xài bộc phát, trong khi một cơ chế hoàn vốn hào phóng lại khuyến khích thử nghiệm và linh hoạt thay đổi chiến lược giữa ván đấu. Đây chính là "bảng màu" mà một nhà thiết kế game auto chess có thể lựa chọn và phối trộn để tạo nên bản sắc riêng cho sản phẩm của mình — và cũng là bối cảnh nền để Chương 3 trình bày những lựa chọn cụ thể đã được đưa ra cho đề tài này, cùng lý do đằng sau mỗi lựa chọn đó.

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

**Nguyên tắc cốt lõi — tính toán trước, trình diễn sau:** Toàn bộ một lượt chiến đấu được tính toán đầy đủ và tức thời thành một chuỗi sự kiện xác định, sau đó mới được "phát lại" tuần tự cho người chơi quan sát bằng hoạt ảnh (khoảng 0.1 giây/hành động, tổng cộng 5–15 giây mỗi trận). Đây chính là sự áp dụng trực tiếp nguyên tắc tách biệt tính toán và trình diễn đã giới thiệu ở mục 2.2.3 — và là điều cho phép cùng một bộ luật chiến đấu vừa vận hành trong một trận đấu thực sự, vừa chạy lặp lại hàng nghìn lần liên tiếp ở chế độ huấn luyện AI mà không cần viết thêm bất kỳ phiên bản nào khác (trình bày chi tiết ở Chương 4).

Một lượt chiến đấu trải qua ba giai đoạn nối tiếp:
- *Thiết lập:* Toàn bộ unit kích hoạt các kỹ năng "đầu trận"
- *Vòng lặp giao tranh (tối đa 50 round):* Các unit lần lượt tấn công theo hàng đợi (mục 3.6.2), chọn mục tiêu theo thứ tự ưu tiên (mục 3.6.4); mỗi khi có unit gục ngã, toàn bộ chuỗi phản ứng dây chuyền phát sinh từ đó được giải quyết trọn vẹn trước khi giao tranh tiếp tục (mục 3.6.5)
- *Kết thúc:* Một phía bị xóa sổ hoàn toàn → phân định thắng/thua; hết 50 round mà cả hai còn unit → hòa

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
- *Thoth* (Tier 5, Reborn): khi bị tiêu diệt → toàn bộ Niles nhận vĩnh viễn +2 ATK — kể cả những unit được mua *sau* thời điểm đó, nhờ cơ chế ghi nhận buff ở cấp độ toàn bộ tộc (xem mục 3.4.3)

Chiến lược điển hình: *chaos resilience* — đội hình dễ chết nhưng mỗi cái chết kích hoạt chuỗi buff và hồi sinh.

**Olympus — tấn công thuần túy *(thiết kế dự kiến)*:** ATK synergy qua combat events. *Lưu ý: phiên bản hiện tại chưa có unit Olympus; gene[19] (sOlympus) được giữ trong kiến trúc để mở rộng về sau.*

> **[HÌNH 3.7 — So Sánh Ba Triết Lý Tribe Synergy]** *Bảng 3 cột: Babylon/Niles/Olympus theo trigger chính, giai đoạn mạnh nhất, rủi ro, unit tiêu biểu.*

---

### 3.3.3 Passive Keywords

Ba keyword tồn tại từ đầu đến hết combat trừ khi bị tiêu thụ.

**Taunt — buộc phải bị nhắm:** Một unit mang Taunt luôn đứng đầu thứ tự ưu tiên chọn mục tiêu của đối phương — vượt qua mọi quy tắc khoảng cách hay hàng đầu/hàng sau thông thường (xem chi tiết về cơ chế chọn mục tiêu ở mục 3.6.4). Taunt không nhất thiết phải là một thuộc tính bẩm sinh: nó có thể được *trao* cho một unit khác thông qua hiệu ứng buff — biến unit đó thành một "cái bẫy" buộc đối phương phải tấn công đúng nơi người chơi muốn.

**Reborn — hồi sinh một lần:** Khi một unit mang Reborn gục ngã, nó hồi sinh ngay lập tức với đúng 1 HP — và "cơ hội thứ hai" này chỉ tồn tại đúng một lần trong mỗi trận. Điều thú vị về mặt thiết kế nằm ở trình tự: unit vẫn được tính là *đã chết hợp lệ* và kích hoạt trọn vẹn kỹ năng "khi chết" của nó *trước khi* hồi sinh — combo Horus (buff toàn đội khi chết) + Reborn nghĩa là phần thưởng đó kích hoạt hai lần chỉ từ một unit, trong cùng một trận đấu.

**Safeguard — chặn một đòn:** Một lá chắn dùng một lần — vô hiệu hóa hoàn toàn sát thương của đòn tấn công kế tiếp nhắm vào unit mang nó, sau đó tự tiêu biến.

Sự kết hợp giữa các keyword tạo ra những lớp tương tác sâu hơn nhiều so với tổng các phần riêng lẻ: **Taunt + Reborn** buộc đối phương phải tốn trọn hai lượt giao tranh chỉ để hạ gục một unit; **Reborn + kỹ năng "khi chết"** biến một lần gục ngã thành cơ hội kích hoạt hợp lệ một phần thưởng mạnh, trong khi unit vẫn tiếp tục chiến đấu ngay sau đó.

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

Cơ chế trung tâm là một phép quay ngẫu nhiên có trọng số theo bảng tỉ lệ 6×6 dưới đây — mỗi shop level ứng với một phân phối xác suất riêng giữa 6 tier:

| Shop Level | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 | Tier 6 |
|:----------:|:------:|:------:|:------:|:------:|:------:|:------:|
| 1          | 100%   | 0%     | 0%     | 0%     | 0%     | 0%     |
| 2          | 70%    | 30%    | 0%     | 0%     | 0%     | 0%     |
| 3          | 50%    | 35%    | 15%    | 0%     | 0%     | 0%     |
| 4          | 25%    | 40%    | 25%    | 10%    | 0%     | 0%     |
| 5          | 15%    | 25%    | 35%    | 15%    | 10%    | 0%     |
| 6          | 10%    | 15%    | 20%    | 25%    | 20%    | 10%    |

Cơ chế hoạt động đơn giản như một vòng quay số được chia thành các lát cắt theo đúng tỉ lệ trong bảng: hệ thống rút một số ngẫu nhiên, xác định số đó rơi vào "lát cắt" tier nào — tier có tỉ lệ cao hơn chiếm lát cắt rộng hơn, do đó dễ "trúng" hơn một cách tự nhiên — rồi chọn ngẫu nhiên đồng đều một lá bài thuộc đúng tier đó từ kho bài. Nếu vì lý do nào đó không còn lá bài nào thuộc tier vừa quay trúng, hệ thống tự động lùi xuống tier liền kề thấp hơn — đảm bảo shop không bao giờ xuất hiện một ô trống.

**Đọc bảng:** Tier 1 vẫn còn 10% ở shop level 6 — người chơi có thể tìm bản sao merge unit thấp tier ở late-game. Tier 6 chỉ xuất hiện 10% ở shop level tối đa — phần thưởng hiếm cho người chơi sống sót đủ lâu.

---

### 3.4.2 Đóng Băng Shop (Freeze)

Freeze tốn **0 coin** nhưng có chi phí ẩn: từ bỏ quyền xem 7 lá mới đầu lượt tiếp theo. Khi người chơi chọn freeze trước lúc bước vào combat, shop ở đầu lượt kế tiếp sẽ không làm mới toàn bộ — chỉ những ô đã được mua (nay đang trống) mới được lấp đầy bằng card mới theo đúng phân phối tier hiện hành, còn những ô chưa mua vẫn giữ nguyên lá bài cũ. Trạng thái đóng băng này chỉ tồn tại trong đúng một lượt rồi tự động được gỡ bỏ.

Nên freeze khi thấy lá bài cần thiết nhưng chưa đủ coin; không nên freeze khi shop hiện tại không có gì đặc biệt. Reroll trong lượt hiện tại hủy freeze — không thể vừa freeze vừa reroll.

---

### 3.4.3 Buff Toàn Bộ Tộc (Global Tribe Buff)

Một quy tắc thiết kế đơn giản nhưng mang sức ảnh hưởng lâu dài xuyên suốt cả ván đấu: **mọi lá bài mới xuất hiện trong shop đều tự động được cộng toàn bộ phần buff vĩnh viễn theo bộ tộc đã từng được kích hoạt trước đó trong trận** — ngay tại thời điểm nó vừa hiện ra, không cần đợi đến lúc người chơi mua hay triển khai.

Hệ quả thiết kế ở đây khá tinh tế: nếu Thoth đã từng kích hoạt buff +2 ATK cho toàn bộ tộc Niles, thì mọi unit Niles *xuất hiện trong shop kể từ thời điểm đó trở đi* sẽ hiện ra với chỉ số đã được cộng sẵn — người chơi nhìn thấy đúng con số mình sẽ nhận được, không phải tự nhẩm tính thêm. Nói cách khác, dù Thoth có gục ngã rất sớm trong trận, giá trị của nó không hề biến mất: nó đã "khóa" phần thưởng +2 ATK vào mọi unit Niles sẽ xuất hiện và được mua cho đến tận cuối ván đấu.

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

*Wager* — kỳ vọng giá trị: `E = 4p − 2` (dương khi xác suất thắng trận tiếp theo p > 0.5) — một phép đặt cược thuần túy, phần thưởng phụ thuộc vào một sự kiện ngẫu nhiên trong tương lai mà người chơi không kiểm soát hoàn toàn. Đây cũng chính là kiểu giá trị mà mô hình đánh giá bài của hệ thống AI *không thể* nắm bắt được bằng một phép chấm điểm tĩnh trên từng lá riêng lẻ — mục 5.3.3 bàn đến đây như một điểm mù có chủ đích của mô hình, chứ không phải một thiếu sót.

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

Mỗi lần giao tranh — gọi là một **Clash** — diễn ra hoàn toàn đồng thời: bên tấn công và bên phòng thủ cùng nhận sát thương bằng đúng chỉ số tấn công của đối phương, tại cùng một thời điểm (trừ khi Safeguard chặn được một phía). Không tồn tại khái niệm "ai ra đòn trước" trong một Clash, cũng không có yếu tố trượt đòn hay chí mạng — đây là lựa chọn thiết kế có chủ đích nhằm loại bỏ hoàn toàn yếu tố may rủi mang tính thời điểm, chỉ giữ lại lợi thế đến từ chỉ số và kỹ năng. Nhờ vậy, kết quả combat hoàn toàn xác định (deterministic) từ trạng thái hai sân ngay trước khi giao tranh bắt đầu — và đây là điều kiện tiên quyết để một AI có thể "học" để chơi tốt: nếu kết quả phụ thuộc vào yếu tố may rủi không kiểm soát được, không có gì để học cả.

Triggers phụ sau clash: `OnAttack` chỉ kích hoạt nếu attacker **còn sống** sau đòn; `OnTakeDamage` chỉ kích hoạt nếu unit nhận damage **còn sống** sau khi nhận.

---

### 3.6.4 Chọn Mục Tiêu — Ba Tầng Ưu Tiên

Khi đến lượt tấn công, một unit không chọn mục tiêu một cách tùy ý — nó tuân theo một trật tự ưu tiên cố định gồm ba tầng, áp dụng tuần tự:

| Tầng ưu tiên | Áp dụng khi | Mục tiêu được chọn |
|:---:|---|---|
| 1 — Taunt | Phía đối phương còn ít nhất một unit mang Taunt | Bắt buộc nhắm vào unit Taunt gần nhất — **bỏ qua hoàn toàn** ranh giới hàng đầu/hàng sau |
| 2 — Hàng đầu (Frontline) | Không có Taunt nào còn sống | Unit còn sống ở hàng đầu (slot 0–3), tính theo khoảng cách vị trí gần nhất |
| 3 — Hàng sau (Backline) | Toàn bộ hàng đầu đã bị hạ gục | Unit còn sống ở hàng sau (slot 4–6), gần nhất |

Điểm đáng chú ý nhất trong ba tầng này là tầng 1: Taunt có quyền "vượt rào" hoàn toàn — một unit mang Taunt đứng ở hàng sau vẫn sẽ bị nhắm tới đầu tiên, dù toàn bộ hàng đầu phía trước nó vẫn còn nguyên vẹn. Đây chính là cơ sở cho chiến thuật đặt một unit trụ (tank) mang Taunt ở vị trí an toàn phía sau, biến nó thành "lá chắn từ xa" cho cả đội hình — một lớp chiều sâu chiến thuật chỉ có thể nhận ra khi đã hiểu rõ thứ tự ưu tiên này.

---

### 3.6.5 Khi Một Unit Gục Ngã — Chuỗi Phản Ứng Dây Chuyền

Trong một đội hình được xây dựng quanh các kỹ năng phản ứng theo sự kiện — "khi đồng minh chết", "khi hồi sinh", "khi có đồng minh mới xuất hiện"... — một cái chết hiếm khi là một sự kiện đơn lẻ. Nó thường là điểm khởi đầu của cả một **chuỗi phản ứng dây chuyền**: unit A gục ngã → kích hoạt kỹ năng ban Reborn cho đồng minh yếu nhất (Anubis) → đồng minh đó hồi sinh → kích hoạt kỹ năng nhân bội chỉ số (Osiris) ngay trên unit vừa hồi sinh → trong tích tắc, một unit tầm thường trở thành mối đe dọa lớn nhất sân đấu.

Nguyên tắc thiết kế xuyên suốt cho mọi tình huống như vậy chỉ gói gọn trong một câu: **một chuỗi phản ứng, khi đã bắt đầu, luôn được giải quyết trọn vẹn từ đầu đến cuối trước khi trận đấu tiếp tục diễn ra bình thường** — bất kể chuỗi đó dài bao nhiêu bước, hay có bao nhiêu unit cùng gục ngã trong cùng một khoảnh khắc. Nhờ vậy, người chơi luôn quan sát được toàn bộ hệ quả của một cái chết theo đúng trật tự nhân–quả, không bao giờ bị "cắt ngang" nửa chừng bởi một sự kiện không liên quan xen vào.

> **[HÌNH 3.10 — Một chuỗi phản ứng dây chuyền điển hình]** *Sơ đồ dòng thời gian: Unit A gục ngã → kỹ năng "khi đồng minh chết" kích hoạt → Unit B được ban Reborn → B hồi sinh → kỹ năng nhân bội kích hoạt trên B → chuỗi khép lại, trận đấu tiếp tục bình thường.*

Giữ vững được nguyên tắc "giải quyết trọn vẹn trước khi tiếp tục" — trong khi vẫn ngăn chặn được các vòng lặp vô hạn khi nhiều chuỗi phản ứng chồng lấn hoặc tự kích hoạt lẫn nhau (chính là "bài toán đệ quy" đã đặt ra ở mục 2.3.3) — là một trong những thử thách kỹ thuật khó nhất của toàn bộ hệ thống chiến đấu. Cơ chế cụ thể đứng sau nó, cùng câu chuyện về quá trình thử–sai để đi đến lời giải cuối cùng, được trình bày ở Chương 4.

---

### 3.6.6 Reborn Trong Combat — Một Ví Dụ Cụ Thể Về Chuỗi Phản Ứng

Để cụ thể hóa nguyên tắc vừa nêu ở mục 3.6.5, hãy xét trường hợp một unit mang Reborn gục ngã giữa một đội hình Niles đầy đủ thành phần — đây là chuỗi phản ứng dài và giàu kịch tính nhất mà hệ thống có thể tạo ra. Reborn, xét đến cùng, không phải "miễn nhiễm với cái chết" mà là "chết một cách hợp lệ, rồi được trao thêm một cơ hội": unit vẫn kích hoạt trọn vẹn kỹ năng "khi chết" của nó (ví dụ Horus ban buff cho toàn đội) *trước khi* hồi sinh trở lại với đúng 1 HP — và ngay khoảnh khắc hồi sinh ấy lại tiếp tục là một sự kiện mới, có thể kích hoạt kỹ năng của các đồng minh khác (Osiris nhân đôi/ba chỉ số unit vừa hồi sinh, Sobek tự cộng chỉ số khi có đồng minh "xuất hiện" trên sân) — trước khi unit quay trở lại hàng đợi tấn công và tiếp tục chiến đấu với 1 HP.

> **[HÌNH 3.11 — Chuỗi phản ứng khi một unit Reborn gục ngã]** *Timeline ngang minh họa trọn vẹn trình tự: HP về 0 → kỹ năng "khi chết" kích hoạt (Horus) → hồi sinh với 1 HP → kỹ năng "khi hồi sinh" kích hoạt (Osiris) → kỹ năng "khi có đồng minh mới" lan tỏa (Sobek) → unit trở lại hàng đợi, chuỗi khép lại.*

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
Gilgamesh tự cộng thêm chỉ số vĩnh viễn mỗi khi *nhận* một buff vĩnh viễn từ bên ngoài — nhưng xét kỹ thì chính hành động "tự cộng thêm chỉ số vĩnh viễn" đó cũng là một sự kiện cùng loại. Nói cách khác, kỹ năng của Gilgamesh có khả năng tự kích hoạt chính nó — đây là một minh chứng thực tế, ngay trong luật chơi, cho "bài toán đệ quy" đã đặt ra như một câu hỏi kiến trúc mở ở mục 2.3.3 (cách hệ thống ngăn nó trở thành vòng lặp vô hạn được trình bày ở Chương 4). Lever cân bằng nằm ở chính công thức merge đã trình bày tại mục 3.3.4: hệ số chiết giảm 0.7 khiến phần thưởng tích lũy không tăng tuyến tính theo thời gian — dù tích lũy được 50 đơn vị buff, chỉ số thực nhận về chỉ vào khoảng 35. Snowball vẫn xảy ra đúng như kỳ vọng thiết kế, nhưng có trần — đủ để tạo cảm giác "đáng đầu tư" mà không phá vỡ thế cân bằng tổng thể.

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

**Merge Hint** — khung của lá bài nhấp nháy đều đặn, chuyển dần qua lại giữa màu gốc và màu vàng kim (khoảng bốn nhịp mỗi giây), bất cứ khi nào người chơi chỉ còn thiếu đúng một bản sao để hoàn thành một lượt merge. Đây là một tín hiệu thị giác có chủ đích được "căn chỉnh" ở mức tinh tế: đủ liên tục để không bị bỏ lỡ giữa nhịp độ nhanh của Shop Phase, nhưng đủ nhẹ nhàng để không gây cảm giác phiền nhiễu hay thúc ép.

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

# CHƯƠNG 4: HÀNH TRÌNH XÂY DỰNG HỆ THỐNG KỸ THUẬT

Hai chương trước mô tả game *nên* vận hành như thế nào — luật chơi, cảm giác, những lựa chọn thiết kế đứng sau từng cơ chế. Chương này kể câu chuyện về việc bản thiết kế đó đã thực sự *trở thành* một hệ thống chạy được, theo đúng nghĩa đen: không phải một chuyến tham quan qua các lớp đã hoàn thiện, mà là ghi lại những quyết định, những ngã rẽ sai, và những phát hiện dọc đường. Ba ràng buộc đặt ra luật chơi cho kỹ thuật ngay từ ngày đầu (mục 4.1); đáp ứng chúng buộc phải trả lời những câu hỏi mà không một giáo trình thiết kế game nào đặt ra (mục 4.3), và phải vượt qua ít nhất một vấn đề mà trong một thời gian, trông có vẻ không thể giải (mục 4.4).

---

## 4.1 Ba Ràng Buộc Không Thể Thỏa Hiệp

Ngay từ những buổi thảo luận đầu tiên, ba yêu cầu đã được đặt lên bàn — không phải như mong muốn có thể linh hoạt đánh đổi, mà như điều kiện tồn tại: vi phạm bất kỳ điều nào cũng khiến dự án không thể hoàn thành đúng mục tiêu ban đầu.

- **Data-driven:** Mọi tham số nội dung (chỉ số lá bài, giá trị kỹ năng, tỉ lệ rơi bài) phải nằm trong file dữ liệu, không cứng hóa trong code — để có thể chỉnh cân bằng mà không cần biên dịch lại, và để 68 lá bài có thể được mô tả bằng dữ liệu thay vì 68 lớp con.
- **Headless-compatible:** Huấn luyện GA đòi hỏi hàng trăm nghìn trận mô phỏng. Toàn bộ phần "luật chơi" — combat, quyết định mua bán, đánh giá đội hình — phải chạy được mà không cần cửa sổ hiển thị, không cần cảnh dựng sẵn, không cần một khung hình nào được vẽ ra.
- **Phụ thuộc một chiều:** Thành phần ở tầng dưới không được biết đến sự tồn tại của tầng trên. Cỗ máy chiến đấu không được biết giao diện trông ra sao; AI phải có thể dùng cỗ máy đó mà không hề hay biết một giao diện từng tồn tại.

Ba ràng buộc này nghe có vẻ là những phát biểu trừu tượng về "kiến trúc tốt" — nhưng từng cái, khi va chạm với thực tế xây dựng, lại đẻ ra một câu hỏi cụ thể không có câu trả lời sẵn trong sách. Phần còn lại của chương kể lại quá trình tìm câu trả lời cho từng câu hỏi đó.

---

## 4.2 Một Bộ Luật, Hai Cách Vận Hành

Mục 3.2.3 đã giới thiệu nguyên tắc "tính toán trước, trình diễn sau" như một lời hứa: *cơ chế cụ thể đứng sau nó được trình bày ở Chương 4*. Đây là nơi lời hứa đó được thực hiện.

Vấn đề xuất hiện gần như ngay khi việc xây dựng hệ thống chiến đấu bắt đầu: cùng một bộ luật combat phải đồng thời phục vụ hai mục đích hoàn toàn khác nhau. Với người chơi, nó cần diễn ra theo nhịp độ quan sát được — từng đòn đánh, từng cái chết, từng buff phải xuất hiện tuần tự, đủ chậm để hiểu, đủ kịch tính để hồi hộp. Với AI, nó cần điều ngược lại tuyệt đối — chạy nhanh nhất có thể, hàng trăm nghìn lần liên tiếp, không một khung hình nào được vẽ ra. Viết hai cỗ máy combat riêng biệt — một "có hoạt ảnh" và một "headless" — là phương án bị loại ngay từ đầu: chỉ cần hai cỗ máy lệch nhau một chi tiết nhỏ, AI sẽ học cách chơi giỏi một trò chơi *khác* với trò chơi mà con người thực sự trải nghiệm.

Lời giải nằm ở việc tách câu hỏi "luật chơi nói gì sẽ xảy ra" ra khỏi câu hỏi "người xem có cần thấy nó xảy ra hay không". `CombatResolver` — cỗ máy combat duy nhất — tính toán trọn vẹn một lượt giao tranh ngay tức khắc, thành một chuỗi sự kiện rời rạc, có thứ tự, hoàn toàn xác định: ai đánh ai, ai chết, ai hồi sinh, buff nào kích hoạt, theo đúng trình tự nhân-quả. Chuỗi sự kiện đó sau đó được trao cho bên gọi nó — và bên gọi mới là nơi quyết định phải làm gì tiếp theo. Trong một trận đấu thật, `GameManager` ghi chuỗi đó vào một `TurnRecord`, rồi giao cho tầng giao diện "phát lại" tuần tự bằng hoạt ảnh — khoảng 0.1 giây mỗi hành động. Trong huấn luyện, `GameSimulator` truyền `null` thay cho `TurnRecord` — và `CombatResolver`, khi thấy không có nơi nào cần ghi log, đơn giản là bỏ qua toàn bộ bước "viết lại câu chuyện" ấy. Cùng một bộ luật, cùng một kết quả — chỉ khác nhau ở việc có ai đó đang "xem" hay không.

Cách tiếp cận này — vốn là một minh họa cụ thể cho nguyên tắc tách biệt tính toán và trình diễn đã trình bày ở mục 2.2.3 — định hình toàn bộ phần còn lại của kiến trúc. Hệ thống tách thành bốn tầng xếp chồng, cộng thêm một tầng AI nằm tách biệt sang một bên:

> **[HÌNH 4.1 — Hình Dạng Kiến Trúc Khi Áp Dụng Ba Ràng Buộc]** *Sơ đồ 4 tầng xếp dọc (Data ở đáy → Core Engine → Manager → UI), tầng AI nằm tách biệt bên phải, nối thẳng xuống Core Engine — bỏ qua hoàn toàn Manager và UI. Mũi tên phụ thuộc chỉ đi một chiều từ trên xuống. Màu xanh lá đánh dấu mã thuần C#; vàng đánh dấu MonoBehaviour; xám đánh dấu dữ liệu thuần.*

Vị trí của tầng AI là điểm mấu chốt: nó không nằm "trên cùng" hay "dưới cùng" trong tháp — nó đứng tách hẳn ra, với một đường nối duy nhất đi thẳng xuống Core Engine. Đó chính là hiện thân vật lý của ràng buộc headless: AI không cần — và không được phép cần — bất kỳ thứ gì nằm phía trên lớp luật chơi thuần túy.

Ranh giới quan trọng nhất xuyên suốt cả bốn tầng là ranh giới giữa **MonoBehaviour** và **Plain C#**: bất kỳ lớp nào chỉ chứa logic thuần — tính toán, ra quyết định, xử lý dữ liệu — đều là một lớp C# bình thường, có thể tạo ra bằng `new` ở bất cứ đâu, kể cả trong một tiến trình chưa từng mở một cảnh game nào. Chỉ những lớp thực sự cần vòng đời của Unity (`Awake`, `Update`, sự kiện input từ người chơi) mới kế thừa `MonoBehaviour`. Đường ranh giới này không phải một quy ước phong cách — nó là điều kiện vật lý quyết định AI training có thể tồn tại hay không.

---

## 4.3 Khi Kỹ Năng Can Thiệp Lẫn Nhau — Ba Câu Hỏi Không Có Trong Sách Giáo Khoa

Mục 2.3.3 đã đặt ra ba câu hỏi kiến trúc mở khi đem mô hình Trigger-Target-Effect ra khỏi trang giấy: **thứ tự**, **tính đệ quy**, và **đối tượng động**. Không phải vì những khái niệm này khó hiểu về mặt lý thuyết — mà vì không một giáo trình nào buộc người đọc phải trả lời chúng bằng code thực sự chạy được, hàng nghìn lần một phút, không được phép treo máy. Đây là nơi cả ba nhận được câu trả lời cụ thể — từ chính những tình huống mà mục 3.6 đã mô tả ở góc nhìn người chơi.

### 4.3.1 Thứ Tự — Ai Phản Ứng Trước, Ai Phản Ứng Sau?

Một sự kiện đơn lẻ — "một unit vừa chết" — có thể đồng thời là tín hiệu cho *nhiều* kỹ năng khác nhau: kỹ năng "khi chính mình chết" của unit đó, và kỹ năng "khi đồng minh chết" của mọi unit còn sống xung quanh. Engine cần một quy tắc rõ ràng cho việc ai lên tiếng trước.

Câu trả lời được hiện thực hóa bằng hai luồng phân phối tách biệt: một luồng "tự kích hoạt" luôn xử lý phản ứng của chính chủ thể trước tiên, và chỉ sau khi luồng đó hoàn tất, một luồng "phát sự kiện cho đồng đội" mới quét qua toàn bộ đồng minh còn sống — theo đúng thứ tự vị trí trên sân, bỏ qua chính chủ thể, và (theo mặc định) chỉ thông báo cho những unit cùng bộ tộc, trừ khi kỹ năng của unit đó được đánh dấu rõ ràng là "phản ứng với bất kỳ ai" (như trường hợp Anubis ở mục 3.3.2 — phản ứng với cái chết của *bất kỳ* đồng minh nào, không riêng gì Niles).

Thứ tự này không phải một lựa chọn tùy tiện — nó là điều kiện để chuỗi phản ứng Reborn ở mục 3.6.6 diễn ra đúng như mô tả: kỹ năng "khi chết" của Horus phải hoàn tất buff cho toàn đội *trước khi* cơ hội hồi sinh xuất hiện — nếu thứ tự đảo ngược, buff sẽ được tính trên một bộ chỉ số không còn đúng nữa, và toàn bộ chuỗi nhân-quả mà người chơi quan sát được sẽ trở nên vô nghĩa.

### 4.3.2 Tính Đệ Quy — Khi Một Kỹ Năng Tự Kích Hoạt Chính Nó

Mục 3.7.3 đã hé lộ câu chuyện: kỹ năng của Gilgamesh — "tự cộng thêm chỉ số vĩnh viễn mỗi khi nhận một buff vĩnh viễn từ bên ngoài" — trông hoàn toàn vô hại trên giấy. Nó chỉ là một phản ứng đơn giản với một loại sự kiện.

Vấn đề lộ ra khi nhìn kỹ hơn một bước: hành động "tự cộng thêm chỉ số vĩnh viễn" — chính là *bản thân nó* — cũng tạo ra một sự kiện "nhận buff vĩnh viễn". Nếu engine xử lý nó theo đúng quy tắc thông thường, Gilgamesh sẽ phản ứng với chính phản ứng của mình, rồi lại phản ứng với phản ứng đó, không hồi kết — một vòng lặp vô hạn sẽ đóng băng trận đấu ngay lập tức (hoặc, tệ hơn, treo cứng phiên huấn luyện ở trận thứ vài chục nghìn trong đêm).

Lời giải là một "cánh cổng tự khóa": ngay trước khi phát sự kiện "nhận buff vĩnh viễn", engine dựng lên một cờ báo hiệu "đang xử lý sự kiện này rồi — đừng vào nữa", và đảm bảo cờ đó luôn được hạ xuống khi xong việc, kể cả khi có lỗi xảy ra giữa chừng. Trong lúc cờ đang dựng, mọi nỗ lực phát lại đúng loại sự kiện ấy đều bị bỏ qua trong im lặng — chuỗi phản ứng vẫn hoàn tất, Gilgamesh vẫn nhận được phần thưởng của nó, nhưng cánh cửa dẫn ngược về chính nó đã bị khóa lại đúng một lần. Đây chính là "chiếc lever cân bằng nằm trong chính kiến trúc" mà mục 3.7.3 đã nhắc tới như một minh chứng sống cho bài toán đệ quy nêu ở mục 2.3.3 — không phải một nỗi lo lý thuyết, mà là một ràng buộc kỹ thuật có thật, đòi hỏi một giải pháp cụ thể.

### 4.3.3 Đối Tượng Động — Khi Một Đối Tượng Vừa Biến Mất Vừa Phải Được Ghi Nhớ

Sekhmet "nuốt" đồng minh để hấp thụ sức mạnh — và ở đây, engine gặp phải một loại đối tượng kỳ lạ: một unit đã biến mất khỏi sân, nhưng chưa thực sự "kết thúc vai trò" của nó trong trận đấu. Mã định danh của unit bị nuốt được lưu lại trong một danh sách riêng — để nếu chính Sekhmet sau đó gục ngã, toàn bộ những unit ấy có thể được triệu hồi trở lại y hệt như chưa từng biến mất. Engine phải theo dõi một thực thể tồn tại đồng thời ở hai trạng thái: "đã rời khỏi sân" và "vẫn còn nợ một lần xuất hiện".

Một cánh cổng tự khóa thứ hai xuất hiện ngay tại đây: Sekhmet bị chặn hoàn toàn khỏi việc "nuốt" bất kỳ ai trong giai đoạn Shop — cơ chế này được thiết kế như một phần của combat, không phải một cách để âm thầm xóa vĩnh viễn lá bài khỏi tay người chơi chỉ vì Sekhmet đang đứng trên sân.

Một quan sát thú vị nảy sinh từ chính vấn đề này: engine thực ra dùng *hai cấu trúc dữ liệu khác nhau* cho hai "danh sách chờ" trông có vẻ tương tự nhau. Những cái chết đang chờ xử lý xếp vào một ngăn xếp — phần tử vào sau ra trước; còn những lượt triệu hồi đang chờ lại xếp vào một hàng đợi — phần tử vào trước ra trước. Sự khác biệt này không phải ngẫu nhiên: nó là câu trả lời cho chính câu hỏi mà mục tiếp theo xoay quanh.

---

## 4.4 Vấn Đề Khó Nhất — Khi Nhiều Cái Chết Xảy Ra Cùng Lúc

Mục 3.6.5 đã mô tả hiện tượng từ góc nhìn người chơi — một chuỗi phản ứng dây chuyền luôn được giải quyết trọn vẹn trước khi trận đấu tiếp tục — và đã hứa hẹn: *cơ chế cụ thể đứng sau nó, cùng câu chuyện về quá trình thử–sai để đi đến lời giải cuối cùng, được trình bày ở Chương 4*. Đây là câu chuyện đó.

**Cách tiếp cận đầu tiên — và lý do nó thất bại:** Phương án trực giác nhất là xử lý cái chết *ngay khi nó xảy ra* — ngay giữa lúc đang giải quyết một đòn tấn công, gỡ unit khỏi sân, kích hoạt kỹ năng "khi chết" của nó, rồi lập tức phát sự kiện cho đồng đội. Cách này đổ vỡ theo hai hướng cùng lúc. Thứ nhất, việc gỡ một phần tử khỏi danh sách trong khi engine vẫn đang duyệt qua chính danh sách đó để xử lý hàng đợi tấn công làm hỏng toàn bộ vòng lặp đang chạy — một lỗi kinh điển của lập trình. Thứ hai, và nguy hiểm hơn nhiều: khi cái chết của unit A ngay lập tức kéo theo cái chết của unit B, rồi B kéo theo C — và đặc biệt khi một Clash đồng thời (mục 3.6.3) khiến *cả hai* unit cùng gục ngã trong cùng một khoảnh khắc — engine rất dễ đánh mất dấu vết: phản ứng nào đã kích hoạt, phản ứng nào còn đang chờ, không còn gì là chắc chắn nữa.

**Lời giải — trì hoãn thay vì xử lý ngay:** Thay vì xử lý tức khắc, mỗi khi chỉ số máu của một unit chạm 0, nó chỉ đơn giản được *đánh dấu* (để không bao giờ bị đưa vào danh sách chờ hai lần) và xếp vào một ngăn xếp chờ — Death Stack. Lượt giao tranh hiện tại tiếp tục diễn ra bình thường; chỉ sau khi nó kết thúc, một quy trình riêng mới rút cạn toàn bộ ngăn xếp đó, theo đúng thứ tự, từ đầu đến cuối, trước khi bất kỳ điều gì khác được phép xảy ra.

**Vì sao là Ngăn xếp (LIFO) chứ không phải Hàng đợi (FIFO) — lý do thực sự:** Đây là quyết định có chủ đích, không phải ngẫu nhiên. Khi việc giải quyết một cái chết làm phát sinh một cái chết *hoàn toàn mới* ngay giữa chừng — ví dụ Anubis ban Reborn cho một đồng minh, đồng minh đó hồi sinh, kỹ năng nhân bội của Osiris kích hoạt trên nó, và chính hành động đó vô tình khiến một unit thứ ba gục ngã — chuỗi mới phát sinh ấy cần được giải quyết *trọn vẹn* trước khi engine quay lại tiếp tục chuỗi ban đầu. Ngăn xếp đảm bảo đúng tính chất đó một cách tự nhiên: chuỗi được mở ra gần đây nhất luôn là chuỗi được khép lại đầu tiên. Đây chính là điều kiện kỹ thuật đứng sau trải nghiệm mà người chơi quan sát được ở mục 3.6.5 — "luôn thấy được toàn bộ hệ quả của một cái chết theo đúng trật tự nhân–quả, không bao giờ bị cắt ngang giữa chừng".

Quy trình rút ngăn xếp, nhìn từ trên xuống, vận hành theo một chu trình lặp: lấy ra cái chết gần nhất → kích hoạt kỹ năng "khi chết" của chính nó → quét lại toàn bộ sân để phát hiện bất kỳ cái chết mới nào vừa phát sinh (và xếp chúng vào ngăn xếp) → phát sự kiện "khi đồng minh chết" cho từng đồng minh còn sống theo đúng thứ tự đã nêu ở mục 4.3.1 → quét lại lần nữa → lặp lại cho đến khi ngăn xếp trống hoàn toàn. Chỉ sau đó, engine mới chuyển sang bước "dọn sân": với từng unit đã được đánh dấu chết, áp dụng Reborn nếu còn quyền hồi sinh, hoặc gỡ hẳn khỏi sân nếu không. Và chỉ sau khi sân đã sạch, một lượt triệu hồi đang chờ (nếu có) mới được giải phóng — đúng một lượt mỗi lần — để nếu unit vừa xuất hiện lại trở thành nạn nhân của một phản ứng dây chuyền khác (chẳng hạn bị Sekhmet nuốt ngay khi vừa xuất hiện), toàn bộ chu trình phía trên lại được kích hoạt lại từ đầu cho đến khi mọi thứ thực sự lắng xuống. Một bộ đếm an toàn — giới hạn 500 vòng lặp — tồn tại như tuyến phòng thủ cuối cùng: không phải một phần của thiết kế, mà là một tấm lưới bắt lỗi, ghi nhận cảnh báo thay vì để cả trò chơi treo cứng, phòng khi một sự kết hợp kỹ năng nào đó tạo ra một chuỗi mà không ai lường trước được khi thiết kế.

> **[HÌNH 4.2 — Quy Trình Rút Ngăn Xếp Cái Chết]** *Sơ đồ luồng dạng vòng lặp lồng nhau: "lấy cái chết gần nhất" → "kích hoạt phản ứng của chính nó" → "quét cái chết mới" → "phát sự kiện cho đồng minh" → quay lại nếu ngăn xếp còn phần tử; khi trống → "áp Reborn / gỡ khỏi sân" → "giải phóng một lượt triệu hồi đang chờ" → quay lại đầu chu trình nếu có biến động mới. Mũi tên màu đỏ cho nhánh loại bỏ, xanh lam cho nhánh hồi sinh.*

---

## 4.5 Kiểm Chứng Tính Headless — Và Một Dấu Chấm Hỏi Nhỏ

Yêu cầu "headless-compatible" nghe có vẻ trừu tượng — cho đến khi nó va chạm với một tình huống rất cụ thể: khi một kỹ năng kiểu "tấn công thì nhận coin" kích hoạt, engine kỹ năng cần cộng tiền vào ví của người chơi. Trong một trận đấu thật, điều đó có nghĩa là gọi đến `GameManager` — nơi duy nhất nắm giữ trạng thái ví tiền. Nhưng trong một phiên huấn luyện, `GameManager` *không tồn tại* — chưa từng có cảnh nào được dựng lên, chưa từng có singleton nào được khởi tạo. Một lệnh gọi trực tiếp sẽ ném ra lỗi tham chiếu rỗng và làm sập cả tiến trình — có thể là phiên mô phỏng thứ năm mươi nghìn của đêm hôm đó.

Có hai hướng để giải quyết: rải khắp nơi những câu kiểm tra "nếu `GameManager` tồn tại thì mới gọi" (dài dòng, và chỉ cần quên một chỗ là đủ để sập), hoặc — hướng đã được chọn — dựa hẳn vào một toán tử duy nhất. Toán tử điều kiện rỗng biến toàn bộ lệnh gọi thành một hành động "không làm gì cả" trong im lặng khi đối tượng không tồn tại — không nhánh rẽ, không ngoại lệ, không cần xử lý đặc biệt. Một dấu chấm hỏi (`?`) chính là toàn bộ cây cầu nối giữa "đang chạy trong một trận đấu thật" và "đang chạy trong một hộp cát chưa từng được báo cho biết rằng có một trò chơi đang tồn tại".

Trong huấn luyện, kinh tế của bot không hề bị bỏ qua — nó được một hệ thống kinh tế độc lập, thuộc sở hữu riêng của từng `BotAgent`, quản lý hoàn toàn trong một không gian khép kín, tách biệt khỏi mọi trạng thái toàn cục. Cùng một bộ luật chi tiêu chi phối cả người chơi thật lẫn bot — chỉ khác ở chỗ bot không bao giờ cần đến `GameManager` thật để thực thi những luật đó.

---

## 4.6 Từ Mã Nguồn Đến Năm Chiến Binh — Pipeline Huấn Luyện Khép Kín

Mọi quyết định ở các mục trên hội tụ lại thành một quy trình duy nhất, vận hành hoàn toàn tự động từ đầu đến cuối. Một kịch bản dòng lệnh khởi động Unity ở chế độ không giao diện — không cửa sổ, không bộ dựng hình, chỉ thuần xử lý — và gọi vào điểm khởi đầu huấn luyện: nơi `GATrainer` được dựng lên, điều khiển `GameSimulator` chạy qua hàng trăm nghìn trận đấu (432.000), tiến hóa năm quần thể độc lập theo năm triết lý chơi khác biệt, và cuối cùng ghi lại những cá thể sống sót tốt nhất vào một file kết quả. Lần tiếp theo trò chơi thật khởi động, `AIManager` đọc file đó và đưa năm `BotAgent` đã được huấn luyện đầy đủ vào cuộc sống.

> **[HÌNH 4.3 — Pipeline Huấn Luyện Khép Kín]** *Sơ đồ ngang sáu bước: kịch bản khởi động → Unity chế độ không giao diện → điểm vào huấn luyện → [GATrainer điều khiển GameSimulator × 432.000 trận] → file kết quả huấn luyện → AIManager nạp 5 BotAgent vào game thật. Phần headless đặt trong khung xám; phần game runtime tô xanh nhạt.*

Pipeline này chính là khoản "trả lại" cho mọi ràng buộc đã được tôn trọng dọc suốt năm mục trước: chỉ cần một trong số chúng bị phá vỡ — dữ liệu hóa, tính headless, phụ thuộc một chiều, nguyên tắc tính-toán-trước-trình-diễn-sau, hay những cánh cổng tự khóa chống đệ quy — toàn bộ chuỗi này sẽ đứt gãy ở đâu đó, và 432.000 trận đấu sẽ không bao giờ chạy trọn vẹn được.

---

## 4.7 Nhìn Lại Hành Trình

Quay lại ba ràng buộc đặt ra ở mục 4.1 — mỗi ràng buộc, khi đối chiếu với những gì vừa được kể, hóa ra không phải một khẩu hiệu trừu tượng mà là một sợi chỉ xuyên suốt mọi quyết định:

**Data-driven** trở thành hiện thực qua việc định nghĩa lá bài và kỹ năng đều là dữ liệu thuần — không một dòng code riêng nào cho "Anubis" hay "Gilgamesh"; thay đổi một con số trong file dữ liệu là đủ để cân bằng lại cả một bộ tộc.

**Headless-compatible** trở thành hiện thực qua ranh giới rõ ràng giữa mã thuần và mã gắn với engine, qua nguyên tắc tính-toán-trước-trình-diễn-sau biến `TurnRecord` thành một cây cầu tùy chọn chứ không phải một phần bắt buộc của luật chơi, và qua một toán tử nhỏ bé đủ sức ngăn cả hệ thống sụp đổ khi chạy trong một thế giới không có giao diện.

**Phụ thuộc một chiều** trở thành hiện thực qua việc tầng luật chơi không bao giờ "biết" đến sự tồn tại của tầng quản lý hay giao diện — và nhờ vậy, tầng AI có thể đứng tách biệt, dùng đúng những luật chơi mà người chơi thật trải nghiệm, mà không cần venture vào bất kỳ thứ gì nằm phía trên nó.

Nhìn lại, kỹ thuật ở đây chưa bao giờ là một giai đoạn "đến sau" thiết kế — nó là một cuộc thương lượng liên tục giữa điều mà trò chơi *muốn trở thành* và điều có thể thực sự được xây dựng, vận hành, và huấn luyện hàng trăm nghìn lần mỗi đêm. Toàn bộ hành trình ấy — ba ràng buộc, ba câu hỏi kiến trúc, một vấn đề khó nhất, và một pipeline khép kín — tồn tại vì một lý do duy nhất: để "năm chiến binh" nhắc đến ở mục 4.6 có một sân chơi đáng để học. Câu chuyện về việc chúng học như thế nào — và học được gì — được kể tiếp ở Chương 5.

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

### 6.4.1 Huấn Luyện Có Đạt Mục Tiêu "Học Chiến Lược Tốt Hơn Ngẫu Nhiên" Không?

Toàn bộ dữ liệu huấn luyện chi tiết — đường cong hội tụ qua 180 thế hệ chia ba giai đoạn, bảng phân phối tribe, log thực tế từ `training_20260601_213435.csv` — đã được trình bày đầy đủ ở mục 5.7 (HÌNH 5.6, 5.7). Mục này không lặp lại các con số đó; thay vào đó, nó đặt thẳng câu hỏi đánh giá: những con số ấy có thực sự chứng minh hệ thống đạt được mục tiêu thứ hai — "huấn luyện AI học chiến lược tốt hơn ngẫu nhiên, không dùng rule cứng hay Neural Network" — hay không?

Ba dấu hiệu cho thấy câu trả lời là có. **Thứ nhất**, fitness trung bình của *toàn bộ* quần thể — chứ không chỉ cá thể tốt nhất — tăng từ 2.871 ở thế hệ 0 (phần lớn là chromosome khởi tạo gần-ngẫu nhiên) lên ổn định quanh 3.050 từ thế hệ 70 trở đi, một mức cải thiện +6,2% áp dụng cho cả phân phối. Đây đúng là dấu hiệu của "học" theo nghĩa chọn lọc tiến hóa: không chỉ tìm ra một cá thể giỏi may mắn, mà nâng mặt bằng chung lên. **Thứ hai**, composite score của summonerBot và resilientBot — hai archetype được đo bằng tiêu chí riêng, tách biệt khỏi fitness thô — cũng tăng lần lượt 10,2% và 9,7% qua 180 thế hệ (mục 5.7.4), cho thấy cải thiện không chỉ tập trung ở chiến lược "mạnh nhất nói chung" mà lan ra cả những archetype có mục tiêu hẹp hơn. **Thứ ba**, và có lẽ là bằng chứng thuyết phục nhất, quá trình huấn luyện tự phát hiện một sự thật về cân bằng game mà không lập trình viên nào viết sẵn: tribe Niles có lợi thế tự nhiên trong chính trò chơi này (mục 5.7.5) — một insight chỉ có thể nổi lên nếu hệ thống thực sự *khám phá* không gian chiến lược, chứ không tái tạo lại định kiến đã có sẵn của người thiết kế.

Để giữ tính khách quan, cần nói thêm một giới hạn của lập luận trên: "tốt hơn ngẫu nhiên" là một ngưỡng so sánh tương đối thấp — nó chứng minh quá trình chọn lọc *có hướng*, nhưng không tự động chứng minh AI đã chơi *giỏi* theo nghĩa tuyệt đối. Để khẳng định mức độ giỏi tuyệt đối sẽ cần một thước đo nằm ngoài chính hệ thống đang được đánh giá — ví dụ cho bot đấu với người chơi thật, hoặc với một AI rule-based độc lập làm baseline đối chứng — một hướng đánh giá nằm ngoài phạm vi của tiểu luận này (xem thêm mục 6.5, Hạn chế 2).

---

### 6.4.2 Năm Bot — Có Thực Sự Là Năm Phong Cách Khác Biệt?

Mục tiêu thứ ba của dự án — "huấn luyện ra nhiều phong cách chơi khác nhau để tạo trải nghiệm đa dạng" — là mục tiêu khó đánh giá nhất, vì "phong cách chơi" về bản chất là một khái niệm định tính. Bảng đầy đủ giá trị gene, fitness, và cơ sở chọn của 5 bot đã có ở mục 5.1.3 và 5.7.4 (HÌNH 5.8); câu hỏi đánh giá đặt ra ở đây khác: liệu khác biệt *trên giấy* giữa 5 bộ gene có chuyển hóa thành khác biệt *quan sát được* khi chơi thật hay không — và để trả lời thuyết phục, cần đối chiếu cả hai loại bằng chứng độc lập với nhau.

**Bằng chứng định lượng — từ chính cơ chế chọn lọc:** Tiêu chí chọn 5 bot đặt ngưỡng GeneDistance (khoảng cách Euclidean trong không gian 37 chiều) tối thiểu 0,18 giữa các specialist. Trong không gian [0,1]^37, khoảng cách Euclidean tối đa về mặt lý thuyết là √37 ≈ 6,08, nên ngưỡng 0,18 nhìn theo tỉ lệ tuyệt đối có vẻ khiêm tốn. Nhưng giá trị thật của ngưỡng này không nằm ở độ lớn tuyệt đối, mà ở việc nó buộc bước chọn phải *từ chối* những chromosome "tốt nhưng giống nhau" — chính cơ chế đứng sau hiện tượng đáng chú ý đã ghi nhận ở mục 5.7.4: babylonBot và hardBot có cùng fitness tuyệt đối (4.764) nhưng vẫn được hệ thống xem là hai cá thể tách biệt, vì khoảng cách giữa *cách* chúng đạt đến cùng một con số đó đủ lớn để vượt ngưỡng.

**Bằng chứng định tính — từ quan sát chơi thật:** Quan sát hành vi của 5 bot khi vận hành trong game xác nhận sự khác biệt đó là có thật, không chỉ tồn tại như một con số nội bộ:

- *babylonBot* tích lũy chậm nhưng chắc — mỗi lượt cố gắng giữ đủ 3 unit Babylon trên bàn để kích hoạt tribe synergy, để buff HP vĩnh viễn cộng dồn dần thành một khối "tường thành" chỉ số cao theo thời gian. Đối thủ thấy board của nó tương đối ổn định (ít reroll, ít trao đổi unit) nhưng ngày càng khó xuyên phá.
- *summonerBot* chơi gần như đối lập — đội hình nhỏ hơn về số lượng nhưng liên tục "đẻ ra" thêm unit ngay trong lúc combat đang diễn ra, qua chuỗi Summon. Một trận đấu có vẻ tất thắng ở những lượt đầu có thể đảo ngược hoàn toàn khi một loạt unit bất ngờ xuất hiện từ deathrattle chain — một playstyle khó đọc và khó counter hơn hẳn babylonBot.
- *resilientBot* xây dựng board dày đặc Taunt và Reborn, kéo combat dài đến mức đối thủ không thể clear nổi trong 20 lượt. Win condition của nó không phải sát thương dồn dập mà là outlast — trong nhiều trận nó hòa nhiều hơn thắng, nhưng hòa liên tiếp cũng đủ ngăn đối thủ tích lũy cups để giành chiến thắng chung cuộc.

Sự hội tụ giữa hai loại bằng chứng độc lập — một đến từ con số nội tại của thuật toán chọn lọc, một đến từ quan sát hành vi từ bên ngoài hệ thống — là cơ sở vững chắc để kết luận mục tiêu thứ ba đã đạt được theo cách *có thể kiểm chứng*, chứ không chỉ là một tuyên bố thiết kế suông. Quan trọng hơn cả: không một dòng code nào "chèn" sẵn 5 kịch bản hành vi riêng biệt — toàn bộ khác biệt nói trên nổi lên hoàn toàn từ chênh lệch giá trị gene được chọn lọc qua hàng trăm nghìn trận mô phỏng, đúng với nguyên tắc "không có quy tắc cứng" đặt ra ngay từ mục 5.1.1.

---

### 6.4.3 Cơ Chế Đa Dạng Hóa — Có Thực Sự Cần Thiết, Hay Chỉ Là Phức Tạp Hóa Thừa?

Câu hỏi đánh giá đáng đặt ra ở đây không chỉ là "Island Model có hoạt động không", mà còn sắc hơn: nó có thực sự *cần thiết*, hay chỉ thêm độ phức tạp không tương xứng cho một bài toán có thể giải đơn giản hơn? Đầu tư vào elitism per-archetype, immigration thích nghi và seeded sub-population (mục 5.5) chiếm một phần đáng kể công sức triển khai — câu hỏi công bằng là: phần đầu tư đó có "đáng" không?

Dữ liệu phân phối tribe qua 180 thế hệ (HÌNH 5.7, mục 5.7.3) trả lời câu hỏi này một cách thuyết phục — không phải bằng lý thuyết, mà thông qua chính một "phép thử tự nhiên" mà nó vô tình ghi lại được: ở khoảng thế hệ 33–65, chromosome Niles tăng trưởng đến mức chiếm tới 70% quần thể tại đỉnh điểm (thế hệ 45) — không do injection lỗi, mà vì chromosome Niles *thực sự thắng nhiều hơn* trong đấu trường nội bộ, nhờ cơ chế Reborn + OnAllyDeath chain tạo sức mạnh combat tự nhiên cao hơn trong các trận kéo dài. Đây chính xác là kịch bản mà một GA "trần" — không cơ chế bảo vệ diversity — gần như chắc chắn sẽ kết thúc bằng việc xóa sổ hoàn toàn archetype yếu thế hơn khỏi quần thể.

Điều ngăn kịch bản đó xảy ra là elitism per-archetype: hai chromosome Babylon tốt nhất được bảo toàn vô điều kiện mỗi thế hệ, bất kể fitness tương đối của chúng so với phần còn lại của quần thể. Nhờ vậy, ngay giữa lúc Babylon co lại còn 26,7%, vẫn luôn có ứng viên đủ chất lượng để cuối cùng trở thành babylonBot. Std_dev cuối training (~540) so với đầu (952) chỉ giảm 43% — không tiệm cận 0 — là chỉ dấu định lượng cho thấy quần thể hội tụ *có chủ đích*, không rơi vào premature convergence.

Trả lời câu hỏi đặt ra ở đầu mục: phép thử tự nhiên này — Niles tình cờ mạnh hơn Babylon ngay trong chính trò chơi đang được huấn luyện — chứng minh cơ chế đa dạng hóa không phải một lớp phức tạp dư thừa, mà là điều kiện *cần* để đạt được mục tiêu thứ ba. Không có nó, kết quả gần như chắc chắn sẽ là một quần thể đơn sắc thiên Niles, và babylonBot — nếu còn tồn tại — sẽ chỉ là một bản sao gần của hardBot mang vài gene Babylon, chứ không phải một specialist khác biệt thực sự như đã chứng minh ở mục 6.4.2. Bài học rút ra ở đây vượt khỏi phạm vi riêng của game này: trong bất kỳ bài toán tối ưu nào đòi hỏi *nhiều* nghiệm đa dạng — chứ không chỉ một nghiệm tốt nhất — cơ chế bảo toàn đa dạng phải được thiết kế có chủ đích ngay từ đầu, vì một thuật toán tiến hóa "trần" luôn có xu hướng tự nhiên hội tụ về một điểm; "may mắn" không phải một chiến lược đáng tin cậy để chống lại xu hướng đó.

---

## 6.5 Hạn Chế Và Điểm Chưa Hoàn Thiện

Một đánh giá trung thực đòi hỏi nhìn nhận rõ những gì dự án chưa đạt được, không chỉ những gì đã làm được. Có bốn hạn chế đáng kể.

**Hạn chế 1 — Game chỉ là PvE, không có PvP thực sự.** Trong Auto Chess thương mại, trải nghiệm cốt lõi là đấu với nhiều người chơi thật cùng lúc trong một lobby — cạnh tranh không chỉ về combat mà còn về draft (tranh nhau card từ pool chung). Hệ thống này chỉ có một người chơi đối đầu vòng tròn với 5 bot — thiếu yếu tố social và competitive của thể loại. Đây là hạn chế có chủ ý (scope của dự án đặt rõ là single-player), nhưng nó ảnh hưởng đến tính đại diện của game với thể loại Auto Chess.

**Hạn chế 2 — Chromosome real-valued có thể bị stuck trong local optima.** 37 gene thực trong đoạn [0, 1] tạo ra không gian liên tục 37 chiều — về lý thuyết GA có thể tìm nghiệm toàn cục, nhưng thực tế với training 20–30 phút, không có gì đảm bảo nghiệm tìm được không phải local optima. Best fitness đạt đỉnh ở thế hệ 6 và không cải thiện sau đó là dấu hiệu có thể của một local optimum plateau — hoặc cũng có thể là true global optimum với thiết kế game hiện tại. Không có cách phân biệt hai khả năng này mà không có baseline so sánh từ nhiều lần training độc lập.

**Hạn chế 3 — Combat không có visual animation.** Người chơi hiện tại xem combat qua cơ chế "replay từ TurnRecord" — các stat number nhảy số, flash màu đỏ/xanh, unit biến mất khi chết. Đây là thiếu hụt visual đáng kể so với kỳ vọng của người chơi game card hiện đại, nơi combat thường có animation di chuyển, projectile, và hiệu ứng kỹ năng. Giới hạn này xuất phát từ ưu tiên thiết kế: headless combat engine không có khái niệm "vị trí vật lý" hay "animation state" — chỉ có slot index và chỉ số. Để có animation đầy đủ đòi hỏi refactor đáng kể ở cả combat engine lẫn UI layer.

**Hạn chế 4 — Tribe Olympus là dead gene.** Gene `genes[19]` (sOlympus — weight cho tribe Olympus) không có ý nghĩa thực tế vì không có unit nào có `tribe = Olympus` trong bộ 68 card hiện tại. Bot học được rằng gene này vô dụng và đặt giá trị thấp, nhưng đây là lãng phí một gene trong chromosome — không gian tìm kiếm bị "ô nhiễm" bởi một chiều không có tác dụng. Trong phiên bản phát triển tiếp theo, hoặc Olympus cần được implement như một tribe thực sự với unit đầy đủ, hoặc gene cần được loại bỏ khỏi chromosome.

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

Genetic Algorithm, cụ thể hơn, dạy một bài học về sự khiêm tốn của người thiết kế: AI không học theo cách tôi nghĩ nó sẽ học, mà tìm ra những con đường trong không gian gene mà tôi không dự đoán trước. Giai đoạn Niles Domination, cách summonerBot tiến hóa ra genes[27] (proactive sell) = 0,826 — mức cao nhất trong cả năm bot — dù chính công thức `SummonerScore` mà tôi viết để chọn ra nó lại phạt nặng giá trị này (với giả định "summoner cần giữ quân số cho chuỗi triệu hồi"), cách resilientBot độc lập tìm ra tổ hợp Taunt + Reborn là phòng thủ tốt nhất — tất cả những hành vi đó phát sinh từ áp lực selection, không từ instruction của tôi. Đây là điều thú vị và hơi đáng sợ cùng lúc về evolutionary computation: hệ thống tìm ra những giải pháp mà người thiết kế không hình dung được, và đôi khi những giải pháp đó tiết lộ những điều về bài toán mà người thiết kế đã bỏ qua.

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
| 7 | tStartBattle | Trigger | Trọng số trigger StartOfBattle |
| 8 | tOnDeath | Trigger | Trọng số trigger OnDeath (deathrattle) |
| 9 | tOnAttack | Trigger | Trọng số trigger OnAttack |
| 10 | tOnTakeDmg | Trigger | Trọng số trigger OnTakeDamage |
| 11 | tEndTurnShop | Trigger | Trọng số trigger EndTurnShop |
| 12 | tOnDeploy | Trigger | Trọng số trigger OnDeploy |
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
| 7 | tStartBattle | 0.360 | 0.360 | 0.401 | 0.091 | 0.324 |
| 8 | tOnDeath | **1.000** | **1.000** | 0.746 | 0.545 | **1.000** |
| 9 | tOnAttack | **0.000** | **0.000** | **0.983** | **0.812** | **0.000** |
| 10 | tOnTakeDmg | 0.720 | 0.710 | 0.710 | 0.070 | 0.693 |
| 11 | tEndTurnShop | 0.692 | 0.692 | 0.323 | 0.219 | **0.947** |
| 12 | tOnDeploy | 0.594 | 0.578 | 0.174 | 0.553 | 0.540 |
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

Hàm cốt lõi kết nối chromosome với game state, được gọi trong mọi quyết định mua. Trích nguyên văn từ `BotAgent.cs`:

```csharp
private float Evaluate(CardDefinition c)
{
    float s = c.baseATK * brain.genes[0]
            + c.baseHP  * brain.genes[1]
            + (c.tier - 1) * brain.genes[2] * 5f;

    if (c.hasTaunt)     s += brain.genes[4] * 10f;
    if (c.hasReborn)    s += brain.genes[5] * 12f;
    if (c.hasSafeguard) s += brain.genes[6] * 8f;

    if (c.abilities != null)
    {
        int sameTribeCount = 0;
        if (c.tribe != Tribe.None)
            foreach (var u in board)
                if (u != null && !u.IsDead && u.Data.tribe == c.tribe)
                    sameTribeCount++;

        foreach (var a in c.abilities)
        {
            if (a == null) continue;
            float tw = TriggerWeight(a.trigger);
            if (a.trigger == TriggerType.OnAllyDeath ||
                a.trigger == TriggerType.OnAllySummon ||
                a.trigger == TriggerType.OnAllyReborn)
                tw *= Mathf.Clamp01(sameTribeCount / 2f);

            float ew = EffectWeight(a.effect, a.isConsume);
            s += tw * ew * 10f;
            if (a.isEscalating) s += tw * ew * 3f;
        }
    }

    float sw = SynergyWeight(c.tribe);
    if (sw > 0f)
    {
        int same = 0;
        foreach (var u in board)
            if (u != null && !u.IsDead && u.Data.tribe == c.tribe) same++;
        s += same * sw * 4f;
    }

    // Merge proximity — tính cả mergeLevel > 0
    {
        int copies = 0;
        foreach (var u in board)
            if (u != null && !u.IsDead && u.Data.cardID == c.cardID && u.mergeLevel == 0)
                copies++;
        s += copies * brain.genes[21] * (copies == 2 ? 16f : 8f);
    }

    if (c.hasTaunt)
    {
        int emptyFront = 0;
        for (int i = 0; i < FrontlineCount && i < board.Count; i++)
            if (board[i] == null) emptyFront++;
        s += emptyFront * brain.genes[22] * 2f;
    }

    if (c.cost > 0)
        s = s / c.cost * (1f + brain.genes[3]);

    return s;
}
```

### D.2 Hàm Fitness Một Trận — GameSimulator.ScoreFromA()

Phiên bản hiện hành (final) cộng dồn sáu tín hiệu có trọng số khác nhau — kết quả nhị phân, chênh lệch HP, hai cặp tín hiệu cuối trận (`lateScore`, `cardScore`, mỗi cặp gồm thành phần tuyệt đối và thành phần chênh lệch tương đối) và một khoản thưởng nhỏ cho trận kéo dài. Trích nguyên văn từ `GameSimulator.cs`:

```csharp
private static float ScoreFromA(int result, int hpA, int hpB, int turns, float lateScoreA, float lateScoreB, float cardScoreA, float cardScoreB)
{
    float score = result > 0 ? 300f : result == 0 ? 100f : 10f;
    score += hpA * 8f;
    score -= hpB * 4f;
    score += Mathf.Max(0f, lateScoreA) * 0.06f;
    score += Mathf.Clamp(lateScoreA - lateScoreB, -300f, 300f) * 0.04f;
    score += Mathf.Max(0f, cardScoreA) * 0.035f;
    score += Mathf.Clamp(cardScoreA - cardScoreB, -250f, 250f) * 0.025f;
    if (turns >= 12)
        score += (turns - 11) * 2f;
    return Mathf.Max(1f, score);
}
```

### D.3 Tournament Selection Thích Nghi — GATrainer

Kích thước tournament `k` tăng theo tiến trình huấn luyện để tăng dần áp lực chọn lọc; bản thân lựa chọn là vòng đấu loại ngẫu nhiên `k` cá thể, giữ lại cá thể có `fitness` cao nhất. Trích nguyên văn từ `GATrainer.cs`:

```csharp
private static int CurrentTournamentSize(float progress)
{
    if (progress < 0.35f) return 3;
    if (progress < 0.75f) return 4;
    return 5;
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
```

### D.4 Elitism Năm Tầng — GATrainer

Bảo toàn top global và top mỗi archetype riêng (Babylon, Nile, Summoner, Resilient) sang thế hệ kế tiếp bằng cách nhân bản (`Clone()`), không cho lai ghép/đột biến phá vỡ. Trích nguyên văn từ `GATrainer.cs`:

```csharp
var nextGen = new List<Chromosome>();
AddTopClones(nextGen, population, c => true, eliteCount);
AddTopClones(nextGen, population, IsBabylon, 2);
AddTopClones(nextGen, population, IsNile, 2);
AddTopClones(nextGen, population.OrderByDescending(SummonerScore), c => true, 2);
AddTopClones(nextGen, population.OrderByDescending(ResilientScore), c => true, 2);
```

`AddTopClones` chỉ đơn giản lọc theo `predicate`, lấy `count` cá thể đầu (population đã được sắp xếp giảm dần theo `fitness` trước đó) và nhân bản:

```csharp
private static void AddTopClones(List<Chromosome> target, IEnumerable<Chromosome> source, System.Func<Chromosome, bool> predicate, int count)
{
    foreach (var c in source.Where(predicate).Take(count))
        target.Add(c.Clone());
}
```

### D.5 Death Stack — CombatResolver.FlushDeathStack()

Xử lý cái chết theo LIFO (chết sau được xử lý trước, để các phản ứng dây chuyền — VD lính mới triệu hồi bị giết ngay — được giải quyết triệt để trước khi quay lại cấp cha), kèm vòng lặp ngoài tái quét board cho tới khi không còn cái chết hay summon nào đang chờ. Trích nguyên văn từ `CombatResolver.cs`:

```csharp
private void FlushDeathStack(List<CardInstance> pBoard, List<CardInstance> eBoard)
{
    int safetyCounter = 0;
    bool hasMore = true;
    while (hasMore && safetyCounter++ < 500)
    {
        if (safetyCounter == 499)
            Debug.LogError("[CombatResolver] FlushDeathStack hit safety limit (500) — infinite loop detected! Check OnAllySummon/Summon chains.");
        // --- Xử lý toàn bộ death stack (LIFO: death mới nhất được xử lý trước) ---
        while (deathStack.Count > 0)
        {
            DeathEvent evt = deathStack.Pop();

            // OnDeath của unit vừa chết (có thể gây deaths mới)
            engine.TriggerAbility(TriggerType.OnDeath, evt.victim, evt.killer,
                                  evt.victimBoard, evt.killerBoard);
            ScanAllBoardsForNewDeaths(pBoard, eBoard);

            // Broadcast OnAllyDeath cho từng đồng minh còn sống
            // Snapshot list tránh modification khi đang duyệt
            var allySnapshot = new List<CardInstance>(evt.victimBoard);
            foreach (var ally in allySnapshot)
            {
                if (ally == null || ally.IsDead || ally == evt.victim) continue;
                // OnAllyDeath chỉ kích hoạt khi đồng minh CÙNG BỘ TỘC chết,
                // TRỪ KHI ability có anyAllyTrigger = true (VD: Anubis react với bất kỳ ally chết)
                if (ally.Data.tribe != evt.victim.Data.tribe)
                {
                    bool hasAnyTrigger = ally.Data.abilities != null &&
                        ally.Data.abilities.Exists(a => a.trigger == TriggerType.OnAllyDeath && a.anyAllyTrigger);
                    if (!hasAnyTrigger) continue;
                }
                engine.TriggerAbility(TriggerType.OnAllyDeath, ally, evt.victim,
                                      evt.victimBoard, evt.killerBoard);
                ScanAllBoardsForNewDeaths(pBoard, eBoard);
            }
        }

        // --- CleanupBoard: Apply Reborn hoặc xóa unit ---
        // BroadcastAllyEvent(OnAllyReborn) bên trong có thể gây deaths mới
        CleanupBoard(pBoard, eBoard);
        CleanupBoard(eBoard, pBoard);

        // Quét sau cleanup: OnAllyReborn effects có thể kill thêm
        ScanAllBoardsForNewDeaths(pBoard, eBoard);

        // --- Stack-based summon: pop 1 pending summon sau khi board sạch ---
        // Chỉ xử lý khi death stack đã hết — đảm bảo chain của unit trước
        // (NW1 → Mummy → ...) hoàn toàn resolve trước khi NW2 xuất hiện.
        // Nếu NW2 bị Sekhmet nuốt (gây death mới) → vòng lặp ngoài sẽ tiếp tục.
        // Nếu không còn slot → SummonUnit trả null, NW2 biến mất theo đúng luật.
        if (deathStack.Count == 0 && engine.HasPendingSummons)
        {
            engine.ProcessNextPendingSummon();
            ScanAllBoardsForNewDeaths(pBoard, eBoard);
        }

        hasMore = deathStack.Count > 0 || engine.HasPendingSummons;
    }
}
```

`anyAllyTrigger` là cờ trong dữ liệu ability (`CardsData.json`) cho phép một số lá đặc biệt — ví dụ Anubis, Sekhmet — phản ứng với cái chết của *bất kỳ* đồng minh nào, vượt qua giới hạn "cùng bộ tộc" mặc định của `OnAllyDeath`.

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