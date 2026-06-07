# Báo Cáo Hệ Thống Gen & AI — AutoChess

> Cập nhật: 2026-05-28

---

## 1. Tổng Quan Kiến Trúc

Hệ thống AI của dự án gồm 4 thành phần chính, hoạt động độc lập với Unity MonoBehaviour để có thể chạy trong quá trình huấn luyện mà không cần scene:

```
GATrainer (MonoBehaviour)
    │
    ├── Tạo & tiến hóa population (List<Chromosome>)
    │
    ├── GameSimulator
    │       └── Gọi BotAgent × 2 để chạy trận mô phỏng
    │               └── CombatResolver (tính kết quả chiến đấu)
    │
    └── AILibrary (JSON)
            ├── easyBot   (snapshot Gen 25%)
            ├── mediumBot (snapshot Gen 50%)
            └── hardBot   (snapshot Gen cuối)
```

---

## 2. Chromosome — Bộ Gen 32 Gene

Mỗi `Chromosome` là một mảng `float[32]`, mỗi giá trị trong `[0, 1]`.  
Bot **không có logic if-else cứng** — toàn bộ hành vi được điều khiển bởi 32 con số này.

### Nhóm 1 — Chỉ Số Gốc (gene[0..3])

| Index | Tên           | Ý nghĩa |
|-------|---------------|---------|
| [0]   | wATK          | Trọng số ATK khi đánh giá unit |
| [1]   | wHP           | Trọng số HP khi đánh giá unit |
| [2]   | wTierBonus    | Bonus thêm mỗi cấp Tier vượt 1: `score += (tier-1) × gene × 5` |
| [3]   | wCostEff      | Hiệu quả chi phí: `score = score / cost × (1 + gene)` |

### Nhóm 2 — Passive Keywords (gene[4..6])

| Index | Tên        | Ý nghĩa |
|-------|------------|---------|
| [4]   | wTaunt     | Giá trị cộng thêm khi unit có Taunt |
| [5]   | wReborn    | Giá trị cộng thêm khi unit có Reborn |
| [6]   | wSafeguard | Giá trị cộng thêm khi unit có Safeguard |

### Nhóm 3 — Trigger Weights (gene[7..12])

Dùng trong `TriggerWeight()` để nhân vào điểm ability khi đánh giá unit.

| Index | Tên           | TriggerType tương ứng |
|-------|---------------|-----------------------|
| [7]   | tStartBattle  | StartOfBattle (buff trước trận) |
| [8]   | tOnDeath      | OnDeath (deathrattle) |
| [9]   | tOnAttack     | OnAttack |
| [10]  | tOnTakeDmg    | OnTakeDamage |
| [11]  | tEndTurnShop  | EndTurnShop (growth tích lũy) |
| [12]  | tOnDeploy     | OnDeploy / OnAllyDeploy / OnAllySummon / OnAllyReborn / OnAllySell |

> **Lưu ý:** `OnAllyDeath`, `OnAllySummon`, `OnAllyReborn` dùng `gene[12] × 0.8f`.  
> `OnSell` dùng `gene[8] × 0.5f`. `OnStatGain` dùng `gene[13] × 0.4f`.

### Nhóm 4 — Effect Weights (gene[13..17])

Dùng trong `EffectWeight()` — nhân với `TriggerWeight` để ra điểm tổng của ability.

| Index | Tên           | EffectType tương ứng |
|-------|---------------|----------------------|
| [13]  | eAddStats     | AddStats, TriggerAbility×0.5, AbsorbStats×1.5, GiveStats×1.2, ScaleTargetStats×1.3 |
| [14]  | eSummon       | Summon, SummonConsumed×1.2, Destroy(consume)×0.7, GiveCard×0.8 |
| [15]  | eDealDmg      | DealDamage, Destroy×0.8 |
| [16]  | eGainCoin     | GainCoin |
| [17]  | eGiveBuff     | GiveBuff, Reborn×1.1 — cũng kiểm soát ưu tiên đặt unit support về backline |

### Nhóm 5 — Tribe Synergy (gene[18..20])

| Index | Tên       | Tribe |
|-------|-----------|-------|
| [18]  | sBabylon  | Babylon — `score += sameTribeCount × gene × 4` |
| [19]  | sOlympus  | Olympus |
| [20]  | sNiles    | Niles |

