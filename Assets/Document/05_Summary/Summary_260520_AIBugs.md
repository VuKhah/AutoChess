# Summary — 2026-05-20 — Bug Analysis: Hệ thống AI (GATrainer + GameSimulator + BotAgent)

## Tổng quan

Code AI chưa được sửa lần nào kể từ khi viết. Phân tích sâu toàn bộ luồng huấn luyện phát hiện **8 bug**, chia làm 3 mức độ.

---

## 🔴 CRITICAL — Simulation không trung thực với luật game

### BUG-AI-01: Bot mua lặp cùng 1 thẻ nhiều lần (shop không bị tiêu)

**File:** `BotAgent.cs` — `DecidePrepPhase`

**Code lỗi:**
```csharp
while (bought)
{
    bought = false;
    CardDefinition bestCard = null;

    foreach (var card in shop)       // ← shop KHÔNG bao giờ bị xóa thẻ
    {
        if (score > bestScore) bestCard = card;
    }

    if (bestCard != null && HasEmptySlot())
    {
        BuyAndPlace(bestCard);       // ← mua xong nhưng shop vẫn giữ nguyên
        bought = true;
    }
}
```

**Root cause:** Sau mỗi lần mua, `bestCard` không bị xóa khỏi `shop`. Vòng lặp tiếp theo chọn lại đúng thẻ đó và mua tiếp.

**Hậu quả:** Bot điền toàn board 7 slot bằng **1 loại thẻ duy nhất** (thẻ có điểm cao nhất đủ tiền mua). Quá trình huấn luyện GA hoàn toàn bị lệch vì bot trong simulation chơi khác hẳn real game.

**Fix:**
```csharp
// Tạo local copy để không mutate tham số gốc
List<CardDefinition> availableShop = new List<CardDefinition>(shop);

bool bought = true;
while (bought)
{
    bought = false;
    CardDefinition bestCard = null;
    float bestScore = -1f;

    foreach (var card in availableShop)
    {
        if (card.cost > economy.CurrentCoin) continue;
        float score = Evaluate(card);
        if (score > bestScore) { bestScore = score; bestCard = card; }
    }

    if (bestCard != null && HasEmptySlot())
    {
        BuyAndPlace(bestCard);
        availableShop.Remove(bestCard);   // ← tiêu thụ slot shop
        bought = true;
    }
}
```

---

### BUG-AI-02: Spell cards lọt vào shop simulation → dead unit ngay từ đầu trận

**File:** `GameSimulator.cs` — `SimulateMatch`

**Root cause:** `CardDatabase.GetRandomShop` gọi `GetAllCards()` trả về cả Unit lẫn Spell. Bot không phân biệt, dùng `BuyAndPlace` đặt spell thẳng lên board.

Spell có `baseHP = 0` → `CardInstance.currentHP = 0` → `IsDead = true` ngay khi tạo.

**Chuỗi hậu quả trong `CombatResolver`:**
1. `ScanAllBoardsForNewDeaths` phát hiện spell chết từ đầu trận
2. `FlushDeathStack` trigger `OnDeath` của spell (nếu có) → có thể spawn unit, deal damage, v.v.
3. `OnAllyDeath` broadcast đến các đồng đội cùng tribe → noise
4. Board bị ô nhiễm trước khi trận thực sự bắt đầu

**Fix:**
```csharp
// GameSimulator.cs
botA.DecidePrepPhase(CardDatabase.Instance.GetRandomShop(5, currentTier));
//                                          ↑ đổi method thành:

// Thêm method vào CardDatabase:
public List<CardDefinition> GetRandomUnitShop(int count, int shopLevel) {
    // giống GetRandomShop nhưng chỉ lấy từ unitList
}
```

Hoặc đơn giản hơn trong `BotAgent.Evaluate`:
```csharp
private float Evaluate(CardDefinition c)
{
    if (c.cardType == CardType.Spell) return -1f;  // ← bỏ qua spell
    ...
}
```

---

### BUG-AI-03: Board và HP không được reset giữa các lượt — 3 lỗi phát sinh

**File:** `GameSimulator.cs` — `SimulateMatch`

Trong real game, sau mỗi battle `EndCombatAndPrepareNextTurn` thực hiện:
1. Xóa toàn board của bot, reset về `null` (xem `SummonEnemyTeam`)
2. Gọi `unit.ResetStats()` cho mọi unit gốc
3. Loại bỏ `isBattleSpawned` units

Trong simulation, không có bước nào trong số này.

**Lỗi phát sinh A — HP không phục hồi:**
Unit sống sót qua trận với HP thấp, tiếp tục chiến đấu với HP thấp đó mãi mãi. Mỗi lượt combat tiếp theo chúng yếu đi dần cho đến khi chết trong round 1.

**Lỗi phát sinh B — `abilityTriggerCounts` không reset:**
```csharp
// CardInstance.ResetStats() — KHÔNG được gọi trong simulation
abilityTriggerCounts = new List<int>(new int[abCount]);
```
Ability có `triggerLimit = 2` đã dùng hết từ lượt trước → lượt sau không bao giờ kích hoạt nữa.
Ability có `isEscalating = true` tăng mãi qua nhiều lượt → damage/buff bất thường.
`consumedCardIDs` của Sekhmet không bị xóa → `SummonConsumed` triệu hồi unit từ nhiều lượt trước.

