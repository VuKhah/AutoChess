# CHƯƠNG 5: THUẬT TOÁN DI TRUYỀN VÀ HỆ THỐNG AI

---

## 5.1 Tổng Quan Hệ Thống AI

### 5.1.1 Mục Tiêu Thiết Kế

Khi đặt bút thiết kế hệ thống AI cho dự án này, câu hỏi đầu tiên không phải là "dùng thuật toán gì" mà là "bot AI cần làm được những gì?" Ba yêu cầu được xác định ngay từ đầu:

**Yêu cầu 1 — Không có quy tắc cứng:** Toàn bộ hành vi của bot — từ cách đánh giá một lá bài, quyết định khi nào reroll, ưu tiên bộ tộc nào, bán unit nào, cast spell lên ai — phải phát sinh từ quá trình học, không phải từ if-else do lập trình viên viết tay. Nếu một hành vi nào đó không thể truy xuất về một gene cụ thể trong chromosome, nó không được phép tồn tại trong hệ thống.

**Yêu cầu 2 — Nhiều phong cách chơi, không phải một:** Không đủ nếu chỉ có một bot "tốt nhất". Người chơi cần đối mặt với những đối thủ chơi *khác nhau* — kẻ tích lũy Babylon từng lượt, kẻ dựa vào chuỗi chết-hồi sinh của Niles, kẻ xây đội hình thuần phòng thủ. Điều này đòi hỏi hệ thống training phải tự nhiên tạo ra diversity thay vì hội tụ về một chiến lược duy nhất.

**Yêu cầu 3 — Khả thi trên phần cứng cá nhân:** Toàn bộ quá trình training phải hoàn thành trong vài chục phút trên máy tính cá nhân thông thường. Điều này loại bỏ mọi phương pháp đòi hỏi GPU hay thời gian training tính bằng giờ.

Ba yêu cầu này cùng trỏ về một kiến trúc: Genetic Algorithm với chromosome real-valued, đánh giá qua headless simulation, và cơ chế island model để duy trì diversity.

---

### 5.1.2 Kiến Trúc Tổng Thể — Bốn Thành Phần

Hệ thống AI được chia thành bốn lớp, mỗi lớp có trách nhiệm rõ ràng và không phụ thuộc vào Unity lifecycle:

