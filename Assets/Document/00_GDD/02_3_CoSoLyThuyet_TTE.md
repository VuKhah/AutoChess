## 2.3 Mô Hình Trigger → Target → Effect (TTE)

### 2.3.1 Bài Toán Thiết Kế: Hành Vi Phức Tạp Từ Tập Dữ Liệu Nhỏ

Một trong những thách thức thiết kế căn bản nhất của bất kỳ card game hay auto-battler nào là câu hỏi: *làm thế nào để 68 lá bài có hành vi phong phú và khác biệt mà không biến codebase thành rừng rậm của những lớp con, những hàm đặc biệt và những điều kiện hardcode?*

Cách tiếp cận ngây thơ nhất là viết một lớp riêng cho từng lá bài:

```csharp
// Cách BAD — không bao giờ làm thế này
public class Gilgamesh : CardBase {
    public override void OnStatGain() { /* logic riêng */ }
}
public class Sekhmet : CardBase {
    public override void OnAllySummon() { /* logic riêng */ }
}
public class Osiris : CardBase {
    public override void OnAttack() { /* logic riêng */ }
}
// ... 65 lớp tiếp theo
```

Cách này không chỉ tạo ra 68 file C# mà còn gây ra vấn đề sâu hơn: mỗi lần thêm lá bài mới, nhà thiết kế phải nhờ lập trình viên viết code mới, biên dịch lại project, và kiểm thử lại. Cân bằng một lá bài (ví dụ: đổi giá trị buff từ 2 thành 3) đòi hỏi sửa code C#, không phải sửa dữ liệu. Đây là kiến trúc tạo ra vòng lặp phát triển chậm và phụ thuộc chặt giữa thiết kế và kỹ thuật.

Giải pháp tốt hơn đến từ quan sát rằng dù các lá bài có vẻ rất khác nhau về mặt narrative ("Horus báo thù khi đồng minh chết", "Anubis triệu hồi khi tấn công", "Thoth tăng chỉ số toàn đội mỗi lượt"), tất cả chúng đều có thể được mô tả bởi cùng một cấu trúc ba thành phần: **điều gì kích hoạt** khả năng đó, **ai là mục tiêu** của nó, và **điều gì xảy ra** với mục tiêu đó. Đây chính là nền tảng của mô hình **Trigger → Target → Effect**.

---

### 2.3.2 Cấu Trúc Ba Chiều và Không Gian Tổ Hợp

Mô hình TTE biểu diễn mỗi ability của một lá bài bằng ba trường độc lập trong struct `AbilityData`:

```csharp
// AbilityData.cs
[System.Serializable]
public class AbilityData
{
    public TriggerType trigger;   // KHI NÀO kích hoạt?
    public TargetType  target;    // NHẮM VÀO AI?
    public EffectType  effect;    // LÀM GÌ?

    public int effectValue1;      // tham số 1 của effect
    public int effectValue2;      // tham số 2 của effect
    // ... các modifier bổ sung
}
```

Mỗi chiều là một enum độc lập với tập giá trị hữu hạn. **TriggerType** định nghĩa 14 sự kiện có thể kích hoạt ability: từ những sự kiện trong combat (`OnAttack`, `OnTakeDamage`, `OnDeath`, `StartOfBattle`) đến những sự kiện trong shop phase (`OnDeploy`, `OnSell`, `EndTurnShop`) và những sự kiện phản ứng với đồng minh (`OnAllyDeath`, `OnAllySummon`, `OnAllyReborn`, `OnAllyDeploy`, `OnAllySell`). **TargetType** định nghĩa 12 cách chọn mục tiêu: từ đơn giản như `Self` (bản thân) đến phức tạp như `TriggerSubject` (unit vừa gây ra sự kiện — dùng trong OnAllyDeath để nhắm vào chính unit vừa chết), hay `LowestHealthAlly` (đồng minh có HP thấp nhất). **EffectType** định nghĩa 13 loại hành động: `AddStats`, `DealDamage`, `GiveBuff`, `Summon`, `Destroy`, `Reborn`, `AbsorbStats`, `ScaleTargetStats`...

