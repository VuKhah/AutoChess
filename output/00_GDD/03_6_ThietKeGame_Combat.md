## 3.6 Hệ Thống Chiến Đấu (Combat System)

Mục 3.2 đã mô tả Combat Phase từ góc nhìn vĩ mô: snapshot sân → bot deploy → tính toán đồng bộ → playback từng action. Mục này đi sâu vào *bên trong* trận chiến: sân được bố trí như thế nào, unit tấn công theo thứ tự nào, cái chết được xử lý ra sao, và những quy tắc thiết kế nào làm cho mọi quyết định về vị trí đặt bài đều có ý nghĩa.

---

### 3.6.1 Bố Cục Sân — Frontline Và Backline

Sân chiến đấu có **7 slot** (0–6), được chia thành hai vùng với quy tắc nhắm mục tiêu khác nhau:

```
 ┌───┬───┬───┬───┬───┬───┬───┐
 │ 0 │ 1 │ 2 │ 3 │ 4 │ 5 │ 6 │  ← slot index
 └───┴───┴───┴───┴───┴───┴───┘
  ◄─── FRONTLINE ───►◄─ BACK ─►
     slot 0–3 (4 ô)   slot 4–6 (3 ô)
```

> **[HÌNH 3.8 — Sơ Đồ Sân Chiến Đấu 7 Slot]** *Hình vẽ chuyên nghiệp của sân 7 slot với hai vùng màu khác nhau: Frontline (slot 0–3, màu đỏ nhạt) và Backline (slot 4–6, màu xanh nhạt). Mũi tên minh họa hướng tấn công: địch tấn công frontline trước, chỉ vào backline khi frontline trống. Thêm ví dụ đặt unit Taunt và carry.*

**Frontline (slot 0–3):** Luôn lộ ra trước đòn tấn công của đối thủ. Bất kỳ unit nào ở frontline đều là mục tiêu hợp lệ ngay lập tức.

**Backline (slot 4–6):** Được bảo vệ sau frontline. Unit backline chỉ có thể bị nhắm đến khi **toàn bộ frontline đã bị tiêu diệt** — kể cả sau khi Reborn và các unit được triệu hồi vào frontline trong combat.

Phân chia 4:3 (frontline nhiều hơn backline) phản ánh triết lý thiết kế: phần lớn đội hình nên ở frontline, backline là vị trí đặc quyền cho một đến hai unit có chỉ số thấp nhưng ability quan trọng. Backline không phải "chỗ ẩn náu vô hạn" — khi đã mất 4 unit frontline, backline lập tức lộ ra.

Quyết định này tạo ra một trục thiết kế đội hình có ý nghĩa: **unit nào xứng đáng backline?** Câu trả lời phụ thuộc vào ability: *Thoth* (Niles) cần sống đủ lâu để bị giết và trigger globalTribeBuff — backline; *Osiris* cần đồng minh hồi sinh để nhân đôi chỉ số — backline gần trung tâm; *Enki* (Babylon) có Taunt và phản dame — frontline cứng. Không có cấu hình sân "đúng tuyệt đối", chỉ có cấu hình phù hợp với đội hình cụ thể.

---

### 3.6.2 Thứ Tự Tấn Công — Attack Queue

Mỗi round chiến đấu, **tất cả unit còn sống** tham gia tấn công một lần, theo thứ tự được xác định trước:

```
Thứ tự tấn công trong một round:
  Slot 0 (enemy) → Slot 0 (player)
  → Slot 1 (enemy) → Slot 1 (player)
  → Slot 2 (enemy) → Slot 2 (player)
  → ...
  → Slot 6 (enemy) → Slot 6 (player)

  (Slot trống → bỏ qua; unit ATK = 0 → bỏ qua)
```

> **[HÌNH 3.9 — Thứ Tự Tấn Công Trong Một Round]** *Sơ đồ hai sân đối mặt nhau (enemy trên, player dưới), mỗi slot có unit. Các con số 1–14 đánh thứ tự tấn công theo hàng đợi: enemy slot 0 → player slot 0 → enemy slot 1 → player slot 1 → ... Slot trống bị bỏ qua (gạch chéo xám).*

Quy tắc "enemy trước, player sau trong cùng slot" không phải tuỳ tiện — nó tạo ra tính nhất quán: đối thủ luôn ra đòn trước ở mỗi vị trí slot, giúp người chơi *học được* pattern và dự đoán trận chiến. Nếu đối thủ có unit mạnh ở slot 0, unit player ở slot 0 sẽ chịu đòn đầu tiên trước khi kịp đánh.