```
┌─────────────────────────────────────────────────────────┐
│                    KIẾN TRÚC HỆ THỐNG AI                │
│                                                         │
│  ┌─────────────┐   "bộ não"    ┌──────────────────┐    │
│  │ Chromosome  │ ────────────► │    BotAgent       │    │
│  │  37 genes   │               │ DecidePrepPhase() │    │
│  └─────────────┘               └────────┬─────────┘    │
│                                          │ board state  │
│  ┌──────────────────────────────────────▼──────────┐   │
│  │               GameSimulator                      │   │
│  │   EvaluateMatch(botA, botB) → MatchResult       │   │
│  │   20 lượt × [DecidePrepPhase + ResolveTurn]     │   │
│  └───────────────────────┬──────────────────────────┘  │
│                           │ fitness scores              │
│  ┌────────────────────────▼──────────────────────────┐ │
│  │                   GATrainer                        │ │
│  │   Init → Evaluate → Select → Crossover → Mutate   │ │
│  │   → 5 specialist bots → AI_Library.json           │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

**Chromosome** là biểu diễn của một "chiến lược chơi" dưới dạng mảng 37 số thực trong đoạn [0, 1]. Nó không chứa logic — chỉ chứa trọng số.

**BotAgent** là bộ thực thi: nhận chromosome làm "bộ não", dùng các gene để đưa ra quyết định trong mỗi lượt shop. Đây là lớp duy nhất có kiến thức về game state.

**GameSimulator** là môi trường đánh giá: chạy một trận đấu đầy đủ 20 lượt giữa hai BotAgent, trả về điểm số phản ánh chất lượng chiến lược.

**GATrainer** là vòng lặp tiến hóa: điều phối toàn bộ quá trình — khởi tạo quần thể, đánh giá fitness qua GameSimulator, chọn lọc, lai ghép, đột biến — qua nhiều thế hệ cho đến khi tìm được 5 bot chuyên biệt.

> **[HÌNH 5.1 — Kiến Trúc Tổng Thể Hệ Thống AI]** *Sơ đồ bốn thành phần với mũi tên luồng dữ liệu: Chromosome (genes[37]) → BotAgent (quyết định) → GameSimulator (kết quả trận) → GATrainer (vòng lặp tiến hóa) → AI_Library.json (5 bots). Mỗi thành phần một màu, kèm class name C# tương ứng.*

---

### 5.1.3 Kết Quả — Năm Bot Chuyên Biệt

Đầu ra của quá trình training không phải là một bot mà là **năm bot chuyên biệt**, mỗi bot biểu diễn một góc khác nhau trong không gian chiến lược của game:

| Bot | Đặc trưng gene | Phong cách chơi |
|-----|---------------|-----------------|
| **hardBot** | Chromosome có fitness tổng cao nhất (Hall of Fame) | Generalist — cân bằng mọi khía cạnh, không có điểm yếu rõ ràng |
| **babylonBot** | `genes[18]` (sBabylon) dominant | Tích lũy buff qua deploy/sell, snowball dài hạn theo bộ tộc Babylon |
| **nileBot** | `genes[20]` (sNiles) dominant | Chuỗi chết-hồi sinh, tích lũy buff OnAllyDeath theo bộ tộc Niles |
| **summonerBot** | `genes[14]` (eSummon), `genes[5]` (wReborn), `genes[8]` (tOnDeath) cao | Summon/consume chain, giữ shell units, ưu tiên ability hơn chỉ số thuần |
| **resilientBot** | `genes[1]` (wHP), `genes[4]` (wTaunt), `genes[5]` (wReborn), `genes[6]` (wSafeguard) cao | Phòng thủ bền bỉ, hút damage, phản đòn, không ưu tiên tấn công sớm |

Năm bot này phục vụ cả hai mục đích: về mặt gameplay, chúng tạo ra đối thủ đa dạng cho người chơi ở các cấp độ khó khác nhau; về mặt học thuật, chúng là bằng chứng rằng GA với island model có khả năng khám phá nhiều vùng khác nhau trong không gian gene — không hội tụ về một điểm duy nhất.

---

## 5.2 Thiết Kế Chromosome — 37 Gene

Chromosome là trái tim của toàn bộ hệ thống AI. Thiết kế nó không phải là việc đặt ra 37 con số ngẫu nhiên — mỗi gene phải ánh xạ chính xác đến một **chiều quyết định** cụ thể trong vòng lặp gameplay, đảm bảo không gian gene và không gian chiến lược tương đương nhau.

Bảng tổng hợp toàn bộ 37 gene theo 9 nhóm chức năng:

| Nhóm | Gene | Tên | Phạm vi | Chức năng trong hàm đánh giá |
|:----:|:----:|-----|:-------:|------------------------------|
| **1** | 0 | wATK | [0,1] | Trọng số cho `baseATK` khi chấm điểm card |
| | 1 | wHP | [0,1] | Trọng số cho `baseHP` |
| | 2 | wTierBonus | [0,1] | Bonus mỗi tier vượt 1: `(tier−1) × gene × 5` |
| | 3 | wCostEff | [0,1] | Hệ số hiệu quả chi phí: điểm `÷ cost × (1+gene)` |
| **2** | 4 | wTaunt | [0,1] | Giá trị keyword Taunt: `+ gene × 10` |
| | 5 | wReborn | [0,1] | Giá trị keyword Reborn: `+ gene × 12` |
| | 6 | wSafeguard | [0,1] | Giá trị keyword Safeguard: `+ gene × 8` |
| **3** | 7 | tStartBattle | [0,1] | TriggerWeight cho `StartOfBattle` |
| | 8 | tOnDeath | [0,1] | TriggerWeight cho `OnDeath` |
| | 9 | tOnAttack | [0,1] | TriggerWeight cho `OnAttack` |
| | 10 | tOnTakeDmg | [0,1] | TriggerWeight cho `OnTakeDamage` |
| | 11 | tEndTurnShop | [0,1] | TriggerWeight cho `EndTurnShop` (tích lũy theo lượt) |
| | 12 | tOnDeploy | [0,1] | TriggerWeight cho `OnDeploy` |
| **4** | 13 | eAddStats | [0,1] | EffectWeight cho `AddStats` (và AbsorbStats, ScaleTargetStats) |
| | 14 | eSummon | [0,1] | EffectWeight cho `Summon`, `SummonConsumed`, `Destroy+isConsume` |
| | 15 | eDealDmg | [0,1] | EffectWeight cho `DealDamage`, `Destroy` |
| | 16 | eGainCoin | [0,1] | EffectWeight cho `GainCoin` và spell kinh tế |
| | 17 | eGiveBuff | [0,1] | EffectWeight cho `GiveBuff`, `Reborn` effect |
| **5** | 18 | sBabylon | [0,1] | Bonus mỗi đồng minh Babylon đang có trên sân |
| | 19 | sOlympus | [0,1] | Bonus mỗi đồng minh Olympus đang có trên sân |
| | 20 | sNiles | [0,1] | Bonus mỗi đồng minh Niles đang có trên sân |
| **6** | 21 | wMerge | [0,1] | Bonus per bản sao cùng cardID: `copies × gene × (copies==2 ? 16 : 8)` |
| | 22 | wFrontline | [0,1] | Bonus mua Taunt khi frontline còn trống: `emptyFront × gene × 2` |
| | 23 | wSaveThreshold | [0,1] | Ngưỡng tối thiểu để mua: bỏ qua card nếu `score < gene × 3` |
| **7** | 24 | wRerollThresh | [0,1] | Reroll nếu `bestShop < gene × bestBoard` (0 = không bao giờ reroll) |
| | 25 | wRerollMax | [0,1] | Số lần reroll tối đa: `floor(gene×3)+1` → [1..4 lần] |
| | 26 | wRerollKeep | [0,1] | Coin dự phòng trước khi reroll: `floor(gene×4)` → [0..4 coin] |
| | 27 | wProactiveSell | [0,1] | Bán unit có `score < gene × 3` dù board chưa đầy |
| **8** | 28 | wSpellThresh | [0,1] | Ngưỡng mua spell: `spellScore/cost > gene × 3` |
| | 29 | wSpellOnStrong | [0,1] | Ưu tiên cast spell lên unit có EvaluateInstance cao nhất |
| | 30 | wSpellOnMerged | [0,1] | Ưu tiên cast spell lên unit có mergeLevel cao hơn |
| | 31 | wSpellEconomy | [0,1] | Nhân thêm cho GainCoin / GainIncome trong EvaluateSpell |
| **9** | 32 | tAura | [0,1] | TriggerWeight cho `Aura` (trước: genes[7]×0.6 cứng) |
| | 33 | tOnSell | [0,1] | TriggerWeight cho `OnSell` (trước: genes[8]×0.5 cứng) |
| | 34 | tOnAllyGroup | [0,1] | TriggerWeight cho `OnAllyDeath`, `OnAllySummon`, `OnAllyReborn` |
| | 35 | tOnAllyDeploy | [0,1] | TriggerWeight cho `OnAllyDeploy` |
| | 36 | tOnAllySell | [0,1] | TriggerWeight cho `OnAllySell` |

> **[HÌNH 5.2 — Biểu Đồ 37 Gene Phân Nhóm Màu Sắc]** *Hình thanh ngang 37 ô, mỗi nhóm một màu nền: Nhóm 1 (xanh lam nhạt), Nhóm 2 (tím nhạt), Nhóm 3 (cam nhạt), Nhóm 4 (xanh lá nhạt), Nhóm 5 (vàng nhạt), Nhóm 6 (hồng nhạt), Nhóm 7 (đỏ nhạt), Nhóm 8 (nâu nhạt), Nhóm 9 (xám). Mỗi ô ghi chỉ số gene và tên viết tắt.*

---

### 5.2.1 Nhóm 1 — Đánh Giá Chỉ Số Cơ Bản (Gene 0–3)

Đây là nhóm gene đơn giản nhất và cũng là nền tảng của mọi quyết định mua bài. Bốn gene này kiểm soát cách bot chuyển đổi các thuộc tính số của card thành một điểm số vô thứ nguyên.

Hàm đánh giá cơ bản cho một `CardDefinition c`:

```csharp
// BotAgent.Evaluate() — phần chỉ số cơ bản
float s = c.baseATK * brain.genes[0]     // wATK
        + c.baseHP  * brain.genes[1]     // wHP
        + (c.tier - 1) * brain.genes[2] * 5f;  // wTierBonus

