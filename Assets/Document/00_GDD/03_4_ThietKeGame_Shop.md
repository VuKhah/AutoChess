## 3.4 Hệ Thống Shop

Nếu Combat Phase là nơi người chơi *gặt hái* kết quả của các quyết định trước đó, thì Shop Phase — và đặc biệt là hệ thống shop — là nơi những quyết định đó được đưa ra. Shop không chỉ đơn giản là "cửa hàng mua bài": nó là **giao diện giữa người chơi và sự ngẫu nhiên** — bộ lọc kiểm soát những lá bài nào có thể xuất hiện, theo tỉ lệ nào, và người chơi có thể tác động lên đó đến mức nào.

Phần này đi sâu vào cơ chế sinh ngẫu nhiên của shop — thuật toán lấy mẫu, bảng tỉ lệ xuất hiện, kiến trúc pool — để giải thích tại sao shop tạo ra căng thẳng chiến lược thực sự thay vì chỉ là nguồn cung ngẫu nhiên thuần túy.

---

### 3.4.1 Kiến Trúc Pool Và Phân Tầng Theo Tier

Toàn bộ 47 unit card trong game được tổ chức thành một **pool đơn** (không phân chia theo tribe hay thể loại), chia tầng theo thuộc tính `tier` của mỗi card. Token cards (`isToken = true`) bị loại hoàn toàn khỏi pool — chúng chỉ xuất hiện qua ability triệu hồi, không bao giờ qua shop. Tương tự, 21 spell card tạo thành pool riêng biệt với logic lấy mẫu khác.

Mỗi lượt, shop được điền bằng **5 lần roll độc lập cho unit** và **2 lần roll độc lập cho spell** — mỗi ô shop là một lần rút xổ số độc lập, không liên quan đến ô khác. Điều này có hai hệ quả quan trọng:

**Hệ quả thứ nhất — Trùng lặp là có thể và có ý nghĩa.** Vì 5 ô unit roll độc lập từ cùng pool, hoàn toàn có thể thấy 2–3 bản sao của cùng một card trong cùng một shop. Đây không phải lỗi thiết kế mà là tính năng: nó là cơ chế cốt lõi cho phép người chơi hoàn thành bộ merge mà không cần reroll nhiều lần. Nếu mỗi ô shop đảm bảo card khác nhau, hệ thống merge sẽ mất đi nguồn "may mắn" của nó.

**Hệ quả thứ hai — Phân bố xác suất không đồng đều.** Không phải mọi tier đều có cùng số lượng card. Tier thấp (1–2) có ít card hơn tier cao, nghĩa là khi một ô roll vào tier thấp ở shop level cao, xác suất gặp một card cụ thể trong tier đó lại *cao hơn* — dễ tìm bản sao hơn để merge unit tier thấp ở giai đoạn late-game.

---

### 3.4.2 Thuật Toán Weighted Random — RollTier

Cơ chế trung tâm kiểm soát "card tier nào xuất hiện" là hàm `RollTier(shopLevel)` — một weighted random đơn giản dựa trên **bảng tỉ lệ 6×6**:

```
shopDropRates[shopLevel - 1][tierIndex] = tỉ lệ % xuất hiện tier đó
```

| Shop Level | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 | Tier 6 |
|:----------:|:------:|:------:|:------:|:------:|:------:|:------:|
| 1          | 100%   | 0%     | 0%     | 0%     | 0%     | 0%     |
| 2          | 70%    | 30%    | 0%     | 0%     | 0%     | 0%     |
| 3          | 50%    | 35%    | 15%    | 0%     | 0%     | 0%     |
| 4          | 25%    | 40%    | 25%    | 10%    | 0%     | 0%     |
| 5          | 15%    | 25%    | 35%    | 15%    | 10%    | 0%     |
| 6          | 10%    | 15%    | 20%    | 25%    | 20%    | 10%    |

Thuật toán: roll số ngẫu nhiên trong [0, 99], cộng dồn tỉ lệ từng tier cho đến khi tổng vượt giá trị đó.

```csharp
int roll = Random.Range(0, 100);
int cumulative = 0;
for (int tierIndex = 0; tierIndex < 6; tierIndex++) {
    cumulative += shopDropRates[levelIndex, tierIndex];
    if (roll < cumulative) return tierIndex + 1;
}
```

