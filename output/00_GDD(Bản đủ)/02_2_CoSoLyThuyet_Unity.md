## 2.2 Lập Trình Game Với Unity Engine

### 2.2.1 Mô Hình Thành Phần — Nền Tảng Kiến Trúc Unity

Một trong những quyết định thiết kế quan trọng nhất trong lịch sử các engine game là cách tổ chức các thực thể (entity) trong thế giới game. Cách tiếp cận cổ điển nhất là **phân cấp thừa kế (inheritance hierarchy)**: `Animal` kế thừa từ `GameObject`, `Dog` kế thừa từ `Animal`, `GuardDog` kế thừa từ `Dog`... Cách này có vẻ tự nhiên nhưng nhanh chóng sụp đổ khi một thực thể cần kết hợp nhiều tính năng không liên quan — ví dụ: một "enemy có thể di chuyển, bắn đạn, phát âm thanh và có thanh máu" không thể kế thừa từ tất cả các nhánh đó cùng lúc mà không tạo ra mạng phụ thuộc phức tạp.

Unity giải quyết vấn đề này bằng mô hình **Component-Entity**: mọi thực thể trong game đều là một **GameObject** — một vật chứa rỗng không có hành vi cụ thể nào — và tính năng được thêm vào thông qua việc gắn các **Component** lên đó. Mỗi Component là một lớp C# kế thừa từ `MonoBehaviour` và thực thi một trách nhiệm đơn lẻ. Một lá bài trong game này không phải là một đối tượng "CardWithDragAndVisuals" khổng lồ, mà là một GameObject có nhiều Component gắn vào: `CardUI` hiển thị dữ liệu, `CardDraggable` xử lý kéo-thả, `CardVisuals` chạy animation, và `CardSlot` quản lý việc thả vào vị trí. Mỗi Component có thể được phát triển, kiểm thử và tái sử dụng độc lập.

> **[HÌNH 2.7 — Mô hình Component-Entity trong Unity]** *Sơ đồ minh họa một GameObject "Card" với bốn Component gắn vào: CardUI, CardDraggable, CardVisuals, CardSlot — mỗi Component một hộp độc lập. So sánh bên cạnh với cách tiếp cận inheritance hierarchy để làm rõ ưu điểm.*

`MonoBehaviour` là lớp cơ sở của mọi Component trong Unity và là cổng vào vòng đời (lifecycle) của engine. Nó cấp cho Component quyền truy cập vào các sự kiện quan trọng:

- **`Awake()`** — gọi khi GameObject được tạo ra, trước khi scene được tải xong. Dùng để khởi tạo tham chiếu nội bộ (không phụ thuộc Component khác).
- **`Start()`** — gọi sau `Awake()` của mọi object, trước frame đầu tiên. Dùng để thiết lập trạng thái ban đầu khi cần các Component khác đã sẵn sàng.
- **`Update()`** — gọi mỗi frame. Là trái tim của game loop thời gian thực, nhưng trong đề tài này hầu như không dùng do game là turn-based, không cần xử lý liên tục mỗi frame.

Sự phân tách giữa `Awake()` và `Start()` giải quyết một vấn đề thực tế quan trọng: nếu Component A cần tham chiếu đến Singleton của Component B, nhưng cả hai `Awake()` chạy không theo thứ tự đảm bảo, ta dễ gặp NullReferenceException. Quy ước trong dự án là: **Singleton tự đăng ký trong `Awake()`**, các Component khác **sử dụng Singleton trong `Start()`** — đảm bảo thứ tự khởi tạo đúng ngay cả khi Unity không cam kết thứ tự `Awake()` giữa các object.

---

### 2.2.2 Singleton Pattern — Quản Lý Trạng Thái Toàn Cục

Trong một game, nhiều hệ thống cần tồn tại duy nhất một thực thể trong toàn bộ scene và cần được truy cập từ bất kỳ đâu: chỉ có một `GameManager` điều khiển luồng game, một `CardDatabase` lưu toàn bộ dữ liệu bài, một `UIManager` cập nhật giao diện, một `AudioManager` phát âm thanh. **Singleton pattern** là giải pháp cổ điển cho bài toán này: lớp tự lưu trữ tham chiếu đến instance duy nhất của chính mình vào một trường `static`, cho phép mọi code khác truy cập qua `ClassName.Instance` mà không cần truyền tham chiếu qua tham số.