// ... (các thành phần khác) ...

// Chia cho cost, nhân thêm hệ số hiệu quả
if (c.cost > 0)
    s = s / c.cost * (1f + brain.genes[3]);   // wCostEff
```

**Gene[0] wATK và Gene[1] wHP:** Hai gene này định nghĩa bot "muốn" gì hơn trong một lá bài — tấn công hay sức bền. Bot có `genes[0]` cao sẽ đánh giá cao unit có ATK lớn, từ đó ưu tiên chiến lược aggro. Bot có `genes[1]` cao sẽ tìm unit HP cao, phản ánh playstyle phòng thủ. Hai trọng số này đơn giản nhưng cực kỳ ảnh hưởng — chúng là tầng đầu tiên lọc hàng chục unit trong pool.

**Gene[2] wTierBonus:** Bot có `genes[2]` cao nhận thức rõ rằng unit tier cao *về mặt cơ cấu* tốt hơn unit tier thấp, ngay cả khi baseATK/HP chưa phản ánh điều đó (ví dụ Osiris tier 6 chỉ có 3/3 nhưng ability cực mạnh). Hệ số nhân 5 đảm bảo bonus tier đủ lớn để cạnh tranh với trọng số chỉ số thuần: một unit tier 6 nhận bonus `5 × genes[2] × 5 = 25 × genes[2]` — tương đương khoảng 5 điểm ATK nếu `genes[2] = 0.4`.

**Gene[3] wCostEff:** Đây là gene tinh tế nhất trong nhóm. Thay vì tính điểm tuyệt đối, nó chuẩn hóa điểm *theo cost*: `s / cost × (1 + genes[3])`. Bot có `genes[3]` cao sẽ ưu tiên mua unit "rẻ nhưng tốt" (high value-per-coin), thể hiện chiến lược economy. Bot có `genes[3]` thấp ít quan tâm đến hiệu quả chi phí và sẵn sàng mua unit đắt nếu chỉ số tuyệt đối cao. Điều thú vị là `wCostEff` ảnh hưởng đến cả đánh giá spell — `EvaluateSpell` cũng kết thúc bằng `/ spell.cost × (1 + genes[3])`, nghĩa là bot economy sẽ thích spell rẻ-mà-tốt hơn spell đắt-nhưng-mạnh.

---

### 5.2.2 Nhóm 2 — Passive Keywords (Gene 4–6)

Ba gene này đánh giá giá trị của ba keyword passive trong game: Taunt, Reborn, Safeguard. Không giống chỉ số (ATK/HP có thể tính toán được), giá trị của keyword phụ thuộc vào ngữ cảnh chiến lược — một unit Taunt đặt sai vị trí là vô dụng, nhưng đặt đúng là công cụ phòng thủ mạnh nhất game.

```csharp
// BotAgent.Evaluate() — phần keywords
if (c.hasTaunt)     s += brain.genes[4] * 10f;   // wTaunt
if (c.hasReborn)    s += brain.genes[5] * 12f;    // wReborn
if (c.hasSafeguard) s += brain.genes[6] * 8f;    // wSafeguard
```

Hệ số nhân cố định (10, 12, 8) phản ánh sức mạnh *cơ bản* của từng keyword — Reborn (12) được đánh giá cao hơn Taunt (10) vì nó đảm bảo unit sống ít nhất thêm một lần, còn Safeguard (8) ít mạnh hơn vì chỉ block một đòn.

**Gene[4] wTaunt:** Bot có `genes[4]` cao sẽ ưu tiên mua và giữ unit Taunt, đặt chúng lên frontline. Cơ chế này kết hợp với gene[22] tạo ra sự nhất quán: bot không chỉ đánh giá cao Taunt mà còn *thực sự đặt Taunt lên frontline đúng chỗ*.

**Gene[5] wReborn:** Gene quan trọng nhất trong nhóm theo ngữ cảnh game này vì Reborn tương tác với nhiều cơ chế: summonerBot cần `genes[5]` cao để nhận ra giá trị của unit Reborn trong chuỗi Osiris-nhân-đôi; resilientBot cần nó để xây đội hình kiên cường. Seed khởi tạo của cả summonerBot lẫn resilientBot đều có `genes[5] ∈ [0.70, 1.0]`, phản ánh tầm quan trọng của gene này với cả hai archetype.

**Gene[6] wSafeguard:** Bot có `genes[6]` cao đánh giá cao tính khó chết một lần. Ít bot nào hiện tại specialise sâu vào Safeguard (không có seeded archetype cho Safeguard thuần túy), nhưng gene này ảnh hưởng đến việc resilientBot có mua spell `GiveDoubleAtkAndSafeguard` hay không (vì `EvaluateSpell` case 18 dùng `genes[6]`).

---

### 5.2.3 Nhóm 3 — Trigger Weights (Gene 7–12)

Đây là nhóm gene quan trọng thứ hai về tính biểu đạt chiến lược: sáu gene này kiểm soát cách bot đánh giá *loại kích hoạt* của ability. Cùng một effect `AddStats`, nhưng một unit có trigger `StartOfBattle` (buff một lần đầu trận) khác hẳn unit có trigger `EndTurnShop` (buff tích lũy mỗi lượt) — và gene trong nhóm này quyết định bot nào quan tâm đến sự khác biệt đó.

Hàm `TriggerWeight()` ánh xạ từng `TriggerType` sang giá trị gene tương ứng:

```csharp
private float TriggerWeight(TriggerType t)
{
    switch (t)
    {
        case TriggerType.StartOfBattle: return brain.genes[7];
        case TriggerType.OnDeath:       return brain.genes[8];
        case TriggerType.OnAttack:      return brain.genes[9];
        case TriggerType.OnTakeDamage:  return brain.genes[10];
        case TriggerType.EndTurnShop:   return brain.genes[11];
        case TriggerType.OnDeploy:      return brain.genes[12];
        // ... (các trigger con trong nhóm 9)
    }
}
```

Điểm ability của một card được tính qua tích:

```
abilityScore += TriggerWeight(trigger) × EffectWeight(effect) × 10
```

Nhân trực tiếp trigger weight với effect weight tạo ra tính tổ hợp: bot có cả `genes[7]` (StartOfBattle) lẫn `genes[13]` (AddStats effect) cao sẽ đặc biệt thích card StartOfBattle + AddStats — đây chính là archetype "buff mạnh đầu trận".

**Gene[7] tStartBattle:** Bot có gene này cao ưu tiên unit có buff mạnh ngay đầu trận — các card như "khi trận bắt đầu, tăng ATK toàn đội 3". Chiến lược này phù hợp với aggro: tăng sức mạnh đột ngột trước round đầu, cố gắng tiêu diệt địch nhanh.

**Gene[8] tOnDeath:** Gene của deathrattle — cơ chế "khi chết thì làm gì đó". Bot có `genes[8]` cao sẽ đánh giá cao các unit như Horus (khi chết buff đồng minh), Sekhmet (khi chết giải phóng unit đã nuốt). Đây là một trong các gene quan trọng nhất của summonerBot, và cũng ảnh hưởng đến `PositionScore` — unit có OnDeath được ưu tiên đặt lên *frontline* vì muốn chúng chết sớm để trigger:

```csharp
// PositionScore() — deathrattle muốn ở frontline
case TriggerType.OnDeath:
    score += brain.genes[8] * 5f;
    break;
