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

Với bức tranh tổng quan lĩnh vực đã được thiết lập — lịch sử và đặc trưng của thể loại Auto Chess, các hướng tiếp cận AI trong game chiến lược, và lý do GA là lựa chọn phù hợp cho bài toán cụ thể này — chương tiếp theo có thể đi thẳng vào nền tảng lý thuyết mà không cần giải thích ngữ cảnh thêm nữa.

---

*[Tiếp theo: Chương 2 — Cơ Sở Lý Thuyết]*