Trong Unity, Singleton được kết hợp với MonoBehaviour như sau:

```csharp
// GameManager.cs
public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance;   // trường static — sống suốt ứng dụng

    private void Awake()
    {
        Instance = this;   // đăng ký ngay khi object được tạo
        // ... khởi tạo board, economy ...
    }
}
```

```csharp
// CardDatabase.cs — phiên bản an toàn hơn
private void Awake()
{
    if (Instance == null) Instance = this;
    else Destroy(gameObject);   // hủy bản sao nếu Instance đã tồn tại
}
```

Hai biến thể này giải quyết hai tình huống khác nhau. `GameManager` dùng cách gán trực tiếp vì luôn chỉ có một instance và cần override nếu scene được reload. `CardDatabase` dùng cách kiểm tra trước vì nếu game chuyển scene mà object được đánh dấu `DontDestroyOnLoad`, sẽ có nguy cơ tạo ra hai bản sao — bản sao thứ hai phải tự hủy ngay lập tức.

Điều đáng chú ý là dự án có đúng năm Singleton tương ứng với năm trách nhiệm chính: `GameManager` (luồng game), `CardDatabase` (dữ liệu bài), `AIManager` (nạp AI Library), `UIManager` (giao diện), `AudioManager` (âm thanh). Mỗi Singleton sở hữu một domain rõ ràng và không chồng chéo — đây là dấu hiệu của thiết kế Singleton được áp dụng đúng chỗ, thay vì trở thành "God Object" chứa mọi thứ vào một lớp duy nhất.

---

### 2.2.3 Partial Class — Tổ Chức Code Cho Lớp Phức Tạp

`GameManager` là lớp phức tạp nhất trong dự án: nó vừa quản lý vòng lặp lượt chơi, vừa điều phối combat, vừa xử lý shop logic, vừa cập nhật board state. Đặt tất cả vào một file duy nhất sẽ tạo ra một file hàng nghìn dòng khó đọc và khó bảo trì.

C# cung cấp từ khóa `partial` cho phép định nghĩa một lớp trải rộng trên nhiều file — trình biên dịch ghép chúng lại thành một lớp duy nhất khi build:

```
GameManager.cs          →  Trạng thái core: HP, coins, board lists, Singleton, Awake/Start/ExecuteNextTurn
GameManager.Shop.cs     →  Shop logic: RefreshShop, BuyCard, Roll, Lock, MergeHints
GameManager.Combat.cs   →  Combat flow: StartCombatPhase, CombatSequence, VisualizeAction
GameManager.Board.cs    →  Board management: SyncBoards, Snapshot, Restore, SpawnUI
```

Khai báo trong mỗi file chỉ cần thêm từ khóa `partial`:

```csharp
// GameManager.cs
public partial class GameManager : MonoBehaviour { ... }

// GameManager.Combat.cs — cùng lớp, file khác
public partial class GameManager { ... }
```

Kết quả là bốn file với trách nhiệm rõ ràng, mỗi file có thể đọc độc lập mà không cần cuộn qua code không liên quan. Khi cần sửa logic combat, chỉ mở `GameManager.Combat.cs`; khi cần sửa shop, chỉ mở `GameManager.Shop.cs`. Đây không chỉ là quy ước tổ chức code mà là một quyết định kiến trúc thực sự: `partial class` là cách C# cho phép **tách biệt mối quan tâm (separation of concerns)** mà không cần tạo ra nhiều lớp nhỏ với các mối phụ thuộc phức tạp.

> **[HÌNH 2.8 — Partial Class GameManager]** *Sơ đồ cây file: GameManager.cs (core state) → GameManager.Shop.cs, GameManager.Combat.cs, GameManager.Board.cs là bốn file riêng cùng tạo thành một class duy nhất. Mỗi file một màu, kèm mô tả trách nhiệm ngắn gọn.*

