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
