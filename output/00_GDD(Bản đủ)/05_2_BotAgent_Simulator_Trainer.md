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
private static float ScoreFromA(int result, int hpA, int hpB, int turns,
                                  float lateScoreA, float lateScoreB,
                                  float cardScoreA, float cardScoreB)
{
    float score = result > 0 ? 300f   // thắng
                : result == 0 ? 100f  // hòa
                : 10f;                // thua

    score += hpA * 8f;
    score -= hpB * 4f;
    score += Mathf.Max(0f, lateScoreA) * 0.06f;
    score += Mathf.Clamp(lateScoreA - lateScoreB, -300f, 300f) * 0.04f;
    score += Mathf.Max(0f, cardScoreA) * 0.035f;
    score += Mathf.Clamp(cardScoreA - cardScoreB, -250f, 250f) * 0.025f;
    if (turns >= 12)
        score += (turns - 11) * 2f;

    return Mathf.Max(1f, score);
}
```

`lateScoreA` và `cardScoreA` là kết quả của `BoardPower()` và `CardQuality()` — hai hàm tính sức mạnh board và chất lượng tay bài sau khi trận kết thúc. Giá trị tuyệt đối của chúng lớn (thường 50.000–140.000) nhưng trọng số rất nhỏ (0.06 và 0.035), đảm bảo chúng chỉ là tín hiệu phụ.

Bốn thành phần của công thức này encode bốn khía cạnh khác nhau của "chơi tốt":

**Thành phần 1 — Kết quả nhị phân (300/100/10):** Khoảng cách thắng-thua tăng từ 85 (120−35) lên **290 (300−10)** — thắng và thua giờ cách nhau hoàn toàn, không còn "thua nhưng được điểm bù đáng kể". Fitness function hiện tại *đặt ưu tiên thắng trận là tuyệt đối*, thay vì cân bằng giữa thắng-thua và board power như phiên bản trước.

**Thành phần 2 — Biên thắng (`hpA×8 − hpB×4`):** Tăng hệ số so với trước (6→8, 3→4). HP của bản thân quan trọng hơn HP đối thủ — khuyến khích bot giữ mình sống và thắng rõ ràng, không chỉ thắng sát.

**Thành phần 3 — Board power phụ (lateScore + cardScore):** Trọng số cực nhỏ (0.06 và 0.035) — chỉ ảnh hưởng khi kết quả và HP gần nhau. Vai trò là tie-breaker giữa hai chromosome có thành tích tương đương, không phải mục tiêu chính.

**Thành phần 4 — Bonus late game (`(turns−11)×2` nếu turns ≥ 12):** Ngược với phiên bản trước (thưởng thắng nhanh), giờ thưởng nhẹ cho trận kéo dài qua lượt 12. Điều này khuyến khích bot xây đội hình bền vững vào late game thay vì all-in sớm.

Phạm vi điểm thực tế: thắng ở lượt 15 với HP đầy có thể đạt `300 + 7×8 + (15−11)×2 + lateBonus ≈ 365+`; thua với đối thủ còn 7 HP có thể xuống tới `10 + 0 − 7×4 = −18 → 1` (clamp). Phạm vi phân tán đủ rộng nhờ khoảng cách thắng-thua 290 điểm.

> **[HÌNH 5.5 — Hàm Fitness ScoreFromA]** *Biểu đồ thanh minh họa điểm của ba kịch bản: Thắng sớm (HP cao, ít lượt), thắng muộn (HP vừa, nhiều lượt), thua sát (HP gần 0, địch HP gần 0). Mỗi thanh chia thành 3 phần màu: kết quả nhị phân, biên thắng, tốc độ.*

---

## 5.5 GATrainer — Vòng Lặp Tiến Hóa

### 5.5.1 Tham Số Huấn Luyện

GATrainer có hai bộ tham số: *quick mode* để kiểm tra nhanh và *production mode* để training thực sự:

| Tham số | Quick Mode | Production Mode | Ý nghĩa |
|---------|:----------:|:---------------:|---------|
| `populationSize` | 30 | 320 | Số chromosome trong quần thể |
| `generations` | 40 | 200 | Số thế hệ tối đa (có early stop) |
| `matchesPerChrom` | 5 | 32 | Số trận đấu mỗi chromosome/thế hệ |
| `mutationRate` | 0.10 | 0.10 | Xác suất mutation mỗi gene (ban đầu → 6% cuối) |
| `mutationMag` | 0.12 | 0.12 | Biên độ Gaussian mutation (σ, ban đầu → 0.06 cuối) |
| `immigrantRate` | 0.12 | 0.12 | Tỉ lệ chromosome mới mỗi thế hệ (ban đầu → 8% cuối) |
| `minLibraryDistance` | 0.18 | 0.18 | Khoảng cách Euclidean tối thiểu giữa các specialist bot |

Quick mode hoàn thành trong ~2 phút, dùng để kiểm tra logic và debug. Production mode (~20–30 phút) cho ra kết quả đủ tốt để tích hợp vào game — thực tế có thể dừng sớm hơn 200 thế hệ nếu plateau detection kích hoạt (tối thiểu 75% số thế hệ, tức gen 150, trước khi có thể dừng).

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
    case 2: // Summoner/Niles chain
        c.genes[14] = Random.Range(0.75f, 1.0f);  // eSummon
        c.genes[13] = Random.Range(0.65f, 0.95f); // eAddStats
        c.genes[15] = Random.Range(0.55f, 0.85f); // eDealDmg
        c.genes[2]  = Random.Range(0.60f, 0.90f); // wTierBonus
        c.genes[8]  = Random.Range(0.55f, 0.85f); // tOnDeath (tăng từ [0.45, 0.75])
        c.genes[35] = Random.Range(0.50f, 0.85f); // tOnAllyDeploy (mới)
        c.genes[27] = Random.Range(0.00f, 0.15f); // wProactiveSell thấp — giữ shell units
        c.genes[21] = Random.Range(0.00f, 0.25f); // wMerge thấp
        c.genes[9]  = Random.Range(0.00f, 0.20f); // tOnAttack thấp
        c.genes[5]  = Random.Range(0.45f, 0.75f); // wReborn (điều chỉnh từ [0.70, 1.0])
        c.genes[23] = Random.Range(0.10f, 0.40f); // wSaveThreshold thấp — cần thực sự mua
        break;
    case 3: // Resilient seed — bền bỉ, phản đòn, không chết dễ
        c.genes[1]  = Random.Range(0.75f, 1.0f);  // wHP
        c.genes[4]  = Random.Range(0.70f, 1.0f);  // wTaunt
        c.genes[5]  = Random.Range(0.70f, 1.0f);  // wReborn
        c.genes[6]  = Random.Range(0.70f, 1.0f);  // wSafeguard
        c.genes[10] = Random.Range(0.70f, 1.0f);  // tOnTakeDmg
        c.genes[17] = Random.Range(0.50f, 0.90f); // eGiveBuff (mới) — buff đồng minh
        c.genes[0]  = Random.Range(0.25f, 0.55f); // wATK thấp — phòng thủ, không ATK
        break;
    case 4: // Random — hoàn toàn ngẫu nhiên
}
```

