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

### 6.4.1 Kết Quả Training — Dữ Liệu Từ 200 Thế Hệ

Lần training được ghi nhận trong báo cáo này sử dụng production mode với 320 cá thể, tối đa 200 thế hệ, và 32 trận đấu mỗi chromosome mỗi thế hệ. Dữ liệu được đọc từ file `training_20260604_221518.csv` — log thực tế từ lần chạy training trên máy phát triển, bao gồm warm-start từ library cũ.

Toàn bộ quá trình 200 thế hệ chia thành ba giai đoạn, với đặc trưng khác biệt so với training không có warm-start:

**Giai đoạn 1 — Khởi đầu cao, avg hội tụ nhanh (Thế hệ 0–40):** Nhờ warm-start, best fitness bắt đầu ở mức 19.392 — không phải từ 0. Avg tăng mạnh chỉ trong 5 thế hệ đầu từ 8.553 lên 10.056 (+17,6%), phản ánh toàn quần thể đang leo nhanh về vùng fitness tốt. Diversity_index tăng từ 89,7 lên ~96–99.

| Thế hệ | Best | Avg | Std Dev | Babylon% | Niles% | Diversity |
|:------:|:----:|:---:|:-------:|:--------:|:------:|:---------:|
| 0 | **19.392** | 8.553 | 3.407 | 31,3% | 52,8% | 89,7 |
| 5 | 19.392 | 10.056 | 2.705 | 32,8% | 44,7% | 96,3 |
| 10 | 19.392 | 10.256 | 2.419 | 40,3% | 38,8% | 96,5 |
| 40 | 19.392 | 10.590 | 2.900 | 44,1% | 23,8% | 96,9 |

**Giai đoạn 2 — Other tribe nổi lên, best cải thiện (Thế hệ 40–150):** Với scoring mới ưu tiên thắng/thua, chromosome generalist (không bias tribe) có lợi thế ổn định trong đấu trường đa dạng. "Other" tribe (Olympus/mixed) tăng lên chiếm 46–70%. Best fitness cải thiện một lần duy nhất ở gen 116 (19.392 → **20.275**, +4,55%).

| Thế hệ | Best | Other% | Diversity | Ghi chú |
|:------:|:----:|:------:|:---------:|---------|
| 60 | 19.392 | 46,6% | 96,0 | Other bắt đầu nổi lên |
| 100 | 19.392 | **61,3%** | 82,4 | Other domination |
| 116 | **20.275** | — | — | Best cải thiện duy nhất |
| 150 | 20.275 | **69,7%** | 70,1 | Đỉnh Other domination |

**Giai đoạn 3 — Diversity phục hồi, ổn định (Thế hệ 150–199):** Immigration tỉ lệ động tự động cân bằng lại khi Other quá chiếm ưu thế. Đến gen 199, diversity_index đạt **99,8** — gần hoàn hảo (35% Babylon / 30% Niles / 35% Other). Avg ổn định quanh 10.700–10.900, std_dev giảm xuống 2.475 (-27% so với gen 0).

> **[HÌNH 6.3 — Đường Cong Fitness Qua 200 Thế Hệ]** *Biểu đồ đường: trục hoành thế hệ 0–199, trục tung fitness. Ba đường: Best (đỏ — bắt đầu cao nhờ warm-start, cải thiện tại gen 116), Avg (xanh lam — tăng liên tục), Diversity Index (tím — trục phụ bên phải, tăng từ 89,7 lên 99,8). Đánh dấu gen 116 (best cải thiện) và vùng gen 100–150 (Other dominance).*

---

### 6.4.2 Năm Bot Chuyên Biệt — Từ Số Liệu Đến Hành Vi

Kết thúc training, 5 bot được chọn với kết quả định lượng như sau:

