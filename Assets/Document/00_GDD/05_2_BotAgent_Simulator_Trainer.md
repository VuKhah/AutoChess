## 5.3 BotAgent — Bộ Não Quyết Định

### 5.3.1 Vai Trò Và Thiết Kế Tổng Thể

`BotAgent` là lớp dịch ngôn ngữ của chromosome sang hành động trong game. Nếu chromosome là một tập trọng số trừu tượng, BotAgent là engine thực thi — nó biết *cách chơi game* và dùng trọng số từ chromosome để quyết định chơi *như thế nào*. Đây là điểm giao nhau giữa GA và game logic, và cũng là lớp đòi hỏi thiết kế cẩn thận nhất: một BotAgent kém không phản ánh được ý nghĩa thực sự của chromosome, khiến quá trình tiến hóa không còn chọn lọc đúng hướng.

Toàn bộ hành vi của bot trong một lượt chuẩn bị được gói trong một hàm duy nhất:

```csharp
public void DecidePrepPhase(List<CardDefinition> unitShop,
                            List<CardDefinition> spellShop,
                            int shopTier)
```

Hàm này được gọi một lần mỗi lượt bởi `GameSimulator`, không cần tương tác người dùng, không cần coroutine, không cần Unity API. Bên trong, nó thực hiện tuần tự **bảy phase**:

```
1. RerollPhase      — quyết định có trả coin để xem shop mới không
2. BuyUnitsPhase    — mua unit tốt nhất có thể với coin còn lại
3. BuySpellsPhase   — mua và dùng spell nếu đủ tốt
4. ProactiveSellPhase — bán unit kém để giải phóng slot và coin
5. TryMerge         — ghép 3 bản sao thành unit cấp cao hơn
6. RepositionPhase  — sắp xếp lại unit vào đúng vị trí (frontline/backline)
7. FreezePhase      — quyết định giữ nguyên shop cho lượt sau
```

Thứ tự bảy phase không phải ngẫu nhiên — nó phản ánh thứ tự ưu tiên logic: reroll trước khi mua (để có cơ hội mua tốt hơn), mua trước khi bán (để biết có cần slot không), merge sau khi mua (có thể vừa mua đủ bộ), sắp xếp sau khi merge (vì merge thay đổi vị trí), freeze sau cùng (biết đã mua gì trước khi quyết định giữ shop).

> **[HÌNH 5.4 — Flowchart 7-Phase DecidePrepPhase]** *Sơ đồ luồng 7 bước từ trên xuống: RerollPhase → BuyUnitsPhase → BuySpellsPhase → ProactiveSellPhase → TryMerge → RepositionPhase → FreezePhase. Mỗi phase một hộp màu, kèm điều kiện kích hoạt (gene threshold) và output chính.*

---

### 5.3.2 Hàm Đánh Giá Card — Cầu Nối Chromosome Và Game State

Trước khi đi vào từng phase, cần hiểu hàm `Evaluate(CardDefinition c)` — hàm trả về điểm số của một card định nghĩa, được gọi trong hầu hết mọi quyết định mua. Đây là công thức cốt lõi kết nối 37 gene với game state:

```
S(c) = baseATK × genes[0]
     + baseHP  × genes[1]
     + (tier − 1) × genes[2] × 5

     + [hasTaunt]     × genes[4] × 10
     + [hasReborn]    × genes[5] × 12
     + [hasSafeguard] × genes[6] × 8

     + Σ_ability ( TriggerWeight(trigger) × EffectWeight(effect) × 10
                   × context_scale(trigger, sameTribeCount)
                   + [isEscalating] × TW × EW × 3 )

     + sameTribeCount × SynergyWeight(tribe) × 4

     + copies_on_board × genes[21] × (copies == 2 ? 16 : 8)

     + [hasTaunt] × emptyFrontlineSlots × genes[22] × 2

     ÷ cost × (1 + genes[3])
```

Trong đó `context_scale` là hệ số điều chỉnh cho các trigger phụ thuộc đồng minh:

```csharp
// OnAllyDeath/Summon/Reborn chỉ có giá trị khi có đồng minh cùng tộc
if (trigger == OnAllyDeath || OnAllySummon || OnAllyReborn)
    tw *= Clamp01(sameTribeCount / 2f);
```

Hệ số này giải quyết một vấn đề thiết kế tinh tế: trigger như OnAllyDeath có giá trị gần bằng 0 khi chỉ có một unit trên sân (không có "đồng minh" để chết và trigger). Bot cần học rằng card với OnAllyDeath chỉ mạnh trong đội hình đông — và hệ số `sameTribeCount / 2` phản ánh đúng điều đó mà không cần thêm gene mới.

Công thức kết thúc bằng `÷ cost × (1 + genes[3])` — chuẩn hóa điểm theo chi phí. Điều này quan trọng vì không có nó, bot sẽ luôn ưu tiên unit tier 6 đắt tiền hơn unit tier 1 rẻ hơn dù value/coin thấp hơn. `genes[3]` cho phép GA tinh chỉnh mức độ nhấn mạnh hiệu quả chi phí: bot economy có `genes[3]` cao, bot aggro có `genes[3]` thấp hơn.

---

### 5.3.3 Phase 1 — Reroll: Tìm Kiếm Hay Tiết Kiệm?

```csharp
private void RerollPhase(ref List<CardDefinition> unitShop, ref List<CardDefinition> spellShop)
{
    if (brain.genes[24] < 0.05f) return;   // không bao giờ reroll

    int maxRerolls = Mathf.FloorToInt(brain.genes[25] * 3f) + 1;  // [1..4]
    int keepCoins  = Mathf.FloorToInt(brain.genes[26] * 4f);      // [0..4]

    for (int r = 0; r < maxRerolls; r++)
    {
        if (economy.CurrentCoin < keepCoins + 1) break;

        float shopBest  = BestUnitScore(unitShop);
        float boardBest = board.Max(EvaluateInstance);

        if (shopBest >= brain.genes[24] * boardBest) break;  // shop đủ tốt

        economy.TrySpend(1);
        unitShop  = CardDatabase.Instance.GetRandomUnitShop(5, _shopTier);
        spellShop = CardDatabase.Instance.GetRandomSpellShop(2, _shopTier);
    }
}
```

Ba gene phối hợp tạo ra một chính sách reroll đầy đủ:

- `genes[24]` quyết định *khi nào* reroll (shop phải kém hơn bao nhiêu so với board)
- `genes[25]` quyết định *tối đa bao nhiêu lần* (1–4 lần mỗi lượt)
- `genes[26]` quyết định *phòng thủ tài chính* (không reroll nếu coin còn ít hơn ngưỡng)

