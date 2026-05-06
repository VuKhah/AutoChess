# AutoChess — Tổng quan Dự án

> Cập nhật: 2026-05-07 | Branch: Khanh-dev

---

## 1. Tổng quan

Trò chơi thẻ bài tự động theo lượt (**Auto-Battler**), mỗi lượt gồm hai pha:

- **Shop Phase** — Người chơi mua/bán/sắp xếp thẻ, dùng phép
- **Battle Phase** — Chiến đấu tự động, hoàn toàn tất định (Deterministic)

Người chơi đấu với **Bot** được huấn luyện bằng **Genetic Algorithm** (GA).

---

## 2. Cấu trúc thư mục

```
AutoChess/
├── Assets/
│   ├── Scripts/
│   │   ├── AI/           BotAgent, Chromosome, GATrainer, GameSimulator, AILibrary
│   │   ├── Core/         CombatResolver, CardInstance, GameRecord
│   │   ├── Data/         AbilityData, CardDefinition
│   │   ├── Managers/     GameManager, EconomyManager, CardDatabase, AIManager, UIManager
│   │   ├── UI/           CardDraggable, CardSlot, CardUI, CardVisuals
│   │   └── Test/         ProjectSanityChecker, scan_assets.py
│   └── Resources/
│       ├── CardsData.json       (60 thẻ bài)
│       ├── AI_Library.json      (bộ não GA đã train)
│       └── Document/TienDo.md  (nhật ký tiến độ)
└── ProjectSettings/
```

---

## 3. Luật game

### 3.1 Cấu hình cơ bản

| Tham số | Giá trị | File |
|---|---|---|
| HP ban đầu | 7 | `GameManager.cs` |
| Mục tiêu thắng | 10 Cup | `GameManager.cs` |
| Số lượt tối đa | 20 | `GameManager.cs` |
| Coin mỗi lượt | 10 (reset cố định) | `EconomyManager.cs` |
| Chi phí Roll | 1 Coin | `GameManager.cs` |
| Chi phí mua thẻ | 3 Coin | `CardDefinition.cs` |
| Bán thẻ | +1 Coin | `CardSlot.cs` |
| Ô board | 6 ô | `GameManager.cs` |
| Ô tay | 6 ô | `GameManager.cs` |

### 3.2 Tier System

Tier shop tăng theo lượt: `tier = clamp((turn + 1) / 2, 1, 6)`

| Lượt | Tier mở khóa |
|---|---|
| 1–2 | Tier 1 |
| 3–4 | Tier 2 |
| 5–6 | Tier 3 |
| 7–8 | Tier 4 |
| 9–10 | Tier 5 |
| 11+ | Tier 6 |

Shop chỉ hiển thị thẻ có `tier <= currentTier`.

### 3.3 Merge System

- 3 thẻ cùng `cardID` và cùng `mergeLevel` → hợp nhất thành 1 thẻ cấp cao hơn
- Nhân chỉ số: `Base × cấp_độ + keepRatio × sum(sum(growth), sum(permanent))`
- Tối đa: Merge cấp 2 (từ cấp 0 lên cấp 2)

### 3.4 Economy

- Coin hồi đầu mỗi lượt: 10 (cố định, không carry-over)
- Bonus Coin: cộng thêm từ `bonusCoinNextTurn` (magic hoặc ability)
- Unit có ability `GainCoin` (Trigger=StartOfBattle) đào thêm Coin đầu trận

---

## 4. TTE Engine — Hệ thống kỹ năng

Kiến trúc **Data-Driven**: mọi kỹ năng định nghĩa trong JSON, không hardcode.

Mỗi ability gồm 3 thành phần: **Trigger → Target → Effect**

### TriggerType (`AbilityData.cs`)

| ID | Tên | Mô tả |
|---|---|---|
| 0 | None | Không có kỹ năng |
| 1 | OnTakeDamage | Khi bị đánh |
| 2 | OnAttack | Khi tấn công |
| 3 | OnDeath | Khi chết |
| 4 | StartOfBattle | Đầu trận |
| 5 | EndTurnShop | Cuối pha Shop |
| 6 | OnDeploy | Khi đặt xuống board |
| 7 | OnSell | Khi bán |
| 8 | OnAllyDeath | Khi đồng minh chết |
| 9 | OnAllySummon | Khi đồng minh được triệu hồi |
| 10 | OnAllyReborn | Khi đồng minh hồi sinh |
| 11 | Aura | Hiệu ứng liên tục |