### Nhóm 6 — Board Context (gene[21..23])

| Index | Tên              | Ý nghĩa |
|-------|------------------|---------|
| [21]  | wMerge           | Bonus khi đã có bản sao cùng cardID trên sân (xúc tiến merge) |
| [22]  | wFrontline       | Bonus Taunt khi frontline còn trống (ưu tiên slot hàng đầu) |
| [23]  | wSaveThreshold   | Ngưỡng mua tối thiểu: `score < gene × 3` → bỏ qua card |

### Nhóm 7 — Reroll Behavior (gene[24..27])

| Index | Tên            | Ý nghĩa |
|-------|----------------|---------|
| [24]  | wRerollThresh  | Reroll nếu `bestShopScore < gene × bestBoardScore`. `< 0.05` → không bao giờ reroll |
| [25]  | wRerollMax     | Số lần reroll tối đa = `floor(gene × 3) + 1` → **[1..4] lần/lượt** |
| [26]  | wRerollKeep    | Giữ lại `floor(gene × 4)` coin trước khi reroll → **[0..4] coin dự phòng** |
| [27]  | wProactiveSell | Bán unit khi `score < gene × 3`, dù board chưa đầy |

### Nhóm 8 — Spell Behavior (gene[28..31])

| Index | Tên             | Ý nghĩa |
|-------|-----------------|---------|
| [28]  | wSpellThresh    | Ngưỡng mua spell: `score < gene × 3` → bỏ qua |
| [29]  | wSpellOnStrong  | Ưu tiên cast spell lên unit có `EvaluateInstance` cao nhất |
| [30]  | wSpellOnMerged  | Ưu tiên cast spell lên unit có `mergeLevel` cao hơn |
| [31]  | wSpellEconomy   | Nhân thêm vào điểm spell kinh tế (GainCoin, GainIncome, GiveEndTurnBuff) |

---

## 3. BotAgent — Đại Lý Ra Quyết Định

`BotAgent` là "người chơi ảo" — nhận shop mỗi lượt và quyết định toàn bộ hành vi chuẩn bị.  
**Không dùng MonoBehaviour** → chạy được trong simulation.

### 3.1 Cấu Trúc Dữ Liệu

```
BotAgent
├── Chromosome brain         — 32 gene điều khiển hành vi
├── EconomyManager economy   — quản lý coin
├── List<CardInstance> board — sân đấu (BoardSlotCount slots, thường 8)
├── int _shopTier            — tier hiện tại của shop
├── List<CardDefinition> _currentUnitShop
├── bool isShopFrozen
├── List<CardDefinition> frozenUnitShop / frozenSpellShop
└── int startingCoins        — coin mỗi lượt (mặc định 10)
```

> **Không có Hand**: Bot không mô phỏng hand slots. Unit và spell được xử lý trực tiếp lên board.

### 3.2 Vòng Quyết Định — DecidePrepPhase()

Mỗi lượt, bot chạy **7 phase theo thứ tự**:

```
DecidePrepPhase(unitShop, spellShop, shopTier)
 │
 ├── [Pre] Áp frozen shop nếu đã freeze lượt trước
 ├── [Pre] ResetEconomy() + trừ coin thiếu nếu startingCoins < 10
 │
 ├── Phase 1: RerollPhase      — reroll nếu shop kém hơn board hiện tại
 ├── Phase 2: BuyUnitsPhase    — mua unit tốt nhất còn mua được
 ├── Phase 3: BuySpellsPhase   — mua & cast spell ngay lên board
 ├── Phase 4: ProactiveSellPhase — bán unit điểm thấp để dọn board
 ├── Phase 5: TryMerge         — merge 3 bản sao cùng cardID+mergeLevel
 ├── Phase 6: RepositionPhase  — sắp xếp lại vị trí tối ưu
 └── Phase 7: FreezePhase      — đóng băng shop nếu có unit muốn chưa đủ tiền
```

### 3.3 Chi Tiết Từng Phase

#### Phase 1 — Reroll

- Không chạy nếu `gene[24] < 0.05`
- Tối đa `floor(gene[25] × 3) + 1` lần reroll
- Điều kiện reroll: `bestShopScore < gene[24] × bestBoardScore`
- Giữ tối thiểu `floor(gene[26] × 4)` coin trước khi reroll

#### Phase 2 — Buy Units

