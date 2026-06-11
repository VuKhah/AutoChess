# Danh Mục Hình Minh Họa — Chương 2 → Chương 6 + Phụ Lục

Tài liệu này liệt kê toàn bộ hình minh họa (`[HÌNH X.Y — ...]`) xuất hiện trong
các chương 2–6 của tiểu luận, phân loại theo cách tạo ra chúng, và — với các sơ
đồ/biểu đồ khái niệm — kèm một **spec kỹ thuật chi tiết** dùng làm tài liệu để
viết trực tiếp code matplotlib/Python render ra ảnh PNG.

> **Phụ Lục (`08_PhuLuc.md`)**: đã rà soát — không có placeholder `[HÌNH ...]`
> nào trong phần Phụ Lục. Mục này được liệt kê cho đầy đủ nhưng không có nội
> dung cần xử lý.

---

## 1. Quy ước chung

**Vị trí lưu trữ ảnh:** `Document/03_Figures/hinh_X_Y.png`
**Tool sinh ảnh:** `Document/04_Tools/generate_figures_ch*.py`
**Bảng màu dùng chung** (đã thiết lập trong `generate_figures_ch2.py`):

| Hằng số | Mã màu | Dùng cho |
|---|---|---|
| `BG` | `#FAFAFA` | nền figure/axes |
| `C_GRID` | `#dddddd` | lưới, đường phụ |
| `C_BLUE` | `#1565C0` | hộp/đường "tính toán", "GA chuẩn", trục chính |
| `C_RED` | `#C62828` | hộp/đường cảnh báo, nhánh "lá", trạng thái xấu |
| `C_ORANGE` | `#EF6C00` | nút quyết định, thành phần trung gian quan trọng |
| `C_GREEN` | `#2E7D32` | trạng thái khởi tạo/thành công, nhánh tối ưu hóa |
| `C_PURPLE` | `#6A1B9A` | cá thể quần thể (đa dạng màu) |
| `C_GRAY` | `#757575` | chú thích phụ, phần tử "ở xa/không quan trọng" |
| `C_PINK` | `#AD1457` | hộp kết thúc, trình diễn, UI |

**Quy ước vẽ hộp/mũi tên** (helper có sẵn trong file):
- `draw_box(ax, cx, cy, w, h, text, fc, ec, fs, bold, radius)` — `FancyBboxPatch` bo góc (`boxstyle="round,pad=...`)
- `draw_arrow(ax, x0, y0, x1, y1, color, lw, style)` — `FancyArrowPatch`, đầu mũi tên `-|>`
- `save(fig, name)` — lưu `dpi=150, bbox_inches="tight"`, nền `facecolor=BG`
- Font: `plt.rcParams["font.family"] = "DejaVu Sans"` (hỗ trợ tiếng Việt có dấu)
- In console dùng ASCII thuần (Windows console không decode được dấu tiếng Việt khi `print`)

**Phân loại (cột "Loại")** dùng xuyên suốt bảng dưới:
- 🔧 **Sơ đồ khái niệm** — minh họa ý tưởng/luồng/cấu trúc, không phụ thuộc số liệu thật → vẽ trực tiếp bằng matplotlib (ưu tiên xử lý sớm)
- 📊 **Biểu đồ dữ liệu thực** — cần số liệu thật (CSV/JSON huấn luyện, danh sách thẻ bài, gene...) → phải trích xuất dữ liệu trước khi vẽ
- 📷 **Ảnh chụp màn hình** — cần chạy game thật và chụp lại → không thể tạo bằng matplotlib

---

## 2. Tổng quan theo chương

| Chương | Số hình | Sơ đồ khái niệm | Biểu đồ dữ liệu thực | Ảnh chụp màn hình | Trạng thái |
|---|---|---|---|---|---|
| 2 — Cơ sở lý thuyết | 7 | 7 | 0 | 0 | ✅ **Hoàn thành** (script + ảnh đã tạo, đã soát lỗi trực quan) |
| 3 — Thiết kế Game (GDD) | 14 | 7 | 1 | 6 | 🟡 **8/14 xong** (7 sơ đồ + 1 biểu đồ dữ liệu — script + ảnh đã tạo, đã soát lỗi trực quan; còn 6 ảnh chụp màn hình chờ chạy game) |
| 4 — Kiến trúc hệ thống | 3 | 3 | 0 | 0 | ⬜ Chưa làm |
| 5 — Hệ thống AI / GA | 8 | 4 | 4 | 0 | ✅ **Hoàn thành** (script + ảnh đã tạo, đã soát lỗi trực quan) |
| 6 — Kết quả đánh giá | 2 | 0 | 0 | 2 | ⬜ Chưa làm |
| Phụ Lục | 0 | — | — | — | — (không có hình) |
| **Tổng** | **34** | **21** | **5** | **8** | |

---

## 3. Chương 2 — Cơ Sở Lý Thuyết (7 hình) ✅ ĐÃ HOÀN THÀNH

Toàn bộ 7 hình đều là sơ đồ/biểu đồ minh họa khái niệm — không phụ thuộc số
liệu thật — nên đã được lập trình trực tiếp trong
**`Document/04_Tools/generate_figures_ch2.py`** (chạy bằng
`python generate_figures_ch2.py` từ thư mục `04_Tools`, ảnh xuất ra
`Document/03_Figures/hinh_2_1.png` … `hinh_2_7.png`). Đã regenerate và soát lỗi
trực quan từng ảnh — sửa 4 lỗi bố cục/logic được phát hiện trong quá trình review
(chi tiết trong từng mục).

---

### Hình 2.1 — Tìm Kiếm Đơn-Điểm Và Tìm Kiếm Theo Quần Thể
- **Vị trí placeholder:** `02_1_CoSoLyThuyet_GA.md:19`
- **Loại:** 🔧 Sơ đồ khái niệm — 2 panel đối chiếu trên cùng một bề mặt fitness
- **File ảnh:** `hinh_2_1.png` · **Hàm:** `hinh_2_1()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi mâu thuẫn hình ảnh (xem ghi chú)

**Spec kỹ thuật:**
- Layout: `1×2 subplots`, `figsize=(11.5, 4.7)`, nền `BG`
- Bề mặt fitness `f(x)` dùng chung cho cả 2 panel, `x = linspace(0, 10, 400)`,
  tổng 3 hàm Gauss tách biệt rõ ràng — **KHÔNG dùng hàm sin** (bản đầu dùng sin
  khiến cực trị địa phương "mắc kẹt" trùng vị trí với cực trị toàn cục, tạo mâu
  thuẫn logic — đã thay bằng tổ hợp Gauss để 3 đỉnh tách biệt và có độ cao khác
  nhau rõ ràng):
  ```
  f(x) = 2.2·exp(-(x-2.0)²/(2·0.65²))      # cực trị địa phương, x=2.0, f≈2.6
       + 1.5·exp(-(x-5.0)²/(2·0.90²))      # đỉnh phụ thấp hơn, x=5.0, f≈1.9
       + 3.0·exp(-(x-8.0)²/(2·0.60²))      # cực trị TOÀN CỤC, x≈8.0, f≈3.4
       + 0.4
  ```
  Vẽ đường viền `color="#444444"`, tô nền dưới đường bằng `fill_between(alpha nhạt, màu xám)`
- **Panel (a)** "Tìm kiếm đơn-điểm (gradient / hill-climbing)":
  - Một quỹ đạo đỏ (`C_RED`) gồm 4 điểm nối tiếp leo dốc: `path_x = [0.6, 1.1, 1.6, 2.0]`,
    dừng đúng tại đỉnh cực trị địa phương `local_x = 2.0`
  - Chú thích đỏ "Mắc kẹt ở cực trị địa phương gần điểm khởi đầu" — mũi tên trỏ
    vào điểm dừng `(2.0, f(2.0))`, đặt lệch lên-phải để không che quỹ đạo
  - Ngôi sao xám (marker `*`, size 170, viền trắng) tại đúng đỉnh toàn cục `(global_x, f.max())`
  - Chú thích xám "Cực trị toàn cục — ở xa, ngoài tầm 'nhìn thấy' của một điểm
    đơn lẻ" — đặt **bên trái và thấp hơn** đỉnh toàn cục, trong vùng trắng phía
    trên đường cong giữa 2 đỉnh phụ (toạ độ `xytext=(global_x-2.0, f.max()-1.3)`,
    `ha="right"`) để tránh đè lên quỹ đạo đỏ hoặc lọt vào vùng tô bóng dưới đường cong
- **Panel (b)** "Tìm kiếm theo quần thể (Genetic Algorithm)":
  - Nhiều điểm màu khác nhau (đại diện các cá thể qua các thế hệ) rải rác ở
    nhiều mức fitness, nối với nhau bằng các đoạn thẳng màu nhạt biểu diễn
    "đường đi qua các thế hệ kế tiếp", hội tụ dần về cùng một vùng gần đỉnh toàn cục
  - Ngôi sao đỏ đánh dấu điểm hội tụ cuối cùng gần đỉnh toàn cục
  - Chú thích đỏ "Quần thể hội tụ dần về vùng cực trị tốt hơn"
  - Chú thích chú giải nhỏ ở góc dưới: "● = một cá thể trong quần thể — đường
    nối = đường đi qua các thế hệ kế tiếp"
- Trục: `"Không gian lời giải  x"` (hoành), `"f(x) — fitness"` (tung), ẩn số trên trục (minh họa thuần túy)
- `fig.suptitle("Hình 2.1 — Tìm Kiếm Đơn-Điểm Và Tìm Kiếm Theo Quần Thể")`

> **Lỗi đã sửa:** bản đầu đặt cực trị toàn cục ("không bao giờ chạm tới") gần
> như trùng vị trí với nơi quỹ đạo đơn-điểm "mắc kẹt" — tự mâu thuẫn về mặt thị
> giác. Đã thiết kế lại hàm `f(x)` thành 3 đỉnh Gauss tách biệt rõ ràng (địa
> phương ở x≈2, toàn cục ở x≈8) và định vị lại 2 chú thích để không chồng lấn
> lên nhau hoặc lọt vào vùng tô bóng dưới đường cong.

---

### Hình 2.2 — Vòng Lặp Tiến Hóa Tổng Quát Của Một GA Chuẩn
- **Vị trí placeholder:** `02_1_CoSoLyThuyet_GA.md:53`
- **Loại:** 🔧 Sơ đồ khái niệm — flowchart dọc
- **File ảnh:** `hinh_2_2.png` · **Hàm:** `hinh_2_2()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi nhãn đè lên hộp

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(7.2, 9.8)`, hệ trục ảo `xlim(0,10) × ylim(0,12.4)`, `axis("off")`
- 6 hộp xanh dương xếp dọc chính giữa (`cx=5`), nối bằng mũi tên thẳng đứng đi xuống:
  1. `"Khởi tạo quần thể"` — nền xanh lá nhạt `#E8F5E9`, viền `C_GREEN` (bước khởi tạo, tách màu để nổi bật)
  2. `"Đánh giá fitness\ntừng cá thể"` — nền `#E3F2FD`, viền `C_BLUE`
  3. `"Chọn lọc\n(Selection)"`
  4. `"Lai ghép\n(Crossover)"`
  5. `"Đột biến\n(Mutation)"`
  6. `"Thay thế thế hệ\n(+ Elitism)"`
  (kích thước mỗi hộp `3.3 × 1.15`, `fontsize=9.3`)
