# Hướng Dẫn Retrain AI — Seed Bot & Population

> Dành cho người muốn thay đổi seed chromosome hoặc tham số training.  
> Có **hai file cần sửa song song** — chúng duplicate nhau, một dùng cho Unity Editor, một dùng cho headless.

---

## Hai Cách Chạy Training

| Cách | File điều khiển | Dùng khi nào |
|------|----------------|-------------|
| **Headless** (nhanh, không cần mở Editor) | `Assets/Editor/AITrainingBatch.cs` | Chạy qua `train_ai.ps1` |
| **Trong Unity Editor** | `Assets/Scripts/AI/GATrainer.cs` | Chạy scene có object GATrainer, hoặc chuột phải → *Retrain AI (Force)* |

```
# Chạy headless:
.\train_ai.ps1              # Quick  — 30 pop × 40 gen  (~2 phút)
.\train_ai.ps1 -Production  # Prod   — 200 pop × 150 gen (~20-30 phút)
```

Output luôn ghi vào:
- `Assets/Resources/AI_Library.json` — 5 bot được load khi game chạy
- `Assets/Document/02_Data/Train/training_YYYYMMDD_HHMMSS.csv` — log từng thế hệ

---

## 1. Thay Đổi Tham Số Population / Generation

### Headless → `Assets/Editor/AITrainingBatch.cs`, dòng 20–26:

```csharp
// ── SỬA Ở ĐÂY ─────────────────────────────────────────────────────────
private const int   QUICK_POP      = 30;    // pop cho quick mode
private const int   QUICK_GEN      = 40;    // gen cho quick mode
private const int   QUICK_MATCHES  = 5;     // trận/chromosome/gen (quick)

private const int   PROD_POP       = 200;   // ← đây là production pop
private const int   PROD_GEN       = 150;   // ← đây là production gen
private const int   PROD_MATCHES   = 25;    // ← trận/chromosome/gen (prod)
// ──────────────────────────────────────────────────────────────────────
```

### Trong Editor → `Assets/Scripts/AI/GATrainer.cs`, Inspector:

Chỉnh trực tiếp trên Inspector của GameObject có component GATrainer:
- `Population Size` — mặc định 30 (test) / production nên đặt 120–200
- `Generations` — mặc định 40 (test) / production nên đặt 150–180
- `Matches Per Chrom` — mặc định 5 / production nên đặt 20–25

---

## 2. Thay Đổi Seed Chromosome

Seed quyết định 20% đầu population mỗi archetype bắt đầu từ đâu.  
**Phải sửa ở cả hai file** — cấu trúc giống hệt nhau.

### Vị trí trong `AITrainingBatch.cs`:

**Seed dùng khi init population** — dòng 108–138:
```csharp
// Assets/Editor/AITrainingBatch.cs — RunGA(), dòng ~103
for (int i = 0; i < popSize; i++)
{
    var c     = new Chromosome();
    int group = Mathf.Min(i / groupSize, 4);   // 0=Babylon, 1=Niles, 2=Summoner, 3=Resilient, 4=Random

    switch (group)
    {
        case 0: // ← SỬA ĐÂY để thay seed Babylon
            c.genes[18] = Random.Range(0.7f, 1.0f);   // sBabylon cao
            c.genes[19] = Random.Range(0.0f, 0.3f);   // sOlympus thấp
            c.genes[20] = Random.Range(0.0f, 0.3f);   // sNiles thấp
            break;

        case 1: // ← SỬA ĐÂY để thay seed Niles
            c.genes[20] = Random.Range(0.7f, 1.0f);   // sNiles cao
            c.genes[18] = Random.Range(0.0f, 0.3f);
            c.genes[19] = Random.Range(0.0f, 0.3f);
            break;

        case 2: // ← SỬA ĐÂY để thay seed Summoner
            c.genes[14] = Random.Range(0.75f, 1.0f);  // eSummon
            c.genes[5]  = Random.Range(0.70f, 1.0f);  // wReborn
            c.genes[8]  = Random.Range(0.65f, 1.0f);  // tOnDeath
            c.genes[34] = Random.Range(0.65f, 1.0f);  // tOnAllyGroup
            c.genes[35] = Random.Range(0.50f, 0.85f); // tOnAllyDeploy
            c.genes[27] = Random.Range(0.00f, 0.15f); // wProactiveSell thấp
            c.genes[0]  = Random.Range(0.10f, 0.40f); // wATK thấp
            break;

        case 3: // ← SỬA ĐÂY để thay seed Resilient
            c.genes[1]  = Random.Range(0.75f, 1.0f);  // wHP
            c.genes[4]  = Random.Range(0.70f, 1.0f);  // wTaunt
            c.genes[5]  = Random.Range(0.70f, 1.0f);  // wReborn
            c.genes[6]  = Random.Range(0.70f, 1.0f);  // wSafeguard
            c.genes[10] = Random.Range(0.70f, 1.0f);  // tOnTakeDmg
            break;
        // case 4: không có seed → random hoàn toàn (hardBot pool)
    }
}
```

**Seed dùng khi tạo immigrant** — dòng 485–518 (hàm `CreateSeededChromosome`):
```csharp
// Assets/Editor/AITrainingBatch.cs — CreateSeededChromosome(), dòng ~485
// ← SỬA cùng pattern với block trên để hai chỗ nhất quán
private static Chromosome CreateSeededChromosome(int group) { ... }
```