Lặp lại cho đến khi không mua được nữa:
1. Tìm card có điểm cao nhất trong shop còn đủ tiền
2. Nếu điểm < `gene[23] × 3` → dừng
3. Nếu board đầy: so sánh với unit yếu nhất — bán nếu unit mới > `worstScore × (1.5 + gene[23])`
4. Mua và đặt lên board → fire `OnDeploy` + `OnAllyDeploy`

#### Phase 3 — Buy Spells

- Sắp xếp spell theo `EvaluateSpell()` giảm dần
- Mua nếu đủ tiền và điểm ≥ `gene[28] × 3`
- Cast ngay lên board (`ApplySpellToBoard`) không lưu vào hand

#### Phase 4 — Proactive Sell

- Không chạy nếu `gene[27] < 0.05`
- Bán mọi unit có `EvaluateInstance < gene[27] × 3`

#### Phase 5 — TryMerge

- Quét board tìm 3+ bản sao cùng `cardID` + `mergeLevel`
- Giữ lại keeper có tổng bonus (`permanentATK + permanentHP + growthATK + growthHP`) cao nhất
- Sau mỗi merge: gọi `SimulateMergeReward()` — đặt ngay 1 unit ngẫu nhiên `_shopTier + 1` lên board nếu còn slot
- Lặp lại cho đến khi không còn merge nào

#### Phase 6 — Reposition

Bot chia board thành:
- **Frontline** (slot 0..3): unit tấn công trước
- **Backline** (slot 4..7): unit hỗ trợ

Sắp xếp theo `PositionScore()`:
- Taunt: điểm cao nhất → frontline đầu tiên
- Support triggers (Aura, OnAllyDeath, OnAllyReborn, OnAllySummon, EndTurnShop): `score -= gene[17] × 15` → đẩy về backline
- Deathrattle (OnDeath): `score += gene[8] × 5` → có xu hướng lên frontline

#### Phase 7 — Freeze

Đóng băng shop nếu:
- `1 - gene[24] ≥ 0.35` (bot không thích reroll nhiều)
- Có unit trong shop tốt nhưng chưa đủ tiền (`score ≥ gene[23] × 3` mà `cost > currentCoin`)

### 3.4 Hàm Đánh Giá

#### Evaluate(CardDefinition) — Điểm Mua Card

```
score = baseATK × gene[0]
      + baseHP  × gene[1]
      + (tier-1) × gene[2] × 5

+ passive keywords (Taunt/Reborn/Safeguard)
+ Σ abilities: TriggerWeight(trigger) × EffectWeight(effect) × 10
+ synergy bonus: sameTribeOnBoard × SynergyWeight(tribe) × 4
+ merge proximity: copies × gene[21] × (copies==2 ? 16 : 8)
+ frontline bonus nếu Taunt và slot frontline trống

÷ cost × (1 + gene[3])
```

#### EvaluateInstance(CardInstance) — Điểm Unit Đang Có

Phiên bản đơn giản hơn, dùng `currentATK`, `currentHP`, `mergeLevel`:

```
score = currentATK × gene[0]
      + currentHP  × gene[1]
      + mergeLevel × gene[2] × 5
      + passive keywords
      + synergy bonus
```

#### EvaluateSpell(CardDefinition) — Điểm Spell

| Effect | Công thức điểm |
|--------|---------------|
| BuffStats (1) | `(atk × gene[0] + hp × gene[1]) × (permanent ? 2.5 : 1)` |
| GainCoin (6) | `value × gene[16] × gene[31]` |
| GetRandomUnit (10) | `gene[2] × 6` |
| StealFromShop (11) | `gene[2] × 7` |
| GetSameRealmUnit (13) | `gene[2] × 5 + gene[7] × 4` |
| GainIncome (12) | `value × gene[16] × gene[31] × (permanent ? 12 : 1.5)` |
| UpgradeTierUnit (17) | `gene[2] × 12` |
| GetUnitAtNextTier (22) | `gene[2] × (7 + shopTier × 0.4)` |
| GiveDoubleAtkSafeguard (18) | `gene[0] × 8 + gene[6] × 8` |
| ToggleTaunt (19) | `gene[4] × 6` |
| BuffByShopTier (20) | `shopTier × (gene[0] + gene[1]) × 0.6` |
| GiveEndTurnBuff (21) | `(atk×gene[0] + hp×gene[1]) × gene[11] × gene[31] × 3` |
| LoseLife (14) | `-25` (phạt nặng) |
| TransferStats (15) | `-8` (rủi ro) |