```

**Gene[9] tOnAttack:** Bot có gene này cao đánh giá unit phát động hiệu ứng mỗi khi tấn công — những unit có utility khi còn sống và đang chiến đấu. SummonerScore phạt `genes[9]` (dấu trừ 0.8f) để đảm bảo summonerBot không bị thu hút về chiến lược OnAttack vốn mâu thuẫn với việc giữ shell units.

**Gene[10] tOnTakeDmg:** Gene điều chỉnh mức độ bot quan tâm đến phản ứng khi bị đánh. resilientBot được seed với `genes[10] ∈ [0.70, 1.0]` — phản ánh đúng thiết kế: một đội hình phòng thủ muốn có unit *phát huy tác dụng khi chịu đòn*, không phải khi tấn công.

**Gene[11] tEndTurnShop:** Trigger EndTurnShop là cơ chế "tích lũy theo thời gian" — unit nhận buff đầu mỗi lượt mới. Bot có `genes[11]` cao theo đuổi chiến lược dài hạn: chấp nhận đội hình yếu ở đầu game nhưng rất mạnh ở late game. Gene này cũng có vai trò kép trong `PositionScore` — unit EndTurnShop bị giảm điểm frontline (muốn để backline để sống lâu tích lũy buff):

```csharp
case TriggerType.EndTurnShop:
    score -= brain.genes[17] * 15f;   // đẩy về backline
    break;