### TargetType (`AbilityData.cs`)

| ID | Tên | Mô tả |
|---|---|---|
| 0 | None | Không có mục tiêu |
| 1 | Self | Bản thân |
| 2 | RandomAlly | Đồng minh ngẫu nhiên |
| 3 | AllAllies | Toàn bộ đồng minh |
| 4 | RandomEnemy | Kẻ địch ngẫu nhiên |
| 5 | DirectEnemy | Kẻ địch đối diện |
| 6 | LowestHealthAlly | Đồng minh HP thấp nhất |
| 7 | LeftAlly | Đồng minh bên trái |
| 8 | RightAlly | Đồng minh bên phải |
| 9 | AllNilesAllies | Toàn bộ đồng minh Niles |
| 10 | AllBabylonAllies | Toàn bộ đồng minh Babylon |

### EffectType (`AbilityData.cs`)

| ID | Tên | Mô tả |
|---|---|---|
| 0 | None | Không có hiệu ứng |
| 1 | AddStats | Tăng ATK/HP |
| 2 | DealDamage | Gây sát thương |
| 3 | GiveBuff | Ban hiệu ứng (Taunt, Reborn) |
| 4 | Summon | Triệu hồi unit |
| 5 | Destroy | Hủy/Banish |
| 6 | GainCoin | Thêm Coin |
| 7 | Reborn | Hồi sinh |
| 8 | TriggerAbility | Kích hoạt kỹ năng khác |

### Ví dụ combo

| Tên | Trigger | Effect | Mô tả |
|---|---|---|---|
| Growth | StartOfBattle | AddStats | Tăng chỉ số vĩnh viễn qua các lượt |
| Thorns | OnTakeDamage | DealDamage | Phản sát thương khi bị đánh |
| Slain Effect | OnDeath | AddStats | Buff đồng minh khi chết |
| Economy | StartOfBattle | GainCoin | Đào Coin đầu trận |

---

## 5. Hệ thống chiến đấu (`CombatResolver.cs`)

### Luồng xử lý `ResolveTurn()`

```
1. ApplyTribeSynergies()        → Babylon ≥2 unit: tất cả +1 HP tạm thời
2. TriggerAbility(StartOfBattle) → Growth, Economy
3. Combat Loop (max 50 vòng):
   ├─ Player side: slot 0→5 tấn công
   ├─ Enemy side: slot 0→5 tấn công
   ├─ ExecuteClash()             → Sát thương đồng thời, trigger OnTakeDamage/OnDeath
   └─ CleanupBoard()             → Xóa unit chết
4. RecordSnapshots()             → Lưu trạng thái cho AI học
```

### FindTarget() — Ưu tiên chọn mục tiêu

1. Unit có `isTaunt = true` → **Bắt buộc tấn công vào**
2. Ripple Search từ slot đối diện (khoảng 0 → 1 → 2 …)

### ExecuteClash() — Cơ chế sát thương

- **Simultaneous damage**: Cả hai nhận sát thương cùng lúc
- Trigger `OnTakeDamage` ngay sau nhận damage
- Trigger `OnDeath` ngay khi HP ≤ 0
- Ghi `CombatAction` log để UI chạy animation

### Synergy

| Tribe | Điều kiện | Buff |
|---|---|---|
| Babylon (1) | ≥ 2 unit | +1 HP tạm thời (chỉ combat này) |
| Niles (2) | — | Chưa implement |

---

## 6. Hệ thống Stat & Buff (`CardInstance.cs`)

```
currentATK = baseATK × (mergeLevel+1) + permanentATKBonus + growthATKBonus
currentHP  = baseHP  × (mergeLevel+1) + permanentHPBonus  + growthHPBonus
```

| Loại buff | Nguồn | Tồn tại |
|---|---|---|
| Permanent | Magic StatBoost | Không reset |
| Growth | Trigger=StartOfBattle + AddStats | Tích lũy qua các lượt |
| Temporary | Buff trong combat | Reset sau trận |

