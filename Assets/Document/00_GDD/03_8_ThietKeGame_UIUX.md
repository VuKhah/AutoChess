## 3.8 Thiết Kế Giao Diện Và Trải Nghiệm Người Dùng (UI/UX)

Nếu các mục 3.2 đến 3.7 mô tả *những gì* game làm — vòng lặp, bài, shop, kinh tế, chiến đấu, cân bằng — thì mục này mô tả *cách người chơi nhìn thấy và tương tác* với tất cả những thứ đó. Nguyên tắc xuyên suốt thiết kế UI: **mỗi phần tử giao diện phải phục vụ một quyết định gameplay cụ thể** — không có thông tin thừa, không có bước tương tác không cần thiết.

---

### 3.8.1 Hai Trạng Thái Giao Diện — Shop Phase Và Combat Phase

Toàn bộ giao diện tồn tại trong hai trạng thái phân biệt hoàn toàn, chuyển đổi đồng bộ khi người chơi nhấn nút hành động:

```
SHOP PHASE                          COMBAT PHASE
─────────────────────────           ─────────────────────────
shopPanel: Active        ←──── ────→ shopPanel: Hidden
handPanel: Active                    handPanel: Hidden
enemyBoardPanel: Hidden             enemyBoardPanel: Active
Roll/Lock buttons: Hiện             Roll/Lock buttons: Ẩn
Nút: "BẮT ĐẦU"                     Nút: "LƯỢT TIẾP"
Background: shopSprite              Background: combatSprite
Resource icon: Coin                 Resource icon: Cup
ShopTierPanel: Hiện                 ShopTierPanel: Ẩn
```

> **[HÌNH 3.13 — So Sánh Hai Trạng Thái Giao Diện]** *Hai ảnh chụp màn hình đặt cạnh nhau: (trái) Shop Phase với shopPanel, handPanel, nút Reroll/Lock hiển thị, background shop; (phải) Combat Phase với enemyBoardPanel hiển thị, shopPanel ẩn, background combat. Các thành phần thay đổi được khoanh vòng và nối bằng mũi tên.*

Chuyển đổi này không chỉ ẩn/hiện panel — nó thay đổi toàn bộ không khí màn hình. **Background thay đổi theo phase** là tín hiệu trực quan mạnh nhất: người chơi lập tức nhận ra đang ở giai đoạn nào mà không cần đọc bất kỳ chữ nào. Background shop phase thường ấm, yên bình; background combat phase thường tối, kịch tính. Âm nhạc nền cũng chuyển đổi đồng thời — `PlayPrepBGM()` ở shop, `PlayCombatBGM()` ở combat.

**Resource icon cũng thay đổi theo phase:** Trong shop, icon đồng coin và số tiền hiện tại (kèm `+N` nếu có permanent income bonus). Trong combat, icon cup và số cup tích lũy. Cùng một widget, hai ý nghĩa — người chơi không cần nhìn nhiều vị trí khác nhau để biết hai thông số quan trọng nhất.

---

### 3.8.2 HUD — Màn Hình Thông Tin Liên Tục

Ba thông số xuất hiện liên tục trên HUD, không bao giờ ẩn dù ở phase nào:

**HP (Mạng):** Hiển thị số nguyên đơn giản. Không cần thanh máu — trong game này HP là số lần thua tối đa còn lại, không phải "phần trăm sức sống". Số đơn giản truyền đạt thông tin nhanh hơn thanh máu trong ngữ cảnh này.

**Cup:** Luôn hiển thị song song với HP. Hai con số này cùng nhau cho người chơi biết vị thế của mình: "3 HP / 7 Cup" nghĩa là đang thắng nhưng vẫn còn 3 lần thua nữa; "6 HP / 2 Cup" nghĩa là vẫn còn nhiều mạng nhưng tiến độ thắng chậm.

**Coin/Cup (Resource):** Widget duy nhất thay đổi icon theo phase — coin trong shop (cần biết có bao nhiêu để mua/reroll), cup trong combat (tiến độ đang dẫn của trận này). Khi có `permanentIncomeBonus > 0`, hiển thị dưới dạng `"12G (+2)"` — thông báo thêm rằng lượt sau sẽ nhận thêm 2 coin tự động, hỗ trợ lên kế hoạch chi tiêu ngay từ đầu lượt.

**Nút hành động chính:** Một nút duy nhất, thay đổi nhãn theo trạng thái ("BẮT ĐẦU" / "LƯỢT TIẾP"). Thiết kế một nút thay vì hai nút riêng tránh nhầm lẫn — người chơi luôn biết "nhấn nút đó để tiến lên bước tiếp theo".