```

**Gene[12] tOnDeploy:** Đánh giá unit kích hoạt hiệu ứng khi được đặt lên sân. Khác với các trigger combat, OnDeploy xảy ra ngay trong Shop Phase — bot có `genes[12]` cao sẽ mua và đặt unit nhiều lần trong một lượt nếu có thể, tận dụng tối đa các trigger OnDeploy tích lũy.

---

### 5.2.4 Nhóm 4 — Effect Weights (Gene 13–17)

Nếu nhóm 3 hỏi "ability kích hoạt *khi nào*?", nhóm 4 hỏi "ability *làm gì*?". Năm gene này đánh giá các loại hiệu ứng từ cơ bản (tăng chỉ số) đến phức tạp (triệu hồi chuỗi). Chúng hoạt động cùng nhóm 3 qua hàm `EffectWeight()`:

```csharp
private float EffectWeight(EffectType e, bool isConsume = false)
{
    switch (e)
    {
        case EffectType.AddStats:        return brain.genes[13];
        case EffectType.Summon:          return brain.genes[14];
        case EffectType.SummonConsumed:  return brain.genes[14] * 1.2f;
        case EffectType.Destroy:
            return isConsume ? brain.genes[14] * 0.7f : brain.genes[15] * 0.8f;
        case EffectType.DealDamage:      return brain.genes[15];
        case EffectType.GainCoin:        return brain.genes[16];
        case EffectType.GiveBuff:        return brain.genes[17];
        case EffectType.Reborn:          return brain.genes[17] * 1.1f;
        case EffectType.AbsorbStats:     return brain.genes[13] * 1.5f;
        case EffectType.GiveStats:       return brain.genes[13] * 1.2f;
        case EffectType.ScaleTargetStats:return brain.genes[13] * 1.3f;
        // ...
    }
}
```

Đáng chú ý là các effect không có gene riêng mà được *suy ra* từ gene gần nhất về ngữ nghĩa, kèm hệ số nhân cố định:

- `SummonConsumed` = `genes[14] × 1.2` — cùng gene Summon nhưng mạnh hơn 20% vì vừa summon vừa tiêu diệt mục tiêu
- `AbsorbStats` = `genes[13] × 1.5` — cùng gene AddStats nhưng "hút" chỉ số từ kẻ địch, mạnh hơn
- `ScaleTargetStats` = `genes[13] × 1.3` — nhân chỉ số (Osiris), tiềm năng cao hơn AddStats thông thường

Thiết kế này giữ số gene tối thiểu mà vẫn bao phủ được toàn bộ 13 effect type trong TTE engine.

**Gene[13] eAddStats:** Gene của chiến lược stat-heavy. Bot có `genes[13]` cao muốn mọi unit đều lớn lên về ATK/HP. Đây là gene quan trọng nhất trong nhóm vì AddStats là effect phổ biến nhất trong game — hầu hết unit có ít nhất một ability AddStats. Nó cũng ánh hưởng đến cả AbsorbStats và ScaleTargetStats qua hệ số nhân.

**Gene[14] eSummon:** Gene của summon chain và consume mechanic. Bot có `genes[14]` cao đặc biệt thích unit có khả năng triệu hồi thêm unit trong combat, và quan trọng hơn, các unit có `isConsume` (Sekhmet nuốt đồng minh → SummonConsumed). Đây là gene trung tâm của summonerBot: `genes[14] ∈ [0.75, 1.0]` được seed ngay từ khởi tạo.

**Gene[15] eDealDmg:** Gene của chiến lược burst damage. Bot có `genes[15]` cao thích unit gây sát thương trực tiếp lên địch (DealDamage effect) và unit có Destroy thông thường. Đây thường là gene cao của hardBot generalist hơn là của bất kỳ specialist nào.

**Gene[16] eGainCoin:** Gene kinh tế. Bot có `genes[16]` cao đánh giá cao unit và spell tạo ra coin, thể hiện chiến lược economy dài hạn. Gene này có mặt trong cả `TriggerWeight` (qua `OnStatGain`) lẫn `EvaluateSpell` (spell GainCoin, GainIncome), tạo ra tính nhất quán: bot economy coi trọng coin ở mọi tầng.

**Gene[17] eGiveBuff:** Gene của utility và support — buff trạng thái (Reborn, Taunt) thay vì chỉ số số. Ngoài việc đánh giá ability, gene này còn điều khiển `PositionScore`: unit cần sống lâu để hỗ trợ (Aura, EndTurnShop accumulator) bị giảm điểm frontline tỉ lệ với `genes[17]`, đẩy chúng ra backline tự động.

---

### 5.2.5 Nhóm 5 — Tribe Synergy (Gene 18–20)

Ba gene này kiểm soát mức độ bot "tin vào" cộng hưởng bộ tộc. Không giống với các nhóm trước đánh giá từng card độc lập, nhóm 5 đánh giá card *trong ngữ cảnh đội hình* — cùng một card Babylon sẽ được đánh giá cao hơn nhiều nếu bot đã có nhiều unit Babylon trên sân.

```csharp
// BotAgent.Evaluate() — phần tribe synergy
float sw = SynergyWeight(c.tribe);   // trả về genes[18/19/20]
if (sw > 0f)
{
    int same = 0;
    foreach (var u in board)
        if (u != null && !u.IsDead && u.Data.tribe == c.tribe) same++;
    s += same * sw * 4f;
}
```

Công thức `same × sw × 4` nghĩa là mỗi đồng minh cùng bộ tộc đang có trên sân cộng thêm `4 × genes[tribe]` điểm cho card đang được xét. Với bot có `genes[18] = 0.9` và 3 unit Babylon trên sân, card Babylon tiếp theo nhận thêm `3 × 0.9 × 4 = 10.8` điểm — đủ để vượt qua một card non-Babylon có chỉ số tốt hơn.

**Gene[18] sBabylon:** Gene định nghĩa babylonBot. Điều kiện nhận dạng babylonBot trong GATrainer: `genes[18] > genes[19] && genes[18] > genes[20]`. Seed khởi tạo: `genes[18] ∈ [0.70, 1.0]`, các bộ tộc khác `∈ [0.0, 0.3]`.

**Gene[19] sOlympus:** Gene cho bộ tộc Olympus. Trong thiết kế game hiện tại, Olympus là bộ tộc thuần ATK — synergy buff ATK toàn đội. Bot có `genes[19]` cao sẽ xây đội hình Olympus aggro. Đây là gene "dead" nhất trong nhóm vì không có seeded archetype chuyên cho Olympus và không có bot được chọn theo `sOlympus` dominant.

**Gene[20] sNiles:** Gene định nghĩa nileBot. Điều kiện nhận dạng: `genes[20] > genes[19] && genes[20] > genes[18]`. Seed: `genes[20] ∈ [0.70, 1.0]`. Tộc Niles có synergy buff HP cho đội hình đủ lớn — bot có `genes[20]` cao sẽ ưu tiên dồn đông unit Niles để đạt ngưỡng buff và kết hợp với các trigger OnAllyDeath chain.

---

### 5.2.6 Nhóm 6 — Board Context (Gene 21–23)

Ba gene này đánh giá card không chỉ theo chỉ số mà theo **trạng thái board hiện tại**. Đây là nhóm gene đưa bot từ "đánh giá card trong chân không" sang "đánh giá card trong ngữ cảnh đội hình".

**Gene[21] wMerge — Merge Proximity:**

```csharp
// BotAgent.Evaluate() — merge bonus
int copies = 0;
foreach (var u in board)
    if (u != null && !u.IsDead && u.Data.cardID == c.cardID && u.mergeLevel == 0)
        copies++;
