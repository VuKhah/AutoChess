## 3.5 Hệ Thống Kinh Tế

Mục 2.4 đã phân tích kinh tế học trong Auto Chess từ góc độ lý thuyết — bài toán quyết định tuần tự, tradeoff tempo–economy, và cách 8 gene trong chromosome mã hóa chính sách kinh tế của bot AI. Mục này tiếp cận cùng hệ thống từ góc độ thiết kế game: *những quy tắc cụ thể nào tạo nên hệ sinh thái kinh tế của game, và tại sao chúng được thiết kế như vậy*?

---

### 3.5.1 Nguồn Thu Nhập — Ba Thành Phần Độc Lập

Mỗi lượt, người chơi nhận coin theo công thức ba thành phần:

```
CurrentCoin = 10  +  bonusNextTurn  +  permanentIncomeBonus
bonusNextTurn ← 0 sau khi áp dụng
```

**Thành phần 1 — Base income = 10:**
Cố định hoàn toàn, không thay đổi theo lượt, không phụ thuộc vào kết quả chiến đấu. So sánh với các Auto Chess thương mại: TFT có hệ thống interest (giữ gold = nhận lãi) và streak bonus (liên thắng/liên thua); Hearthstone Battlegrounds có gold tăng dần theo lượt cứng. Thiết kế này chọn mô hình **phẳng và đơn giản** — loại bỏ ưu thế tích lũy từ việc "chơi đúng meta tài chính" và đặt mọi quyết định kinh tế vào trục chi tiêu thay vì trục tích lũy.

**Thành phần 2 — bonusNextTurn (bonus một lần):**
Khoản thưởng trì hoãn do spell tạo ra, chỉ cộng vào lượt tiếp theo rồi bị xóa. Ví dụ: *Trader's Trick* (Tier 2, giá 1) cộng ngay +2 coin vào thu nhập lượt này; *Wager* (Tier 2, giá 1) cộng +3 coin vào lượt sau nếu thắng trận kế tiếp. Đây là thu nhập "tiêu ngay lần sau" — giá trị rõ ràng nhưng không tích lũy.

**Thành phần 3 — permanentIncomeBonus (bonus vĩnh viễn):**
Phần thưởng cộng dồn suốt ván đấu từ các spell đặc biệt. Ví dụ tiêu biểu: *Caishen's Knock* (Tier 2, giá 2) tăng vĩnh viễn +1 thu nhập mỗi lượt. Đây là đầu tư dài hạn — với 20 lượt tối đa, mỗi +1 thu nhập vĩnh viễn tương đương nhận thêm tối đa 20 coin trong suốt ván, dù chỉ trả 2 coin để mua spell.

Ba thành phần này độc lập và cộng hưởng với nhau: một người chơi đầu tư 2 coin vào Caishen's Knock ở lượt 5 sẽ thu hồi 2 coin sau 2 lượt và có lãi từ lượt 8 trở đi. Đây là mô hình ROI (return on investment) thực sự trong game — không phải ẩn dụ.

---

### 3.5.2 Các Kênh Chi Tiêu — Taxonomy Đầy Đủ

Coin rời khỏi tài khoản người chơi qua bốn kênh, mỗi kênh tạo ra loại giá trị khác nhau:

| Kênh | Chi phí | Giá trị nhận được | Loại đầu tư |
|------|---------|-------------------|-------------|
| Mua unit | = `card.cost` | Unit trên sân/tay | Sức mạnh tức thời hoặc merge tiến độ |
| Mua spell | = `spell.cost` | Hiệu ứng tức thì (buff/coin/unit) | Tùy loại spell (xem 3.5.3) |
| Reroll | 1 | 7 lá bài mới | Thông tin và cơ hội |
| Bán unit | −1 (nhận) | +1 coin + giải phóng slot | Tái phân bổ nguồn lực |

Lưu ý đặc biệt: **bán unit không phải kênh "chi tiêu"** mà là kênh thu hồi — người chơi nhận lại đúng 1 coin bất kể đã trả bao nhiêu khi mua. Quyết định thiết kế này đã được phân tích trong Mục 3.3.5: loại bỏ sunk cost và tập trung quyết định vào giá trị hiện tại.

**Chi phí reroll không đổi** (luôn 1 coin) là điểm quan trọng: reroll không bao giờ "đắt hơn" hay "rẻ hơn" dù ở bất kỳ lượt nào. Điều này giữ cho bài toán reroll nhất quán xuyên suốt ván — không cần người chơi học thêm meta "lượt nào reroll được/không được".

---

### 3.5.3 Phân Loại Spell Theo Chức Năng Kinh Tế

21 spell trong game phục vụ sáu chức năng kinh tế khác nhau. Bảng dưới đây tổ chức toàn bộ spell theo nhóm chức năng — mỗi nhóm có logic đánh giá và timing sử dụng riêng:

