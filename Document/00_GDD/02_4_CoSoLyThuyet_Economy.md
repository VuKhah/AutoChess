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