**Lỗi phát sinh C — `isBattleSpawned` units tích tụ:**
```
Lượt 1: Bot mua 3 thẻ. Trận: 2 unit được summon thêm (isBattleSpawned=true) → board: 5 occupied
Lượt 2: Bot chỉ mua được 2 thẻ nữa (2 slot null) → board: 7 occupied
Lượt 3: Bot không mua được gì (board full) → economy không có ý nghĩa
```

Unit summoned lấp đầy board qua các lượt, bot dần mất khả năng mua bài mới.

**Fix:**
```csharp
// GameSimulator.SimulateMatch — sau ResolveTurn:
resolver.ResolveTurn(botA.board, botB.board, new TurnRecord());

// Reset bot boards sau mỗi trận:
ResetBotBoardAfterCombat(botA.board);
ResetBotBoardAfterCombat(botB.board);

// ---

private void ResetBotBoardAfterCombat(List<CardInstance> board)
{
    for (int i = 0; i < board.Count; i++)
    {
        var unit = board[i];
        if (unit == null) continue;
        if (unit.isBattleSpawned)
        {
            board[i] = null;        // xóa unit sinh ra trong combat
        }
        else if (!unit.IsDead)
        {
            unit.ResetStats();      // phục hồi HP, reset triggerCounts, v.v.
        }
        else
        {
            board[i] = null;        // xóa unit đã chết
        }
    }
}
```

---

## 🟡 MEDIUM — Logic sai làm lệch kết quả huấn luyện

### BUG-AI-04: `hasTaunt` và `hasReborn` bị gate sai trong hàm Evaluate

**File:** `BotAgent.cs` — `Evaluate`

**Code lỗi:**
```csharp
if (c.abilities != null && c.abilities.Count > 0)  // ← gate sai
{
    if (c.hasTaunt)  s += 8  * brain.genes[3];  // không chạy nếu abilities rỗng
    if (c.hasReborn) s += 10 * brain.genes[5];  // không chạy nếu abilities rỗng
    ...
}
```

`hasTaunt` và `hasReborn` là **passive keywords** trực tiếp trên `CardDefinition`, không liên quan đến `abilities` list. Một thẻ thuần passive (không có abilities) sẽ bị đánh giá thấp hơn giá trị thực.

**Fix:**
```csharp
private float Evaluate(CardDefinition c)
{
    if (c.cardType == CardType.Spell) return -1f;

    float s = c.baseATK * brain.genes[0] + c.baseHP * brain.genes[1];

    // Passive keywords — độc lập với abilities list
    if (c.hasTaunt)  s += 8  * brain.genes[3];
    if (c.hasReborn) s += 10 * brain.genes[5];
    if (c.hasSafeguard) s += 6 * brain.genes[3]; // (nếu muốn đánh giá Safeguard)

    // Abilities
    if (c.abilities != null)
    {
        foreach (var a in c.abilities)
        {
            if (a == null) continue;
            if (a.trigger == TriggerType.OnTakeDamage && a.effect == EffectType.DealDamage)
                s += 8 * brain.genes[3];
            if (a.trigger == TriggerType.StartOfBattle && a.effect == EffectType.AddStats)
                s += 12 * brain.genes[4];
            if (a.trigger == TriggerType.OnDeath && a.effect == EffectType.AddStats)
                s += 10 * brain.genes[2];
        }
    }
    return s;
}
```

---

### BUG-AI-05: Crossover chỉ dùng elites — parentA có thể bằng parentB

**File:** `GATrainer.cs` — `CrossoverAndMutate` + vòng `while (nextGen.Count < populationSize)`

**Code lỗi:**
```csharp
List<Chromosome> nextGen = population.Take(populationSize / 10).ToList(); // 5 elites

while (nextGen.Count < populationSize)
{
    Chromosome parentA = nextGen[Random.Range(0, nextGen.Count)]; // pool = 5 elites
    Chromosome parentB = nextGen[Random.Range(0, nextGen.Count)]; // có thể trùng A
    nextGen.Add(CrossoverAndMutate(parentA, parentB));
}
```

Vấn đề 1: `Random.Range(0, 5)` hai lần → P(A==B) = 20% mỗi cặp. Khi A==B, uniform crossover tạo ra clone hoàn hảo của cha/mẹ, chỉ mutation tạo ra khác biệt.

Vấn đề 2: Sau khi `nextGen` có thêm con cái, `Random.Range(0, nextGen.Count)` bắt đầu chọn cả con làm cha mẹ cho các con tiếp theo — không có ranh giới thế hệ rõ ràng.

**Hậu quả:** Quần thể converge sớm vào local optimum, mất genetic diversity từ gen 15-20.