> Cuối cùng: `score ÷ cost × (1 + gene[3])` nếu `cost > 0`.

### 3.5 Simulation Ability — FireTrigger & ApplyBotEffect

Bot mô phỏng các trigger trong shop phase (không dùng `AbilityEngine`):

**Triggers được hỗ trợ**: `OnDeploy`, `OnAllyDeploy`, `OnSell`, `OnAllySell`, `EndTurnShop`

**Effects được hỗ trợ**:
- `AddStats` — cộng permanent bonus
- `GiveBuff` — Reborn / Taunt / Safeguard
- `GainCoin` — cộng coin cho economy

**Triggers KHÔNG mô phỏng trong shop phase** (chỉ có trong combat):  
`OnAttack`, `OnTakeDamage`, `OnDeath`, `StartOfBattle`, `OnAllyDeath`, `OnAllyReborn`

---

## 4. GameSimulator — Trận Đấu Mô Phỏng

`GameSimulator` dùng để đánh giá fitness: cho 2 `BotAgent` đấu nhau trong tối đa 20 lượt.

### Luồng Mô Phỏng

```
SimulateMatch(botA, botB)
 │
 ├── hpA = hpB = 7
 │
 └── for turn 1..20:
       ├── shopTier = clamp((turn+1)/2, 1, 6)
       ├── Tạo shop cho A và B (tôn trọng freeze nếu có)
       ├── botA.DecidePrepPhase(shopA, spellA, tier)
       ├── botB.DecidePrepPhase(shopB, spellB, tier)
       ├── resolver.ResolveTurn(boardA, boardB, log)   — chiến đấu thực sự
       ├── Kiểm tra sống còn → trừ HP bên thua
       ├── if hpA <= 0 || hpB <= 0: break
       ├── botA/B.EndCombatPhase()   — dọn unit chết, reset stats
       └── botA/B.TriggerEndTurnShop() — kích EndTurnShop growth
 │
 └── return +1 (A thắng), -1 (B thắng), 0 (hòa)
```

> **Ghi chú**: Combat được giải quyết bởi `CombatResolver` thực (cùng engine dùng cho player) — đảm bảo độ chính xác mô phỏng tuyệt đối, không phải ước tính.

---

## 5. GATrainer — Huấn Luyện Tiến Hóa

### Tham Số Mặc Định

| Tham số | Test nhanh | Production |
|---------|-----------|-----------|
| `populationSize` | 30 | 100 |
| `generations` | 40 | 150 |
| `matchesPerChrom` | 5 | 15 |
| `mutationRate` | 0.08 | 0.08 |
| `mutationMag` | 0.12 | 0.12 |

### Vòng Huấn Luyện

```
for generation g in [0, generations):
 │
 ├── ĐÁNH GIÁ FITNESS
 │     └── Mỗi chromosome đấu matchesPerChrom trận
 │           ├── Chọn đối thủ: uniform random (loại trừ chính mình)
 │           ├── Thắng: +10 điểm
 │           └── Hòa:   +2 điểm
 │
 ├── SẮP XẾP: giảm dần theo fitness
 │
 ├── SNAPSHOT
 │     ├── Gen 25%: lưu easyBot
 │     ├── Gen 50%: lưu mediumBot
 │     └── Gen cuối: lưu hardBot
 │
 └── TẠO THẾ HỆ MỚI
       ├── Elite: clone top 10% (tối thiểu 2) → giữ nguyên
       └── Còn lại: TournamentSelect(k=3) × 2 → CrossoverAndMutate
```

### Crossover — Uniform

Mỗi gene của con được chọn ngẫu nhiên 50/50 từ cha hoặc mẹ:
```csharp
child.genes[i] = Random.value > 0.5f ? parentA.genes[i] : parentB.genes[i];
```

### Mutation — Gaussian

Xác suất `mutationRate = 8%` mỗi gene bị đột biến:
```
z = sqrt(-2 × ln(u1)) × cos(2π × u2)   // Box-Muller transform
gene[i] += z × mutationMag              // mutationMag = 0.12
gene[i] = clamp(gene[i], 0, 1)
```

