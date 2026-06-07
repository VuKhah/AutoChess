## 3.7 Cân Bằng Game (Balancing)

Một game được gọi là cân bằng khi không có một chiến lược duy nhất "ăn tất cả" — khi người chơi có lý do để thử nhiều hướng xây đội hình khác nhau, và mỗi hướng đều có điểm mạnh riêng đáng đánh đổi. Cân bằng không có nghĩa là mọi lá bài có sức mạnh bằng nhau — nó có nghĩa là **độ mạnh yếu của từng lá bài được phân bổ theo chiều sâu chiến lược**, không theo chiều "tốt hơn hoàn toàn".

Mục này phân tích phân phối bài theo tier, triết lý thiết kế chỉ số, những tổ hợp nguy hiểm và cách chúng được kiềm chế, và quy trình điều chỉnh qua thực nghiệm.

---

### 3.7.1 Phân Phối Bài Theo Tier

Tổng 68 lá bài được phân bổ qua 6 tier theo cấu trúc sau:

**Unit cards (47 lá):**

| Tier | Babylon | Niles | Tổng unit | Tỉ lệ |
|:----:|:-------:|:-----:|:---------:|:-----:|
| 6    | 3       | 3     | 6         | 12.8% |
| 5    | 4       | 4     | 8         | 17.0% |
| 4    | 6       | 4     | 10        | 21.3% |
| 3    | 3       | 4     | 7         | 14.9% |
| 2    | 3       | 4     | 7         | 14.9% |
| 1    | 4       | 5     | 9         | 19.1% |

**Spell cards (21 lá, bao gồm 1 token):**