**Fix:**
```csharp
List<Chromosome> elites = population.Take(populationSize / 10).ToList();
List<Chromosome> nextGen = new List<Chromosome>(elites.Select(e => e.Clone()));

while (nextGen.Count < populationSize)
{
    // Lấy 2 cha mẹ khác nhau từ elites
    int idxA = Random.Range(0, elites.Count);
    int idxB;
    do { idxB = Random.Range(0, elites.Count); } while (idxB == idxA && elites.Count > 1);

    nextGen.Add(CrossoverAndMutate(elites[idxA], elites[idxB]));
}
```

---

### BUG-AI-06: Shop size không nhất quán giữa simulation và real game

**File:** `GameSimulator.cs` vs `GameManager.Board.cs`

| Nơi dùng | GetRandomShop(count, ...) |
|----------|--------------------------|
| `SimulateMatch` | `count = 3` |
| `SummonEnemyTeam` (real game) | `count = 5` |

Bot được huấn luyện để tối ưu chiến lược với **3 lựa chọn** mỗi lượt, nhưng khi chơi thật nó có **5 lựa chọn**. Với 5 lựa chọn, có nhiều khả năng được thẻ diversity hơn, và chiến lược tối ưu có thể khác.

**Fix:**
```csharp
// GameSimulator.cs
botA.DecidePrepPhase(CardDatabase.Instance.GetRandomShop(5, currentTier)); // ← 3 → 5
botB.DecidePrepPhase(CardDatabase.Instance.GetRandomShop(5, currentTier)); // ← 3 → 5
```

---

## 🟢 MINOR — Lãng phí hoặc code smell

### BUG-AI-07: `new Chromosome()` lãng phí random init trong Clone() và CrossoverAndMutate

**File:** `Chromosome.cs:Clone`, `GATrainer.cs:CrossoverAndMutate`

```csharp
public Chromosome Clone()
{
    Chromosome copy = new Chromosome();  // ← tạo 8 random genes...
    for (int i = 0; i < 8; i++) copy.genes[i] = this.genes[i]; // ← ghi đè hết
    copy.fitness = this.fitness;
    return copy;
}
```

`new Chromosome()` kích hoạt `_rng.NextDouble()` 8 lần, rồi lập tức bị ghi đè. Không ảnh hưởng kết quả nhưng là code smell và lãng phí RNG state.

**Fix:**
```csharp
public Chromosome Clone()
{
    var copy = new Chromosome(skipRandomInit: true); // thêm overload
    System.Array.Copy(this.genes, copy.genes, 8);
    copy.fitness = this.fitness;
    return copy;
}

// Hoặc đơn giản dùng array copy trực tiếp mà không cần overload constructor
```

---

### BUG-AI-08: `AIManager.GetBrain` crash nếu chưa có AI_Library.json

**File:** `AIManager.cs` — `GetBrain`

```csharp
public Chromosome GetBrain(string difficulty)
{
    switch (difficulty)
    {
        case "Easy":   return loadedLibrary.easyBot;   // NullReferenceException nếu loadedLibrary == null
        case "Medium": return loadedLibrary.mediumBot;
        default:       return loadedLibrary.hardBot;
    }
}
```

Nếu `AI_Library.json` chưa tồn tại (chưa train lần nào), `loadedLibrary = null`, `GetBrain` crash.

**Fix:**
```csharp
public Chromosome GetBrain(string difficulty)
{
    if (loadedLibrary == null)
    {
        Debug.LogWarning("[AI] Chưa có AI Library — dùng chromosome ngẫu nhiên.");
        return new Chromosome();
    }
    return difficulty switch
    {
        "Easy"   => loadedLibrary.easyBot   ?? new Chromosome(),
        "Medium" => loadedLibrary.mediumBot ?? new Chromosome(),
        _        => loadedLibrary.hardBot   ?? new Chromosome(),
    };
}
```

---

## Bảng tổng kết

| ID | File | Mức độ | Tóm tắt | Ảnh hưởng |
|----|------|--------|---------|-----------|
| AI-01 | `BotAgent.cs` | 🔴 Critical | Shop không bị tiêu sau mua | Board đầy 7 thẻ giống nhau |
| AI-02 | `GameSimulator.cs` | 🔴 Critical | Spell lọt vào board simulation | Combat bị nhiễu từ đầu trận |
| AI-03 | `GameSimulator.cs` | 🔴 Critical | Board/HP không reset giữa lượt | Simulation không trung thực với game |
| AI-04 | `BotAgent.cs` | 🟡 Medium | `hasTaunt`/`hasReborn` gate sai | Bot đánh giá thấp passive cards |
| AI-05 | `GATrainer.cs` | 🟡 Medium | Crossover parentA == parentB | Population converge sớm |
| AI-06 | `GameSimulator.cs` | 🟡 Medium | Shop size 3 vs 5 trong real game | Chiến lược train ≠ chiến lược real |
| AI-07 | `Chromosome.cs` | 🟢 Minor | `new Chromosome()` lãng phí RNG | Code smell |
| AI-08 | `AIManager.cs` | 🟢 Minor | `GetBrain` NullRef nếu chưa train | Crash khi chạy lần đầu |

**Thứ tự ưu tiên sửa:** AI-01 → AI-03 → AI-02 → AI-04 → AI-06 → AI-05 → AI-08 → AI-07
