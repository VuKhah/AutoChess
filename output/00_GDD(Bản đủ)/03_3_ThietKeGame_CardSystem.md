## 3.3 Hệ Thống Bài (Card System)

Bài (card) là đơn vị nội dung cơ bản của toàn bộ game — mọi quyết định trong Shop Phase, mọi kết quả trong Combat Phase đều quy về việc người chơi đang sở hữu những lá bài nào và đặt chúng ở đâu. Hệ thống bài được thiết kế theo nguyên tắc **tổ hợp theo tầng**: sức mạnh của một lá bài không đến từ một thuộc tính duy nhất mà từ sự cộng hưởng của nhiều lớp — chỉ số cơ bản, tribe synergy, passive keywords, ability, và mức merge tích lũy. Mỗi mục con dưới đây mô tả một lớp trong tầng đó, theo thứ tự từ nền tảng đến phức tạp.

---

### 3.3.1 Hai Loại Bài — Unit và Spell

Tập 68 lá bài trong game được phân thành hai loại (`CardType`) với vai trò hoàn toàn khác nhau trong vòng lặp gameplay.

**Unit card** là loại bài chủ yếu — chiếm 47 trong tổng 68 lá. Unit được đặt lên sân (7 slot), tham chiến trong Combat Phase, và là đối tượng của hầu hết mọi cơ chế trong game. Mỗi unit có bộ thuộc tính tĩnh (static) được định nghĩa trong JSON:

| Thuộc tính | Kiểu dữ liệu | Ý nghĩa |
|------------|-------------|---------|
| `cardID` | string | Khóa định danh duy nhất (VD: `U_11_Babylon`) |
| `cardName` | string | Tên hiển thị (VD: *Gilgamesh*) |
| `tribe` | enum | Bộ tộc: None / Babylon / Olympus / Niles |
| `baseATK` | int | Sát thương gây ra mỗi đòn tấn công (trước buff) |
| `baseHP` | int | Máu tối đa (trước buff) |
| `cost` | int | Coin cần trả để mua trong shop |
| `tier` | int (1–6) | Cấp độ card trong pool — xác định lượt nào card này phổ biến trong shop |
| `hasTaunt` | bool | Unit có keyword Taunt không |
| `hasReborn` | bool | Unit có keyword Reborn không |
| `hasSafeguard` | bool | Unit có keyword Safeguard không |
| `abilities[]` | AbilityData[] | Danh sách kỹ năng theo mô hình TTE |
| `isToken` | bool | Token: không xuất hiện trong shop, không được spell chọn ngẫu nhiên |

> **[HÌNH 3.6 — Giải Phẫu Lá Bài Unit]** *Ảnh chụp một lá bài unit trong game (ví dụ: Gilgamesh) với 8 thành phần được đánh số và mũi tên chú thích: (1) Tên, (2) Tier icon, (3) Character art, (4) Ability icon + keywords, (5) ATK, (6) HP, (7) Merge stars, (8) Description text.*

**Spell card** là 21 lá còn lại. Spell không có ATK/HP, không chiếm slot sân, và không tham chiến. Mua xong là sử dụng ngay — tác động một lần rồi biến mất. Spell tạo ra tầng kinh tế thứ hai trong game: thay vì mua unit mới, người chơi có thể đầu tư vào công cụ buff đội hình, kiếm coin, hoặc thao túng shop theo những cách mà mua unit không thể làm được.

Sự phân chia Unit/Spell tạo ra một quyết định thường xuyên trong shop: mỗi lượt, 2 trong 7 ô shop là spell — và người chơi phải cân nhắc liệu 1–2 coin bỏ ra cho spell có đáng hơn dùng để mua unit hay reroll.

---

### 3.3.2 Hệ Thống Bộ Tộc (Tribe Synergy)

Ba bộ tộc — Babylon, Olympus, Niles — không chỉ là nhãn phân loại; chúng định nghĩa ba **triết lý cộng sinh** khác nhau thể hiện qua cách các unit trong cùng bộ tộc tương tác với nhau qua ability.