Cơ chế seeding giải quyết một vấn đề thực tiễn: trong không gian 37 chiều, xác suất để GA *ngẫu nhiên tìm ra* chromosome Summoner — đòi hỏi đồng thời nhiều gene cụ thể ở mức cao — là cực kỳ thấp nếu khởi đầu từ ngẫu nhiên hoàn toàn. Bằng cách "đặt" 20% quần thể vào vùng lân cận của mỗi archetype, GA được dẫn đến các vùng không gian mà nó có thể không bao giờ khám phá được trong thời gian training hạn chế.

Quan trọng: seeding chỉ khởi tạo *một số* gene, phần còn lại vẫn ngẫu nhiên. Điều này để GA tự tìm giá trị tối ưu cho các gene "phụ" của mỗi archetype, thay vì hoàn toàn cứng hóa chromosome từ đầu.

**Warm-start từ thư viện cũ:** Ngoài 5 nhóm seeded trên, trước khi vào vòng lặp GA, trainer còn nạp `AI_Library.json` hiện tại (nếu có) và inject các bot đã được training trước đó vào quần thể ban đầu qua `InjectLibrarySeeds()`:

```csharp
if (previousLibrary != null)
    InjectLibrarySeeds(population, previousLibrary, popSize);
// Inject hardBot, babylonBot, nileBot, summonerBot, resilientBot
// + ~5 clone mutate nhẹ (mutRate=0.18, mutMag=0.045) cho mỗi bot
```

