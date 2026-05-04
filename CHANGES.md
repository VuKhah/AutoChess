# AutoChess — Summary thay đổi

## 1. Fix MagicGroup (int → string)

**Vấn đề:** `CardDefinition.magicGroup` là `MagicGroup` enum. Unity's `JsonUtility` serialize enum ra **integer**, nhưng JSON đã dùng string (`"StatBoost"`, `"AddAbility"`...) → deserialize ra `0` (None) hết → magic không có effect nào chạy được.

**Fix:**
- `CardDefinition.cs`: xóa `enum MagicGroup`, đổi field thành `public string magicGroup`
- `CardSlot.cs`: switch trực tiếp trên string thay vì `.ToString()` (vốn cho ra `"0"`, `"1"`...)

---

## 2. Taunt độc lập với TTE Ability

**Vấn đề:** `isTaunt` nằm trong `AbilityData`. Khi Magic `AddAbility` gán `unit.Data.ability = magic.Data.ability` → toàn bộ ability bị thay, kéo theo Taunt bị mất hoặc bị gán nhầm.

**Fix:**
- `CardInstance.cs`: thêm `public bool isTaunt` (runtime field, không bị reset). Khởi tạo trong constructor từ `data.ability?.isTaunt`.
- `CombatResolver.cs`: `FindTarget()` đọc `u.isTaunt` thay vì `u.Data.ability.isTaunt`.
- `CardSlot.cs`: thêm case `"AddTaunt"` trong `ApplyMagicEffect` → chỉ set `unit.isTaunt = true`, không đụng ability.
- `CardUI.cs`: icon logic dùng `instance.isTaunt` (runtime) thay vì `Data.ability.isTaunt`.
- `CardsData.json`: M_02 "Giao ước Olympus" đổi `magicGroup` từ `"AddAbility"` → `"AddTaunt"`, bỏ ability block thừa.

**Kết quả:** Unit có thể có Taunt + TTE ability độc lập. Gán ability mới không mất Taunt, gán Taunt không mất ability.

---

## 3. Luật Merge (3 quân cùng loại → 1 quân cấp cao)

**Thiết kế:** Thay trên CardInstance, không tạo CardDefinition mới.

**Fix:**
- `CardInstance.cs`:
  - Thêm `public int mergeLevel = 0`
  - `ResetStats()` đổi công thức: `currentATK = baseATK × (mergeLevel + 1) + bonus` (merge 1 lần = x2 stat)
- `CardSlot.cs`: sau mỗi lần đặt/mua quân, gọi coroutine delay 1 frame (chờ `OnEndDrag` chạy xong) rồi:
  - Quét toàn bộ `playerSlots + handSlots` tìm quân cùng `cardID` **và** cùng `mergeLevel`
  - Nếu đủ 3: giữ 1 cái, `Destroy` 2 cái còn lại, tăng `mergeLevel++`, `ResetStats()`, `Setup()` lại UI
  - Tự gọi check tiếp (chain merge nếu vừa tạo ra bộ 3 ở cấp mới)

---

## Bug đã phát hiện (chưa fix)

`U_02` (Hộ vệ Gai — Thorns) trong `CardsData.json` có `"effect": 3` nhưng `EffectType.DealDamage = 2` (giá trị 3 không tồn tại trong enum). Thorns không gây sát thương phản đòn. Sửa: đổi `"effect": 3` → `"effect": 2`.