- Hình thoi cam (`fc="#FFF3E0", ec=C_ORANGE`) bên dưới hộp 6: `"Đạt điều kiện\ndừng?"`
- **Nhánh "chưa đạt"**: đường xám đi từ cạnh trái hình thoi sang trái (`x=1.0`),
  rồi đi thẳng lên tới ngang hàng hộp 2 ("Đánh giá fitness"), mũi tên chỉ vào
  cạnh trái hộp đó — tạo vòng lặp khép kín. Nhãn `"chưa đạt\n→ lặp lại"` xoay
  90°, đặt dọc theo đoạn thẳng đứng bên trái
- **Nhánh "đạt"**: mũi tên ngang sang phải tới hộp kết thúc màu hồng
  (`fc="#FCE4EC", ec=C_PINK`, chữ đậm) `"Kết thúc\n(trả về cá thể\ntốt nhất)"`.
  Nhãn `"đạt"` màu xám đặt **phía trên** đoạn mũi tên ngang, căn giữa
  (`ha="center"`, toạ độ giữa hình thoi và hộp kết thúc — KHÔNG đặt đè lên góc
  trái hộp kết thúc, đó là lỗi bản đầu)
- `ax.set_title("Hình 2.2 — Vòng Lặp Tiến Hóa Tổng Quát Của Một GA Chuẩn")`

> **Lỗi đã sửa:** nhãn `"đạt"` ban đầu đặt tại toạ độ lấn vào góc trên-trái của
> hộp "Kết thúc" (chồng cả về x lẫn y với vùng hộp), gây khó đọc. Đã dời nhãn
> lên phía trên đường mũi tên ngang và canh giữa, tách hẳn khỏi hộp.

---

### Hình 2.3 — Hội Tụ Sớm Và Tiến Hóa Lành Mạnh: Hai Kịch Bản Đối Chiếu
- **Vị trí placeholder:** `02_1_CoSoLyThuyet_GA.md:68`
- **Loại:** 🔧 Sơ đồ khái niệm — 2 biểu đồ đường minh họa (hàm toán học, không phải số liệu thật)
- **File ảnh:** `hinh_2_3.png` · **Hàm:** `hinh_2_3()`
- **Trạng thái:** ✅ Đã tạo — không phát hiện lỗi khi review trực quan

**Spec kỹ thuật:**
- Layout: `1×2 subplots`, mỗi panel có **trục tung phụ** (twin y-axis):
  trục trái màu xanh dương = "Đa dạng (Std Dev)" thang `0.0–1.0`,
  trục phải màu đỏ = "Best fitness" thang `2–10`
- Trục hoành chung: `"Thế hệ"` (generation), khoảng `0–60`
- **Panel (a)** "Hội tụ sớm (premature convergence)":
  - Đường xanh dương: giảm theo hàm mũ rất nhanh từ 1.0 về gần 0 trong ~20 thế hệ đầu
  - Đường đỏ: tăng nhanh rồi **plateau sớm ở mức thấp** (~5/10)
  - Hộp chú thích góc dưới-phải: `"Đa dạng mất sớm —\nfitness mắc kẹt ở mức thấp"`
- **Panel (b)** "Tiến hóa lành mạnh":
  - Đường xanh dương: giảm chậm và đều, kết thúc quanh ~0.2 (còn giữ một phần đa dạng)
  - Đường đỏ: tăng dần đều, đạt mức cao hơn hẳn (~8.3/10) — thể hiện tiếp tục cải thiện
  - Hộp chú thích: `"Đa dạng giảm chậm và đều —\nfitness tăng ổn định"`
- Legend dùng chung đặt phía trên, giữa 2 panel: 2 mục — đường xanh
  `"Độ lệch chuẩn quần thể (đa dạng)"`, đường đỏ `"Best fitness"`
- `fig.suptitle("Hình 2.3 — Hội Tụ Sớm Và Tiến Hóa Lành Mạnh: Hai Kịch Bản Đối Chiếu")`

---

### Hình 2.4 — Kế Thừa Sâu Và Component Hóa
- **Vị trí placeholder:** `02_2_CoSoLyThuyet_Unity.md:9`
- **Loại:** 🔧 Sơ đồ khái niệm — 2 sơ đồ cấu trúc đối chiếu
- **File ảnh:** `hinh_2_4.png` · **Hàm:** `hinh_2_4()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi các hộp lá đè lên nhau

**Spec kỹ thuật:**
- Layout: `1×2 subplots`, `figsize=(12, 5.8)`, mỗi panel hệ trục ảo `10×10`, `axis("off")`
- **Panel (a)** "Kế thừa sâu — bùng nổ tổ hợp lớp": cây phân cấp 4 tầng, vẽ từ trên xuống
  - Tầng 1 (gốc): `"Card"` — hộp xanh dương `#E3F2FD/C_BLUE`
  - Tầng 2: `"MeleeCard"`, `"RangedCard"` (cùng màu)
  - Tầng 3: `"TauntMelee"`, `"RebornMelee"`, `"TauntRanged"`, `"RebornRanged"`
    — đặt cách đều, **không chồng lấn** (khoảng cách tâm ≥ chiều rộng hộp `1.95`,
    cụ thể `x = 1.7 / 3.95 / 6.05 / 8.3`)
  - Tầng 4 (lá — nền đỏ nhạt `#FFEBEE/C_RED`, hộp **hẹp hơn** `1.45` và font nhỏ
    hơn `6.0` để 6 hộp vừa khít hàng ngang mà không đè lên nhau, `x` cách đều
    `1.6` đơn vị: `1.0 / 2.6 / 4.2 / 5.8 / 7.4 / 9.0`):
    `"TauntRebornMeleeA"`, `"...x?"`, `"..x??"`, `"...x???"`, `"..x????"`, `"TauntRebornRangedB"`
    — các tên viết tắt/dấu `...x?` mô phỏng việc số lớp lá nổ ra theo tổ hợp
  - Đường nối xám nhạt (`#bbbbbb`) giữa tâm cha–con, lệch theo nửa chiều cao hộp
  - Chú thích đỏ bên dưới: `"→ thêm 1 đặc điểm mới (vd. \"Safeguard\") buộc nhân
    đôi số lớp lá đã tồn tại — chi phí tăng theo cấp số nhân"`
- **Panel (b)** "Component hóa — lắp ráp hành vi từ các phần độc lập": sơ đồ tâm-vệ tinh
  - Hộp trung tâm màu cam `#FFF8E1/C_ORANGE`, chữ đậm: `"Entity\n(Card instance)"`
  - 6 hộp vệ tinh xanh lá (`#E8F5E9/C_GREEN`) bố trí toả đều quanh tâm theo lưới
    2 cột × 3 hàng (trên/giữa/dưới mỗi bên): `StatsComponent (ATK, HP)`,
    `RenderComponent (sprite, anim)`, `AbilityComponent (TTE skill)`,
    `CombatComponent (target, dmg)`, `AIWeightComponent (gene mapping)`,
    `DataRefComponent (card định nghĩa)`
  - Mỗi vệ tinh nối thẳng với tâm bằng đường xanh lá `alpha=0.6`
  - Chú thích xanh lá bên dưới: `"→ thêm 1 đặc điểm mới = gắn thêm 1 component —
    không phải sửa hay nhân bản bất kỳ lớp nào đã có"`
- `fig.suptitle("Hình 2.4 — Kế Thừa Sâu Và Component Hóa")`

> **Lỗi đã sửa:** hàng 6 hộp lá ở panel (a) ban đầu đặt quá sát nhau (khoảng
> cách tâm < chiều rộng hộp) khiến chúng chồng lên nhau, chữ không đọc được,
> trông như 1 khối liền; đồng thời 2 hộp tầng 3 ở giữa (`RebornMelee`/`TauntRanged`)
> cũng đè nhau. Đã giảm bề rộng + tăng khoảng cách của hộp lá, và giãn 2 hộp
> tầng 3 ra (`3.95`/`6.05` thay vì `4.3`/`5.7`).

---

### Hình 2.5 — Hai Mô Hình Tổ Chức Vòng Lặp Game
- **Vị trí placeholder:** `02_2_CoSoLyThuyet_Unity.md:27`
- **Loại:** 🔧 Sơ đồ khái niệm — 2 sơ đồ luồng đối chiếu
- **File ảnh:** `hinh_2_5.png` · **Hàm:** `hinh_2_5()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi hộp bị cắt ở mép phải

**Spec kỹ thuật:**
- Layout: `1×2 subplots`, `figsize=(12, 5.2)`, mỗi panel hệ trục ảo `10×10`, `axis("off")`
- **Panel (a)** "Tính toán và trình diễn đan xen trong cùng vòng lặp":
  - 2 cột so le bậc thang xuống: cột trái xanh dương `"Tính bước 1/2/3"`
    (`y = 8.6 / 5.8 / 3.0`), cột phải hồng `"Trình diễn bước 1/2/3"`
    (`y = 7.2 / 4.4 / 1.6`, lệch xuống so với cột trái)
  - Mũi tên xám nối tuần tự: Tính 1 → Trình diễn 1 → Tính 2 → Trình diễn 2 → Tính 3 → Trình diễn 3
    (chuỗi zig-zag minh họa "đan xen", mỗi bước phải đợi bước trước)
  - Chú thích dưới: `"mỗi bước phải đợi bước trước \"trình diễn xong\"\n→ không
    thể tách rời để chạy độc lập, khó kiểm thử, khó headless"`
