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

Bốn điểm này hội tụ vào một khoảng trống cụ thể: một nghiên cứu ứng dụng GA vào Auto Chess, với chromosome thiết kế đủ chi tiết để phân tích được hành vi bot, và pipeline training đủ nhẹ để chạy được trên phần cứng học thuật. Đó chính xác là những gì đề tài này thực hiện.

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
