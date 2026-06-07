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
