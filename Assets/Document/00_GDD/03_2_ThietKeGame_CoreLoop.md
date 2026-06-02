## 3.2 Vòng Lặp Gameplay Cốt Lõi (Core Loop)

### 3.2.1 Tổng Quan — Cấu Trúc Một Ván Đấu

Toàn bộ trải nghiệm gameplay được tổ chức thành một vòng lặp lặp đi lặp lại, mỗi vòng là một **lượt** (turn) gồm hai pha nối tiếp: **Pha Chuẩn Bị** (Shop Phase) và **Pha Chiến Đấu** (Combat Phase). Người chơi bắt đầu với 7 HP và không có cup nào. Mục tiêu là tích lũy đủ 10 cup (cup = điểm thắng trận) trước khi HP về 0, trong tối đa 20 lượt.

```
┌──────────────────────────────────────────────────────────────────┐
│                    VÒNG LẶP MỘT VÁN ĐẤU                         │
│                                                                  │
│   [BẮT ĐẦU] HP = 7, Cup = 0, Lượt = 1                           │
│       │                                                          │
│       ▼                                                          │
│   ╔═══════════════╗                                              │
│   ║  SHOP PHASE   ║  ← nhận 10 coin, shop mới, quyết định       │
│   ╚═══════╤═══════╝                                              │
│           │  [Nhấn Fight]                                        │
│           ▼                                                      │
│   ╔═══════════════╗                                              │
│   ║ COMBAT PHASE  ║  ← chiến đấu tự động, tối đa 50 bước        │
│   ╚═══════╤═══════╝                                              │
│           │                                                      │
│     ┌─────┴──────┐                                               │
│  Thắng?      Thua?      Hòa?                                     │
│  Cup +1      HP −1     (không đổi)                               │
│     │          │           │                                     │
│     └────┬─────┴───────────┘                                     │
│          ▼                                                       │
│   ┌──────────────────────────────────────┐                       │
│   │ Cup ≥ 10? → Thắng ván                │                       │
│   │ HP ≤ 0?  → Thua ván                  │                       │
│   │ Lượt > 20? → Thắng ván (vượt thời hạn)                      │
│   │ Còn lại  → Lượt + 1, quay lại Shop  │                       │
│   └──────────────────────────────────────┘                       │
└──────────────────────────────────────────────────────────────────┘
```

> **[HÌNH 3.3 — Sơ Đồ Vòng Lặp Một Ván Đấu]** *Flowchart hoàn chỉnh dựa trên sơ đồ ASCII bên trên, được vẽ lại dạng hình chuyên nghiệp với màu sắc phân biệt: hộp Shop Phase (xanh lá), hộp Combat Phase (đỏ), hộp kiểm tra điều kiện (vàng), các mũi tên kèm nhãn điều kiện (Thắng/Thua/Hòa, Cup≥10/HP≤0).*

Thiết kế hai điều kiện kết thúc (cup và HP) phản ánh hai triết lý chơi đối lập. Người chơi tấn công — mua đội hình mạnh sớm, thắng liên tiếp — tiến nhanh về mốc 10 cup nhưng gặp rủi ro nếu đội hình quá tốn tiền mà không đủ chiều sâu late-game. Người chơi phòng thủ — tiết kiệm coin, build dần — có thể chịu mất HP ban đầu đổi lấy đội hình vượt trội về sau, nhưng phải cẩn thận không để HP cạn trước khi kịp thống trị. Đây là căng thẳng thiết kế cốt lõi mà mọi quyết định trong Shop Phase đều phải đối mặt.

---

### 3.2.2 Pha Chuẩn Bị — Shop Phase

**Khởi đầu lượt: nạp tiền và làm mới shop**

Mỗi lượt mới bắt đầu bằng việc hệ thống tự động nạp tiền cho người chơi theo công thức:

```
CurrentCoin = 10 + bonusNextTurn + permanentIncomeBonus
```

