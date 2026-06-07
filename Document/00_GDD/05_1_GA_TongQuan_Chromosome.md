# CHƯƠNG 5: THUẬT TOÁN DI TRUYỀN VÀ HỆ THỐNG AI

---

## 5.1 Tổng Quan Hệ Thống AI

### 5.1.1 Mục Tiêu Thiết Kế

Ba yêu cầu được xác định ngay từ đầu khi thiết kế hệ thống AI:

- **Không có quy tắc cứng:** Mọi hành vi của bot — đánh giá card, quyết định reroll, ưu tiên bộ tộc, bán unit — phải phát sinh từ quá trình học, không phải từ if-else viết tay. Nếu một hành vi không truy xuất được về gene cụ thể, nó không tồn tại trong hệ thống.
- **Nhiều phong cách chơi:** Người chơi cần đối mặt với đối thủ chơi *khác nhau* — bot tích lũy Babylon, bot dựa vào chain hồi sinh Niles, bot phòng thủ thuần túy. Hệ thống training phải tự nhiên tạo ra diversity, không hội tụ về một chiến lược duy nhất.
- **Khả thi trên phần cứng cá nhân:** Training hoàn thành trong vài chục phút trên máy thông thường, không cần GPU hay training tính bằng giờ.

Ba yêu cầu này cùng trỏ về một kiến trúc: Genetic Algorithm với chromosome real-valued, đánh giá qua headless simulation, và island model để duy trì diversity.

---

### 5.1.2 Kiến Trúc Tổng Thể — Bốn Thành Phần

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

**Chromosome** — mảng 37 số thực [0,1], biểu diễn chiến lược chơi, không chứa logic.  
**BotAgent** — engine thực thi: nhận chromosome làm "bộ não", dùng gene để quyết định trong mỗi lượt shop.  
**GameSimulator** — môi trường đánh giá: chạy trận đấu 20 lượt giữa hai BotAgent, trả về điểm fitness.  
**GATrainer** — vòng lặp tiến hóa: điều phối khởi tạo, đánh giá, chọn lọc, lai ghép, đột biến qua nhiều thế hệ.

> **[HÌNH 5.1 — Kiến Trúc Tổng Thể Hệ Thống AI]** *Sơ đồ bốn thành phần với mũi tên luồng dữ liệu: Chromosome → BotAgent → GameSimulator → GATrainer → AI_Library.json. Mỗi thành phần một màu, kèm class name C# tương ứng.*

---

### 5.1.3 Kết Quả — Năm Bot Chuyên Biệt

| Bot | Đặc trưng gene | Phong cách chơi |
|-----|---------------|-----------------|
| **hardBot** | Fitness tổng cao nhất (Hall of Fame) | Generalist — cân bằng, không có điểm yếu rõ ràng |
| **babylonBot** | `genes[18]` (sBabylon) dominant | Tích lũy buff qua deploy/sell, snowball dài hạn |
| **nileBot** | `genes[20]` (sNiles) dominant | Chuỗi chết-hồi sinh, tích lũy OnAllyDeath |
| **summonerBot** | `genes[14]` (eSummon), `[5]` (wReborn), `[8]` (tOnDeath) cao | Summon/consume chain, giữ shell units |
| **resilientBot** | `genes[1]` (wHP), `[4]` (wTaunt), `[5]` (wReborn), `[6]` (wSafeguard) cao | Phòng thủ bền bỉ, hút damage, phản đòn |

Năm bot vừa tạo đa dạng trải nghiệm gameplay, vừa là bằng chứng GA với island model khám phá được nhiều vùng khác nhau trong không gian gene thay vì hội tụ về một điểm duy nhất.

---

## 5.2 Thiết Kế Chromosome — 37 Gene

Mỗi gene phải ánh xạ chính xác đến một **chiều quyết định** cụ thể trong `DecidePrepPhase`, đảm bảo không gian gene và không gian chiến lược tương đương nhau. Bảng dưới tổng hợp 37 gene theo 9 nhóm chức năng:

| Nhóm | Gene | Tên | Phạm vi | Chức năng |
|:----:|:----:|-----|:-------:|-----------|
| **1** | 0 | wATK | [0,1] | Trọng số `baseATK` khi chấm điểm card |
| | 1 | wHP | [0,1] | Trọng số `baseHP` |
| | 2 | wTierBonus | [0,1] | Bonus tier: `(tier−1) × gene × 5` |
| | 3 | wCostEff | [0,1] | Hệ số hiệu quả chi phí: `÷ cost × (1+gene)` |
| **2** | 4 | wTaunt | [0,1] | Giá trị keyword Taunt: `+ gene × 10` |
| | 5 | wReborn | [0,1] | Giá trị keyword Reborn: `+ gene × 12` |
| | 6 | wSafeguard | [0,1] | Giá trị keyword Safeguard: `+ gene × 8` |
| **3** | 7 | tStartBattle | [0,1] | TriggerWeight cho `StartOfBattle` |
| | 8 | tOnDeath | [0,1] | TriggerWeight cho `OnDeath` |
| | 9 | tOnAttack | [0,1] | TriggerWeight cho `OnAttack` |
| | 10 | tOnTakeDmg | [0,1] | TriggerWeight cho `OnTakeDamage` |
| | 11 | tEndTurnShop | [0,1] | TriggerWeight cho `EndTurnShop` |
| | 12 | tOnDeploy | [0,1] | TriggerWeight cho `OnDeploy` |
| **4** | 13 | eAddStats | [0,1] | EffectWeight cho `AddStats`, `AbsorbStats`, `ScaleTargetStats` |
| | 14 | eSummon | [0,1] | EffectWeight cho `Summon`, `SummonConsumed` |
| | 15 | eDealDmg | [0,1] | EffectWeight cho `DealDamage`, `Destroy` |
| | 16 | eGainCoin | [0,1] | EffectWeight cho `GainCoin` và spell kinh tế |
| | 17 | eGiveBuff | [0,1] | EffectWeight cho `GiveBuff`, `Reborn` effect |
| **5** | 18 | sBabylon | [0,1] | Bonus mỗi đồng minh Babylon trên sân |
| | 19 | sOlympus | [0,1] | Bonus mỗi đồng minh Olympus trên sân |
| | 20 | sNiles | [0,1] | Bonus mỗi đồng minh Niles trên sân |
| **6** | 21 | wMerge | [0,1] | Bonus per bản sao cùng cardID: `copies × gene × (copies==2 ? 16 : 8)` |
| | 22 | wFrontline | [0,1] | Bonus mua Taunt khi frontline trống: `emptyFront × gene × 2` |
| | 23 | wSaveThreshold | [0,1] | Ngưỡng tối thiểu để mua: bỏ qua card nếu `score < gene × 3` |
| **7** | 24 | wRerollThresh | [0,1] | Reroll nếu `bestShop < gene × bestBoard` |
| | 25 | wRerollMax | [0,1] | Số lần reroll tối đa: `floor(gene×3)+1` → [1..4 lần] |
| | 26 | wRerollKeep | [0,1] | Coin dự phòng trước khi reroll: `floor(gene×4)` → [0..4] |
| | 27 | wProactiveSell | [0,1] | Bán unit có `score < gene × 3` dù board chưa đầy |
| **8** | 28 | wSpellThresh | [0,1] | Ngưỡng mua spell: `spellScore/cost > gene × 3` |
| | 29 | wSpellOnStrong | [0,1] | Ưu tiên cast spell lên unit `EvaluateInstance` cao nhất |
| | 30 | wSpellOnMerged | [0,1] | Ưu tiên cast spell lên unit đã merge nhiều nhất |
| | 31 | wSpellEconomy | [0,1] | Nhân thêm cho GainCoin / GainIncome trong `EvaluateSpell` |
| **9** | 32 | tAura | [0,1] | TriggerWeight cho `Aura` |
| | 33 | tOnSell | [0,1] | TriggerWeight cho `OnSell` |
| | 34 | tOnAllyGroup | [0,1] | TriggerWeight cho `OnAllyDeath`, `OnAllySummon`, `OnAllyReborn` |
| | 35 | tOnAllyDeploy | [0,1] | TriggerWeight cho `OnAllyDeploy` |
| | 36 | tOnAllySell | [0,1] | TriggerWeight cho `OnAllySell` |

> **[HÌNH 5.2 — Biểu Đồ 37 Gene Phân Nhóm Màu Sắc]** *Hình thanh ngang 37 ô, mỗi nhóm một màu nền. Mỗi ô ghi chỉ số gene và tên viết tắt.*

---

### 5.2.1 Nguyên Tắc Thiết Kế Gene

**Tính tổ hợp giữa nhóm 3 và nhóm 4.** Điểm ability của mỗi card được tính qua tích `TriggerWeight(trigger) × EffectWeight(effect) × 10`. Nhân trực tiếp hai gene từ hai nhóm khác nhau tạo ra đặc tính cộng hưởng: bot có cả `genes[8]` (tOnDeath) lẫn `genes[14]` (eSummon) cao sẽ đặc biệt đánh giá cao card dạng *khi chết triệu hồi* — chính là archetype summonerBot. Không cần gene riêng cho từng tổ hợp trigger-effect; `6 × 5 = 30` kết hợp xuất hiện tự nhiên từ 11 gene.

