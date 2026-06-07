# Báo Cáo Phân Tích Kỹ Thuật AI — Dự Án AutoChess

**Ngày:** 2026-05-28  
**Dự án:** AutoChess (Unity)  
**Nhánh:** Khanh-dev

---

## 1. Tổng Quan Kiến Trúc AI

Dự án AutoChess sử dụng hai kỹ thuật AI kết hợp:

- **Genetic Algorithm (GA):** Huấn luyện offline, tiến hóa bộ tham số của bot qua nhiều thế hệ.
- **Weighted Decision Tree:** Chạy online trong game, bot dùng bộ tham số đã học để ra quyết định pha Shop.

Hai kỹ thuật này hoàn toàn tách biệt về vai trò — GA chịu trách nhiệm tối ưu hóa, còn Decision Tree chịu trách nhiệm thực thi hành động.

---

## 2. Genetic Algorithm (GA)

### 2.1 Mục đích

GA được dùng để tự động tìm ra bộ trọng số tối ưu cho bot mà không cần lập trình thủ công (hand-craft) từng quy tắc chiến lược. Thay vào đó, bot học thông qua cạnh tranh giữa các cá thể trong quần thể.

### 2.2 File liên quan

| File | Vai trò |
|------|---------|
| `Assets/Scripts/AI/GATrainer.cs` | Vòng lặp GA chính (MonoBehaviour, chạy trong Editor) |
| `Assets/Editor/AITrainingBatch.cs` | Phiên bản batch headless (không cần mở Unity GUI) |
| `train_ai.ps1` | PowerShell launcher cho headless training |

### 2.3 Tham số GA

| Chế độ | Quần thể | Thế hệ | Trận/cá thể | Thời gian ước tính |
|--------|----------|--------|-------------|-------------------|
| Quick | 30 | 40 | 5 | ~2 phút |
| Production | 100 | 150 | 15 | ~20–30 phút |

### 2.4 Quy Trình Mỗi Thế Hệ

```
1. Đánh giá Fitness
   └── Mỗi Chromosome đấu matchesPerChrom trận vs đối thủ ngẫu nhiên
         Win  → +10 điểm fitness
         Draw → +2  điểm fitness
         Loss → +0  điểm fitness

2. Sắp xếp quần thể theo fitness giảm dần

3. Snapshot độ khó (lưu checkpoint theo tiến độ)
   ├── Gen 25%  → easyBot
   ├── Gen 50%  → mediumBot
   └── Gen 100% → hardBot

4. Sinh thế hệ tiếp theo
   ├── Elitism: giữ nguyên top 10% (tối thiểu 2 cá thể)
   ├── Tournament Selection (k=3): chọn cha mẹ
   ├── Uniform Crossover: con kế thừa từng gene 50/50
   └── Gaussian Mutation: nhiễu loạn ngẫu nhiên
         Rate: 8% mỗi gene
         Magnitude: σ = 0.12 (Box-Muller)
```

### 2.5 Chiến lược Chọn lọc & Lai ghép

- **Tournament Selection (k=3):** Chọn ngẫu nhiên 3 cá thể, lấy cá thể có fitness cao nhất làm cha/mẹ. Cân bằng tốt giữa exploitation và exploration.
- **Uniform Crossover:** Mỗi gene của con được kế thừa ngẫu nhiên từ cha hoặc mẹ với xác suất 50/50, đảm bảo đa dạng gen.
- **Gaussian Mutation (Box-Muller):** Nhiễu loạn theo phân phối chuẩn, giúp tìm kiếm cục bộ mịn hơn so với mutation đồng đều.
- **Elitism:** Bảo toàn top 10% tốt nhất qua thế hệ kế tiếp, ngăn mất mát kiến thức tốt đã tích lũy.

---

## 3. Chromosome — Bộ Gen 32 Chiều

### 3.1 File liên quan

`Assets/Scripts/AI/Chromosome.cs`

### 3.2 Cấu trúc

Mỗi Chromosome là một vector **32 giá trị float ∈ [0, 1]**, mã hóa toàn bộ chiến lược của bot trong pha Shop:

| Index | Nhóm | Ý nghĩa |
|-------|------|---------|
| [0–3] | Chỉ số cơ bản | Trọng số ATK, HP, Tier bonus, Cost efficiency |
| [4–6] | Keyword thụ động | Trọng số Taunt, Reborn, Safeguard |
| [7–12] | Trigger | Trọng số StartOfBattle, OnDeath, OnAttack, OnTakeDamage, EndTurnShop, OnDeploy |
| [13–17] | Hiệu ứng | Trọng số AddStats, Summon, DealDamage, GainCoin, GiveBuff |
| [18–20] | Tộc (Tribe Synergy) | Trọng số Babylon, Olympus, Niles |
| [21–23] | Ngữ cảnh bàn đấu | Merge proximity, Frontline bonus, Save threshold |
| [24–27] | Hành vi Reroll | Reroll threshold, Max reroll count, Coin reserve, Proactive sell |
| [28–31] | Hành vi Spell | Buy threshold, Target strong, Target merged, Economy weight |

