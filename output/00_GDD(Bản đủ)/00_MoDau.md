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