**Context-dependency trong nhóm 5.** Ba gene tribe (18–20) không đánh giá card trong chân không mà đánh giá *trong ngữ cảnh board*: mỗi đồng minh cùng bộ tộc cộng thêm `4 × genes[tribe]` điểm cho card đang xét. Với `genes[18] = 0.9` và 3 unit Babylon trên sân, card Babylon tiếp theo nhận thêm 10.8 điểm — đủ để vượt qua card non-Babylon có chỉ số tốt hơn. Cơ chế này đảm bảo bot tự nhiên theo đuổi chiến lược bộ tộc, không cần rule cứng.

**Tính nhất quán chiến lược qua các nhóm.** Chromosome không thể chứa mâu thuẫn nội tại giữa các gene: bot aggressive reroll (`genes[24]` cao) bị gene[24] ngầm ngăn freeze (`FreezePhase` dùng `1 − genes[24]`), nên không thể vừa reroll mạnh vừa freeze shop. summonerBot cần `genes[27]` (proactive sell) thấp để giữ shell units — và seed khởi tạo phản ánh đúng điều đó.

**Nhóm 9 — bài học thiết kế lặp.** Thiết kế ban đầu chỉ có 32 gene: các trigger như Aura, OnSell, OnAllyDeath dùng chung gene với trigger cha kèm hệ số cứng (`Aura = genes[7] × 0.6`). Vấn đề: GA chỉ học được trọng số trigger cha, không học được giá trị *thực tế* của trigger con. Nếu Aura mạnh hơn StartOfBattle trong thực tế game, hệ số cứng 0.6 luôn đánh giá thấp nó — GA không thể sửa sai này. Giải pháp ở lần mở rộng thứ hai: tách 5 trigger con thành gene độc lập (genes[32–36]), cho GA hoàn toàn tự do xác định giá trị. Kết quả sau training xác nhận: summonerBot hội tụ về `genes[34]` (tOnAllyGroup) rất cao — phản ánh đúng rằng OnAllyDeath/Summon/Reborn chain quan trọng hơn OnDeploy nhiều với archetype này.

---

### 5.2.2 Tính Đầy Đủ Của Không Gian Gene

Mỗi quyết định quan trọng trong `DecidePrepPhase` đều có ít nhất một gene chi phối:

| Quyết định | Gene chi phối |
|-----------|---------------|
| Đánh giá chỉ số unit | [0] wATK, [1] wHP, [2] wTierBonus |
| Cân nhắc giá/giá trị | [3] wCostEff |
| Mua unit có keyword | [4] wTaunt, [5] wReborn, [6] wSafeguard |
| Đánh giá ability (trigger × effect) | [7–12] Trigger + [13–17] Effect + [32–36] Trigger con |
| Ưu tiên bộ tộc (context-aware) | [18] sBabylon, [19] sOlympus, [20] sNiles |
| Theo đuổi merge | [21] wMerge |
| Quản lý frontline | [22] wFrontline |
| Ngưỡng mua tối thiểu | [23] wSaveThreshold |
| Quyết định reroll | [24] wRerollThresh, [25] wRerollMax, [26] wRerollKeep |
| Bán unit kém | [27] wProactiveSell |
| Mua và target spell | [28] wSpellThresh, [29] wSpellOnStrong, [30] wSpellOnMerged, [31] wSpellEconomy |
| Vị trí đặt unit | [8] tOnDeath→frontline, [17] eGiveBuff→backline, [22] Taunt→frontline |

Không có quyết định chiến lược quan trọng nào bị bỏ sót. Đây là điều kiện cần để GA tiến hóa ra các phong cách chơi đa dạng: vì không gian gene đủ rộng để biểu diễn nhiều chiến lược khác nhau, quá trình chọn lọc không bị ép vào một hướng cứng nào.

> **[HÌNH 5.3 — Bảng 37 Gene Với Giá Trị 5 Bot]** *Bảng 37 hàng phân nhóm màu sắc (9 nhóm). Phía dưới: biểu đồ cột thể hiện giá trị gene của hardBot, babylonBot, nileBot, summonerBot, resilientBot (5 đường màu).*

---

*[Tiếp theo: Mục 5.3 — BotAgent: Bộ Não Quyết Định]*