Ví dụ minh họa hành vi của hai bot cực đoan:
- **Bot aggressive reroll** (`genes[24]=0.9, genes[25]=1.0, genes[26]=0.0`): reroll đến 4 lần miễn là shop chưa đạt 90% sức mạnh board, không giữ lại coin dự phòng.
- **Bot tiết kiệm** (`genes[24]=0.2, genes[25]=0.0, genes[26]=0.8`): chỉ reroll 1 lần nếu shop đặc biệt tệ (< 20% board), luôn giữ lại ít nhất 3 coin.

---

### 5.3.4 Phase 2 — Mua Unit: Vòng Lặp Greedy Có Ngưỡng

BuyUnitsPhase là vòng lặp greedy: mỗi iteration, bot tìm card tốt nhất trong shop còn có thể mua (đủ coin và chưa bị lấy), mua nó nếu điểm vượt ngưỡng tối thiểu `genes[23] × 3`, rồi lặp lại cho đến khi hết coin hoặc hết card đáng mua.

```
while (còn card đáng mua và còn coin):
    bestCard = argmax Evaluate(c) over shop
    if Evaluate(bestCard) < genes[23] × 3:
        break    // dưới ngưỡng → không mua gì nữa
    if board đầy:
        worstUnit = argmin EvaluateInstance(u) over board
        if Evaluate(bestCard) > EvaluateInstance(worstUnit) × (1.5 + genes[23]):
            bán worstUnit  // trao đổi có lợi rõ ràng
        else:
            break          // không bán → không mua được
    mua bestCard, đặt lên sân
```

Điều kiện trao đổi `Evaluate(new) > worstScore × (1.5 + genes[23])` có hệ số tối thiểu 1.5 (luôn > 1, vì `genes[23] ≥ 0`) — bot không bao giờ bán unit cũ để mua unit mới kém hơn. Hệ số `(1.5 + genes[23])` tạo ra một "ngưỡng lợi nhuận tối thiểu" cho việc trao đổi: bot tiêu chuẩn cao (`genes[23]` lớn) chỉ trao đổi khi lợi ích rõ ràng hơn nhiều.

Khi mua và đặt unit lên sân, bot kích hoạt ngay trigger shop-phase nếu có:

```csharp
board[idx] = newUnit;
FireTrigger(TriggerType.OnDeploy, newUnit);             // hiệu ứng của unit mới
BroadcastAllyTrigger(TriggerType.OnAllyDeploy, newUnit); // đồng minh phản ứng
```

Điều này đảm bảo bot tận dụng đúng các ability OnDeploy ngay khi mua — ví dụ bot mua Ninurta sẽ nhận unit bonus từ OnDeploy của nó ngay lập tức, ảnh hưởng đến các quyết định mua tiếp theo trong cùng lượt.

---

### 5.3.5 Phase 3 — Mua Spell: Hàm Đánh Giá Riêng Biệt

Spell không có ATK/HP nên không thể dùng `Evaluate()` chung. `EvaluateSpell()` tính điểm spell theo từng loại hiệu ứng, nhân với các gene liên quan:

| Loại spell (effect code) | Công thức điểm | Gene chi phối |
|:--:|---|:--:|
| BuffStats (1) | `(val1×genes[0] + val2×genes[1]) × (isPermanent ? 2.5 : 1)` | [0], [1] |
| GainCoin (6) | `val × genes[16] × genes[31]` | [16], [31] |
| GetRandomUnit (10) | `genes[2] × 6` | [2] |
| StealFromShop (11) | `genes[2] × 7` | [2] |
| GainIncome permanent (12) | `val × genes[16] × genes[31] × 12` | [16], [31] |
| UpgradeTierUnit (17) | `genes[2] × 12` | [2] |
| GetUnitAtNextTier (22) | `genes[2] × (7 + shopTier × 0.4)` | [2] |
| GiveDoubleAtkAndSafeguard (18) | `genes[0]×8 + genes[6]×8` | [0], [6] |
| ToggleTaunt (19) | `genes[4] × 6` | [4] |
| GiveEndTurnBuff (21) | `(val1×genes[0]+val2×genes[1]) × genes[11] × genes[31] × 3` | [0],[1],[11],[31] |
| LoseLife (14) | `-25` (phạt cứng) | — |
| TransferStats (15) | `-8` (phạt cứng) | — |

Điểm cuối cùng được chuẩn hóa theo cost: `score / spell.cost × (1 + genes[3])`. Bot chỉ mua spell nếu điểm chuẩn hóa này vượt ngưỡng `genes[28] × 3`.

Hai điều đáng chú ý trong bảng trên:

**Hệ số 12 cho GainIncome permanent:** Một spell "+1 coin/lượt vĩnh viễn" được đánh giá cao hơn 12 lần so với "+1 coin ngay lập tức". Với 20 lượt tối đa, mỗi coin/lượt thêm tích lũy lên đến 20 coin — hệ số 12 thực ra còn thận trọng. Bot có cả `genes[16]` và `genes[31]` cao sẽ rất tích cực mua spell kinh tế permanent.

**GiveEndTurnBuff (21) — spell phức tạp nhất:** Spell này thêm một ability EndTurnShop vào unit được chọn. Điểm phụ thuộc vào ba gene cùng lúc: `genes[0]`/`genes[1]` (giá trị stat được buff), `genes[11]` (coi trọng EndTurnShop trigger), và `genes[31]` (economy weight). Chỉ bot có cả ba gene này ở mức trung bình hoặc cao mới thực sự muốn mua spell này — thể hiện tính nhất quán của chromosome: bot không thể "ngẫu nhiên" thích spell EndTurnBuff nếu nó không cũng có xu hướng xây dội hình dài hạn.

---

### 5.3.6 Phase 4 — Proactive Sell: Dọn Dẹp Chủ Động

```csharp
private void ProactiveSellPhase()
{
    if (brain.genes[27] < 0.05f) return;   // không bao giờ bán chủ động
    float sellBelow = brain.genes[27] * 3f;

    for (int i = 0; i < board.Count; i++)
    {
        var unit = board[i];
        if (unit == null || unit.IsDead || unit.isBattleSpawned) continue;
        if (EvaluateInstance(unit) < sellBelow)
        {
            FireTrigger(TriggerType.OnSell, unit);
            BroadcastAllyTrigger(TriggerType.OnAllySell, unit);
            board[i] = null;
            economy.Sell();   // +1 coin
        }
    }
}
```