Sức mạnh của thiết kế này nằm ở tính tổ hợp: với 14 × 12 × 13 = 2.184 tổ hợp cơ bản, cộng thêm các modifier như `isPermanent`, `triggerLimit`, `conditionCount`, `isEscalating`... không gian hành vi có thể biểu diễn là hàng chục nghìn khả năng khác nhau — tất cả chỉ từ dữ liệu JSON, không cần một dòng code mới. Đây là điều mà một lập trình viên senior thường gọi là **data-driven design**: logic của hệ thống được mã hóa trong engine một lần, còn nội dung (content) được định nghĩa hoàn toàn qua dữ liệu.

> **[HÌNH 2.10 — Không Gian Tổ Hợp TTE]** *Hình khối lập phương 3D với ba trục: Trigger (14 giá trị), Target (12 giá trị), Effect (13 giá trị). Mỗi ô trong khối là một tổ hợp ability có thể. Một vài ô được tô màu và gán nhãn ví dụ cụ thể (ví dụ: OnDeath × Self × AddStats = Horus tăng stats khi chết).*

---

### 2.3.3 Luồng Thực Thi — Từ Sự Kiện Đến Hiệu Ứng

Khi một sự kiện xảy ra trong game — một unit tấn công, một đồng minh chết, một turn kết thúc — engine gọi `TriggerAbility()` với ngữ cảnh tương ứng. Hàm này là trái tim của TTE engine, thực hiện một chuỗi kiểm tra và quyết định có thứ tự chặt chẽ:

```
TriggerAbility(triggerContext, source, directEnemy, allyBoard, enemyBoard)
    │
    ├─ [1] source có abilities không? → nếu không, thoát ngay
    │
    ├─ [2] Với mỗi ability trong source.abilities:
    │       ├─ ability.trigger == triggerContext?    → nếu không, bỏ qua
    │       ├─ Đã đạt triggerLimit chưa?             → nếu rồi, bỏ qua
    │       ├─ conditionCount: đây có phải lần thứ N?→ nếu không, bỏ qua
    │       ├─ subjectTribe filter hợp lệ?           → nếu không, bỏ qua
    │       ├─ isConsume trong shop phase?           → block nếu cần
    │       │
    │       ├─ FindTargets(ability, source, ...) → List<CardInstance>
    │       │
    │       ├─ triggerLimit > 0 VÀ targets rỗng? → không tính lần kích hoạt
    │       │
    │       └─ Với mỗi target:
    │               isGrowth? → ApplyGrowth()
    │               else      → ExecuteEffect()
    │
    └─ isEscalating? → tăng escalationBonus cho lần sau
```

> **[HÌNH 2.11 — Luồng Thực Thi TriggerAbility]** *Flowchart chi tiết của hàm TriggerAbility(): bắt đầu từ "source có abilities?" → duyệt từng ability → kiểm tra trigger match → kiểm tra limit/condition → FindTargets → ExecuteEffect. Các nhánh thoát sớm (skip) được tô màu xám, nhánh thực thi được tô màu xanh.*

Thiết kế này có một chi tiết tinh tế quan trọng trong bước kiểm tra `triggerLimit`: **chỉ đếm lần kích hoạt khi thực sự có mục tiêu hợp lệ**. Quyết định này giải quyết một edge case cụ thể trong game: nếu hai unit Sekhmet cùng muốn "nuốt" một unit đang triệu hồi, unit Sekhmet đầu tiên sẽ nuốt thành công, unit thứ hai sẽ không tìm được mục tiêu (đã bị nuốt rồi). Nếu đếm lần kích hoạt bất kể có mục tiêu hay không, Sekhmet thứ hai sẽ "lãng phí" một lần sử dụng dù không làm được gì — hành vi sai về mặt thiết kế game. Bằng cách gắn việc đếm vào sự tồn tại của mục tiêu, engine đảm bảo `triggerLimit` phản ánh đúng số lần ability *thực sự có tác dụng*.