**Thiết kế nguyên tắc chung:** Synergy trong game này là *emergent* (tự nổi lên) thay vì *threshold* (ngưỡng kích hoạt cứng). Không có cơ chế "có đủ 3 unit Babylon → kích hoạt hiệu ứng X" như trong Teamfight Tactics. Thay vào đó, synergy được xây dựng từ các ability của từng card: nhiều unit cùng bộ tộc trên sân → nhiều trigger cùng bộ tộc xảy ra → nhiều buff tích lũy hơn. Sức mạnh tribe tỉ lệ thuận mềm với số lượng unit, không phải theo bước nhảy cứng.

**Bộ tộc Babylon — Cộng sinh qua vòng kinh tế:**
Babylon unit buff lẫn nhau khi có sự kiện kinh tế xảy ra — deploy, sell — tạo ra vòng lặp: đặt lên sân → buff, bán → buff. Ví dụ tiêu biểu:

- *Utu* (Tier 6): mỗi khi một đồng minh Babylon được deploy, toàn bộ đồng minh Babylon nhận vĩnh viễn +2/+2
- *Ashur* (Tier 5): mỗi khi bất kỳ đồng minh bị bán, toàn bộ đồng minh nhận vĩnh viễn +1/+2
- *Lamashtu* / *Uridimmu* (Tier 3/2): buff ATK/HP toàn Babylon mỗi khi đồng minh Babylon bị bán

Kết quả thực tế: Babylon deck muốn có nhiều unit cùng tộc để nhân số lần trigger. Mỗi unit được deploy là một làn sóng buff; mỗi bán đi cũng tạo buff. Chiến lược Babylon điển hình là *snowball* dài hạn — yếu ở đầu game nhưng trở nên khó chặn khi đã tích lũy đủ trigger qua nhiều lượt.

**Bộ tộc Niles — Cộng sinh qua cái chết và tái sinh:**
Niles unit buff lẫn nhau qua các sự kiện chiến đấu — summon, reborn, death. Chu trình cốt lõi:

- *Anubis* (Tier 6): khi đồng minh chết → ban Reborn cho đồng minh có HP thấp nhất (tối đa 2 lần)
- *Osiris* (Tier 6): khi đồng minh hồi sinh (Reborn) → nhân đôi/ba/bốn chỉ số unit đó
- *Sobek* (Tier 5): mỗi khi đồng minh được triệu hồi hoặc hồi sinh → bản thân nhận vĩnh viễn +1/+2
- *Isis* (Tier 5): khi đồng minh hồi sinh → toàn đội nhận vĩnh viễn +3/+3
- *Thoth* (Tier 5, Reborn): khi bị tiêu diệt → toàn bộ unit Niles (board/hand/shop/future) nhận vĩnh viễn +2 ATK — hiệu ứng tích lũy qua các lượt và áp dụng cho unit mua sau

Điểm đặc biệt của Thoth là sử dụng cờ `globalTribeBuff` trong AbilityData — một bypass hoàn toàn hệ thống target thông thường, áp dụng buff không chỉ lên unit đang tồn tại mà còn được ghi nhớ và áp tự động cho mọi unit Niles được tạo ra sau đó (bao gồm cả unit trong shop và unit mua tương lai). Kết hợp với Reborn của chính Thoth, mỗi ván đấu Thoth có thể chết hai lần — mỗi lần buff toàn bộ tộc Niles thêm +2 ATK tích lũy.

Chiến lược Niles điển hình là *chaos resilience* — nhìn bề ngoài đội hình dễ chết, nhưng thực ra mỗi cái chết kích hoạt một chuỗi buff và hồi sinh khiến đội hình mạnh dần theo từng lượt chiến đấu.