Phase này phân chia hai trường phái chiến lược rõ nét. Bot có `genes[27] = 0` không bao giờ bán chủ động — giữ mọi unit dù chúng kém, ưu tiên đội hình đầy đủ cho combat. Bot có `genes[27]` cao liên tục "làm mới" đội hình: bán unit cũ lấy coin để mua tốt hơn lượt sau, chấp nhận có lúc ra trận với ít unit hơn đổi lấy chất lượng tốt hơn dài hạn.

Flag `unit.isBattleSpawned` bảo vệ các unit được triệu hồi tạm thời trong combat (spell case 10) không bị bán — chúng sẽ tự biến mất sau combat, bán chúng là vô nghĩa và có thể phá hỏng logic game.

Quan trọng: trước khi xóa unit khỏi board, bot kích hoạt đúng trigger — `FireTrigger(OnSell)` cho unit bị bán và `BroadcastAllyTrigger(OnAllySell)` cho các đồng minh. Điều này đảm bảo bot mô phỏng đúng hành vi game thực: ví dụ Ashur (Babylon, OnAllySell → buff tất cả đồng minh) sẽ kích hoạt ngay cả khi bot tự bán unit của mình trong ProactiveSell.

---

### 5.3.7 Phase 5 — Merge: Logic Ghép Bài

`TryMerge` scan toàn board tìm bộ đủ bản sao (3 bản sao lv0 để thành lv1, 2 bản sao lv1 để thành lv2) và merge ngay lập tức:

```csharp
// Tìm unit chứa nhiều bonus nhất để giữ lại (unit "tốt nhất" trong bộ)
int keepIdx = copies.MaxBy(idx =>
    board[idx].permanentATKBonus + board[idx].permanentHPBonus +
    board[idx].growthATKBonus    + board[idx].growthHPBonus);

board[keepIdx].mergeLevel++;
board[keepIdx].ResetStats();   // tính lại chỉ số với tier mới
// xóa các bản sao còn lại
```

Việc giữ unit có *tổng bonus lớn nhất* (không phải base stat lớn nhất) đảm bảo không lãng phí các buff tích lũy từ các lượt trước. Nếu một bản sao đã nhận buff từ spell hay synergy trong khi bản sao kia thì chưa, bot sẽ giữ bản sao đã được buff.

Sau mỗi merge, bot gọi `SimulateMergeReward()` — đặt một unit ngẫu nhiên tier+1 lên board (mô phỏng spell "Tinh Hoa Hợp Nhất" tự động trigger khi merge). Đây là chi tiết gameplay quan trọng để game simulation khớp chính xác với gameplay thực.

---

### 5.3.8 Phase 6 — Reposition: Sắp Xếp Chiến Thuật

```csharp
private void RepositionPhase()
{
    var units = board.Where(u => u != null && !u.IsDead).ToList();

    units = units.OrderByDescending(u =>
        u.isTaunt
        ? brain.genes[22] * 20f + PositionScore(u)   // Taunt → frontline
        : PositionScore(u)
    ).ToList();

    // Đặt vào board: slot 0-3 (frontline) trước, rồi slot 4-6 (backline)
    int frontSlot = 0, backSlot = FrontlineCount;
    foreach (var unit in units)
    {
        if (frontSlot < FrontlineCount) board[frontSlot++] = unit;
        else board[backSlot++] = unit;
    }
}
```

`PositionScore` không chỉ dựa vào `EvaluateInstance` mà còn điều chỉnh theo ability của unit:

```csharp
private float PositionScore(CardInstance unit)
{
    float score = EvaluateInstance(unit);
    foreach (var ability in unit.Data.abilities)
    {
        switch (ability.trigger)
        {
            // Cần sống lâu → backline (giảm điểm frontline)
            case TriggerType.Aura:
            case TriggerType.OnAllyDeath:
            case TriggerType.OnAllyReborn:
            case TriggerType.OnAllySummon:
            case TriggerType.EndTurnShop:
                score -= brain.genes[17] * 15f;
                break;
            // Deathrattle → muốn chết sớm → frontline (tăng điểm)
            case TriggerType.OnDeath:
                score += brain.genes[8] * 5f;
                break;
        }
    }
    return score;
}
```

Hai gene chi phối vị trí theo hai hướng đối lập: `genes[17]` (eGiveBuff) đẩy unit support về backline (giảm điểm frontline), `genes[8]` (tOnDeath) kéo unit deathrattle lên frontline (tăng điểm frontline). Bot có cả hai gene này cao sẽ tự nhiên sắp xếp: deathrattle unit ở trước, support unit ở sau — một cấu hình chiến thuật hợp lý mà không cần rule cứng.

---

### 5.3.9 Phase 7 — Freeze: Đặt Cọc Shop

```csharp
private void FreezePhase(List<CardDefinition> unitShop, List<CardDefinition> spellShop)
{
    float freezeTendency = 1f - brain.genes[24];   // ít reroll → nhiều freeze
    if (freezeTendency < 0.35f) return;            // reroll aggressive → không freeze

    float saveThreshold = brain.genes[23] * 3f;
    bool wantedButCantAfford = unitShop.Exists(c =>
        c.cost > economy.CurrentCoin && Evaluate(c) >= saveThreshold);

    if (wantedButCantAfford)
    {
        isShopFrozen = true;
        frozenUnitShop  = new List<CardDefinition>(unitShop);
        frozenSpellShop = spellShop != null ? new List<CardDefinition>(spellShop) : null;
    }
}
```

Hai điều kiện phải đồng thời thỏa mãn để freeze:
1. Bot có xu hướng freeze (`1 - genes[24] ≥ 0.35`, tức là không phải bot aggressive reroll)
2. Có card đáng mua trong shop nhưng hiện chưa đủ tiền mua lượt này

Tính nhất quán gene quan trọng ở đây: `genes[24]` kiểm soát cả *reroll aggressiveness* lẫn *freeze tendency* theo hướng đối lập — bot thích reroll (genes[24] cao) thì không thích freeze, và ngược lại. Đây là tính nhất quán chiến lược: không thể vừa aggressive reroll vừa cố giữ shop, hai hành vi này mâu thuẫn nhau và chromosome phản ánh đúng điều đó.

---

## 5.4 GameSimulator — Môi Trường Huấn Luyện

### 5.4.1 Vai Trò Và Thiết Kế

