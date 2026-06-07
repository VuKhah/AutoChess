## 3.4 Hệ Thống Shop

Shop là giao diện giữa người chơi và sự ngẫu nhiên — bộ lọc kiểm soát những lá bài nào có thể xuất hiện, theo tỉ lệ nào.

---

### 3.4.1 Kiến Trúc Pool Và Weighted Random

Toàn bộ 47 unit card (không gồm token) tạo thành một pool đơn, chia tầng theo `tier`. Mỗi lượt, 5 ô unit và 2 ô spell được roll **độc lập** — cùng card có thể xuất hiện nhiều lần trong cùng một shop, cho phép hoàn thành bộ merge mà không cần reroll nhiều lần.

Cơ chế trung tâm là một phép quay ngẫu nhiên có trọng số theo bảng tỉ lệ 6×6 dưới đây — mỗi shop level ứng với một phân phối xác suất riêng giữa 6 tier:

| Shop Level | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 | Tier 6 |
|:----------:|:------:|:------:|:------:|:------:|:------:|:------:|
| 1          | 100%   | 0%     | 0%     | 0%     | 0%     | 0%     |
| 2          | 70%    | 30%    | 0%     | 0%     | 0%     | 0%     |
| 3          | 50%    | 35%    | 15%    | 0%     | 0%     | 0%     |
| 4          | 25%    | 40%    | 25%    | 10%    | 0%     | 0%     |
| 5          | 15%    | 25%    | 35%    | 15%    | 10%    | 0%     |
| 6          | 10%    | 15%    | 20%    | 25%    | 20%    | 10%    |

Cơ chế hoạt động đơn giản như một vòng quay số được chia thành các lát cắt theo đúng tỉ lệ trong bảng: hệ thống rút một số ngẫu nhiên, xác định số đó rơi vào "lát cắt" tier nào — tier có tỉ lệ cao hơn chiếm lát cắt rộng hơn, do đó dễ "trúng" hơn một cách tự nhiên — rồi chọn ngẫu nhiên đồng đều một lá bài thuộc đúng tier đó từ kho bài. Nếu vì lý do nào đó không còn lá bài nào thuộc tier vừa quay trúng, hệ thống tự động lùi xuống tier liền kề thấp hơn — đảm bảo shop không bao giờ xuất hiện một ô trống.

**Đọc bảng:** Tier 1 vẫn còn 10% ở shop level 6 — người chơi có thể tìm bản sao merge unit thấp tier ở late-game. Tier 6 chỉ xuất hiện 10% ở shop level tối đa — phần thưởng hiếm cho người chơi sống sót đủ lâu.

---

### 3.4.2 Đóng Băng Shop (Freeze)

Freeze tốn **0 coin** nhưng có chi phí ẩn: từ bỏ quyền xem 7 lá mới đầu lượt tiếp theo. Khi người chơi chọn freeze trước lúc bước vào combat, shop ở đầu lượt kế tiếp sẽ không làm mới toàn bộ — chỉ những ô đã được mua (nay đang trống) mới được lấp đầy bằng card mới theo đúng phân phối tier hiện hành, còn những ô chưa mua vẫn giữ nguyên lá bài cũ. Trạng thái đóng băng này chỉ tồn tại trong đúng một lượt rồi tự động được gỡ bỏ.

Nên freeze khi thấy lá bài cần thiết nhưng chưa đủ coin; không nên freeze khi shop hiện tại không có gì đặc biệt. Reroll trong lượt hiện tại hủy freeze — không thể vừa freeze vừa reroll.

---

### 3.4.3 Buff Toàn Bộ Tộc (Global Tribe Buff)

Một quy tắc thiết kế đơn giản nhưng mang sức ảnh hưởng lâu dài xuyên suốt cả ván đấu: **mọi lá bài mới xuất hiện trong shop đều tự động được cộng toàn bộ phần buff vĩnh viễn theo bộ tộc đã từng được kích hoạt trước đó trong trận** — ngay tại thời điểm nó vừa hiện ra, không cần đợi đến lúc người chơi mua hay triển khai.

Hệ quả thiết kế ở đây khá tinh tế: nếu Thoth đã từng kích hoạt buff +2 ATK cho toàn bộ tộc Niles, thì mọi unit Niles *xuất hiện trong shop kể từ thời điểm đó trở đi* sẽ hiện ra với chỉ số đã được cộng sẵn — người chơi nhìn thấy đúng con số mình sẽ nhận được, không phải tự nhẩm tính thêm. Nói cách khác, dù Thoth có gục ngã rất sớm trong trận, giá trị của nó không hề biến mất: nó đã "khóa" phần thưởng +2 ATK vào mọi unit Niles sẽ xuất hiện và được mua cho đến tận cuối ván đấu.

---

*[Tiếp theo: Mục 3.5 — Hệ Thống Kinh Tế]*
