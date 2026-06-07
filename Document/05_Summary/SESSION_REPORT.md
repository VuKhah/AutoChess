# Báo cáo Phiên Làm Việc — AutoChess
**Commit:** `ca03b80` | **Branch:** `Khanh-dev` | **Ngày:** 2026-05-21

---

## Tổng quan

Phiên này tập trung vào 5 mảng chính: tách Shop, hoàn thiện hệ thống Spell, cải tổ Combat targeting, thêm Visual effects, và nâng cấp toàn diện hệ thống AI/Training.

---

## 1. Shop — Tách 5 Unit + 2 Spell

### Vấn đề
`GetRandomShop()` dùng `GetAllCards()` nên Unit và Spell xuất hiện lẫn lộn ở mọi slot, không theo thứ tự.

### Giải pháp
| File | Thay đổi |
|---|---|
| `CardDatabase.cs` | Thay `GetRandomShop` bằng 2 hàm riêng: `GetRandomUnitShop()` và `GetRandomSpellShop()` |
| `GameManager.cs` | Thêm field `shopUnitCount = 5` (chỉnh được trong Inspector) |
| `GameManager.Shop.cs` | `RefreshShop()` fill slot 0–4 bằng unit, slot 5–6 bằng spell |
| `GameSimulator.cs` | Đổi shop bot từ 3 → 5 unit (khớp game thật) |
| `GameManager.Board.cs` | `GetRandomShop` → `GetRandomUnitShop` cho enemy bot |

---

## 2. Hệ thống Spell — Hoàn thiện

### 2a. Visual (CardUI)
- **Star icon** không còn hiển thị trên spell card (`mergeLevel=0` → ẩn toàn bộ sao)
- Thêm `Debug.LogWarning` khi sprite art load thất bại → dễ trace lỗi Texture Type trong Unity

### 2b. Effects — Implement đủ 12 effect còn thiếu
| Effect | Tên | Spell sử dụng |
|---|---|---|
| 10 | GetRandomUnit | Quick Recruit, Military Support |
| 11 | StealFromShop | Sanctum Heist |
| 12 | GainIncome | Trader's Trick, Caishen's Knock |
| 13 | GetSameRealmUnit | Tailored Recruit |
| 14 | LoseLife | Devil's Deal |
| 15 | TransferStats | Gate of Destruction |
| 16 | ConditionalCoinGain | Wager |
| 17 | UpgradeTierUnit | Ritual of the Realm |
| 18 | GiveDoubleAtkAndSafeguard | Olympic Flame |
| 20 | BuffByShopTier | Divine Inspiration |
| 21 | GiveEndTurnBuff | Rising Spirit |

### 2c. Helper methods thêm vào `GameManager.Shop.cs`
```
AddUnitToHand()        — effects 10, 13
StealRandomUnitFromShop() — effect 11
LoseLife()             — effect 14
RegisterWagerReward()  — effect 16 (wager + combat hook)
AddPermanentIncome()   — effect 12 vĩnh viễn
TransferStatsToRandom() — effect 15
UpgradeSameTribeUnit() — effect 17
DestroyUnit() / RefreshCardUI() — internal helpers
```

### 2d. EconomyManager
- Thêm `permanentIncomeBonus` — tăng thu nhập mỗi turn vĩnh viễn (effect 12 isPermanent)

### 2e. Bug Taunt không được gán
**Root cause:** `ApplySpellToUnit` gọi `targetUnit.ResetStats()` sau mỗi effect → `isTaunt = Data.hasTaunt` ghi đè giá trị vừa set.

**Fix:** Set đồng thời `Data.hasTaunt = true` (Data đã là clone riêng của instance) trong effect 1 và effect 19.

---

## 3. Combat Targeting — Frontline/Backline

### 3a. Layout cũ → Lỗi
Layout cũ dùng index chẵn/lẻ (0,2,4,6 = frontline; 1,3,5 = backline) không đúng với thiết kế thực tế.

### 3b. Layout mới
```
Frontline : slot 1–4  (index 0–3) — luôn lộ ra, tấn công trước
Backline  : slot 5–7  (index 4–6) — chỉ lộ khi TOÀN BỘ frontline đã chết
```

