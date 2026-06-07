## 3.6 Hệ Thống Chiến Đấu (Combat System)

### 3.6.1 Bố Cục Sân — Frontline Và Backline

Sân chiến đấu có **7 slot** (0–6), chia thành hai vùng:

```
 ┌───┬───┬───┬───┬───┬───┬───┐
 │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │  ← slot index
 └───┴───┴───┴───┴───┴───┴───┘
  ◄─── FRONTLINE ───►◄─ BACK ─►
     slot 0–3 (4 ô)   slot 4–6 (3 ô)
```

> **[HÌNH 3.8 — Sơ Đồ Sân Chiến Đấu 7 Slot]** *Sân 7 slot với hai vùng màu khác nhau: Frontline (slot 0–3) và Backline (slot 4–6). Mũi tên minh họa hướng tấn công: địch nhắm frontline trước, chỉ vào backline khi frontline trống.*

**Frontline (0–3):** Luôn lộ ra trước đòn địch.  
**Backline (4–6):** Chỉ bị nhắm khi toàn bộ frontline đã bị tiêu diệt (kể cả sau Reborn và unit được triệu hồi vào frontline trong combat).

Phân chia 4:3 là có chủ ý: phần lớn đội hình ở frontline, backline là vị trí đặc quyền cho unit có chỉ số thấp nhưng ability quan trọng (Thoth, Osiris…).

---

### 3.6.2 Thứ Tự Tấn Công — Attack Queue

Mỗi round, **tất cả unit còn sống** tấn công một lần theo thứ tự slot tăng dần, enemy trước player ở cùng slot:

```
Slot 0 (enemy) → Slot 0 (player) → Slot 1 (enemy) → Slot 1 (player) → ... → Slot 6
(Slot trống hoặc ATK = 0 → bỏ qua)
```

> **[HÌNH 3.9 — Thứ Tự Tấn Công Trong Một Round]** *Sơ đồ hai sân đối mặt, số thứ tự 1–14 đánh theo hàng đợi. Slot trống gạch chéo xám.*

Thứ tự tấn công là **động**: unit mới triệu hồi hoặc hồi sinh trong round được chèn vào đúng vị trí slot trong phần hàng đợi chưa xử lý — bảo toàn thứ tự nhất quán dù board thay đổi liên tục.

---

### 3.6.3 Cơ Chế Va Chạm — Clash Đồng Thời

Mỗi lần giao tranh — gọi là một **Clash** — diễn ra hoàn toàn đồng thời: bên tấn công và bên phòng thủ cùng nhận sát thương bằng đúng chỉ số tấn công của đối phương, tại cùng một thời điểm (trừ khi Safeguard chặn được một phía). Không tồn tại khái niệm "ai ra đòn trước" trong một Clash, cũng không có yếu tố trượt đòn hay chí mạng — đây là lựa chọn thiết kế có chủ đích nhằm loại bỏ hoàn toàn yếu tố may rủi mang tính thời điểm, chỉ giữ lại lợi thế đến từ chỉ số và kỹ năng. Nhờ vậy, kết quả combat hoàn toàn xác định (deterministic) từ trạng thái hai sân ngay trước khi giao tranh bắt đầu — và đây là điều kiện tiên quyết để một AI có thể "học" để chơi tốt: nếu kết quả phụ thuộc vào yếu tố may rủi không kiểm soát được, không có gì để học cả.

Triggers phụ sau clash: `OnAttack` chỉ kích hoạt nếu attacker **còn sống** sau đòn; `OnTakeDamage` chỉ kích hoạt nếu unit nhận damage **còn sống** sau khi nhận.

---

### 3.6.4 Chọn Mục Tiêu — Ba Tầng Ưu Tiên

Khi đến lượt tấn công, một unit không chọn mục tiêu một cách tùy ý — nó tuân theo một trật tự ưu tiên cố định gồm ba tầng, áp dụng tuần tự:

| Tầng ưu tiên | Áp dụng khi | Mục tiêu được chọn |
|:---:|---|---|
| 1 — Taunt | Phía đối phương còn ít nhất một unit mang Taunt | Bắt buộc nhắm vào unit Taunt gần nhất — **bỏ qua hoàn toàn** ranh giới hàng đầu/hàng sau |
| 2 — Hàng đầu (Frontline) | Không có Taunt nào còn sống | Unit còn sống ở hàng đầu (slot 0–3), tính theo khoảng cách vị trí gần nhất |
| 3 — Hàng sau (Backline) | Toàn bộ hàng đầu đã bị hạ gục | Unit còn sống ở hàng sau (slot 4–6), gần nhất |

Điểm đáng chú ý nhất trong ba tầng này là tầng 1: Taunt có quyền "vượt rào" hoàn toàn — một unit mang Taunt đứng ở hàng sau vẫn sẽ bị nhắm tới đầu tiên, dù toàn bộ hàng đầu phía trước nó vẫn còn nguyên vẹn. Đây chính là cơ sở cho chiến thuật đặt một unit trụ (tank) mang Taunt ở vị trí an toàn phía sau, biến nó thành "lá chắn từ xa" cho cả đội hình — một lớp chiều sâu chiến thuật chỉ có thể nhận ra khi đã hiểu rõ thứ tự ưu tiên này.

---

### 3.6.5 Khi Một Unit Gục Ngã — Chuỗi Phản Ứng Dây Chuyền

