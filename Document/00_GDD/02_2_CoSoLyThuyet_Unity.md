## 2.2 Mô Hình Lập Trình Game Hướng Component Và Mô Phỏng Headless

### 2.2.1 Kiến Trúc Hướng Thành Phần (Entity – Component)

Phần lớn engine game hiện đại (Unity, Unreal, Godot...) tổ chức đối tượng trong thế giới game theo nguyên lý **composition over inheritance**: thay vì xây một cây phân cấp lớp kế thừa sâu (ví dụ `Card → MeleeCard → TauntMeleeCard → ...`) — vốn nhanh chóng trở nên cứng nhắc và khó mở rộng khi số lượng đặc điểm cần kết hợp tăng lên — mỗi đối tượng trong game (**Entity**) được xem như một "vỏ" gần như rỗng, và *hành vi* được thêm vào bằng cách gắn các **Component** độc lập, mỗi Component phụ trách đúng một khía cạnh (hiển thị, vật lý, tương tác, dữ liệu...).

Lợi ích chính của mô hình này: (1) mỗi Component có thể được phát triển, kiểm thử và tái sử dụng độc lập với phần còn lại; (2) một đối tượng có thể "lắp ráp" hành vi mới chỉ bằng cách thêm/bớt Component, không cần sửa đổi mã nguồn của các phần khác; (3) tránh được hiện tượng "lớp Chúa" (god class) — nơi một lớp duy nhất gánh quá nhiều trách nhiệm và trở thành điểm nghẽn của toàn bộ codebase.

> **[HÌNH 2.4 — Kế thừa sâu và Component hóa]** *Đối chiếu hai sơ đồ: (trái) cây kế thừa sâu dễ "nổ tổ hợp" lớp khi cần kết hợp nhiều đặc điểm khác nhau; (phải) một Entity lắp ráp từ nhiều Component độc lập, mỗi Component phụ trách một khía cạnh hành vi riêng biệt.*

Một hệ quả thiết kế quan trọng đi kèm mô hình này: vì các Component có thể được khởi tạo theo thứ tự không xác định trước, hệ thống cần một **quy ước rõ ràng cho trình tự khởi tạo** (chẳng hạn: tách bạch một giai đoạn "tự đăng ký dịch vụ" diễn ra sớm, và một giai đoạn "sử dụng các dịch vụ đã đăng ký" diễn ra sau) — nếu không, các Component dễ rơi vào tình huống phụ thuộc vào một dịch vụ chưa kịp khởi tạo.

---

### 2.2.2 Singleton — Mẫu Hình Quản Lý Trạng Thái Toàn Cục

**Singleton** là một mẫu thiết kế (design pattern) đảm bảo một lớp chỉ có đúng một thực thể tồn tại trong toàn bộ vòng đời chương trình, đồng thời cung cấp một điểm truy cập toàn cục đến thực thể đó. Trong lập trình game, mẫu hình này thường được áp dụng cho các đối tượng mang tính "trung tâm — nơi việc tồn tại nhiều bản sao đồng thời sẽ gây ra trạng thái mâu thuẫn (ví dụ: bộ điều phối luật chơi, bộ quản lý âm thanh, kho dữ liệu tĩnh dùng chung).

Đánh đổi cần cân nhắc khi sử dụng mẫu hình này: Singleton giúp truy cập tiện lợi từ bất kỳ đâu trong codebase, nhưng đồng thời tạo ra **phụ thuộc ẩn (hidden dependency)** — một đoạn mã có thể âm thầm dựa vào trạng thái toàn cục mà không khai báo tường minh trong giao diện của nó, gây khó khăn cho việc kiểm thử cô lập (unit testing) và việc lần theo luồng dữ liệu khi gỡ lỗi. Một biến thể thường gặp nhằm giảm thiểu rủi ro "tồn tại nhiều bản sao" — tình huống đặc biệt dễ xảy ra khi một đối tượng được giữ lại xuyên suốt nhiều màn chơi — là để bản thân Singleton tự phát hiện và loại bỏ các bản sao thừa ngay tại thời điểm khởi tạo, thay vì đặt niềm tin hoàn toàn vào việc lập trình viên tuân thủ quy ước một cách thủ công.

---

### 2.2.3 Tách Biệt Giữa Tính Toán Và Trình Diễn

Một quyết định kiến trúc có ảnh hưởng sâu rộng trong các game theo lượt (turn-based) hoặc có yếu tố mô phỏng là: **tách bạch lớp tính toán trạng thái (đồng bộ, tất định) khỏi lớp trình diễn (bất đồng bộ, trải dài theo thời gian thực)**. Nói cách khác: trước tiên tính toán *toàn bộ* kết quả của một lượt chơi một cách tức thời và đầy đủ, ghi lại thành một chuỗi sự kiện; sau đó mới "phát lại" tuần tự từng sự kiện cho người chơi quan sát bằng hoạt ảnh, âm thanh, hiệu ứng hình ảnh — thay vì để quá trình hoạt ảnh "điều khiển ngược" luồng tính toán.

