# Báo cáo Tiến độ Dự án — AutoChess
**Cập nhật:** 26/05/08 | **Nhánh:** Khanh-dev | **Author:** VuKhah

---

## 1. Tổng quan Dự án

AutoChess là game thể loại **Auto-battler** (tự động chiến đấu) trên Unity. Người chơi mua và bố trí quân trên bàn cờ trong **Shop Phase**, sau đó các quân tự động chiến đấu trong **Battle Phase**. Mục tiêu: tích lũy đủ **10 Cup** trước khi cạn **7 HP**.

**Hai bộ tộc hiện có:** Babylon · Niles  
**Tổng số thẻ:** ~49 (22 Unit Babylon + 24 Unit Niles + 3 Magic)

---

## 2. Kiến trúc Hệ thống

```
┌─────────────────────────────────────────────────────────────┐
│                        DATA LAYER                           │
│  CardDefinition  ←→  AbilityData  ←→  CardsData.json       │
│  CardInstance (runtime state per unit)                      │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                      CORE ENGINE                            │
│  CombatResolver — hệ thống TTE (Trigger-Target-Effect)      │
│  GameRecord / TurnRecord / CombatAction (combat log)        │
│  EconomyManager (coin logic)                                │
└──────┬──────────────────────────────────────┬───────────────┘
       │                                      │
┌──────▼──────────┐                  ┌────────▼──────────────┐
│    MANAGERS     │                  │     AI SYSTEM         │
│  GameManager    │                  │  BotAgent             │
│  UIManager      │                  │  GATrainer (GA)       │
│  AIManager      │                  │  GameSimulator        │
│  CardDatabase   │                  │  Chromosome           │
└──────┬──────────┘                  │  AILibrary (3 levels) │
       │                             └───────────────────────┘
┌──────▼──────────┐
│      UI LAYER   │
│  CardUI         │ ← stats, ability icon, passive icons
│  CardVisuals    │ ← attack/die/shake animation
│  CardSlot       │ ← drag & drop, merge trigger, deploy event
│  CardDraggable  │ ← kéo thả trong shop phase
└─────────────────┘
```

---

## 3. Hệ thống TTE (Trigger → Target → Effect)

Đây là engine cốt lõi xử lý toàn bộ kỹ năng của unit.

### 3.1 TriggerType (12 loại)
| Value | Tên | Mô tả |
|-------|-----|-------|
| 0 | None | Không trigger |
| 1 | OnTakeDamage | Khi bị tấn công |
| 2 | OnAttack | Khi tấn công |
| 3 | OnDeath | Khi chết |
| 4 | StartOfBattle | Đầu trận |
| 5 | EndTurnShop | Cuối lượt Shop |
| 6 | OnDeploy | Khi đặt lên bàn |
| 7 | OnSell | Khi bán |
| 8 | OnAllyDeath | Khi đồng minh chết |
| 9 | OnAllySummon | Khi đồng minh được triệu hồi |
| 10 | OnAllyReborn | Khi đồng minh hồi sinh |
| 11 | Aura | Hiệu ứng thụ động đầu trận |

### 3.2 TargetType (12 loại)
| Value | Tên | Ghi chú |
|-------|-----|---------|
| 0 | None | — |
| 1 | Self | Bản thân |
| 2 | RandomAlly | Đồng minh ngẫu nhiên |
| 3 | AllAllies | Toàn bộ đồng minh |
| 4 | RandomEnemy | Kẻ địch ngẫu nhiên |
| 5 | DirectEnemy | Kẻ địch đối diện |
| 6 | LowestHealthAlly | Đồng minh HP thấp nhất |
| 7 | LeftAlly | Đồng minh bên trái |
| 8 | RightAlly | Đồng minh bên phải |
| 9 | AllNilesAllies | Toàn bộ Niles |
| 10 | AllBabylonAllies | Toàn bộ Babylon |
| 11 | TriggerSubject | Unit gây ra sự kiện (OnAllySummon, OnAllyDeath…) |

### 3.3 EffectType (10 loại)
| Value | Tên | Mô tả |
|-------|-----|-------|
| 0 | None | — |
| 1 | AddStats | Tăng ATK/HP |
| 2 | DealDamage | Gây sát thương |
| 3 | GiveBuff | Ban passive (Taunt/Reborn/Safeguard) |
| 4 | Summon | Triệu hồi unit |
| 5 | Destroy | Tiêu diệt tức thì |
| 6 | GainCoin | Cộng coin |
| 7 | Reborn | Hồi sinh unit (active effect) |
| 8 | TriggerAbility | Sao chép ability của target |
| 9 | SummonConsumed | Triệu hồi lại unit đã bị Consume |

### 3.4 Passive Keywords (độc lập với TTE)
| Keyword | Data field | Runtime field | Cơ chế |
|---------|-----------|---------------|--------|
| **Taunt** | `hasTaunt` | `isTaunt` | `FindTarget()` ưu tiên unit có Taunt |
| **Reborn** | `hasReborn` | `isReborn` | `CleanupBoard()` hồi sinh 1 HP thay vì xóa |
| **Safeguard** | `hasSafeguard` | `safeguardActive` | `ExecuteClash()` block đòn đầu, tiêu hao 1 lần |