s += copies * brain.genes[21] * (copies == 2 ? 16f : 8f);
```

Khi đã có 1 bản sao trên sân (`copies = 1`): `+8 × genes[21]`. Khi đã có 2 bản sao (chỉ thiếu 1 để merge): `+16 × genes[21]`. Hệ số tăng gấp đôi ở ngưỡng "sắp merge" có chủ ý — khi chỉ còn 1 bản sao nữa là hoàn thành, giá trị hoàn thành bộ rất cao so với giá trị bắt đầu bộ. Bot có `genes[21]` cao sẽ rất kiên định theo đuổi merge đến cùng thay vì bị xao nhãng bởi card mới.

**Gene[22] wFrontline — Frontline Demand:**

```csharp
// Bonus mua Taunt khi frontline còn chỗ
if (c.hasTaunt)
{
    int emptyFront = 0;
    for (int i = 0; i < FrontlineCount && i < board.Count; i++)
        if (board[i] == null) emptyFront++;
    s += emptyFront * brain.genes[22] * 2f;
}
```

Gene này tạo ra hành vi thực dụng: nếu frontline còn trống và bot có xu hướng giữ frontline đầy (genes[22] cao), nó sẽ ưu tiên mua unit Taunt có ngay để lấp chỗ trống. Điều này phản ánh một nguyên tắc chiến lược thực tế — frontline trống đồng nghĩa với việc backline bị lộ sớm.

**Gene[23] wSaveThreshold — Minimum Buy Bar:**

Gene này xác định ngưỡng tối thiểu để bot *thèm mua* một card. Mọi card có điểm thấp hơn `genes[23] × 3` đều bị bỏ qua dù bot có đủ tiền. Gene này ảnh hưởng đến cả reactive sell (`sellBar = worstBoardScore × (1.5 + genes[23])`) — bot tiêu chuẩn cao hơn cũng đặt ngưỡng trao đổi cao hơn, không dễ dàng bán unit cũ để mua unit mới tầm thường.

---

### 5.2.7 Nhóm 7 — Reroll Behavior (Gene 24–27)

Bốn gene này là nhóm hành vi kinh tế quan trọng nhất — chúng kiểm soát toàn bộ logic reroll và bán unit chủ động, hai quyết định ảnh hưởng trực tiếp đến dòng coin của bot.

**Gene[24] wRerollThresh — Ngưỡng Kích Hoạt Reroll:**

```csharp
// RerollPhase — điều kiện reroll
float shopBest  = BestUnitScore(unitShop);
float boardBest = board.Select(u => EvaluateInstance(u)).Max();