| Bot | Fitness (thang mới) | Điểm đặc trưng | Cơ sở chọn |
|-----|:-------------------:|----------------|------------|
| **hardBot** | **21.233** | Cân bằng mọi khía cạnh | Hall of Fame (fitness cao nhất *bao giờ*) |
| **babylonBot** | 13.812 | genes[18] Babylon dominant | Best Babylon, cách xa hardBot ≥ 0,18 |
| **nileBot** | 13.575 | genes[20] Niles dominant | Best Niles, cách xa 2 bot trước |
| **summonerBot** | 11.178 | SummonerScore cao nhất | Viable (≥ ngưỡng sàn), score cao nhất |
| **resilientBot** | 12.320 | ResilientScore cao nhất | Viable (≥ ngưỡng sàn), score cao nhất |

*Lưu ý quan trọng: Fitness trong thang mới (Win base = 300) không so sánh được trực tiếp với thang cũ (Win base = 120). Giá trị tuyệt đối thấp hơn không có nghĩa bot yếu hơn — thang đo đã thay đổi.*

Điều đáng chú ý nhất: hardBot có fitness cao hơn babylonBot/nileBot rõ rệt (21.233 vs ~13.600). Với scoring mới ưu tiên thắng/thua tuyệt đối (Win premium 290 điểm), chromosome generalist của hardBot có lợi thế trước các specialist — nó không có điểm yếu rõ ràng trước bất kỳ playstyle nào. Các specialist bot "hy sinh" fitness tổng để chuyên sâu vào archetype, nhưng có thể thắng áp đảo khi gặp đúng match-up.

summonerBot và resilientBot được chọn qua composite score dựa trên profile gene đặc trưng (không phải fitness thô). Ngưỡng chọn được nâng lên: chromosome phải đạt ít nhất `max(avgFinal × 0.8, hardFitness × 0.55)` để qualify — đảm bảo ngay cả specialist bot cũng có chất lượng tối thiểu so với hardBot.

> **[HÌNH 6.4 — Radar Chart 5 Bot Được Chọn]** *Biểu đồ radar 8 trục (rút gọn từ 9 nhóm gene): Stat Weight (genes 0–1), Keywords (4–6), Trigger (7–12), Effect (13–17), Tribe (18–20), Board Context (21–23), Reroll (24–27), Spell (28–31). Giá trị mỗi trục là trung bình nhóm gene tương ứng. Năm đường màu khác nhau cho 5 bot. hardBot gần đều tất cả các trục; babylonBot spike ở Tribe; summonerBot spike ở Trigger và Effect; resilientBot spike ở Keywords.*

---

### 6.4.3 Phân Tích Diversity — Island Model Có Hoạt Động Không?

Một câu hỏi quan trọng khi đánh giá GA là: cơ chế đa dạng hóa (Island Model + Immigration động) có thực sự ngăn premature convergence không? Dữ liệu training cung cấp câu trả lời rõ ràng — và đáng ngạc nhiên hơn kỳ vọng.

Nếu premature convergence xảy ra, diversity_index sẽ giảm về gần 0 và quần thể trở thành clone của chromosome tốt nhất. Điều ngược lại đã xảy ra: diversity_index *tăng* từ 89,7 (gen 0) lên **99,8 (gen 199)** — quần thể kết thúc training cân bằng hơn lúc bắt đầu, dù đã trải qua giai đoạn Other tribe chiếm tới 70%.

Cơ chế `SelectImmigrantGroup()` đã phát huy hiệu quả: khi Other tăng lên 69,7% ở gen 150 (Babylon chỉ 17,5%, Niles chỉ 12,8%), immigration tự động tăng immigrant Babylon và Niles vào thế hệ tiếp theo. Đến gen 199, ba tribe phân bố 35%/30%/35% — gần hoàn hảo. Babylon không bao giờ bị xóa sổ nhờ kết hợp elitism per-archetype (bảo toàn top-2 Babylon mỗi thế hệ) và immigration tỉ lệ động.

Std_dev giảm từ 3.407 (gen 0) xuống 2.475 (gen 199), giảm 27% — ít hơn nhiều so với training cũ (-43%). Nguyên nhân: với quần thể 320 người và warm-start, fitness landscape đa dạng hơn từ đầu — GA không cần phải "dọn dẹp" nhiều chromosome yếu như training từ đầu.

