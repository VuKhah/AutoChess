# Luật Game — AutoChess

> Tài liệu này mô tả toàn bộ luật game được triển khai trong source code.
> Mọi con số đều lấy trực tiếp từ code; không có giả định.

---

## 1. Tổng quan

AutoChess là game chiến thuật theo lượt. Mỗi lượt chia làm 2 pha:
**Shop Phase** (mua bán, chuẩn bị đội hình) → **Combat Phase** (tự động chiến đấu).

Người chơi đấu với một đội AI. Trò chơi kết thúc khi đạt điều kiện thắng hoặc thua.

| Chỉ số | Giá trị khởi đầu | Điều kiện kết thúc |
|--------|-----------------|-------------------|
| HP người chơi | 7 | ≤ 0 → **THUA** |
| Cup người chơi | 0 | ≥ 10 → **THẮNG** |
| Số lượt tối đa | — | > 20 lượt → **THẮNG** |

---

## 2. Vòng lặp lượt

```
[Bắt đầu game]
       ↓
  [Shop Phase]  ←──────────────────────┐
  • Nhận 10 coin (+ bonus từ lượt trước)│
  • Shop tự động làm mới (trừ khi bị khoá)│
  • Mua / bán / di chuyển / merge quân  │
  • Nhấn BẮT ĐẦU                       │
       ↓                               │
  [Combat Phase]                       │
  • Quân địch (Bot) xuất hiện           │
  • Tự động chiến đấu đến khi 1 bên hết │
  • Tính kết quả (Cup / HP)             │
  • Khôi phục sân player từ snapshot    │
  • Nhấn LƯỢT TIẾP                     │
       └──────────────────────────────┘
```

---

## 3. Shop Phase

### 3.1 Kinh tế

- Đầu mỗi lượt: **reset về 10 coin** + `bonusNextTurn` (tích từ lượt trước bởi phép kinh tế).
- `bonusNextTurn` bị xoá sau khi cộng vào đầu lượt.

### 3.2 Shop

- **5 ô** hiển thị lá bài ngẫu nhiên từ database.
- Tier của shop tính theo công thức:

```
shopTier = floor((currentTurn + 1) / 2),  kẹp trong [1, 6]

Turn 1–2  → Tier 1
Turn 3–4  → Tier 2
Turn 5–6  → Tier 3
Turn 7–8  → Tier 4
Turn 9–10 → Tier 5
Turn 11+  → Tier 6
```

- Mỗi ô roll độc lập theo **bảng tỷ lệ xuất hiện** dựa trên Shop Tier:

| Shop Tier | T1 | T2 | T3 | T4 | T5 | T6 |
|-----------|----|----|----|----|----|----|
| 1 | 100% | 0 | 0 | 0 | 0 | 0 |
| 2 | 70% | 30% | 0 | 0 | 0 | 0 |
| 3 | 50% | 35% | 15% | 0 | 0 | 0 |
| 4 | 25% | 40% | 25% | 10% | 0 | 0 |
| 5 | 15% | 25% | 35% | 15% | 10% | 0 |
| 6 | 10% | 15% | 20% | 25% | 20% | 10% |

- Nếu không có lá nào ở tier đã roll, hệ thống tự động lấy lá ở tier thấp hơn.
- Shop chứa cả Unit lẫn Magic theo cùng pool.

### 3.3 Làm mới Shop (Roll)

- Chi phí: **1 coin** (cố định, `rollCost`).
- Roll luôn làm mới ngay cả khi shop đang bị khoá — khoá bị xoá đồng thời.

### 3.4 Khoá Shop (Lock)

- Nút Lock bật/tắt trạng thái `isShopFrozen`.
- Khi shop bị khoá: shop **không tự làm mới** vào đầu lượt sau.
- Khoá tự động hết hiệu lực sau 1 lượt (dù không roll).

### 3.5 Mua lá bài

- Kéo lá từ shop → **Slot sân (PlayerBoard)** hoặc **Slot tay (Hand)**.
- Trừ `card.cost` coin. Nếu không đủ tiền → lá bài trả về shop.
- Mua vào sân: kích hoạt sự kiện `OnDeploy`.
- Mua vào tay: không kích hoạt `OnDeploy`.

### 3.6 Bán lá bài