### 3c. Thứ tự tấn công
```
0 → 1 → 2 → 3 → 4 → 5 → 6   (frontline trước, backline sau)
```

### 3d. FindTarget — 3 ưu tiên
```
1. Taunt  → bypass hoàn toàn frontline protection
2. Frontline còn sống (index 0–3) → chọn gần attacker nhất
3. Backline lộ ra (index 4–6)    → chỉ khi frontline đã sạch
```

Thêm helper `ClosestTo()` — chọn unit có `|slot - attackerSlot|` nhỏ nhất trong pool.

### 3e. Hằng số
```csharp
private const int FrontlineCount = 4; // dễ điều chỉnh sau
```

---

## 4. Bug Fixes — Upamaki (U_11_Niles)

Card "OnDeploy: Consume đồng minh bên trái. Slain: Triệu hồi lại tất cả unit đã Consume."

### Bug 1 — `slotIndex` không cập nhật
`CardInstance` luôn có `slotIndex = 0` (gán lúc tạo, không đổi) → `LeftAlly` target tính ra index `-1` → không tìm được mục tiêu.

**Fix:** `SyncBoards()` nay update `unit.slotIndex = i` mỗi lần sync.

### Bug 2 — `consumedCardIDs` bị xóa
`ResetStats()` reset `consumedCardIDs = new List<string>()` mỗi combat → Upamaki chết trong combat nhưng `SummonConsumed` không có gì để triệu hồi.

**Fix:** Xóa dòng đó khỏi `ResetStats()`. List chỉ bị clear khi `SummonConsumed` thực sự kích hoạt.

### Bug 3 — Unit bị Consume không biến mất khỏi board
`Destroy` effect chỉ set HP=0, không destroy GameObject trong shop phase.

**Fix:** `CleanupDeadBoardUnitsUI()` gọi sau `TriggerOnDeploy` — destroy GameObject của mọi unit có `IsDead=true` trên sân player.

### Bug 4 — Animation deploy
`PlayDeployToBoard()` chỉ chạy từ Shop→Board.

**Fix:** Điều kiện đổi thành `sourceSlot != PlayerBoard` → animation chạy cả từ Hand→Board.

---

## 5. Visual Effects — Real-time & Flash

### 5a. Real-time stat updates trong battle
**Root cause:** `ResolveTurn()` tính offline → stat thay đổi từ ability không cập nhật UI trong battle.

**Giải pháp — Observer pattern:**
```
AbilityEngine.onStatChanged  →  CombatResolver log StatChange action
→  VisualizeStatChange() cập nhật ATK/HP + play FlashEffect
```

| File | Thay đổi |
|---|---|
| `GameRecord.cs` | Thêm `StatChange` vào `CombatActionType`, thêm `FlashType` enum |
| `AbilityEngine.cs` | Thêm `onStatChanged` observer, fire sau AddStats / GiveBuff / DealDamage / ApplyGrowth |
| `CombatResolver.cs` | Set up observer trong `ResolveTurn()` |
| `GameManager.Combat.cs` | `VisualizeStatChange()` + `GetFlashColor()`, skip 0.1s delay cho StatChange |

StatChange không có delay → không block nhịp battle.

### 5b. Flash màu theo loại effect
| FlashType | Màu | Khi nào |
|---|---|---|
| Buff | 🟢 Xanh lá | AddStats dương, Growth |
| Debuff | 🔴 Đỏ | DealDamage, AddStats âm |
| Status | 🔵 Xanh dương | Taunt, Safeguard, Reborn |
| SynergyBabylon | 🟡 Vàng | Synergy Babylon kích hoạt |
| SynergyOlympus | 🩵 Cyan | Synergy Olympus kích hoạt |
| SynergyNiles | 💚 Lục | Synergy Niles kích hoạt |

`FlashEffect()` lerp `characterArt.color` → flash màu → trở về trắng (0.35s, fire-and-forget).

### 5c. Synergy flash
`ApplyTribeSynergies()` nay log `StatChange` với FlashType tương ứng cho mỗi unit được buff → flash đồng loạt lúc battle bắt đầu.

---

## 6. AI — Chromosome 24 Genes