Kết quả: quần thể bắt đầu với một số chromosome đã qua training — fitness tại gen 0 không phải gần 0 mà đã ở mức cao. Toàn bộ quá trình training sau đó là *cải thiện từ nền tảng sẵn có*, không phải khởi đầu từ đầu.

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
// Giảm từ 10% xuống 6% — giữ khả năng explore ở giai đoạn cuối
float currentMutationRate = Lerp(0.10f, 0.06f, SmoothStep(0f, 1f, progress));

// Giảm từ σ=0.12 xuống σ=0.06 — bước nhảy đủ lớn khi bị stuck
float currentMutationMag  = Lerp(0.12f, 0.06f, SmoothStep(0f, 1f, progress));

// Giảm từ 12% xuống 8% — nhiều máu mới hơn ở giai đoạn cuối
float currentImmigrantRate = Lerp(0.12f, 0.08f, SmoothStep(0f, 1f, progress));
```

So với thiết kế ban đầu (giảm về 3.5%), giá trị cuối được giữ cao hơn đáng kể (6%–8%). Lý do: khi training với warm-start, quần thể bắt đầu ở vùng gene tốt — mutation quá nhỏ sẽ không đủ để thoát khỏi local optimum xung quanh bộ gene hiện tại. Mutation và immigration ở mức 6–8% vào cuối training đảm bảo GA vẫn có khả năng khám phá vùng lân cận rộng hơn, không bị "đóng băng" xung quanh kết quả warm-start.

`SmoothStep` tạo ra đường cong giảm chậm ở đầu và cuối, nhanh ở giữa — thay vì linear. Kết quả: đầu training khám phá rộng (mutation mạnh), cuối training fine-tune nhưng vẫn giữ đủ entropy để vượt plateau.

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
// CurrentImmigrantRate giảm từ 12% xuống 8% theo progress

// Bơm thêm theo tỉ lệ popSize — không còn hardcoded +2/+3
if (pctB < 12f) immigrantCount += Mathf.RoundToInt(popSize * 0.06f);
if (pctN < 12f) immigrantCount += Mathf.RoundToInt(popSize * 0.08f);
if (pctO < 10f) immigrantCount += Mathf.RoundToInt(popSize * 0.07f);

// Điền phần còn lại bằng SelectImmigrantGroup — chọn nhóm còn thiếu
while (nextGen.Count < populationSize)
    nextGen.Add(CreateSeededChromosome(SelectImmigrantGroup(nextGen.Count, pctB, pctN, pctO)));
```

`SelectImmigrantGroup()` không còn random mà kiểm tra diversity thực tế: nếu Babylon < 15% → ưu tiên group Babylon; nếu Niles < 15% → Niles; nếu Other < 12% → Summoner/Resilient. Chỉ khi tất cả đều đủ, immigrant mới được random. Cơ chế này đảm bảo immigrant không "phung phí" vào tribe đã đủ mạnh mà tập trung vào tribe thiếu hụt.

Immigrant rate giảm từ 12% xuống 8% (thay vì 4% trước đây) — giữ lượng máu mới đủ lớn trong suốt training để tránh stuck ở vùng gene warm-start. Tỉ lệ diversity boost cũng tỉ lệ theo `populationSize` thay vì hardcoded, đảm bảo cơ chế này có hiệu lực tương đương với mọi quy mô quần thể.

---

### 5.5.8 Dừng Sớm — Plateau Detection

```csharp
const int   PLATEAU_PATIENCE = 28;
const float PLATEAU_EPS      = 150f;
int minStopGen = Mathf.RoundToInt(generations * 0.75f);   // ≥75% gen trước khi stop

// progressScore là chỉ số tổng hợp thay vì chỉ std_dev
float progressScore = bestEver + avgEma * 0.45f + avgLate * 0.08f + avgCard * 0.035f;

if (progressScore - prevProgressScore < PLATEAU_EPS)
    plateauCount++;
else
    plateauCount = 0;

prevProgressScore = Mathf.Max(prevProgressScore, progressScore);

if (plateauCount >= PLATEAU_PATIENCE && g >= minStopGen)
{
    Debug.LogWarning($"Early stop tại gen {g} — plateau {PLATEAU_PATIENCE} gen.");
    break;
}
```