### 3.3 Khởi tạo & Sao chép

- **Constructor:** Khởi tạo ngẫu nhiên toàn bộ 32 gene trong [0, 1].
- **Clone():** Deep copy để tạo bản sao độc lập khi lai ghép hoặc đột biến.

---

## 4. Weighted Decision Tree — Ra Quyết Định Pha Shop

### 4.1 File liên quan

`Assets/Scripts/AI/BotAgent.cs`

### 4.2 Mô tả

BotAgent là tác nhân AI không kế thừa MonoBehaviour, hoạt động hoàn toàn trong bộ nhớ (headless). Mỗi turn trong pha Shop, bot thực thi **7 giai đoạn tuần tự**:

```
DecidePrepPhase()
├── 1. RerollPhase        Quyết định có quay lại shop không (tốn 1 coin)
│                         Điều kiện: shop hiện tại có điểm thấp + đủ coin
│
├── 2. BuyUnitsPhase      Mua quân theo điểm Evaluate()
│                         Chỉ mua nếu điểm vượt ngưỡng gene [24]
│
├── 3. BuySpellsPhase     Mua Spell và áp dụng ngay lên quân trên bàn
│                         Điểm EvaluateSpell() so với ngưỡng gene [28]
│
├── 4. ProactiveSellPhase Bán quân yếu nhất dù bàn chưa đầy
│                         Kích hoạt khi gene [27] (proactive sell) đủ cao
│
├── 5. TryMerge           Gộp 3 lv0 → lv1, hoặc 2 lv1 → lv2
│                         Tự động khi đủ bản sao
│
├── 6. RepositionPhase    Sắp xếp bàn chiến lược
│                         Quân Taunt/HP cao → Frontline
│                         Quân ATK cao/ability → Backline
│
└── 7. FreezePhase        Đóng băng shop nếu có quân tốt chưa mua được
                          Điều kiện: coin thiếu + shop có giá trị cao
```

### 4.3 Hàm Đánh Giá

| Hàm | Đầu vào | Mô tả |
|-----|---------|-------|
| `Evaluate(CardDefinition)` | Card chưa mua | Điểm xem xét mua |
| `EvaluateInstance(CardInstance)` | Card trên bàn | Điểm đánh giá quân hiện có |
| `EvaluateSpell(CardDefinition)` | Card Spell | Điểm xem xét mua Spell |
| `TriggerWeight(triggerType)` | Trigger enum | Lấy gene trọng số trigger |
| `EffectWeight(effectType)` | Effect enum | Lấy gene trọng số hiệu ứng |
| `SynergyWeight(tribe)` | Tribe enum | Bonus tộc từ gene [18–20] |

---

## 5. Game Simulation — Mô phỏng Trận Đấu

### 5.1 File liên quan

`Assets/Scripts/AI/GameSimulator.cs`

### 5.2 Mô tả

GameSimulator mô phỏng đầy đủ một trận 1v1 giữa hai BotAgent để đánh giá fitness trong quá trình training.

**Luồng `SimulateMatch()`:**

```
HP ban đầu: 7 mỗi bot
Vòng lặp: Turn 1–20 (dừng nếu HP ≤ 0)
  ├── Tạo shop ngẫu nhiên (theo Tier hiện tại)
  ├── Bot1.DecidePrepPhase()
  ├── Bot2.DecidePrepPhase()
  ├── CombatResolver.ResolveTurn()  ← combat thực tế
  └── Cập nhật HP dựa trên kết quả

Kết quả trả về: +1 (Bot1 thắng) / -1 (Bot2 thắng) / 0 (hòa)
```

**Lưu ý thiết kế:** Combat không có randomness → đánh giá fitness ổn định, tái lập được. Shop tier tăng theo công thức `Clamp((turn+1)/2, 1, 6)`.

---

## 6. Runtime Integration — Triển Khai Trong Game

### 6.1 File liên quan

| File | Vai trò |
|------|---------|
| `Assets/Scripts/AI/AILibrary.cs` | Data container, serialize ra JSON |
| `Assets/Scripts/Managers/AIManager.cs` | Singleton, load và phân phối Chromosome |
| `Assets/Resources/AI_Library.json` | File JSON chứa 3 bộ gene (easy/medium/hard) |

### 6.2 Luồng triển khai