---

### 3.8.3 Giải Phẫu Một Lá Bài — Tám Lớp Thông Tin

Một lá bài hiển thị đồng thời tám lớp thông tin, mỗi lớp phục vụ một loại quyết định khác nhau:

```
┌─────────────────────────────┐
│ [NAME]         [TIER ICON]  │ ← tên + tier pool (quan trọng cho reroll strategy)
│                             │
│       [CHARACTER ART]       │ ← nhận diện visual nhanh
│                             │
│ [ABILITY ICON] [KEYWORDS]   │ ← TTE effect + Taunt/Reborn/Safeguard icons
│                             │
│ [ATK]              [HP]     │ ← stats (màu HP đỏ khi bị thương trong combat)
│                             │
│ [★ ★ ☆]                    │ ← merge stars (1-3)
│                             │
│ [description text]          │ ← mô tả ability bằng tiếng Việt
└─────────────────────────────┘
  ↑ Frame blink vàng = merge hint
```

> **[HÌNH 3.14 — Giải Phẫu Giao Diện Lá Bài]** *Ảnh chụp thực tế một lá bài unit trong game, được chú thích tám thành phần bằng số và mũi tên: (1) Tên, (2) Tier icon góc trên phải, (3) Character art giữa, (4) Ability/keyword icons, (5) ATK góc dưới trái, (6) HP góc dưới phải, (7) Merge stars dưới cùng, (8) Description text. Frame blink vàng khi có merge hint.*

**Character Art:** Mỗi unit/spell có sprite riêng, nạp theo `cardID` (unit) hoặc `fileName` (spell) từ `Resources/Sprites/Cards/`. Đây là identifier thị giác nhanh nhất — sau một vài ván, người chơi nhận ra Gilgamesh hay Sekhmet bằng hình ảnh trước khi đọc tên.

**Tier Icon:** Hiển thị tier pool của card (1–6). Đây là thông tin chiến lược quan trọng: tier 6 xuất hiện ít và chỉ ở late game — thấy tier icon cao trong shop đầu game là tín hiệu "shop ngon, nên cân nhắc mua".

**Ability Icon:** Icon đại diện cho `EffectType` của ability đầu tiên trong danh sách (`Abi_{effectID}`). Không thể hiển thị toàn bộ TTE trong card nhỏ — một icon gợi ý loại effect (summon, damage, buff, coin...) đủ để người chơi nhớ khả năng đặc biệt của card mà không cần đọc description mỗi lần.

**Passive Keyword Icons:** Ba icon độc lập cho Taunt (hình khiên), Reborn (hình bình thuốc), Safeguard (vòng bảo vệ). Hiển thị đồng thời nếu unit có nhiều keyword — không ẩn/ưu tiên nhau.

**Star Icons (Merge Level):** 1 sao = base, 2 sao = đã merge một lần, 3 sao = đã merge hai lần. Thông tin merge level quan trọng để so sánh hai bản sao cùng card — không phải cùng mergeLevel thì không thể merge với nhau.

**HP Color Feedback:** Trong combat, HP text chuyển sang đỏ khi unit bị thương (`IsDamaged = currentHP < maxHP`). Trở về trắng sau khi heal. Thông tin trạng thái tức thì không cần hover hay tooltip.

**Stat Punch Animation:** Khi ATK hoặc HP thay đổi (từ buff/debuff trong combat), số tương ứng play animation:
- *Tăng*: số phóng to 1.45× rồi thu lại trong 0.22 giây, flash xanh lá
- *Giảm*: số flash đỏ rồi fade về màu gốc trong 0.28 giây

Animation ngắn nhưng đủ để người chơi nhận ra ngay "unit này vừa được buff" hay "vừa bị debuff" mà không cần đọc log.

---

### 3.8.4 Hệ Thống Kéo-Thả — Drag And Drop

Toàn bộ tương tác mua/bán/sắp xếp trong Shop Phase được thực hiện bằng **drag and drop** — không có menu bối cảnh (right-click menu), không có nút "mua" riêng. Một cử chỉ (kéo) thay cho nhiều bước (click → chọn action → xác nhận).

**Luồng kéo thả đầy đủ:**