---

### 2.3.4 Chiều Thứ Nhất — Trigger: Lắng Nghe Thế Giới Game

14 loại trigger không phải là danh sách tùy tiện mà phản ánh một phân loại có hệ thống của mọi "khoảnh khắc đáng quan tâm" trong vòng lặp game. Chúng chia thành bốn nhóm tự nhiên:

**Nhóm combat cá nhân** — phản ứng với hành động của chính unit đó trong trận đấu:
- `OnAttack`: kích hoạt sau khi unit tấn công và còn sống
- `OnTakeDamage`: kích hoạt khi unit nhận damage và còn sống
- `OnDeath`: kích hoạt khi unit chết (qua death stack, không phải ngay lập tức)
- `StartOfBattle`: kích hoạt một lần ở đầu trận

**Nhóm shop phase** — phản ứng với hành động kinh tế:
- `OnDeploy`: kích hoạt khi unit được kéo từ tay lên sân
- `OnSell`: kích hoạt khi unit bị bán
- `EndTurnShop`: kích hoạt đầu lượt tiếp theo với mọi unit đang trên sân

**Nhóm phản ứng đồng minh** — kích hoạt khi đồng minh làm gì đó:
- `OnAllyDeath`: khi một đồng minh (cùng bộ tộc, trừ khi `anyAllyTrigger=true`) chết
- `OnAllySummon`: khi một đồng minh được triệu hồi trong combat
- `OnAllyReborn`: khi một đồng minh hồi sinh qua Reborn
- `OnAllyDeploy`: khi đồng minh được deploy lên sân trong shop phase
- `OnAllySell`: khi đồng minh bị bán trong shop phase

**Nhóm đặc biệt** — hai trigger không theo quy luật thông thường:
- `Aura`: kích hoạt một lần ở đầu trận, nhưng nhắm vào `AllAlliesExceptSelf` — dùng để tạo hiệu ứng passive buff toàn đội
- `OnStatGain`: kích hoạt khi unit nhận chỉ số *vĩnh viễn* từ `AddStats(isPermanent)`, `AbsorbStats`, hoặc `GiveStats` — một trigger meta phản ứng với sự kiện bên trong hệ thống ability

`OnStatGain` là trigger thú vị nhất từ góc độ kỹ thuật vì nó tạo ra khả năng "chain reaction": unit nhận buff vĩnh viễn → trigger `OnStatGain` → kích hoạt một ability khác → ability đó có thể cộng thêm stats cho unit khác → nếu unit đó cũng có `OnStatGain`... Để ngăn vòng đệ quy vô hạn khi một unit phản ứng với chính buff của nó (Gilgamesh tăng stats → OnStatGain của Gilgamesh → lại tăng stats → lại OnStatGain...), engine dùng một flag boolean:

```csharp
// AbilityEngine.cs
private bool _firingOnStatGain = false;

// Khi AddStats permanent:
if (!_firingOnStatGain)
{
    _firingOnStatGain = true;
    try { TriggerAbility(TriggerType.OnStatGain, target, ...); }
    finally { _firingOnStatGain = false; }
}
```

Pattern `try/finally` đảm bảo flag luôn được reset ngay cả khi có exception — đây là kỹ thuật bảo vệ guard condition chuẩn trong lập trình hệ thống.

> **[HÌNH 2.12 — Phân Loại 14 Trigger Types]** *Bảng phân nhóm 14 trigger theo 4 nhóm màu sắc: Combat cá nhân (OnAttack, OnTakeDamage, OnDeath, StartOfBattle), Shop phase (OnDeploy, OnSell, EndTurnShop), Phản ứng đồng minh (OnAllyDeath, OnAllySummon, OnAllyReborn, OnAllyDeploy, OnAllySell), Đặc biệt (Aura, OnStatGain). Kèm ví dụ card tiêu biểu cho mỗi trigger.*

---