**Nhóm 1 — Buff Chỉ Số Trực Tiếp (Stat Buff)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Sharpen Blade | 1 | 1 | +3/+0 vĩnh viễn cho 1 đồng minh |
| Plated Armor | 1 | 1 | +0/+3 + Taunt vĩnh viễn cho 1 đồng minh |
| Let's Feast | 1 | 2 | +0/+2 cho 3 đồng minh trên board (trận này) |
| Balance Stance | 1 | 2 | +3/+3 vĩnh viễn cho 1 đồng minh |
| Strengthen Bond | 2 | 1 | +1/+1 vĩnh viễn cho toàn bộ đồng minh |
| Divine Inspiration | 3 | 2 | ATK+HP = shop tier hiện tại cho 1 đồng minh |
| Olympic Flame | 4 | 3 | Nhân đôi ATK + Safeguard cho 1 đồng minh |
| Change of Heart | 2 | 2 | Taunt → xóa Taunt +3/+0 | không Taunt → +Taunt +0/+5 |

Đây là nhóm spell "an toàn" nhất — giá trị xác định, không có rủi ro, hiệu quả ngay lập tức. *Sharpen Blade* (1 coin → +3 ATK vĩnh viễn) là spell giá trị nhất trong Tier 1 thuần túy: không có unit tier 1 nào bán với 1 coin cho +3 ATK.

**Nhóm 2 — Tuyển Quân (Recruiting)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Quick Recruit | 1 | 2 | Nhận 1 unit ngẫu nhiên Tier 1 vào Hand |
| Sanctum Heist | 1 | 2 | Lấy 1 unit ngẫu nhiên từ Shop hiện tại vào Hand |
| Tailored Recruit | 2 | 3 | Chọn 1 đồng minh → nhận 1 unit cùng tộc khác |
| Military Support | 5 | 5 | Nhận 3 unit ngẫu nhiên Tier 5 vào Hand |

Nhóm này tạo ra unit mà không tốn thêm slot shop — giá trị chính là **bỏ qua bước reroll**: thay vì trả 1 coin/lần reroll và hy vọng thấy unit cần, trả một lần nhận ngay unit phù hợp. *Sanctum Heist* đặc biệt mạnh vì lấy unit đang có trong shop (đã biết giá trị) thay vì ngẫu nhiên blind — tương đương mua thêm một unit mà không trả coin mua.

**Nhóm 3 — Kinh Tế Coin (Economy)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Ancient Coin | 1 | 1 | Nhận 1 coin ngay |
| Trader's Trick | 2 | 1 | +2 coin lượt này |
| Caishen's Knock | 2 | 2 | +1 thu nhập vĩnh viễn/lượt |

*Ancient Coin* (1 coin → 1 coin) là spell có vẻ vô nghĩa nhưng thực ra là công cụ **timing**: mua khi đang có coin lẻ không đủ để mua unit, đổi ô spell (không cần) lấy coin để cộng vào lượt sau. Giá trị thực = giá trị của ô spell đó × cơ hội bỏ lỡ. *Caishen's Knock* là spell ROI dài hạn điển hình — mua ở lượt 5 hòa vốn ở lượt 7, sinh lãi từ lượt 8 đến cuối ván.

**Nhóm 4 — Nâng Cấp Unit (Upgrade)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Rising Spirit | 3 | 2 | Grant EndTurnShop: +2/+2 mỗi lượt cho 1 đồng minh |
| Ritual of the Realm | 3 | 3 | Nâng 1 unit cùng tộc lên +1 merge level |

*Rising Spirit* là spell có tiềm năng tổng hợp cao nhất — cộng +2/+2 mỗi lượt cộng dồn qua `growthATKBonus`/`growthHPBonus`, không phải buff tạm. Đến lượt 15, unit nhận Rising Spirit lúc lượt 5 đã tích lũy +20/+20 từ spell này, và toàn bộ bonus này được nhân theo mergeLevel khi merge. *Ritual of the Realm* là spell merge tức thì — thay vì tìm thêm bản sao, ép một unit lên 2 sao/3 sao ngay lập tức.

**Nhóm 5 — Tái Phân Bổ (Redistribution)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Gate of Destruction | 4 | 3 | Loại bỏ 1 đồng minh → toàn chỉ số chuyển cho 1 đồng minh ngẫu nhiên |

Spell này là "hy sinh có chủ đích" — loại bỏ unit yếu nhất (hoặc unit đã dùng xong vai trò trigger) để dồn toàn bộ chỉ số vào một unit mạnh hơn. Phù hợp nhất với Babylon deck khi muốn tập trung stat vào một "carry" duy nhất thay vì phân tán.

**Nhóm 6 — Rủi Ro Cao (High Risk)**

