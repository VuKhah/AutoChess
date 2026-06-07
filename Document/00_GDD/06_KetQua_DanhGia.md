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