- Kéo lá từ sân hoặc tay → **bất kỳ ô Shop nào**.
- Nhận lại **+1 coin** (cố định).
- Kích hoạt sự kiện `OnSell` trước khi lá bị xoá.

### 3.7 Di chuyển lá bài

- Kéo lá giữa các ô sân và tay tự do (không tốn coin).
- Kéo từ Tay → Sân: kích hoạt `OnDeploy`.
- Kéo giữa sân ↔ sân: không kích hoạt `OnDeploy`.

### 3.8 Sự kiện EndTurnShop

- Fire cho tất cả unit **đang ở trên sân** khi bắt đầu lượt mới (sau combat kết thúc).
- Coin đã được reset trước khi event này fire — khả năng `AddCoin` từ event có tác dụng ngay lượt này.

---

## 4. Hệ thống Merge

- Khi có **≥ 3 lá cùng cardID cùng mergeLevel** trên sân + tay: **tự động merge**.
- **Keeper** = lá có tổng `permanentATKBonus + permanentHPBonus + growthATKBonus + growthHPBonus` cao nhất → giữ lại và nâng cấp.
- 2 lá còn lại bị xoá.
- Keeper: `mergeLevel++`, reset chỉ số theo công thức mới.
- Sau merge, hệ thống kiểm tra tiếp (cascade) nếu merge mới vừa tạo thêm bộ 3.
- **mergeLevel** tối đa không bị giới hạn trong code hiện tại.

### Công thức chỉ số sau merge/reset

```
tier = mergeLevel + 1
currentATK = round(baseATK × tier + 0.7 × (growthATKBonus + permanentATKBonus))
maxHP      = round(baseHP  × tier + 0.7 × (growthHPBonus  + permanentHPBonus))
currentHP  = maxHP
```

---

## 5. Loại lá bài

### 5.1 Unit

Quân chiến đấu. Có `baseATK`, `baseHP`, `cost`, `tier`, `tribe`, `abilities`, và 3 keyword: `Taunt`, `Reborn`, `Safeguard`.

### 5.2 Magic

Lá phép. Không tham chiến. Dùng bằng cách kéo lên một Unit đang ở trên sân.

| magicGroup | Hiệu ứng |
|-----------|---------|
| `StatBoost` | +`statBonusATK` ATK / +`statBonusHP` HP vĩnh viễn cho unit |
| `AddAbility` | Thêm ability vào danh sách abilities của unit |
| `AddTaunt` | Grant Taunt cho unit |
| `RemoveTaunt` | Xoá Taunt của unit |
| `Economy` | +1 `bonusNextTurn` (coin nhận đầu lượt sau) |

- Sau khi áp dụng: `ResetStats()` được gọi → chỉ số hiển thị cập nhật ngay.
- Lá phép cũng có thể cất vào Tay (Hand) để dùng sau.
- Kéo từ Shop → Unit: trừ tiền trước, áp dụng, tiêu huỷ lá phép.
- Kéo từ Tay → Unit: miễn phí (đã mua), áp dụng, tiêu huỷ.

---

## 6. Combat Phase

### 6.1 Chuẩn bị

1. **Snapshot** sân player (bao gồm vị trí slot và CardInstance).
2. **Tạo đội địch** (Bot AI quyết định đội hình từ shop ngẫu nhiên cùng tier).
3. Sync board từ UI → list logic.
4. Áp dụng **Tribe Synergy** cho cả 2 đội.
5. Fire `StartOfBattle` + `Aura` cho tất cả unit còn sống (player trước, địch sau).
6. Xử lý mọi cái chết phát sinh từ bước 5 (Death Stack).

### 6.2 Vòng lặp tấn công

- Tối đa **50 round**.
- Mỗi round:
  1. **BuildAttackStack** — tạo danh sách tấn công xen kẽ theo slot:
     - Thứ tự pop: P[0], E[0], P[1], E[1], ..., P[5], E[5]
     - Unit phải: còn sống, `currentATK > 0`, có mục tiêu hợp lệ.
  2. **Pop từng intent**, thực thi:
     - Bỏ qua nếu attacker đã chết.
     - Nếu defender đã chết: tìm mục tiêu thay thế gần nhất.
     - Thực thi `ExecuteClash`.
     - Scan deaths → Flush Death Stack ngay sau mỗi đòn.
     - Early-exit nếu một bên bị xóa sổ.
  3. Kiểm tra kết thúc: một bên không còn unit nào sống → thoát vòng lặp.