| Tier | Spell | Ghi chú |
|:----:|:-----:|:--------|
| 1    | 8     | 7 non-token + 1 token (Tinh Hoa Hợp Nhất) |
| 2    | 6     | Bao gồm spell kinh tế chính (Caishen's Knock) |
| 3    | 3     | Spell tầm trung (Rising Spirit, Ritual, Divine) |
| 4    | 3     | Spell power (Olympic Flame, Gate of Destruction, Devil's Deal) |
| 5    | 1     | Military Support — spell đắt nhất (5 coin) |

**Đọc phân phối unit:**

Tier 4 là tier đông nhất (10 unit, 21.3%), tạo ra một vùng mid-game rất phong phú. Đây có chủ ý — tier 4 xuất hiện phổ biến nhất ở shop level 4–5 (drop rate 25–40%), trùng với giai đoạn người chơi đang định hình đội hình cuối. Tier 1 đứng thứ hai (9 unit) vì vai trò kép: starter units đầu game và nguyên liệu merge rẻ suốt ván.

Tier 5–6 ít nhất (8 và 6 unit) — không phải vì thiếu thiết kế mà vì độ hiếm có chủ ý. Với pool nhỏ, khi shop tier đủ cao để roll vào tier 6, xác suất ra *đúng unit mong muốn* tương đối cao hơn — bù đắp cho việc tier 6 chỉ có 10% tỉ lệ xuất hiện ở shop level 6. Người chơi không cảm thấy "roll mãi không ra" dù tỉ lệ thấp.

> **[HÌNH 3.12 — Phân Phối 68 Lá Bài Theo Tier]** *Biểu đồ cột đôi: cột trái là unit (47 lá, phân tầng màu theo Babylon/Niles), cột phải là spell (21 lá). Trục hoành là Tier 1–6, trục tung là số lượng. Tổng số mỗi tier được ghi trên đầu cột.*

**Đọc phân phối spell:**

Spell tier 1 chiếm gần 40%, phù hợp với vai trò của chúng là công cụ linh hoạt giá rẻ dùng được mọi lúc. Spell tier 5 chỉ có một lá (Military Support, giá 5) — đây là "win condition spell" xuất hiện rất hiếm và đòi hỏi lượng coin cực lớn, giới hạn tự nhiên tần suất sử dụng.

Nhìn tổng thể, phân phối 68 lá bài phản ánh một triết lý thiết kế nhất quán: **nội dung phong phú ở tầng giữa** (tier 3–4) để tạo ra mid-game thú vị và nhiều lựa chọn, trong khi tier 1 giữ số lượng đủ để cung cấp nguyên liệu merge ổn định từ đầu game, và tier 5–6 đủ hiếm để tạo ra cảm giác "khám phá" khi chúng xuất hiện. Phân phối này không hướng đến tỷ lệ bằng nhau — mà hướng đến tỷ lệ đúng với vai trò của mỗi tier trong vòng lặp gameplay.

---

### 3.7.2 Triết Lý Chỉ Số — Stat Không Nói Lên Tất Cả

Một nguyên tắc thiết kế nhất quán: **tier của unit xác định *khi nào* nó mạnh nhất, không phải *tại sao* nó mạnh**. Nhiều unit tier cao có base stat thấp hơn mong đợi — sức mạnh đến từ ability, không từ số liệu:

| Unit | Tribe | Tier | ATK | HP | Ghi chú về sức mạnh |
|------|-------|:----:|:---:|:--:|---------------------|
| Osiris | Niles | 6 | 3 | 3 | ScaleTargetStats khi OnAllyReborn → nhân đôi/ba/bốn chỉ số |
| Anubis | Niles | 6 | 5 | 5 | Grant Reborn mỗi khi đồng minh chết (≤2/4/6 lần) |
| Sobek | Niles | 5 | 2 | 2 | +1/+2 vĩnh viễn *mỗi lần* đồng minh summon/reborn |
| Enki | Babylon | 5 | 2 | 15 | Taunt + phản dame 5/10/15 khi bị đánh |
| Humbaba | Babylon | 4 | 7 | 1 | Nhận 2 coin mỗi khi tấn công |
| Pazuzu | Babylon | 3 | 1 | 2 | Nhận 2 coin khi deploy |

*Osiris (3/3 tier 6)* là ví dụ cực đoan nhất: raw stat chỉ bằng unit tier 2. Nhưng mỗi lần đồng minh hồi sinh (Reborn), Osiris nhân đôi/ba chỉ số của đơn vị đó ngay lập tức. Kết hợp với Anubis grantReborn, một unit trung bình có thể trở thành unit mạnh nhất sân trong một round. Sức mạnh của Osiris không đến từ bản thân nó mà từ hệ sinh thái xung quanh.

Ngược lại, *Enki (2/15 tier 5)* là ví dụ "stat extreme": HP bất thường cao (15, gấp đôi unit cùng tier), ATK gần như 0 (2). Đây là unit được thiết kế thuần túy cho vai trò tank: Taunt bắt địch tấn công, HP cao chịu nhiều đòn, phản dame 5 per đòn gây chip damage không ngừng. Enki không bao giờ kill địch bằng ATK — nó giết qua phản dame tích lũy.

Triết lý này tạo ra **đa dạng vai trò** mà không cần nhiều mechanic phức tạp: cùng hệ thống TTE, unit có thể là pure tank, pure carry, economic engine, summon chain piece, hoặc buff amplifier — tùy cách thiết kế tổ hợp Trigger × Target × Effect và base stat.

---

### 3.7.3 Combo Mạnh Và Cách Kiềm Chế

Ba tổ hợp sau đây được xác định qua quá trình test là đặc biệt mạnh, đòi hỏi các lever cân bằng rõ ràng.

**Combo 1 — Taunt + Reborn (ví dụ: Kingu)**

Kingu (tier 2, ATK 2, HP 2) có đồng thời Taunt và Reborn — một trong những combo rẻ nhất trong game. Kẻ địch bắt buộc phải dùng 2 round để hạ một unit tier 2 với chỉ số tổng 4.

Tại sao không bị overpowered? Vì **Kingu không làm gì ngoài việc chịu đòn**. Nó không triệu hồi, không buff, không trigger chain. Tác động duy nhất là hút 2 round tấn công địch — đủ để frontline sau nó sống thêm 2 đòn, không hơn. Trong late game khi địch có unit ATK 15+, 2 HP của Kingu (sau Reborn) bị hạ trong đúng một đòn tiếp theo.

Cân bằng bẩm sinh: unit tier thấp sẽ không còn đủ HP để tank sau khi địch đã scale stats. Taunt + Reborn là combo mạnh ở early-mid game, tự động yếu dần ở late game — không cần điều chỉnh số liệu.

**Combo 2 — Anubis + Reborn + Osiris (Niles Reborn Chain)**

Đây là combo Niles bậc cao và nguy hiểm nhất trong thiết kế:

```
Đồng minh chết (bất kỳ)
  → Anubis: Grant Reborn cho LowestHealthAlly
  → Ally đó chết → Reborn với 1 HP
  → Osiris: ScaleTargetStats(x2/x3/x4 theo mergeLevel) lên unit vừa hồi sinh
  → Unit đó: từ trung bình → cực mạnh trong 1 round
```

Nhân đôi một unit bình thường không đáng sợ, nhưng nhân đôi unit đã được buff vĩnh viễn nhiều lượt (từ Sobek/Isis/Bastet) có thể tạo ra unit với ATK 30+. Combo này là "win condition" của Niles deck — mọi thứ hướng đến khoảnh khắc Osiris kích hoạt.

**Hai lever cân bằng chính:**

Thứ nhất, *Anubis có triggerLimit = 2 + isScaledTriggerLimit*: ở merge level 0, Anubis chỉ grant Reborn tối đa 2 lần trong một trận chiến. Điều này nghĩa là combo chỉ kích hoạt 2 lần — không phải mỗi cái chết. Merge Anubis lên 1 sao cho phép 4 lần, 2 sao cho 6 lần — reward cho đầu tư merge mà không bỏ ngỏ hoàn toàn.

Thứ hai, *Osiris chỉ scale unit ở 1 HP sau Reborn*: unit được nhân đôi bắt đầu với đúng 1 HP. Kẻ địch chỉ cần một đòn bất kỳ để hạ lại. Nếu đối thủ có đủ ATK để one-shot 1 HP, toàn bộ Osiris buff bị xóa ngay lập tức. Điều này tạo ra counterplay rõ ràng: giữ unit có ATK cao đủ để xử lý unit HP 1 sau Reborn.

**Combo 3 — Gilgamesh + Buff Chain (Babylon Snowball)**

Gilgamesh kích hoạt OnStatGain: mỗi khi nhận bất kỳ permanent buff nào, tự cộng thêm vĩnh viễn +2/+1. Trong Babylon deck với nhiều unit như Utu (buff toàn Babylon mỗi deploy), Ashur (buff mỗi bán), Lamashtu/Uridimmu — Gilgamesh có thể nhận permanent buff hàng chục lần mỗi ván.

**Lever cân bằng:** Flag `_firingOnStatGain = true` trong AbilityEngine ngăn Gilgamesh trigger chính nó (nhận buff → OnStatGain → buff thêm → lại OnStatGain → vòng lặp vô hạn). Gilgamesh chỉ react với buff đến từ nguồn *bên ngoài* — từ unit khác hoặc spell — không tự kích hoạt đệ quy.

Ngoài ra, công thức `keepRatio = 0.7` khi tính chỉ số sau merge làm giảm hiệu quả tích lũy: `currentATK = round(baseATK × tier + 0.7 × (growthATKBonus + permanentATKBonus))`. Dù có 50 permanent ATK bonus, thực tế chỉ nhận 35. Hệ số 0.7 không phải ngẫu nhiên — nó đảm bảo snowball không vô hạn mà vẫn đủ để cảm giác đáng đầu tư.

---

### 3.7.4 Quy Trình Cân Bằng — Lặp Qua Thực Nghiệm

Không có công thức toán học đảm bảo cân bằng hoàn hảo. Quá trình cân bằng game này là **vòng lặp thực nghiệm**: thiết kế → test (bằng GA training + chơi thực) → quan sát → điều chỉnh.

**Bước 1 — Thiết kế ban đầu với nguyên tắc kinh nghiệm:**
Mỗi lá bài được thiết kế theo nguyên tắc: base stat nhân theo tier, ability bù đắp cho stat thấp hoặc ngược lại. Tier 1 unit được định hướng stat thấp + ability đơn giản/không có; Tier 6 unit có thể có stat thấp nhưng ability phức tạp, hoặc stat cao nhưng ability thụ động.

**Bước 2 — GA training như công cụ stress test:**
Sau khi thiết kế hoàn chỉnh một batch card, chạy GA training với population đủ lớn (100+ cá thể, 150 thế hệ). Quan sát kết quả: nếu tất cả 5 specialist bots đều hội tụ về cùng một chiến lược (ví dụ: tất cả đều ưu tiên gene[18] = sBabylon cực cao), đó là dấu hiệu một bộ tộc đang quá mạnh.

Fitness convergence curve cũng cung cấp thông tin: nếu một archetype bot (ví dụ resilientBot) không thể đạt fitness cạnh tranh dù sau nhiều thế hệ, chiến lược phòng thủ đó có thể đang bị yếu về thiết kế — thiếu unit phòng thủ tốt hoặc Reborn chain bị counterplay quá dễ.

**Bước 3 — Điều chỉnh qua các lever cụ thể:**

| Vấn đề quan sát | Lever điều chỉnh |
|-----------------|-----------------|
| Unit quá mạnh so với tier | Giảm base stat hoặc thêm triggerLimit |
| Combo kích hoạt quá nhiều lần | Giảm triggerLimit hoặc thêm conditionCount |
| Tribe quá yếu — bot không chọn | Tăng effectValue hoặc bỏ bớt điều kiện kích hoạt |
| Snowball quá nhanh | Giảm keepRatio hoặc thêm điều kiện subjectTribe |
| Merge không đáng đầu tư | Thêm isScaledTriggerLimit để reward merge |

**Ví dụ điều chỉnh cụ thể — Sekhmet:**

*Sekhmet* (Niles tier 6): OnAllySummon → nuốt unit vừa triệu hồi, lấy chỉ số. Thiết kế ban đầu không giới hạn số lần. Kết quả test: Sekhmet có thể nuốt toàn bộ 6 đồng minh còn lại trong một trận chiến khi kết hợp với nhiều đơn vị triệu hồi — trở thành unit có chỉ số tổng của toàn đội, bất khả chiến bại. Điều chỉnh: thêm `triggerLimit = 2` và `isScaledTriggerLimit = true`. Kết quả: Sekhmet nuốt tối đa 2 unit ở merge level 0 — đủ mạnh để có identity riêng, không đủ để phá vỡ toàn bộ đội hình.

**Bước 4 — Giới hạn của quá trình:**

Cân bằng thông qua GA training có một giới hạn quan trọng: GA tối ưu hóa cho fitness function, không cho trải nghiệm người chơi. Một tổ hợp card có thể hoàn toàn "cân bằng" theo win rate bot-vs-bot nhưng vẫn gây frustrating khi người chơi thực đối mặt — vì một số mechanic cảm giác không fair dù về mặt số liệu là công bằng.

Điều chỉnh cuối cùng cần kết hợp cả hai nguồn thông tin: **bot training** (kiểm tra cân bằng hệ thống, không thiên vị) và **playtest thực tế** (kiểm tra cảm giác gameplay). Trong phạm vi dự án này, playtest được thực hiện bởi nhóm phát triển — một giới hạn thực tế mà mục 6.4 sẽ thảo luận thêm.

Quy trình bốn bước này có một tính chất quan trọng: nó không kết thúc. Mỗi lần thêm card mới hoặc điều chỉnh số liệu là một lần khởi động lại vòng lặp. Phần tiếp theo (3.7.5) mô tả kết quả cuối của nhiều vòng lặp như vậy — thiết kế asymmetry có chủ đích giữa Babylon và Niles, hai bộ tộc đã được điều chỉnh nhiều nhất trong quá trình phát triển.

---

### 3.7.5 Asymmetry Có Chủ Đích — Babylon Vs. Niles

Hai bộ tộc được thiết kế với **asymmetry rõ ràng**, không phải mirror image của nhau:

**Babylon** là bộ tộc *accumulation* — tích lũy chậm rãi, buff lên theo lượt, mạnh hơn khi trận đấu kéo dài và nhiều đồng minh được deploy/sell. Babylon phụ thuộc vào **thời gian** và **kích thước đội hình** để phát huy sức mạnh.

**Niles** là bộ tộc *reaction* — mạnh lên bất ngờ qua các chuỗi tử-sinh. Niles không cần thời gian dài để scale; một chuỗi Anubis → Reborn → Osiris trong một trận có thể quyết định ngay lập tức. Niles phụ thuộc vào **đúng tổ hợp unit** xuất hiện trên sân.

Asymmetry này tạo ra counterplay tự nhiên:
- Babylon tốt vs. Niles vì Babylon buff tích lũy không bị ảnh hưởng bởi combat outcomes trong lượt đó — dù thua một trận, buff vẫn còn đó
- Niles tốt vs. Babylon vì Reborn chain làm Babylon tank không tiêu diệt được toàn bộ Niles dễ dàng
- Cả hai đều mạnh theo cách riêng, tạo ra câu hỏi thú vị khi gặp nhau: "liệu Babylon đã tích đủ buff để outscale Niles trong một trận không?"

Chính sự không đối xứng này — chứ không phải sự cân bằng đối xứng hoàn toàn — là nguồn gốc của chiều sâu chiến thuật. Nếu Babylon và Niles hoàn toàn mirror, chỉ cần học một bộ tộc là đủ.

---

*[Tiếp theo: Mục 3.8 — Thiết Kế UI/UX]*