Sau khi có kết quả tier, hệ thống lọc pool: `unitList.Where(c => c.tier == rolledTier && !c.isToken)`, rồi chọn ngẫu nhiên đều (uniform) từ danh sách đó.

**Đọc bảng theo chiều dọc:** tỉ lệ xuất hiện của từng tier *chuyển dịch dần* theo shop level thay vì bị cắt đột ngột. Tier 1 không biến mất hoàn toàn kể cả ở shop level 6 — vẫn còn 10%. Điều này có nghĩa là ở giai đoạn late-game, người chơi vẫn có thể tìm thêm bản sao để merge unit tier thấp đang nuôi, dù xác suất giảm đáng kể. Ngược lại, tier 6 chỉ xuất hiện ở shop level tối đa (10%) — unit mạnh nhất là phần thưởng hiếm cho người chơi đã sống sót đủ lâu.

**Đọc bảng theo chiều ngang:** ở shop level 4, tier 2 là phổ biến nhất (40%) — đây là "tier trung tâm" của mid-game khi người chơi đang xây dựng core đội hình. Ở shop level 6, tier 4 chiếm 25% — cao nhất ở late-game — phản ánh thiết kế: unit tier 4 là "backbone" của đội hình cuối trận, đủ mạnh để chiến nhưng đủ phổ biến để merge được.

---

### 3.4.3 Cơ Chế Fallback — Shop Không Bao Giờ Trống

Một thiết kế quan trọng nhưng ít được chú ý là cơ chế **fallback** khi pool tier đang tìm không có card nào:

```csharp
// Unit: fallback về bất kỳ tier nào ≤ rolled tier
List<CardDefinition> tierPool = unitList.Where(c => c.tier == rolledTier && !c.isToken).ToList();
if (tierPool.Count == 0)
    tierPool = unitList.Where(c => c.tier <= rolledTier && !c.isToken).ToList();

// Spell: fallback về bất kỳ spell nào (không cần đúng tier)
if (tierPool.Count == 0)
    tierPool = spellList.Where(c => !c.isToken).ToList();
```

Fallback unit chỉ "hạ tier" (không bao giờ lấy tier cao hơn đã roll) — đảm bảo người chơi không nhận được card vượt quá kỳ vọng của shop level hiện tại. Fallback spell không phân biệt tier — bất kỳ spell nào cũng hợp lệ — vì spell ít phụ thuộc vào tier hơn và pool spell nhỏ hơn nhiều.

Trong thực tế với 47 unit card được phân bổ đều qua 6 tier, fallback hiếm khi xảy ra. Nhưng trong các edge case như training với pool card nhỏ hơn hay các điều kiện test đặc biệt, cơ chế này đảm bảo shop luôn đủ 5 ô unit — không bao giờ có "ô trống do thiếu card".

---

### 3.4.4 Đóng Băng Shop (Freeze) — Cam Kết Với Tương Lai Đã Biết

Freeze là hành động tốn **0 coin** nhưng có chi phí ẩn: từ bỏ quyền xem 7 lá mới đầu lượt tiếp theo. Đổi lại, các lá bài trong shop hiện tại được giữ nguyên.

Về mặt kỹ thuật, freeze không "khóa" shop theo nghĩa cứng — nó chỉ đặt `isShopFrozen = true`. Đầu lượt tiếp theo, thay vì gọi `RefreshShop()` (thay toàn bộ), hệ thống gọi `FillEmptyShopSlots()` — chỉ tìm và điền vào các ô đang trống (những ô người chơi đã mua). Sau khi điền xong, `isShopFrozen` được reset về `false` — freeze chỉ có hiệu lực một lượt, không tự động kéo dài.

```
Freeze trước khi nhấn Fight
  → Đầu lượt tiếp theo: FillEmptyShopSlots()
       • Ô đã mua (trống) → điền card mới theo shopTier hiện tại
       • Ô chưa mua (còn card) → giữ nguyên
  → isShopFrozen = false
```

Freeze tạo ra hai phép tính chiến lược đối lập:

**Khi nên freeze:** Người chơi nhìn thấy trong shop một lá bài có giá trị cao (bản sao còn thiếu để merge, unit đặc biệt phù hợp với đội hình) nhưng lượt này chưa đủ coin để mua. Freeze đảm bảo lá đó còn đó lượt sau — đổi lại mất cơ hội xem 7 lá ngẫu nhiên khác có thể tốt hơn.

**Khi không nên freeze:** Nếu shop hiện tại không có gì đặc biệt, freeze là phí lượt — tốt hơn để shop refresh tự nhiên và tìm kiếm trong pool mới. Tương tự, reroll ngay trong lượt hiện tại hủy freeze (`isShopFrozen = false`), nên không thể vừa freeze vừa reroll để "xem thêm".

Khác với reroll (tốn 1 coin, chủ động), freeze là công cụ **tiết kiệm thông tin** — người chơi trả bằng sự không chắc chắn của lượt tiếp theo để đảm bảo lá bài cụ thể họ đang muốn. Trong bài toán quyết định tuần tự dưới sự không chắc chắn (đã phân tích trong Mục 2.4), freeze là cách giảm không gian trạng thái của lượt sau: thay vì 7 biến ẩn, người chơi biết trước ít nhất một phần của shop.

---

### 3.4.5 Shop Và Hệ Thống Global Tribe Buff

Một tương tác quan trọng giữa shop và hệ thống bộ tộc thường ít được nhận ra: mỗi `CardInstance` mới tạo ra trong shop — kể cả khi chưa mua — tự động nhận toàn bộ **global tribe buff** đã tích lũy từ trước đó.

```csharp
// GameManager.Shop.cs — CreateCardInSlot()
CardInstance instance = new CardInstance(data, 0);
ApplyGlobalPermBuffToNewUnit(instance);  // áp ngay khi tạo
```

Điều này có nghĩa là: nếu Thoth đã chết và buff +2 ATK cho toàn bộ Niles, thì bất kỳ unit Niles nào *xuất hiện trong shop* sau đó cũng đã có sẵn +2 ATK — ngay cả trước khi được mua. Người chơi thấy card hiển thị chỉ số đã được buff, không phải chỉ số base. Thiết kế này đảm bảo thông tin trên màn hình phản ánh đúng thực tế: "card này nếu mua sẽ có ATK bao nhiêu?" luôn cho ra con số chính xác, không cần tính thêm trong đầu.

Quyết định kỹ thuật này cũng có hệ quả thiết kế: global tribe buff trở thành *lợi thế mua sắm trong tương lai*, không chỉ là lợi thế chiến đấu hiện tại. Thoth chết sớm vẫn có giá trị — nó đã khóa +2 ATK vào mọi Niles unit mà người chơi sẽ mua cho đến hết ván.

---

### 3.4.6 Thiết Kế Cân Bằng — 5 Unit Và 2 Spell

Tỉ lệ **5 unit : 2 spell** trong mỗi shop không phải ngẫu nhiên. Nó phản ánh vai trò thứ yếu nhưng không thể thiếu của spell trong hệ sinh thái kinh tế:

Nếu spell chiếm 0 ô: người chơi hoàn toàn không thể tương tác với tầng meta-economy, game mất chiều sâu chiến lược.

Nếu spell chiếm 3–4 ô: mỗi lượt có quá nhiều spell xuất hiện, người chơi thường xuyên thấy những spell không cần thiết và cảm thấy mình bị "ép mua spell" thay vì tập trung xây unit.

**2 ô spell** là điểm cân bằng: đủ để spell xuất hiện thường xuyên (trung bình 2 spell/lượt), nhưng không đủ để lấn át unit. Người chơi biết rằng mỗi lượt sẽ thấy 2 spell — đủ để lên kế hoạch kinh tế — nhưng nếu không có spell nào phù hợp, mất đi 2 ô không ảnh hưởng nhiều đến chiến lược unit tổng thể.

Tỉ lệ này cũng ảnh hưởng đến **giá trị của reroll**: với 7 lá (5 unit + 2 spell), mỗi reroll thay thế gần như toàn bộ shop. Đây là mức đủ lớn để reroll có cảm giác "bắt đầu lại", không phải chỉ thay thế một vài ô, đủ nhỏ để 1 coin trả cho 7 lá mới không bị cảm giác "quá rẻ".

---

*[Tiếp theo: Mục 3.5 — Hệ Thống Kinh Tế]*