### 6.3 ExecuteClash (1 đòn tấn công)

```
dmgToDefender = attacker.currentATK
dmgToAttacker = defender.currentATK   (phản đòn)

Nếu defender có Safeguard → dmgToDefender = 0, xoá Safeguard
Nếu attacker có Safeguard → dmgToAttacker = 0, xoá Safeguard

defender.currentHP -= dmgToDefender
attacker.currentHP -= dmgToAttacker

Nếu attacker còn sống → fire OnAttack(attacker)
Nếu attacker còn sống và bị damage → fire OnTakeDamage(attacker)
Nếu defender còn sống và bị damage → fire OnTakeDamage(defender)
```

### 6.4 Chọn mục tiêu (FindTarget)

- **Ripple Search**: lan từ slot của attacker ra 2 bên (trái → phải, cân bằng khoảng cách).
- Nếu đội địch có bất kỳ unit nào **Taunt**: chỉ xét các unit Taunt.
- Nếu không có Taunt: xét tất cả unit còn sống.
- Ưu tiên unit Taunt **gần slot nhất**, không phải đầu list.

### 6.5 Kết quả Combat

| Kết quả | Thay đổi |
|---------|---------|
| Player thắng (địch bị xóa sổ trước) | `playerCups++` |
| Player thua (player bị xóa sổ trước) | `playerHP--` |
| Hòa (cả 2 cùng chết hoặc 50 round hết) | Không thay đổi |

---

## 7. Death Stack System

Mọi cái chết đều đi qua Death Stack — đảm bảo OnDeath / OnAllyDeath / Reborn xử lý đúng thứ tự và đầy đủ.

### Quy trình

```
1. ScanAllBoardsForNewDeaths()
   → Mọi unit IsDead && !onDeathProcessed → RegisterDeath → push vào stack

2. FlushDeathStack():
   WHILE stack không rỗng HOẶC có deaths mới:
     WHILE stack.Count > 0:
       Pop DeathEvent
       → Fire OnDeath(victim)
       → ScanAllBoardsForNewDeaths()
       → Broadcast OnAllyDeath cho từng đồng minh còn sống của victim
       → ScanAllBoardsForNewDeaths()
     END WHILE
     
     CleanupBoard(player), CleanupBoard(enemy):
       → Unit IsDead + onDeathProcessed:
           Nếu isReborn && !hasRebornUsed → Revive(1 HP), fire OnAllyReborn
           Ngược lại                      → null slot
       → Unit IsDead + !onDeathProcessed: bỏ qua (chờ vòng sau xử lý)
     
     ScanAllBoardsForNewDeaths()   ← bắt deaths từ OnAllyReborn
     hasMore = (stack.Count > 0)
```

### Ưu tiên triệu hồi (SummonUnit)

1. Slot trống hoàn toàn (`null`).
2. Slot chứa unit đã chết + đã qua death stack (`onDeathProcessed = true`) + **không có Reborn đang chờ**.

---

## 8. Tribe Synergy

Áp dụng 1 lần duy nhất vào **đầu Combat Phase**, trước `StartOfBattle`.

| Tribe | Điều kiện | Bonus |
|-------|-----------|-------|
| **Babylon** | ≥ 2 unit Babylon cùng đội | Mỗi unit Babylon: +1 HP (currentHP và maxHP) |
| **Olympus** | ≥ 2 unit Olympus cùng đội | Mỗi unit Olympus: +1 ATK |
| **Niles** | ≥ 3 unit Niles cùng đội | Mỗi unit Niles: +2 HP (currentHP và maxHP) |

- Tribe synergy **không** giữ lại sau trận (chỉ `currentATK`/`currentHP` trong combat; ResetStats đầu lượt sau không bao gồm tribe bonus).

---

## 9. Keywords

### Taunt
- Unit địch **bắt buộc** tấn công unit Taunt trước (không thể chọn unit khác).
- Nếu có nhiều unit Taunt: tấn công unit Taunt **gần slot nhất**.
- Cấp bởi: `hasTaunt` trong data, hoặc `GiveBuff(isTaunt)`, hoặc Magic `AddTaunt`.