### 2.3.5 Chiều Thứ Hai — Target: Ngôn Ngữ Của Sự Nhắm Mục Tiêu

Sau khi trigger xác định "khi nào", target xác định "nhắm vào ai". Hệ thống target được thiết kế để bao phủ tất cả các mẫu nhắm mục tiêu có ý nghĩa trong game mà không cần viết logic tùy chỉnh cho từng lá bài.

Hai target type thú vị nhất về mặt kỹ thuật là `TriggerSubject` và `LowestHealthAlly`. `TriggerSubject` là một con trỏ đến "unit gây ra sự kiện hiện tại" — khi `OnAllyDeath` kích hoạt, `TriggerSubject` là unit vừa chết; khi `OnAllySummon` kích hoạt, `TriggerSubject` là unit vừa được triệu hồi. Điều này cho phép viết những ability như "khi đồng minh bị triệu hồi, buff chính unit đó" — một mẫu hành vi rất phổ biến trong game Auto Chess.

Hệ thống cũng hỗ trợ lọc mục tiêu theo bộ tộc qua trường `targetTribe`: khi giá trị này khác 0, engine chỉ chọn các unit thuộc bộ tộc đó trong pool mục tiêu. Ví dụ, một ability có `target = AllAllies` và `targetTribe = 1 (Babylon)` sẽ buff tất cả unit Babylon trên sân, không buff các bộ tộc khác. Đây là cách toàn bộ tribe synergy bonus được cài đặt mà không cần viết code riêng cho từng bộ tộc.

Quá trình tìm mục tiêu được tách ra trong `AbilityEngine.Targets.cs` (partial class), một quyết định tổ chức code có chủ ý: logic tìm mục tiêu (ai là `LowestHealthAlly`?) và logic thực thi effect (buff target đó như thế nào?) là hai concerns khác nhau, cần tách biệt để dễ đọc và kiểm thử.

---

### 2.3.6 Chiều Thứ Ba — Effect: Không Gian Hành Động

13 loại effect bao phủ toàn bộ những gì một unit có thể "làm" với target của nó. Một số effect đáng chú ý vì độ phức tạp của chúng:

**`AddStats` với Growth mechanic:** Khi `trigger = StartOfBattle` và `effect = AddStats`, engine nhận diện đây là **Growth** — một trường hợp đặc biệt quan trọng. Thay vì chỉ cộng vào `currentATK/HP` (mất sau trận), giá trị được tích lũy vào `growthATKBonus`/`growthHPBonus` — các trường tồn tại xuyên suốt nhiều lượt:

```csharp
// ApplyGrowth() trong AbilityEngine.cs
target.growthATKBonus += atkGain;
target.growthHPBonus  += hpGain;
target.currentATK     += atkGain;   // có hiệu lực ngay trong trận hiện tại
target.currentHP      += hpGain;
```

Công thức `ResetStats()` sau mỗi lượt tính lại chỉ số từ base và giữ nguyên `growthBonus`: `currentATK = baseATK × tier + 0.7 × (growthATKBonus + permanentATKBonus)`. Kết quả là những unit có trigger StartOfBattle + AddStats trở nên mạnh dần theo thời gian — một mechanic "snowball" tạo ra sức căng chiến lược: phải giữ unit đó sống sót qua nhiều lượt để nó đạt được tiềm năng tối đa.

**`Destroy` với `isConsume`:** Effect `Destroy` thông thường chỉ đặt HP = 0 của target và trigger death stack. Khi kết hợp với flag `isConsume`, nó còn lưu `cardID` của unit bị tiêu diệt vào danh sách `source.consumedCardIDs`. Danh sách này được dùng bởi effect `SummonConsumed` sau đó — khi source chết, tất cả unit đã bị nó "nuốt" được triệu hồi lại. Đây là mechanic của Sekhmet: ăn unit đồng minh trong combat, khi Sekhmet chết, giải phóng tất cả chúng ra cùng lúc.