---

### 2.2.4 Coroutine — Đồng Thời Hóa Không Cần Đa Luồng

Vấn đề trung tâm của lập trình game là: làm thế nào để thực hiện một chuỗi hành động kéo dài theo thời gian (animation đánh, animation chết, delay, animation hồi sinh...) mà không chặn game loop chính? Câu trả lời thông thường là lập trình bất đồng bộ (async/await) hoặc đa luồng (multithreading), nhưng cả hai đều phức tạp khi tương tác với Unity API vì Unity không thread-safe — mọi thao tác trên GameObject phải chạy trên main thread.

Unity giải quyết bài toán này bằng **Coroutine**: một hàm đặc biệt có thể tạm dừng (`yield`) và tiếp tục sau khi điều kiện nào đó thỏa mãn — sau một khoảng thời gian, sau một frame, hay sau khi một Coroutine khác kết thúc. Từ góc độ người dùng, code Coroutine trông như code đồng bộ tuần tự nhưng thực ra chạy bất đồng bộ:

```csharp
// GameManager.Combat.cs
private IEnumerator CombatSequence()
{
    // Bước B: tính toán TOÀN BỘ trận đấu ngay lập tức, lưu log
    TurnRecord combatLog = new TurnRecord();
    resolver.ResolveTurn(playerBoard, enemyBoard, combatLog);

    // Bước C: phát lại từng action theo thời gian thực
    foreach (var action in combatLog.actions)
    {
        yield return StartCoroutine(VisualizeAction(action));
        yield return new WaitForSeconds(0.1f);
    }

    // Bước E: xử lý kết quả sau khi hiệu ứng kết thúc
    CheckVictoryConditions();
}
```

Đoạn code này tiết lộ một quyết định kiến trúc rất thú vị: **tách biệt hoàn toàn giữa tính toán và trình diễn**. Toàn bộ combat — mọi đòn tấn công, mọi cái chết, mọi reborn, mọi ability trigger — được tính toán đồng bộ trong một lần gọi `resolver.ResolveTurn()` và kết quả được lưu vào `TurnRecord combatLog`. Chỉ sau đó, Coroutine mới phát lại từng action một cách có kiểm soát để người chơi có thể theo dõi. Điều này có nhiều ưu điểm: không bao giờ xảy ra tình trạng animation "đi trước" hay "đi sau" logic game vì logic đã hoàn tất trước; đồng thời, cùng một `resolver.ResolveTurn()` có thể được dùng trong headless simulation (không cần Coroutine, không cần frame, chỉ cần kết quả) mà không cần viết lại bất kỳ dòng logic nào.

---

### 2.2.5 Ranh Giới Quan Trọng Nhất: MonoBehaviour và Plain C#

Đây là quyết định kiến trúc trọng tâm và tinh tế nhất của toàn bộ dự án — một quyết định mà nếu không được thực hiện từ đầu, toàn bộ hệ thống training GA sẽ không thể tồn tại.

`MonoBehaviour` không chỉ là một lớp cơ sở — nó là **sự ràng buộc với Unity engine lifecycle**. Một lớp kế thừa từ `MonoBehaviour` không thể được khởi tạo bằng `new ClassName()` như một đối tượng C# bình thường; nó phải được gắn vào một GameObject trong một scene, nghĩa là cần màn hình, cần Unity Editor hoặc runtime, cần toàn bộ infrastructure của engine. Để chạy một lớp như vậy hàng nghìn lần trong một vòng lặp training, ta cần hàng nghìn scene — điều hoàn toàn phi thực tế.

Ngược lại, một lớp C# thuần (`plain C# class`) không kế thừa MonoBehaviour có thể được khởi tạo bằng `new` ở bất cứ đâu, kể cả từ một thread nền hay từ một script đang chạy trong Unity batch mode (không có cửa sổ, không có rendering):