`GameSimulator` là cầu nối giữa BotAgent và GATrainer: nó cung cấp môi trường để đánh giá một chromosome *thực sự tốt đến đâu* bằng cách cho bot chơi các trận đấu hoàn chỉnh. Không có GameSimulator, GATrainer không có cơ sở để so sánh chromosome — mọi gene đều có giá trị như nhau.

Yêu cầu thiết kế quan trọng nhất là **tính nhất quán với gameplay thực**: GameSimulator phải dùng đúng cùng `CombatResolver`, đúng cùng `CardDatabase`, đúng cùng logic shop-tier progression như game thật. Nếu có bất kỳ sự khác biệt nào giữa simulation và game thật, chromosome được training sẽ học chiến lược cho một game *khác* — không phải game người chơi đang chơi.

Thiết kế plain C# (không kế thừa MonoBehaviour) cho phép `GameSimulator` được khởi tạo và gọi tự do từ GATrainer mà không cần scene:

```csharp
public class GameSimulator     // plain C# — OK
{
    private CombatResolver resolver = new CombatResolver();   // plain C#
    private const int StartingHP = 7;
    private const int MaxTurns   = 20;

    public MatchResult EvaluateMatch(BotAgent botA, BotAgent botB) { ... }
}
```

---

### 5.4.2 Vòng Lặp 20 Lượt

```csharp
public MatchResult EvaluateMatch(BotAgent botA, BotAgent botB)
{
    int hpA = StartingHP, hpB = StartingHP;   // mỗi bot bắt đầu với 7 HP

    for (int i = 0; i < MaxTurns; i++)
    {
        int shopTier = Mathf.Clamp((i + 1 + 1) / 2, 1, 6);

        // Lấy shop — ưu tiên shop đã frozen từ lượt trước
        var unitShopA  = (botA.isShopFrozen && botA.frozenUnitShop  != null)
            ? botA.frozenUnitShop  : CardDatabase.Instance.GetRandomUnitShop(5, shopTier);
        // ... tương tự cho spellShopA, unitShopB, spellShopB

        botA.DecidePrepPhase(unitShopA, spellShopA, shopTier);
        botB.DecidePrepPhase(unitShopB, spellShopB, shopTier);

        resolver.ResolveTurn(botA.board, botB.board, new TurnRecord());

        bool aAlive = botA.board.Exists(u => u != null && !u.IsDead);
        bool bAlive = botB.board.Exists(u => u != null && !u.IsDead);

        if (!aAlive && bAlive) hpA--;   // A thua lượt này
        else if (aAlive && !bAlive) hpB--;   // B thua

        if (hpA <= 0 || hpB <= 0) break;   // kết thúc sớm

        botA.EndCombatPhase();   // reset board, xóa unit chết
        botB.EndCombatPhase();
        botA.TriggerEndTurnShop();   // áp buff EndTurnShop cho lượt tiếp
        botB.TriggerEndTurnShop();
    }

    int result = (hpA > hpB) ? 1 : (hpB > hpA) ? -1 : 0;
    return new MatchResult { result, hpA, hpB, turns=turnsPlayed, scoreA=ScoreFromA(...) };
}
```

Một số điểm kỹ thuật đáng chú ý:

**Shop Tier tự động:** `shopTier = clamp((turn+1+1)/2, 1, 6)` nghĩa là lượt 1 → tier 1, lượt 3 → tier 2, lượt 5 → tier 3... Cả hai bot đối đầu ở cùng shop tier, đảm bảo training công bằng — không bot nào có lợi thế truy cập unit mạnh hơn.

**Freeze shop được tôn trọng:** Nếu bot đã freeze shop lượt trước (`isShopFrozen = true`), GameSimulator dùng shop đã lưu thay vì tạo shop mới. Điều này đảm bảo hành vi freeze của bot có hiệu lực thực sự trong simulation — nếu không, gene[24]/freeze strategy sẽ không có tác dụng gì và GA không có lý do để tiến hóa hành vi này.

**EndCombatPhase và TriggerEndTurnShop:** Sau mỗi combat, bot reset board (loại bỏ unit chết, reset chỉ số về base) rồi apply EndTurnShop buffs. Thứ tự này quan trọng: EndTurnShop buff được apply *sau* khi combat đã kết thúc và board được reset, đảm bảo buff tích lũy đúng cho lượt tiếp theo.

---

### 5.4.3 Hàm Fitness — Đo Lường Chất Lượng Chiến Lược

```csharp
private static float ScoreFromA(int result, int hpA, int hpB, int turns)
{
    float score = result > 0 ? 120f   // thắng
                : result == 0 ? 70f   // hòa
                : 35f;                // thua

    score += hpA * 6f;    // thưởng HP còn lại của A
    score -= hpB * 3f;    // phạt HP còn lại của B

    if (result > 0)
        score += (MaxTurns - turns) * 2f;  // thưởng tốc độ thắng

    return Mathf.Max(1f, score);
}
```

Ba thành phần của công thức này encode ba khía cạnh khác nhau của "chơi tốt":

**Thành phần 1 — Kết quả nhị phân (120/70/35):** Thắng vẫn là mục tiêu chính. Điểm hòa (70) cao hơn thua (35) nhưng thấp hơn thắng (120) — phản ánh đúng giá trị thực: hòa tốt hơn thua nhưng vẫn cần thắng để tiến bộ.

**Thành phần 2 — Biên thắng (`hpA×6 − hpB×3`):** Bot thắng với HP còn nhiều (hpA cao) được thưởng thêm; bot thua nhưng đã hạ HP đối thủ (hpB thấp) cũng được thưởng bù. Hệ số không cân xứng (6 vs. 3): HP của bản thân quan trọng hơn HP đối thủ — khuyến khích bot giữ mình sống, không chỉ tấn công.

**Thành phần 3 — Tốc độ thắng (`(20−turns)×2`):** Thắng ở lượt 5 được thêm 30 điểm so với thắng ở lượt 19. Thành phần này ngầm chọn lọc chiến lược *tempo* — áp lực sớm và kết thúc nhanh thay vì kéo dài đến late game. Qua nhiều thế hệ, áp lực fitness này "dạy" chromosome cân bằng tempo và economy mà không cần chỉ định cụ thể.

Phạm vi điểm thực tế: thắng sớm với HP cao có thể đạt `120 + 7×6 + (20-1)×2 = 120 + 42 + 38 = 200`; thua sớm với HP thấp có thể xuống tới `35 + 0 - 7×3 = 35 - 21 = 14`. Phạm vi 14–200 đủ rộng để phân biệt các chiến lược, không bị flatten.