| Spell | Tier | Giá | Hiệu ứng |
|-------|------|-----|----------|
| Devil's Deal | 4 | 1 | Mất 1 Mạng → nhận 6 coin ngay |
| Wager | 2 | 1 | Nếu thắng trận kế tiếp → nhận 3 coin |

Nhóm này được xử lý đặc biệt trong BotAgent với điểm đánh giá âm cứng (phân tích trong Mục 2.4.8) — bot mặc định né tránh trừ khi chromosome có gene kinh tế rất cao. Thiết kế hai spell này được phân tích chi tiết trong mục tiếp theo.

---

### 3.5.4 Thiết Kế Spell Rủi Ro Cao — Asymmetric Payoff

*Devil's Deal* và *Wager* là hai spell không nằm trong phổ "an toàn" của phần còn lại — chúng tạo ra tình huống **đánh đổi phi tuyến** mà các spell khác không có.

**Devil's Deal** (Tier 4, giá 1): Đây là giao dịch kinh tế thuần túy — đổi 1 HP lấy 6 coin ngay lập tức. Phân tích trực tiếp: người chơi bắt đầu với 7 HP, cần thắng 10 lần để win. Mỗi HP có giá trị sống còn khác nhau tùy trạng thái ván:

- HP thứ 7→6 (đầu game, HP còn nhiều): thiệt hại biên thấp, 6 coin có thể mua 2 unit tier 3 hoặc reroll 6 lần — đầu tư đáng giá
- HP thứ 2→1 (cuối game, HP nguy hiểm): một thất bại nữa là thua ván — thiệt hại biên cực cao, 6 coin không đủ bù

Vì spell xuất hiện theo drop rate của shop level 4, Devil's Deal thường xuất hiện ở mid-game khi HP trung bình còn 4–6. Đây là sweet spot mà rủi ro và phần thưởng cân bằng nhau — người chơi phải tự đánh giá vị thế của mình.

**Wager** (Tier 2, giá 1): Cấu trúc khác — trả 1 coin để *đặt cược* 3 coin vào trận tiếp theo. Nếu thắng: hiệu quả thu về 3 coin (net +2). Nếu thua: mất 1 coin (net −1). Expected value với xác suất thắng p: `E = 3p − 1(1−p) − 1 = 4p − 2`. Dương khi p > 0.5 — chỉ có lãi khi người chơi tự tin thắng trận tiếp theo trên 50%.

Thiết kế này không phải ngẫu nhiên: nó biến Wager thành **công cụ bày tỏ tự tin**. Người chơi dùng Wager khi nhìn đội hình của mình và đội địch mà tin rằng mình sẽ thắng — đây là một quyết định đòi hỏi game sense thực sự, không thể tối ưu hóa mà không có đánh giá tình huống.

---

### 3.5.5 Permanent Vs. One-Time — Cùng Coin, Giá Trị Khác

Một nguyên tắc thiết kế nhất quán xuyên suốt hệ thống spell là phân biệt **permanent** và **one-time** (hay "this battle"). Cùng amount stats, permanent luôn có giá trị cao hơn vì nó tồn tại qua mọi lượt còn lại:

```
Let's Feast (2 coin): +0/+2 cho 3 unit, CHỈ trận này
Balance Stance (2 coin): +3/+3 cho 1 unit, VĨNH VIỄN
```

Nhìn vào "tổng HP tăng": Let's Feast cho +6 HP tổng lượt này; Balance Stance cho +3 HP nhưng còn lại đến hết ván. Với 15 lượt còn lại, +3 HP vĩnh viễn mang lại tổng 15 lần giá trị chiến đấu, so với 1 lần của Let's Feast. Nhưng Let's Feast nhắm vào 3 unit ngẫu nhiên trên board combat — hữu ích nhất khi combat đang xảy ra và đội hình cần sinh tồn ngay lượt này.

Sự phân biệt này tạo ra quyết định timing: **permanent spells tốt nhất ở đầu game** (tổng hợp nhiều lượt, buff unit sẽ merge), **one-time spells tốt nhất khi cần thắng một trận cụ thể** (emergency heal, cứu unit quan trọng trước khi thua HP).

---

### 3.5.6 Vòng Lưu Thông Kinh Tế — Tổng Quan Flows

Toàn bộ kinh tế của game có thể được tóm gọn trong sơ đồ lưu thông coin:

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

Sơ đồ này làm rõ rằng không có "cách chơi kinh tế sai" hoàn toàn — mỗi kênh chi tiêu tạo ra giá trị theo cách khác nhau. Câu hỏi không phải "nên mua gì?" mà là "loại giá trị nào cần nhất lúc này?" — đó chính là bài toán mà Chromosome trong Chương 5 học cách trả lời qua hàng nghìn trận đấu mô phỏng.

---

*[Tiếp theo: Mục 3.6 — Hệ Thống Chiến Đấu (Combat System)]*
