# CHƯƠNG 4: KIẾN TRÚC HỆ THỐNG KỸ THUẬT

## 4.1 Câu Hỏi Nền Tảng: Làm Thế Nào Để Một Hệ Thống Phức Tạp Không Tự Sập Dưới Sức Nặng Của Chính Nó?

Dự án này bắt đầu với một mô tả nghe có vẻ đơn giản: xây dựng một game Auto Chess với AI bot có thể học qua Genetic Algorithm. Nhưng ngay khi đặt bút xuống thiết kế kỹ thuật, sự phức tạp bắt đầu lộ rõ. Hệ thống cần quản lý 68 lá bài với hàng trăm kỹ năng khác nhau. Nó cần một AI có thể đưa ra quyết định chiến thuật qua hàng trăm nghìn trận đấu để học được chiến lược tốt. Nó cần giao diện người dùng mượt mà cho người chơi thật, nhưng đồng thời cũng phải chạy được không cần màn hình để training AI. Và tất cả những thứ này phải tồn tại trong cùng một codebase mà không làm rối tung nhau.

Câu hỏi kiến trúc đặt ra không phải là "viết code như thế nào" mà là "tổ chức code như thế nào để từng phần có thể tồn tại và phát triển độc lập". Kinh nghiệm trong phát triển phần mềm cho thấy rằng phần lớn các dự án không thất bại vì thiếu tính năng — mà vì kiến trúc ban đầu tạo ra các phụ thuộc chéo khiến mỗi thay đổi mới đều có nguy cơ phá vỡ những gì đã hoạt động. Trong game development, điều này đặc biệt nghiêm trọng: một lỗi trong logic combat có thể làm hỏng toàn bộ kết quả training AI; một dependency từ AI vào UI layer có thể khiến headless training trở nên bất khả thi.

Chương này giải thích cách hệ thống được tổ chức để tránh những bẫy đó — không phải bằng cách liệt kê các thành phần, mà bằng cách lần theo các quyết định thiết kế: bắt đầu từ bài toán, qua các phương án có thể, đến giải pháp được chọn và lý do đằng sau.

---

## 4.2 Nguyên Tắc Tổ Chức: Ba Yêu Cầu Quyết Định Mọi Thứ

Khi phân tích kỹ các tính năng cần xây dựng, ba yêu cầu nổi lên như những ràng buộc không thể thỏa hiệp. Chúng không phải là mục tiêu đẹp để có — chúng là điều kiện cứng mà nếu vi phạm, toàn bộ dự án sẽ không thể hoàn thành.

**Yêu cầu đầu tiên là tính data-driven.** Với 68 lá bài, mỗi lá có từ 1 đến 4 kỹ năng với những tổ hợp tham số phức tạp, câu hỏi đặt ra là: khi cần điều chỉnh giá trị buff của một lá bài từ +2 lên +3, quy trình đó tốn bao lâu? Nếu câu trả lời là "mở file C#, tìm đúng hằng số, sửa, biên dịch lại, kiểm thử" — thì việc cân bằng game sẽ trở thành cơn ác mộng. Yêu cầu đặt ra là: mọi tham số nội dung game (chỉ số lá bài, giá trị kỹ năng, cấu hình drop rate) phải được định nghĩa trong dữ liệu JSON, không cứng hóa trong code.

**Yêu cầu thứ hai là tính headless-compatible.** GA training đòi hỏi chạy hàng trăm nghìn trận đấu mô phỏng. Unity không thể render song song hàng trăm trận đấu — mỗi trận cần một scene, một Camera, một vòng lặp render. Giải pháp duy nhất là tách biệt hoàn toàn game logic ra khỏi Unity runtime: toàn bộ phần tính toán (combat, quyết định mua bài, đánh giá board state) phải có thể chạy hoàn toàn không cần MonoBehaviour, không cần scene, không cần màn hình.

**Yêu cầu thứ ba là ranh giới phụ thuộc một chiều.** Hệ thống AI phải có khả năng dùng combat engine mà không cần biết UI tồn tại. UI phải có khả năng hiển thị kết quả combat mà không cần biết AI hoạt động ra sao. Và cả hai phải có thể thay đổi độc lập mà không làm vỡ game logic. Đây là yêu cầu về cấu trúc phụ thuộc: thành phần cấp dưới (core engine, data) không được phép biết đến thành phần cấp trên (manager, UI).

Ba yêu cầu này cùng nhau kéo theo một kiến trúc phân tầng bắt buộc.

---

## 4.3 Kiến Trúc Phân Tầng — Giải Pháp Cho Ba Ràng Buộc

Toàn bộ codebase được tổ chức thành bốn tầng với nguyên tắc phụ thuộc một chiều từ trên xuống dưới:

```
┌─────────────────────────────────────────────────────────┐
│                    TẦNG UI                              │
│   CardUI · CardDraggable · CardSlot · UIManager         │
│   (MonoBehaviour — chỉ hiển thị, không chứa logic game) │
├─────────────────────────────────────────────────────────┤
│                  TẦNG MANAGER                           │
│   GameManager (partial) · CardDatabase · EconomyManager  │
│   AIManager · AudioManager                              │
│   (Singleton MonoBehaviour — điều phối game state)      │
├─────────────────────────────────────────────────────────┤
│                 TẦNG CORE ENGINE                        │
│   AbilityEngine · CombatResolver · CardInstance         │
│   (Plain C# — không phụ thuộc Unity API)               │
├─────────────────────────────────────────────────────────┤
│                   TẦNG DATA                             │
│   CardDefinition · AbilityData · AI_Library.json        │
│   CardsData.json · CardsSpells.json                     │
│   (Dữ liệu thuần — serializable, không có logic)        │
└─────────────────────────────────────────────────────────┘
                          ↕ (song song, không phụ thuộc)
        ┌──────────────────────────────────────┐
        │            TẦNG AI                   │
        │  BotAgent · GameSimulator · GATrainer │
        │  (Plain C# — dùng Core Engine trực tiếp, │
        │   bỏ qua hoàn toàn Manager và UI)    │
        └──────────────────────────────────────┘
```