Kết luận: Island Model kết hợp immigration tỉ lệ động không chỉ ngăn premature convergence mà còn *chủ động tái tạo* sự đa dạng khi bị mất — đây là cải thiện đáng kể so với phiên bản hardcoded trước đó.

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

**Hạn chế 5 — Bot không nhận biết được bonus chỉ số battle-phase.** Hàm đánh giá của BotAgent (`Evaluate()`, `EvaluateInstance()`) chỉ dùng `currentATK`/`currentHP` — là chỉ số *trước khi combat bắt đầu*. Các bonus chỉ xuất hiện trong trận như `StartOfBattle + AddStats` (buff đầu trận), `Aura` (buff liên tục cho đồng minh mỗi round), hay `GiveStats`/`ScaleTargetStats` (nhân chỉ số trong combat) đều vô hình với bot. Bot xử lý những ability này qua `TriggerWeight × EffectWeight × 10` — là điểm ước lượng trừu tượng, không phải con số thực tế. Hệ quả: bot có thể undervalue unit có Aura mạnh (nhìn chỉ số thấp nhưng thực tế buff cả board trong trận), và công thức chia điểm theo cost càng làm unit đắt tiền nhưng có Aura khó được mua hơn. Khắc phục đòi hỏi thêm một lớp "pre-battle stat estimation" vào hàm đánh giá — nằm ngoài phạm vi dự án này.

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

Phiên bản fitness hiện tại thưởng bốn thành phần: kết quả trận (**300/100/10** cho thắng/hòa/thua — win premium 290 điểm), biên thắng (`hpA×8 − hpB×4`), board power phụ (`lateScore × 0.06` và `cardScore × 0.035` — tie-breaker), và bonus late game (`(turns−11)×2` nếu turns ≥ 12). So với phiên bản trước (120/70/35, win premium 85), công thức mới *đặt ưu tiên thắng trận là tuyệt đối* — bot không còn được "bù điểm" nhiều bằng board power nếu thua trận.

Sự thay đổi này có ảnh hưởng quan sát được qua training data: với scoring cũ (Win=120), Niles chiếm 70% quần thể ở mid-training — phản ánh tribe Niles mạnh nhất theo thang đo board power. Với scoring mới (Win=300), "Other" tribe (generalist) chiếm ưu thế — phản ánh chromosome không bias tribe có nhiều cơ hội thắng ổn định hơn khi không có lợi thế board power cụ thể nào được thưởng lớn.

Bài học: fitness function không chỉ đo "bot nào tốt" mà còn *định hướng GA tìm kiếm ở đâu* trong không gian gene. Thay đổi nhỏ trong thang điểm thắng-thua có thể thay đổi hoàn toàn tribe nào được chọn lọc tự nhiên — là lý do cần iterate và quan sát training behavior, không chỉ đặt công thức một lần duy nhất.

### 6.6.3 Island Model — Đầu Tư Complexity Có Giá Trị

Khi nhìn lại, cơ chế Island Model (5 sub-population seeded + elitism per-archetype + immigration) là phần tốn nhiều công implement nhất nhưng cũng là phần tạo ra giá trị lớn nhất.

Không có Island Model, GA gần như chắc chắn sẽ hội tụ về một hoặc hai archetype tốt nhất và loại bỏ phần còn lại. Giai đoạn Niles Domination (thế hệ 33–65) là ví dụ cụ thể: nếu không có elitism per-archetype bảo vệ Babylon, tribe này có thể biến mất hoàn toàn khỏi quần thể. Mà không có Babylon trong quần thể, babylonBot cuối cùng sẽ không tồn tại — hoặc tồn tại nhưng chỉ là hardBot với một vài gene Babylon, không phải một specialist thực sự.

Bài học tổng quát: trong các bài toán tối ưu đòi hỏi nhiều nghiệm *đa dạng* (không chỉ nghiệm tốt nhất đơn lẻ), đầu tư vào cơ chế diversity preservation là cần thiết — không phải tùy chọn. GA standard không tự nhiên tạo ra diversity; phải được thiết kế có chủ đích.

---

*[Kết thúc Chương 6 — Tiếp theo: Kết Luận]*
