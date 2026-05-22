# AutoChess — Tài liệu Hệ thống

> Cập nhật lần cuối: 2026-05-22

---

## Mục lục

1. [Luật Game](#1-luật-game)
2. [Hệ thống Chiến đấu](#2-hệ-thống-chiến-đấu)
3. [Hệ thống Kinh tế](#3-hệ-thống-kinh-tế)
4. [Hệ thống Merge](#4-hệ-thống-merge)
5. [Hệ thống Spell](#5-hệ-thống-spell)
6. [Hệ thống AI — Chromosome & Gen](#6-hệ-thống-ai--chromosome--gen)
7. [Thuật toán GA Training](#7-thuật-toán-ga-training)
8. [Cách chạy Training](#8-cách-chạy-training)
9. [Kiến trúc Code](#9-kiến-trúc-code)

---

## 1. Luật Game

### Điều kiện thắng/thua

| Điều kiện | Kết quả |
|-----------|---------|
| Tích lũy đủ **10 cups** | Thắng |
| Đến lượt **21** (vượt maxTurns=20) | Thắng |
| **playerHP ≤ 0** | Thua |

Mỗi lượt player thua combat → mất 1 HP (bắt đầu với 7 HP).  
Mỗi lượt player thắng combat → +1 cup.  
Hòa (cả 2 bên cùng chết) → không thay đổi HP hay cup.

### Cấu trúc 1 lượt

```
Shop Phase
  ├── Xem shop (5 unit + N spell)
  ├── Reroll (tốn 1 coin), Freeze, Mua/Bán
  ├── Đặt unit lên board (kéo thả)
  └── Nhấn "Fight" → bắt đầu Combat Phase

Combat Phase
  ├── Bot cũng chuẩn bị board riêng
  ├── ResolveTurn() — tính toán tức thì toàn bộ trận
  ├── Visualization — chiếu lại từng đòn có animation
  └── Kết quả → cập nhật HP/cups → quay lại Shop Phase
```

### Board layout

```
Backline  [ 4 ][ 5 ][ 6 ]          ← chỉ lộ khi frontline đã chết
Frontline [ 0 ][ 1 ][ 2 ][ 3 ]     ← luôn bị nhắm trước
```

Taunt: bỏ qua frontline protection, phải bị tấn công trước.

---

## 2. Hệ thống Chiến đấu

### Flow `ResolveTurn`

```
1. ApplyTribeSynergies (Babylon/Olympus/Niles)
2. TriggerAbility(StartOfBattle) cho tất cả units
3. Battle Loop (tối đa 50 round):
   │  BuildAttackStack — xen kẽ P[0], E[0], P[1], E[1]...
   │  Pop từng AttackIntent:
   │    ├── ExecuteClash(attacker, defender)
   │    ├── TriggerAbility(OnAttack, OnTakeDamage)
   │    ├── FlushDeathStack (OnDeath → OnAllyDeath → Reborn)
   │    └── EarlyExit nếu 1 bên bị xóa sổ
   └── Break khi 1 bên empty
4. RecordSnapshots
```

### Tribe Synergies

| Tribe | Yêu cầu | Hiệu ứng |
|-------|---------|---------|
| Babylon | ≥ 2 units | Mỗi Babylon unit +1 HP |
| Olympus | ≥ 2 units | Mỗi Olympus unit +1 ATK |
| Niles | ≥ 3 units | Mỗi Niles unit +2 HP |

Synergy chỉ tồn tại trong 1 trận, không tích lũy qua turn.

### Target Selection Priority

1. **Taunt** — phải bị tấn công trước, bỏ qua mọi thứ
2. **Frontline** (slot 0–3) — chặn backline
3. **Backline** (slot 4–6) — chỉ lộ khi frontline đã chết hết
4. Trong mỗi nhóm: chọn unit **gần nhất** (theo slot index)

### Death Stack System

Mỗi khi unit chết → đưa vào `deathStack`. FlushDeathStack xử lý theo LIFO:

```
OnDeath(victim) → OnAllyDeath(đồng minh cùng tribe) → CleanupBoard
  ├── isReborn + !hasRebornUsed → ReviveDefault() + OnAllyReborn
  └── else → board[i] = null
```

### Stat Formula

```
currentATK = Round(baseATK × tier + 0.7 × (growthATK + permanentATK)) + tempSpellATK
maxHP      = Round(baseHP  × tier + 0.7 × (growthHP  + permanentHP))  + tempSpellHP
```

`tier = mergeLevel + 1` (1 / 2 / 3)  
`keepRatio = 0.7` — bonus không compound 100% khi tier tăng

---

## 3. Hệ thống Kinh tế

### Coin mỗi lượt

```
Coin = 10 + bonusNextTurn + permanentIncomeBonus
```

- `bonusNextTurn`: từ ability `GainCoin` với `AddBonus()`
- `permanentIncomeBonus`: từ spell `GainIncome` (vĩnh viễn)
- Coin **không cộng dồn** qua turn — coin thừa mất

### Giá

| Hành động | Chi phí |
|-----------|---------|
| Reroll shop | 1 coin |
| Mua unit | Cost của card (1–6) |
| Bán unit | +1 coin |
| Mua spell | Cost của spell |

### Bot Handicap

| Độ khó | Coin/turn |
|--------|-----------|
| Easy | 7 |
| Medium | 9 |
| Hard | 10 |

---

## 4. Hệ thống Merge

### Quy tắc

| Merge | Cần | Kết quả |
|-------|-----|---------|
| lv0 → lv1 | 3 × lv0 | 1 × lv1 (1-star) |
| lv1 → lv2 | 2 × lv1 | 1 × lv2 (2-star) |

**Tổng nguyên liệu cho lv2:** 3 × lv0 → lv1, rồi thêm 1 lv1 nữa = **6 cards lv0 gốc**.

```csharp
CardInstance.MergeRequiredCount(mergeLevel)
// mergeLevel == 0  →  3
// mergeLevel != 0  →  2
```

### Cơ chế merge (player)

Khi đặt/mua card lên board/hand:
1. `CheckForMerge(cardID, mergeLevel)` quét toàn bộ board + hand
2. Nếu tìm đủ số cần → `PerformMerge(matches.GetRange(0, required))`
3. Keeper = bản sao có `permanentATK + permanentHP + growthATK + growthHP` cao nhất
4. Trigger `CheckMergeNextFrame` → chain merge (lv0→lv1→lv2 liên tiếp)

### Shop Blink Hint

Shop card nhấp nháy khi `owned ≥ MergeRequiredCount(lv) - 1`:
- lv0 card: blink khi đang có ≥ 2 lv0 copies (mua thêm 1 = merge)
- lv1 card: blink khi đang có ≥ 1 lv1 copy (mua thêm 1 = merge)

---

## 5. Hệ thống Spell

### Spell Effects (Player)

| Effect | Mã | Mô tả |
|--------|----|-------|
| BuffStats | 1 | Buff ATK/HP (permanent hoặc tạm thời 1 trận) |
| GainCoin | 6 | Nhận coin ngay |
| GetRandomUnit | 10 | Nhận N unit ngẫu nhiên Tier X vào hand |
| StealFromShop | 11 | Lấy 1 unit từ shop vào hand (miễn phí) |
| GainIncome | 12 | Tăng thu nhập (permanent → tích lũy mỗi turn) |
| GetSameRealmUnit | 13 | Nhận 1 unit cùng tộc với target vào hand |
| LoseLife | 14 | Mất HP ngay lập tức |
| TransferStats | 15 | Hy sinh 1 unit, chuyển ATK+HP sang unit ngẫu nhiên |
| ConditionalCoinGain | 16 | Wager: thắng trận kế tiếp nhận coin |
| UpgradeTierUnit | 17 | Nâng mergeLevel +1 cho 1 unit cùng tộc |
| GiveDoubleAtkAndSafeguard | 18 | ATK × ~1.7 + Safeguard |
| ToggleTaunt | 19 | Bật/tắt Taunt (tắt → +ATK; bật → +HP) |
| BuffByShopTier | 20 | +ATK và +HP = Tier shop hiện tại |
| GiveEndTurnBuff | 21 | Unit nhận buff mỗi cuối lượt (vĩnh viễn) |

### Non-Permanent Spell Buff (Bug đã sửa)

Buff tạm thời (`isPermanent = false`) được lưu vào `tempSpellATKBonus/HP`:
- Tồn tại qua `ResetStats()` đầu tiên (khi apply spell)
- Có hiệu lực trong trận đấu
- Bị xóa bởi `ResetStats()` sau trận (`RestorePreCombatPlayerBoard`)

### Bot Spell Application

Bot dùng `ApplySpellToBoard()` riêng (không gọi GameManager):
- Hoạt động trong cả gameplay lẫn `GameSimulator` training
- Support: BuffStats, GainCoin, GetRandomUnit, GainIncome, Upgrade, DoubleAtk, ToggleTaunt, BuffByShopTier, GiveEndTurnBuff
- Skip: LoseLife, TransferStats, ConditionalCoinGain (không phù hợp context bot)

---

## 6. Hệ thống AI — Chromosome & Gen

Mỗi bot AI được điều khiển bởi 1 `Chromosome` gồm **32 gene**, mỗi gene là `float` trong `[0, 1]`.

### 32 Gene

```
── NHÓM 1: Chỉ số gốc ─────────────────────────────────────
[0]  wATK          Trọng số ATK khi đánh giá card
[1]  wHP           Trọng số HP khi đánh giá card
[2]  wTierBonus    Bonus thêm mỗi cấp Tier vượt 1
[3]  wCostEff      Hệ số hiệu quả chi phí (÷ cost × (1+gene))

── NHÓM 2: Passive keywords ────────────────────────────────
[4]  wTaunt        Giá trị passive Taunt
[5]  wReborn       Giá trị passive Reborn
[6]  wSafeguard    Giá trị passive Safeguard

── NHÓM 3: Trigger weights ─────────────────────────────────
[7]  tStartBattle  StartOfBattle — buff trước trận
[8]  tOnDeath      OnDeath — deathrattle
[9]  tOnAttack     OnAttack
[10] tOnTakeDmg    OnTakeDamage
[11] tEndTurnShop  EndTurnShop — growth tích lũy
[12] tOnDeploy     OnDeploy / OnAlly events

── NHÓM 4: Effect weights ──────────────────────────────────
[13] eAddStats     AddStats
[14] eSummon       Summon / SummonConsumed / Consume
[15] eDealDmg      DealDamage
[16] eGainCoin     GainCoin / kinh tế
[17] eGiveBuff     GiveBuff / Reborn / status

── NHÓM 5: Tribe synergy ───────────────────────────────────
[18] sBabylon      Bonus per đồng đội Babylon
[19] sOlympus      Bonus per đồng đội Olympus
[20] sNiles        Bonus per đồng đội Niles

── NHÓM 6: Board context ───────────────────────────────────
[21] wMerge        Bonus merge proximity (đang có bản sao trên board)
[22] wFrontline    Bonus khi mua Taunt vào frontline trống
[23] wSaveThresh   Ngưỡng tối thiểu để mua card (< gene×3 → bỏ qua)

── NHÓM 7: Reroll behavior ─────────────────────────────────
[24] wRerollThresh Reroll nếu bestShop < gene × bestBoard
[25] wRerollMax    Số lần reroll tối đa = floor(gene×3)+1 → [1..4]
[26] wRerollKeep   Giữ lại floor(gene×4) coin trước khi reroll → [0..4]
[27] wProactiveSell Bán unit điểm < gene×3 dù board chưa đầy

── NHÓM 8: Spell behavior ──────────────────────────────────
[28] wSpellThresh  Ngưỡng mua spell: score/cost > gene×3
[29] wSpellOnStrong Ưu tiên cast spell lên unit có EvaluateInstance cao nhất
[30] wSpellOnMerged Ưu tiên cast spell lên unit mergeLevel cao hơn
[31] wSpellEconomy Trọng số riêng cho spell kinh tế (GainCoin, GainIncome...)
```

### Bot Decision Loop mỗi lượt

```
DecidePrepPhase(unitShop, spellShop, shopTier)
  │
  ├── 1. RerollPhase
  │      • Nếu gene[24] > 0.05
  │      • Tối đa floor(gene[25]×3)+1 lần reroll
  │      • Reroll nếu bestShopScore < gene[24] × bestBoardScore
  │      • Giữ ≥ floor(gene[26]×4) coin trong túi
  │
  ├── 2. BuyUnitsPhase (Greedy)
  │      • Tìm card có điểm cao nhất còn đủ tiền
  │      • Bỏ qua nếu điểm < gene[23]×3
  │      • Board đầy → cân nhắc bán unit yếu nhất
  │
  ├── 3. BuySpellsPhase
  │      • Sort spells theo EvaluateSpell() giảm dần
  │      • Mua nếu score/cost > gene[28]×3
  │      • Dùng ngay lên unit tốt nhất (theo gene[29], gene[30])
  │
  ├── 4. ProactiveSellPhase
  │      • Bán unit có EvaluateInstance < gene[27]×3
  │      • Tạo chỗ cho các lượt sau
  │
  └── 5. TryMerge
         • 3 lv0 → 1 lv1 | 2 lv1 → 1 lv2
         • Giữ bản sao có tổng bonus cao nhất
         • Chain merge (lv0→lv1→lv2 nếu đủ)
```

### Evaluate(card) — Công thức chấm điểm

```
score = ATK × g[0] + HP × g[1] + (tier-1) × g[2] × 5

      + Taunt?     g[4] × 10
      + Reborn?    g[5] × 12
      + Safeguard? g[6] × 8

      + Σ ability: TriggerWeight × EffectWeight × 10
                   (OnAlly*: nhân thêm sameTribeCount/2)

      + sameTribe × SynergyWeight × 4

      + copies_lv0 × g[21] × (copies==2 ? 16 : 8)   ← merge proximity

      + (Taunt: emptyFront × g[22] × 2)              ← frontline bias

      ÷ cost × (1 + g[3])                            ← cost efficiency
```

### 3 Đối thủ thay phiên

Game dùng 3 bot song song:

```
enemyBots[0] = BotAgent(easyBrain,    7 coins)
enemyBots[1] = BotAgent(mediumBrain,  9 coins)
enemyBots[2] = BotAgent(hardBrain,   10 coins)

Mỗi lượt:
  • Tất cả 3 bot đều EndCombatPhase + DecidePrepPhase (phát triển board)
  • currentOpponentIndex = (currentTurn - 1) % 3
  • Turn 1→Easy, 2→Medium, 3→Hard, 4→Easy, ...
```

Board của mỗi bot tồn tại độc lập — bot không đấu sẽ tích lũy unit dần dần.

---

## 7. Thuật toán GA Training

### Tổng quan

```
Population (N chromosome)
  ↓  Evaluate fitness (M matches mỗi chromosome)
  ↓  Sort by fitness
  ↓  Snapshot → easyBot (gen 25%), mediumBot (gen 50%)
  ↓  Elitism (top 10% giữ nguyên)
  ↓  Tournament Selection (k=3) → Crossover + Mutation
  ↓  Repeat × generations
  ↓  hardBot = best của gen cuối
  ↓  Save → AI_Library.json
```

### Cấu hình

| | Quick | Production |
|-|-------|------------|
| Population | 30 | 100 |
| Generations | 40 | 150 |
| Matches/chromo | 5 | 15 |
| Mutation rate | 8% | 8% |
| Mutation mag | 0.12 | 0.12 |

### Fitness

```
Win  → +10
Draw →  +2
Loss →  +0
```

### Opponent Selection (Unbiased)

```csharp
int selfIdx = population.IndexOf(chromo);
int oppIdx  = Random.Range(0, population.Count - 1);  // [0, N-2]
if (oppIdx >= selfIdx) oppIdx++;                       // shift → [0,N-1]\{selfIdx}
```

Đảm bảo mọi chromosome đều có xác suất bằng nhau làm đối thủ.

### Crossover (Uniform)

```
child[i] = Random(0,1) > 0.5  ?  parentA[i]  :  parentB[i]
```

### Mutation (Box-Muller Gaussian)

```
Với xác suất 8% mỗi gene:
  u1 = Random.value
  z  = √(-2 × ln(u1)) × cos(2π × Random.value)   ← phân phối chuẩn N(0,1)
  gene[i] += z × 0.12
  gene[i] = Clamp01(gene[i])
```

### Snapshots (Difficulty Levels)

| Bot | Lấy từ | Ý nghĩa |
|-----|--------|---------|
| Easy | Gen 25% (gen 10/40) | Chromosome chưa tối ưu, còn nhiều sai lầm |
| Medium | Gen 50% (gen 20/40) | Chromosome học được cơ bản |
| Hard | Gen cuối (gen 39/40) | Chromosome tốt nhất, tối ưu nhất |

---

## 8. Cách chạy Training

### Không cần mở Unity Editor

```powershell
# Trong thư mục project:

# Quick mode (~2 phút, test xem hoạt động không)
.\train_ai.ps1

# Production mode (~20 phút, kết quả thực sự)
.\train_ai.ps1 -Production
```

Script tự tìm `Unity.exe` trong `C:\Program Files\Unity\Hub\Editor\`.  
Log lưu vào `training_log.txt`.  
Kết quả lưu vào `Assets/Resources/AI_Library.json`.

### Từ Unity Editor (menu)

**Tools → AI → Train AI — Quick (30 pop × 40 gen)**  
**Tools → AI → Train AI — Production (100 pop × 150 gen)**

### Khi nào cần retrain?

Khi mở game, `GATrainer.IsLibraryValid()` kiểm tra:
- File `AI_Library.json` tồn tại không?
- Có đủ 3 bot (easy/medium/hard) không?
- Mỗi bot có đủ 32 gene không?

Nếu **không đủ điều kiện** → training tự động bắt đầu.  
File 24-gene cũ sẽ tự bị thay thế vì `genes.Length < GeneCount (32)`.

---

## 9. Kiến trúc Code

### Scripts quan trọng

```
Assets/Scripts/
  Core/
    CombatResolver.cs    — Điều phối toàn bộ chiến đấu, death stack
    AbilityEngine.cs     — TTE engine: Trigger → Target → Effect
    AbilityEngine.Targets.cs — Resolve target từ TargetType enum
    CardInstance.cs      — Runtime state của 1 card đang chơi
  Data/
    CardDefinition.cs    — Định nghĩa card (static data từ JSON)
    AbilityData.cs       — Định nghĩa ability (TriggerType, EffectType...)
  Managers/
    GameManager.cs       — State game, win/loss, multi-bot setup
    GameManager.Combat.cs — Combat visualization, phase control
    GameManager.Board.cs — Board sync, enemy spawning, restore
    GameManager.Shop.cs  — Shop refresh, merge hints, spell helpers
    EconomyManager.cs    — Coin management
    CardDatabase.cs      — Load/serve card data từ Resources
    AIManager.cs         — Load AI_Library.json, serve brains
  AI/
    Chromosome.cs        — 32-gene genotype
    BotAgent.cs          — Bot decision loop (reroll, buy, spell, merge)
    GATrainer.cs         — GA training coroutine (chạy trong Unity)
    GameSimulator.cs     — Fast simulation cho training
    AILibrary.cs         — Serializable container (easy/medium/hard)
  UI/
    CardSlot.cs          — Drag-drop, trigger merge check
    CardUI.cs            — Hiển thị card stats, star icons
    CardVisuals.cs       — Animations (attack, die, reborn, burst)

Assets/Editor/
  AITrainingBatch.cs     — Training đồng bộ, chạy không cần GUI
```

### Các constant quan trọng

| Constant | Giá trị | Ý nghĩa |
|----------|---------|---------|
| `GameManager.BoardSlotCount` | 7 | Số slot trên board (slot 0-6) |
| `CombatResolver.FrontlineCount` | 4 | Frontline là slot 0-3 |
| `Chromosome.GeneCount` | 32 | Số gene mỗi chromosome |
| `GameManager.maxTurns` | 20 | Số lượt tối đa |
| `GameManager.playerHP` | 7 | HP ban đầu |
| `GameManager.winConditionCups` | 10 | Cups cần để thắng |
| `EconomyManager.base` | 10 | Coin mỗi lượt |
| `keepRatio` | 0.7 | Tỷ lệ giữ lại bonus khi ResetStats |

### Luồng dữ liệu qua 1 trận đấu

```
StartCombatPhase()
  │
  ├── SnapshotPreCombatBoard()     // Lưu board hiện tại
  ├── SummonEnemyTeam()
  │     ├── bot.EndCombatPhase()   // Dọn dead units từ trận trước
  │     ├── bot.DecidePrepPhase()  // Bot mua + dùng spell
  │     └── SpawnBotBoard()        // Render bot board (instance thật)
  ├── SyncBoards()                 // enemyBoard[i] = bot.board[i]
  └── CombatSequence()
        ├── ResolveTurn()          // Tính toán toàn bộ combat instant
        ├── VisualizeAction()×N    // Chiếu lại từng action
        ├── CheckVictoryConditions()
        └── EndCombatAndPrepareNextTurn()
              ├── RestorePreCombatPlayerBoard()  // ResetStats player units
              ├── CleanupEnemySlots()
              └── ExecuteNextTurn()              // Sang lượt mới
```