> **[HÌNH 4.1 — Kiến Trúc Phân Tầng]** *Sơ đồ hộp 4 tầng chính xếp dọc (Data ở đáy → Core Engine → Manager → UI ở đỉnh), với AI System nằm ngoài sang phải, kết nối trực tiếp xuống Core Engine và Data, bỏ qua Manager/UI. Mũi tên một chiều đi từ trên xuống dưới (chiều phụ thuộc được phép). Màu xanh lá cho plain C# (Core + AI), màu vàng cho MonoBehaviour (Manager + UI), màu xám cho Data.*

Vị trí của tầng AI trong sơ đồ là điểm then chốt: nó nằm ngoài tháp bốn tầng, kết nối trực tiếp xuống Core Engine mà không đi qua Manager hay UI. Đây là hiện thân kiến trúc của yêu cầu headless: `GameSimulator` và `BotAgent` biết cách dùng `CombatResolver`, `CardDatabase`, và `EconomyManager`, nhưng không biết `GameManager` hay `UIManager` tồn tại. Khi training chạy không có Unity scene, những class này không cần — và không đòi hỏi — bất cứ thứ gì từ tầng trên.

Ranh giới cứng nhất trong toàn bộ kiến trúc nằm ở chỗ phân chia **MonoBehaviour** và **Plain C#**. Quy tắc đơn giản nhưng được áp dụng nghiêm ngặt: bất kỳ lớp nào có logic thuần (tính toán, quyết định game, xử lý dữ liệu) đều là plain C#, có thể được khởi tạo tự do bằng `new ClassName()`. Chỉ những lớp thực sự cần Unity lifecycle (`Awake`, `Start`, `Update`, sự kiện kéo-thả từ input system) mới kế thừa `MonoBehaviour`. Ranh giới này không phải phong cách code — nó là điều kiện vật lý để AI training có thể tồn tại.

---

## 4.4 Tầng Data — Nơi Mọi Thứ Bắt Đầu

### 4.4.1 Bài Toán: Lá Bài Biết Nó Làm Gì Như Thế Nào?

Câu hỏi đơn giản nhất trong thiết kế card game lại ẩn chứa một quyết định kiến trúc quan trọng: khi một lá bài được tạo ra trong game, nó biết chỉ số và kỹ năng của mình từ đâu?

Cách tiếp cận trực tiếp nhất là hardcode mọi thứ: `Marduk` có `baseATK = 4`, `baseHP = 6`, và khi chết nó tăng +2/+2 cho một đồng minh ngẫu nhiên. Nhưng cách này sẽ yêu cầu 68 lớp C# cho 68 lá bài, mỗi lớp chứa các giá trị cụ thể của lá bài đó. Mỗi lần cân bằng game, ta phải sửa code và biên dịch lại. Mỗi lá bài mới đòi hỏi một lập trình viên, không phải một game designer.

Giải pháp là tách **định nghĩa** (những gì lá bài là) ra khỏi **trạng thái** (những gì lá bài đang là trong lúc chơi). Định nghĩa là bất biến và được chia sẻ — mọi bản sao "Marduk" trong game đều đọc từ cùng một template. Trạng thái là riêng tư và thay đổi theo từng bản sao cụ thể — Marduk trên bàn người chơi có thể đã nhận buff qua nhiều lượt trong khi Marduk trong shop vẫn có chỉ số gốc. Hai khái niệm này được hiện thực hóa bởi hai lớp: `CardDefinition` (định nghĩa) và `CardInstance` (trạng thái runtime).

---

### 4.4.2 CardDefinition — Template Bất Biến

`CardDefinition` là bản thiết kế của một lá bài, được đọc từ `CardsData.json` khi game khởi động và tồn tại duy nhất trong `CardDatabase`. Toàn bộ 68 unit card và các spell card được mô tả bởi cùng một lớp:

```csharp
public class CardDefinition
{
    public string   cardID;       // khóa tra cứu: "babylon_marduk"
    public string   cardName;
    public CardType cardType;     // Unit hoặc Spell
    public Tribe    tribe;        // None, Babylon, Olympus, Niles
    public int      baseATK;
    public int      baseHP;
    public int      cost;         // 1–3 coin
    public int      tier;         // 1–3, xác định khi nào xuất hiện trong shop

    public List<AbilityData> abilities;   // danh sách kỹ năng theo mô hình TTE

    public bool hasTaunt;         // bắt buộc bị nhắm mục tiêu
    public bool hasReborn;        // hồi sinh với 1 HP sau khi chết
    public bool hasSafeguard;     // giảm/chặn damage

    public bool isToken;          // không xuất hiện trong shop tự nhiên
    public bool isRepeatingDeploy; // OnDeploy kích hoạt lại mỗi lần đặt xuống bàn
}
```

Ba passive keyword (Taunt, Reborn, Safeguard) được lưu trực tiếp là boolean thay vì đưa vào hệ thống TTE, bởi chúng không phải "kỹ năng kích hoạt theo sự kiện" mà là trạng thái thụ động ảnh hưởng đến cơ chế game ở cấp độ engine (xác định mục tiêu của kẻ địch, xử lý cái chết). Đây là ví dụ của nguyên tắc "đừng over-engineer": không phải mọi mechanic đều cần được đưa vào cùng một framework.

Một chi tiết nhỏ nhưng quan trọng: `CardDefinition` có phương thức `Clone()` tạo ra bản sao độc lập của danh sách `abilities`. Lý do xuất phát từ một lỗi thiết kế đã gặp trong quá trình phát triển: khi một spell "AddAbility" cấp thêm kỹ năng cho một unit cụ thể bằng cách thêm vào `abilities` list của nó, nếu list đó là reference chung với `CardDefinition` trong database, tất cả Marduk được tạo ra sau đó sẽ có thêm kỹ năng ngoài ý muốn. Clone đảm bảo mỗi `CardInstance` nhận một bản sao `abilities` riêng, cô lập hoàn toàn với template gốc.