> ⚠️ **Quan trọng:** `CreateSeededChromosome` cũng được dùng để tạo `benchmarkOpponents`.  
> Nếu chỉ sửa block init mà quên sửa `CreateSeededChromosome`, benchmark sẽ dùng seed cũ.

### Vị trí tương ứng trong `GATrainer.cs`:

- **Seed init population** — hàm `BeginTraining()`, dòng ~70–106, cùng cấu trúc switch-case
- **Seed immigrant** — hàm `CreateSeededChromosome()`, dòng ~489–526, cùng cấu trúc

---

## 3. Thêm Một Archetype Mới (Ví Dụ: "Aggressor")

Nếu muốn thêm bot thứ 6 hoặc thay thế một archetype hiện có:

**Bước 1** — Thêm case mới vào switch seed (cả 2 file, cả 2 chỗ init + `CreateSeededChromosome`):
```csharp
case 5: // Aggressor — tấn công nhanh, ATK cao
    c.genes[0]  = Random.Range(0.75f, 1.0f);  // wATK cao
    c.genes[12] = Random.Range(0.70f, 1.0f);  // tOnAttack
    c.genes[24] = Random.Range(0.70f, 1.0f);  // wRerollThresh — reroll nhiều
    c.genes[27] = Random.Range(0.50f, 0.85f); // wProactiveSell — bán unit yếu
    c.genes[1]  = Random.Range(0.10f, 0.35f); // wHP thấp
    break;
```

**Bước 2** — Thêm scoring function trong cả 2 file:
```csharp
private static float AggressorScore(Chromosome c) =>
    c.genes[0] * 2.0f   // wATK
  + c.genes[12] * 1.5f  // tOnAttack
  + c.genes[24] * 1.0f  // wRerollThresh
  - c.genes[1] * 0.5f;  // phạt wHP cao
```

**Bước 3** — Thêm vào elitism trong `BreedNextGen()`:
```csharp
AddTopClones(nextGen, population.OrderByDescending(AggressorScore), c => true, 2);
```

**Bước 4** — Thêm vào selection cuối và lưu vào `AILibrary`:
```csharp
// AILibrary.cs cần thêm field:
public Chromosome aggressorBot;

// Trong selection cuối của RunGA():
var aggressor = SelectDistinct(viable, selected, AggressorScore);
library.aggressorBot = aggressor?.Clone();
```

**Bước 5** — Thêm vào `GameManager.SetDifficulty()` để game dùng bot mới:
```csharp
// Assets/Scripts/Managers/GameManager.cs — SetDifficulty(), dòng ~139
AddBot(lib.aggressorBot, 10, "Aggressor");
```

---

## 4. Kiểm Tra Training Đang Chạy Đúng Không

```powershell
# Xem live log trong terminal (headless):
Get-Content training_log.txt -Wait | Select-String "Gen "

# Xem CSV kết quả (mở trong Excel hoặc terminal):
# Col quan trọng: gen, best, avg, std_dev, pct_babylon, pct_niles, pct_other
# Healthy training: std_dev giảm dần, pct_babylon và pct_niles đều > 10%
```

**Dấu hiệu training bị lỗi:**
- `pct_babylon` hoặc `pct_niles` về 0% → elitism per-archetype không hoạt động
- `std_dev` về 0 sau < 20 gen → premature convergence, tăng `mutationRate` hoặc `immigrantRate`
- `avg` không tăng sau gen 0 → fitness function có vấn đề hoặc CardDatabase chưa load

---

## 5. Bảng Gene Index Nhanh

| Gene | Tên | Dùng khi seed |
|:----:|:----|:-------------|
| 0 | wATK | Aggressor (cao), Summoner (thấp) |
| 1 | wHP | Resilient (cao) |
| 4 | wTaunt | Resilient (cao) |
| 5 | wReborn | Resilient + Summoner (cao) |
| 6 | wSafeguard | Resilient (cao) |
| 8 | tOnDeath | Summoner (cao) |
| 10 | tOnTakeDmg | Resilient (cao) |
| 12 | tOnAttack | Aggressor (cao) |
| 14 | eSummon | Summoner (cao) |
| 18 | sBabylon | Babylon (cao), Niles (thấp) |
| 20 | sNiles | Niles (cao), Babylon (thấp) |
| 24 | wRerollThresh | Aggressor (cao) |
| 27 | wProactiveSell | Summoner (thấp = giữ unit) |
| 34 | tOnAllyGroup | Summoner (cao) |
| 35 | tOnAllyDeploy | Summoner (trung bình–cao) |

Danh sách đầy đủ 37 gene: xem `Assets/Document/00_GDD/08_PhuLuc.md` — Phụ lục B.

---

## Tóm Tắt Checklist Khi Retrain

- [ ] Sửa tham số population/gen (nếu cần) trong `AITrainingBatch.cs` **hoặc** Inspector
- [ ] Sửa seed trong `AITrainingBatch.cs` — **cả 2 chỗ**: init loop + `CreateSeededChromosome`
- [ ] Sửa seed trong `GATrainer.cs` — **cả 2 chỗ**: `BeginTraining` + `CreateSeededChromosome`
- [ ] Xóa hoặc rename `Assets/Resources/AI_Library.json` cũ nếu muốn force retrain từ đầu  
  *(GATrainer.cs tự bỏ qua training nếu file đã hợp lệ)*
- [ ] Chạy `.\train_ai.ps1 -Production` và theo dõi CSV
