# Summary — 2026-05-20 — Cơ chế Frontline/Backline & Targeting

## Phiên làm việc này

### Mục tiêu
Thiết kế lại board cờ 7 slot theo mô hình frontline/backline xen kẽ và cập nhật logic tìm mục tiêu (FindTarget) để khớp với thiết kế mới.

---

## 1. Thiết kế Board mới — Frontline / Backline

### Quy ước slot

| Người chơi nhìn (1-indexed) | 1 | 2 | 3 | 4 | 5 | 6 | 7 |
|------------------------------|---|---|---|---|---|---|---|
| Code index (0-indexed)       | 0 | 1 | 2 | 3 | 4 | 5 | 6 |
| Vai trò                      | F | B | F | B | F | B | F |

- **Frontline (F):** slot 1,3,5,7 → code index **chẵn** 0,2,4,6
- **Backline (B):** slot 2,4,6   → code index **lẻ** 1,3,5

### Quy tắc bảo vệ (Frontline Protection)

Backline chỉ có thể bị tấn công khi **cả 2 frontline kề bên đều trống**:

| Target (1-indexed) | Điều kiện |
|--------------------|-----------|
| Slot 2 (backline)  | Slot 1 VÀ Slot 3 phải trống |
| Slot 4 (backline)  | Slot 3 VÀ Slot 5 phải trống |
| Slot 6 (backline)  | Slot 5 VÀ Slot 7 phải trống |
| Slot 1,3,5,7 (frontline) | Luôn có thể bị tấn công |

**Ngoại lệ:** Unit có **Taunt** kéo toàn bộ fire bất kể vị trí, kể cả khi đang nằm ở backline và được frontline che chắn.

---

## 2. CHANGE — BattlePhaseLayout.cs (Visual Layout)

**File:** `Assets/Scripts/UI/BattlePhaseLayout.cs`

**Vấn đề cũ:** Anchors đặt 7 slot theo dạng "4 slot hàng trên + 3 slot hàng dưới tuần tự" — không phản ánh mô hình frontline/backline xen kẽ.

**Fix:** Anchors được tổ chức lại theo thứ tự index, xen kẽ frontline (y gần địch) và backline (y lùi ra sau, x nằm giữa 2 frontline kề):

```csharp
// Player
private readonly Vector2[] playerCombatAnchors =
{
    new Vector2(0.342f, 0.494f),  // [0] frontline
    new Vector2(0.394f, 0.370f),  // [1] backline  (giữa [0] và [2])
    new Vector2(0.445f, 0.494f),  // [2] frontline
    new Vector2(0.499f, 0.370f),  // [3] backline  (giữa [2] và [4])
    new Vector2(0.552f, 0.494f),  // [4] frontline
    new Vector2(0.607f, 0.370f),  // [5] backline  (giữa [4] và [6])
    new Vector2(0.662f, 0.494f),  // [6] frontline
};
```

Enemy board là mirror Y (frontline y=0.714, backline y=0.846 — gần player hơn là frontline).

---

## 3. CHANGE — CombatResolver.cs (Targeting Logic)

**File:** `Assets/Scripts/Core/CombatResolver.cs`

### 3a. IsAttackableTarget — đã đúng (không đổi)

Logic `IsAttackableTarget` đã được implement đúng từ trước:

```csharp
private bool IsAttackableTarget(List<CardInstance> board, int slot)
{
    if (slot < 0 || slot >= board.Count) return false;
    var unit = board[slot];
    if (unit == null || unit.IsDead) return false;

    // Index chẵn = frontline → luôn bị tấn công được
    if (slot % 2 == 0) return true;

    // Index lẻ = backline → chỉ lộ ra khi cả 2 frontline kề bên trống
    return !IsAlive(board, slot - 1) && !IsAlive(board, slot + 1);
}
```

### 3b. BUG FIX — FindTarget: Taunt không bypass frontline protection

**Vấn đề cũ:**
```csharp
// Chỉ xét Taunt unit đang "lộ ra" (IsAttackableTarget = true)
bool hasTaunt = false;
for (int i = 0; i < board.Count; i++)
{
    if (IsAttackableTarget(board, i) && board[i].isTaunt)  // ← SAI
    {
        hasTaunt = true;
        break;
    }
}
```
→ Taunt unit ở backline đang được frontline che sẽ **không được xét là Taunt** → địch đánh frontline bình thường, bỏ qua Taunt hoàn toàn.

**Fix:**
```csharp
// Taunt bỏ qua bảo vệ frontline — bất kỳ unit Taunt nào còn sống đều kéo toàn bộ fire
bool hasTaunt = board.Exists(u => u != null && !u.IsDead && u.isTaunt);
```

### 3c. Refactor — Tách IsValidTarget helper

`FindTarget` được refactor để tách biệt logic chọn target:

```csharp
private CardInstance FindTarget(List<CardInstance> board, int prefSlot)
{
    bool hasTaunt = board.Exists(u => u != null && !u.IsDead && u.isTaunt);

    for (int d = 0; d < board.Count; d++)
    {
        int left  = prefSlot - d;
        int right = prefSlot + d;

        if (IsValidTarget(board, left,  hasTaunt)) return board[left];
        if (d > 0 && IsValidTarget(board, right, hasTaunt)) return board[right];
    }
    return null;
}

private bool IsValidTarget(List<CardInstance> board, int slot, bool tauntMode)
{
    if (slot < 0 || slot >= board.Count) return false;
    var u = board[slot];
    if (u == null || u.IsDead) return false;
    // Taunt mode: nhắm bất kỳ unit Taunt (bỏ qua frontline shield)
    // Normal mode: chỉ nhắm unit exposed (IsAttackableTarget)
    return tauntMode ? u.isTaunt : IsAttackableTarget(board, slot);
}
```

**Ripple search** vẫn hoạt động như cũ: lan từ `prefSlot` ra 2 bên, ưu tiên gần nhất.

---

## 4. Tóm tắt hành vi targeting hoàn chỉnh

```
FindTarget(board, prefSlot):
  1. Có unit Taunt nào còn sống không?
     └─ Có → Ripple search, nhắm Taunt gần nhất (bỏ qua frontline shield)
     └─ Không → Ripple search, nhắm unit exposed gần nhất
        ├─ Frontline (index chẵn): luôn exposed
        └─ Backline (index lẻ): chỉ exposed nếu cả 2 frontline (i-1, i+1) trống
```

---

## Files đã thay đổi trong phiên này

| File | Thay đổi |
|------|----------|
| `Assets/Scripts/UI/BattlePhaseLayout.cs` | Cập nhật `playerCombatAnchors` và `enemyCombatAnchors` sang layout xen kẽ frontline/backline |
| `Assets/Scripts/Core/CombatResolver.cs` | Sửa `FindTarget` (Taunt bypass frontline), thêm `IsValidTarget` helper |
| `GAME_DESIGN_DOCUMENT.md` | Cập nhật section 10 — Board & Vị trí |