**Reborn**: kích hoạt 1 lần, flag `hasRebornUsed` ngăn tái kích hoạt.

---

## 7. Dữ liệu thẻ bài (`CardsData.json`)

**60 thẻ tổng**: 22 Babylon + 24 Niles + 3 Magic

| Tộc | Số thẻ | Tier | Đặc trưng |
|---|---|---|---|
| Babylon (1) | 22 | 1–6 | Tăng stats, Reborn, Taunt |
| Niles (2) | 24 | 1–6 | OnDeath buff, Summon, Triệu hồi |
| Magic | 3 | — | StatBoost, AddTaunt, Economy |

**Magic spells:**

| ID | Tên | Hiệu ứng |
|---|---|---|
| M_01 | Sức mạnh Babylon | permanentATKBonus + permanentHPBonus cho một unit |
| M_02 | Giao ước Olympus | Set `isTaunt = true` cho một unit |
| M_03 | Quà tặng Thương Nhân | `bonusCoinNextTurn += 1` |

---

## 8. Hệ thống Shop & UI

### Shop Phase flow

```
RefreshShop()
→ GetRandomShop(5 thẻ, tier hiện tại)
→ Render CardUI trong shopSlots

Player actions:
- Roll        : -2 Coin, refresh shop
- Lock        : Giữ shop không đổi lượt sau
- Drag shop → hand/board : Mua (-3 Coin)
- Drag hand/board → shop : Bán (+1 Coin)
- 3× cùng loại           : Tự động Merge
- Magic → Unit           : Kích hoạt effect magic
```

### Magic apply logic (`CardSlot.cs:ApplyMagicEffect`)

| magicGroup | Hành động |
|---|---|
| StatBoost | +permanentATKBonus, +permanentHPBonus |
| AddAbility | Thay ability của unit |
| AddTaunt | `unit.isTaunt = true` |
| Economy | `bonusCoinNextTurn += 1` |

---

## 9. AI — Genetic Algorithm

### BotAgent (`BotAgent.cs`)

Mỗi bot có `Chromosome` (8 gene float[0,1]) làm hàm đánh giá thẻ.

```
score = baseATK × genes[0] + baseHP × genes[1]
if Taunt/Thorns  → score += 8  × genes[3]
if Growth        → score += 12 × genes[4]
if Reborn        → score += 10 × genes[5]
if SlainEffect   → score += 10 × genes[2]
```

Bot mua thẻ có `score` cao nhất cho đến hết Coin.

### GA Training (`GATrainer.cs`)

```
Population: 50 cá thể
Generations: 100
Fitness: 10 trận/cá thể → Thắng +10, Hòa +2, Thua +0
Selection: Elitism 10% (giữ 5 tốt nhất)
Crossover: Uniform (50% từ cha, 50% từ mẹ)
Mutation: 5% cơ hội, delta ±0.1, clamp [0,1]

Snapshot:
- Gen 10  → easyBot
- Gen 50  → mediumBot
- Gen 100 → hardBot
```

### Headless Simulator (`GameSimulator.cs`)

Chạy trận đấu không render UI. 1 trận ≈ 0.05 giây.  
100 thế hệ training ≈ 42 phút.

---

## 10. Sự kiện & vị trí xử lý