```
Người chơi bắt đầu kéo (IBeginDragHandler):
  1. Card "nhấc lên" DRAGGING_LAYER (z-order cao nhất, không bị che)
  2. CanvasGroup.blocksRaycasts = false (chuột "xuyên qua" card, thấy slot bên dưới)
  3. Nếu nguồn là PlayerBoard/Hand → hiện SellZone

Trong lúc kéo (IDragHandler):
  4. Card bay theo tọa độ chuột

Người chơi thả (IEndDragHandler → CardSlot.OnDrop):
  5. Xác định destination slot type và source slot type
  6. Xử lý theo bảng logic (xem dưới)
  7. Card "hạ xuống" vào slot mới, play animation phù hợp
  8. CheckMergeNextFrame() — kiểm tra merge sau một frame
  9. SellZone ẩn đi
```

**Bảng hành vi theo destination:**

| Destination | Source | Hành vi |
|-------------|--------|---------|
| PlayerBoard (ô trống) | Shop | Mua + Deploy (trừ coin, trigger OnDeploy) |
| PlayerBoard (ô trống) | Hand | Di chuyển + Deploy (không trừ coin) |
| PlayerBoard (ô trống) | PlayerBoard | Đổi chỗ trong board |
| Hand (ô trống) | Shop | Mua vào tay (không Deploy) |
| Shop area | Board/Hand | Bán (+1 coin, trigger OnSell) |
| SellZone | Board/Hand | Bán (+1 coin, trigger OnSell) |
| Unit slot | Spell từ shop | Mua + Cast spell lên unit đó |