`progressScore` là chỉ số tổng hợp bốn chiều: best fitness (chỉ số chính), `avgEma` (trung bình có trọng số chuỗi — phản ánh xu hướng avg), `avgLate` (sức mạnh late game của đội hình), và `avgCard` (chất lượng tay bài). Bot có board đẹp nhưng avg đang cải thiện sẽ không bị dừng sớm — chỉ dừng khi cả bốn chiều đều đình trệ.

`PLATEAU_PATIENCE = 28` thế hệ và `PLATEAU_EPS = 150` — ngưỡng lớn hơn nhiều so với phiên bản trước (15 thế hệ, 0.5 điểm) vì progressScore có biên độ lớn hơn. Điều kiện `g >= minStopGen` (75% số thế hệ) ngăn dừng sớm bất ngờ: dù plateau xảy ra sớm, training vẫn chạy ít nhất 150 thế hệ để warm-start seeds có đủ thời gian phát triển.

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

Ngưỡng `viable` được cập nhật để ổn định hơn khi hallOfFame rất mạnh:

```csharp
float hardFitness = hallOfFame?.fitness ?? population[0].fitness;
float threshold   = Mathf.Max(avgFinal * 0.8f, hardFitness * 0.55f);
```

Ngưỡng sàn `hardFitness * 0.55f` ngăn trường hợp avgFinal thấp (do quần thể đa dạng) làm lọt những chromosome quá yếu vào pool `viable`. summonerBot và resilientBot phải đạt ít nhất 55% fitness của hallOfFame — đảm bảo cả 5 bot đều có chất lượng tối thiểu trong cùng thang đo.

`SummonerScore` và `ResilientScore` là composite metrics phản ánh profile gene của archetype:

```csharp
// SummonerScore — chuỗi triệu hồi combat
float SummonerScore(Chromosome c) =>
    c.genes[14]       // eSummon — identity chính
  + c.genes[5] * 0.8f // wReborn — sinh tử lặp
  + c.genes[8] * 0.9f // tOnDeath — deathrattle chain
  + c.genes[34]       // tOnAllyGroup — phản ứng đồng minh
  - c.genes[9] * 0.8f // tOnAttack — penalty: không phải OnAttack archetype
  - c.genes[27] * 1.2f // wProactiveSell — penalty: không bán shell units
  - c.genes[23] * 1.5f // wSaveThreshold — penalty: không tiết kiệm quá nhiều

// ResilientScore — phòng thủ bền bỉ
float ResilientScore(Chromosome c) =>
    c.genes[1] * 1.4f  // wHP — máu cao là core
  + c.genes[4]         // wTaunt
  + c.genes[5]         // wReborn
  + c.genes[6]         // wSafeguard
  + c.genes[10]        // tOnTakeDmg — phản ứng khi bị đánh
  + c.genes[17]        // eGiveBuff — buff đồng minh (mới)
  + c.genes[0] * 0.2f  // wATK — nhỏ nhưng dương (không hoàn toàn 0 ATK)
  - c.genes[9] * 0.5f  // tOnAttack — penalty nhẹ
```

Thứ tự chọn:
1. **hardBot** = hallOfFame (chromosome có fitness cao nhất *bao giờ* qua toàn bộ training)
2. **babylonBot** = chromosome Babylon fitness cao nhất, cách xa hardBot
3. **nileBot** = chromosome Niles fitness cao nhất, cách xa hardBot và babylonBot
4. **summonerBot** = chromosome `SummonerScore` cao nhất trong `viable`
5. **resilientBot** = chromosome `ResilientScore` cao nhất trong `viable`

---

### 5.5.10 Validation Sau Training — Tránh Regression

Sau khi training hoàn tất và library mới được tạo, một bước cuối cùng so sánh library mới với library cũ trước khi ghi đè:

```csharp
var previousLibrary = LoadExistingLibrary();
var candidateLibrary = RunGA(popSize, generations, matchesPerChrom, previousLibrary);

if (previousLibrary != null)
{
    var prevScore      = EvaluateLibrary("previous", previousLibrary).overallScore;
    var candidateScore = EvaluateLibrary("candidate", candidateLibrary).overallScore;

    if (prevScore > candidateScore * 1.03f)
    {
        // Previous library thắng validation → giữ nguyên
        library = previousLibrary;
    }
}
SaveLibrary(library);
```