### Reborn
- Khi chết lần đầu: **hồi sinh với 1 HP** tại cùng slot.
- Chỉ kích hoạt **1 lần** (kiểm tra bằng `hasRebornUsed`).
- Sau Reborn: `onDeathProcessed` reset → có thể chết lại và fire OnDeath lần 2.
- Cấp bởi: `hasReborn` trong data, hoặc `GiveBuff(isReborn)`.

### Safeguard
- **Chặn hoàn toàn** 1 đòn tấn công hoặc 1 hiệu ứng `DealDamage` (nhận 0 damage).
- Tiêu hao ngay sau khi chặn (không dùng lại được trong trận).
- Cấp bởi: `hasSafeguard` trong data, hoặc `GiveBuff(isSafeguard)`.

---

## 10. Hệ thống Ability — TTE Engine

Mỗi ability gồm 3 phần: **Trigger → Target → Effect**.

### 10.1 Trigger (khi nào kích hoạt)

| TriggerType | Mô tả |
|-------------|-------|
| `StartOfBattle` | Đầu mỗi trận, trước vòng lặp tấn công |
| `Aura` | Đầu mỗi trận, ngay sau StartOfBattle |
| `OnAttack` | Khi unit này tấn công (và còn sống sau phản đòn) |
| `OnTakeDamage` | Khi unit này nhận damage và còn sống sau đó |
| `OnDeath` | Khi unit này chết (qua Death Stack) |
| `OnDeploy` | Khi unit được đặt lên sân (mua / kéo từ tay lên) |
| `OnSell` | Khi unit bị bán |
| `EndTurnShop` | Đầu lượt mới, sau khi nhận coin (unit phải ở trên sân) |
| `OnAllyDeath` | Khi một đồng minh chết |
| `OnAllySummon` | Khi một đồng minh được triệu hồi |
| `OnAllyReborn` | Khi một đồng minh hồi sinh qua Reborn |

### 10.2 Target (nhắm vào ai)

| TargetType | Mô tả |
|-----------|-------|
| `Self` | Bản thân |
| `DirectEnemy` | Kẻ địch trực tiếp gây ra trigger |
| `AllAllies` | Toàn bộ đồng minh còn sống (lọc thêm bằng `targetTribe` nếu ≠ 0) |
| `AllAlliesExceptSelf` | Toàn bộ đồng minh còn sống trừ bản thân |
| `RandomAlly` | `targetCount` đồng minh ngẫu nhiên (không trùng) |
| `RandomEnemy` | `targetCount` kẻ địch ngẫu nhiên |
| `LowestHealthAlly` | Đồng minh có HP thấp nhất |
| `LeftAlly` | Đồng minh ở slot trái (slotIndex - 1) |
| `RightAlly` | Đồng minh ở slot phải (slotIndex + 1) |
| `AllNilesAllies` | Tất cả đồng minh thuộc tộc Niles |
| `AllBabylonAllies` | Tất cả đồng minh thuộc tộc Babylon |
| `TriggerSubject` | Unit gây ra sự kiện (dùng trong OnAllyDeath / OnAllySummon / OnAllyReborn) |

### 10.3 Effect (làm gì)

| EffectType | Mô tả |
|-----------|-------|
| `AddStats` | +`effectValue1` ATK / +`effectValue2` HP cho target |
| `DealDamage` | Gây `effectValue1` sát thương (Safeguard chặn được) |
| `GiveBuff` | Cấp keyword: `isTaunt` / `isReborn` / `isSafeguard` |
| `Summon` | Triệu hồi unit theo `summonCardID` vào slot trống |
| `Destroy` | Set HP = 0 ngay lập tức (vẫn trigger OnDeath bình thường) |
| `GainCoin` | +`effectValue1` coin cho người chơi ngay lập tức |
| `Reborn` | Hồi sinh target với `effectValue1` HP (nếu chưa dùng Reborn) |
| `TriggerAbility` | Kích hoạt lại OnDeath của target `scaleFactor` lần |
| `SummonConsumed` | Triệu hồi lại tất cả unit đã bị Consume bởi source |

### 10.4 Tham số điều kiện