---

### 4.4.3 CardInstance — Trạng Thái Sống Của Một Lá Bài

Trong khi `CardDefinition` là template bất biến, `CardInstance` là thực thể đang tồn tại trong game — nó có chỉ số hiện tại, lịch sử buff, mức merge, và một tập trạng thái runtime phục vụ cho những mechanic phức tạp nhất của game.

Phức tạp nhất trong `CardInstance` là cách chỉ số được quản lý. Thay vì một con số duy nhất cho ATK hay HP, hệ thống chia thành ba lớp bonus riêng biệt, mỗi lớp với vòng đời khác nhau:

```
currentATK = baseATK × tier
           + 0.7 × (permanentATKBonus + growthATKBonus)   ← tích lũy lâu dài
           + tempSpellATKBonus                             ← tồn tại 1 combat rồi xóa
           + globalPermATKBonus                            ← từ tribe buff toàn cục
```

**Permanent bonus** đến từ các kỹ năng với `isPermanent = true` — một unit nhận được +3 ATK vĩnh viễn từ ability OnDeath của đồng minh. **Growth bonus** tích lũy từ EndTurnShop — một số lá bài tăng chỉ số mỗi lượt như "rèn luyện" giữa các trận đấu. **Temp spell bonus** đến từ spell mua trong shop phase với `isPermanent = false` — buff chỉ có hiệu lực cho combat kế tiếp rồi tự xóa sau khi `ResetStats()` được gọi lần thứ hai (lần đầu là đầu combat để áp buff, lần thứ hai là sau combat để xóa). **Global tribe bonus** đến từ các kỹ năng như Thoth của Babylon — buff vĩnh viễn *tất cả* unit cùng bộ tộc, kể cả unit chưa được mua.

Hệ số `0.7` (keepRatio) cho permanent và growth bonus là một quyết định balance có chủ ý: buff tích lũy qua nhiều lượt chỉ mang 70% giá trị vào chỉ số thực tế. Nếu không có hệ số này, một unit nhận liên tiếp nhiều buff nhỏ qua nhiều lượt có thể đạt chỉ số phi thực tế. Với `keepRatio = 0.7`, tổng buff tích lũy bị "suy giảm tự nhiên" — không phải mất đi, mà được cân bằng tỷ lệ khi tính vào chỉ số thực.

Ngoài chỉ số, `CardInstance` theo dõi nhiều trạng thái runtime phục vụ các mechanic đặc thù: `consumedCardIDs` lưu ID của các unit bị Sekhmet "nuốt" (phục vụ cho effect `SummonConsumed`); `abilityTriggerCounts` đếm số lần mỗi kỹ năng được kích hoạt trong combat hiện tại để enforce `triggerLimit`; `onDeathProcessed` đánh dấu đã được đưa vào Death Stack để tránh xử lý cái chết hai lần; `isBattleSpawned` đánh dấu unit được triệu hồi tạm thời trong combat để ngăn bán hoặc merge.

> **[HÌNH 4.2 — Mối Quan Hệ CardDefinition / CardInstance]** *Sơ đồ: CardDefinition "Marduk" (box đơn, nhãn "template - 1 bản duy nhất") với ba mũi tên ra đến ba CardInstance khác nhau (box riêng, nhãn "Instance A - board player", "Instance B - board bot", "Instance C - shop"). Mỗi instance có chỉ số khác nhau (mergeLevel, bonus khác nhau). Dưới đó, công thức ResetStats() với 4 thành phần bonus được highlight.*

---

### 4.4.4 AbilityData — Ngôn Ngữ Mô Tả Hành Vi

`AbilityData` là lớp data quan trọng thứ ba trong tầng Data, mô tả một kỹ năng theo mô hình TTE đã được giới thiệu ở Chương 2. Nếu `CardDefinition` trả lời "lá bài này là gì", thì `AbilityData` trả lời "lá bài này làm gì và khi nào". Một `CardDefinition` có thể chứa nhiều `AbilityData` trong danh sách `abilities`, mỗi entry là một kỹ năng độc lập.

Ngoài ba trường cốt lõi (`trigger`, `target`, `effect`), `AbilityData` chứa một tập các modifier định hình cách kỹ năng hoạt động: `isPermanent` quyết định buff có tồn tại qua combat hay không; `triggerLimit` giới hạn số lần kích hoạt tối đa (ví dụ: "chỉ trigger 2 lần mỗi trận"); `conditionCount` cho phép kỹ năng "skip" — chỉ kích hoạt mỗi N lần trigger (ví dụ: "mỗi lần thứ 3 bị đánh"); `isEscalating` làm cho giá trị tăng dần mỗi lần kích hoạt; `subjectTribe` lọc trigger `OnAllySell`/`OnAllyDeploy` để chỉ phản ứng với unit thuộc bộ tộc cụ thể.

Sức mạnh của thiết kế này hiện rõ khi đọc JSON của một lá bài phức tạp. Anubis, lá bài Niles có kỹ năng "mỗi khi tấn công, triệu hồi một Mummy ngẫu nhiên; chỉ làm vậy tối đa 3 lần mỗi trận, và sau lần thứ nhất, buff combat của mỗi Mummy tăng thêm", được biểu diễn đầy đủ bởi một `AbilityData` với `trigger=OnAttack`, `target=Self`, `effect=Summon`, `summonCardID="niles_mummy"`, `triggerLimit=3`, `isEscalating=true`. Không cần một dòng C# riêng cho Anubis.

---

## 4.5 Tầng Core Engine — Nơi Logic Game Sống

### 4.5.1 Ability Engine: Biến Dữ Liệu Thành Hành Động

Tầng Data định nghĩa các kỹ năng bằng struct `AbilityData`, nhưng bản thân dữ liệu không làm được gì — nó cần một engine để diễn giải và thực thi. `AbilityEngine` là engine đó: nó nhận một sự kiện game (unit vừa tấn công, đồng minh vừa chết, lượt vừa kết thúc), tìm tất cả kỹ năng phù hợp với sự kiện đó, chọn mục tiêu, rồi thực thi hiệu ứng.