> **[HÌNH 5.5 — Hàm Fitness ScoreFromA]** *Biểu đồ thanh minh họa điểm của ba kịch bản: Thắng sớm (HP cao, ít lượt), thắng muộn (HP vừa, nhiều lượt), thua sát (HP gần 0, địch HP gần 0). Mỗi thanh chia thành 3 phần màu: kết quả nhị phân, biên thắng, tốc độ.*

---

## 5.5 GATrainer — Vòng Lặp Tiến Hóa

### 5.5.1 Tham Số Huấn Luyện

GATrainer có hai bộ tham số: *quick mode* để kiểm tra nhanh và *production mode* để training thực sự:

| Tham số | Quick Mode | Production Mode | Ý nghĩa |
|---------|:----------:|:---------------:|---------|
| `populationSize` | 30 | 120 | Số chromosome trong quần thể |
| `generations` | 40 | 180 | Số thế hệ tối đa |
| `matchesPerChrom` | 5 | 20 | Số trận đấu mỗi chromosome/thế hệ |
| `mutationRate` | 0.10 | 0.10 | Xác suất mutation mỗi gene (ban đầu) |
| `mutationMag` | 0.12 | 0.12 | Biên độ Gaussian mutation (σ) |
| `immigrantRate` | 0.12 | 0.12 | Tỉ lệ chromosome mới mỗi thế hệ |
| `minLibraryDistance` | 0.18 | 0.18 | Khoảng cách Euclidean tối thiểu giữa các specialist bot |

Quick mode hoàn thành trong ~2 phút, dùng để kiểm tra logic và debug. Production mode (~20–30 phút) cho ra kết quả training đủ tốt để tích hợp vào game.

---

### 5.5.2 Khởi Tạo Quần Thể — 5 Sub-Population Seeded

Thay vì khởi tạo ngẫu nhiên hoàn toàn, quần thể được chia thành 5 nhóm bằng nhau (mỗi nhóm `populationSize / 5 = 24` cá thể trong production), mỗi nhóm được "đinh hướng" bằng seed gene:

```csharp
switch (group)
{
    case 0: // Babylon seed
        c.genes[18] = Random.Range(0.7f, 1.0f);   // sBabylon cao
        c.genes[19] = Random.Range(0.0f, 0.3f);   // sOlympus thấp
        c.genes[20] = Random.Range(0.0f, 0.3f);   // sNiles thấp
        break;
    case 1: // Niles seed
        c.genes[20] = Random.Range(0.7f, 1.0f);   // sNiles cao
        c.genes[18] = Random.Range(0.0f, 0.3f);
        c.genes[19] = Random.Range(0.0f, 0.3f);
        break;
    case 2: // Summoner seed
        c.genes[14] = Random.Range(0.75f, 1.0f);  // eSummon
        c.genes[5]  = Random.Range(0.70f, 1.0f);  // wReborn
        c.genes[8]  = Random.Range(0.65f, 1.0f);  // tOnDeath
        c.genes[34] = Random.Range(0.65f, 1.0f);  // tOnAllyGroup
        c.genes[27] = Random.Range(0.00f, 0.15f); // wProactiveSell thấp
        break;
    case 3: // Resilient seed
        c.genes[1]  = Random.Range(0.75f, 1.0f);  // wHP
        c.genes[4]  = Random.Range(0.70f, 1.0f);  // wTaunt
        c.genes[5]  = Random.Range(0.70f, 1.0f);  // wReborn
        c.genes[6]  = Random.Range(0.70f, 1.0f);  // wSafeguard
        c.genes[10] = Random.Range(0.70f, 1.0f);  // tOnTakeDmg
        break;
    case 4: // Random — hoàn toàn ngẫu nhiên
}
```

Cơ chế seeding giải quyết một vấn đề thực tiễn: trong không gian 37 chiều, xác suất để GA *ngẫu nhiên tìm ra* chromosome Summoner — đòi hỏi đồng thời nhiều gene cụ thể ở mức cao — là cực kỳ thấp nếu khởi đầu từ ngẫu nhiên hoàn toàn. Bằng cách "đặt" 20% quần thể vào vùng lân cận của mỗi archetype, GA được dẫn đến các vùng không gian mà nó có thể không bao giờ khám phá được trong thời gian training hạn chế.

Quan trọng: seeding chỉ khởi tạo *một số* gene, phần còn lại vẫn ngẫu nhiên. Điều này để GA tự tìm giá trị tối ưu cho các gene "phụ" của mỗi archetype, thay vì hoàn toàn cứng hóa chromosome từ đầu.

---

### 5.5.3 Vòng Lặp Đánh Giá Fitness

Mỗi thế hệ, mỗi chromosome được đánh giá qua hai tập trận đấu song song:

**Tập 1 — Self-play với quần thể:**
```csharp
for (int m = 0; m < matchesPerChrom; m++)
{
    BotAgent me  = new BotAgent(chromo);
    int oppIdx   = Random.Range(0, population.Count - 1);
    if (oppIdx >= selfIdx) oppIdx++;   // tránh đấu với chính mình
    BotAgent opp = new BotAgent(population[oppIdx]);
    chromo.fitness += sim.EvaluateMatch(me, opp).scoreA;
}
```

Đối thủ được chọn ngẫu nhiên từ quần thể với unbiased shift pattern (tránh self-play). `matchesPerChrom = 20` trận giảm nhiễu từ yếu tố ngẫu nhiên của shop — cùng một chromosome có thể nhận điểm khác nhau ở hai lần chạy khác nhau do shop roll khác nhau, và trung bình 20 trận cho ước lượng fitness ổn định hơn.

**Tập 2 — Benchmark opponents:**
```csharp
foreach (var benchmark in benchmarkOpponents)
{
    MatchResult result = sim.EvaluateMatch(new BotAgent(chromo), new BotAgent(benchmark));
    chromo.fitness += result.scoreA * 0.5f;   // trọng số nhỏ hơn
}
```

`benchmarkOpponents` là 10 chromosome seeded cố định (2 của mỗi archetype), được tạo mới mỗi 30 thế hệ. Mục đích: cung cấp một tập đối thủ ổn định để so sánh fitness qua các thế hệ — không phụ thuộc hoàn toàn vào chất lượng của quần thể hiện tại. Trọng số 0.5× cho phần benchmark đảm bảo self-play (phản ánh khả năng cạnh tranh trong quần thể thực) vẫn là thành phần chính của fitness.

---

### 5.5.4 Chọn Lọc — Tournament Selection Thích Nghi