Tuy nhiên, `isConsume` có một ngoại lệ quan trọng: nếu target còn Reborn chưa dùng và đang trong combat, unit đó *chưa thực sự chết* — nó sẽ hồi sinh. Nếu tính là "đã consume" ngay lúc đó, khi Reborn kích hoạt và unit hồi sinh, Sekhmet sẽ phản ứng lần nữa với `OnAllyReborn` và có thể nuốt lại — tạo ra double-consume không hợp lý. Engine giải quyết bằng cách delay việc ghi nhận consume đến *sau* khi Reborn được xử lý:

```csharp
// AbilityEngine.cs — case EffectType.Destroy
bool skipForPendingReborn = _isCombatPhase && target.isReborn && !target.hasRebornUsed;
if (!skipForPendingReborn)
    source.consumedCardIDs.Add(target.Data.cardID);
```

**`copiesAbilitiesOnConsume`:** Đây là flag mở rộng khả năng của `Destroy+isConsume` lên một bậc. Khi bật, source không chỉ "nuốt" unit mà còn **sao chép toàn bộ ability** của unit đó vào danh sách ability của mình. Đây là mechanic của Upamaki — con quái vật thần thoại Ai Cập nuốt chửng linh hồn và tiếp thu sức mạnh của chúng. Về mặt kỹ thuật, những ability được sao chép này là *runtime mutation* của `CardDefinition.abilities` — dữ liệu được thêm vào trong khi game đang chạy, nằm ngoài JSON tĩnh.

**`ScaleTargetStats`:** Effect này nhân chỉ số hiện tại của target với một hệ số: `newATK = currentATK + currentATK × effectValue1 × scaleFactor`. Vì `scaleFactor = mergeLevel + 1`, một unit đã merge nhiều lần sẽ nhận được buff khổng lồ. Đây là cách Osiris được thiết kế — unit có ATK/HP thấp (3/3) nhưng khi một đồng minh hồi sinh (`OnAllyReborn`), Osiris nhân đôi/ba/bốn chỉ số của unit đó tùy mergeLevel. Kết hợp với Anubis grant Reborn, một unit trung bình có thể trở thành con quái vật trong một round mà không cần merge Osiris lên cao.

---

### 2.3.7 Modifier — Không Gian Thứ Tư

Ngoài ba chiều cốt lõi, `AbilityData` có một tập modifier làm giàu thêm không gian hành vi mà không cần thêm enum value mới:

**`conditionCount`:** Chỉ kích hoạt vào lần thứ N, 2N, 3N... (0 = mọi lần). Cho phép thiết kế ability "mỗi lần thứ 3 tấn công, gây damage kép" mà không cần state machine phức tạp — engine chỉ cần kiểm tra `triggerCount % conditionCount == 0`.

**`isEscalating`:** Sau mỗi lần kích hoạt, `effectValue1` và `effectValue2` tăng thêm 1 (nhân scaleFactor). Mechanic này tạo ra unit "tích lũy momentum" — lần đầu buff +1, lần hai buff +2, lần ba buff +3... Engine theo dõi giá trị này trong `source.abilityEscalationBonuses[i]` thay vì mutate `AbilityData` gốc, đảm bảo nhiều instance của cùng một lá bài không ảnh hưởng lẫn nhau.

**`isScaledTriggerLimit`:** Thay vì giới hạn cứng, giới hạn kích hoạt tỉ lệ thuận với mergeLevel: `effectiveLimit = triggerLimit × (mergeLevel + 1)`. Điều này tự nhiên tạo ra sự cân bằng: unit merge nhiều hơn không chỉ mạnh hơn về chỉ số mà còn có thể dùng ability nhiều lần hơn, thưởng cho người chơi đầu tư vào merge.