- **Panel (b)** "Tính toán hoàn tất trước, trình diễn độc lập sau": pipeline dọc
  - Hộp xanh dương (to nhất, `4.0×1.7`) `"TÍNH TOÁN\n(đồng bộ, tức thời)\nchạy
    trọn vẹn N bước liên tiếp"` ở trên cùng
  - Mũi tên xuống → hộp cam đậm (`#FFF8E1/C_ORANGE`, chữ đậm)
    `"Nhật ký hành động\n(TurnRecord — chuỗi sự kiện có thứ tự)"` ở giữa
  - Mũi tên xuống → hộp hồng `"TRÌNH DIỄN\n(bất đồng bộ, theo thời gian thực)\n
    phát lại hoạt ảnh / âm thanh / UI"` ở dưới cùng
  - Một nhánh phụ màu xanh lá tách từ hộp cam, đi ngang sang phải tới hộp xanh
    lá nhỏ hơn ở rìa phải `"Headless / Batch:\nbỏ qua nhánh trình diễn,\nlặp lại
    hàng trăm nghìn lần"` — **đặt tâm hộp đủ gần lề phải để vẫn nằm trọn trong
    `xlim(0,10)` sau khi cộng nửa-chiều-rộng + padding bo góc** (cụ thể
    `cx=8.45, w=2.3` thay vì `cx=8.7, w=2.55` — bản đầu khiến viền/chữ bị cắt mép)
  - Chú thích dưới: `"cùng MỘT lõi tính toán — chạy có người xem (game thật)\n
    hoặc không (huấn luyện AI), không có nguy cơ phân kỳ logic"`
- `fig.suptitle("Hình 2.5 — Hai Mô Hình Tổ Chức Vòng Lặp Game")`

> **Lỗi đã sửa:** hộp `"Headless / Batch"` ở rìa phải panel (b) đặt với tâm và
> bề rộng khiến mép phải vượt quá `xlim=10` — viền hộp và dòng chữ cuối bị cắt
> khi `bbox_inches="tight"` crop ảnh. Đã thu nhỏ hộp và dịch tâm vào trong.

---

### Hình 2.6 — Khuôn Mẫu Trigger – Target – Effect Như Một Không Gian Tổ Hợp Ba Chiều
- **Vị trí placeholder:** `02_3_CoSoLyThuyet_TTE.md:33`
- **Loại:** 🔧 Sơ đồ khái niệm — scatter 3D minh họa không gian tổ hợp
- **File ảnh:** `hinh_2_6.png` · **Hàm:** `hinh_2_6()`
- **Trạng thái:** ✅ Đã tạo (cảnh báo `"Tight layout not applied"` khi chạy —
  không ảnh hưởng tới ảnh xuất ra, chỉ là matplotlib không tối ưu được margin
  cho `Axes3D`; ảnh vẫn lưu đầy đủ và đọc được)

**Spec kỹ thuật:**
- `fig.add_subplot(111, projection="3d")`, `figsize=(8.8, 7.4)`, nền `BG` cho figure/axes/3 mặt phẳng (pane)
- 3 trục rời rạc, mỗi trục là danh sách nhãn dạng category (không phải số):
  - Trục X — **Trigger** (6 giá trị): `StartBattle, OnDeath, OnAttack, OnTakeDmg, EndTurnShop, OnDeploy`
  - Trục Y — **Target** (5 giá trị): `Self, RandomAlly, WeakestEnemy, AllAllies, AllEnemies`
  - Trục Z — **Effect** (5 giá trị): `AddStats, Summon, DealDmg, GainCoin, GiveBuff`
- Dùng `np.meshgrid(range(nt), range(nx), range(ne), indexing="ij")` để sinh
  toàn bộ `6×5×5 = 150` điểm lưới — vẽ bằng `ax.scatter` màu xám rất nhạt, kích
  thước nhỏ, `alpha` thấp — biểu diễn "toàn bộ không gian tổ hợp khả dĩ"
- Đánh dấu **một điểm cụ thể** bằng marker đỏ đậm, kích thước lớn hơn hẳn, viền
  trắng, tại toạ độ `(OnDeath, Self, AddStats)` — minh họa cho một kỹ năng thật
  (vd. unit "Anubis")
- Chú thích đỏ: `"Một điểm = một kỹ năng cụ thể\nVD: Anubis = (OnDeath, Self, AddStats)"`
  — đặt lệch trái, gần mặt phẳng đáy, không che điểm đỏ
- Nhãn 3 trục in nghiêng: `"Trigger — Khi nào?"`, `"Target — Nhắm vào ai?"`, `"Effect — Làm gì?"`
- `fig.suptitle("Hình 2.6 — Khuôn Mẫu Trigger – Target – Effect\nNhư Một Không Gian Tổ Hợp Ba Chiều")`

---

### Hình 2.7 — Đánh Đổi Tempo Và Economy Theo Bối Cảnh
- **Vị trí placeholder:** `02_4_CoSoLyThuyet_Economy.md:20`
- **Loại:** 🔧 Sơ đồ khái niệm — biểu đồ đường minh họa (hàm toán học, không phải số liệu thật)
- **File ảnh:** `hinh_2_7.png` · **Hàm:** `hinh_2_7()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi 2 nhãn chú thích chồng lên nhau / đè lên legend

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(8.4, 5.2)`, `x = linspace(0, 1, 240)`
- 2 đường hình chuông (Gaussian), mỗi đường tô bóng nhạt phía dưới (`fill_between alpha=0.08`):
  - Xanh dương `"Đang dẫn đầu — \"có thể chờ khoản đầu tư phát huy\""`:
    đỉnh tại `x=0.70`, `y_lead = exp(-(x-0.70)²/(2·0.17²))`
  - Đỏ `"Đang nguy cấp — \"phải mạnh ngay, chưa chắc còn tương lai\""`:
    đỉnh tại `x=0.25`, `y_danger = exp(-(x-0.25)²/(2·0.15²))`
- Mỗi đỉnh đánh dấu bằng 1 chấm tròn cùng màu (viền trắng), kèm chú thích
  `"điểm lựa chọn\ntối ưu"` cùng màu với mũi tên trỏ vào đỉnh — **2 vị trí đặt
  riêng biệt, không trùng nhau và không đè lên legend** (legend nằm góc trên-trái):
  - Đỉnh xanh dương (x=0.70): nhãn đặt ngay phía trên đỉnh, `xytext=(0.70, 1.16)`, `ha="center"`
  - Đỉnh đỏ (x=0.25): nhãn đặt **chếch xuống bên phải đỉnh**, vào vùng trắng
    giữa 2 đường cong, `xytext=(0.42, 0.74)`, `ha="left"`, có mũi tên chỉ ngược
    lại lên đỉnh — tránh vùng legend phía trên-trái
- Trục hoành ẩn số, thay bằng 1 mũi tên ngang phía dưới biểu đồ nối 2 nhãn neo 2 đầu:
  `"ưu tiên Tempo\n(mạnh tức thời)"` (trái) ↔ `"ưu tiên Economy\n(đầu tư dài hạn)"` (phải)
- Trục tung: `"Xác suất sống sót / thắng cuộc"`; trục hoành: `"Mức độ ưu tiên đầu tư dài hạn"`
- Legend góc trên-trái, `frameon=False`, liệt kê 2 đường theo đúng màu
- `ax.set_title("Hình 2.7 — Đánh Đổi Tempo Và Economy Theo Bối Cảnh")`

> **Lỗi đã sửa:** cả 2 nhãn `"điểm lựa chọn tối ưu"` ban đầu đặt cùng một công
> thức `xytext=(xm, y.max()+0.16)` — với đỉnh đỏ ở `x=0.25`, nhãn rơi thẳng vào
> vùng chiếm chỗ của legend (góc trên-trái), gây chồng chữ khó đọc. Đã tách
> thành 2 vị trí riêng: nhãn xanh dương giữ phía trên đỉnh (vùng trống), nhãn đỏ
> dời xuống-phải vào khoảng trắng giữa 2 đường cong kèm mũi tên dẫn ngược lên đỉnh.

---

## 4. Chương 3 — Thiết Kế Game / GDD (14 hình) 🟡 ĐÃ LÀM PHẦN SƠ ĐỒ + DỮ LIỆU (8/14)

8 hình loại 🔧/📊 (3.1, 3.3, 3.7, 3.8, 3.9, 3.10, 3.11, 3.12) đã được lập trình
trong **`Document/04_Tools/generate_figures_ch3.py`** (chạy bằng
`python generate_figures_ch3.py` từ thư mục `04_Tools`, ảnh xuất ra
`Document/03_Figures/hinh_3_1.png`, `hinh_3_3.png`, `hinh_3_7.png` …
`hinh_3_12.png`). Đã regenerate và soát lỗi trực quan từng ảnh — sửa 6 lỗi bố
cục/logic phát hiện trong quá trình review (chi tiết trong từng mục). 6 hình
còn lại (3.2, 3.4, 3.5, 3.6, 3.13, 3.14) là ảnh chụp màn hình thật — ngoài phạm
vi script, vẫn ở trạng thái "chưa làm" chờ chạy game để chụp.

---

### Hình 3.1 — Ba Bộ Tộc Và Triết Lý Chơi
- **Vị trí placeholder:** `03_1_ThietKeGame_TamNhin.md:27`
- **Loại:** 🔧 Sơ đồ khái niệm — infographic 3 cột
- **File ảnh:** `hinh_3_1.png` · **Hàm:** `hinh_3_1()`
- **Trạng thái:** ✅ Đã tạo — không phát hiện lỗi khi review trực quan

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(12, 6.6)`, hệ trục ảo `xlim(0,11) × ylim(0,6.7)`, `axis("off")`
- 3 cột đặt tại `cx = 2.0 / 5.5 / 9.0`, mỗi cột có khung viền mảnh bao ngoài
  (`FancyBboxPatch` nền trắng, viền màu tộc, `alpha=0.55`) cho cảm giác "thẻ":
  - **BABYLON** (`C_BLUE`, nền `#E3F2FD`)
  - **OLYMPUS** (`C_PURPLE`, nền `#F1E9FB`)
  - **NILES** (`C_GREEN`, nền `#E5F2E8`)
