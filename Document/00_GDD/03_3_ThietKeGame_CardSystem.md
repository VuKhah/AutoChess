## 3.3 Hệ Thống Bài (Card System)

Bài là đơn vị nội dung cơ bản của game. Sức mạnh của một lá bài không đến từ một thuộc tính duy nhất mà từ sự cộng hưởng của nhiều lớp: chỉ số cơ bản, tribe synergy, passive keywords, ability, và mức merge tích lũy.

---

### 3.3.1 Hai Loại Bài — Unit và Spell

**Unit card (47 lá):** Đặt lên sân, tham chiến. Thuộc tính định nghĩa trong JSON:

| Thuộc tính | Kiểu | Ý nghĩa |
|------------|------|---------|
| `cardID` | string | Khóa định danh duy nhất |
| `tribe` | enum | None / Babylon / Olympus / Niles |
| `baseATK` / `baseHP` | int | Chỉ số trước buff |
| `cost` | int | Coin mua trong shop |
| `tier` | int 1–6 | Cấp độ trong pool |
| `hasTaunt` / `hasReborn` / `hasSafeguard` | bool | Passive keywords |
| `abilities[]` | AbilityData[] | Kỹ năng theo mô hình TTE |
| `isToken` | bool | Không xuất hiện trong shop |

> **[HÌNH 3.6 — Giải Phẫu Lá Bài Unit]** *Ảnh chụp lá bài unit (ví dụ: Gilgamesh) với 8 thành phần chú thích: Tên, Tier icon, Character art, Ability icon + keywords, ATK, HP, Merge stars, Description text.*

**Spell card (21 lá):** Không có ATK/HP, không chiếm slot sân, mua xong dùng ngay rồi biến mất. Mỗi lượt shop có 2 ô spell — tạo quyết định thường xuyên: spell hay reroll/mua unit?

---

### 3.3.2 Hệ Thống Bộ Tộc (Tribe Synergy)

Synergy trong game là *emergent* (tự nổi lên), không phải *threshold* (ngưỡng cứng như TFT). Nhiều unit cùng bộ tộc → nhiều trigger cùng bộ tộc xảy ra → buff tích lũy nhiều hơn. Sức mạnh tribe tỉ lệ thuận *mềm* với số lượng unit.

**Babylon — cộng sinh qua vòng kinh tế:** Buff lẫn nhau khi có sự kiện deploy/sell.
- *Utu* (Tier 6): đồng minh Babylon deploy → toàn Babylon +2/+2 vĩnh viễn
- *Ashur* (Tier 5): bất kỳ đồng minh bị bán → toàn đội +1/+2 vĩnh viễn
- *Lamashtu/Uridimmu*: buff ATK/HP Babylon mỗi khi đồng minh Babylon bị bán

Chiến lược điển hình: *snowball* dài hạn — yếu đầu game, khó chặn ở late-game sau nhiều lượt trigger.

**Niles — cộng sinh qua cái chết và tái sinh:** Buff lẫn nhau qua summon/reborn/death.
- *Anubis* (Tier 6): đồng minh chết → ban Reborn cho đồng minh HP thấp nhất (max 2 lần)
- *Osiris* (Tier 6): đồng minh hồi sinh → nhân đôi/ba/bốn chỉ số unit đó
- *Sobek* (Tier 5): đồng minh summon/reborn → bản thân +1/+2 vĩnh viễn
- *Thoth* (Tier 5, Reborn): khi bị tiêu diệt → toàn bộ Niles nhận vĩnh viễn +2 ATK — kể cả những unit được mua *sau* thời điểm đó, nhờ cơ chế ghi nhận buff ở cấp độ toàn bộ tộc (xem mục 3.4.3)

Chiến lược điển hình: *chaos resilience* — đội hình dễ chết nhưng mỗi cái chết kích hoạt chuỗi buff và hồi sinh.

**Olympus — tấn công thuần túy *(thiết kế dự kiến)*:** ATK synergy qua combat events. *Lưu ý: phiên bản hiện tại chưa có unit Olympus; gene[19] (sOlympus) được giữ trong kiến trúc để mở rộng về sau.*