Trong một đội hình được xây dựng quanh các kỹ năng phản ứng theo sự kiện — "khi đồng minh chết", "khi hồi sinh", "khi có đồng minh mới xuất hiện"... — một cái chết hiếm khi là một sự kiện đơn lẻ. Nó thường là điểm khởi đầu của cả một **chuỗi phản ứng dây chuyền**: unit A gục ngã → kích hoạt kỹ năng ban Reborn cho đồng minh yếu nhất (Anubis) → đồng minh đó hồi sinh → kích hoạt kỹ năng nhân bội chỉ số (Osiris) ngay trên unit vừa hồi sinh → trong tích tắc, một unit tầm thường trở thành mối đe dọa lớn nhất sân đấu.

Nguyên tắc thiết kế xuyên suốt cho mọi tình huống như vậy chỉ gói gọn trong một câu: **một chuỗi phản ứng, khi đã bắt đầu, luôn được giải quyết trọn vẹn từ đầu đến cuối trước khi trận đấu tiếp tục diễn ra bình thường** — bất kể chuỗi đó dài bao nhiêu bước, hay có bao nhiêu unit cùng gục ngã trong cùng một khoảnh khắc. Nhờ vậy, người chơi luôn quan sát được toàn bộ hệ quả của một cái chết theo đúng trật tự nhân–quả, không bao giờ bị "cắt ngang" nửa chừng bởi một sự kiện không liên quan xen vào.

> **[HÌNH 3.10 — Một chuỗi phản ứng dây chuyền điển hình]** *Sơ đồ dòng thời gian: Unit A gục ngã → kỹ năng "khi đồng minh chết" kích hoạt → Unit B được ban Reborn → B hồi sinh → kỹ năng nhân bội kích hoạt trên B → chuỗi khép lại, trận đấu tiếp tục bình thường.*

Giữ vững được nguyên tắc "giải quyết trọn vẹn trước khi tiếp tục" — trong khi vẫn ngăn chặn được các vòng lặp vô hạn khi nhiều chuỗi phản ứng chồng lấn hoặc tự kích hoạt lẫn nhau (chính là "bài toán đệ quy" đã đặt ra ở mục 2.3.3) — là một trong những thử thách kỹ thuật khó nhất của toàn bộ hệ thống chiến đấu. Cơ chế cụ thể đứng sau nó, cùng câu chuyện về quá trình thử–sai để đi đến lời giải cuối cùng, được trình bày ở Chương 4.

---

### 3.6.6 Reborn Trong Combat — Một Ví Dụ Cụ Thể Về Chuỗi Phản Ứng

Để cụ thể hóa nguyên tắc vừa nêu ở mục 3.6.5, hãy xét trường hợp một unit mang Reborn gục ngã giữa một đội hình Niles đầy đủ thành phần — đây là chuỗi phản ứng dài và giàu kịch tính nhất mà hệ thống có thể tạo ra. Reborn, xét đến cùng, không phải "miễn nhiễm với cái chết" mà là "chết một cách hợp lệ, rồi được trao thêm một cơ hội": unit vẫn kích hoạt trọn vẹn kỹ năng "khi chết" của nó (ví dụ Horus ban buff cho toàn đội) *trước khi* hồi sinh trở lại với đúng 1 HP — và ngay khoảnh khắc hồi sinh ấy lại tiếp tục là một sự kiện mới, có thể kích hoạt kỹ năng của các đồng minh khác (Osiris nhân đôi/ba chỉ số unit vừa hồi sinh, Sobek tự cộng chỉ số khi có đồng minh "xuất hiện" trên sân) — trước khi unit quay trở lại hàng đợi tấn công và tiếp tục chiến đấu với 1 HP.

> **[HÌNH 3.11 — Chuỗi phản ứng khi một unit Reborn gục ngã]** *Timeline ngang minh họa trọn vẹn trình tự: HP về 0 → kỹ năng "khi chết" kích hoạt (Horus) → hồi sinh với 1 HP → kỹ năng "khi hồi sinh" kích hoạt (Osiris) → kỹ năng "khi có đồng minh mới" lan tỏa (Sobek) → unit trở lại hàng đợi, chuỗi khép lại.*

Unit hồi sinh với HP=1 — một đòn bất kỳ đủ để hạ lần thứ hai. Đây là counterplay rõ ràng: giữ unit ATK đủ để one-shot HP=1 sau Reborn.

---

### 3.6.7 Hòa Và Tổng Hợp

Sau **50 round**, nếu cả hai phía vẫn còn unit sống: kết quả **hòa** — không ai mất HP. Giới hạn 50 được chọn để rất hiếm khi đạt được trong gameplay bình thường (trận đầy đủ 7v7 thường kết thúc trong 10–20 round).

Toàn bộ hệ thống chiến đấu xây dựng trên một nguyên tắc nhất quán: **mọi mechanic tạo ra quyết định** cho người chơi trong Shop Phase — bố cục sân, thứ tự tấn công, target selection, Clash đồng thời, Death Stack LIFO, Reborn timing. Không có may mắn trong combat (không miss, không critical) — kết quả hoàn toàn deterministic từ trạng thái hai sân. Người chơi học được pattern và đưa ra quyết định shop tốt hơn qua nhiều ván.

---

*[Tiếp theo: Mục 3.7 — Cân Bằng Game (Balancing)]*
