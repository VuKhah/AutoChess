# AutoChess — Game Design & Technical Document

> Dự án game đối kháng thẻ bài tự động (Auto Battler) xây dựng bằng Unity C#, tích hợp thuật toán Genetic Algorithm để huấn luyện bot AI.

---

## Mục lục

1. [Tổng quan dự án](#1-tổng-quan-dự-án)
2. [Cấu trúc thư mục](#2-cấu-trúc-thư-mục)
3. [Vòng lặp gameplay](#3-vòng-lặp-gameplay)
4. [Shop Phase](#4-shop-phase)
5. [Battle Phase](#5-battle-phase)
6. [Hệ thống thẻ bài](#6-hệ-thống-thẻ-bài)
7. [Hệ thống năng lực (TTE Framework)](#7-hệ-thống-năng-lực-tte-framework)
8. [Tribe Synergy](#8-tribe-synergy)
9. [Kinh tế trong game](#9-kinh-tế-trong-game)
10. [Bản cờ & vị trí](#10-bản-cờ--vị-trí)
11. [Hệ thống AI — Genetic Algorithm](#11-hệ-thống-ai--genetic-algorithm)
12. [Luồng dữ liệu & Kiến trúc code](#12-luồng-dữ-liệu--kiến-trúc-code)
13. [Hệ thống UI](#13-hệ-thống-ui)
14. [Điều kiện thắng/thua](#14-điều-kiện-thắng--thua)
15. [Cơ chế đặc biệt & Edge cases](#15-cơ-chế-đặc-biệt--edge-cases)

---

## 1. Tổng quan dự án

AutoChess là game chiến thuật thẻ bài tự động (Auto Battler) lấy cảm hứng từ thể loại "Teamfight Tactics / Hearthstone Battlegrounds". Người chơi xây dựng đội hình thẻ bài trong **Shop Phase**, sau đó hai đội tự động chiến đấu nhau trong **Battle Phase** — hoàn toàn không có sự can thiệp của người chơi.

**Điểm đặc biệt của dự án:**
- Ba bộ tộc thẻ (Tribe) với cộng hưởng riêng
- Hệ thống năng lực cực kỳ linh hoạt dạng **Trigger-Target-Effect (TTE)**
- **Death Stack LIFO** xử lý chuỗi cái chết phức tạp không bị lỗi cascade
- Bot AI được huấn luyện bằng **Genetic Algorithm** qua 100 thế hệ
- Ba độ khó (Easy/Medium/Hard) tương ứng ba checkpoint huấn luyện

---

## 2. Cấu trúc thư mục

```
Assets/
├── Scripts/
│   ├── AI/                  # Genetic Algorithm, BotAgent, GameSimulator
│   ├── Core/                # CombatResolver, AbilityEngine, CardInstance
│   ├── Data/                # CardDefinition, AbilityData (blueprint dữ liệu)
│   ├── Managers/            # GameManager (+ partials), UIManager, CardDatabase, AIManager
│   └── UI/                  # CardUI, CardDraggable, CardSlot, CardVisuals, BattlePhaseLayout
├── Resources/
│   ├── CardsData.json       # Toàn bộ dữ liệu thẻ (Unit + Spell)
│   ├── AI_Library.json      # Chromosome đã huấn luyện (Easy/Medium/Hard)
│   └── Sprites/             # Art: Cards/Units/, Cards/Spells/, Icons/Passives/
├── Prefabs/                 # Card prefab (CardUI + CardVisuals + CardDraggable)
├── Scenes/
│   ├── SceneMain.unity      # Scene chính (gameplay)
│   └── SampleScene.unity    # Scene thử nghiệm
└── Document/                # Tài liệu thiết kế, mockup giao diện
```

**Scripts theo vai trò:**

| File | Vai trò |
|------|---------|
| `GameManager.cs` | Singleton trung tâm, quản lý state game |
| `GameManager.Shop.cs` | Logic mua bán, roll shop |
| `GameManager.Combat.cs` | Điều phối Battle Phase |
| `GameManager.Board.cs` | Sinh đội bot, sync UI ↔ board |
| `CombatResolver.cs` | Engine chiến đấu tự động |
| `AbilityEngine.cs` | Thực thi năng lực TTE |
| `AbilityEngine.Targets.cs` | Phân giải TargetType → danh sách đích |
| `CardInstance.cs` | Trạng thái runtime của mỗi thẻ |
| `CardDefinition.cs` | Blueprint tĩnh (dữ liệu JSON) |
| `AbilityData.cs` | Cấu hình một năng lực |
| `BotAgent.cs` | Agent AI ra quyết định |
| `GATrainer.cs` | Vòng huấn luyện Genetic Algorithm |
| `GameSimulator.cs` | Mô phỏng trận đấu cho GA |
| `Chromosome.cs` | Vector gene của bot |
| `CardDatabase.cs` | Load + random shop từ JSON |
| `EconomyManager.cs` | Quản lý coins |
| `UIManager.cs` | Điều phối toàn bộ UI |
| `CardUI.cs` | Hiển thị thông tin thẻ |
| `CardDraggable.cs` | Kéo thả thẻ |
| `CardSlot.cs` | Nhận thẻ, xử lý mua/bán/merge |
| `CardVisuals.cs` | Animation (deploy, attack, die) |
| `BattlePhaseLayout.cs` | Chuyển đổi layout Shop ↔ Battle |

---

## 3. Vòng lặp gameplay

```
[Chọn độ khó]
       │
       ▼
┌─────────────────────────────────────┐
│            SHOP PHASE               │  ← Turn N
│  • Nhận 10 coin (+ bonus)           │
│  • Shop hiển thị 5 thẻ ngẫu nhiên  │
│  • Mua / Bán / Roll / Khóa shop    │
│  • Sắp xếp thẻ lên board (7 ô)     │
│  • Nhấn "Start Fight"              │
└─────────────┬───────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│           BATTLE PHASE              │  ← Hoàn toàn tự động
│  • Tribe synergy kích hoạt         │
│  • StartOfBattle abilities         │
│  • Vòng lặp tấn công xen kẽ       │
│  • Death Stack xử lý cái chết     │
│  • Pending Summons xử lý triệu hồi │
└─────────────┬───────────────────────┘
              │
              ├─ Thắng → +1 Cup
              ├─ Thua  → -1 HP
              │
              ▼
       [Kiểm tra thắng thua]
        HP ≤ 0   → Game Over
        Cup ≥ 10 → Victory
        Else     → Turn N+1 (quay lại Shop Phase)
```

**Giới hạn:** Tối đa **20 lượt**. Sau 20 lượt không phân thắng bại thì kết thúc theo điểm.

---

## 4. Shop Phase

### 4.1 Cấp độ shop (Shop Tier)

Shop Tier tăng dần theo lượt, quyết định pool thẻ xuất hiện:

```
ShopTier = min( (currentTurn + 1) / 2,  6 )
```

| Lượt | Shop Tier |
|------|-----------|
| 1–2  | 1 |
| 3–4  | 2 |
| 5–6  | 3 |
| 7–8  | 4 |
| 9–10 | 5 |
| 11+  | 6 |

### 4.2 Bảng tỷ lệ rơi (Drop Rate)

Mỗi ô shop chọn ngẫu nhiên tier theo bảng xác suất (Shop Level × Card Tier):

| Shop Lv | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Tier 5 | Tier 6 |
|---------|--------|--------|--------|--------|--------|--------|
| 1 | cao | — | — | — | — | — |
| 2 | nhiều | vừa | — | — | — | — |
| 3 | vừa | nhiều | ít | — | — | — |
| 4 | ít | vừa | nhiều | ít | — | — |
| 5 | — | ít | vừa | nhiều | ít | — |
| 6 | — | — | ít | vừa | nhiều | ít |

*(Số liệu chính xác trong `CardDatabase.cs` — bảng 6×6 float)*

### 4.3 Các hành động trong Shop Phase

| Hành động | Mô tả | Chi phí |
|-----------|-------|---------|
| **Mua thẻ** | Kéo từ Shop → Hand hoặc Board | Gold bằng `cost` của thẻ |
| **Bán thẻ** | Kéo từ Hand/Board → Shop slot | +1 Gold, kích hoạt `OnSell` |
| **Roll** | Làm mới 5 thẻ trong shop | 1 Gold |
| **Khóa (Lock)** | Giữ nguyên shop sang lượt sau | Miễn phí; tự mở sau 1 lượt |
| **Deploy** | Kéo từ Hand → Board | Miễn phí; kích hoạt `OnDeploy` |
| **Move** | Kéo từ Board ↔ Board | Miễn phí; không kích hoạt OnDeploy |
| **Dùng Spell** | Kéo Spell → Unit trên board | Tiêu thụ spell, áp dụng hiệu ứng |

### 4.4 Merge thẻ (Nâng sao)

Khi một thẻ drop vào ô đã có thẻ **cùng loại**:

```
mergeLevel 0 (★)   → ATK × 1, HP × 1
mergeLevel 1 (★★)  → ATK × 2, HP × 2
mergeLevel 2 (★★★) → ATK × 3, HP × 3
```

Merge tự động khi:
- Kéo thẻ cùng ID vào ô nhau (trong Hand hoặc Board)
- `CardSlot.CheckMergeNextFrame()` quét toàn bộ Hand + Board sau mỗi thao tác

---

## 5. Battle Phase

### 5.1 Tổng quan

Battle Phase hoàn toàn tự động. Người chơi chỉ quan sát. Toàn bộ logic chạy trong `CombatResolver.ResolveTurn()`.

### 5.2 Trình tự chiến đấu

```
Phase 1: Setup
  ├── Áp dụng Tribe Synergy (buff ATK/HP)
  ├── Trigger StartOfBattle abilities
  └── Áp dụng Aura (passive liên tục)

Phase 2: Vòng lặp chiến đấu (tối đa 50 round)
  ├── BuildAttackStack()
  │     └── Xen kẽ: P[0]↔E[0], P[1]↔E[1], ..., P[6]↔E[6]
  ├── ExecuteClash()
  │     ├── Cả hai đánh nhau đồng thời
  │     ├── Trigger OnAttack (attacker)
  │     ├── Trigger OnTakeDamage (nếu còn sống sau đòn)
  │     └── Ghi lastAttacker
  ├── ScanAllBoardsForNewDeaths()
  ├── FlushDeathStack() ← LIFO
  │     ├── OnDeath (victim)
  │     ├── OnAllyDeath (cùng tribe)
  │     ├── Reborn check
  │     ├── OnAllyReborn (nếu hồi sinh)
  │     ├── CleanupBoard (null hóa ô chết)
  │     └── ProcessNextPendingSummon()
  └── Lặp lại cho đến khi một bên trống

Phase 3: Kết quả
  ├── Thắng → +1 Cup
  ├── Thua  → -1 HP
  └── RecordSnapshots() (lưu cho AI phân tích)
```

### 5.3 Death Stack (LIFO)

Cơ chế xử lý cái chết theo **Last-In-First-Out** tránh lỗi cascade:

```
[Unit A chết] → push vào stack
[Unit B chết] → push vào stack

FlushDeathStack():
  Pop B → OnDeath(B) → OnAllyDeath(B) → Reborn(B)?
  Pop A → OnDeath(A) → OnAllyDeath(A) → Reborn(A)?
  → ProcessNextPendingSummon (1 unit/cycle)
  → Lặp lại nếu còn death mới
```

Điều này đảm bảo:
- Ability OnDeath không bị chen ngang bởi cái chết khác
- Reborn được xử lý đúng thứ tự
- Summon từ death không tức thì (queue 1 per cycle)

### 5.4 Pending Summon Queue

Khi ability triệu hồi unit mới trong combat:
- Unit không xuất hiện ngay → push vào `pendingSummons` stack
- Mỗi cycle của FlushDeathStack pop **1 unit** ra đặt vào board
- Ngăn Sekhmet consume unit vừa được triệu hồi trong cùng tick

### 5.5 Cơ chế tấn công

**Targeting mặc định:** slot cùng vị trí đối diện
**Taunt override:** nếu địch có unit Taunt đang sống → bắt buộc nhắm vào đó
**Safeguard:** hấp thụ 1 đòn (cả attack lẫn ability damage) → deactivate

---

## 6. Hệ thống thẻ bài

### 6.1 CardDefinition (Blueprint)

Dữ liệu tĩnh lưu trong `CardsData.json`:

```csharp
string  cardID          // ID duy nhất
string  cardName        // Tên hiển thị
CardType type           // Unit | Spell
Tribe   tribe           // Babylon | Olympus | Niles | None
int     baseATK         // Tấn công cơ bản
int     baseHP          // Máu cơ bản
int     cost            // Giá mua (gold)
int     tier            // 1–6 (độ hiếm)
List<AbilityData> abilities  // Danh sách năng lực
bool    hasTaunt        // Passive: Khiêu khích
bool    hasReborn       // Passive: Hồi sinh 1 lần
bool    hasSafeguard    // Passive: Giáp phòng thủ 1 đòn
List<SpellEffectData> spellEffects  // Chỉ dành cho Spell
```

### 6.2 CardInstance (Runtime State)

Trạng thái trong khi game chạy:

```csharp
CardDefinition def       // Blueprint gốc
int  mergeLevel          // 0, 1, 2 (★ ★★ ★★★)
int  currentATK          // ATK thực tế hiện tại
int  currentHP           // HP hiện tại
int  maxHP               // HP tối đa (có bonus)

// Bonuses (tích lũy theo thời gian)
int  permanentATK/HP     // Buff vĩnh viễn (sống qua battle)
int  growthATK/HP        // Tích lũy từ ability Growth

// Passive state
bool isTaunt, isReborn, safeguardActive
bool hasRebornUsed       // Đã dùng Reborn lần này chưa

// Tracking chiến đấu
bool onDeathProcessed    // Đã xử lý death chưa
CardInstance lastAttacker // Ai đã giết unit này
bool isBattleSpawned     // Sinh ra trong combat (không restore)
List<string> consumedCardIDs // Sekhmet mechanic
```

**Công thức tính stat:**
```
currentATK = baseATK × (mergeLevel + 1) + keepRatio × (growthATK + permanentATK)
currentHP  = baseHP  × (mergeLevel + 1) + keepRatio × (growthHP  + permanentHP)
keepRatio  = 0.7
```

### 6.3 Spell Cards

- Không có ATK/HP, không lên board chiến đấu
- Kéo Spell → Unit để áp dụng `spellEffects`
- Tiêu thụ sau khi dùng
- Có thể: BuffStats, GainCoin, ToggleTaunt, LoseLife,...

---

## 7. Hệ thống năng lực (TTE Framework)

Mỗi ability định nghĩa bằng ba chiều: **Trigger → Target → Effect**

### 7.1 TriggerType (11 loại)

| Trigger | Mô tả |
|---------|-------|
| `OnTakeDamage` | Khi bị đánh trúng và còn sống |
| `OnAttack` | Khi tấn công (dù thắng hay thua) |
| `OnDeath` | Khi chết |
| `StartOfBattle` | Đầu trận chiến |
| `EndTurnShop` | Cuối lượt (trong shop) |
| `OnDeploy` | Khi đặt lên board lần đầu |
| `OnSell` | Khi bán |
| `OnAllyDeath` | Khi đồng đội cùng tribe chết |
| `OnAllySummon` | Khi đồng đội cùng tribe được triệu hồi |
| `OnAllyReborn` | Khi đồng đội cùng tribe hồi sinh |
| `Aura` | Passive liên tục (áp dụng đầu trận) |

### 7.2 TargetType (12 loại)

| Target | Mô tả |
|--------|-------|
| `Self` | Bản thân |
| `RandomAlly` | 1 đồng đội ngẫu nhiên |
| `AllAllies` | Tất cả đồng đội |
| `AllAlliesExceptSelf` | Tất cả đồng đội trừ bản thân |
| `RandomEnemy` | 1 địch ngẫu nhiên |
| `DirectEnemy` | Địch đối diện (theo slot) |
| `LowestHealthAlly` | Đồng đội ít máu nhất |
| `LeftAlly` | Đồng đội bên trái |
| `RightAlly` | Đồng đội bên phải |
| `AllNilesAllies` | Tất cả đồng đội tộc Niles |
| `AllBabylonAllies` | Tất cả đồng đội tộc Babylon |
| `TriggerSubject` | Unit đã trigger sự kiện (ví dụ đồng đội vừa chết) |

### 7.3 EffectType (9 loại)

| Effect | Mô tả | Params |
|--------|-------|--------|
| `AddStats` | Tăng ATK/HP | effectValue1=ATK, effectValue2=HP |
| `DealDamage` | Gây sát thương | effectValue1=damage |
| `GiveBuff` | Cho passive (Taunt/Reborn/Safeguard) | flags |
| `Summon` | Triệu hồi unit mới | effectValue1=cardID |
| `Destroy` | Tiêu diệt ngay (+ consume nếu isConsume) | — |
| `GainCoin` | Thêm coins | effectValue1=amount |
| `Reborn` | Hồi sinh với HP cố định | effectValue1=hp |
| `TriggerAbility` | Kích hoạt lại OnDeath abilities | — |
| `SummonConsumed` | Triệu hồi tất cả unit đã consume | — |

### 7.4 Các cờ điều khiển ability

| Cờ | Mô tả |
|----|-------|
| `isPermanent` | Buff AddStats sống qua battle (tích vào permanentATK/HP) |
| `triggerLimit` | Số lần kích hoạt tối đa (0 = vô hạn) |
| `conditionCount` | Chỉ kích hoạt mỗi N lần trigger |
| `isEscalating` | effectValue tăng thêm mỗi lần kích hoạt |
| `isConsume` | Destroy + lưu cardID vào `consumedCardIDs` |

### 7.5 Growth Mechanic

Ability với `StartOfBattle` + `AddStats` (không có `isPermanent`) là **Growth**:
- Tích lũy vào `growthATK/HP` mỗi lần kích hoạt
- Nhân với `(mergeLevel + 1)` (unit ★★★ grow nhanh gấp 3)
- Thể hiện chiến lược "đầu tư dài hạn"

---

## 8. Tribe Synergy

Ba bộ tộc mỗi loại có synergy riêng áp dụng đầu trận:

| Tribe | Ngưỡng | Buff |
|-------|--------|------|
| **Babylon** | ≥ 2 units | +1 HP mỗi unit Babylon |
| **Olympus** | ≥ 2 units | +1 ATK mỗi unit Olympus |
| **Niles** | ≥ 3 units | +2 HP mỗi unit Niles |

**Lưu ý:** OnAllyDeath, OnAllySummon, OnAllyReborn chỉ kích hoạt giữa các unit **cùng tribe**. Điều này tạo ra động lực xây đội theo tribe để tối ưu hóa chuỗi combo.

---

## 9. Kinh tế trong game

### 9.1 Vòng tiền

```
Đầu lượt: Coins = 10 + bonus_from_last_turn
Roll:      -1 coin
Mua thẻ:   -cost coin
Bán thẻ:   +1 coin
Spell:     có thể tặng coin (GainCoin effect)
```

### 9.2 EconomyManager

```csharp
ResetEconomy()    // Áp dụng bonus từ lượt trước, reset về 10+bonus
TrySpend(cost)    // Trừ coin, trả false nếu không đủ
Earn(amount)      // Cộng coin ngay lập tức
AddBonus(amount)  // Xếp hàng cho lượt sau
Sell()            // +1 coin ngay
```

---

## 10. Bản cờ & vị trí

### 10.1 Cấu trúc Board

7 slot được tổ chức theo mô hình **xen kẽ frontline/backline**:

```
Người chơi nhìn (1-indexed):  1   2   3   4   5   6   7
Code index (0-indexed):        0   1   2   3   4   5   6
Vai trò:                      [F] [B] [F] [B] [F] [B] [F]
                               Frontline  Backline
```

- **Frontline** (F): slot 1,3,5,7 → code index 0,2,4,6 (index chẵn)
- **Backline** (B): slot 2,4,6   → code index 1,3,5   (index lẻ)

**Visual layout trong Battle Phase:**

```
                     ← gần địch ←

ENEMY BOARD:
  Frontline  [E0]   [E2]   [E4]   [E6]
  Backline      [E1]   [E3]   [E5]

──────────────── trung tâm bàn cờ ────────────────

PLAYER BOARD:
  Backline      [P1]   [P3]   [P5]
  Frontline  [P0]   [P2]   [P4]   [P6]

                     ← gần địch ←
```

### 10.2 Quy tắc Frontline Protection

Backline chỉ bị tấn công khi **cả 2 frontline kề bên đã trống**:

| Target (1-indexed) | Điều kiện để tấn công được |
|--------------------|---------------------------|
| Slot 2 (backline)  | Slot 1 VÀ Slot 3 phải trống |
| Slot 4 (backline)  | Slot 3 VÀ Slot 5 phải trống |
| Slot 6 (backline)  | Slot 5 VÀ Slot 7 phải trống |
| Slot 1,3,5,7 (frontline) | Luôn có thể bị tấn công |

**Taunt là ngoại lệ:** Unit có Taunt kéo toàn bộ fire **bất kể** vị trí frontline/backline. Nếu unit Taunt đang nằm ở backline và được che bởi frontline, kẻ địch vẫn phải nhắm vào nó.

**Ripple search:** Khi target ưu tiên (cùng index) bị block, attacker tìm unit hợp lệ gần nhất tính từ slot đó ra 2 bên.

### 10.3 Quy tắc vị trí khác trong Battle

- **LeftAlly / RightAlly:** dựa trên index trong board list (i-1, i+1)
- **Aura:** áp dụng cho toàn đội vào đầu trận

### 10.4 Layout chuyển đổi

`BattlePhaseLayout` lưu vị trí RectTransform cho cả hai trạng thái:
- **Shop Phase:** Grid layout bình thường
- **Battle Phase:** Frontline và backline xen kẽ — frontline gần địch hơn, backline lùi ra sau và nằm giữa 2 frontline kề

---

## 11. Hệ thống AI — Genetic Algorithm

### 11.1 Chromosome

Mỗi bot AI biểu diễn bằng **8 gene** (float 0–1):

| Index | Tên | Ý nghĩa trong đánh giá thẻ |
|-------|-----|---------------------------|
| 0 | Aggression | Trọng số baseATK |
| 1 | Survival | Trọng số baseHP |
| 2 | Persistence | Trọng số hiệu ứng OnDeath |
| 3 | Defense | Trọng số Taunt + counter-damage |
| 4 | Growth | Trọng số StartOfBattle AddStats |
| 5 | Reborn | Trọng số khả năng hồi sinh |
| 6 | BoardPresence | (Dự phòng) |
| 7 | Risk | (Dự phòng) |

### 11.2 Hàm đánh giá thẻ (BotAgent.Evaluate)

```csharp
score = baseATK * genes[0]       // Sát thương cơ bản
      + baseHP  * genes[1]       // Độ bền
      + 8 * genes[3]             // nếu hasTaunt
      + 8 * genes[3]             // nếu có counter-damage ability
      + 12 * genes[4]            // nếu có Growth (StartOfBattle AddStats)
      + 10 * genes[5]            // nếu hasReborn
      + 10 * genes[2]            // nếu có OnDeath AddStats
```

Bot tham lam chọn thẻ có `score` cao nhất còn đủ tiền mua.

### 11.3 Quy trình huấn luyện (GATrainer)

```
Population: 50 chromosomes
Generations: 100

Mỗi generation:
  1. Evaluate fitness:
     - Mỗi chromosome đấu 10 trận vs bot ngẫu nhiên
     - Thắng → +10, Hòa → +2, Thua → +0
  
  2. Snapshot:
     - Gen 10  → easyBot   (AI yếu nhất)
     - Gen 50  → mediumBot
     - Gen 100 → hardBot   (AI mạnh nhất)
  
  3. Selection:
     - Rank by fitness, giữ top 5 (10% elitism)
  
  4. Crossover:
     - Uniform crossover: mỗi gene có 50% xác suất từ parent A hoặc B
  
  5. Mutation:
     - 5% xác suất mỗi gene biến đổi ±0.1
  
  6. Refill đến 50 chromosomes, lặp lại
```

### 11.4 GameSimulator

Mô phỏng trận đấu **không có UI** để huấn luyện nhanh:

```
SimulateMatch(botA, botB):
  For turn = 1..20:
    botA.DecidePrepPhase(randomShop)
    botB.DecidePrepPhase(randomShop)
    result = CombatResolver.ResolveTurn(boardA, boardB)
    Cập nhật HP theo kết quả
  Return: 1 (A thắng), -1 (B thắng), 0 (hòa)
```

### 11.5 Độ khó & AI Library

```json
// AI_Library.json
{
  "easyBot":   { "genes": [...], "fitness": 42 },
  "mediumBot": { "genes": [...], "fitness": 78 },
  "hardBot":   { "genes": [...], "fitness": 95 }
}
```

Người chơi chọn độ khó qua UI → `GameManager.selectedDifficulty` → `AIManager.GetBrain(difficulty)` → `new BotAgent(chromosome)`.

---

## 12. Luồng dữ liệu & Kiến trúc code

### 12.1 Singleton pattern

```
GameManager.Instance
UIManager.Instance
CardDatabase.Instance
AIManager.Instance
EconomyManager.Instance
BattlePhaseLayout.Instance
```

Tất cả kế thừa pattern: `Awake()` set `Instance = this`, duplicate bị `Destroy`.

### 12.2 Luồng một lượt hoàn chỉnh

```
Turn Start
  ├── EconomyManager.ResetEconomy()
  ├── CardDatabase.GetRandomShop(5, shopTier)
  ├── GameManager.Shop.RefreshShop()
  └── UIManager.UpdateStats()

[Người chơi thao tác...]

"Start Fight" button
  ├── GameManager.Combat.SnapshotPreCombatBoard()
  ├── GameManager.Board.SummonEnemyTeam()
  │     └── bot.DecidePrepPhase(shop)
  ├── GameManager.Combat.SyncBoards()
  ├── CombatResolver.ResolveTurn(playerBoard, enemyBoard, log)
  │     └── [Toàn bộ battle tự động]
  ├── CheckVictoryConditions()
  └── GameManager.Combat.EndCombatAndPrepareNextTurn()
        ├── RestorePreCombatPlayerBoard()
        └── ExecuteNextTurn() → RefreshShop()
```

### 12.3 Đồng bộ UI ↔ Internal State

`SyncBoards()` duyệt Card gameObject trong hierarchy UI → build lại `playerBoard[]` / `enemyBoard[]`:
- Thứ tự trong hierarchy = thứ tự slot
- Critical: kéo thả thay đổi hierarchy, sync phải chạy trước battle

### 12.4 GameRecord & Logging

```csharp
GameRecord
  └── List<TurnRecord>
        └── List<CombatAction>   // attacker → defender, damage
        └── CardSnapshot[]       // HP, ATK snapshot cuối trận
```

Dùng để phân tích sau này hoặc AI học từ lịch sử.

---

## 13. Hệ thống UI

### 13.1 HUD

| Element | Mô tả |
|---------|-------|
| HP counter | Máu người chơi (bắt đầu cao, giảm khi thua) |
| Coin/Cup display | Hiển thị Coins (shop) hoặc Cups (battle/end) |
| Roll button | Làm mới shop -1 coin |
| Lock button | Khóa shop sang lượt sau |
| Start Fight button | Bắt đầu chiến đấu |
| Difficulty panel | Easy / Medium / Hard (chọn trước khi bắt đầu) |
| Victory / Game Over screen | Hiện cuối game với nút Restart |

### 13.2 Card Visual

```
┌──────────────┐
│ [ATK]  [HP]  │  ← Stats (Unit) hoặc [Cost] (Spell)
│              │
│   [Card Art] │  ← Sprite từ Resources/Sprites/Cards/
│              │
│ [AbilIcon]   │  ← Icon ability đầu tiên
│ [T][R][S]    │  ← Passive icons: Taunt, Reborn, Safeguard
│      [Tier]  │  ← Merge level badge
└──────────────┘
```

HP bar đổi màu đỏ khi bị thương.

### 13.3 Drag & Drop

```
BeginDrag → layer DRAGGING_LAYER, tắt raycast
Drag      → follow cursor
EndDrag   → CardSlot.OnDrop(PointerEventData)
  ├── Xác định type: Spell hay Unit
  ├── Xác định nguồn: Shop / Hand / Board
  ├── Xác định đích: Shop / Hand / Board
  └── Thực thi: Buy / Sell / Move / Deploy / Spell Apply / Merge
```

Không cho kéo trong Battle Phase (`GameManager.isCombatActive`).

### 13.4 Animations (CardVisuals)

| Animation | Trigger | Mô tả |
|-----------|---------|-------|
| `SetUprightPose` | Shop | Thẻ đứng thẳng |
| `SetBoardPose` | Board | Thẻ xoay 90° nằm |
| `PlayDeployToBoard` | Deploy | Rotate, lift lên, overshoot xuống |
| `AttackAnimation` | Combat | Bay đến đích, shake, về chỗ |
| `ShakeEffect` | Nhận đòn | Rung lắc |
| `DieAnimation` | Chết | Đỏ → thu nhỏ → fade out |

---

## 14. Điều kiện thắng / thua

| Điều kiện | Kết quả |
|-----------|---------|
| Người chơi thắng battle | +1 Cup |
| Người chơi thua battle | -1 HP |
| HP ≤ 0 | **GAME OVER** |
| Cup ≥ 10 | **VICTORY** |
| Đủ 20 lượt | Kết thúc theo điểm hiện tại |

---

## 15. Cơ chế đặc biệt & Edge cases

### 15.1 Sekhmet — Consume & SummonConsumed

Unit Sekhmet có ability `Destroy + isConsume`:
- Khi destroy một unit: lưu cardID vào `consumedCardIDs`
- OnDeath: `SummonConsumed` → hồi sinh **tất cả** unit đã consume với HP 1
- `isBattleSpawned = true` + `hasRebornUsed = true` để ngăn chúng dùng Reborn lại

### 15.2 Reborn Chain

```
Unit chết → isReborn && !hasRebornUsed?
  → Revive(1 HP), hasRebornUsed = true
  → OnAllyReborn broadcast
  → Tiếp tục chiến đấu
```

Sau battle: `hasRebornUsed` reset về false (ready cho lượt sau).

### 15.3 Safeguard Interaction

- Block **mọi** nguồn damage: attack trực tiếp, DealDamage ability, Destroy-as-damage
- Một khi deactivate không tự restore trong cùng battle
- Có thể được grant lại bởi `GiveBuff + isSafeguard`

### 15.4 Growth vs Permanent

| Loại bonus | Tích ở | Sống qua battle? | Reset? |
|------------|--------|-----------------|--------|
| Growth (`StartOfBattle + AddStats`) | `growthATK/HP` | Có | Không (cộng dồn mãi) |
| Permanent (`isPermanent + AddStats`) | `permanentATK/HP` | Có | Không |
| Temporary (trong battle) | `currentATK/HP` | Không | Reset sau battle |

### 15.5 Restore sau Battle

Sau mỗi battle:
1. `isBattleSpawned == true` → xóa unit (không restore)
2. Các unit gốc → `ResetStats()` (về max HP, giữ growth/permanent)
3. Enemy board bị xóa hoàn toàn

### 15.6 Escalating Abilities

`isEscalating = true`: mỗi lần kích hoạt, effectValue1 tự tăng +1:
```
Lần 1: damage = 2
Lần 2: damage = 3
Lần 3: damage = 4
...
```

Tạo ra snowball effect cho unit tồn tại lâu.

### 15.7 ConditionCount Trigger

`conditionCount = N`: chỉ fire ability sau **mỗi N lần** trigger:
```csharp
abilityTriggerCounts[i]++;
if (abilityTriggerCounts[i] % conditionCount == 0)
    ExecuteEffect();
```

Ví dụ: "Cứ 3 lần bị đánh thì phản đòn".

---

## Phụ lục: File dữ liệu chính

| File | Nội dung | Được load bởi |
|------|----------|---------------|
| `Resources/CardsData.json` | Toàn bộ thẻ Unit + Spell | `CardDatabase.cs` |
| `Resources/AI_Library.json` | 3 chromosome AI đã train | `AIManager.cs` |
| `Document/02_Data/CardsBabylons.json` | (Archive) Thẻ Babylon | — |
| `Document/02_Data/CardsNiles.json` | (Archive) Thẻ Niles | — |
| `Document/02_Data/CardsSpells.json` | (Archive) Spell data | — |

---

*Tài liệu này phản ánh trạng thái code tại branch `Khanh-dev`, commit `15808e7` — "Update targeting and merge tier icons".*
