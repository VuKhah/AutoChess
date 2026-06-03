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