Hai hàm chính mà phần còn lại của hệ thống gọi vào `AbilityEngine` là:

- `TriggerAbility(triggerContext, source, directEnemy, allyBoard, enemyBoard)` — kích hoạt tất cả kỹ năng của `source` có trigger khớp với `triggerContext`
- `BroadcastAllyEvent(trigger, subject, allyBoard, enemyBoard)` — broadcast tới tất cả đồng minh khi một sự kiện "ally-wide" xảy ra, để mỗi unit có cơ hội phản ứng

Cặp hàm này phản ánh hai mô hình kích hoạt khác nhau trong game. `TriggerAbility` là kích hoạt tự thân: khi Marduk chết, chính Marduk kích hoạt `OnDeath`. `BroadcastAllyEvent` là phản ứng đồng đội: khi Marduk chết, tất cả unit còn sống nhận được broadcast `OnAllyDeath` với Marduk là "subject", để những unit như Horus hoặc Ninhursag có cơ hội phản ứng với cái chết của đồng minh.

Bên trong `TriggerAbility`, trước khi một kỹ năng được thực thi, engine đi qua một chuỗi kiểm tra: trigger type có khớp không; số lần kích hoạt có vượt `triggerLimit` chưa; điều kiện `conditionCount` có thỏa mãn không (đây có phải lần thứ N trigger); filter `subjectTribe` có hợp lệ không khi trigger là `OnAllySell`; và một guard đặc biệt chặn Sekhmet ăn unit trong shop phase. Chỉ khi tất cả điều kiện đều thỏa, engine mới chuyển sang chọn mục tiêu và thực thi hiệu ứng.

Một chi tiết thiết kế tinh tế: engine chỉ tăng `abilityTriggerCounts[i]` khi **thực sự có mục tiêu hợp lệ**. Nếu một ability `OnAllySummon + Destroy` tìm thấy unit để nuốt, lần kích hoạt mới được đếm. Nếu không có mục tiêu, `triggerLimit` không bị tiêu hao. Điều này đảm bảo giới hạn kích hoạt phản ánh số lần ability *thực sự có tác dụng* — không phải số lần sự kiện xảy ra.

---

### 4.5.2 Mục Tiêu Và Những Edge Case Không Lường Trước

Thuật toán chọn mục tiêu trong `AbilityEngine.Targets.cs` là straightforward với các `TargetType` đơn giản: `Self` trả về chính unit đó, `AllAllies` trả về tất cả đồng minh còn sống, `LowestHealthAlly` trả về unit có HP thấp nhất. Nhưng ba `TargetType` đặc biệt đòi hỏi xử lý cẩn thận hơn:

`TriggerSubject` — khi `OnAllyDeath` trigger, unit gây ra sự kiện là unit vừa chết; khi `OnAllySummon`, subject là unit vừa được triệu hồi. Engine tái sử dụng parameter `directEnemy` để truyền subject qua call stack, cho phép kỹ năng như "khi đồng minh chết, buff chính unit đó ngay trước khi nó biến mất" hoạt động đúng theo ý định thiết kế.

`AllNilesAllies` và `AllBabylonAllies` — kết hợp với flag `globalTribeBuff` trong `AbilityData`, chúng cho phép những kỹ năng như Thoth buff vĩnh viễn *toàn bộ* unit cùng bộ tộc, bao gồm cả unit đang trong tay, trong shop, và thậm chí unit chưa được mua. `GameManager` duy trì một accumulator riêng cho global tribe buff; mỗi khi unit mới được tạo ra (`CreateCardInSlot()`), `ApplyGlobalPermBuffToNewUnit()` được gọi để áp tổng buff đã tích lũy cho tribe của unit đó — đảm bảo Thoth "nhớ" mọi lá bài Babylon đã từng được mua.

Ba edge case phức tạp nhất phát sinh từ sự kết hợp của nhiều mechanic:

**Reborn chain.** Khi một unit có `isReborn` chết, nó hồi sinh với 1 HP. Sau đó, `OnAllyReborn` của đồng minh kích hoạt. Nếu một Summoner đang phản ứng với `OnAllyReborn` bằng cách triệu hồi thêm unit, `OnAllySummon` của Sekhmet có thể kích hoạt, và Sekhmet nuốt unit vừa được triệu hồi. Nếu Sekhmet chết trong trận đó, `SummonConsumed` triệu hồi lại mọi unit đã bị nuốt, kích hoạt thêm `OnAllySummon`... Chuỗi này có thể sâu tùy ý và không thể được giải quyết bằng cách "xử lý ngay khi xảy ra" — nó đòi hỏi cơ chế đặc biệt được mô tả trong phần 4.5.3.

**SummonConsumed với Sekhmet.** `isConsume = true` trong `AbilityData` khi effect là `Destroy` có nghĩa là unit bị hủy không hoàn toàn biến mất — cardID của nó được lưu vào `consumedCardIDs` của Sekhmet. Khi Sekhmet chết, effect `SummonConsumed` đọc danh sách này và triệu hồi lại từng unit. Guard `_isCombatPhase` đảm bảo Sekhmet không thể ăn unit trong shop phase — một unit đồng minh được deploy lên bàn trong shop phase không nên biến mất.

**Gilgamesh và OnStatGain.** Gilgamesh có kỹ năng phản ứng với bất kỳ việc nào nó nhận được chỉ số vĩnh viễn: `OnStatGain → Self → AddStats(+1/+1, permanent)`. Vấn đề: buff đó bản thân nó cũng là permanent stat gain, sẽ lại kích hoạt `OnStatGain` → vòng đệ quy vô hạn. Guard `_firingOnStatGain` là một boolean được bật trước khi kích hoạt `OnStatGain` và tắt sau — mọi kỹ năng `OnStatGain` được thực thi trong khi flag đang bật sẽ không được phép kích hoạt thêm `OnStatGain` nữa.

---