> **[HÌNH 2.5 — Hai mô hình tổ chức vòng lặp game]** *Đối chiếu hai sơ đồ: (trái) tính toán và trình diễn đan xen trong cùng một vòng lặp — khó tách rời, khó kiểm thử độc lập; (phải) tính toán hoàn tất trọn vẹn trước, kết quả được lưu thành một "nhật ký hành động" và phát lại với tốc độ trình diễn riêng, độc lập với tốc độ tính toán.*

Lợi ích của cách tách biệt này vượt xa mục đích trình diễn đơn thuần — nó là điều kiện tiên quyết cho ba khả năng thường gặp ở các hệ thống game quy mô lớn: **hệ thống phát lại (replay)** — tái hiện một trận đấu từ nhật ký đã lưu; **kiến trúc server có thẩm quyền (server-authoritative)** trong game nhiều người chơi — nơi server đảm nhận toàn bộ tính toán còn client chỉ trình diễn; và đặc biệt quan trọng với phạm vi đề tài này — khả năng vận hành ở **chế độ không giao diện (headless / batch mode)**: cùng một lõi tính toán có thể được gọi lặp lại hàng nghìn lần liên tiếp mà không cần dựng cảnh hay kết xuất hình ảnh, một yêu cầu then chốt khi muốn dùng chính engine của game làm môi trường huấn luyện cho AI.

Hệ quả kỹ thuật đi kèm là cần phân định rõ **ranh giới giữa phần mã phụ thuộc vào runtime của engine** (cần một "khung cảnh" đang sống để hoạt động, không thể khởi tạo tùy ý ngoài ngữ cảnh đó) và **phần mã thuần logic, độc lập với engine** (có thể khởi tạo và thực thi ở bất kỳ đâu, kể cả bên ngoài một tiến trình game thực sự). Ranh giới này càng được vạch rõ ràng — và càng mang tính **một chiều** (lớp phụ thuộc engine được phép gọi vào lớp thuần logic, nhưng chiều ngược lại bị cấm) — thì phần lõi tính toán càng dễ được tái sử dụng cho các mục đích nằm ngoài một phiên chơi thông thường: huấn luyện AI hàng loạt, kiểm thử tự động, hay mô phỏng cân bằng game trên quy mô lớn.

---

### 2.2.4 Thiết Kế Hướng Dữ Liệu (Data-Driven Design)

**Thiết kế hướng dữ liệu** là triết lý tách nội dung (data — số liệu, cấu hình, định nghĩa hành vi) ra khỏi cỗ máy xử lý nó (engine — phần mã diễn giải và thực thi data đó tại runtime). Thay vì viết một đoạn mã chuyên biệt cho từng đối tượng nội dung (mỗi loại quái, mỗi lá bài, mỗi vũ khí riêng một lớp...), người thiết kế định nghĩa chúng dưới dạng **dữ liệu có cấu trúc** (thường ở định dạng văn bản như JSON/XML, hoặc các định dạng serialize riêng của engine), và một engine xử lý *chung* sẽ "đọc hiểu" cùng thực thi dựa trên dữ liệu đó.

Lợi ích chính của hướng tiếp cận này: (1) **mở rộng nội dung mà không cần biên dịch lại mã nguồn** — thêm một đối tượng nội dung mới đơn thuần là thêm một bản ghi dữ liệu; (2) **tách bạch vai trò** — người thiết kế nội dung không cần biết lập trình để tạo ra hành vi mới, miễn là nằm trong "ngôn ngữ" mà engine đã hỗ trợ; (3) **dễ kiểm tra, theo dõi phiên bản và chia sẻ** — dữ liệu dạng văn bản có thể đọc, so sánh và lưu trữ bằng các công cụ quản lý phiên bản mã nguồn thông thường, không cần công cụ chuyên dụng.

Đánh đổi đi kèm là sự **mất mát một phần an toàn kiểu (type safety)** và khả năng gỡ lỗi trực tiếp bằng công cụ của ngôn ngữ lập trình: lỗi nằm trong dữ liệu (ví dụ một tham chiếu trỏ đến định danh không tồn tại) thường chỉ lộ diện khi engine cố gắng diễn giải nó lúc chạy, chứ không phải tại thời điểm biên dịch. Vì vậy, mức độ "hướng dữ liệu" của một hệ thống luôn là một điểm cân bằng có chủ đích giữa *tính linh hoạt* và *tính an toàn* — không phải một thước đo mà "càng cao càng tốt".

---

*[Tiếp theo: Mục 2.3 — Mô hình thiết kế kỹ năng hướng sự kiện]*