- Mỗi cột: 1 hộp tiêu đề đậm màu tộc (chữ trắng, `fontsize=13.5`) + 4 hộp nội
  dung xếp dọc, mỗi hộp có **nhãn nhỏ in nghiêng màu tộc ở mép trên** (label
  `"Triết lý cốt lõi" / "Cơ chế đặc trưng" / "Phong cách chơi" / "Unit tiêu
  biểu"`, `fontsize=7.0`) và nội dung căn giữa bên dưới (`fontsize≈7.4–8.6`)
- Nội dung lấy nguyên văn từ bảng §3.1.1 trong tài liệu nguồn:
  - Babylon: *"ACCUMULATION — snowball dài hạn"* / *"Buff lẫn nhau qua sự kiện
    deploy/sell, hấp thụ chỉ số tích lũy vĩnh viễn"* / unit tiêu biểu Utu, Ashur,
    Lamashtu–Uridimmu
  - Olympus: *"AGGRESSION — áp đảo sớm"* / *"ATK synergy qua các sự kiện combat"*
    / chú thích rõ **"(thiết kế dự kiến — chưa có unit thực)"**
  - Niles: *"REACTION — mạnh bất ngờ qua chuỗi trigger"* / *"Chu trình chết – tái
    sinh, chuỗi phản ứng OnAllyDeath lan tỏa toàn đội hình"* / Anubis, Osiris,
    Sobek, Thoth
- `ax.set_title("Hình 3.1 — Ba Bộ Tộc Và Triết Lý Chơi")`

---

### Hình 3.3 — Sơ Đồ Vòng Lặp Một Ván Đấu (Match Loop)
- **Vị trí placeholder:** `03_2_ThietKeGame_CoreLoop.md:38`
- **Loại:** 🔧 Sơ đồ khái niệm — flowchart dọc có vòng lặp
- **File ảnh:** `hinh_3_3.png` · **Hàm:** `hinh_3_3()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi đường vòng lặp tràn khung và đè nhãn (xem ghi chú)

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(8.6, 10.5)`, hệ trục ảo `xlim(0,10) × ylim(1.2,14.2)`, `axis("off")`
- Chuyển thể trực tiếp từ sơ đồ ASCII tại `03_2_ThietKeGame_CoreLoop.md:8-36`,
  xếp dọc theo trục chính `cx=5`:
  1. `[BẮT ĐẦU VÁN ĐẤU]` — hộp xám trung tính `"HP=7 · Cup=0 · Lượt=1"`
  2. **SHOP PHASE** — hộp xanh lá (`fc="#E5F2E8", ec=C_GREEN`) `"nhận 10 coin ·
     làm mới shop · mua/bán/xếp đội hình/reroll"`
  3. mũi tên có nhãn nghiêng `"[Nhấn Fight]"` →
  4. **COMBAT PHASE** — hộp đỏ (`fc="#FBE3E3", ec=C_RED`) `"chiến đấu tự động
     (auto-battler) · tối đa 50 round/trận"`
  5. 3 nhánh kết quả nằm ngang, mỗi nhánh có màu riêng: `THẮNG / Cup +1` (xanh
     lá), `THUA / HP −1` (đỏ), `HÒA / không đổi` (xám) — đường cong nối Combat
     Phase xuống từng nhánh bằng `connectionstyle="arc3,rad=..."` nhỏ
  6. Cả 3 nhánh hội tụ vào **hộp điều kiện kết thúc** màu vàng cam (`fc="#FCEFD0",
     ec=C_ORANGE`) liệt kê đủ 4 nhánh rẽ:
     `"Cup ≥ 10 ? → Thắng ván / HP ≤ 0 ? → Thua ván / Lượt > 20 ? → Thắng ván
     (vượt thời hạn) / Còn lại → Lượt += 1, quay lại Shop"`
  7. **Vòng lặp "còn lại"**: vẽ bằng **đường gấp khúc 3 đoạn thẳng** (KHÔNG dùng
     `connectionstyle="arc3"` với `rad` lớn) — đoạn ngang trái từ mép trái hộp
     điều kiện (`x=cx-3.3`) tới `x=loop_x=0.75`, đoạn dọc lên tới độ cao Shop
     Phase, đoạn ngang phải có mũi tên trỏ vào mép trái hộp Shop Phase. Nhãn
     `"còn lại → Lượt + 1, quay lại Shop"` đặt **bên dưới** hộp điều kiện (vùng
     trống phía dưới-trái), không chạm vào đường line hay hộp nào
  8. 2 nhánh kết thúc: `[KẾT THÚC] THẮNG VÁN (Cup ≥ 10 / Lượt > 20)` (xanh lá,
     bên phải) và `[KẾT THÚC] THUA VÁN (HP ≤ 0)` (đỏ, bên trái-dưới) — mỗi
     nhánh nối từ hộp điều kiện bằng đường cong `arc3,rad` nhỏ
- `ax.set_title("Hình 3.3 — Sơ Đồ Vòng Lặp Một Ván Đấu (Match Loop)")`

> **Lỗi đã sửa:** bản đầu vẽ đường vòng lặp "còn lại → quay lại Shop" bằng một
> `FancyArrowPatch` với `connectionstyle="arc3,rad=0.55"` — cung cong rất lớn
> bị bẻ quá xa sang trái, gần như tràn ra ngoài `xlim=0`, cắt ngang qua nhãn chú
> thích. Đã thay bằng đường gấp khúc 3 đoạn thẳng kiểu sơ đồ khối chuẩn (ngang
> – dọc – ngang có mũi tên), đồng thời dời nhãn xuống vùng trống phía dưới hộp
> điều kiện thay vì đặt cạnh đường line dọc (ban đầu đè lên cả đường line lẫn
> mép hộp điều kiện ở 2 lần thử vị trí trước).

---

### Hình 3.7 — So Sánh Ba Triết Lý Tribe Synergy
- **Vị trí placeholder:** `03_3_ThietKeGame_CardSystem.md:49`
- **Loại:** 🔧 Sơ đồ khái niệm — bảng so sánh 3 cột × 4 hàng
- **File ảnh:** `hinh_3_7.png` · **Hàm:** `hinh_3_7()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi nhãn hàng đè lên nội dung ô (xem ghi chú)

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(11.5, 6.6)`, hệ trục ảo `xlim(0,11) × ylim(0,6.5)`, `axis("off")`
- 3 cột **BABYLON / NILES / OLYMPUS** tại `cx = 2.0 / 5.5 / 9.0`, mỗi cột có
  khung viền mảnh bao ngoài (giống Hình 3.1) + hộp tiêu đề đậm màu tộc
- Mỗi cột gồm 4 ô xếp dọc, **nhãn tiêu chí đặt ngay trong từng ô** (in nghiêng,
  đậm, màu tộc, `fontsize=7.0`, căn trên) thay vì một cột nhãn riêng bên trái:
  `Trigger chính / Giai đoạn mạnh nhất / Rủi ro chính / Unit tiêu biểu`
- Nội dung trích nguyên văn §3.3.2 của tài liệu nguồn:
  - **Babylon**: Deploy/Sell (vòng kinh tế) · Late-game — snowball tích lũy dài
    hạn · "Yếu đầu game, khó chặn khi đã bùng nổ" · Utu, Ashur, Lamashtu–Uridimmu
  - **Niles**: Summon/Reborn/Death (chuỗi phản ứng) · Bất kỳ lúc nào — bùng nổ
    qua chuỗi trigger · "Đội hình dễ vỡ, phụ thuộc vào chuỗi kích hoạt" ·
    Anubis, Osiris, Sobek, Thoth
  - **Olympus**: Combat events (ATK synergy) · Early-game — áp đảo ngay từ đầu
    · "Hiện chưa có unit — thiết kế dự kiến" · "(chưa có — gene[19] giữ chỗ mở rộng)"
- `ax.set_title("Hình 3.7 — So Sánh Ba Triết Lý Tribe Synergy")`