### 4.5.3 Combat Engine — Khi Mọi Thứ Chết Cùng Lúc

`CombatResolver` là engine chiến đấu: nhận hai board (hai list 7 slot), chạy một lượt combat hoàn chỉnh, và trả về kết quả. Mọi logic trong `CombatResolver` là plain C# — không MonoBehaviour, không Camera, không Unity API nào. Điều này cho phép `GameSimulator` của AI gọi `CombatResolver.ResolveTurn()` hàng nghìn lần mỗi phút trong training mà không cần Unity runtime.

Nhưng việc implement combat đúng về mặt kỹ thuật phức tạp hơn nhiều so với việc chỉ lần lượt cho các unit tấn công nhau. Vấn đề cơ bản là: **điều gì xảy ra khi một unit chết trong lúc đang xử lý turn của unit khác?**

Nếu xử lý cái chết ngay lập tức — xóa unit khỏi danh sách, kích hoạt `OnDeath`, kích hoạt `BroadcastAllyEvent(OnAllyDeath)`, xử lý Reborn nếu có — ta đang modify collection trong khi đang iterate qua nó, dẫn đến `InvalidOperationException` hoặc missed events. Tệ hơn, nếu xử lý cái chết của unit A kích hoạt một chain của events mà cuối cùng gây ra cái chết của unit B, và unit B có Reborn, ta cần quyết định: Reborn của B xảy ra trước hay sau khi `OnAllyDeath` của A được broadcast hoàn toàn?

Giải pháp là **Death Stack** — một cấu trúc dữ liệu riêng biệt thu gom mọi "cái chết đang chờ xử lý":

```csharp
private struct DeathEvent
{
    public CardInstance victim;
    public CardInstance killer;
    public List<CardInstance> victimBoard;
    public List<CardInstance> killerBoard;
}

private readonly Stack<DeathEvent> deathStack = new Stack<DeathEvent>();
```

Khi một unit có `currentHP ≤ 0` và chưa được xử lý (`!onDeathProcessed`), nó được đưa vào stack thay vì xử lý ngay. Flag `onDeathProcessed = true` được bật ngay lúc đưa vào stack để tránh cùng cái chết được đưa vào hai lần. Sau mỗi đòn đánh — hoặc sau mỗi ability effect — `FlushDeathStack()` được gọi để giải quyết tất cả cái chết đang chờ:

```
FlushDeathStack():
  while stack không rỗng:
    pop (victim, killer, victimBoard, killerBoard)
    
    if victim.isReborn và chưa dùng Reborn:
        victim.ReviveDefault()     → hồi sinh 1 HP
        BroadcastAllyEvent(OnAllyReborn, victim, ...)
        ProcessNextPendingSummon() → xử lý summon từ OnAllyReborn ngay
    else:
        xóa victim khỏi board
        TriggerAbility(OnDeath, victim, killer, ...)
        BroadcastAllyEvent(OnAllyDeath, victim, ...)
        ProcessNextPendingSummon() → xử lý summon từ OnDeath/OnAllyDeath ngay
```

> **[HÌNH 4.3 — Death Stack Flow]** *Sơ đồ luồng: hộp "unit HP≤0" → mũi tên đến "đưa vào deathStack" → vòng lặp "stack không rỗng?" → nhánh trái "isReborn?" (có: Revive → broadcast OnAllyReborn → ProcessSummon), nhánh phải "xóa khỏi board → trigger OnDeath → broadcast OnAllyDeath → ProcessSummon" → vòng lại đầu loop. Màu đỏ cho "xóa thật", xanh lam cho "Reborn".*

Lý do chọn Stack (LIFO) thay vì Queue (FIFO) là chủ ý: khi một chain death tạo ra thêm death events (ví dụ: unit A chết → kích hoạt `OnAllyDeath` → unit B nhận buff quá lớn và ngay lập tức chết), cái chết mới nhất (B) cần được giải quyết trước khi quay lại giải quyết hậu quả của cái chết gốc (A). LIFO đảm bảo chain death gần nhất được resolve hoàn toàn trước khi quay lại chain trước đó — một thứ tự xử lý nhất quán và có thể dự đoán được.

---

### 4.5.4 Thứ Tự Tấn Công Và Vấn Đề Của Queue Tĩnh

Một chi tiết khác của combat engine đáng chú ý là cách thứ tự tấn công được xác định. Thứ tự tấn công trong Auto Chess thường được sắp xếp theo tốc độ (đây, ATK được dùng như đại diện cho tốc độ — unit có ATK cao tấn công trước). Cách tiếp cận đơn giản nhất là build một danh sách thứ tự tấn công một lần ở đầu trận, rồi duyệt qua danh sách đó. Nhưng cách này có một vấn đề: unit mới có thể được triệu hồi trong combat thông qua các ability `Summon`. Nếu thứ tự tấn công đã cố định, unit mới triệu hồi sẽ không bao giờ tấn công trong trận đó.

Giải pháp là rebuild danh sách tấn công ở đầu *mỗi sub-turn* thay vì một lần cho cả trận:

```csharp
// Mỗi sub-turn trong vòng lặp combat:
var attackOrder = new List<AttackEntry>();
for (int slot = 0; slot < BoardSlotCount; slot++)
{
    if (pBoard[slot] != null && !pBoard[slot].IsDead)
        attackOrder.Add(new AttackEntry { attacker = pBoard[slot], atkBoard = pBoard, defBoard = eBoard });
    if (eBoard[slot] != null && !eBoard[slot].IsDead)
        attackOrder.Add(new AttackEntry { attacker = eBoard[slot], atkBoard = eBoard, defBoard = pBoard });
}
attackOrder.Sort((a, b) => b.attacker.currentATK.CompareTo(a.attacker.currentATK));
```

Sau mỗi đòn đánh, `FlushDeathStack()` chạy, unit mới có thể được triệu hồi. Sub-turn kế tiếp rebuild danh sách từ board hiện tại — unit mới tự động được đưa vào thứ tự tấn công theo đúng vị trí ATK của nó. Đây là ví dụ của việc rebuild đắt hơn một chút về mặt tính toán (O(n log n) mỗi sub-turn thay vì O(1)) nhưng đổi lại đảm bảo tính đúng đắn trong mọi trường hợp — một đánh đổi hoàn toàn hợp lý trong game turn-based.