```csharp
// Có thể chạy trong training loop mà không cần scene:
BotAgent botA = new BotAgent(chromosome);        // plain C# — OK
CombatResolver resolver = new CombatResolver();  // plain C# — OK
GameSimulator sim = new GameSimulator();         // plain C# — OK

// KHÔNG thể chạy nếu không có scene:
GameManager gm = new GameManager();   // MonoBehaviour — COMPILE ERROR
CardDatabase db = new CardDatabase(); // MonoBehaviour — COMPILE ERROR
```

Hiểu được điều này, dự án phân tách rõ ràng: mọi lớp thuộc **tầng AI và combat logic** — `Chromosome`, `BotAgent`, `CombatResolver`, `GameSimulator`, `EconomyManager`, `CardInstance` — đều là plain C# class. Chúng có thể được khởi tạo tự do, không phụ thuộc lifecycle, không cần scene. Ngược lại, `GameManager`, `CardDatabase`, `UIManager`, `AudioManager` là MonoBehaviour vì chúng cần tương tác với GameObject, Transform, Coroutine và các Unity API khác.

Sự phân tách này không tự nhiên xảy ra — nó đòi hỏi kỷ luật thiết kế. Khi viết `CombatResolver`, phải liên tục tự hỏi: "Logic này có thực sự cần Unity không, hay chỉ cần C# thuần?" Khi viết `BotAgent`, phải đảm bảo không gọi bất kỳ Unity API nào (`Debug.Log`, `Random.Range`, `Resources.Load`...) mà chỉ dùng `System.Random` và các thư viện C# chuẩn. Đây là nguyên tắc mà cộng đồng Unity gọi là **"Fat Model, Thin View"** — logic nghiệp vụ đặt trong plain C# (Model), Unity chỉ đảm nhiệm hiển thị (View).

Kết quả thực tế của nguyên tắc này: `GameSimulator.EvaluateMatch()` — hàm chạy một trận đấu đầy đủ 20 lượt với tất cả mechanic game — chỉ là một plain C# method có thể gọi từ bất kỳ đâu mà không cần một dòng Unity API:

```csharp
// GameSimulator.cs — không có dòng nào liên quan đến Unity scene
public class GameSimulator
{
    private CombatResolver resolver = new CombatResolver();

    public MatchResult EvaluateMatch(BotAgent botA, BotAgent botB)
    {
        int hpA = 7, hpB = 7;
        for (int i = 0; i < 20; i++)
        {
            // ... shop, decision, combat, check HP ...
        }
        return new MatchResult { ... };
    }
}
```

Chính nhờ ranh giới này, `GATrainer` — dù là MonoBehaviour để có thể chạy coroutine training trong Unity — có thể gọi `sim.EvaluateMatch()` hàng nghìn lần mà không cần tạo ra một scene mới nào cả.