`EvaluateLibrary()` chạy mỗi bot trong library qua 4×benchmark (tổng ~80 trận mỗi bot) với seed ngẫu nhiên cố định (`Random.InitState(260604)`) để kết quả reproducible, tính `overallScore = trung bình botScore` của cả 5 bot. Ngưỡng 3% (`candidateScore * 1.03f`) đảm bảo chỉ giữ lại library mới khi nó thực sự tốt hơn — tránh trường hợp training ra library kém hơn do may mắn ngẫu nhiên trong một lần chạy.

Cơ chế này quan trọng khi kết hợp với warm-start: nếu training khởi động từ library cũ nhưng vì lý do nào đó (seed ngẫu nhiên xấu, plateau sớm) ra kết quả tệ hơn, game sẽ tự động tiếp tục dùng library cũ thay vì bị downgrade.

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

Lần training được ghi nhận trong báo cáo này sử dụng production mode trên máy tính cá nhân (Windows 10, CPU 8 nhân), dữ liệu từ `training_20260604_221518.csv`:

| Tham số | Giá trị |
|---------|---------|
| Population size | 320 |
| Generations (max) | 200 (dừng sớm nếu plateau) |
| Matches per chromosome | 32 |
| Mutation rate (ban đầu → cuối) | 10% → 6% |
| Mutation magnitude | σ=0.12 → 0.06 |
| Immigrant rate | 12% → 8% |
| Tournament size | k=3 → 4 → 5 |
| Warm-start | Có (inject library từ lần training trước) |

Tổng số trận đấu simulation tối thiểu: `320 × 32 × 200 = 2.048.000 trận`. Thực tế cao hơn do benchmark opponents (thêm `320 × 10` trận mỗi thế hệ). Tuy nhiên với early stopping, training này dừng ở gen 199 — gần đủ số thế hệ tối đa.

---

### 5.7.2 Đường Cong Hội Tụ Fitness

Dữ liệu từ CSV log training thể hiện ba giai đoạn với đặc trưng khác hẳn các lần training không có warm-start:

**Giai đoạn 1 — Khởi đầu cao, avg hội tụ nhanh (Gen 0–40)**

| Gen | Best | Avg | Std Dev | Niles% | Other% | Diversity |
|:---:|:----:|:---:|:-------:|:------:|:------:|:---------:|
| 0 | **19.392** | 8.553 | 3.407 | 52,8% | 15,9% | 89,7 |
| 5 | 19.392 | 10.056 | 2.705 | 44,7% | 22,5% | 96,3 |
| 10 | 19.392 | 10.256 | 2.419 | 38,8% | 20,9% | 96,5 |
| 20 | 19.392 | 10.416 | 2.958 | 39,1% | 27,8% | 99,0 |
| 40 | 19.392 | 10.590 | 2.900 | 23,8% | 32,2% | 96,9 |

Best fitness bắt đầu ở 19.392 — đây là trực tiếp kết quả của warm-start: chromosome từ library cũ đã được inject vào quần thể ban đầu. Không có giai đoạn "tìm kiếm nhanh" như training từ đầu. Avg tăng mạnh chỉ trong 5 thế hệ đầu (+17.6%), phản ánh quần thể còn lại (không phải warm-start seeds) đang leo lên nhanh về vùng fitness tốt. Diversity tăng từ 89.7 lên ~96–99 (gần cân bằng hoàn toàn giữa các tribe).

**Giai đoạn 2 — Other tribe nổi lên (Gen 40–150)**

| Gen | Best | Avg | Babylon% | Niles% | Other% | Diversity |
|:---:|:----:|:---:|:--------:|:------:|:------:|:---------:|
| 60 | 19.392 | 10.896 | 25,0% | 28,4% | **46,6%** | 96,0 |
| 100 | 19.392 | 10.761 | 20,6% | 18,1% | **61,3%** | 82,4 |
| 116 | **20.275** | — | — | — | — | — |
| 150 | 20.275 | 10.700 | 17,5% | 12,8% | **69,7%** | 70,1 |

Best fitness cải thiện lần duy nhất ở gen 116 (+4,55%), tăng từ 19.392 lên 20.275. Đây là cải thiện nhỏ nhưng có ý nghĩa — GA đã tìm được chromosome tốt hơn library cũ, không chỉ dừng lại ở warm-start.

Điểm đáng chú ý: "Other" tribe (generalist / non-Babylon / non-Niles) chiếm ưu thế mạnh ở giai đoạn này, đỉnh điểm 69,7% ở gen 150. Điều này xảy ra vì với scoring mới (Win=300), chromosome generalist có thể thắng ổn định hơn các specialist trong đấu trường quần thể — nhất là khi quần thể đã đủ mạnh để specialist phải đối mặt với đa dạng chiến lược.