GATrainer dùng Tournament Selection với *kích thước tournament tăng theo thế hệ*:

```csharp
private static int CurrentTournamentSize(float progress)
{
    if (progress < 0.35f) return 3;   // early: k=3 — áp lực thấp, diversity cao
    if (progress < 0.75f) return 4;   // mid: k=4
    return 5;                          // late: k=5 — áp lực cao, hội tụ nhanh
}
```

Tournament size tăng theo `progress` (0.0 → 1.0 theo thế hệ) tạo ra một lịch trình chọn lọc tự nhiên: đầu training cần diversity cao để khám phá rộng → áp lực thấp (k=3); cuối training cần hội tụ nhanh về nghiệm tốt → áp lực cao (k=5). Đây là kỹ thuật *annealing selection pressure* — tương tự simulated annealing nhưng áp dụng cho áp lực chọn lọc thay vì nhiệt độ.

---

### 5.5.5 Lai Ghép Và Đột Biến Thích Nghi

Mutation rate và mutation magnitude cũng giảm dần theo progress qua hàm SmoothStep:

```csharp
// Giảm từ 10% xuống 3.5% (mutation rate)
float currentMutationRate = Lerp(0.10f, 0.035f, SmoothStep(0f, 1f, progress));

// Giảm từ σ=0.12 xuống σ=0.035 (mutation magnitude)
float currentMutationMag  = Lerp(0.12f, 0.035f, SmoothStep(0f, 1f, progress));
```

`SmoothStep` tạo ra đường cong giảm chậm ở đầu và cuối, nhanh ở giữa — thay vì linear. Kết quả: đầu training khám phá rộng (mutation mạnh), cuối training fine-tune (mutation nhẹ). Đây là lý do fitness trung bình tiếp tục cải thiện dù best fitness đã hội tụ từ rất sớm.

Bên cạnh self-play + crossover/mutate, GATrainer bổ sung một cơ chế *refinement clone* trong giai đoạn mid-late:

```csharp
// progress > 0.35: clone elite với mutation nhẹ hơn nhiều (fine-tuning)
foreach (var elite in population.Take(eliteCount))
    nextGen.Add(MutateClone(elite, mutationRate * 0.65f, mutationMag * 0.45f));
```

Elite chromosome được clone với mutation rate và magnitude giảm thêm 35–55% — tạo ra các "biến thể tinh chỉnh" của các cá thể tốt nhất. Không phải crossover (không trộn với chromosome khác), chỉ là fine-tune quanh nghiệm đang tốt.

---

### 5.5.6 Elitism — Bảo Toàn Đa Tầng

```csharp
int eliteCount = Mathf.Max(3, Mathf.RoundToInt(
    Mathf.Lerp(populationSize / 18f, populationSize / 8f, progress)));
// production: từ ~7 (early) đến ~15 (late) cá thể elite

// 4 tầng elitism:
AddTopClones(nextGen, population, c => true, eliteCount);       // global elite
AddTopClones(nextGen, population, IsBabylon, 2);                // Babylon elite
AddTopClones(nextGen, population, IsNile, 2);                   // Niles elite
AddTopClones(nextGen, population.OrderByDescending(SummonerScore), 2);   // Summoner elite
AddTopClones(nextGen, population.OrderByDescending(ResilientScore), 2);  // Resilient elite
```

Tổng cộng tối thiểu `eliteCount + 8` cá thể được bảo toàn mỗi thế hệ. Bốn tầng cuối đảm bảo mỗi archetype luôn có đại diện tốt nhất được giữ lại — dù fitness tổng của babylonBot có thể không vào top global elite, top-2 Babylon chắc chắn được bảo toàn.

Elitism count tăng theo progress: đầu training bảo toàn ít (~7) để dành chỗ cho crossover đa dạng; cuối training bảo toàn nhiều hơn (~15) vì đã tìm được nhiều nghiệm tốt cần giữ.

---

### 5.5.7 Immigration — Chống Premature Convergence

```csharp
int immigrantCount = Mathf.Max(2, Mathf.RoundToInt(
    populationSize * CurrentImmigrantRate(progress)));
// CurrentImmigrantRate giảm từ 12% xuống 4% theo progress

// Bơm thêm nếu diversity của tribe thấp
if (pctB < 12f) immigrantCount += 2;   // Babylon underrepresented
if (pctN < 12f) immigrantCount += 2;   // Niles underrepresented
if (pctO < 8f)  immigrantCount += 3;   // "Other" quá thấp

// Điền phần còn lại bằng seeded chromosome mới hoàn toàn
while (nextGen.Count < populationSize)
    nextGen.Add(CreateSeededChromosome(Random.Range(0, 5)));
```

Immigrant rate giảm từ 12% xuống 4% theo progress — nhiều immigrant ở đầu (đa dạng hóa), ít dần ở cuối (cho phép hội tụ). Cơ chế kiểm tra diversity theo tribe và bơm thêm immigrant khi cần ngăn một bộ tộc nào đó bị "xóa sổ" khỏi quần thể — đảm bảo babylonBot và nileBot luôn có nguyên liệu di truyền để được chọn cuối training.

---

### 5.5.8 Dừng Sớm — Plateau Detection

```csharp
const int   PLATEAU_PATIENCE = 15;
const float PLATEAU_EPS      = 0.5f;

if (|stdDev_current - stdDev_prev| < PLATEAU_EPS)
    plateauCount++;
else
    plateauCount = 0;

if (plateauCount >= PLATEAU_PATIENCE)
{
    Debug.LogWarning($"Early stop tại gen {g} — plateau {PLATEAU_PATIENCE} gen.");
    break;
}
```

Thay vì dừng theo best fitness (dễ bị "dừng sai" khi best không đổi nhưng avg đang cải thiện), hệ thống theo dõi `std_dev` — độ lệch chuẩn fitness của toàn quần thể. Khi `std_dev` ổn định trong 15 thế hệ liên tiếp (thay đổi < 0.5 điểm), quần thể đã hội tụ và training không còn mang lại cải thiện có ý nghĩa.

---

### 5.5.9 Chọn 5 Bot Cuối — Diversity-Aware Selection

Sau khi training xong, quần thể được sort lần cuối và 5 bot được chọn qua hàm `SelectDistinct`:

```csharp
private Chromosome SelectDistinct(IEnumerable<Chromosome> candidates,
    List<Chromosome> selected, Func<Chromosome, float> score)
{
    var distinct = candidates
        .Where(c => selected.All(s => GeneDistance(c, s) >= 0.18f))
        .OrderByDescending(c => score(c) + DiversityBonus(c, selected) * 100f)
        .FirstOrDefault();
    // ...
}

// Khoảng cách Euclidean chuẩn hóa trong không gian 37 chiều
private static float GeneDistance(Chromosome a, Chromosome b)
    => Mathf.Sqrt(Σ(ai - bi)² / GeneCount);
```

Ưu tiên chọn candidate có `GeneDistance ≥ 0.18` so với tất cả bot đã chọn. Nếu không có candidate đủ xa (quần thể đã hội tụ quá mức), `DiversityBonus` — khoảng cách tối thiểu đến các bot đã chọn × 100 — được cộng thêm vào score, thưởng cho chromosome "khác biệt nhất" ngay cả khi nó không phải candidate tốt nhất về fitness.

Thứ tự chọn:
1. **hardBot** = hallOfFame (chromosome có fitness cao nhất *bao giờ* qua toàn bộ training)
2. **babylonBot** = chromosome Babylon fitness cao nhất, cách xa hardBot
3. **nileBot** = chromosome Niles fitness cao nhất, cách xa hardBot và babylonBot
4. **summonerBot** = chromosome `SummonerScore` cao nhất trong `viable` (fitness ≥ 80% avg)
5. **resilientBot** = chromosome `ResilientScore` cao nhất trong `viable`

---

## 5.6 AILibrary — Lưu Trữ Và Nạp Kết Quả

### 5.6.1 Cấu Trúc JSON

Kết quả training được lưu vào `Assets/Resources/AI_Library.json`:

```json
{
  "hardBot":     { "genes": [0.71, 0.68, ...], "fitness": 4764.0 },
  "babylonBot":  { "genes": [0.12, 0.45, ..., 0.91, 0.04, 0.08, ...], "fitness": 4764.0 },
  "nileBot":     { "genes": [0.38, 0.72, ..., 0.07, 0.03, 0.88, ...], "fitness": 4727.0 },
  "summonerBot": { "genes": [0.20, 0.35, ..., 0.94, 0.85, ...], "fitness": 3892.0 },
  "resilientBot":{ "genes": [0.14, 0.89, 0.31, ...], "fitness": 3645.0 }
}
```

File này được đọc bởi `AIManager.Instance` khi game khởi động và nạp vào runtime. Tính minh bạch của định dạng JSON cho phép kiểm tra thủ công bộ gene — ví dụ, có thể xác nhận babylonBot có `genes[18]` cao nhất trong năm bot chỉ bằng cách mở file.

### 5.6.2 Kiểm Tra Tính Hợp Lệ

```csharp
private bool IsLibraryValid()
{
    TextAsset file = Resources.Load<TextAsset>("AI_Library");
    if (file == null) return false;
    var lib = JsonUtility.FromJson<AILibrary>(file.text);
    return lib?.hardBot?.genes != null && lib.hardBot.genes.Length >= Chromosome.GeneCount;
}
```

Chỉ yêu cầu `hardBot` hợp lệ (có đủ 37 gene). Bốn specialist bot là optional — nếu training không tìm được babylonBot đủ cách xa hardBot, trường `babylonBot` sẽ là null và `AIManager` fallback về hardBot cho slot đó. Điều này đảm bảo game luôn có thể chạy dù training không hoàn hảo.

---

## 5.7 Kết Quả Thực Nghiệm

### 5.7.1 Thiết Lập Thí Nghiệm

Lần training được ghi nhận trong báo cáo này sử dụng production mode trên máy tính cá nhân (Windows 10, CPU 8 nhân):

| Tham số | Giá trị |
|---------|---------|
| Population size | 120 |
| Generations (max) | 180 |
| Matches per chromosome | 20 |
| Mutation rate (ban đầu → cuối) | 10% → 3.5% |
| Mutation magnitude | σ=0.12 → 0.035 |
| Immigrant rate | 12% → 4% |
| Tournament size | k=3 → 4 → 5 |

Tổng số trận đấu simulation tối thiểu: `120 × 20 × 180 = 432.000 trận`. Thực tế cao hơn do benchmark opponents (thêm 10 trận × 120 cá thể mỗi thế hệ).

---

### 5.7.2 Đường Cong Hội Tụ Fitness

Dữ liệu từ CSV log training thể hiện ba giai đoạn rõ ràng:

**Giai đoạn 1 — Khám phá (Gen 0–10): Hội tụ nhanh**

| Gen | Best | Avg | Std Dev | Babylon% | Niles% |
|:---:|:----:|:---:|:-------:|:--------:|:------:|
| 0 | 4727 | 2871 | 952 | 42.5% | 40.8% |
| 6 | **4764** | 3033 | 717 | 34.2% | 32.5% |
| 10 | 4764 | 3084 | 663 | 35.0% | 36.7% |

Best fitness đạt đỉnh 4764 ngay ở thế hệ 6 — chỉ sau 6 vòng lặp, Hall of Fame đã tìm được chromosome tốt nhất. Đây là dấu hiệu rằng không gian gene đủ "lành mạnh" để quá trình chọn lọc sớm xác định được hướng đúng. Trong khi đó, std_dev giảm từ 952 xuống 663 (-30%) — quần thể hội tụ về vùng fitness cao hơn nhưng vẫn còn đa dạng.

**Giai đoạn 2 — Tinh chỉnh (Gen 11–70): Avg tăng ổn định**

Best fitness giữ nguyên 4764 trong suốt giai đoạn này, nhưng avg liên tục tăng từ ~3040 lên ~3030 (dao động quanh 3000–3100). Std_dev tiếp tục giảm chậm từ 663 xuống ~550–600.

Điểm đáng chú ý: **pct_other (non-Babylon, non-Niles) tăng mạnh** ở gen 20–45 (từ 19% lên đến 48%), phản ánh quần thể đang khám phá chiến lược generalist sau khi đã xác định xong hai extreme Babylon và Niles. Ở gen 40–45, Niles chiếm 66–70% quần thể — dấu hiệu Niles có thể có lợi thế tự nhiên trong game.

**Giai đoạn 3 — Ổn định (Gen 71–179): Duy trì diversity**

Avg fitness dao động quanh 2970–3100, std_dev quanh 480–650. Babylon và Niles luôn được duy trì trên 10% nhờ immigrant injection, tránh mất đi nguyên liệu genetic cho specialist selection.