if (shopBest >= brain.genes[24] * boardBest) break;   // shop đủ tốt → dừng
// ngược lại → tiêu 1 coin, lấy shop mới
```

Nếu `genes[24] = 0.9`: bot chỉ hài lòng với shop khi điểm tốt nhất của shop đạt ít nhất 90% điểm tốt nhất của board — tức là shop phải gần ngang ngửa board mới thôi reroll. Đây là bot reroll aggressive. Nếu `genes[24] = 0.3`: chỉ cần shop có card đạt 30% sức mạnh board hiện tại là đủ — bot tiết kiệm, hiếm khi reroll. Giá trị `< 0.05` kích hoạt short-circuit: bot *không bao giờ* reroll.

**Gene[25] wRerollMax — Số Lần Reroll Tối Đa:**

```
maxRerolls = floor(genes[25] × 3) + 1   →   [1..4 lần/lượt]
```

Ngay cả khi điều kiện reroll luôn thỏa mãn, bot không thể reroll vô hạn — gene này đặt trần. Bot với `genes[25] = 1.0` reroll tối đa 4 lần/lượt; bot với `genes[25] = 0.0` tối đa 1 lần. Kết hợp với gene[26], đây tạo ra hành vi "tích cực nhưng có kiểm soát".

**Gene[26] wRerollKeep — Buffer Coin Trước Khi Reroll:**

```
keepCoins = floor(genes[26] × 4)   →   [0..4 coin]
// Điều kiện bổ sung: economy.CurrentCoin >= keepCoins + 1 → mới reroll
```

Bot không reroll nếu làm vậy sẽ tụt xuống dưới ngưỡng dự phòng. Bot với `genes[26] = 1.0` luôn giữ lại ít nhất 4 coin — ngay cả khi shop rất tệ, bot không reroll đến kiệt tiền. Đây là gene "bảo thủ kinh tế", ngăn hành vi tự hại: reroll quá nhiều rồi không có tiền mua card vừa tìm được.

**Gene[27] wProactiveSell — Bán Unit Chủ Động:**

```csharp
// ProactiveSellPhase
if (brain.genes[27] < 0.05f) return;   // không bao giờ bán chủ động
float sellBelow = brain.genes[27] * 3f;

foreach unit on board:
    if (EvaluateInstance(unit) < sellBelow) → Sell
```

Bot với `genes[27]` cao sẵn sàng bán unit kém ngay cả khi board chưa đầy, để giải phóng slot và coin cho các mua lượt sau. Bot với `genes[27] < 0.05` không bao giờ bán chủ động — giữ mọi unit dù chúng có điểm bao nhiêu. summonerBot được seed với `genes[27] ∈ [0.00, 0.15]` — đây là thiết kế có chủ ý: summonerBot cần giữ các "shell units" (unit yếu về chỉ số nhưng là container trong chain SummonConsumed), và bán chủ động sẽ phá hỏng chuỗi đó.

---

### 5.2.8 Nhóm 8 — Spell Behavior (Gene 28–31)

Bốn gene này xử lý toàn bộ logic liên quan đến spell — loại card không chiến đấu nhưng có thể thay đổi cục diện kinh tế và đội hình. Spell được đánh giá qua hàm `EvaluateSpell()` riêng biệt, không dùng cùng formula với unit.

**Gene[28] wSpellThresh — Ngưỡng Mua Spell:**

Tương tự `wSaveThreshold` cho unit, gene này đặt ngưỡng tối thiểu để mua spell: `spellScore / spell.cost > genes[28] × 3`. Bot với `genes[28]` cao rất kén spell — chỉ mua khi value/coin cao. Bot với `genes[28]` thấp sẵn sàng mua spell bất kỳ có giá hợp lý.

**Gene[29] wSpellOnStrong và Gene[30] wSpellOnMerged — Chọn Target Spell:**

Khi quyết định cast spell lên đơn vị nào trên sân, bot dùng:

```csharp
return alive.OrderByDescending(u =>
    EvaluateInstance(u) * brain.genes[29]   // ưu tiên unit mạnh nhất
    + u.mergeLevel * brain.genes[30] * 5f   // ưu tiên unit đã merge
).First();
```

Bot với `genes[29]` cao cast spell lên unit mạnh nhất hiện tại (khuếch đại carry). Bot với `genes[30]` cao ưu tiên unit đã merge nhiều (đầu tư vào unit đã "trưởng thành"). Hai gene này phối hợp tạo ra triết lý "nuôi dưỡng" khác nhau cho mỗi bot.

**Gene[31] wSpellEconomy — Trọng Số Spell Kinh Tế:**

Gene này là nhân tử bổ sung cho các spell tạo coin trong `EvaluateSpell()`:

```csharp
case 6:  // GainCoin
    score += fx.effectValue1 * brain.genes[16] * brain.genes[31];
    break;
case 12: // GainIncome (permanent)
    score += fx.effectValue1 * brain.genes[16] * brain.genes[31]
           * (fx.isPermanent ? 12f : 1.5f);
    break;
