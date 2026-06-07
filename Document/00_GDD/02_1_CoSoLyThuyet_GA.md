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