> **[HÌNH 5.6 — Đường Cong Fitness Qua 180 Thế Hệ]** *Biểu đồ đường: trục hoành là thế hệ (0–179), trục tung là điểm fitness. Ba đường: Best (đỏ đậm — plateau tại 4764 từ gen 6), Avg (xanh lam — tăng dần), Worst (xám). Trục phụ bên phải: Std Dev (vàng — giảm dần). Đánh dấu gen 6 (best đạt đỉnh) và vùng 20–45 (pct_other spike).*

---

### 5.7.3 Diversity — Phân Phối Bộ Tộc Qua Các Thế Hệ

> **[HÌNH 5.7 — Phân Phối Tribe Qua Các Thế Hệ]** *Biểu đồ area chart xếp chồng: trục hoành gen 0–179, trục tung 0–100%. Ba vùng màu: Babylon (vàng), Niles (xanh lam), Other (xám). Thể hiện sự dao động nhưng không có bộ tộc nào bị tuyệt chủng — Babylon và Niles luôn > 6%.*

Phân tích phân phối tribe qua CSV data cho thấy:

- **Babylon** dao động 6.7%–53.3%, trung bình ~30%. Không bao giờ xuống 0 nhờ elitism + immigration.
- **Niles** dao động 22.5%–70.0%, trung bình ~43%. Có xu hướng chiếm ưu thế ở mid-game training (gen 35–60), sau đó diversity tái lập.
- **Other** (generalist / Olympus / mixed): dao động 3.3%–68.3%. Giá trị thấp ở gen 33–47 (Niles domination period) là dấu hiệu immigration đã kích hoạt và bổ sung chromosome mới.

Std_dev cuối training (~500–565) so với ban đầu (952) — giảm ~41% — cho thấy quần thể hội tụ có chủ ý mà không bị premature convergence (nếu premature, std_dev sẽ tiến về 0).

---

### 5.7.4 Profile Gene Của 5 Bot Được Chọn

Từ dữ liệu training và theo dõi `best_summoner`, `best_resilient` scores qua các thế hệ:

| Chỉ số | hardBot | babylonBot | nileBot | summonerBot | resilientBot |
|--------|:-------:|:----------:|:-------:|:-----------:|:------------:|
| Fitness cuối | **4764** | **4764** | 4727 | — | — |
| Best score (cumulative) | 4764 | 4764 | 4764 | **8.56** | **6.13** |
| Cải thiện so với gen 0 | +37 | +37 | +0 | +0.79 (10.2%) | +0.54 (9.7%) |

*Lưu ý: SummonerScore và ResilientScore là composite metrics khác với fitness thô, không so sánh trực tiếp.*

hardBot và babylonBot đều đạt fitness 4764 — bằng nhau về điểm tuyệt đối. nileBot đạt 4727, tương đương fitness tốt nhất của quần thể ở thế hệ 0 (không cải thiện thêm về điểm số, nhưng gene profile ngày càng đặc trưng hơn cho archetype Niles). Điều này có nghĩa là **cả ba specialist đều là chromosome xuất sắc**, được phân biệt không phải bởi chất lượng tổng thể mà bởi profile gene khác nhau (GeneDistance ≥ 0.18 giữa các bot). Đây chính là mục tiêu thiết kế: không phải tìm bot tốt nhất duy nhất, mà tìm nhiều bot giỏi theo cách khác nhau.

summonerBot và resilientBot có fitness thô thấp hơn nhưng được chọn qua composite score, phản ánh triết lý: bot specialise vào summon chain hay phòng thủ không nhất thiết thắng nhiều nhất, nhưng chúng có *phong cách chơi riêng biệt* đủ để tạo ra sự đa dạng trải nghiệm cho người chơi.

> **[HÌNH 5.8 — Radar Chart 5 Bot Được Chọn]** *Biểu đồ radar 8 trục (rút gọn từ 9 nhóm gene): Stat (genes 0-1), Keywords (4-6), Trigger (7-12), Effect (13-17), Tribe (18-20), Board (21-23), Reroll (24-27), Spell (28-31). Mỗi trục là giá trị trung bình của nhóm gene tương ứng. Năm đường màu cho 5 bot. Thể hiện profile chiến lược khác biệt.*

---

### 5.7.5 Thảo Luận — Điều Gì Training Học Được?

Một số quan sát từ kết quả training đáng giá phân tích:

**Quan sát 1 — Niles có lợi thế tự nhiên trong game này:**

Niles chiếm tỉ lệ quần thể cao hơn Babylon trong phần lớn các thế hệ (~43% vs ~30%). Điều này có thể phản ánh rằng cơ chế Reborn + OnAllyDeath chain của Niles tạo ra sức mạnh combat tự nhiên hơn — đội hình tái sinh liên tục khó bị tiêu diệt trong 50 round. Đây là insight về game balance không thể thu được chỉ từ thiết kế tay — chỉ sau khi để GA "chơi thật" hàng nghìn trận mới lộ ra.

**Quan sát 2 — Best fitness hội tụ rất sớm (gen 6), avg vẫn tiếp tục cải thiện:**

Best fitness không tăng sau gen 6, nhưng avg fitness tiếp tục tăng từ 2871 → ~3050 (+6.2%). Điều này cho thấy GA đang làm đúng vai trò của nó: không chỉ tìm ra cá thể tốt nhất mà *nâng cao chất lượng trung bình của toàn quần thể*. Khi quần thể trở nên mạnh hơn tổng thể, ngay cả các bot không được chọn vào thư viện cũng chơi tốt hơn — điều này có ý nghĩa cho benchmark system.

**Quan sát 3 — Std Dev giảm lành mạnh, không về 0:**

Std dev giảm từ 952 xuống ~500–565 (giảm ~41%) nhưng không tiến về 0. Điều này chứng tỏ island model + immigration đã thành công trong việc ngăn premature convergence: quần thể hội tụ về vùng fitness tốt nhưng vẫn duy trì đủ đa dạng để chọn được 5 specialist khác nhau.

**Quan sát 4 — summonerBot và resilientBot cải thiện theo thế hệ:**

`best_summoner` tăng từ 7.77 → 8.56 (+10.2%) và `best_resilient` tăng từ 5.59 → 6.13 (+9.7%) qua 180 thế hệ. Điều này cho thấy GA không chỉ tối ưu cho hardBot fitness tổng mà cũng tiến hóa ra các chromosome có profile đặc thù hơn theo thời gian — kết quả trực tiếp của seeded sub-population và elitism per-archetype.

---

*[Kết thúc Chương 5 — Tiếp theo: Chương 6 — Kết Quả Và Đánh Giá]*