**Unit với ATK = 0 không tấn công** nhưng vẫn bị tấn công — chúng là mục tiêu hợp lệ, chỉ không tự ra đòn. Điều này có ý nghĩa thiết kế: một unit với ATK = 0 nhưng có Taunt tạo ra "lá chắn hoàn toàn thụ động" — hút damage mà không phản công.

**Thứ tự tấn công là động, không tĩnh.** Khi một unit chết và một unit mới được triệu hồi hoặc hồi sinh trong cùng round đó, unit mới được chèn vào **đúng vị trí slot** trong phần hàng đợi chưa xử lý — không vào cuối queue. Điều này đảm bảo unit xuất hiện ở slot thấp hơn vẫn đánh trước unit ở slot cao hơn, bảo toàn thứ tự nhất quán dù board thay đổi liên tục.

Hai tính chất này — ATK=0 hợp lệ như "lá chắn thuần thụ động", và queue tái tạo liên tục khi board thay đổi — đều phục vụ cùng một nguyên tắc: **thứ tự tấn công phải có thể dự đoán được và nhất quán, ngay cả trong những trận chiến phức tạp nhất**. Không có unit nào "bỗng nhiên đánh sai lượt" hay unit mới summon "cắt hàng" — người chơi và bot đều có thể lên kế hoạch dựa trên thứ tự slot, không cần thêm điều kiện ngoại lệ.

---

### 3.6.3 Cơ Chế Va Chạm — Đánh Nhau Đồng Thời

Mỗi lần tấn công là một **Clash** (va chạm) — **cả attacker lẫn defender đều nhận damage đồng thời**:

```
Clash(attacker A, defender D):
  damage_to_D = A.currentATK
  damage_to_A = D.currentATK

  [kiểm tra Safeguard — nếu có, block 1 phía]

  D.currentHP -= damage_to_D
  A.currentHP -= damage_to_A
```

Không có cơ chế "né" hay "miss" — mọi đòn đều trúng, chỉ Safeguard mới block. Điều này tạo ra chiến đấu **predictable và transparent**: người chơi luôn có thể tính toán được kết quả của một clash nếu biết chỉ số cả hai bên, không có yếu tố may mắn ẩn trong combat.

**Hai unit có thể chết đồng thời** trong một clash — khi cả hai HP về 0 sau khi nhận damage. Đây là "clash deaths" và được xử lý song song: cả hai được đưa vào death stack, cả hai kích hoạt OnDeath. Trường hợp này quan trọng về mặt thiết kế vì nó ảnh hưởng đến thứ tự: ai trong death stack được xử lý trước (LIFO — cái cuối vào được xử lý trước) sẽ trigger OnAllyDeath cho phía còn sống của unit đó trước.

**Triggers phụ sau clash:**
- `OnAttack`: chỉ kích hoạt nếu attacker **còn sống** sau đòn (tránh unit chết vừa đánh vừa trigger)
- `OnTakeDamage`: chỉ kích hoạt nếu unit nhận damage **còn sống** sau khi nhận (tránh unit chết "phản ứng" với đòn giết chết nó)

Hai điều kiện này giải quyết một bài toán thiết kế tinh tế: nếu unit có OnAttack = "triệu hồi đồng minh" mà nó lại chết từ counterattack, liệu có nên triệu hồi không? Câu trả lời thiết kế là không — đã chết thì không còn năng lực hành động.

---

### 3.6.4 Chọn Mục Tiêu — Ba Tầng Ưu Tiên

`FindTarget()` xác định unit nào bị tấn công, theo ba tầng ưu tiên:

```
Tầng 1 — TAUNT (ưu tiên tuyệt đối)
  Nếu có bất kỳ unit nào có isTaunt = true:
    → Bắt buộc tấn công unit Taunt gần nhất với slot của attacker
    → Bypass hoàn toàn frontline/backline

Tầng 2 — FRONTLINE
  Nếu không có Taunt:
    → Tấn công unit sống ở frontline (slot 0–3) gần nhất

Tầng 3 — BACKLINE (chỉ khi frontline đã trống)
  Nếu không còn unit sống ở slot 0–3:
    → Tấn công unit sống ở backline (slot 4–6) gần nhất
```

**"Gần nhất" được tính bằng khoảng cách slot tuyệt đối** (`|attackerSlot - targetSlot|`). Trong cùng tầng ưu tiên, nếu có nhiều mục tiêu hợp lệ, unit ở slot gần attacker nhất được chọn. Điều này tạo ra một hành vi tự nhiên: unit ở slot 0 của player thường nhắm unit ở slot 0–1 của địch, không phải unit ở slot 5–6 trừ khi chúng là duy nhất còn sống.

