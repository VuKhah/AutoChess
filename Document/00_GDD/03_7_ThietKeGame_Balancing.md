## 3.7 Cân Bằng Game (Balancing)

Cân bằng không có nghĩa là mọi lá bài có sức mạnh bằng nhau — nó có nghĩa là độ mạnh yếu được phân bổ theo chiều sâu chiến lược, không theo chiều "tốt hơn hoàn toàn".

---

### 3.7.1 Phân Phối Bài Theo Tier

**Unit cards (47 lá):**

| Tier | Babylon | Niles | Tổng unit | Tỉ lệ |
|:----:|:-------:|:-----:|:---------:|:-----:|
| 6    | 3       | 3     | 6         | 12.8% |
| 5    | 4       | 4     | 8         | 17.0% |
| 4    | 6       | 4     | 10        | 21.3% |
| 3    | 3       | 4     | 7         | 14.9% |
| 2    | 3       | 4     | 7         | 14.9% |
| 1    | 4       | 5     | 9         | 19.1% |

**Spell cards (21 lá):**

| Tier | Spell | Ghi chú |
|:----:|:-----:|:--------|
| 1    | 8     | 7 non-token + 1 token (Tinh Hoa Hợp Nhất) |
| 2    | 6     | Bao gồm spell kinh tế chính (Caishen's Knock) |
| 3    | 3     | Rising Spirit, Ritual, Divine Inspiration |
| 4    | 3     | Olympic Flame, Gate of Destruction, Devil's Deal |
| 5    | 1     | Military Support — spell đắt nhất (5 coin) |

Tier 4 đông nhất (21.3%) — vùng mid-game phong phú, trùng với giai đoạn người chơi định hình đội hình cuối (shop level 4–5, drop rate 25–40%). Tier 5–6 ít unit có chủ ý: pool nhỏ → xác suất ra đúng unit mong muốn cao hơn khi tier đủ cao để roll vào.

> **[HÌNH 3.12 — Phân Phối 68 Lá Bài Theo Tier]** *Biểu đồ cột đôi: unit (47 lá, phân tầng Babylon/Niles) và spell (21 lá). Trục hoành Tier 1–6, tổng số mỗi tier ghi trên đầu cột.*

---

### 3.7.2 Triết Lý Chỉ Số — Stat Không Nói Lên Tất Cả

Tier của unit xác định *khi nào* nó mạnh nhất, không phải *tại sao* nó mạnh. Nhiều unit tier cao có base stat thấp — sức mạnh đến từ ability:

| Unit | Tribe | Tier | ATK | HP | Nguồn sức mạnh thực sự |
|------|-------|:----:|:---:|:--:|------------------------|
| Osiris | Niles | 6 | 3 | 3 | ScaleTargetStats khi OnAllyReborn — nhân đôi/ba/bốn chỉ số |
| Anubis | Niles | 6 | 5 | 5 | Grant Reborn mỗi khi đồng minh chết (≤2/4/6 lần) |
| Sobek | Niles | 5 | 2 | 2 | +1/+2 vĩnh viễn mỗi lần đồng minh summon/reborn |
| Enki | Babylon | 5 | 2 | 15 | Taunt + phản dame 5/10/15 khi bị đánh |
| Humbaba | Babylon | 4 | 7 | 1 | Nhận 2 coin mỗi khi tấn công |
| Pazuzu | Babylon | 3 | 1 | 2 | Nhận 2 coin khi deploy |

*Osiris (3/3 tier 6)* là ví dụ cực đoan: raw stat bằng unit tier 2, nhưng mỗi Reborn đồng minh → nhân đôi/ba chỉ số unit đó ngay lập tức. Sức mạnh đến từ hệ sinh thái xung quanh, không từ bản thân.

---

### 3.7.3 Combo Mạnh Và Lever Kiềm Chế

**Combo 1 — Taunt + Reborn (Kingu):**
Kingu (tier 2, 2/2, Taunt + Reborn) buộc địch tốn 2 round để hạ một unit tier 2. Tự cân bằng bẩm sinh: ở late-game khi địch có ATK 15+, 1 HP sau Reborn bị hạ trong đúng một đòn tiếp theo. Mạnh ở early-mid, tự yếu dần ở late — không cần điều chỉnh số liệu.

**Combo 2 — Anubis + Reborn + Osiris (Niles Reborn Chain):**
```
Đồng minh chết → Anubis: Grant Reborn cho LowestHealthAlly
→ Ally đó chết → Reborn (HP=1)
→ Osiris: ScaleTargetStats(x2/x3/x4) lên unit vừa hồi sinh
→ Unit từ trung bình → cực mạnh trong 1 round
```
Hai lever cân bằng: (1) `triggerLimit = 2 + isScaledTriggerLimit` — Anubis chỉ grant Reborn tối đa 2 lần/trận ở merge level 0; (2) unit được scale bắt đầu với đúng 1 HP — một đòn bất kỳ đủ xử lý.

**Combo 3 — Gilgamesh + Buff Chain (Babylon Snowball):**
Gilgamesh tự cộng thêm chỉ số vĩnh viễn mỗi khi *nhận* một buff vĩnh viễn từ bên ngoài — nhưng xét kỹ thì chính hành động "tự cộng thêm chỉ số vĩnh viễn" đó cũng là một sự kiện cùng loại. Nói cách khác, kỹ năng của Gilgamesh có khả năng tự kích hoạt chính nó — đây là một minh chứng thực tế, ngay trong luật chơi, cho "bài toán đệ quy" đã đặt ra như một câu hỏi kiến trúc mở ở mục 2.3.3 (cách hệ thống ngăn nó trở thành vòng lặp vô hạn được trình bày ở Chương 4). Lever cân bằng nằm ở chính công thức merge đã trình bày tại mục 3.3.4: hệ số chiết giảm 0.7 khiến phần thưởng tích lũy không tăng tuyến tính theo thời gian — dù tích lũy được 50 đơn vị buff, chỉ số thực nhận về chỉ vào khoảng 35. Snowball vẫn xảy ra đúng như kỳ vọng thiết kế, nhưng có trần — đủ để tạo cảm giác "đáng đầu tư" mà không phá vỡ thế cân bằng tổng thể.

---

### 3.7.4 Quy Trình Cân Bằng — Lặp Qua Thực Nghiệm

**Vòng lặp:** Thiết kế → Test (GA training + playtest) → Quan sát → Điều chỉnh.

GA training là công cụ *stress test*: nếu tất cả 5 specialist bots hội tụ về cùng một chiến lược, đó là dấu hiệu một tribe đang quá mạnh. Fitness convergence curve của từng archetype bot cho biết chiến lược nào không thể cạnh tranh.

| Vấn đề quan sát | Lever điều chỉnh |
|-----------------|-----------------|
| Unit quá mạnh so với tier | Giảm base stat hoặc thêm triggerLimit |
| Combo kích hoạt quá nhiều lần | Giảm triggerLimit hoặc thêm conditionCount |
| Tribe quá yếu — bot không chọn | Tăng effectValue hoặc bỏ bớt điều kiện kích hoạt |
| Snowball quá nhanh | Giảm keepRatio hoặc thêm điều kiện subjectTribe |
| Merge không đáng đầu tư | Thêm isScaledTriggerLimit để reward merge |

*Ví dụ cụ thể — Sekhmet:* Thiết kế ban đầu không giới hạn lần nuốt unit. Kết quả test: Sekhmet nuốt toàn bộ 6 đồng minh → chỉ số của toàn đội. Điều chỉnh: thêm `triggerLimit = 2 + isScaledTriggerLimit`. Kết quả: tối đa 2 lần nuốt ở merge level 0 — đủ identity riêng, không phá vỡ toàn đội hình.

Giới hạn: GA tối ưu hóa cho fitness function, không cho trải nghiệm người chơi. Điều chỉnh cuối cùng kết hợp cả bot training (khách quan) và playtest thực tế (cảm giác gameplay).

---

### 3.7.5 Asymmetry Có Chủ Đích — Babylon Vs. Niles

**Babylon** là bộ tộc *accumulation*: tích lũy chậm, phụ thuộc vào **thời gian** và **kích thước đội hình**, mạnh khi trận kéo dài.

**Niles** là bộ tộc *reaction*: mạnh lên bất ngờ qua chuỗi tử-sinh, phụ thuộc vào **đúng tổ hợp unit**, không cần thời gian dài để scale.

Asymmetry tạo ra counterplay tự nhiên: Babylon buff tích lũy không bị ảnh hưởng bởi combat outcomes — dù thua một trận, buff vẫn còn đó. Niles Reborn chain làm Babylon khó tiêu diệt toàn bộ Niles dễ dàng. Chính sự không đối xứng — chứ không phải cân bằng đối xứng hoàn toàn — là nguồn gốc của chiều sâu chiến thuật.

---

*[Tiếp theo: Mục 3.8 — Thiết Kế UI/UX]*