`bonusNextTurn` là khoản thưởng trì hoãn từ các spell kinh tế lượt trước (ví dụ "nhận +3 coin đầu lượt sau") và bị xóa về 0 sau khi áp dụng. `permanentIncomeBonus` là thu nhập cộng dồn vĩnh viễn từ các spell tăng thu nhập — một khi đã có thì cộng thêm mỗi lượt mãi mãi. Thu nhập cơ sở cố định ở mức **10 coin mỗi lượt** — không có hệ thống streak (liên thắng/liên thua) hay interest (lãi suất giữ tiền). Quyết định thiết kế này giữ vòng lặp kinh tế đơn giản và dễ đọc: người chơi không cần theo dõi điều kiện phụ, chỉ cần quản lý 10 coin (cộng thêm bất kỳ bonus nào đã tích lũy) một cách tối ưu nhất.

Sau khi nạp tiền, shop được làm mới tự động với **7 lá bài**: 5 ô dành cho unit và 2 ô dành cho spell. Các lá bài được chọn ngẫu nhiên theo trọng số (weighted random) dựa trên **Shop Tier** — cấp độ shop hiện tại — được tính tự động theo lượt:

```
shopTier = clamp( ⌊(currentTurn + 1) / 2⌋, 1, 6 )
```

| Lượt    | Shop Tier | Ý nghĩa |
|---------|-----------|---------|
| 1–2     | Tier 1    | Chỉ có unit tier thấp, chi phí 1 coin |
| 3–4     | Tier 2    | Bắt đầu thấy unit tier 2 |
| 5–6     | Tier 3    | Pool đa dạng hơn, unit tier 3 phổ biến |
| 7–8     | Tier 4    | Unit mạnh bắt đầu xuất hiện |
| 9–10    | Tier 5    | Gần đến các unit tier 5–6 |
| 11+     | Tier 6    | Pool đầy đủ, mọi tier đều có thể xuất hiện |

Tier shop tăng tự động mà không tốn coin — đây là khác biệt quan trọng so với TFT hay Hearthstone Battlegrounds, nơi người chơi phải trả XP/gold để "lên level" truy cập unit tốt hơn. Trong thiết kế này, cả người chơi lẫn bot đều tiến đến late-game theo cùng nhịp độ, loại bỏ lợi thế từ việc đầu tư kinh tế vào tier progression và đặt focus hoàn toàn vào quyết định *mua gì* thay vì *mua cấp độ khi nào*.

**Nếu shop đang bị đóng băng (frozen)** từ lượt trước: các ô đã mua biến mất, các ô còn lại giữ nguyên, chỉ các ô trống được điền thêm lá mới. Điều này cho phép người chơi "đặt cọc" một lá bài hấp dẫn qua lượt khi chưa đủ tiền mua.

**Các hành động người chơi trong Shop Phase**

Trong Shop Phase, người chơi có toàn quyền sắp xếp trước khi nhấn Fight. Tập hành động khả dụng:

**(1) Mua và đặt unit lên sân (Board):**
Kéo unit từ shop lên một trong 7 ô trên sân. Unit được đặt lên sân kích hoạt trigger `OnDeploy` và bắt đầu nhận hiệu ứng bộ tộc (tribe synergy) ngay lập tức. Sân có hai hàng ngầm định — frontline (slot 0–2, hàng trước) và backline (slot 4–6, hàng sau) — mặc dù về mặt gameplay các slot này tương đương nhau về chức năng, sự phân biệt chỉ ảnh hưởng đến việc bot AI sắp xếp đội hình.

**(2) Giữ unit trong tay (Hand):**
Người chơi có thêm các ô tay (hand slots) để giữ unit chưa sẵn sàng đặt lên sân — ví dụ đang chờ thêm bản sao để merge. Unit trong tay không tham chiến nhưng vẫn nhận global tribe buff nếu có.

**(3) Bán unit:**
Kéo unit từ sân hoặc tay vào vùng bán (sell zone) để nhận lại **1 coin**, bất kể giá mua ban đầu hay mergeLevel của unit. Đây là quyết định thiết kế quan trọng: mọi unit đều có giá bán bằng nhau, tránh hiện tượng "sunk cost fallacy" — không có lý do nào để giữ lại unit kém chỉ vì đã trả giá cao cho nó.