---

### 4.5.5 Pre-Combat Snapshot: Bảo Toàn Trạng Thái Sau Chiến Đấu

Một vấn đề thiết kế quan trọng: sau combat, board người chơi cần được khôi phục về trạng thái trước khi chiến đấu. Các unit chết phải biến mất; các unit được triệu hồi tạm thời (`isBattleSpawned = true`) phải biến mất; chỉ số phải được reset về base stats (buff tạm thời không được giữ lại); nhưng buff vĩnh viễn phải được bảo toàn.

`GameManager` lưu snapshot trước combat:

```csharp
// Lưu snapshot — tuple (slotIndex, unit, consumedCardIDs tại thời điểm đó)
preCombatSnapshot = new (int slot, CardInstance unit, List<string> consumedIDs)[BoardSlotCount];
for (int i = 0; i < BoardSlotCount; i++)
{
    var unit = playerBoard[i];
    preCombatSnapshot[i] = (i, unit,
        unit?.consumedCardIDs != null ? new List<string>(unit.consumedCardIDs) : null);
}
```

Sau combat, `RestorePreCombatPlayerBoard()` không chỉ đơn giản rollback về snapshot — nó còn xử lý `consumedCardIDs` của Sekhmet riêng biệt. Trong combat, Sekhmet có thể nuốt thêm unit được triệu hồi tạm thời. Sau combat, unit tạm thời đó biến mất — nhưng nếu không restore `consumedCardIDs` về snapshot, Sekhmet sẽ "nhớ" đã nuốt unit không thực sự tồn tại và cố triệu hồi nó ở combat sau. Snapshot của `consumedCardIDs` đảm bảo Sekhmet chỉ nhớ những unit nó đã nuốt trước trận đấu — đúng với thiết kế game.

---

## 4.6 Tầng Manager — Điều Phối Và Singleton

### 4.6.1 GameManager: Partial Class Như Một Quyết Định Kiến Trúc

`GameManager` là lớp phức tạp nhất trong tầng Manager — nó điều phối vòng lặp game (shop phase → combat → kết quả → lặp lại), quản lý state machine, và là điểm kết nối giữa UI và các hệ thống engine. Vấn đề tự nhiên nảy sinh: một lớp chịu trách nhiệm quá nhiều dễ trở thành "God Object" — một file hàng nghìn dòng nơi mọi logic đều được nhét vào, không thể đọc và khó maintain.

Giải pháp là `partial class` của C#: một lớp được phép tách thành nhiều file, compiler ghép lại thành một khi build. `GameManager` được chia thành bốn file với trách nhiệm không chồng chéo:

- `GameManager.cs` — trạng thái cốt lõi: HP người chơi, turn counter, bộ đối thủ bot, win/lose logic, global tribe buff accumulator, và vòng lặp `ExecuteNextTurn()`
- `GameManager.Shop.cs` — toàn bộ logic shop: refresh shop, tính tier theo turn, reroll, freeze, điền slot trống
- `GameManager.Combat.cs` — khởi động và kết thúc combat: lấy snapshot, gọi `CombatResolver`, restore board, visualize từng action trong `TurnRecord`
- `GameManager.Board.cs` — quản lý board UI: tạo card prefab, xử lý kéo-thả từ CardSlot, merge hints

Khi cần sửa logic combat, chỉ một file cần mở. Khi cần thêm mechanic shop, chỉ `GameManager.Shop.cs` cần thay đổi. Không có sự phụ thuộc nào giữa bốn file partial — `GameManager.Shop.cs` không import hay gọi bất kỳ thứ gì từ `GameManager.Combat.cs`.

---

### 4.6.2 CardDatabase: Quản Lý Pool Thẻ Và Drop Rate

`CardDatabase` là singleton đọc toàn bộ định nghĩa card từ JSON khi khởi động và cung cấp API để lấy shop ngẫu nhiên. Hai phương thức chính là `GetRandomUnitShop(count, shopTier)` và `GetRandomSpellShop(count, shopTier)`.

Cơ chế drop rate theo shop tier quyết định "chất lượng" của shop theo từng thời điểm trong game. Ở lượt đầu (shop tier 1), chỉ unit tier 1 xuất hiện. Từ lượt 3 (shop tier 2), unit tier 2 bắt đầu xuất hiện với xác suất nhỏ. Đến cuối game (shop tier 6), unit tier 3 chiếm đa số. Phân phối này tạo ra cảm giác progression tự nhiên: người chơi xây nền tảng bằng unit rẻ sớm game, rồi dần nâng cấp khi có nhiều tiền hơn và shop chất lượng hơn.

| Shop Tier | Tier 1 | Tier 2 | Tier 3 |
|:---------:|:------:|:------:|:------:|
| 1 | 100% | — | — |
| 2 | 70% | 30% | — |
| 3 | 40% | 45% | 15% |
| 4 | 25% | 40% | 35% |
| 5 | 15% | 30% | 55% |
| 6 | 10% | 20% | 70% |

Flag `isToken` trong `CardDefinition` khiến unit bị lọc bỏ khỏi pool shop — token card là các unit chỉ có thể xuất hiện thông qua ability `Summon` (Zombie, Mummy, Warrior...), không bao giờ xuất hiện để mua.

---

### 4.6.3 EconomyManager: Đơn Giản Có Chủ Ý

`EconomyManager` là lớp plain C# nhỏ nhất trong hệ thống — chỉ vài chục dòng — nhưng quyết định thiết kế của nó phản ánh sự cân nhắc kỹ về mục tiêu tổng thể của dự án:

```csharp
public void ResetEconomy()
{
    CurrentCoin = 10 + bonusNextTurn + permanentIncomeBonus;
    bonusNextTurn = 0;
}
```