**Giai đoạn 3 — Diversity phục hồi, ổn định (Gen 150–199)**

| Gen | Best | Avg | Std Dev | Babylon% | Niles% | Other% | Diversity |
|:---:|:----:|:---:|:-------:|:--------:|:------:|:------:|:---------:|
| 199 | 20.275 | 10.835 | 2.475 | 35,0% | 30,3% | 34,7% | **99,8** |

Diversity đạt 99,8 — gần hoàn hảo cân bằng ba tribe. Std dev giảm từ 3.407 (gen 0) xuống 2.475 (-27%), cho thấy quần thể hội tụ có kiểm soát. Avg cải thiện tổng cộng +26,7% từ gen 0 đến gen 199.

> **[HÌNH 5.6 — Đường Cong Fitness Qua 200 Thế Hệ]** *Biểu đồ đường: trục hoành là thế hệ (0–199), trục tung là điểm fitness. Ba đường: Best (đỏ đậm — bắt đầu cao nhờ warm-start, cải thiện nhỏ tại gen 116), Avg (xanh lam — tăng liên tục), Std Dev (vàng — trục phụ, giảm dần). Đánh dấu gen 0 (warm-start baseline), gen 116 (best cải thiện), vùng 100–150 (Other tribe dominance).*

---

### 5.7.3 Diversity — Phân Phối Bộ Tộc Và Diversity Index

Training lần này bổ sung thêm `diversity_index` — chỉ số đa dạng tính theo công thức Herfindahl đảo ngược, chuẩn hóa về 0–100:

```csharp
private static float DiversityIndex(float pctB, float pctN, float pctO)
{
    float b = pctB / 100f; float n = pctN / 100f; float o = pctO / 100f;
    return Mathf.Clamp01((1f - (b*b + n*n + o*o)) / (2f/3f)) * 100f;
}
```

Giá trị 100 = ba tribe phân phối đều nhau (33%/33%/33%). Giá trị 0 = một tribe chiếm toàn bộ. Đây là thước đo khách quan hơn việc chỉ nhìn từng `pct_*` riêng lẻ.

Phân tích phân phối tribe và diversity_index qua CSV data cho thấy:

- **Gen 0:** diversity = 89,7 (Niles chiếm 52,8% nhưng vẫn còn đa dạng nhờ warm-start có cả 5 bot types)
- **Gen 40–100:** diversity dao động 82–97, Babylon và Niles còn cạnh tranh, Other đang lên
- **Gen 100–150:** diversity giảm xuống ~70–82 khi Other chiếm ưu thế (61–70%)
- **Gen 199:** diversity = **99,8** — gần hoàn hảo, ba tribe phân bố gần đều (35% / 30% / 35%)

Đây là kết quả rõ ràng hơn so với các lần training trước: diversity cuối cao hơn đầu (+10 điểm), tức là training không làm mất đa dạng mà thực sự *tăng* sự cân bằng giữa các tribe. Cơ chế immigration tỉ lệ động (`SelectImmigrantGroup`) đã hoạt động đúng — khi Other chiếm 70% ở gen 150, nó tự động tăng immigrant Babylon và Niles để cân bằng lại.

> **[HÌNH 5.7 — Phân Phối Tribe và Diversity Index Qua Các Thế Hệ]** *Biểu đồ kép: phần trên là area chart xếp chồng (Babylon vàng / Niles xanh / Other xám), phần dưới là đường diversity_index (0–100). Đánh dấu vùng gen 100–150 (Other dominance, diversity xuống ~70) và gen 199 (diversity 99,8 — peak cân bằng).*

---

### 5.7.4 Profile Gene Của 5 Bot Được Chọn

Kết quả 5 bot sau training lần này (lưu trong `AI_Library.json`):

| Bot | Fitness (thang mới) | Đặc trưng gene nổi bật |
|-----|:-------------------:|------------------------|
| **hardBot** | **21.233** | Cân bằng, genes[24] = 0,94 (reroll); genes[3] = 0,96 |
| **babylonBot** | 13.812 | genes[18] Babylon dominant, genes[5] = 0,94 (Reborn) |
| **nileBot** | 13.575 | genes[20] Niles dominant, genes[2] = 1,0 (tier) |
| **summonerBot** | 11.178 | genes[14] = 1,0 (Summon), genes[5] = 1,0 (Reborn) |
| **resilientBot** | 12.320 | genes[1] = 0,93 (HP), genes[17] = 1,0 (GiveBuff) |