> Gaussian cho phép đột biến nhỏ xung quanh giá trị hiện tại (thay vì random hoàn toàn), giúp hội tụ mượt hơn.

### Selection — Tournament (k=3)

Chọn ngẫu nhiên 3 cá thể, lấy cá thể có fitness cao nhất. Cân bằng giữa áp lực chọn lọc và duy trì đa dạng.

---

## 6. AILibrary — Thư Viện Bot

Sau khi training, 3 bot được lưu vào `Resources/AI_Library.json`:

| Bot | Snapshot tại | Đặc điểm |
|-----|-------------|----------|
| `easyBot` | Gen 25% | Chiến lược còn non — mắc nhiều sai lầm |
| `mediumBot` | Gen 50% | Chiến lược tương đối tốt |
| `hardBot` | Gen cuối | Chromosome tốt nhất sau toàn bộ quá trình |

Training **tự động bỏ qua** nếu file đã có đủ 3 bot hợp lệ. Dùng `[ContextMenu] ForceRetrain()` để train lại.

---

## 7. Giới Hạn & Khoảng Trống Hiện Tại

| Vấn đề | Mô tả | Ảnh hưởng |
|--------|-------|-----------|
| **Không có Hand** | Bot không mô phỏng hand slots — unit mua thẳng lên board | Không thể giữ unit khi board đầy, mất cơ hội accumulate merge |
| **Spell dùng ngay** | Spell được cast tức thì sau khi mua, không có chiến lược cất spell | Không mô phỏng "dành spell cho đúng thời điểm" |
| **Tinh Hoa Hợp Nhất** | Bot mô phỏng effect trực tiếp trong TryMerge() (SimulateMergeReward), không đi qua hand | Không chờ đến lúc board có slot tốt — apply ngay lập tức |
| **Fitness đơn giản** | Chỉ tính thắng/hòa, không có reward shaping (margin thắng, số lượt sống) | Có thể bỏ lỡ chiến lược "thắng nhanh" hoặc "sống lâu" |
| **Self-match bias** | Chromosome đấu đối thủ random từ population (uniform) — không guaranteed gặp bot mạnh | Fitness noise cao khi population nhỏ |
| **Combat triggers** | Bot không mô phỏng OnAttack, OnTakeDamage, OnAllyDeath trong shop phase | Đánh giá một số unit có thể không chính xác (ví dụ: Osiris trong shop phase) |
| **Giới hạn merge level** | TryMerge dừng ở mergeLevel == 2 (3 sao) theo thiết kế | Đúng theo gameplay |

---

## 8. Sơ Đồ Luồng Dữ Liệu

```
[JSON CardsData] ──→ CardDatabase ──→ GetRandomUnitShop()
                                   └→ GetRandomSpellShop()
                                            │
                                            ↓
                                    [unitShop, spellShop]
                                            │
                              ┌─────────────┴─────────────┐
                              ↓                           ↓
                        BotAgent A                  BotAgent B
                    (Chromosome brain)          (Chromosome brain)
                              │                           │
                        board[8 slots]              board[8 slots]
                              └──────────┬────────────────┘
                                         ↓
                              CombatResolver.ResolveTurn()
                                (TurnRecord log)
                                         │
                                    ┌────┴────┐
                                  Win/Lose/Draw
                                         │
                                  chromo.fitness += ...
                                         │
                              ┌──────────┴──────────┐
                          Crossover            Mutation
                          (Uniform)            (Gaussian)
                                         │
                                   nextGeneration
                                         │
                              (lặp lại generations lần)
                                         │
                                   AI_Library.json
                              (easyBot / mediumBot / hardBot)
```

---

## 9. Hướng Phát Triển Tiếp Theo

1. **Thêm Hand** — `List<CardInstance> hand` (5 slots), bot giữ unit khi board đầy, merge từ hand+board, quyết định khi nào đưa lên board
2. **Reward shaping** — thưởng thêm theo biên thắng (HP còn lại sau trận), tốc độ thắng (số lượt)
3. **Fitness diversity** — tránh population hội tụ quá sớm: thưởng thêm nếu chromosome khác biệt với elite
4. **Combat ability evaluation** — tích hợp weight cho OnAttack/OnTakeDamage dựa trên số lần kích hoạt dự kiến
5. **Adaptive mutation** — giảm `mutationMag` khi fitness hội tụ (simulated annealing)