```

Để một bot thực sự đánh giá cao spell kinh tế, cần *cả hai* `genes[16]` (eGainCoin) lẫn `genes[31]` (wSpellEconomy) đều cao — thiếu một trong hai sẽ giảm điểm spell coin đáng kể. Hệ số 12 cho permanent income phản ánh tính tích lũy: mỗi +1 coin/lượt tương đương nhận thêm tối đa 20 coin trong một ván đấu dài.

---

### 5.2.9 Nhóm 9 — Trigger Con Độc Lập (Gene 32–36)

Đây là nhóm gene trẻ nhất trong chromosome — được thêm vào ở lần mở rộng thứ hai (từ 32 lên 37 gene). Lý do ra đời của nhóm này phản ánh một bài học thực tiễn quan trọng trong quá trình phát triển.

**Vấn đề trước khi có nhóm 9:**

Ban đầu, các trigger như Aura, OnSell, OnAllyDeath đều dùng chung gene với trigger cha kèm hệ số cứng:

```
// Cách cũ — hệ số CỨNG, GA không học được
Aura       → genes[7] × 0.6        (giả sử Aura = 60% giá trị StartOfBattle)
OnSell     → genes[8] × 0.5        (giả sử OnSell = 50% giá trị OnDeath)
OnAllyDeath → genes[12] × 0.8      (giả sử 80% giá trị OnDeploy)
```

Vấn đề: GA chỉ học được trọng số của trigger cha, không học được giá trị *thực tế* của các trigger con. Nếu trong thực tế game Aura mạnh hơn StartOfBattle (vì hiệu ứng liên tục), hệ số cứng 0.6 sẽ luôn đánh giá thấp Aura — GA không thể sửa sai này.

**Giải pháp — tách gene:**

```csharp
// Cách mới — GA tự học hệ số thực sự
case TriggerType.Aura:          return brain.genes[32];   // tAura
case TriggerType.OnSell:        return brain.genes[33];   // tOnSell
case TriggerType.OnAllyDeath:
case TriggerType.OnAllySummon:
case TriggerType.OnAllyReborn:  return brain.genes[34];   // tOnAllyGroup
case TriggerType.OnAllyDeploy:  return brain.genes[35];   // tOnAllyDeploy
case TriggerType.OnAllySell:    return brain.genes[36];   // tOnAllySell
```

Bây giờ GA hoàn toàn tự do xác định giá trị của từng trigger. Kết quả sau training cho thấy summonerBot thực sự hội tụ về `genes[34]` (tOnAllyGroup) rất cao — phản ánh đúng rằng OnAllyDeath/Summon/Reborn chain là cơ chế cốt lõi của archetype này, quan trọng hơn OnDeploy nhiều.

**Gene[34] tOnAllyGroup** là gene quan trọng nhất trong nhóm: nó bao phủ ba trigger liên quan đến "đồng minh trong combat" — OnAllyDeath, OnAllySummon, OnAllyReborn. Ba trigger này thường đi cùng nhau (Anubis, Osiris, Sobek, Isis đều dùng ít nhất một trong số đó), và cho phép GA gán một trọng số duy nhất phản ánh mức độ bot quan tâm đến "chain combat events". summonerBot được seed `genes[34] ∈ [0.65, 1.0]`.

**Gene[35] tOnAllyDeploy** tách biệt hẳn khỏi nhóm vì OnAllyDeploy xảy ra trong *Shop Phase*, không phải combat — đây là trigger phản ánh hành vi "mỗi khi đặt thêm đồng minh lên sân". Kiểu chiến lược tận dụng trigger này (deploy nhiều unit mỗi lượt để tích buff) khác hẳn với chiến lược summon chain trong combat, nên GA cần gene độc lập để học.

---

### 5.2.10 Tổng Hợp — Tính Đầy Đủ Của Không Gian Gene

Nhìn lại toàn bộ 37 gene qua chín nhóm, có thể kiểm tra tính đầy đủ của thiết kế theo một câu hỏi: *có quyết định chiến lược quan trọng nào trong game mà chromosome không mã hóa được không?*

Mỗi quyết định trong vòng lặp `DecidePrepPhase` đều có ít nhất một gene chi phối:

| Quyết định của bot | Gene chi phối |
|-------------------|---------------|
| Đánh giá unit theo chỉ số | [0] wATK, [1] wHP |
| Ưu tiên unit tier cao | [2] wTierBonus |
| Cân nhắc giá/giá trị | [3] wCostEff |
| Mua unit có keyword | [4] wTaunt, [5] wReborn, [6] wSafeguard |
| Đánh giá ability của unit | [7–12] Trigger + [13–17] Effect + [32–36] Trigger con |
| Ưu tiên bộ tộc | [18] sBabylon, [19] sOlympus, [20] sNiles |
| Theo đuổi merge | [21] wMerge |
| Quản lý frontline | [22] wFrontline |
| Ngưỡng mua tối thiểu | [23] wSaveThreshold |
| Quyết định reroll | [24] wRerollThresh, [25] wRerollMax, [26] wRerollKeep |
| Bán unit kém | [27] wProactiveSell |
| Mua và dùng spell | [28] wSpellThresh, [29] wSpellOnStrong, [30] wSpellOnMerged, [31] wSpellEconomy |
| Vị trí đặt unit | gene[8] (OnDeath→frontline), gene[17] (support→backline), gene[22] (Taunt→frontline) |

Không có quyết định nào bị bỏ sót. Đây là tính đầy đủ mà chromosome 37 gene đạt được — và cũng là lý do GA có thể tiến hóa ra các phong cách chơi đa dạng: vì không gian gene đủ rộng để biểu diễn nhiều chiến lược khác nhau, không bị ép vào một hướng cứng nào.

> **[HÌNH 5.3 — Bảng Đầy Đủ 37 Gene Màu Sắc]** *Bảng 37 hàng, mỗi hàng gồm: chỉ số gene, tên gene, nhóm (màu nền theo nhóm 1–9), phạm vi giá trị, ví dụ ảnh hưởng khi giá trị cao. Phần dưới bảng: biểu đồ cột thể hiện giá trị gene của hardBot, babylonBot, nileBot, summonerBot, resilientBot (5 đường màu chồng lên nhau).*

---

*[Tiếp theo: Mục 5.3 — BotAgent: Bộ Não Quyết Định]*