**(4) Reroll shop:**
Chi **1 coin** để xáo toàn bộ shop, lấy 7 lá mới từ pool. Khi reroll, trạng thái frozen bị hủy. Đây là công cụ tìm kiếm unit cụ thể hoặc bản sao để merge, với chi phí cơ hội rõ ràng: 1 coin reroll = 1 coin không dùng để mua được.

**(5) Đóng băng shop (Freeze):**
Chi **0 coin** để giữ nguyên nội dung shop sang lượt sau. Đầu lượt tiếp theo, shop không tự làm mới — chỉ những ô đã mua (rỗng) mới được điền lá mới. Đây là công cụ đặt cọc: "tôi muốn lá bài này nhưng lượt này chưa đủ tiền, giữ lại đến lượt sau."

**(6) Mua và sử dụng spell:**
Spell không có chỉ số ATK/HP, không chiếm ô sân. Mua xong là được sử dụng ngay — một số spell yêu cầu chọn mục tiêu (ChosenAlly), số khác có hiệu ứng tức thì. Sau khi dùng, spell biến mất.

**(7) Trigger EndTurnShop:**
Ngay khi người chơi nhấn Fight (bắt đầu combat), hệ thống tự động kích trigger `EndTurnShop` cho tất cả unit đang trên sân của người chơi lẫn bot — áp dụng các hiệu ứng "đầu lượt tiếp theo" trước khi combat bắt đầu.

> **[HÌNH 3.4 — Giao Diện Shop Phase]** *Ảnh chụp màn hình game trong Shop Phase: hiển thị 7 ô shop (5 unit + 2 spell), sân 7 slot của người chơi, tay (hand), HUD với HP/Cup/Coin, nút Reroll, nút Lock và nút BẮT ĐẦU. Các thành phần được đánh số chú thích.*

**Hệ thống gợi ý merge (Merge Hint):**
Khi người chơi đã có đủ `(MergeRequiredCount − 1)` bản sao của một unit (trên sân + trong tay), lá bài tương ứng trong shop sẽ nhấp nháy — báo hiệu "mua thêm 1 là đủ bộ để merge". Đây là hỗ trợ UX giúp người chơi không bỏ lỡ cơ hội merge mà không cần đếm tay.

---

### 3.2.3 Pha Chiến Đấu — Combat Phase

**Chuẩn bị trận đấu**

Khi người chơi nhấn Fight, hệ thống thực hiện tuần tự:

1. **Snapshot sân người chơi** — chụp lại chính xác trạng thái sân (vị trí, chỉ số, consumedCardIDs) *trước khi* bất kỳ thay đổi nào từ combat xảy ra. Snapshot này được dùng để khôi phục sân về đúng trạng thái ban đầu sau khi combat kết thúc — đảm bảo các unit được triệu hồi trong battle, các unit hồi sinh tạm thời, hay các hiệu ứng combat không "rò rỉ" sang lượt tiếp theo.

2. **Bot đối thủ triển khai đội hình** — BotAgent của đối thủ tự động quyết định đội hình cho lượt này (mua, bán, sắp xếp) theo Chromosome của mình. Trong chế độ PvE, người chơi không thấy quá trình này — đội hình đối thủ xuất hiện trực tiếp trên sân enemy.

3. **SyncBoards** — đồng bộ trạng thái sân với dữ liệu logic để đảm bảo mọi tham chiếu đúng trước khi combat bắt đầu.

**Cơ chế tính toán trận đấu — Tách biệt Logic và Hình ảnh**

Một quyết định kiến trúc then chốt của Combat Phase là **tách biệt hoàn toàn giữa tính toán và trình diễn**. Toàn bộ trận đấu — mọi đòn tấn công, mọi cái chết, mọi ability trigger, mọi reborn, mọi summon — được tính toán đồng bộ trong một lần gọi `resolver.ResolveTurn()` và kết quả được ghi vào `TurnRecord combatLog`. Chỉ sau đó, hệ thống mới phát lại từng action trong log một cách có kiểm soát để người chơi theo dõi.