**Bộ tộc Olympus — Cộng sinh qua sức mạnh thuần túy *(thiết kế dự kiến)*:**
Olympus được thiết kế là bộ tộc tập trung vào ATK buff và áp lực tấn công. Trong khi Babylon và Niles có những chuỗi trigger phức tạp, Olympus hướng đến sức mạnh trực tiếp hơn — unit Olympus buffs nhau qua combat events (tấn công, gây sát thương, chiến thắng trận đấu). Đây là bộ tộc cho người chơi ưa chiến lược aggro: xây dựng đội hình tấn công cao và áp đảo đối thủ trước khi các combo phức tạp của Babylon hay Niles có thể triển khai đầy đủ.

> *Lưu ý về phạm vi triển khai: Trong phiên bản hiện tại (68 card), không có unit nào có `tribe = Olympus`. Gene `genes[19]` (sOlympus) trong chromosome bot được giữ lại trong kiến trúc để đơn giản hóa mở rộng về sau, nhưng không có tác dụng thực tế trong training. Thảo luận thêm ở Mục 6.5.*

**Tại sao không dùng threshold cứng?**
Quyết định không hardcode "3 unit = synergy kích hoạt" là có chủ ý. Threshold cứng tạo ra các "điểm gãy" rõ ràng trong giá trị của từng unit — đơn vị thứ 3 đột nhiên mạnh hơn hẳn đơn vị thứ 2. Mô hình emergent tránh hiện tượng này: thêm unit thứ 4, thứ 5 vẫn có giá trị (thêm trigger), và việc "mất một unit" không làm mất đột ngột toàn bộ synergy. Điều này tạo ra quyết định trade-off linh hoạt hơn thay vì binary "đủ/chưa đủ ngưỡng".

> **[HÌNH 3.7 — So Sánh Ba Triết Lý Tribe Synergy]** *Bảng 3 cột so sánh Babylon / Niles / Olympus theo các tiêu chí: cơ chế trigger chính, giai đoạn mạnh nhất (early/mid/late), rủi ro chiến lược, unit tiêu biểu. Kèm biểu tượng màu sắc riêng mỗi tộc.*

---

### 3.3.3 Passive Keywords

Ba keyword passive trong game định nghĩa các "tính chất tồn tại" của unit — hành vi mặc định không cần trigger, tồn tại từ đầu đến hết combat trừ khi bị tiêu thụ. Chúng không phải là ability (không có trigger/effect) mà là trạng thái boolean được khởi tạo từ `CardDefinition` vào `CardInstance` tại mỗi đầu combat.

**Taunt — Buộc Phải Bị Nhắm:**
Unit có Taunt bắt buộc kẻ địch phải tấn công nó trước tất cả unit không có Taunt. Trong `CombatResolver.FindTarget()`, logic kiểm tra: nếu tồn tại unit nào có `isTaunt = true` trên sân địch, đó là mục tiêu bắt buộc — bất kể vị trí slot hay HP còn lại.

Taunt là keyword phòng thủ mạnh nhất trong game vì nó kiểm soát hoàn toàn luồng damage của đối thủ. Đặt một unit có HP cao với Taunt ở frontline có thể giữ toàn bộ đội hình backline (các unit sát thương cao, unit có ability quan trọng) sống sót qua nhiều round.

```
FindTarget logic:
  1. Kiểm tra sân địch: có unit nào isTaunt = true không?
  2. Nếu có → tấn công unit Taunt (ưu tiên tuyệt đối)
  3. Nếu không → tấn công unit sống ở slot nhỏ nhất (frontline)
```

Tương tác quan trọng: Taunt có thể được **grant** qua GiveBuff effect — không chỉ các unit vốn có Taunt trong định nghĩa mới được hưởng. Một unit bình thường nhận được Taunt từ ability của đồng minh sẽ trở thành "cái bẫy" mà kẻ địch bắt buộc phải đánh.

**Reborn — Hồi Sinh Một Lần:**
Unit có Reborn, khi bị hạ xuống 0 HP lần đầu, không chết vĩnh viễn — thay vào đó hồi sinh ngay tại slot cũ với đúng **1 HP**. Sau khi hồi sinh, `isReborn = false` và `hasRebornUsed = true` — keyword bị tiêu thụ, lần chết tiếp theo là vĩnh viễn.