**Taunt bypass frontline** là quyết định thiết kế quan trọng: một unit có Taunt ở backline vẫn bắt buộc bị nhắm dù toàn bộ frontline còn sống. Điều này tránh cho Taunt trở thành keyword vô dụng khi đặt không đúng chỗ — thay vào đó tạo ra một lựa chọn thiết kế có ý nghĩa: "tôi muốn unit Taunt hút damage ở frontline hay ở backline như một bẫy?"

**Hệ quả về vị trí đặt bài:** Bởi vì target selection dùng khoảng cách slot, vị trí đặt unit trên sân thực sự ảnh hưởng đến kết quả combat. Đặt unit fragile (HP thấp, ability quan trọng) ở slot 6 thay vì slot 4 có thể đủ để nó sống thêm một round vì kẻ địch ở slot 0–2 ưu tiên nhắm frontline của mình gần hơn trước.

---

### 3.6.5 Xử Lý Cái Chết — Death Stack

Cái chết trong combat không được xử lý ngay lập tức mà đi qua một **Death Stack** (ngăn xếp cái chết) — cấu trúc LIFO (vào sau ra trước) đảm bảo mọi chuỗi phản ứng từ cái chết được resolve hoàn toàn trước khi combat tiếp tục.

Quy trình chuẩn sau mỗi Clash:

```
1. Scan toàn bộ board: unit nào HP ≤ 0 mà chưa được xử lý?
   → Đẩy vào death stack (RegisterDeath)

2. FlushDeathStack: lặp cho đến khi stack rỗng VÀ không còn pending summon:
   a. Pop victim từ stack
   b. TriggerAbility(OnDeath, victim) — kỹ năng "khi chết" của victim
   c. Scan board tìm death mới (OnDeath có thể giết unit khác)
   d. Broadcast OnAllyDeath cho từng đồng minh cùng tộc còn sống
   e. Scan lại

   f. CleanupBoard:
      - Victim có Reborn chưa dùng? → ReviveDefault (HP = 1, isReborn = false)
        → Broadcast OnAllyReborn + OnAllySummon
      - Victim không có Reborn (hoặc đã dùng)? → Xóa khỏi slot (null)

   g. Nếu stack rỗng VÀ có pending summon: pop một summon, xử lý
   h. Lặp lại nếu còn death mới hoặc pending summon

3. Insert unit mới (summon/reborn) vào đúng vị trí queue
```

> **[HÌNH 3.10 — Quy Trình Death Stack (FlushDeathStack)]** *Flowchart chi tiết vòng lặp FlushDeathStack: Pop victim → TriggerOnDeath → Scan board → BroadcastOnAllyDeath → CleanupBoard (Reborn? → ReviveDefault : Remove) → BroadcastOnAllyReborn/Summon → Check pending summon → Lặp lại cho đến khi stack rỗng và không còn pending. Phân biệt màu nhánh Reborn (xanh) vs. không Reborn (xám).*

Thiết kế LIFO có lý do cụ thể: khi unit A chết và kích hoạt OnDeath summon unit B, B xuất hiện với OnDeploy — nếu B lại kích hoạt thêm gì đó gây chết unit C, thì C phải được xử lý *trước khi quay lại* xử lý hệ quả còn lại của A. Thứ tự LIFO đảm bảo "chuỗi con" bao giờ cũng hoàn chỉnh trước khi chuỗi cha tiếp tục.

**Giới hạn an toàn 500 vòng lặp** bảo vệ chống vòng lặp vô hạn — trường hợp lý thuyết khi hai unit liên tục tạo ra nhau (ví dụ unit A khi chết summon B, B khi chết summon A). Trong thực tế không có cặp card nào trong thiết kế tạo ra vòng lặp này, nhưng giới hạn tồn tại như safety net.

---

### 3.6.6 Reborn Trong Combat — Chết Như Là Cơ Chế

Reborn tương tác với combat theo một cách đặc biệt: **ReviveDefault() không phải là "không chết"** — nó là "chết rồi sống lại". Phân biệt này quan trọng:

```
Unit có Reborn bị đưa HP về 0:
  → Được đưa vào death stack (IsDead = true)
  → OnDeath KÉO TRIGGER (unit đã "chết" về mặt trigger)
  → CleanupBoard phát hiện isReborn = true, chưa dùng
  → ReviveDefault(): HP = 1, isReborn = false, hasRebornUsed = true
  → Broadcast OnAllyReborn (Osiris nhân đôi/ba chỉ số)
  → Broadcast OnAllySummon (Sobek nhận +1/+2, Sekhmet có thể nuốt)
  → Unit được chèn lại vào attack queue (phần chưa xử lý)
```