**`globalTribeBuff`:** Một bypass hoàn toàn hệ thống target thông thường. Thay vì nhắm vào các unit hiện tại trên board, effect được áp dụng cho tất cả unit cùng tribe — kể cả trong tay, trong shop, và thậm chí mọi unit sẽ được mua trong tương lai. Engine thực hiện điều này qua hai bước: cập nhật `_globalTribeATKBonus[]`/`_globalTribeHPBonus[]` trong `GameManager`, và gọi `ApplyGlobalPermBuffToNewUnit()` mỗi khi một `CardInstance` mới được tạo ra. Đây là mechanic của Thoth — và cũng là lý do vì sao người chơi cảm thấy "tribe Babylon càng nhiều unit, mỗi unit càng mạnh hơn theo thời gian".

---

### 2.3.8 Vấn Đề Phức Tạp Nhất: Ordering và Reentrancy

Hệ thống TTE trở nên thực sự phức tạp khi các ability chain với nhau — khi effect của ability A kích hoạt trigger của ability B, và B kích hoạt C. Hai vấn đề kỹ thuật quan trọng xuất hiện.

**Vấn đề ordering** trong `BroadcastAllyEvent()` — hàm lan sự kiện đến tất cả đồng minh. Nếu trong khi broadcast, một unit bị triệu hồi (từ một ability phản ứng với sự kiện đó) và unit mới này cũng ngay lập tức nhận được event, có thể tạo ra hành vi không nhất quán — unit "được thông báo về sự kiện xảy ra trước khi nó tồn tại". Engine giải quyết bằng **snapshot trước khi broadcast**:

```csharp
public void BroadcastAllyEvent(TriggerType context, CardInstance subject,
    List<CardInstance> allyBoard, List<CardInstance> enemyBoard)
{
    var snapshot = new List<CardInstance>(allyBoard);   // chụp trạng thái hiện tại
    foreach (var unit in snapshot)                       // lặp qua bản sao, không qua list gốc
    {
        if (unit == null || unit.IsDead || unit == subject) continue;
        TriggerAbility(context, unit, subject, allyBoard, enemyBoard);
    }
}
```

`snapshot` là bản sao của `allyBoard` tại thời điểm broadcast bắt đầu. Mọi unit được summon *trong quá trình* broadcast sẽ được thêm vào `allyBoard` gốc (ảnh hưởng đến trận đấu) nhưng không xuất hiện trong `snapshot` (không nhận event này). Đây là sự đánh đổi thiết kế có chủ ý: tính nhất quán được ưu tiên hơn tính "phản ứng ngay lập tức".

**Vấn đề pending summon** xuất hiện với Summon chain: nếu unit A summon unit B khi chết, và unit B khi xuất hiện kích hoạt `OnDeploy` để summon unit C, và C lại summon D... Nếu mọi summon được thực hiện ngay lập tức và đệ quy, death stack của unit ban đầu có thể chưa được resolve khi C và D đã xuất hiện và cũng bắt đầu die — tạo ra một chuỗi đệ quy không có điểm dừng rõ ràng. Engine giải quyết bằng **pending summon queue**:

```csharp
// Summon đầu tiên trong batch: thực hiện ngay
CardInstance first = SummonUnit(ability.summonCardID, allyBoard);
TriggerAbility(TriggerType.OnDeploy, first, null, allyBoard, enemyBoard);
BroadcastAllyEvent(TriggerType.OnAllySummon, first, allyBoard, enemyBoard);

// Các summon còn lại: xếp vào queue, đợi death stack xử lý xong
for (int s = 1; s < summonCount; s++)
    _pendingSummons.Enqueue(new PendingSummonEntry { cardID = ..., allyBoard = ..., enemyBoard = ... });
```

`CombatResolver.FlushDeathStack()` sẽ gọi `ProcessNextPendingSummon()` sau khi mỗi death event được xử lý xong, đảm bảo thứ tự: **tất cả hậu quả của một cái chết được resolve hoàn toàn trước khi unit tiếp theo xuất hiện**. Đây là điểm giao thoa quan trọng giữa TTE engine và Death Stack của `CombatResolver` — hai hệ thống khác nhau nhưng phải phối hợp chặt chẽ để game hoạt động đúng.