| Sự kiện | File xử lý | Ghi chú |
|---|---|---|
| `StartOfBattle` | `CombatResolver.cs:ApplyStartOfBattleTriggers` | Growth, GainCoin |
| `OnTakeDamage` | `CombatResolver.cs:ExecuteClash` | Thorns, counter-effects |
| `OnDeath` | `CombatResolver.cs:ExecuteClash` | Slain buff, Reborn check |
| `OnAllyDeath` | `CombatResolver.cs:CleanupBoard` | Buff đồng minh khi đồng đội chết |
| `OnAllySummon` | `CombatResolver.cs` | Trigger khi summon unit mới |
| `OnAllyReborn` | `CombatResolver.cs` | Trigger khi đồng minh hồi sinh |
| `OnAttack` | `CombatResolver.cs` | Chưa có unit dùng trigger này |
| `Aura` | Chưa implement | Defined trong enum, chưa xử lý |
| `EndTurnShop` | Chưa implement | Defined trong enum, chưa xử lý |
| `OnDeploy` | `CardSlot.cs` | Khi kéo thả unit xuống board |
| `OnSell` | `CardSlot.cs` | Ví dụ: Ereskigal buff khi bán |
| Merge | `CardSlot.cs:TryMerge` | 3 thẻ cùng loại tự merge |
| ApplyMagic | `CardSlot.cs:ApplyMagicEffect` | Magic spell lên unit |
| Roll Shop | `GameManager.cs:RollShop` | -2 Coin, refresh |
| Buy Card | `GameManager.cs:BuyCard` | -3 Coin, thêm vào hand |
| End Turn | `GameManager.cs:EndTurn` | Tăng turn, tính tier, reset coin |
| AI Decide | `BotAgent.cs:DecidePrepPhase` | Bot mua/sắp xếp trong headless sim |
| GA Evolve | `GATrainer.cs:RunEvolution` | Chạy 100 thế hệ |

---

## 11. Tiến độ

### Đã hoàn thiện ✅

- Core game loop (Shop → Battle → Score)
- TTE Engine đầy đủ (11 trigger, 10 target, 8 effect)
- Merge system (3 thẻ → cấp cao hơn)
- Deterministic combat với CombatAction log
- Synergy Babylon
- Reborn & Slain Effect
- 60 thẻ bài có đầy đủ ability JSON
- Magic system (3 loại spell)
- UI kéo thả (Shop, Hand, Board)
- Bot Agent với Chromosome evaluation
- GA Training (50 pop, 100 gen, elitism + mutation)
- Headless Simulator

### Chưa hoàn thiện ❌

| Hạng mục | Mức độ ưu tiên | Ghi chú |
|---|---|---|
| Niles Synergy | Cao | Tribe 2 chưa có buff tộc |
| LowestHealthAlly / LeftAlly / RightAlly target | Cao | Defined nhưng chưa có logic |
| Aura trigger | Trung bình | Chưa implement loop liên tục |
| EndTurnShop trigger | Trung bình | Chưa có unit dùng |
| Olympus Tribe (Tribe 3) | Thấp | Chưa có data |
| UI polish / animation | Thấp | Chức năng đủ dùng |
| Sound/Music | Thấp | Chưa có |
| Save/Load game | Thấp | Chưa implement |

---

## 12. Vấn đề trước mắt

| Vấn đề | Mô tả | File liên quan |
|---|---|---|
| Magic logic sai vị trí | `ApplyMagicEffect` nằm trong `CardSlot.cs` (UI layer) thay vì Engine | `CardSlot.cs`, `CombatResolver.cs` |
| Niles thiếu synergy | 24 unit Niles nhưng không có buff tộc như Babylon | `CombatResolver.cs:ApplyTribeSynergies` |
| Target types chưa implement | LowestHealthAlly, LeftAlly, RightAlly được định nghĩa nhưng `FindTarget()` chưa xử lý | `CombatResolver.cs:FindTarget` |
| AI chỉ mua, không sắp xếp vị trí | Bot mua thẻ tốt nhưng không tối ưu thứ tự slot (Taunt nên đứng trước) | `BotAgent.cs:DecidePrepPhase` |
| Fitness function đơn giản | Chỉ tính Win/Draw/Loss, không tính HP còn lại → bot chưa học được giá trị của survival | `GATrainer.cs:EvaluateFitness` |

---

## 13. Kiến trúc nổi bật

- **Data-driven**: Mọi ability định nghĩa bằng JSON, không hardcode trong C#
- **Deterministic combat**: Cùng board → cùng kết quả, đảm bảo AI học được nhân quả
- **Headless simulation**: Train AI không cần render, tốc độ cao
- **Separation of concerns**: AI / Core / Data / Managers / UI tách biệt rõ ràng
- **CombatAction log**: Ghi đầy đủ mọi hành động để UI replay và AI học

---

*File này tổng hợp từ phân tích codebase tại commit `a4aee9a` (branch Khanh-dev, 2026-05-07)*