> **Lỗi đã sửa:** bản đầu đặt 4 nhãn tiêu chí (`Trigger chính`, `Giai đoạn mạnh
> nhất`...) thành một cột nhãn riêng ở lề trái (`x=0.15`), nhưng do cột BABYLON
> bắt đầu quá gần lề (`x≈0.375`) nên các nhãn dài (đặc biệt "Giai đoạn mạnh
> nhất") tràn vào và chồng lấn trực tiếp lên nội dung ô của cột đó. Đã chuyển
> hẳn sang kiểu nhãn-trong-ô (đặt ở mép trên mỗi ô, in nghiêng nhỏ màu tộc) —
> đồng nhất với cách trình bày đã dùng ở Hình 3.1, vừa hết chồng lấn vừa nhất
> quán phong cách giữa hai hình infographic.

---

### Hình 3.8 — Sơ Đồ Sân Chiến Đấu 7 Slot (Frontline / Backline)
- **Vị trí placeholder:** `03_6_ThietKeGame_Combat.md:15`
- **Loại:** 🔧 Sơ đồ khái niệm — lưới 7 ô + 2 vùng + chú thích hướng tấn công
- **File ảnh:** `hinh_3_8.png` · **Hàm:** `hinh_3_8()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi chú thích phụ bị cắt khỏi khung (xem ghi chú)

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(10, 4.6)`, hệ trục ảo `xlim(0,10.4) × ylim(-1.35,4.2)`, `axis("off")`
- 7 ô vuông bo góc xếp ngang (`Slot 0`…`Slot 6`), 4 ô đầu tô đỏ nhạt (Frontline,
  `ec=C_RED`), 3 ô sau tô xanh dương nhạt (Backline, `ec=C_BLUE`)
- 2 khung nét đứt bao quanh từng vùng, có nhãn tiêu đề phía trên:
  `"FRONTLINE (slot 0–3)"` (đỏ) và `"BACKLINE (slot 4–6)"` (xanh dương) — viền
  2 khung **chồng nhẹ lên nhau ở biên slot 3/4** để thể hiện ranh giới chuyển tiếp
- Mũi tên đỏ lớn nằm ngang bên dưới, hướng từ phải sang trái (từ phía Backline
  đối phương hướng vào Frontline), kèm chú thích đỏ
  `"Đối phương luôn nhắm Frontline trước"`
- Mũi tên xám nhỏ thứ hai + chú thích 2 dòng căn giữa
  `"chỉ chuyển sang Backline\nkhi Frontline đã trống hoàn toàn"` đặt **ngay
  dưới** mũi tên đỏ, nằm gọn trong vùng `ylim` đã mở rộng xuống `-1.35`
- `ax.set_title("Hình 3.8 — Sân Chiến Đấu 7 Slot (Frontline / Backline)")`

> **Lỗi đã sửa:** bản đầu đặt chú thích phụ + mũi tên xám của nó tại
> `y = arr_y - 0.78 ≈ -0.40`, nhưng `ylim` khi đó chỉ là `(-0.4, 4.2)` — đúng
> ngay rìa dưới, khiến cả mũi tên lẫn 2 dòng chữ giải thích gần như biến mất
> (chỉ còn sót lại một mũi tên tam giác nhỏ bị cắt ở mép ảnh). Đã hạ tiếp vị
> trí xuống `sub_y = arr_y - 0.85`, đổi `annotate` sang `text` 2 dòng căn giữa
> (tránh chữ tràn `xlim` nếu để 1 dòng dài), và mở rộng `ylim` xuống `-1.35` để
> toàn bộ chú thích nằm gọn trong khung.

---

### Hình 3.9 — Thứ Tự Tấn Công Trong Một Round
- **Vị trí placeholder:** `03_6_ThietKeGame_Combat.md:33`
- **Loại:** 🔧 Sơ đồ khái niệm — 2 sân đối mặt + hàng đợi đánh số
- **File ảnh:** `hinh_3_9.png` · **Hàm:** `hinh_3_9()`
- **Trạng thái:** ✅ Đã tạo — đã bỏ mũi tên gây hiểu nhầm giữa 2 sân (xem ghi chú)

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(10.4, 5.6)`, hệ trục ảo `xlim(-0.3,11.2) ×
  ylim(0.5,5.0)`, `axis("off")`
- 2 hàng × 7 ô: hàng trên `"ĐỊCH"` (đỏ nhạt, nhãn xoay dọc bên trái), hàng dưới
  `"BẠN"` (xanh dương nhạt) — mỗi ô hiển thị số thứ tự `#N` (đậm, màu theo phe)
  phía trên và `"slot i"` phía dưới
- Số thứ tự sinh đúng theo quy tắc tại §3.6.2 của tài liệu nguồn — luân phiên
  địch/bạn theo từng chỉ số slot tăng dần:
  `Slot 0 (địch)=#1 → Slot 0 (bạn)=#2 → Slot 1 (địch)=#3 → … → Slot 6 (bạn)=#14`
  (cài đặt bằng vòng lặp tạo danh sách `order` rồi map sang `queue_no`, đảm bảo
  đúng thứ tự thay vì gán số thủ công dễ sai)
- 2 dòng chú thích phía trên lưới: liệt kê công thức hàng đợi rút gọn, và dòng
  in nghiêng xám `"→ tổng cộng tối đa 14 lượt hành động mỗi round khi cả hai
  sân đều đầy"`
- **Hộp chú giải (legend)** ở góc phải: ô vuông xám gạch chéo (`hatch="////"`)
  kèm chú thích `"Slot trống hoặc ATK = 0 → bị bỏ qua khỏi hàng đợi, không nhận
  số thứ tự"` — minh họa quy tắc loại trừ mà KHÔNG cần làm rối lưới chính bằng
  cách chèn ô trống thật vào (giữ lưới chính đầy đủ 14 số liền mạch, dễ đọc quy
  luật tổng quát; quy tắc "bỏ qua ô trống" được giải thích riêng qua chú giải)
- `ax.set_title("Hình 3.9 — Thứ Tự Tấn Công Trong Một Round")`

> **Lỗi đã sửa:** bản đầu vẽ thêm một mũi tên hai chiều (`<->`) nối thẳng đứng
> giữa ô Slot 0 của hàng "ĐỊCH" và hàng "BẠN", với ý định minh họa "luân phiên
> giữa 2 hàng". Khi xem lại, mũi tên này dễ bị hiểu lầm thành "hai unit này trực
> tiếp giao chiến với nhau" — không đúng bản chất (đây là thứ tự *lượt hành
> động*, không phải cặp đối đầu cố định). Đã bỏ hẳn mũi tên gây nhiễu này; thứ
> tự luân phiên đã được truyền tải đầy đủ qua chuỗi số `#1…#14` và dòng giải
> thích phía trên.

---

### Hình 3.10 — Một Chuỗi Phản Ứng Dây Chuyền Điển Hình
- **Vị trí placeholder:** `03_6_ThietKeGame_Combat.md:67`
- **Loại:** 🔧 Sơ đồ khái niệm — timeline ngang 6 bước
- **File ảnh:** `hinh_3_10.png` · **Hàm:** `hinh_3_10()` (dùng chung helper `_draw_chain_timeline`)
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi đường vòng lặp đè lên nhãn bước (xem ghi chú)

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(12.5, 3.6)`, `axis("off")`, hộp hồng nhạt nối tiếp
  bằng mũi tên ngang (`color=C_PINK, fc="#FBE3F0"`)
- 6 hộp xếp ngang đều nhau (`np.linspace`, khoảng cách `1.55`), mỗi hộp có nhãn
  `"Bước N"` phía trên (đậm, `C_PINK`):
  1. *"Unit A gục ngã"*
  2. *"Kỹ năng 'khi đồng minh chết' kích hoạt"*
  3. *"Unit B được ban hiệu ứng Reborn"*
  4. *"B gục ngã → hồi sinh ngay lập tức"*
  5. *"Kỹ năng nhân bội kích hoạt trên B"*
  6. *"Chuỗi khép lại, trận đấu tiếp tục"*
- Dòng chú thích in nghiêng xám bên dưới: `"chuỗi khép lại — trận đấu tiếp tục
  như bình thường"`
- `ax.set_title("Hình 3.10 — Một Chuỗi Phản Ứng Dây Chuyền Điển Hình")`

> **Lỗi đã sửa:** bản đầu vẽ thêm một cung nét đứt (`connectionstyle="arc3,
> rad=0.32"`) nối từ hộp "Bước 6" vòng ngược về hộp "Bước 1" để minh họa "chuỗi
> khép lại". Hướng cong của `arc3` lại bẻ lên phía TRÊN dãy hộp (ngược dự tính
> là đi vòng phía dưới), xuyên thẳng qua 2 nhãn `"Bước 3"` / `"Bước 4"` tạo hiệu
> ứng gạch ngang chữ rất khó nhìn. Vì dòng chú thích phía dưới đã truyền đạt đầy
> đủ ý "chuỗi khép lại", cung nối bị loại bỏ hoàn toàn thay vì cố định vị lại —
> giúp sơ đồ gọn và rõ hơn.

---

### Hình 3.11 — Chuỗi Phản Ứng Khi Một Unit Reborn Gục Ngã
- **Vị trí placeholder:** `03_6_ThietKeGame_Combat.md:77`
- **Loại:** 🔧 Sơ đồ khái niệm — timeline ngang 6 bước cụ thể
- **File ảnh:** `hinh_3_11.png` · **Hàm:** `hinh_3_11()` (dùng chung helper `_draw_chain_timeline`)
- **Trạng thái:** ✅ Đã tạo — kế thừa bản sửa lỗi từ Hình 3.10 (cùng helper), không phát sinh lỗi mới

**Spec kỹ thuật:**
- Cùng kiểu trình bày timeline với Hình 3.10 nhưng tông màu xanh lá (Niles:
  `color=C_GREEN, fc="#E5F2E8"`) để phân biệt và gợi liên hệ tộc, `figsize=(13.5, 3.6)`
- 6 bước lấy đúng theo ví dụ cụ thể tại §3.6.6 của tài liệu nguồn (có tên unit
  thật, không phải ví dụ tổng quát như 3.10):
  1. *"HP về 0 (unit mang Reborn)"*
  2. *"Kỹ năng 'khi chết' kích hoạt (Horus)"*
  3. *"Hồi sinh ngay với đúng 1 HP"*
  4. *"Kỹ năng 'khi hồi sinh' kích hoạt (Osiris)"*
  5. *"Kỹ năng 'khi có đồng minh mới' lan tỏa (Sobek)"*
  6. *"Unit trở lại hàng đợi — chuỗi khép lại"*
- Thêm 1 dòng chú thích in nghiêng riêng ở cuối (đặt qua `transform=ax.transAxes`,
  `y=-0.34`, nằm dưới khung axes — không chồng lên timeline), nhấn mạnh điểm
  thiết kế quan trọng nêu ở §3.3.3: *"unit vẫn được tính là 'đã chết hợp lệ' và
  kích hoạt trọn vẹn kỹ năng 'khi chết' TRƯỚC khi hồi sinh — Horus + Reborn
  nghĩa là phần thưởng kích hoạt hai lần chỉ từ một unit"*
- `ax.set_title("Hình 3.11 — Chuỗi Phản Ứng Khi Một Unit Reborn Gục Ngã")`

---

### Hình 3.12 — Phân Phối 68 Lá Bài Theo Tier
- **Vị trí placeholder:** `03_7_ThietKeGame_Balancing.md:32`
- **Loại:** 📊 Biểu đồ dữ liệu thực — cột chồng + cột đơn, có nhãn tổng
- **File ảnh:** `hinh_3_12.png` · **Hàm:** `hinh_3_12()`
- **Trạng thái:** ✅ Đã tạo — dùng số liệu thật trích từ `Assets/Resources/CardsData.json`