Ba điểm quan trọng:
1. **OnDeath vẫn kích hoạt** dù unit sẽ hồi sinh — Horus tăng stats toàn đội khi chết, dù sau đó sống lại
2. **Sau hồi sinh, unit vào queue với HP = 1** — cực kỳ dễ chết lần thứ hai, nhưng kịp đánh thêm ít nhất một round và kịp nhận buff từ Osiris
3. **OnAllyReborn + OnAllySummon fire sau hồi sinh** — đây là lúc Osiris nhân đôi chỉ số (do OnAllyReborn), và Sobek nhận +1/+2 (do cả OnAllySummon và OnAllyReborn)

Kết quả thực tế: một unit vừa được Anubis ban Reborn trong combat, sau đó chết, sẽ: (1) trigger OnDeath, (2) hồi sinh với HP = 1, (3) bị Osiris nhân đôi/ba chỉ số ngay tại đó, (4) đánh thêm một lần với chỉ số đã được nhân, rồi chết lần hai. Ba event trong một cái chết.

> **[HÌNH 3.11 — Chuỗi Trigger Khi Reborn]** *Sơ đồ trình tự dạng timeline ngang: unit bị HP về 0 → đẩy vào death stack → OnDeath fires (Horus buff toàn đội) → ReviveDefault HP=1 → OnAllyReborn fires (Osiris nhân đôi chỉ số) → OnAllySummon fires (Sobek nhận +1/+2) → unit vào lại attack queue. Mỗi bước một khung màu khác nhau.*

---

### 3.6.7 Hòa — Khi Không Ai Thắng

Sau **50 round** chiến đấu (mỗi round toàn bộ unit tấn công một lần), nếu cả hai phía vẫn còn unit sống, trận chiến kết thúc với kết quả **hòa** — không ai mất HP.

Hòa xuất hiện khi hai đội có lực lượng cân bằng quá mức và không phía nào đủ sát thương để xóa sổ phía kia trước 50 round. Trường hợp điển hình: hai đội nhiều unit Taunt + Reborn, hoặc một đội có unit liên tục hồi sinh qua Anubis + Sekhmet SummonConsumed chain.

50 round là giới hạn được chọn để **rất hiếm khi đạt được** trong gameplay bình thường — một trận chiến đầy đủ 7 vs 7 với unit có ATK trung bình sẽ kết thúc trong 10–20 round. Giới hạn 50 chỉ được kích hoạt khi có chuỗi hồi sinh/triệu hồi bất thường, đảm bảo combat không kéo dài vô hạn trong trường hợp edge case.

---

### 3.6.8 Tổng Hợp — Thiết Kế Chiến Đấu Như Hệ Thống Phản Hồi

Nhìn lại sáu phần vừa trình bày, hệ thống chiến đấu được xây dựng trên một nguyên tắc nhất quán: **mọi mechanic đều tạo ra quyết định** cho người chơi trong Shop Phase.

Board layout (7 slot, frontline/backline) → quyết định: đặt unit nào ở đâu?
Attack queue (slot tăng dần, enemy trước) → quyết định: slot nào đặt carry, slot nào đặt tank?
Target selection (Taunt → Frontline → Backline, closest) → quyết định: Taunt đặt đâu cho hiệu quả nhất?
Clash đồng thời → quyết định: HP bao nhiêu thì "đủ sống" qua một round?
Death stack LIFO → quyết định: ability nào kích hoạt theo thứ tự nào khi xây chain?
Reborn timing → quyết định: bao giờ nên invest vào Reborn? Kết hợp với ability gì?

Hệ thống không có yếu tố may mắn *trong combat* (không có miss, không có critical) — mọi ngẫu nhiên đã được hấp thụ vào Shop Phase (shop random, drop rate). Khi combat bắt đầu, kết quả là hoàn toàn deterministic từ trạng thái hai sân. Đây là thiết kế có chủ ý: người chơi học được "nếu tôi có đội hình X đấu với đội hình Y, kết quả sẽ là Z" — kiến thức tích lũy được qua nhiều ván, giúp họ đưa ra quyết định shop tốt hơn. Tính determinism này cũng là điều kiện cần thiết để GA training hoạt động đúng: cùng hai chromosome luôn cho cùng kết quả trận đấu — không có nhiễu từ combat random làm sai lệch fitness evaluation.

---

*[Tiếp theo: Mục 3.7 — Cân Bằng Game (Balancing)]*