```csharp
// CardInstance.ReviveDefault()
currentHP     = 1;
isReborn      = false;    // keyword đã dùng, không Reborn lần nữa
hasRebornUsed = true;
```

Reborn kết hợp với OnDeath ability tạo ra combo mạnh: unit vẫn kích hoạt `OnDeath` khi HP về 0 *trước khi* Reborn phục hồi, nên nó "chết" về mặt trigger rồi mới sống lại. Điều này cho phép các combo như: Horus (OnDeath: buff toàn đội) + Reborn = buff đội hai lần trong một ván chiến.

Reborn cũng là mục tiêu ưu tiên của Anubis: khi đồng minh chết → ban Reborn cho đồng minh yếu nhất. Đây là cơ chế bảo vệ thứ yếu — unit gần chết nhất sẽ nhận được "bảo hiểm sống sót" trước khi combat kết thúc.

**Safeguard — Chặn Một Đòn:**
Unit có Safeguard chặn hoàn toàn đòn tấn công **tiếp theo** nhắm vào nó — sát thương bị giảm về 0, sau đó `safeguardActive = false`. Khác với Taunt (kiểm soát ai bị tấn công), Safeguard kiểm soát *kết quả* của một đòn tấn công cụ thể.

```
ExecuteClash:
  if (defender.safeguardActive)
      dmgToDefender = 0        // chặn hoàn toàn
      safeguardActive = false  // tiêu thụ
```

Safeguard bảo vệ tốt nhất các unit có HP thấp nhưng ability quan trọng — một lần chặn có thể đủ để unit đó sống qua round và kịp kích hoạt trigger `OnDeath` hay `StartOfBattle` của mình. Tương tự Taunt, Safeguard cũng có thể được grant qua GiveBuff: một spell hay ability đồng minh có thể cấp Safeguard tạm thời cho unit cụ thể trước combat.

**Tương tác giữa ba keyword:**
Ba keyword không độc lập hoàn toàn — chúng tạo ra các combo có chiều sâu:
- **Taunt + Safeguard**: unit frontline chặn đòn đầu tiên rồi vẫn tiếp tục hút damage, tăng gấp đôi khả năng "giữ frontline"
- **Taunt + Reborn**: unit này bắt buộc bị đánh, chết, hồi sinh với 1 HP, tiếp tục bắt buộc bị đánh — kẻ địch phải tốn ít nhất 2 round chỉ để hạ một unit
- **Reborn + OnDeath ability**: "chết" một lần hợp lệ để kích hoạt OnDeath trigger, sau đó sống lại — tận dụng cái chết như một cơ chế activation

Ba keyword — không nhiều hơn, không ít hơn — đủ để tạo ra chiều sâu defensive mà không làm phức tạp hóa quá mức. Taunt kiểm soát *ai* bị tấn công; Safeguard kiểm soát *kết quả* của một đòn cụ thể; Reborn kiểm soát *số lần* một unit có thể chết. Ba trục kiểm soát này độc lập về nguyên lý (có thể stack đầy đủ) nhưng bổ trợ về chiến lược — đây là lý do chúng đủ để tạo ra toàn bộ không gian chiến thuật phòng thủ của game mà không cần thêm keyword thứ tư.

---

### 3.3.4 Hệ Thống Ability (TTE) — Góc Nhìn Thiết Kế Game

Phần kỹ thuật của hệ thống ability — cấu trúc ba chiều Trigger → Target → Effect, 14 loại trigger, 12 loại target, 13 loại effect, và các modifier — đã được trình bày chi tiết trong Mục 2.3 (Cơ sở lý thuyết). Mục này tiếp cận từ góc độ thiết kế game: tại sao ability quan trọng, và nó thể hiện qua trải nghiệm người chơi như thế nào.