**Spec kỹ thuật:**
- **Nguồn dữ liệu thật** (đã trích xuất bằng Python, đối chiếu khớp 100% với số
  liệu nêu trong tài liệu — 47 unit + 21 spell = 68): `Assets/Resources/CardsData.json`
  ```
  Tier:        1   2   3   4   5   6
  Babylon:     4   3   3   6   4   3   (Σ 23)
  Niles:       5   4   4   4   4   3   (Σ 24)
  Unit total:  9   7   7  10   8   6   (Σ 47)
  Spell:       8   6   3   3   1   0   (Σ 21)
  Tổng/tier:  17  13  10  13   9   6   (Σ 68)
  ```
- Layout: `figsize=(10, 6)`, mỗi tier có 2 cột cạnh nhau (`bw=0.34`):
  - Cột trái = **Unit**, chồng 2 phần `Babylon` (xanh dương) + `Niles` (xanh lá)
  - Cột phải = **Spell** (cam) — riêng Tier 6 = 0, vẫn hiển thị nhãn `"0"` màu xám
- Nhãn số trên đỉnh mỗi cột con (tổng unit / tổng spell), và nhãn `"Σ N"` tổng
  cộng cả tier đặt cao hơn ở giữa cặp cột
- `ax.set_title` 2 dòng: tên hình + dòng tóm tắt số liệu tổng `"(47 unit: 23
  Babylon · 24 Niles + 21 spell = 68)"`
- Dòng nguồn dữ liệu in nghiêng xám ở góc dưới-trái:
  `"Nguồn dữ liệu: Assets/Resources/CardsData.json — 47 unit card (tribe =
  Babylon/Niles) + 21 spell card = 68 lá"`
- Lưới ngang nhạt (`C_GRID`), ẩn viền trên/phải, `legend` góc trên-phải không khung

> **Ghi chú nguồn dữ liệu:** lúc đầu kiểm tra các file staging trong
> `Document/02_Data/Cards*.json` cho ra tổng 46 unit + 20 spell = 66 — lệch 2 so
> với con số tài liệu công bố (47/21/68). Đã đối chiếu với file gộp chính thức
> trong game `Assets/Resources/CardsData.json` và xác nhận nó khớp chính xác
> 100% với số liệu trong tài liệu — dùng file này làm nguồn số liệu chính thức
> cho biểu đồ thay vì các file staging.

---

### Các hình còn lại của Chương 3 (📷 ảnh chụp màn hình — chưa làm)

| Hình | Tiêu đề | Vị trí | Loại | Ghi chú |
|---|---|---|---|---|
| 3.2 | Screenshot Màn Hình Chọn Độ Khó | `03_1_..._TamNhin.md:41` | 📷 Ảnh chụp màn hình | Cần chạy game, chụp UI chọn Easy/Medium/Hard |
| 3.4 | Giao Diện Shop Phase | `03_2_..._CoreLoop.md:77` | 📷 Ảnh chụp màn hình | Cần chạy game, chụp Shop Phase (7 ô shop, sân, HUD) |
| 3.5 | Giao Diện Combat Phase | `03_2_..._CoreLoop.md:90` | 📷 Ảnh chụp màn hình | Cần chạy game, chụp Combat Phase (2 sân đối mặt) |
| 3.6 | Giải Phẫu Lá Bài Unit | `03_3_..._CardSystem.md:22` | 📷 Ảnh chụp màn hình (chú thích) | Cần chụp 1 lá bài thật + vẽ overlay 8 chú thích thành phần |
| 3.13 | So Sánh Hai Trạng Thái Giao Diện | `03_8_..._UIUX.md:24` | 📷 Ảnh chụp màn hình | Cần 2 ảnh Shop/Combat đặt cạnh nhau + vẽ overlay khoanh vùng/mũi tên |
| 3.14 | Giải Phẫu Giao Diện Lá Bài | `03_8_..._UIUX.md:49` | 📷 Ảnh chụp màn hình (chú thích) | Cần chụp lá bài trong game (có frame blink merge hint) + overlay chú thích |

**Tổng Chương 3:** 7 sơ đồ khái niệm (✅ xong) · 1 biểu đồ dữ liệu thực (✅ xong) · 6 ảnh chụp màn hình (⬜ chưa làm — cần chạy game)

---

## 5. Chương 4 — Kiến Trúc Hệ Thống (3 hình) ⬜ chưa làm

| Hình | Tiêu đề | Vị trí | Loại | Ghi chú |
|---|---|---|---|---|
| 4.1 | Hình Dạng Kiến Trúc Khi Áp Dụng Ba Ràng Buộc | `04_KienTrucHeThong.md:31` | 🔧 Sơ đồ khái niệm | Sơ đồ 4 tầng dọc (Data→Core→Manager→UI) + nhánh AI tách biệt nối thẳng Core — vẽ trực tiếp được |
| 4.2 | Quy Trình Rút Ngăn Xếp Cái Chết | `04_KienTrucHeThong.md:81` | 🔧 Sơ đồ khái niệm | Flowchart vòng lặp lồng nhau (death stack), nhánh đỏ/xanh phân biệt loại bỏ/hồi sinh — vẽ trực tiếp được |
| 4.3 | Pipeline Huấn Luyện Khép Kín | `04_KienTrucHeThong.md:99` | 🔧 Sơ đồ khái niệm | Pipeline ngang 6 bước (script khởi động → ... → AIManager nạp 5 BotAgent) — vẽ trực tiếp được |

**Tổng Chương 4:** 3 sơ đồ khái niệm — toàn bộ có thể vẽ trực tiếp, không phụ thuộc số liệu

---

## 6. Chương 5 — Hệ Thống AI / Giải Thuật Di Truyền (8 hình) ✅ ĐÃ HOÀN THÀNH

Toàn bộ 8 hình (4 sơ đồ khái niệm 🔧 + 4 biểu đồ dữ liệu thực 📊) đã được lập
trình trong **`Document/04_Tools/generate_figures_ch5.py`** (chạy bằng
`python generate_figures_ch5.py` từ thư mục `04_Tools`, ảnh xuất ra
`Document/03_Figures/hinh_5_1.png` … `hinh_5_8.png`). Đã regenerate và soát lỗi
trực quan từng ảnh — sửa 4 lỗi bố cục được phát hiện trong quá trình review
(chi tiết trong từng mục).

> **Ghi chú nguồn dữ liệu thật dùng chung cho 5.3 + 5.8 (gene 5 bot):**
> `Assets/Resources/AI_Library.json` hiện tại trên đĩa thuộc một lần huấn luyện
> KHÁC (commit `36d9c1f` "final", `hardBot.fitness ≈ 21000`) — KHÔNG khớp với
> lần huấn luyện được trích dẫn ở mục 5.6/5.7 (`training_20260601_213435.csv`,
> `hardBot.fitness = 4764`). Đã dò git log và xác định commit **`6e1c9f6`**
> ("Tune GA training output", 2026-06-01 21:51) chứa đúng bộ gene của lần chạy
> đó (`hardBot.fitness = 4764.0`, khớp chính xác với CSV — cách thời điểm CSV
> bắt đầu chạy ~17 phút). Trích xuất bằng
> `git show 6e1c9f6:Assets/Resources/AI_Library.json`, nhúng trực tiếp 5 mảng
> 37 gene (`BOT_GENES`) vào script — dùng làm nguồn duy nhất cho cả 5.3 và 5.8
> để đảm bảo nhất quán nội bộ giữa hai hình.

---

### Hình 5.1 — Kiến Trúc Tổng Thể Hệ Thống AI: Bốn Thành Phần
- **Vị trí placeholder:** `05_1_GA_TongQuan_Chromosome.md:49`
- **Loại:** 🔧 Sơ đồ khái niệm
- **File ảnh:** `hinh_5_1.png` · **Hàm:** `hinh_5_1()`
- **Trạng thái:** ✅ Đã tạo — sạch ngay từ lần đầu, không phát hiện lỗi bố cục

**Spec kỹ thuật:**
- Layout: 1 axes hệ trục ảo, `figsize=(12.4, 11.2)`, `axis("off")`, 5 hộp xếp
  dọc tại `cx=3.55`, `ys=[9.2, 6.9, 4.6, 2.3, 0.0]`, mỗi hộp `bw, bh = 4.7, 1.5`
- 5 tầng pipeline nối bằng `draw_arrow` dọc, mỗi mũi tên kèm 1 nhãn ngắn bên
  trái mô tả dữ liệu truyền qua: `Chromosome (37 gene)` → *("bộ não" định hình
  chiến lược)* → `BotAgent · DecidePrepPhase()` → *(board state mỗi lượt shop)*
  → `GameSimulator · EvaluateMatch(...)` → *(fitness scores qua hàng trăm nghìn
  trận)* → `GATrainer · Init→Evaluate→Select→Crossover→Mutate` → *(ghi 5 bot đã
  chọn vào file)* → `AI_Library.json (5 bot chuyên biệt)`
- Mỗi hộp tô màu riêng (`tint(C_PURPLE/C_BLUE/C_ORANGE/C_GREEN/C_GRAY)`), kèm 1
  đoạn mô tả vai trò đặt bên phải hộp bằng chữ nghiêng xám
- `ax.set_title("Hình 5.1 — Kiến Trúc Tổng Thể Hệ Thống AI: Bốn Thành Phần")`

---

### Hình 5.2 — Biểu Đồ 37 Gene, Phân Theo 9 Nhóm Chức Năng
- **Vị trí placeholder:** `05_1_..._Chromosome.md:111`
- **Loại:** 🔧 Sơ đồ khái niệm (bán-data — dùng đúng tên/chỉ số 37 gene thật,
  đối chiếu bảng 9 nhóm trong essay `05_1_..._Chromosome.md:71-109`)