Lợi ích của kiến trúc này là kép. Về phía người dùng: animation không bao giờ bất đồng bộ với logic vì logic đã hoàn tất trước khi animation bắt đầu. Về phía kỹ thuật: cùng một `resolver.ResolveTurn()` được dùng trong headless simulation của GA training mà không cần viết lại bất kỳ dòng code nào.

**Cấu trúc bên trong một trận đấu (ResolveTurn)**

Bên trong `ResolveTurn()`, trận đấu diễn ra theo hai giai đoạn:

*Giai đoạn Setup:* Tất cả unit trên cả hai sân lần lượt kích hoạt trigger `StartOfBattle` và `Aura`. Đây là thời điểm các unit "khởi động" — áp buff đầu trận, thiết lập aura passive, hay tung kỹ năng một lần duy nhất khi chiến đấu bắt đầu. Mọi cái chết phát sinh từ giai đoạn này (ví dụ: AOE damage từ StartOfBattle) được xử lý ngay bởi death stack trước khi vào vòng lặp chính.

*Giai đoạn Battle Loop — tối đa 50 round:* Mỗi round, hệ thống xây dựng một **hàng đợi tấn công động** (attack queue) từ tất cả unit còn sống, sắp xếp theo slot tăng dần (0→6), với enemy trước player ở cùng slot. Từng unit trong hàng đợi tấn công đến mục tiêu gần nhất:

```
FindTarget(): 
  - Nếu có unit có Taunt → bắt buộc nhắm Taunt đầu tiên
  - Ngược lại → nhắm unit sống ở slot gần frontline nhất (slot nhỏ nhất)
```

Sau mỗi đòn tấn công, death stack được flush ngay lập tức — mọi unit chết kích hoạt OnDeath, OnAllyDeath, SummonConsumed, Reborn... theo thứ tự stack. Các unit hồi sinh (Reborn) hoặc unit mới được triệu hồi trong quá trình flush được chèn vào đúng vị trí slot trong phần hàng đợi chưa xử lý — đảm bảo chúng cũng được tấn công/tấn công trong round đó nếu được triệu hồi đủ sớm.

Trận đấu kết thúc sớm khi một phía bị xóa sổ hoàn toàn (`IsSideEliminated`). Nếu sau **50 round** cả hai phía vẫn còn unit sống, trận đấu kết thúc với kết quả **hòa** — không ai mất HP lượt này.

**Trình diễn kết quả**

Sau khi `ResolveTurn()` hoàn thành và `combatLog` đã có đầy đủ dữ liệu, hệ thống phát lại từng `CombatAction` trong log theo trình tự:
- `Attack` action → animation tấn công, cập nhật HP hiển thị
- `Death` action → animation tan biến
- `Reborn` action → animation chết rồi hồi sinh với hiệu ứng phát sáng
- `Summon` action → unit mới xuất hiện trên ô trống
- `StatChange` action → flash màu trên unit (xanh lá = buff, đỏ = debuff, vàng = Babylon synergy...)

Mỗi action cách nhau 0.1 giây để người chơi có thể theo dõi, tổng thời gian trình diễn một trận đấu thường từ 5–15 giây tùy độ phức tạp.

> **[HÌNH 3.5 — Giao Diện Combat Phase]** *Ảnh chụp màn hình game trong Combat Phase: sân người chơi (hàng dưới) và sân đối thủ (hàng trên) đối mặt nhau, các unit đang chiến đấu với hiệu ứng flash damage. HUD hiển thị Cup thay cho Coin. Background chuyển sang chế độ combat.*

---

### 3.2.4 Kết Quả Lượt Và Hậu Quả HP

Sau khi trình diễn kết thúc, hệ thống kiểm tra trạng thái sân để phán định kết quả:

```
Nếu player còn unit sống   VÀ enemy đã xóa sổ  →  Thắng: playerCups += 1
Nếu player đã xóa sổ      VÀ enemy còn unit    →  Thua:  playerHP   -= 1
Nếu cả hai còn unit sống  (hòa sau 50 round)   →  Không đổi gì cả
```