**Spell targeting qua drag:** Spell cần chọn target (ví dụ Balance Stance, Sharpen Blade) được kéo đặt trực tiếp lên unit mục tiêu — bản thân cử chỉ kéo-thả là hành động "chọn target". Spell không cần target (Ancient Coin, Trader's Trick) có thể thả vào bất kỳ ô trống nào hoặc tự động xử lý. Điều này loại bỏ UI chọn target riêng — một bước tương tác ít hơn.

---

### 3.8.5 Sell Zone — Vùng Bán Hiện Lên Khi Cần

Thay vì một nút "Bán" cố định, SellZone là một **panel xuất hiện động** trong khi kéo bài:

- **Vị trí:** Vùng trên-giữa màn hình (30–70% chiều ngang, 78–95% chiều dọc)
- **Hình thức:** Nền đỏ bán trong suốt (`rgba(0.85, 0.15, 0.15, 0.35)`) với chữ **"BÁN +1G"** to, đậm, màu trắng
- **Kích hoạt:** Chỉ xuất hiện khi kéo card từ PlayerBoard hoặc Hand (không hiện khi kéo từ Shop)
- **Tự ẩn:** Ngay khi thả chuột, dù bán thành công hay không

SellZone được xây dựng **runtime** (không cần setup trong scene) qua `SellZone.EnsureExists()` — tạo ra GameObject và gắn vào Canvas ngay trong `UIManager.Start()`. Thiết kế này giúp không cần quản lý thêm prefab trong scene, và đảm bảo SellZone luôn ở z-order trên cùng (`SetAsLastSibling()`).

Tại sao không dùng nút bán cố định? Vì nút cố định luôn chiếm diện tích màn hình kể cả khi không dùng. SellZone chỉ xuất hiện đúng lúc cần — khi tay đang kéo bài — và biến mất ngay sau đó. Đây là ví dụ của **contextual UI**: thông tin và tương tác chỉ hiện khi ngữ cảnh đòi hỏi.

---

### 3.8.6 Card Detail Panel — Xem Chi Tiết Khi Cần

Một lá bài nhỏ trong shop không đủ chỗ để đọc toàn bộ description phức tạp của unit tier 6. **Double-click bất kỳ card nào** mở `CardDetailPanel` — overlay toàn màn hình hiển thị:

- **Phiên bản lớn của card** (scale 3× so với card gốc, 160×240 unit)
- **Tên** (tiêu đề lớn)
- **Stats** (ATK / HP / Tier cho unit; Cost cho spell)
- **Description text** đầy đủ (ability viết bằng ngôn ngữ game, không phải code)

Panel được đóng bằng **phím Escape** — không cần tìm nút đóng. Lá bài trong panel bị tắt `CardDraggable` và `CanvasGroup.blocksRaycasts` — chỉ để xem, không kéo thả được.

Ngưỡng double-click là **0.3 giây** — hai lần click trong 0.3s = double-click, click đơn không làm gì. Giá trị này đủ tự nhiên (không quá nhanh, không quá chậm) để không bao giờ mở panel vô tình khi đang kéo bài.

---

### 3.8.7 Phản Hồi Merge — Thưởng Thị Giác Cho Đầu Tư Dài Hạn

Merge là khoảnh khắc đáng được nhấn mạnh thị giác — người chơi vừa hoàn thành một mục tiêu dài hạn. Ba lớp phản hồi đồng thời:

**1. BurstAnimation:** Lá bài sau merge phát ra hiệu ứng "bùng phát" — mở rộng nhanh rồi thu về — tín hiệu "có gì đó quan trọng vừa xảy ra".

**2. StarUp sound effect:** Âm thanh "lên sao" riêng biệt, khác hẳn âm thanh mua/bán/reroll thông thường. Âm thanh tạo ra *điểm nhấn cảm xúc* ngay cả khi người chơi không nhìn vào màn hình.

**3. Phần thưởng "Tinh Hoa Hợp Nhất":** Mỗi lần merge thành công, token spell `S_00` (*Tinh Hoa Hợp Nhất* — nhận 1 unit tier cao hơn shop hiện tại) tự động xuất hiện trong Hand. Đây là **immediate reward** cụ thể và có giá trị — không chỉ là hiệu ứng trực quan mà là phần thưởng gameplay thực sự, củng cố hành vi "đầu tư vào merge là đáng".

**Merge Hint blink:** Khi shop có lá bài đang thiếu 1 bản sao để hoàn thành merge, frame của lá đó nhấp nháy vàng theo hàm sin:

```csharp
float t = (Mathf.Sin(Time.unscaledTime * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
frameBackground.color = Color.Lerp(originalColor, blinkColor, t);
```

`blinkSpeed = 4f` (4 Hz) — đủ thu hút sự chú ý mà không gây khó chịu. `unscaledTime` đảm bảo blink không bị ảnh hưởng bởi `Time.timeScale` trong trường hợp game pause.

---

### 3.8.8 Ngôn Ngữ Màu Sắc — Color Language

Toàn bộ game dùng một hệ màu nhất quán để truyền đạt trạng thái mà không cần chữ:

| Màu | Ý nghĩa | Nơi dùng |
|-----|---------|----------|
| Xanh lá (`#40FF58`) | Buff / tăng stat / tốt | Stat punch tăng, SynergyNiles flash |
| Đỏ (`#FF2626`) | Debuff / giảm stat / nguy hiểm | Stat punch giảm, HP damaged, SellZone |
| Xanh dương (`#4D99FF`) | Trạng thái (status) / thông tin | Status effect flash |
| Vàng kim (`#FFD90D`) | Babylon synergy / merge hint | Babylon buff flash, blink frame |
| Cyan (`#1AD9FF`) | Olympus synergy / freeze active | Olympus buff flash, Lock button active |
| Lục (`#1AFF73`) | Niles synergy | Niles buff flash |
| Trắng | Bình thường / không đặc biệt | HP đầy, stat gốc |

Hệ màu này cho phép người chơi đọc trạng thái nhanh trong combat mà không cần đọc text — "flash vàng = Babylon đang buff", "flash đỏ = đang nhận debuff/damage". Sau vài ván, màu sắc trở thành ngôn ngữ phản xạ.

---

### 3.8.9 Màn Hình Chọn Độ Khó Và Kết Thúc Ván

**DifficultyPanel** xuất hiện ngay khi game bắt đầu, che toàn bộ gameplay phía sau. Ba nút: *Easy*, *Medium*, *Hard* — nhấn một nút, panel tự ẩn và game bắt đầu. Không có màn hình loading riêng hay transition phức tạp.

**EndGamePanel** xuất hiện khi game kết thúc (thắng hoặc thua), với hai thông điệp rõ ràng: **"CHIẾN THẮNG!"** hoặc **"THUA CUỘC!"**. Toàn bộ nút hành động, Roll, Lock bị disable — không thể tiếp tục chơi vô tình. Nút Restart nạp lại scene hiện tại, đưa người chơi về DifficultyPanel để chọn lại.

Quy trình từ mở game đến kết thúc ván: chọn độ khó → chơi → nhận kết quả → restart — không có màn hình phụ, không có menu phức tạp. Đây là thiết kế phù hợp với scope prototype: tối giản friction, maximize thời gian thực sự chơi.

---

*[Kết thúc Chương 3 — Thiết Kế Game (GDD)]*
*[Tiếp theo: Chương 4 — Kiến Trúc Hệ Thống Kỹ Thuật]*