**Ability là nguồn gốc của tính độc đáo.** Nếu chỉ có chỉ số ATK/HP, mọi unit cùng tier sẽ gần như giống nhau — chọn card trở thành bài toán tính toán đơn giản "cái nào stat tổng cao hơn". Ability phá vỡ sự đơn điệu đó: Gilgamesh có ATK/HP trung bình nhưng mỗi khi nhận buff vĩnh viễn, nó tự nhân lên thêm; Humbaba có HP chỉ 1 nhưng kiếm coin mỗi khi tấn công; Asag có chỉ số bình thường nhưng nuốt 2 đồng minh khi deploy để lấy toàn bộ chỉ số của chúng. Mỗi unit có *identity* riêng và đặt ra câu hỏi chiến lược riêng: "tôi có nên xây đội hình xoay quanh unit này không?"

**Ability phân tầng phức tạp theo tier.** Unit tier thấp (tier 1–2) thường có ability đơn giản hoặc không có — đây là "nguyên liệu" mà người chơi dùng để merge hoặc làm placeholder cho đến khi tìm được unit tốt hơn. Unit tier cao (tier 4–6) có ability phức tạp với nhiều điều kiện, limit, và modifier — chúng là "win condition" mà người chơi xây dựng đội hình hướng tới. Sự phân tầng này làm cho việc upgrade tier của shop (tiến vào giai đoạn muộn) cảm giác có ý nghĩa: không phải chỉ là "unit to hơn", mà là "unit thú vị hơn".

**Ability và AI là một cặp thiết kế:** Mọi 14 loại trigger và 13 loại effect trong game đều được biểu diễn dưới dạng enum integer trong JSON — đây là lý do tại sao hệ thống AI có thể đánh giá toàn bộ 68 lá bài bằng một công thức thống nhất (mô tả chi tiết trong Chương 5). Một lá bài mới thêm vào game không yêu cầu viết code AI mới — bot tự động học cách đánh giá nó qua quá trình training.

---

### 3.3.5 Hệ Thống Merge — Đầu Tư Dài Hạn

Merge là cơ chế nâng cấp unit: khi người chơi tích lũy đủ số bản sao của cùng một unit (cùng `cardID` và `mergeLevel`), chúng tự động hợp thành một unit mạnh hơn đáng kể.

**Yêu cầu merge:**

```
mergeLevel 0 → 1  (1 sao → 2 sao):  cần 3 bản sao
mergeLevel 1 → 2  (2 sao → 3 sao):  cần 2 bản sao
```

Không tuyến tính: lần merge đầu đòi nhiều bản sao hơn lần merge thứ hai. Điều này phản ánh độ khan hiếm — tìm được 3 bản sao từ pool ngẫu nhiên khó hơn tìm thêm 2 bản sao khi đã có 1 sao.

**Công thức chỉ số sau merge:**

```
tier = mergeLevel + 1   (1 sao = tier 1, 2 sao = tier 2, 3 sao = tier 3)

currentATK = round( baseATK × tier  +  0.7 × (growthATKBonus + permanentATKBonus) )
             + tempSpellATKBonus + globalPermATKBonus

maxHP      = round( baseHP × tier   +  0.7 × (growthHPBonus  + permanentHPBonus)  )
             + tempSpellHPBonus + globalPermHPBonus
```

Chỉ số nhân tuyến tính với tier: 2 sao → ATK và HP nhân đôi; 3 sao → nhân ba. Hệ số `0.7` áp dụng cho growth và permanent bonus nghĩa là các buff tích lũy qua nhiều lượt cũng được nhân theo merge — unit đã tích lũy nhiều growth (StartOfBattle stacks) sẽ trở nên cực kỳ mạnh sau khi merge.

**Ví dụ cụ thể:**
Gilgamesh (`baseATK = 8, baseHP = 8`):
- 1 sao (tier 1): ATK 8, HP 8 — trung bình
- 2 sao (tier 2): ATK 16, HP 16 — mạnh, và mỗi permanent buff giờ được nhân ×2
- 3 sao (tier 3): ATK 24, HP 24 — nếu đã tích lũy nhiều permanent buff qua `OnStatGain`, tổng chỉ số có thể vượt 40+