> **[HÌNH 2.13 — Pending Summon Queue và Death Stack]** *Sơ đồ trình tự (sequence diagram) minh họa chuỗi: unit A chết → FlushDeathStack pop A → TriggerOnDeath → Summon unit B (ngay) + Summon unit C (vào queue) → BroadcastOnAllyDeath → CleanupBoard → ProcessNextPendingSummon pop C → TriggerOnDeploy C. Thể hiện rõ thứ tự và sự phối hợp giữa hai cấu trúc dữ liệu.*

---

### 2.3.9 TTE Trong Shop Phase Và Combat Phase

Một trong những thiết kế khôn khéo nhất của TTE là khả năng **phase-awareness** — cùng một ability có thể hành xử khác nhau tùy theo game đang ở phase nào, mà không cần hai ability riêng biệt.

Ví dụ rõ nhất là `Summon` effect. Trong combat phase, unit được triệu hồi thẳng lên board để tham chiến ngay lập tức. Trong shop phase, "triệu hồi" một unit đang không có board chiến đấu không có nghĩa — thay vào đó engine tự động chuyển hướng: unit được thêm vào Hand của người chơi để dùng ở lượt tiếp theo:

```csharp
// AbilityEngine.cs — ExecuteEffect, case EffectType.Summon
bool isShopPhase = GameManager.Instance != null && !GameManager.Instance.isCombatActive;

if (isShopPhase)
    GameManager.Instance.AddUnitToHand(toAdd);   // vào Hand
else
    CardInstance first = SummonUnit(ability.summonCardID, allyBoard);   // vào Board
```

Tương tự, `OnAllySummon + isConsume` (Sekhmet nuốt unit vừa được triệu hồi) chỉ được phép trong combat phase — trong shop phase, cho phép Sekhmet nuốt unit của chính người chơi ngay khi deploy sẽ là mechanic gây confusion, không phải design intent:

```csharp
if (ability.trigger == TriggerType.OnAllySummon && ability.isConsume && !_isCombatPhase) continue;
```

Phase-awareness này cho phép một lá bài như Ninurta có ability `OnDeploy → Self → Summon` tạo ra một unit mới vào tay người chơi khi được deploy trong shop phase, nhưng summoner thẳng ra sân trong combat phase — hai hành vi khác nhau về mặt UX nhưng cùng một định nghĩa ability trong JSON.

---

### 2.3.10 Tác Động Đến Thiết Kế AI

Mô hình TTE không chỉ quan trọng với gameplay mà còn ảnh hưởng trực tiếp đến cách hệ thống AI trong Chương 5 đánh giá một lá bài. Chromosome của bot có 6 gene dành riêng cho trigger weights (gene[7–12] và gene[32–36]) và 5 gene cho effect weights (gene[13–17]). Công thức đánh giá một lá bài bao gồm:

```
abilityScore = Σ TriggerWeight(ability.trigger) × EffectWeight(ability.effect) × 10
```

TTE làm cho việc này có thể: vì mọi ability đều có trigger và effect rõ ràng dưới dạng enum, bot có thể áp dụng cùng công thức đánh giá cho tất cả 68 lá bài mà không cần biết gì về semantic cụ thể của từng lá. Một bot có `gene[8]` (tOnDeath) cao sẽ đánh giá cao mọi unit có trigger OnDeath, dù đó là Horus (tăng stats) hay Sekhmet (summon chain) hay Osiris (deal damage) — chúng đều cùng trigger, chỉ khác effect và target. Đây là sự liên kết sâu giữa hai hệ thống cốt lõi của dự án: **TTE định nghĩa ngôn ngữ của lá bài; Chromosome học cách đánh giá ngôn ngữ đó**.

Phần tiếp theo (2.4) sẽ trình bày cơ sở lý thuyết cuối cùng: kinh tế học trong Auto Chess — hệ thống cân bằng giữa chi tiêu ngắn hạn và tích lũy dài hạn mà cả người chơi lẫn bot AI phải giải quyết mỗi lượt.

---

*[Tiếp theo: Mục 2.4 — Lý thuyết kinh tế trong Auto Chess]*