### 6a. Vấn đề với 8 genes
- Genes [6] và [7] chưa được dùng
- Không tính tribe synergy theo tribe cụ thể
- Không tính merge completion
- Không phân biệt trigger vs effect (hardcode từng case)
- Không có save threshold
- Không nhận biết frontline/backline

### 6b. Thiết kế 24 genes — 6 nhóm chức năng

```
Nhóm 1 — Stat         [0-3]  : ATK, HP, TierBonus, CostEfficiency
Nhóm 2 — Passive      [4-6]  : Taunt, Reborn, Safeguard
Nhóm 3 — Trigger      [7-12] : StartBattle, OnDeath, OnAttack, OnTakeDmg, EndTurn, OnDeploy
Nhóm 4 — Effect       [13-17]: AddStats, Summon, DealDmg, GainCoin, GiveBuff
Nhóm 5 — Synergy      [18-20]: Babylon, Olympus, Niles
Nhóm 6 — Context      [21-23]: MergeBonus, FrontlineBias, SaveThreshold
```

### 6c. Evaluate — Multiplicative scoring
```
ability_score += TriggerWeight(trigger) × EffectWeight(effect) × 10
```

GA tự học kết hợp nào có giá trị (vd: `StartBattle × AddStats` vs `OnDeath × Summon`).

### 6d. Context awareness mới
- **Merge proximity**: 1 copy → +8×gene[21]; 2 copies → +16×gene[21] (gần lên sao)
- **Frontline bias**: bonus khi mua Taunt cho slot frontline còn trống
- **Save threshold**: nếu `bestScore < gene[23] × 3` → dừng mua, tiết kiệm tiền

### 6e. BuyAndPlace — Frontline/Backline aware
```
Taunt  → frontline (0-3) trước, backline sau nếu hết chỗ
Khác   → backline (4-6) trước, frontline sau nếu hết chỗ
```

### 6f. GATrainer — Cải tiến thuật toán
| | Cũ | Mới |
|---|---|---|
| Population | 50 | 100 |
| Generations | 100 | 150 |
| Matches/chromo | 10 | 15 |
| Selection | Random từ elite | Tournament (k=3) |
| Crossover | Random elite (growing pool bug) | Elite pool cố định |
| Mutation rate | 5% | 8% (configurable) |
| Mutation magnitude | ±0.1 | ±0.12 (configurable) |

**Tournament selection:** chọn winner từ k đấu thủ ngẫu nhiên trong elite → tăng selection pressure, tránh genetic drift.

---

## Tổng số thay đổi

| Metric | Số liệu |
|---|---|
| Files thay đổi | 19 |
| Lines thêm | +829 |
| Lines xóa | -226 |
| Commit | `ca03b80` |

### Files chính

| File | Nội dung thay đổi |
|---|---|
| `CardDatabase.cs` | GetRandomUnitShop / GetRandomSpellShop |
| `GameManager.Shop.cs` | RefreshShop tách slot, 8 spell helper methods |
| `GameManager.Combat.cs` | SyncBoards + slotIndex, CleanupDeadBoardUnitsUI, VisualizeStatChange, FlashColor |
| `CombatResolver.cs` | Frontline/Backline layout, FindTarget 3-priority, ClosestTo, Synergy flash log |
| `AbilityEngine.cs` | Tất cả spell effects 10-21, observer onStatChanged với FlashType |
| `CardInstance.cs` | Không reset consumedCardIDs |
| `GameRecord.cs` | StatChange action type, FlashType enum |
| `CardVisuals.cs` | FlashEffect coroutine |
| `CardUI.cs` | Ẩn star icon cho spell, art warning |
| `CardDraggable.cs` | Deploy animation từ Hand→Board |
| `CardSlot.cs` | CleanupDeadBoardUnitsUI sau OnDeploy |
| `EconomyManager.cs` | permanentIncomeBonus |
| `BotAgent.cs` | 24-gene Evaluate, trigger×effect, frontline placement |
| `Chromosome.cs` | 24 genes với documentation đầy đủ |
| `GATrainer.cs` | Tournament selection, configurable params, better logging |
| `GameSimulator.cs` | Shop 5 units, ResetStats giữa rounds |