**Gợi ý merge (Merge Hint):**
Hệ thống theo dõi liên tục số bản sao của từng unit mà người chơi đang sở hữu (trên sân + trong tay). Khi người chơi còn thiếu đúng một bản sao để hoàn thành bộ, lá bài tương ứng trong shop nhấp nháy — báo hiệu "mua thêm 1 là đủ merge". Đây là thông tin UX quan trọng tránh người chơi bỏ lỡ cơ hội merge vì phân tâm quản lý nhiều unit cùng lúc.

**Tính chiến lược của merge:**
Merge tạo ra một trục quyết định riêng biệt so với việc đa dạng hóa đội hình:

*Theo đuổi merge* đòi hỏi: (a) tìm được đủ bản sao từ pool ngẫu nhiên — đòi hỏi reroll hoặc may mắn; (b) giữ unit đó trên sân qua nhiều lượt trong khi nó còn yếu; (c) hy sinh đa dạng đội hình — 3 slot cho cùng một unit thay vì 3 unit khác nhau.

*Phần thưởng của merge* rõ ràng và lớn: unit sau merge mạnh hơn hẳn bất kỳ unit tier thấp nào có thể mua bằng cùng số coin. Một unit 3 sao tier 4 thường vượt trội hoàn toàn một unit 1 sao tier 6. Đây là con đường "nén sức mạnh vào ít unit" thay vì dàn trải.

**Token unit và giới hạn merge:**
Unit có `isToken = true` không xuất hiện trong shop và không thể bị chọn bởi các spell lấy unit ngẫu nhiên. Token thường là unit yếu được triệu hồi bởi ability của unit khác (ví dụ: *Niles Warrior* được triệu hồi bởi *Niles Commander*). Token cũng không thể merge — ngay cả khi có 3 bản sao, chúng không tự động merge vì chúng không phải là unit "thật" trong tay người chơi.

Unit được triệu hồi trong combat (`isBattleSpawned = true`) cũng không thể merge: chúng chỉ tồn tại trong trận chiến đó và biến mất sau combat. Điều này ngăn những combo bất thường như Sekhmet nuốt unit trong combat rồi giải phóng chúng sau combat để merge.

---

### 3.3.6 Kết Hợp Bốn Lớp — Thiết Kế Một Lá Bài Hoàn Chỉnh

Bốn lớp vừa trình bày (chỉ số cơ bản, tribe synergy, passive keywords, ability) và hệ thống merge kết hợp với nhau tạo ra không gian thiết kế phong phú từ một cấu trúc dữ liệu đơn giản. Lấy ví dụ với *Kingu* (Babylon, Tier 2):

```json
{ "cardName": "Kingu", "tribe": 1, "tier": 2,
  "baseATK": 2, "baseHP": 2, "cost": 3,
  "hasTaunt": true, "hasReborn": true, "abilities": [] }
```

Nhìn thuần túy, Kingu là unit yếu nhất có thể — ATK 2, HP 2, không ability. Nhưng kết hợp bốn lớp:

- **Tribe (Babylon)**: khi Kingu được deploy, Utu fire → toàn Babylon nhận +2/+2. Khi Kingu bị bán, Ashur/Lamashtu/Uridimmu fire → buff thêm. Kingu là "nguyên liệu" để trigger chain Babylon.
- **Passive keywords**: Taunt + Reborn cùng lúc trên một unit tier 2 là combo giá rẻ nhất trong game — đòi hỏi kẻ địch tốn ít nhất 2 round chỉ để hạ Kingu, trong khi frontline thực sự nằm yên phía sau.
- **Merge**: Kingu 3 sao (ATK 6, HP 6) với Taunt + Reborn + còn alive để trigger Babylon synergy 3 lần deploy trở thành frontline tank đáng gờm hơn nhiều unit tier 4 thông thường.

Đây là ví dụ điển hình của thiết kế card game tốt: một lá bài "đơn giản" trên mặt giấy có thể trở nên quan trọng bậc nhất trong ngữ cảnh đúng — khi kết hợp đủ các lớp và được đặt trong đội hình phù hợp.

---

*[Tiếp theo: Mục 3.4 — Hệ Thống Shop]*