**Lý do thiết kế hòa không trừ HP:** Trận hòa xảy ra khi cả hai đội đủ mạnh để sống sót qua 50 round chiến đấu mà không bên nào đủ sức tiêu diệt đối thủ. Đây là tình huống hiếm (thường gặp ở các deck phòng thủ cực đoan với nhiều Reborn/Taunt) và không nên bị phạt — cả hai bên đều "chơi tốt" theo cách của mình. Không trừ HP trong trường hợp hòa cũng tạo ra chiến lược thú vị: đôi khi đội hình tốt nhất không phải là đội hình giết địch nhanh nhất, mà là đội hình không bao giờ chịu để mất HP.

**Hệ thống wager:** Nếu người chơi đã mua spell "Wager" trước đó (đặt cược N coin), thắng trận này sẽ nhận lại N coin bonus. Thua thì mất khoản đặt cược đó. Đây là mechanic rủi ro-phần thưởng có thể thay đổi cục diện kinh tế đáng kể nếu được sử dụng đúng thời điểm.

**Phục hồi sân sau combat:** Sau khi phán định kết quả, hệ thống khôi phục sân người chơi về đúng trạng thái snapshot đã chụp trước combat — xóa mọi unit bị triệu hồi trong battle, phục hồi HP của unit hồi sinh về 1 (đúng trạng thái Reborn đã dùng), và reset `isBattleSpawned` flag. Kết quả là đầu lượt tiếp theo, người chơi thấy chính xác đội hình của mình như trước khi họ nhấn Fight — chỉ sắp xếp đội hình, không lo unit bị mất hay bị thay đổi bởi combat.

---

### 3.2.5 Điều Kiện Kết Thúc Ván Đấu

Sau mỗi trận đấu, hệ thống kiểm tra ba điều kiện kết thúc ván:

**Thắng (Win):** `playerCups >= 10` — người chơi thắng đủ 10 trận. Không cần thắng liên tiếp — mỗi trận thắng tích 1 cup bất kể lượt nào.

**Thua (Game Over):** `playerHP <= 0` — người chơi thua 7 trận (vì bắt đầu với 7 HP). Đây là ceiling của độ tha thứ: người chơi có thể thua nhiều lượt đầu trong khi build đội hình mà không bị loại ngay.

**Hết thời gian (Timeout Win):** Nếu người chơi còn sống sau lượt 20, game kết thúc với chiến thắng — đây là an toàn lưới bảo vệ cho trường hợp đã tích đủ điểm ưu thế nhưng chưa đạt 10 cup. Về mặt thiết kế, điều kiện này cũng làm cho chiến lược phòng thủ thuần túy (không cần win, chỉ cần không thua) có lý khi HP còn nhiều và game đã vào giai đoạn muộn.

| Điều kiện | Giá trị khởi đầu | Ngưỡng kết thúc | Thay đổi mỗi lượt |
|-----------|-----------------|-----------------|-------------------|
| playerCups | 0 | ≥ 10 → thắng | +1 khi thắng combat |
| playerHP | 7 | ≤ 0 → thua | −1 khi thua combat |
| currentTurn | 1 | > 20 → thắng | +1 mỗi lượt |

Sự bất đối xứng giữa hai điều kiện kết thúc chính — cần 10 chiến thắng để win, nhưng chỉ cần 7 thất bại để thua — tạo ra một áp lực không gian: người chơi có thể chịu thua thêm 2–3 lần nữa nếu đang ở 5 cup/5 HP, nhưng không thể chịu thua 7 lần liên tiếp dù sau đó thắng tất cả. Điều này buộc người chơi phải cân bằng tất cả quyết định shop trong cả hai hướng cùng lúc — tấn công để tích cup, phòng thủ để giữ HP — thay vì có thể focus hoàn toàn vào một trong hai.

---

*[Tiếp theo: Mục 3.3 — Hệ Thống Bài (Card System)]*