- **File ảnh:** `hinh_5_2.png` · **Hàm:** `hinh_5_2()`
- **Trạng thái:** ✅ Đã tạo — sạch ngay từ lần đầu, không phát hiện lỗi bố cục

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(13.6, 9.4)`, 9 hàng ngang xếp từ trên xuống — mỗi
  hàng = 1 nhóm chức năng (`GROUPS[1..9]`, mỗi nhóm 1 màu cố định trong bảng
  màu chuẩn `C_BLUE/C_PINK/C_RED/C_ORANGE/C_GREEN/C_PURPLE/C_TEAL/C_BROWN/C_OLIVE`)
- Mỗi hàng: 1 thẻ màu + nhãn `"Nhóm N — <tên nhóm> (<số gene> gene)"` bên trái,
  theo sau là dải ô vuông `draw_box`, mỗi ô ghi `"<chỉ số>\n<tên viết tắt gene>"`
  (vd. `"0\nwATK"`, `"21\nwMerge"`) tô màu nhạt (`tint(color, 0.78)`) cùng tông
  với thẻ nhóm — tổng cộng đúng 37 ô trải đều 9 hàng theo dữ liệu `GENES`
  (37 tuple `(chỉ_số, tên, nhóm)`) đối chiếu trực tiếp từ bảng essay
- Dòng chú thích trên cùng: `"37 ô — mỗi ô: chỉ số gene (trên) + tên viết tắt
  (dưới) — sắp theo thứ tự chỉ số trong từng nhóm"`
- `ax.set_title("Hình 5.2 — 37 Gene Của Chromosome, Phân Theo 9 Nhóm Chức Năng")`

---

### Hình 5.3 — Bảng Giá Trị 37 Gene Của 5 Bot (Heatmap + Biểu Đồ Trung Bình Nhóm)
- **Vị trí placeholder:** `05_1_..._Chromosome.md:148`
- **Loại:** 📊 Biểu đồ dữ liệu thực — heatmap bảng + biểu đồ cột nhóm
- **File ảnh:** `hinh_5_3.png` · **Hàm:** `hinh_5_3()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi colorbar đè lên tiêu đề panel dưới

**Spec kỹ thuật:**
- **Lựa chọn thiết kế khác placeholder gốc:** thay vì "bảng 37 hàng + biểu đồ
  cột giá trị gene thô" (sẽ rối mắt với 37×5 cột nhóm), dựng **heatmap 37 hàng
  × 5 cột bot** (màu = giá trị, có chú thích số `value:.2f` trong từng ô, màu
  chữ tự đổi trắng/xám theo độ đậm nền) **+ biểu đồ cột trung bình theo 9 nhóm**
  bên dưới — vừa giữ chi tiết từng gene vừa cho cái nhìn tổng hợp dễ so sánh
- Layout: `figsize=(11.6, 18.4)`, `GridSpec(2,1, height_ratios=[5.1, 1.0])`
  - Panel trên (`ax_hm`): heatmap, mỗi hàng có thẻ màu nhóm bên trái + nhãn
    `"[chỉ số] tên gene"`, vạch phân cách ngang nhạt giữa các nhóm gene
  - Panel dưới (`ax_bar`): biểu đồ cột nhóm — 9 nhóm × 5 bot, giá trị = trung
    bình các gene cùng nhóm chức năng cho mỗi bot
- `cmap = LinearSegmentedColormap("genes", ["#FFFFFF", C_BLUE])`, `vmin=0, vmax=1`
- Colorbar dọc đặt qua `fig.colorbar(ScalarMappable, ax=ax_hm, orientation="vertical",
  fraction=0.022, pad=0.015, shrink=0.5, aspect=24)` — neo theo `ax_hm`, không
  dùng toạ độ tuyệt đối của figure
- **Nguồn dữ liệu thật:** `BOT_GENES` trích từ commit `6e1c9f6` (xem ghi chú đầu mục)

> **Lỗi đã sửa:** bản đầu đặt colorbar bằng toạ độ tuyệt đối của figure
> (`fig.add_axes([0.30, 0.055 + 0.205, 0.42, 0.0085])`), dải màu nằm chồng trực
> tiếp lên dòng tiêu đề của panel biểu đồ cột bên dưới ("Giá trị gene trung bình
> theo từng nhóm…"), cả hai garbled lẫn nhau. Đã thay bằng
> `fig.colorbar(ScalarMappable(...), ax=ax_hm, orientation="vertical", fraction=0.022,
> pad=0.015, shrink=0.5, aspect=24)` — colorbar tự neo bên phải `ax_hm`, layout
> tự tính toán nên không còn va chạm giữa hai panel.

---

### Hình 5.4 — Bảy Phase Của DecidePrepPhase: Hành Vi Và Gene Chi Phối
- **Vị trí placeholder:** `05_2_...Trainer.md:9`
- **Loại:** 🔧 Sơ đồ khái niệm (đối chiếu bảng 7-phase essay `05_2_...Trainer.md:45-53`)
- **File ảnh:** `hinh_5_4.png` · **Hàm:** `hinh_5_4()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi chú thích điều kiện gene đè lên hộp phase kế tiếp

**Spec kỹ thuật:**
- Layout: 1 axes, `figsize=(11.6, 17.4)`, 7 hộp `FancyBboxPatch` xếp dọc tại
  `cx`, `ys = [16.0 − i·2.55 for i in range(7)]`, mỗi hộp `bw, bh = 6.8, 1.85`,
  tô `tint(màu_phase, 0.84)`, nối nhau bằng `draw_arrow` sát mép hộp
- Mỗi hộp chứa **3 dòng văn bản đặt bên trong** (không phải caption rời bên
  ngoài): tiêu đề `"① RerollPhase"` (đậm, cỡ 11.6, vị trí `y + 0.52`), điều kiện
  gene chi phối (cỡ 7.9, `y − 0.32`, vd. `"Reroll nếu shop kém hơn board ×
  genes[24]\ntối đa genes[25]×3+1 lần · giữ ≥ genes[26] coin"`), và kết quả
  đầu ra in nghiêng đậm bên phải hộp cùng màu phase (vd. `"→ shop mới hoặc giữ nguyên"`)
- 7 phase theo đúng thứ tự essay: ① RerollPhase · ② BuyUnitsPhase ·
  ③ BuySpellsPhase · ④ ProactiveSellPhase · ⑤ TryMerge · ⑥ RepositionPhase ·
  ⑦ FreezePhase — mỗi phase 1 màu riêng trong bảng màu chuẩn
- Caption mở đầu phía trên (tóm tắt thứ tự ưu tiên logic) và caption kết phía
  dưới `"[Kết thúc lượt chuẩn bị — board sẵn sàng cho ResolveTurn() ở GameSimulator]"`

> **Lỗi đã sửa:** bản đầu đặt chú thích điều kiện gene (2 dòng) BÊN NGOÀI mỗi
> hộp, ngay phía dưới (`y − bh/2 − 0.62`) — với khoảng cách giữa các hộp
> (`ys` bước `1.95`, `bh=1.35`) chỉ ~0.7 đơn vị, caption 2 dòng tràn xuống đè
> lên cạnh trên của hộp kế tiếp (vd. "Reroll nếu shop kém hơn board × genes[24]"
> đè lên hộp `BuyUnitsPhase`, "Bán unit có điểm dưới genes[27]×3" đè lên
> `TryMerge`, "logic tất định — KHÔNG phụ thuộc gene" đè lên `RepositionPhase`).
> Đã viết lại toàn bộ: hộp cao hơn (`bh: 1.35→1.85`), bước giãn cách lớn hơn
> (`step: 1.95→2.55`), và **dời chú thích vào BÊN TRONG hộp** (tiêu đề + điều
> kiện + kết quả đều nằm trong viền hộp, mũi tên nối khít mép hộp liền kề) —
> loại bỏ hoàn toàn khả năng chồng chữ giữa các phase.

---

### Hình 5.5 — Sự Mở Rộng Của Hàm Fitness Qua Các Vòng Tinh Chỉnh
- **Vị trí placeholder:** `05_2_...Trainer.md:133`
- **Loại:** 🔧 Sơ đồ khái niệm — 2 panel đối chiếu trực quan
- **File ảnh:** `hinh_5_5.png` · **Hàm:** `hinh_5_5()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi 2 dòng chú thích chồng lên nhau ở panel trái

**Spec kỹ thuật:**
- Layout: `1×2 subplots`, `figsize=(14.4, 7.2)`, mỗi panel hệ trục ảo `xlim(0,10)
  ×ylim(-1.1,10.4)`, `axis("off")`
- **Panel trái — "Phiên bản đầu — chỉ thắng/thua":** 3 cột đơn giản
  (`THẮNG=10` xanh lá, `HÒA=2` xám, `THUA=0` đỏ — chiều cao tỉ lệ giá trị),
  mũi tên đỏ chỉ xuống hộp kết luận `"Quần thể hội tụ nhanh về một chiến lược
  RUSH một chiều (áp đảo early-game — sụp đổ trước đối thủ biết tích lũy & scale)"`
- **Panel phải — "Phiên bản hiện hành — sáu tín hiệu cộng dồn, ba nhóm":** 1 cột
  chia 3 đoạn xếp chồng theo tỉ lệ trọng số trực quan (`seg_h = [4.55, 1.55, 0.85]`,
  màu xanh lá/xanh dương/cam), mỗi đoạn ghi rõ công thức + trọng số nhóm tín
  hiệu (`① Kết quả trận đấu — nền 300/100/10`, `② Biên độ HP tức thời — hpA×8 −
  hpB×4`, `③–⑥ Sức mạnh bàn cờ + chất lượng đội hình — trọng số nhỏ
  0.06/0.04/0.035/0.025`), dấu `"+"` xám giữa các đoạn minh hoạ phép cộng dồn,
  mũi tên xanh lá chỉ xuống hộp kết luận đối lập với panel trái