```
train_ai.ps1
    └── AITrainingBatch.RunQuick/Production()
          └── GATrainer → AI_Library.json

Game khởi động
    └── AIManager.LoadAI()
          └── Đọc AI_Library.json từ Resources
                ├── easyBot   (Gen 25%)
                ├── mediumBot (Gen 50%)
                └── hardBot   (Gen 100%)

GameManager chọn độ khó
    └── AIManager.GetBrain(difficulty)
          └── BotAgent(chromosome) → chơi game thực
```

---

## 7. Sơ Đồ Kiến Trúc Đầy Đủ

```
┌─────────────────────────────────────────────────────┐
│                   TRAINING PHASE (Offline)          │
│                                                     │
│  GATrainer / AITrainingBatch                        │
│       │                                             │
│       ├── Population[30-100 Chromosomes]            │
│       │                                             │
│       └── for each Generation:                     │
│             ├── Fitness Evaluation                  │
│             │     └── GameSimulator.SimulateMatch() │
│             │           ├── BotAgent.DecidePrepPhase│
│             │           └── CombatResolver          │
│             ├── Sort by Fitness                     │
│             ├── Elitism (top 10%)                   │
│             ├── Tournament Selection (k=3)          │
│             ├── Uniform Crossover                   │
│             └── Gaussian Mutation                   │
│                                                     │
│  Output → AI_Library.json                          │
└─────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────┐
│                   RUNTIME PHASE (Online)            │
│                                                     │
│  AIManager.LoadAI()                                 │
│       └── GetBrain(Easy/Medium/Hard)                │
│             └── Chromosome (32 genes)               │
│                   │                                 │
│                   └── BotAgent.DecidePrepPhase()    │
│                         ├── RerollPhase             │
│                         ├── BuyUnitsPhase           │
│                         ├── BuySpellsPhase          │
│                         ├── ProactiveSellPhase      │
│                         ├── TryMerge                │
│                         ├── RepositionPhase         │
│                         └── FreezePhase             │
└─────────────────────────────────────────────────────┘
```

---

## 8. Thống Kê Tổng Hợp

| Thuộc tính | Chi tiết |
|-----------|---------|
| Kỹ thuật AI chính | Genetic Algorithm + Weighted Decision Tree |
| Số gene mỗi bot | 32 (float ∈ [0,1]) |
| Số giai đoạn quyết định | 7 tuần tự mỗi turn |
| Phương pháp chọn lọc | Tournament Selection (k=3) + Elitism 10% |
| Phương pháp lai ghép | Uniform Crossover (50/50 per gene) |
| Phương pháp đột biến | Gaussian Box-Muller (rate 8%, σ=0.12) |
| Reward fitness | Thắng +10, Hòa +2, Thua 0 |
| Số mức độ khó | 3 (Easy 25%, Medium 50%, Hard 100% gen) |
| Thời gian training | Quick ~2 phút, Production ~20–30 phút |
| File output | `Assets/Resources/AI_Library.json` |

---

## 9. Đánh Giá & Hạn Chế

### Điểm mạnh

- **Tự học không cần hand-craft rules:** GA tự tìm ra chiến lược tối ưu thông qua tiến hóa.
- **3 mức độ khó tự động:** Easy/Medium/Hard được train trong cùng một lần chạy, không tốn thêm công sức.
- **Headless training:** Chạy qua `train_ai.ps1` không cần mở Unity Editor.
- **Kiến trúc tách biệt rõ ràng:** Training (GA) và Gameplay (BotAgent) hoàn toàn độc lập.
- **Elitism bảo toàn tiến bộ:** Ngăn mất kiến thức tốt qua các thế hệ.

### Hạn chế hiện tại

- **Fitness coarse-grained:** Chỉ tính Win/Draw/Loss, chưa xét damage dealt hay board state.
- **Đấu với random, không phải self-play:** Bot học cách thắng đối thủ ngẫu nhiên, có thể không tối ưu so với đối thủ chuyên biệt.
- **Combat không có randomness:** Môi trường training khác với gameplay thực tế (có thể overfit).
- **Không có memory giữa các turn:** Bot không ghi nhớ lịch sử shop để đưa ra quyết định dài hạn.
- **Fitness không phân biệt chiến thắng sớm hay muộn:** Thắng ở turn 5 và turn 20 có giá trị fitness như nhau.

### Hướng cải tiến tiềm năng

- Thêm reward shaping: thưởng damage dealt, penalize thua sớm.
- Áp dụng self-play: các cá thể tốt nhất đấu với nhau.
- Thêm randomness trong combat simulation để gần với gameplay thực hơn.
- Dùng Neural Network thay thế Weighted Decision Tree để tổng quát hóa tốt hơn.