---

## 4. Trạng thái Tính năng

### ✅ Hoàn thành

| Tính năng | File | Ghi chú |
|-----------|------|---------|
| Combat engine TTE | `CombatResolver.cs` | Đầy đủ 12 trigger, 12 target, 10 effect |
| Multi-ability system | `CardDefinition.cs` | `List<AbilityData> abilities` |
| Passive keywords | `CardInstance.cs`, `CombatResolver.cs` | Taunt, Reborn, Safeguard |
| Merge system | `CardSlot.cs` | 3 thẻ giống → merge lên cấp |
| Merge scale | `CombatResolver.cs` | `scaleFactor = mergeLevel + 1` |
| Tribe synergy | `CombatResolver.cs` | Babylon ≥2: +1 HP combat |
| Deploy trigger (Shop) | `CardSlot.cs` | Defer coroutine sau `OnEndDrag` |
| Deploy trigger (Battle) | `CombatResolver.cs` | Summon → fire OnDeploy |
| OnDeath + Summon fix | `CombatResolver.cs` | Bypass dead-target check cho Summon/SummonConsumed |
| Sekhmet "Consume" | `CombatResolver.cs`, `CardInstance.cs` | `isConsume` + `consumedCardIDs` |
| Aura trigger call | `CombatResolver.cs` | Gọi đầu trận song song StartOfBattle |
| Economy (coin/turn) | `GameManager.cs` | 10 + bonusCoinNextTurn + GainCoin units |
| Shop roll & lock | `GameManager.cs` | Tier tăng theo turn (cứ 2 turn +1 tier, max 6) |
| Shop tier system | `CardDatabase.cs`, `GameManager.cs` | `GetCurrentShopTier()` |
| Enemy team summon | `GameManager.cs` | 3 unit ngẫu nhiên theo tier |
| Combat visualization | `GameManager.cs`, `CardVisuals.cs` | AttackAnimation, DieAnimation |
| Win/lose detection | `GameManager.cs` | Cup tích lũy / HP giảm |
| AI Genetic Algorithm | `GATrainer.cs` | 50 cá thể, 100 thế hệ, 8 genes |
| 3 difficulty levels | `AILibrary.cs` | Easy (Gen10), Medium (Gen50), Hard (Gen100) |
| AI card evaluation | `BotAgent.cs` | Chấm điểm dựa trên stats + abilities + passives |
| CardUI passive icons | `CardUI.cs` | 3 field riêng: `tauntIcon`, `rebornIcon`, `safeguardIcon` |
| Summoned unit UI fix | `GameManager.cs` | `SpawnMissingBoardUI()` sau `ResolveTurn()` |
| Unit data Babylon | `CardsData.json` | 22 unit (Tier 1–6) |
| Unit data Niles | `CardsData.json` | 24 unit (Tier 1–6, gồm summoned tokens) |
| Magic cards | `CardsData.json` | 3 loại: StatBoost, AddTaunt, Economy |

---

### 🔧 Triển khai một phần (cần hoàn thiện)

| Tính năng | Trạng thái | Việc còn lại |
|-----------|-----------|--------------|
| **Passive icons UI** | Code xong | Cần thêm 3 Image objects vào CardUI Prefab trong Unity Editor + 3 sprite (`Passives/Taunt`, `Passives/Reborn`, `Passives/Safeguard`) |
| **Summoned unit mid-battle** | Logic xong | Unit triệu hồi rồi chết trong cùng combat không có animation — `TurnRecord` chưa log summon events |
| **AI Library file** | Trainer xong | `AI_Library.json` chưa được generate — cần chạy `GATrainer` trong Unity Editor một lần |
| **WinGame / GameOver** | Stub | Chỉ `Debug.Log`, chưa có UI screen, chưa có restart flow |
| **Hand system** | Slot tồn tại | `handSlots[]` được khai báo và reset trong `ResetAllCardsInSlots()` nhưng không có logic mua → tay → bàn rõ ràng |
| **BotAgent Safeguard scoring** | Thiếu | `Evaluate()` chưa tính điểm cho `hasSafeguard` |

---

### Chưa làm

| Tính năng | Ưu tiên | Ghi chú |
|-----------|---------|---------|
| Card art sprites | Cao | Tất cả unit dùng placeholder; cần `Resources/Sprites/Cards/Units/` |
| Tier icon sprites | Trung | `Resources/Sprites/Icons/Tiers/Tier_1..6` |
| Ability icon sprites | Trung | `Resources/Sprites/Icons/Abilities/Abi_1..9` |
| Passive icon sprites | Cao | `Resources/Sprites/Icons/Passives/Taunt/Reborn/Safeguard` |
| Merge animation | Thấp | Merge xảy ra instant, chưa có hiệu ứng |
| AI difficulty selection | Thấp | `AIManager.GetBrain()` có nhưng chưa có UI chọn độ khó |

---

## 5. Kiến trúc Dữ liệu Unit