> **[HÌNH 3.7 — So Sánh Ba Triết Lý Tribe Synergy]** *Bảng 3 cột: Babylon/Niles/Olympus theo trigger chính, giai đoạn mạnh nhất, rủi ro, unit tiêu biểu.*

---

### 3.3.3 Passive Keywords

Ba keyword tồn tại từ đầu đến hết combat trừ khi bị tiêu thụ.

**Taunt — buộc phải bị nhắm:** Một unit mang Taunt luôn đứng đầu thứ tự ưu tiên chọn mục tiêu của đối phương — vượt qua mọi quy tắc khoảng cách hay hàng đầu/hàng sau thông thường (xem chi tiết về cơ chế chọn mục tiêu ở mục 3.6.4). Taunt không nhất thiết phải là một thuộc tính bẩm sinh: nó có thể được *trao* cho một unit khác thông qua hiệu ứng buff — biến unit đó thành một "cái bẫy" buộc đối phương phải tấn công đúng nơi người chơi muốn.

**Reborn — hồi sinh một lần:** Khi một unit mang Reborn gục ngã, nó hồi sinh ngay lập tức với đúng 1 HP — và "cơ hội thứ hai" này chỉ tồn tại đúng một lần trong mỗi trận. Điều thú vị về mặt thiết kế nằm ở trình tự: unit vẫn được tính là *đã chết hợp lệ* và kích hoạt trọn vẹn kỹ năng "khi chết" của nó *trước khi* hồi sinh — combo Horus (buff toàn đội khi chết) + Reborn nghĩa là phần thưởng đó kích hoạt hai lần chỉ từ một unit, trong cùng một trận đấu.

**Safeguard — chặn một đòn:** Một lá chắn dùng một lần — vô hiệu hóa hoàn toàn sát thương của đòn tấn công kế tiếp nhắm vào unit mang nó, sau đó tự tiêu biến.

Sự kết hợp giữa các keyword tạo ra những lớp tương tác sâu hơn nhiều so với tổng các phần riêng lẻ: **Taunt + Reborn** buộc đối phương phải tốn trọn hai lượt giao tranh chỉ để hạ gục một unit; **Reborn + kỹ năng "khi chết"** biến một lần gục ngã thành cơ hội kích hoạt hợp lệ một phần thưởng mạnh, trong khi unit vẫn tiếp tục chiến đấu ngay sau đó.

---

### 3.3.4 Hệ Thống Merge — Đầu Tư Dài Hạn

```
mergeLevel 0→1 (1 sao → 2 sao): cần 3 bản sao
mergeLevel 1→2 (2 sao → 3 sao): cần 2 bản sao
```

**Công thức chỉ số sau merge:**

```
currentATK = round( baseATK × tier  +  0.7 × (growthATKBonus + permanentATKBonus) )
             + tempSpellATKBonus + globalPermATKBonus

maxHP      = round( baseHP × tier   +  0.7 × (growthHPBonus  + permanentHPBonus)  )
             + tempSpellHPBonus + globalPermHPBonus
```

Tier = mergeLevel + 1: 2 sao → chỉ số nhân đôi, 3 sao → nhân ba. Hệ số `0.7` cho growth/permanent bonus nghĩa là buff tích lũy qua nhiều lượt cũng được nhân theo merge.

Ví dụ — *Kingu* (Babylon Tier 2, ATK 2, HP 2, Taunt + Reborn, không ability):
- Nhìn thuần túy: unit yếu nhất có thể
- Thực tế: deploy → trigger Utu buff toàn Babylon; Taunt + Reborn buộc địch tốn 2 round; 3 sao = tank frontline với Babylon chain trigger x3
- Đây là ví dụ điển hình: lá bài "đơn giản" trở nên quan trọng trong ngữ cảnh đúng

Khi thiếu 1 bản sao để merge, lá bài trong shop nhấp nháy (Merge Hint) — UX tránh bỏ lỡ cơ hội merge.

---

*[Tiếp theo: Mục 3.4 — Hệ Thống Shop]*