- Mỗi panel có 1 đoạn caption in nghiêng ngắn ngay dưới tiêu đề panel, tóm tắt
  ý nghĩa thiết kế (bên trái: "tín hiệu trung thực nhất với luật chơi — nhưng
  chỉ phân biệt được KẾT QUẢ…"; bên phải: "mỗi nhóm đo một khía cạnh khác nhau…")

> **Lỗi đã sửa:** bản đầu có THÊM một dòng chú thích phụ
> `"phân phối điểm rời rạc — chỉ ba giá trị khả dĩ"` đặt tại
> `axL.text(5, base_y + max_h + 1.15, ...)` ở panel trái — toạ độ này (y≈9.15)
> rơi đúng vào vùng chiếm chỗ của đoạn caption 2 dòng phía trên panel (đặt ở
> y=9.1), khiến hai khối chữ chồng lẫn không đọc được. Vì nội dung dòng này đã
> được truyền tải qua caption phía trên + chính 3 cột trực quan + hộp kết luận
> bên dưới, đã **loại bỏ hẳn dòng chú thích thừa** thay vì cố định lại vị trí —
> giảm rối mắt và dứt điểm chồng chữ.

---

### Hình 5.6 — Đường Cong Fitness Qua 180 Thế Hệ (Production Run)
- **Vị trí placeholder:** `05_2_...Trainer.md:265`
- **Loại:** 📊 Biểu đồ dữ liệu thực — đường Best/Avg/Worst + Std Dev trục phụ
- **File ảnh:** `hinh_5_6.png` · **Hàm:** `hinh_5_6()`
- **Trạng thái:** ✅ Đã tạo — sạch ngay từ lần đầu, không phát hiện lỗi bố cục

**Spec kỹ thuật:**
- **Nguồn dữ liệu thật:** `Document/02_Data/Train/training_20260601_213435.csv`
  (180 dòng, gen 0–179) qua `load_training_csv()` — đọc bằng `csv.DictReader`
- 3 đường trục trái (`ax1`, nhãn `"Điểm fitness"`): `Worst` (xám, mảnh, alpha
  thấp), `Avg` (xanh dương), `Best` (đỏ đậm `#B71C1C`, dày nhất) — `ylim(0, 5300)`
- 1 đường trục phải (`ax2 = ax1.twinx()`, nhãn `"Std Dev"`): nét chấm-gạch cam
  `ls="-."`, `ylim(0, 1100)`, tick label màu cam đồng bộ
- 2 chú thích chính kèm mũi tên `annotate`:
  - `axvline` đỏ tại gen=6 + nhãn `"Gen 6: Best đạt đỉnh 4764 (Hall of Fame xác
    định rất sớm)"` — khớp giá trị `hardBot.fitness=4764` từ commit `6e1c9f6`
  - `axvspan` vùng gen 18–22 tô olive nhạt + nhãn `"vùng pct_other tăng vọt
    (gen 19→20: 43%→48%)"` (đã đối chiếu đúng số liệu CSV cột `pct_other`)
- Legend gộp cả 2 trục (`h1+h2, l1+l2`) đặt góc dưới-phải, `frameon=False`
- Dòng nguồn dữ liệu in nghiêng góc dưới: tên file CSV + tham số huấn luyện
  (`population=120 · generations=180 · matches/chrom=20`)

---

### Hình 5.7 — Phân Phối Bộ Tộc (Tribe) Qua 180 Thế Hệ
- **Vị trí placeholder:** `05_2_...Trainer.md:271`
- **Loại:** 📊 Biểu đồ dữ liệu thực — area chart xếp chồng (stackplot)
- **File ảnh:** `hinh_5_7.png` · **Hàm:** `hinh_5_7()`
- **Trạng thái:** ✅ Đã tạo — đã sửa lỗi chú thích văn bản đè lên đỉnh dữ liệu

**Spec kỹ thuật:**
- **Nguồn dữ liệu thật:** cùng CSV với 5.6, 3 cột `pct_babylon/pct_niles/pct_other`
- `ax.stackplot(gen, pct_b, pct_n, pct_o, colors=["#F0A030", C_BLUE, "#BBBBBB"],
  alpha=0.85, edgecolor="white")` — Babylon (cam, đáy) → Niles (xanh dương,
  giữa) → Other/generalist (xám, đỉnh)
- `xlim(0, 179)`, `ylim(0, 119)` (có khoảng đệm phía trên mốc 100% — xem mục lỗi),
  `yticks` cố định ở `[0, 20, 40, 60, 80, 100]` để giữ đúng ý nghĩa phần trăm
- Chú thích góc trên-trái (trong `ax.transAxes`, `va="top"`) tóm tắt biên độ
  dao động từng tộc: `"Babylon: 6.7%–53.3% (TB ~30%) · Niles: 22.5%–70.0% (TB
  ~43%) · Other: 3.3%–68.3%"` + dòng `"Không bộ tộc nào tuyệt chủng — elitism +
  immigration giữ tối thiểu cho cả ba"`
- Legend 3 màu ở dưới biểu đồ (`bbox_to_anchor=(0.5, -0.16), ncol=3`), dòng
  nguồn dữ liệu in nghiêng cuối trang

> **Lỗi đã sửa:** bản đầu đặt `ylim(0, 100)` đúng bằng miền giá trị dữ liệu —
> chú thích góc trên-trái (2 dòng, neo `va="top"` sát mép trên trục) vì vậy đè
> trực tiếp lên đỉnh sóng của dải Niles (xanh dương) mỗi khi `pct_b + pct_n`
> tiệm cận ~95% (rõ nhất quanh gen 20–50, chữ "elitism" cắt ngang đỉnh sóng
> xanh). Vì dữ liệu phần trăm có tổng cố định = 100 nên không thể "né" bằng
> cách dịch chuyển trong miền 0–100 — đã **mở rộng `ylim` lên `(0, 119)`** để
> tạo khoảng đệm trống phía trên 100% dành riêng cho chú thích, đồng thời cố
> định `yticks=[0,20,...,100]` để trục vẫn chỉ hiển thị các mốc phần trăm có
> ý nghĩa (không lộ vạch 120 thừa).

---

### Hình 5.8 — Radar Chart: Hồ Sơ Gene 8-Trục Của 5 Bot Được Chọn
- **Vị trí placeholder:** `05_2_...Trainer.md:293`
- **Loại:** 📊 Biểu đồ dữ liệu thực — radar/polar chart, 5 đường chồng
- **File ảnh:** `hinh_5_8.png` · **Hàm:** `hinh_5_8()`
- **Trạng thái:** ✅ Đã tạo — sạch ngay từ lần đầu, không phát hiện lỗi bố cục

**Spec kỹ thuật:**
- Layout: `figsize=(9.2, 9.6)`, `subplot_kw=dict(polar=True)`,
  `theta_offset=π/2`, `theta_direction=-1` (trục đầu tiên ở đỉnh, chiều kim đồng hồ)
- **8 trục** = trung bình gene theo nhóm chức năng, **rút gọn từ 9 nhóm essay**
  (gộp nhóm 9 "Trigger con" vào nhóm 3 "Trigger" thành 1 trục `Trigger` — lý do:
  9 trục sẽ quá dày đặc, khó đọc trên radar): `Stat · Keywords · Trigger ·
  Effect · Tribe · Board · Reroll · Spell` — định nghĩa trong `RADAR_AXES`
  (mapping chỉ số gene → trục, vd. `Trigger = [7,8,9,10,11,12,32,33,34,35,36]`)
- 5 đường (`BOT_ORDER`), mỗi đường 1 màu cố định (`BOT_COLORS = ["#37474F"
  (hardBot, xám-xanh đậm), "#F0A030" (babylonBot, cam), C_BLUE (nileBot),
  C_PURPLE (summonerBot), C_GREEN (resilientBot)]`), tô nền nhạt `alpha=0.07`
- `ylim(0, 1.0)`, `yticks=[0.25, 0.5, 0.75, 1.0]`
- **Nguồn dữ liệu thật:** `BOT_GENES` trích từ commit `6e1c9f6` — dùng chung
  nguồn với hình 5.3 (xem ghi chú đầu mục)
- Caption cuối trang ghi rõ cách rút gọn trục + nguồn: `"Mỗi trục = trung bình
  các gene cùng nhóm chức năng (rút gọn từ 9 nhóm — nhóm 9 'Trigger con' gộp
  vào 'Trigger'). Nguồn: AI_Library.json @ commit 6e1c9f6"`

---

## 7. Chương 6 — Kết Quả Đánh Giá (2 hình) ⬜ chưa làm

| Hình | Tiêu đề | Vị trí | Loại | Ghi chú |
|---|---|---|---|---|
| 6.1 | Màn Hình Shop Phase (Mid-Game) | `06_KetQua_DanhGia.md:17` | 📷 Ảnh chụp màn hình | Cần chạy game thật tới giữa ván, chụp Shop Phase với HUD đầy đủ (HP=5, Coin=7, Turn=8, Cups=3) |
| 6.2 | Combat Phase | `06_KetQua_DanhGia.md:23` | 📷 Ảnh chụp màn hình | Cần chụp đúng khoảnh khắc combat đang diễn ra, có animation nhận damage (flash đỏ), turn 3/20 |

**Tổng Chương 6:** 2 ảnh chụp màn hình — đòi hỏi build/chạy game và canh đúng khoảnh khắc gameplay cụ thể (số liệu HUD phải khớp mô tả)

---

## 8. Việc cần làm tiếp theo (gợi ý thứ tự ưu tiên)

1. ✅ ~~Chương 2 (7 hình, toàn bộ sơ đồ khái niệm)~~ — đã xong
2. ✅ ~~Chương 3 phần sơ đồ khái niệm + dữ liệu thật (3.1, 3.3, 3.7, 3.8, 3.9,
   3.10, 3.11, 3.12 — 8 hình)~~ — đã xong (script `generate_figures_ch3.py`,
   đã soát lỗi trực quan, sửa 6 lỗi bố cục/logic)
3. **Chương 4** (3 hình, toàn bộ sơ đồ khái niệm, không phụ thuộc số liệu) —
   cùng dạng với Chương 2/3, có thể tái dùng helper có sẵn
4. ✅ ~~Chương 5 (8 hình: 4 sơ đồ khái niệm + 4 biểu đồ dữ liệu thực)~~ — đã
   xong (script `generate_figures_ch5.py`, đã soát lỗi trực quan, sửa 4 lỗi bố
   cục — chi tiết trong từng mục 5.1–5.8; dữ liệu gene 5 bot trích từ commit
   `6e1c9f6` thay vì `AI_Library.json` hiện tại trên đĩa — xem ghi chú đầu mục Chương 5)
5. **Cụm ảnh chụp màn hình** (8 hình: 3.2, 3.4, 3.5, 3.6, 3.13, 3.14, 6.1, 6.2)
   — làm sau cùng vì đòi hỏi build/chạy game và canh đúng khoảnh khắc/số liệu HUD
   khớp với mô tả trong placeholder
