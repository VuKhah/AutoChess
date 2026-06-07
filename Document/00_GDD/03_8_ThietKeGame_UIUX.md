## 3.8 Thiết Kế Giao Diện Và Trải Nghiệm Người Dùng (UI/UX)

Nguyên tắc xuyên suốt: **mỗi phần tử giao diện phải phục vụ một quyết định gameplay cụ thể** — không có thông tin thừa, không có bước tương tác không cần thiết.

---

### 3.8.1 Hai Trạng Thái Giao Diện

Toàn bộ giao diện tồn tại trong hai trạng thái phân biệt hoàn toàn:

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

> **[HÌNH 3.13 — So Sánh Hai Trạng Thái Giao Diện]** *Hai ảnh chụp màn hình đặt cạnh nhau: (trái) Shop Phase, (phải) Combat Phase. Các thành phần thay đổi được khoanh vòng và nối bằng mũi tên.*

Background và nhạc nền thay đổi đồng thời (`PlayPrepBGM()` / `PlayCombatBGM()`) — tín hiệu trực quan mạnh nhất về phase hiện tại. HUD hiển thị liên tục: HP (số nguyên), Cup (tiến độ thắng), và resource widget thay đổi icon theo phase (coin trong shop, cup trong combat). Khi có `permanentIncomeBonus > 0`, hiển thị dạng `"12G (+2)"`.

---

### 3.8.2 Giải Phẫu Một Lá Bài — Tám Lớp Thông Tin

```
┌─────────────────────────────┐
│ [NAME]         [TIER ICON]  │ ← tên + tier pool
│                             │
│       [CHARACTER ART]       │ ← nhận diện visual nhanh
│                             │
│ [ABILITY ICON] [KEYWORDS]   │ ← TTE effect + Taunt/Reborn/Safeguard icons
│                             │
│ [ATK]              [HP]     │ ← stats (màu HP đỏ khi bị thương)
│                             │
│ [★ ★ ☆]                    │ ← merge stars (1-3)
│                             │
│ [description text]          │ ← mô tả ability
└─────────────────────────────┘
  ↑ Frame blink vàng = merge hint
```

> **[HÌNH 3.14 — Giải Phẫu Giao Diện Lá Bài]** *Ảnh chụp lá bài unit trong game với 8 thành phần chú thích. Frame blink vàng khi có merge hint.*

Stat Punch Animation khi ATK/HP thay đổi: tăng → số phóng to 1.45× rồi thu lại trong 0.22s, flash xanh lá; giảm → flash đỏ fade về màu gốc trong 0.28s.

---

### 3.8.3 Drag And Drop

Toàn bộ tương tác Shop Phase thực hiện bằng drag and drop — không có menu bối cảnh hay nút mua riêng:

| Destination | Source | Hành vi |
|-------------|--------|---------|
| PlayerBoard (ô trống) | Shop | Mua + Deploy (trừ coin, trigger OnDeploy) |
| PlayerBoard (ô trống) | Hand | Di chuyển + Deploy (không trừ coin) |
| PlayerBoard (ô trống) | PlayerBoard | Đổi chỗ trong board |
| Hand (ô trống) | Shop | Mua vào tay (không Deploy) |
| SellZone | Board/Hand | Bán (+1 coin, trigger OnSell) |
| Unit slot | Spell từ shop | Mua + Cast spell lên unit đó |

SellZone (nền đỏ, chữ "BÁN +1G") chỉ xuất hiện khi đang kéo card từ Board/Hand — contextual UI, ẩn ngay khi thả. Spell cần target được kéo đặt trực tiếp lên unit mục tiêu — cử chỉ kéo-thả là hành động chọn target.

---

### 3.8.4 Phản Hồi Merge

Merge là khoảnh khắc đáng nhấn mạnh — ba lớp phản hồi đồng thời:

1. **BurstAnimation:** Lá bài sau merge phát hiệu ứng bùng phát
2. **StarUp sound effect:** Âm thanh riêng biệt cho khoảnh khắc lên sao
3. **Token "Tinh Hoa Hợp Nhất":** `S_00` tự xuất hiện trong Hand — phần thưởng gameplay thực sự, không chỉ visual

**Merge Hint** — khung của lá bài nhấp nháy đều đặn, chuyển dần qua lại giữa màu gốc và màu vàng kim (khoảng bốn nhịp mỗi giây), bất cứ khi nào người chơi chỉ còn thiếu đúng một bản sao để hoàn thành một lượt merge. Đây là một tín hiệu thị giác có chủ đích được "căn chỉnh" ở mức tinh tế: đủ liên tục để không bị bỏ lỡ giữa nhịp độ nhanh của Shop Phase, nhưng đủ nhẹ nhàng để không gây cảm giác phiền nhiễu hay thúc ép.

---

### 3.8.5 Ngôn Ngữ Màu Sắc

| Màu | Ý nghĩa | Nơi dùng |
|-----|---------|----------|
| Xanh lá (`#40FF58`) | Buff / tăng stat | Stat punch tăng, SynergyNiles flash |
| Đỏ (`#FF2626`) | Debuff / giảm stat / nguy hiểm | Stat punch giảm, HP damaged, SellZone |
| Xanh dương (`#4D99FF`) | Trạng thái / thông tin | Status effect flash |
| Vàng kim (`#FFD90D`) | Babylon synergy / merge hint | Babylon buff flash, blink frame |
| Cyan (`#1AD9FF`) | Olympus synergy / freeze active | Olympus buff flash, Lock button active |
| Lục (`#1AFF73`) | Niles synergy | Niles buff flash |
| Trắng | Bình thường | HP đầy, stat gốc |

Hệ màu nhất quán cho phép đọc trạng thái combat mà không cần đọc text. DifficultyPanel (Easy/Medium/Hard) và EndGamePanel (Thắng/Thua + Restart) hoàn thiện luồng trải nghiệm tối giản: chọn độ khó → chơi → nhận kết quả → restart.

---

*[Kết thúc Chương 3 — Thiết Kế Game (GDD)]*
*[Tiếp theo: Chương 4 — Kiến Trúc Hệ Thống Kỹ Thuật]*