*Lưu ý: Fitness trong thang mới (Win=300) không so sánh được với thang cũ (Win=120). Giá trị tuyệt đối thấp hơn không có nghĩa bot yếu hơn.*

Một điểm đáng chú ý: với scoring mới ưu tiên kết quả trận thắng/thua, hardBot có fitness cao nhất tuyệt đối (21.233) nhưng khoảng cách với babylonBot/nileBot lớn hơn so với training cũ. Điều này phản ánh rằng các specialist bot "hy sinh" fitness tổng để chuyên sâu vào archetype của mình — babylonBot/nileBot thắng mạnh khi gặp đúng match-up nhưng có thể thua khi gặp counter.

summonerBot và resilientBot được chọn qua composite score tương ứng, phản ánh triết lý: bot chuyên về summon chain hay phòng thủ không nhất thiết thắng nhiều nhất theo thang fitness thô, nhưng chúng có *phong cách chơi riêng biệt* đủ để tạo ra sự đa dạng trải nghiệm cho người chơi.

> **[HÌNH 5.8 — Radar Chart 5 Bot Được Chọn]** *Biểu đồ radar 8 trục (rút gọn từ 9 nhóm gene): Stat (genes 0-1), Keywords (4-6), Trigger (7-12), Effect (13-17), Tribe (18-20), Board (21-23), Reroll (24-27), Spell (28-31). Mỗi trục là giá trị trung bình của nhóm gene tương ứng. Năm đường màu cho 5 bot. Thể hiện profile chiến lược khác biệt.*

---

### 5.7.5 Thảo Luận — Điều Gì Training Học Được?

Một số quan sát từ kết quả training đáng giá phân tích:

**Quan sát 1 — Warm-start tạo ra đường cong hội tụ khác biệt:**

Thay vì "tìm kiếm nhanh → plateau sớm" như training từ đầu, training warm-start cho thấy pattern: "bắt đầu cao, avg tăng liên tục, best cải thiện một lần duy nhất (+4.55%)". Tổng cải thiện avg là +26,7% — lớn hơn nhiều so với training cũ (~6.2%). Điều này hợp lý: warm-start đã chiếm chỗ "best chromosome slot" của gen 0, nhưng toàn bộ quần thể còn lại vẫn có nhiều chỗ để cải thiện qua selection pressure.

**Quan sát 2 — Other tribe (generalist) nổi lên với scoring mới:**

Với scoring ưu tiên thắng/thua (Win=300), chromosome generalist — không bias về tribe cụ thể — có lợi thế ổn định hơn trong đấu trường quần thể đa dạng. Điều này giải thích tại sao Other chiếm 61–70% ở giai đoạn giữa, trong khi training cũ thấy Niles dominate. Insight về game balance: scoring mới "bình đẳng hóa" các tribe hơn, không có tribe nào tự nhiên ưu thế.

**Quan sát 3 — Diversity cuối cao hơn đầu:**

Diversity_index tăng từ 89,7 (gen 0) lên 99,8 (gen 199). Đây là kết quả đáng ngạc nhiên — thông thường training làm giảm diversity do selection pressure. Nguyên nhân: immigration tỉ lệ động (`SelectImmigrantGroup`) đã tích cực cân bằng lại tribe distribution mỗi khi có lệch. Cuối training, ba tribe ở mức 35%/30%/35% — gần như không thể phân biệt được archetype nào "thống trị".

**Quan sát 4 — Một lần cải thiện best fitness sau 116 thế hệ:**

Best fitness chỉ cải thiện một lần duy nhất trong toàn bộ 200 thế hệ (gen 116: 19.392 → 20.275). Điều này cho thấy warm-start đã cung cấp một chromosome rất cạnh tranh từ đầu, và phần lớn thời gian training dành cho việc nâng chất lượng trung bình. Với plateau detection có minStopGen = 75%, training vẫn tiếp tục dù best không tăng — đây là thiết kế đúng: avg cải thiện đến gen 199 có giá trị cho benchmark system.

---

*[Kết thúc Chương 5 — Tiếp theo: Chương 6 — Kết Quả Và Đánh Giá]*