> **[HÌNH 2.9 — Ranh Giới MonoBehaviour và Plain C#]** *Sơ đồ phân tầng kiến trúc: tầng trên (Unity-dependent) chứa GameManager, CardDatabase, UIManager là MonoBehaviour; tầng dưới (headless-compatible) chứa Chromosome, BotAgent, CombatResolver, GameSimulator là Plain C#. Mũi tên một chiều từ tầng trên xuống tầng dưới, không có chiều ngược lại.*

---

### 2.2.6 Hệ Thống Tài Nguyên và JSON — Kiến Trúc Data-Driven

Một quyết định thiết kế quan trọng khác là **data-driven architecture**: toàn bộ nội dung game — 68 lá bài với abilities, chỉ số, bộ tộc — không được hardcode trong C# mà được lưu trong file JSON và nạp vào runtime. Điều này cho phép thêm, sửa, cân bằng lá bài mà không cần biên dịch lại project.

Unity cung cấp hai cơ chế nạp file. Cơ chế thứ nhất là **Resources system**: bất kỳ file nào đặt trong thư mục `Assets/Resources/` đều có thể được nạp trong runtime bằng `Resources.Load<T>("fileName")` mà không cần đường dẫn tuyệt đối. Đây là cách `CardsData.json` và `AI_Library.json` được truy cập:

```csharp
// CardDatabase.cs
TextAsset jsonFile = Resources.Load<TextAsset>("CardsData");
CardDataWrapper data = JsonUtility.FromJson<CardDataWrapper>(jsonFile.text);
```

Cơ chế thứ hai là `File.ReadAllText()` / `File.WriteAllText()` từ thư viện `System.IO` — dùng khi cần đọc/ghi file theo đường dẫn tuyệt đối, ví dụ khi `GATrainer` ghi kết quả training ra `AI_Library.json`:

```csharp
// GATrainer.cs
string path = Path.Combine(Application.dataPath, "Resources", "AI_Library.json");
File.WriteAllText(path, JsonUtility.ToJson(library, prettyPrint: true));
```

Unity's **`JsonUtility`** là thư viện serialization tích hợp sẵn, hoạt động dựa trên annotation `[System.Serializable]` trên các lớp cần serialize. So với các thư viện phổ biến như Newtonsoft.Json, `JsonUtility` ít tính năng hơn (không hỗ trợ Dictionary, không hỗ trợ interface) nhưng nhanh hơn và tương thích tốt hơn với Unity build pipeline, bao gồm cả batch mode. `Chromosome` được đánh dấu `[System.Serializable]` chính xác vì lý do này — để `JsonUtility.ToJson(library)` có thể serialize toàn bộ `AILibrary` (chứa các `Chromosome`) thành file JSON mà không cần thư viện ngoài:

```csharp
// Chromosome.cs
[System.Serializable]
public class Chromosome
{
    public const int GeneCount = 37;
    public float[] genes = new float[GeneCount];
    public float fitness = 0f;
    // ...
}
```

Kết quả là `AI_Library.json` — file duy nhất chứa "trí tuệ" của 5 bot sau quá trình training — có thể được đọc bằng bất kỳ text editor nào, kiểm tra thủ công, commit vào version control, và nạp lại trong game mà không cần bất kỳ công cụ đặc biệt nào. Tính minh bạch của dữ liệu này là một ưu điểm thực tiễn quan trọng trong quá trình phát triển.

---

### 2.2.7 Kết Nối Ngược — Tại Sao Các Nguyên Tắc Này Quan Trọng Với Đề Tài

Nhìn lại, năm nguyên tắc kỹ thuật vừa trình bày không phải là lý thuyết trừu tượng — mỗi cái đóng một vai trò cụ thể trong việc làm cho dự án khả thi:

**Component Model** cho phép xây dựng lá bài với các tính năng phức tạp (drag, drop, animation, data) mà không cần một lớp "God Card" khổng lồ, từng phần có thể phát triển và kiểm thử độc lập.

**Singleton** đảm bảo `CardDatabase`, `AIManager` và `GameManager` luôn có thể được truy cập từ bất kỳ đâu trong combat và shop logic mà không cần truyền tham chiếu qua chuỗi hàm dài.

**Partial Class** giữ cho `GameManager` — lớp có trách nhiệm lớn nhất — có thể đọc và bảo trì được bằng cách chia nhỏ theo domain chức năng.

**Coroutine** cho phép combat animation phong phú (tấn công, chết, reborn, flash effect) mà không chặn game loop, đồng thời bảo toàn sự tách biệt giữa logic (đồng bộ, headless-compatible) và visualization (bất đồng bộ, Unity-dependent).

**MonoBehaviour/Plain C# boundary** — quan trọng nhất trong ngữ cảnh đề tài — là điều kiện tiên quyết cho toàn bộ hệ thống GA training. Không có ranh giới này, mỗi trận đấu mô phỏng sẽ đòi hỏi một scene Unity đầy đủ, training sẽ mất hàng giờ thay vì vài phút, và dự án ở quy mô tiểu luận chuyên ngành sẽ không khả thi về mặt thời gian.

Phần tiếp theo (2.3) sẽ trình bày hệ thống ability theo mô hình TTE — kiến trúc data-driven cho phép 68 lá bài có hành vi phức tạp mà không cần viết code riêng cho từng lá.

---

*[Tiếp theo: Mục 2.3 — Mô hình Trigger → Target → Effect]*
