## 3.5 Hệ Thống Kinh Tế

### 3.5.1 Thu Nhập Và Chi Tiêu

Mỗi lượt, người chơi nhận coin theo ba thành phần độc lập:

```
CurrentCoin = 10  +  bonusNextTurn  +  permanentIncomeBonus
bonusNextTurn ← 0 sau khi áp dụng
```

- **Base 10:** Cố định, không có streak bonus hay interest. Mọi quyết định kinh tế tập trung vào chi tiêu, không tích lũy.
- **bonusNextTurn:** Thưởng trì hoãn từ spell (ví dụ *Wager* +3 coin nếu thắng trận tiếp theo).
- **permanentIncomeBonus:** Cộng dồn suốt ván từ spell đặc biệt (ví dụ *Caishen's Knock* +1/lượt, mua ở lượt 5 → hòa vốn lượt 7, sinh lãi từ lượt 8).

Coin rời tài khoản qua bốn kênh:

| Kênh | Chi phí | Giá trị nhận được | Loại đầu tư |
|------|---------|-------------------|-------------|
| Mua unit | = `card.cost` | Unit lên sân/tay | Sức mạnh tức thời / merge tiến độ |
| Mua spell | = `spell.cost` | Hiệu ứng tức thì | Tùy loại spell |
| Reroll | 1 | 7 lá bài mới | Thông tin và cơ hội |
| Bán unit | −1 (nhận) | +1 coin + giải phóng slot | Tái phân bổ nguồn lực |

---

### 3.5.2 Phân Loại Spell Theo Chức Năng Kinh Tế

21 spell phục vụ sáu chức năng kinh tế khác nhau:

**Nhóm 1 — Buff Chỉ Số Trực Tiếp**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Sharpen Blade | 1 | 1 | +3/+0 vĩnh viễn cho 1 đồng minh |
| Plated Armor | 1 | 1 | +0/+3 + Taunt vĩnh viễn cho 1 đồng minh |
| Let's Feast | 1 | 2 | +0/+2 cho 3 đồng minh trên board (trận này) |
| Balance Stance | 1 | 2 | +3/+3 vĩnh viễn cho 1 đồng minh |
| Strengthen Bond | 2 | 1 | +1/+1 vĩnh viễn cho toàn bộ đồng minh |
| Divine Inspiration | 3 | 2 | ATK+HP = shop tier hiện tại cho 1 đồng minh |
| Olympic Flame | 4 | 3 | Nhân đôi ATK + Safeguard cho 1 đồng minh |
| Change of Heart | 2 | 2 | Taunt → xóa Taunt +3/+0 \| không Taunt → +Taunt +0/+5 |

**Nhóm 2 — Tuyển Quân (Recruiting)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Quick Recruit | 1 | 2 | Nhận 1 unit ngẫu nhiên Tier 1 vào Hand |
| Sanctum Heist | 1 | 2 | Lấy 1 unit ngẫu nhiên từ Shop hiện tại vào Hand |
| Tailored Recruit | 2 | 3 | Chọn 1 đồng minh → nhận 1 unit cùng tộc khác |
| Military Support | 5 | 5 | Nhận 3 unit ngẫu nhiên Tier 5 vào Hand |

**Nhóm 3 — Kinh Tế Coin**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Ancient Coin | 1 | 1 | Nhận 1 coin ngay |
| Trader's Trick | 2 | 1 | +2 coin lượt này |
| Caishen's Knock | 2 | 2 | +1 thu nhập vĩnh viễn/lượt |

**Nhóm 4 — Nâng Cấp Unit**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Rising Spirit | 3 | 2 | Grant EndTurnShop: +2/+2 mỗi lượt cho 1 đồng minh |
| Ritual of the Realm | 3 | 3 | Nâng 1 unit cùng tộc lên +1 merge level |

**Nhóm 5 — Tái Phân Bổ**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Gate of Destruction | 4 | 3 | Loại bỏ 1 đồng minh → toàn chỉ số chuyển cho 1 đồng minh ngẫu nhiên |

**Nhóm 6 — Rủi Ro Cao**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Devil's Deal | 4 | 1 | Mất 1 Mạng → nhận 6 coin ngay |
| Wager | 2 | 1 | Nếu thắng trận kế tiếp → nhận 3 coin |

*Wager* — kỳ vọng giá trị: `E = 4p − 2` (dương khi xác suất thắng trận tiếp theo p > 0.5) — một phép đặt cược thuần túy, phần thưởng phụ thuộc vào một sự kiện ngẫu nhiên trong tương lai mà người chơi không kiểm soát hoàn toàn. Đây cũng chính là kiểu giá trị mà mô hình đánh giá bài của hệ thống AI *không thể* nắm bắt được bằng một phép chấm điểm tĩnh trên từng lá riêng lẻ — mục 5.3.3 bàn đến đây như một điểm mù có chủ đích của mô hình, chứ không phải một thiếu sót.

**Permanent vs one-time:** Cùng coin, permanent luôn đáng hơn vì tồn tại qua mọi lượt còn lại. *Balance Stance* (2 coin → +3/+3 vĩnh viễn) vs *Let's Feast* (2 coin → +0/+2 cho 3 unit trận này). Với 15 lượt còn lại, Balance Stance tương đương 15× giá trị chiến đấu từ cùng chi phí.

---

### 3.5.3 Sơ Đồ Lưu Thông Coin

```
┌──────────────────────────────────────────────────────┐
│                  NGUỒN THU NHẬP                       │
│                                                      │
│  Base 10/lượt ──┐                                    │
│  bonusNextTurn ─┼──► CurrentCoin                     │
│  permanentBonus─┘                                    │
│  Bán unit (+1) ─────► CurrentCoin                    │
│  Thắng Wager ───────► CurrentCoin                    │
│                                                      │
├──────────────────────────────────────────────────────┤
│                  KÊNH CHI TIÊU                        │
│                                                      │
│  Mua unit ◄──── CurrentCoin ────► Mua spell          │
│  Reroll (1) ◄────────────────────► (giữ = không chi) │
│                                                      │
├──────────────────────────────────────────────────────┤
│              GIÁ TRỊ TẠO RA (Output)                 │
│                                                      │
│  Unit trên sân → Combat power → Cup / HP              │
│  Spell stat    → Combat power (permanent/one-time)   │
│  Spell recruit → Unit mới (→ merge tiến độ)          │
│  Spell economy → Nhiều coin lượt sau (tái đầu tư)    │
│  Reroll info   → Cơ hội tìm unit/merge bản sao       │
└──────────────────────────────────────────────────────┘
```

Không có "cách chơi kinh tế sai" hoàn toàn — mỗi kênh tạo ra giá trị theo cách khác nhau. Câu hỏi là "loại giá trị nào cần nhất lúc này?" — đó là bài toán mà Chromosome trong Chương 5 học cách trả lời qua hàng nghìn trận mô phỏng.

---

*[Tiếp theo: Mục 3.6 — Hệ Thống Chiến Đấu]*