Mỗi lượt, người chơi (và bot) nhận đúng **10 coin cố định**, cộng thêm các bonus từ spell. Không có công thức interest (giữ tiền nhiều → lãi cao), không có streak bonus (thắng/thua liên tiếp → thêm coin), không có base income tăng theo level. So với TFT hay Hearthstone Battlegrounds, đây là economy rất đơn giản.

Quyết định này không phải vì thiếu ý tưởng — mà vì khi AI cần học qua 37 gene, một economy phức tạp sẽ thêm quá nhiều biến số vào fitness landscape, làm chậm convergence. Bot cần học chiến thuật "mua lá bài nào, khi nào reroll, khi nào bán" — không phải tối ưu hóa đường cong interest. Economy đơn giản cho phép GA focus vào phần học quan trọng hơn.

Hai mechanism có trong `EconomyManager` — `bonusNextTurn` (thêm coin một lần cho lượt sau) và `permanentIncomeBonus` (thêm vĩnh viễn mỗi lượt) — đến từ spell, đủ để tạo chiều sâu kinh tế mà không cần thêm biến số tự nhiên vào cơ chế thu nhập.

---

## 4.7 Tầng UI — Hiển Thị Mà Không Chứa Logic

### 4.7.1 Chuỗi Trách Nhiệm Của Một Lá Bài Trong UI

Mỗi lá bài trong game có một chuỗi ba Component phối hợp để hiển thị và tương tác:

`CardUI` chịu trách nhiệm duy nhất là render: đọc dữ liệu từ `CardInstance` và hiển thị tên, ATK, HP, icon tribe, artwork. Nó không biết người chơi có thể kéo lá bài hay không, không biết slot nào trên bàn là hợp lệ để thả vào. Đây là Observer thuần túy — `GameManager` gọi `RefreshCardUI(unit)` sau mỗi sự kiện làm thay đổi chỉ số, `CardUI` cập nhật display tương ứng.

`CardDraggable` xử lý vật lý kéo-thả: bắt đầu drag, theo dõi vị trí con trỏ, kết thúc drag. Khi drag bắt đầu, card được detach khỏi slot và trở thành floating UI element. Khi drag kết thúc, `CardDraggable` thông báo cho slot đích. Nhưng bản thân `CardDraggable` không quyết định điều gì xảy ra với lá bài — nó chỉ là trung gian vận chuyển thông tin.

`CardSlot` quản lý một vị trí cụ thể (board slot, hand slot, hoặc shop slot) và xử lý sự kiện drop:

```csharp
void OnDrop(PointerEventData data)
{
    CardDraggable card = data.pointerDrag?.GetComponent<CardDraggable>();
    if (card == null) return;

    if (card.sourceSlot.IsShopSlot && this.IsBoardSlot)
        GameManager.Instance.BuyAndPlace(card.currentInstance, this.slotIndex);

    else if (card.sourceSlot.IsBoardSlot && this.IsBoardSlot)
        GameManager.Instance.SwapBoardSlots(card.sourceSlot.slotIndex, this.slotIndex);

    else if (card.sourceSlot.IsBoardSlot && this.IsHandSlot)
        GameManager.Instance.ReturnToHand(card.currentInstance);
}
```

`CardSlot` không tự mua card, không tự swap, không tự quyết định hợp lệ hay không. Mọi quyết định game đều được delegate về `GameManager`. Đây là ranh giới UI/logic được áp dụng nhất quán: `CardSlot` biết giao diện đang làm gì, `GameManager` quyết định game logic xảy ra như thế nào.

---

### 4.7.2 GameRecord — Cầu Nối Giữa Headless Engine Và Visual

`CombatResolver.ResolveTurn()` là headless và tức thì: toàn bộ combat được tính xong trong milliseconds không cần Unity render. Nhưng người chơi cần xem combat diễn ra từng bước — ai đánh ai, unit nào nhận buff, unit nào chết. Làm thế nào hiển thị diễn biến của một quá trình đã hoàn thành?

Giải pháp là `TurnRecord` — một danh sách các `CombatAction` được ghi lại trong quá trình resolve:

```csharp
public enum ActionType { Attack, Death, StatChange, Summon }

public struct CombatAction
{
    public ActionType type;
    public int        slotIndex;
    public bool       isPlayerSide;
    public int        newATK, newHP;
    public FlashType  flash;   // Green (buff), Red (damage), Yellow (coin)
}
```

`CombatResolver` nhận `TurnRecord log` như một parameter và ghi mỗi sự kiện vào đó: đòn đánh, cái chết, buff nhận được, unit được triệu hồi — theo đúng thứ tự xảy ra. Sau khi `ResolveTurn()` hoàn thành, `TurnRecord` chứa toàn bộ "kịch bản" của trận đấu. UI layer đọc danh sách này và animate từng action với delay nhỏ giữa các bước, tạo ra ảo giác combat thời gian thực.

Kết quả quan trọng hơn cơ chế: trong AI training, `GameSimulator` truyền `null` thay vì `TurnRecord log` — không cần ghi lại diễn biến, chỉ cần kết quả win/lose. Cùng một `CombatResolver` phục vụ cả hai mục đích mà không cần branch code hay nếu điều kiện nào cả. Đây là ví dụ điển hình của thiết kế tốt: phần tử chỉ có một mục đích cụ thể (ghi nhận), tồn tại hoặc không tùy vào ngữ cảnh gọi.

---

## 4.8 Headless Compatibility — Khi Mọi Quyết Định Kiến Trúc Hội Tụ

### 4.8.1 Ranh Giới Vật Lý Giữa Game Và AI

Toàn bộ chương này đến nay là chuẩn bị cho một khả năng duy nhất: chạy game simulation hoàn toàn không cần Unity runtime. Ranh giới được duy trì nhất quán — plain C# cho mọi thứ có logic, MonoBehaviour chỉ khi thực sự cần Unity lifecycle — cuối cùng cho phép `GameSimulator` được viết như sau:

```csharp
public class GameSimulator   // plain C# — không kế thừa MonoBehaviour
{
    private readonly CombatResolver resolver = new CombatResolver();   // plain C#

    public MatchResult EvaluateMatch(BotAgent botA, BotAgent botB)
    {
        int hpA = 7, hpB = 7;

        for (int turn = 0; turn < 20; turn++)
        {
            int shopTier = Mathf.Clamp((turn + 2) / 2, 1, 6);
            var shopA = CardDatabase.Instance.GetRandomUnitShop(5, shopTier);
            // ...
            botA.DecidePrepPhase(shopA, spellShopA, shopTier);
            botB.DecidePrepPhase(shopB, spellShopB, shopTier);

            resolver.ResolveTurn(botA.board, botB.board, null);   // null log — headless
            // ... cập nhật HP, kết thúc sớm nếu cần
        }

        return new MatchResult { ... };
    }
}
```

Không một dòng code nào trong `GameSimulator` cần `UnityEngine` (ngoại trừ `Mathf.Clamp` — một utility toán học mà Unity cung cấp nhưng có thể thay thế bằng `System.Math.Clamp` trong .NET 5+). `CardDatabase.Instance` là singleton có thể được khởi tạo và populated trong Editor script mà không cần scene. `CombatResolver` và `AbilityEngine` là plain C# hoàn toàn.

---

### 4.8.2 Nullable GameManager — Xử Lý Dependency Không Thể Tránh Khỏi

Một vấn đề thực tế: `AbilityEngine` cần thêm coin khi kỹ năng `GainCoin` được kích hoạt. Trong game thực, `GameManager.Instance.AddCoin(amount)` là cách đúng. Trong training headless, `GameManager` không tồn tại.

Giải pháp tao nhã là null-conditional operator:

```csharp
// Trong AbilityEngine.ExecuteEffect(), case GainCoin:
GameManager.Instance?.AddCoin(ability.effectValue1);
```

Toán tử `?.` của C# biến lời gọi thành no-op khi `Instance` là null — không exception, không branch code, không flag riêng để kiểm tra chế độ training. Đây là ví dụ của cách xử lý dependency bất đối xứng: code cần Unity biết về sự vắng mặt của Unity và tự lo, thay vì forcing phần còn lại của hệ thống phải biết đến sự khác biệt.

Economy của bot trong training được `BotAgent.economy` (một `EconomyManager` riêng) quản lý — không qua `GameManager`. Coin được cấp và tiêu thụ hoàn toàn trong sandbox của bot, không cần bất kỳ global state nào từ game thực.

---

### 4.8.3 Pipeline Training Khép Kín

Toàn bộ quy trình training — từ khởi động đến kết quả — chạy qua một PowerShell script không cần thao tác thủ công:

```powershell
# train_ai.ps1
& $unityPath `
    -batchmode `             # không mở window Unity
    -nographics `            # không khởi tạo graphics device
    -projectPath $PSScriptRoot `
    -executeMethod AITrainingBatch.RunTraining `
    -logFile training_log.txt `
    -quit
```

`AITrainingBatch.RunTraining()` là Editor-only static method: khởi tạo `GATrainer`, chạy vòng lặp tiến hóa đồng bộ (không dùng coroutine vì Editor không có game loop), và lưu kết quả vào `Assets/Resources/AI_Library.json`. Khi game khởi động lần sau, `AIManager` đọc file này và nạp 5 chromosome vào 5 `BotAgent`.

Vòng lặp khép kín: `train_ai.ps1` → `GATrainer` → `GameSimulator` × N trận → `AI_Library.json` → `AIManager` → `BotAgent` trong game → người chơi đối đầu. Mỗi bước là độc lập và có thể chạy lại từ bất kỳ điểm nào — nếu AI Library bị hỏng, chạy lại `train_ai.ps1`; nếu muốn training với tham số khác, sửa `AITrainingBatch` và chạy lại mà không cần mở Unity Editor.

> **[HÌNH 4.4 — Pipeline Headless Training]** *Sơ đồ luồng ngang gồm 6 bước: train_ai.ps1 → Unity -batchmode -nographics → AITrainingBatch.RunTraining() → [GATrainer → GameSimulator × 432.000 trận] → AI_Library.json → AIManager nạp vào game. Phần từ Unity đến AI_Library được bao trong hộp nền xám nhạt nhãn "Headless (không cần màn hình)". Phần AIManager → game được tô màu xanh nhạt nhãn "Game Runtime".*

---

## 4.9 Tổng Hợp: Kiến Trúc Như Kết Quả Của Ràng Buộc

Nhìn lại toàn bộ chương, một điều rõ ràng là: mọi quyết định kiến trúc quan trọng đều xuất phát từ ba ràng buộc được đặt ra ở đầu chương, không phải từ sở thích hay thói quen.

**Data-driven** dẫn đến `CardDefinition`/`AbilityData` là pure data class, `CardDatabase` đọc JSON, và toàn bộ content có thể thay đổi mà không recompile.

**Headless-compatible** dẫn đến ranh giới cứng MonoBehaviour/Plain C#, nullable `GameManager.Instance?.` thay vì direct call, và `TurnRecord` như cầu nối giữa headless engine và visual layer.

**Phụ thuộc một chiều** dẫn đến `AbilityEngine` không import `GameManager`, `GameSimulator` không biết `UIManager` tồn tại, và mọi game logic trong `CardSlot.OnDrop()` đều được delegate về `GameManager`.

Kết quả không phải một kiến trúc "đẹp trên lý thuyết" — mà là một hệ thống có thể chạy 432.000 trận đấu mô phỏng trong vòng 20–30 phút, đồng thời cung cấp trải nghiệm chơi game hoàn chỉnh với UI tương tác và feedback visual. Cùng một codebase phục vụ cả hai mục đích mà không cần bất kỳ điều chỉnh runtime nào — đó là dấu hiệu của một kiến trúc đã làm đúng việc của mình.

---

*[Kết thúc Chương 4 — Tiếp theo: Chương 6 — Kết Quả Và Đánh Giá]*