| Tham số | Mô tả |
|---------|-------|
| `isPermanent` | Buff `AddStats` giữ lại qua trận (vào `permanentBonus`, áp dụng lại sau `ResetStats`) |
| `triggerLimit` | Tổng số lần tối đa ability được kích hoạt (0 = không giới hạn) |
| `conditionCount` | Chỉ fire vào lần trigger thứ N, 2N, 3N... (0 = mỗi lần) |
| `isConsume` | Dùng kèm `Destroy`: lưu `cardID` của target vào `consumedCardIDs` |
| `targetTribe` | Lọc target theo tộc (0 = tất cả; 1 = Babylon; 2 = Olympus; 3 = Niles) |

### 10.5 Scale theo Merge Level

```
scaleFactor = mergeLevel + 1
effectValue1 × scaleFactor
effectValue2 × scaleFactor
```

Tất cả damage, heal, buff đều nhân với `scaleFactor` — unit đã merge càng nhiều, kỹ năng càng mạnh.

### 10.6 Growth (StartOfBattle + AddStats)

Khi trigger = `StartOfBattle` và effect = `AddStats`:
- Được coi là **Growth** (tăng trưởng).
- Tích lũy vào `growthATKBonus` / `growthHPBonus` (giữ qua tất cả lượt).
- Cộng thẳng vào `currentATK` / `currentHP` ngay trong trận.
- Không bị reset khi kết thúc trận — `ResetStats()` tính lại từ `baseStats × tier + 0.7 × (growth + permanent)`.

---

## 11. Khôi phục sân sau Combat

1. Xoá toàn bộ UI sân player.
2. Khôi phục từ **snapshot** chụp trước combat.
3. Mỗi unit trong snapshot: `ResetStats()` → HP/ATK về full theo công thức tier, giữ nguyên `permanentBonus` + `growthBonus`.
4. Tái tạo CardUI cho từng unit.
5. Xoá sân địch.
6. Sync board list từ UI.
7. Reset visuals các card trong Hand.

Unit được **triệu hồi trong combat** (không có trong snapshot) → **không tồn tại** qua lượt.

---

## 12. AI (Bot Agent)

- Bot có **bộ não** (`Chromosome`) gồm các gene đánh giá:
  - gene[0]: trọng số ATK
  - gene[1]: trọng số HP
  - gene[2]: trọng số OnDeath AddStats
  - gene[3]: trọng số Taunt / OnTakeDamage
  - gene[4]: trọng số StartOfBattle AddStats
  - gene[5]: trọng số Reborn

- Mỗi lượt combat, bot nhận shop ngẫu nhiên (5 lá, cùng tier với player) rồi chạy `DecidePrepPhase`:
  1. Đánh giá điểm từng lá theo gene.
  2. Mua lá điểm cao nhất nếu đủ tiền (`card.cost`).
  3. Đặt lên sân tại slot trống đầu tiên.
  4. Lặp đến hết tiền hoặc hết chỗ.

- Bot có **3 mức độ khó** (Easy / Medium / Hard) tương ứng với các bộ gene khác nhau trong `AIManager`.

---

## 13. Sân và Slots

| Loại Slot | Số lượng | Mô tả |
|-----------|----------|-------|
| PlayerBoard | 6 | Sân chiến đấu của player |
| EnemyBoard | 6 | Sân của Bot (chỉ đọc, không tương tác) |
| Hand | N | Cất giữ lá chưa deploy (số lượng theo Scene) |
| Shop | 5 | Hiển thị lá bài đang bán |

- Unit ở Hand **không tham chiến** và **không nhận EndTurnShop**.
- Unit ở Hand **được tính trong Merge** (kể cả combine với unit trên sân).

---

## 14. Hiển thị UI

| Trạng thái | Resource hiển thị | Nút hành động |
|-----------|-------------------|--------------|
| Shop Phase | Coin (số + "G") | **BẮT ĐẦU** |
| Combat Phase | Cup | **LƯỢT TIẾP** |

- HP người chơi luôn hiển thị ở góc trên.
- Nút **Roll** và **Lock** chỉ hiện trong Shop Phase.
- Nhấn **LƯỢT TIẾP** trong Combat Phase chỉ có tác dụng SAU KHI combat animation kết thúc (nếu nhấn giữa chừng sẽ gọi `ExecuteNextTurn()` lập tức).