### CardDefinition (thẻ)
```
cardID, cardName, cardType, tribe, baseATK, baseHP, cost, tier
├── abilities: List<AbilityData>   ← TTE abilities
├── hasTaunt: bool                 ← passive
├── hasReborn: bool                ← passive
├── hasSafeguard: bool             ← passive
└── description, magicGroup, statBonusATK, statBonusHP
```

### CardInstance (runtime)
```
Data (CardDefinition reference)
├── currentATK, currentHP
├── permanentATKBonus, permanentHPBonus
├── growthATKBonus, growthHPBonus
├── isTaunt, isReborn, safeguardActive    ← runtime passives
├── hasRebornUsed, mergeLevel, slotIndex
├── abilityTriggerCounts: List<int>       ← per-ability trigger counter
└── consumedCardIDs: List<string>         ← Sekhmet mechanic
```

---

## 6. Bug đã biết

       | # | Mô tả | File | Nghiêm trọng |
       |---|-------|------|-------------|
       | ~~B1~~ | ~~`ProjectSanityChecker` dùng API cũ: `new AbilityData { isTaunt = true }` thay vì `hasTaunt = true` trên `CardDefinition`~~ | ~~`ProjectSanityChecker.cs:50`~~ | **Đã sửa 26/05/08** |
       | B2 | Unit được triệu hồi rồi chết trong battle không có UI — `VisualizeAction` silently `yield break` các action liên quan | `GameManager.cs:237` | Trung |
       | B3 | `EconomyManager` tồn tại độc lập nhưng `GameManager` tự quản lý coin bằng `playerCoins` riêng — hai hệ thống không đồng bộ | `EconomyManager.cs`, `GameManager.cs` | Thấp (AI dùng EconomyManager, Game dùng GameManager) |
       | B4 | `WinGame()` và `GameOver()` chỉ là `Debug.Log` — không có state reset, không có UI | `GameManager.cs:377-378` | Cao |
       | B5 | `Aura` trigger được gọi đúng nhưng không có TargetType "AllAllies including self" — target Self chỉ buff bản thân, target AllAllies buff cả đội nhưng Aura thường muốn buff đồng minh khác | `CombatResolver.cs` | Trung |

---

## 7. Thống kê Codebase

| Module | File | Số dòng ước tính |
|--------|------|-----------------|
| Core Engine | `CombatResolver.cs` | ~500 |
| Data | `AbilityData.cs`, `CardDefinition.cs`, `CardInstance.cs` | ~200 |
| Managers | `GameManager.cs`, `UIManager.cs`, `AIManager.cs`, `CardDatabase.cs` | ~450 |
| AI | `GATrainer.cs`, `BotAgent.cs`, `GameSimulator.cs`, `Chromosome.cs` | ~250 |
| UI | `CardUI.cs`, `CardVisuals.cs`, `CardSlot.cs`, `CardDraggable.cs` | ~350 |
| Records | `GameRecord.cs` | ~60 |
| Test | `ProjectSanityChecker.cs` | ~80 |
| **Tổng** | **20 files** | **~1890 dòng** |

---

## 8. Lộ trình Tiếp theo (Đề xuất)

### Sprint ngắn hạn (ưu tiên cao)
1. **Fix WinGame/GameOver** — thêm UI screen + restart flow
2. **Sprite assets** — card art, tier icons, passive icons cho Taunt/Reborn/Safeguard
3. **Olympus tribe** — thêm unit data + synergy
4. **Niles synergy** — thiết kế và implement

### Sprint trung hạn
5. **TurnRecord summon events** — log summon actions để VisualizeAction animate đúng
6. **Fix ProjectSanityChecker** — cập nhật test dùng API mới
7. **BotAgent Safeguard scoring** — thêm `hasSafeguard` vào `Evaluate()`
8. **Chạy GATrainer** — generate `AI_Library.json`

### Dài hạn
9. **Difficulty UI** — cho người chơi chọn Easy/Medium/Hard
10. **Sound system** — hiệu ứng âm thanh cơ bản
11. **More tribes** — nếu mở rộng (Olympus units, synergy riêng)
12. **Ranked/scoring** — hệ thống điểm dài hạn

---

## 9. Session Summaries

| File | Ngày | Nội dung chính |
|------|------|----------------|
| `summary_260805_1640.md` | 26/08/05 04:40 | GainCoin phase fix, CleanupBoard UI, OnDeploy stale board, GiveBuff/Destroy/TriggerAbility, Tribe target, triggerLimit, targetTribe filter, Merge scale |
| `Summary_260805_2122.md` | 26/08/05 21:22 | Fix Deploy GainCoin (ExecuteEffect missing case), OnDeploy cho Summon trong Battle, Refactor `ability` → `List<AbilityData> abilities` toàn bộ codebase + 47 JSON entries |
| *(session hiện tại)* | 26/05/08 | Sekhmet Consume mechanic, Passive keyword redesign (Taunt/Reborn/Safeguard tách khỏi TTE), CardUI 3 passive icons riêng, SpawnMissingBoardUI fix |
