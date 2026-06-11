# CHƯƠNG 4: HÀNH TRÌNH XÂY DỰNG HỆ THỐNG KỸ THUẬT

Hai chương trước mô tả game *nên* vận hành như thế nào — luật chơi, cảm giác, những lựa chọn thiết kế đứng sau từng cơ chế. Chương này kể câu chuyện về việc bản thiết kế đó đã thực sự *trở thành* một hệ thống chạy được, theo đúng nghĩa đen: không phải một chuyến tham quan qua các lớp đã hoàn thiện, mà là ghi lại những quyết định, những ngã rẽ sai, và những phát hiện dọc đường. Ba ràng buộc đặt ra luật chơi cho kỹ thuật ngay từ ngày đầu (mục 4.1); đáp ứng chúng buộc phải trả lời những câu hỏi mà không một giáo trình thiết kế game nào đặt ra (mục 4.3), và phải vượt qua ít nhất một vấn đề mà trong một thời gian, trông có vẻ không thể giải (mục 4.4).

---

## 4.1 Ba Ràng Buộc Không Thể Thỏa Hiệp

Trước khi ba ràng buộc dưới đây được viết ra, đã có một câu hỏi cần trả lời trước: dùng **Unity ML-Agents** — framework huấn luyện AI chính thức của Unity, đã tích hợp sẵn PPO, hỗ trợ GPU và được cộng đồng kiểm chứng rộng rãi — hay tự xây một hệ thống AI từ đầu bằng GA? Ba quan sát khiến cán cân nghiêng về vế sau. Một, ML-Agents tạo ra mạng nơ-ron với hàng chục nghìn trọng số — không thể đọc ngược lại "vì sao" nó chơi như vậy, trong khi mục tiêu ở đây là một biểu diễn mà từng thành phần đều mang ý nghĩa tường minh, có thể phân tích được (Chương 5). Hai, PPO cần hàng triệu bước và nhiều giờ huấn luyện trên GPU để hội tụ cho một không gian trạng thái lớn như Auto Chess — quy mô tài nguyên vượt xa một máy tính cá nhân. Ba, để dùng được ML-Agents, toàn bộ trạng thái game phải được mã hóa tay thành observation vector và action mask; chỉ cần thêm một loại trigger hay một bộ tộc mới, schema đó — và cả model đã huấn luyện trên nó — có nguy cơ phải làm lại từ đầu.

Quyết định không dùng framework có sẵn kéo theo một hệ quả trực tiếp: mọi thứ, từ việc mô phỏng trận đấu đến cách biểu diễn chiến lược, phải tự xây bằng những viên gạch chạy được mà không cần đến Unity Editor — và phải chạy đủ nhanh để lặp lại hàng trăm nghìn lần. Đó là lúc ba yêu cầu sau xuất hiện trên bàn — không phải như mong muốn có thể linh hoạt đánh đổi, mà như điều kiện tồn tại: vi phạm bất kỳ điều nào cũng khiến dự án không thể hoàn thành đúng mục tiêu ban đầu.

- **Data-driven:** Mọi tham số nội dung (chỉ số lá bài, giá trị kỹ năng, tỉ lệ rơi bài) phải nằm trong file dữ liệu, không cứng hóa trong code — để có thể chỉnh cân bằng mà không cần biên dịch lại, và để 68 lá bài có thể được mô tả bằng dữ liệu thay vì 68 lớp con.
- **Headless-compatible:** Huấn luyện GA đòi hỏi hàng trăm nghìn trận mô phỏng. Toàn bộ phần "luật chơi" — combat, quyết định mua bán, đánh giá đội hình — phải chạy được mà không cần cửa sổ hiển thị, không cần cảnh dựng sẵn, không cần một khung hình nào được vẽ ra.
- **Phụ thuộc một chiều:** Thành phần ở tầng dưới không được biết đến sự tồn tại của tầng trên. Cỗ máy chiến đấu không được biết giao diện trông ra sao; AI phải có thể dùng cỗ máy đó mà không hề hay biết một giao diện từng tồn tại.

Ba ràng buộc này nghe có vẻ là những phát biểu trừu tượng về "kiến trúc tốt" — nhưng từng cái, khi va chạm với thực tế xây dựng, lại đẻ ra một câu hỏi cụ thể không có câu trả lời sẵn trong sách. Phần còn lại của chương kể lại quá trình tìm câu trả lời cho từng câu hỏi đó.

---

## 4.2 Một Bộ Luật, Hai Cách Vận Hành

Mục 3.2.3 đã giới thiệu nguyên tắc "tính toán trước, trình diễn sau" như một lời hứa: *cơ chế cụ thể đứng sau nó được trình bày ở Chương 4*. Đây là nơi lời hứa đó được thực hiện.

Vấn đề xuất hiện gần như ngay khi việc xây dựng hệ thống chiến đấu bắt đầu: cùng một bộ luật combat phải đồng thời phục vụ hai mục đích hoàn toàn khác nhau. Với người chơi, nó cần diễn ra theo nhịp độ quan sát được — từng đòn đánh, từng cái chết, từng buff phải xuất hiện tuần tự, đủ chậm để hiểu, đủ kịch tính để hồi hộp. Với AI, nó cần điều ngược lại tuyệt đối — chạy nhanh nhất có thể, hàng trăm nghìn lần liên tiếp, không một khung hình nào được vẽ ra. Viết hai cỗ máy combat riêng biệt — một "có hoạt ảnh" và một "headless" — là phương án bị loại ngay từ đầu: chỉ cần hai cỗ máy lệch nhau một chi tiết nhỏ, AI sẽ học cách chơi giỏi một trò chơi *khác* với trò chơi mà con người thực sự trải nghiệm.

Lời giải nằm ở việc tách câu hỏi "luật chơi nói gì sẽ xảy ra" ra khỏi câu hỏi "người xem có cần thấy nó xảy ra hay không". `CombatResolver` — cỗ máy combat duy nhất — tính toán trọn vẹn một lượt giao tranh ngay tức khắc, thành một chuỗi sự kiện rời rạc, có thứ tự, hoàn toàn xác định: ai đánh ai, ai chết, ai hồi sinh, buff nào kích hoạt, theo đúng trình tự nhân-quả. Chuỗi sự kiện đó sau đó được trao cho bên gọi nó — và bên gọi mới là nơi quyết định phải làm gì tiếp theo. Trong một trận đấu thật, `GameManager` ghi chuỗi đó vào một `TurnRecord`, rồi giao cho tầng giao diện "phát lại" tuần tự bằng hoạt ảnh — khoảng 0.1 giây mỗi hành động. Trong huấn luyện, `GameSimulator` truyền `null` thay cho `TurnRecord` — và `CombatResolver`, khi thấy không có nơi nào cần ghi log, đơn giản là bỏ qua toàn bộ bước "viết lại câu chuyện" ấy. Cùng một bộ luật, cùng một kết quả — chỉ khác nhau ở việc có ai đó đang "xem" hay không.

Cách tiếp cận này — vốn là một minh họa cụ thể cho nguyên tắc tách biệt tính toán và trình diễn đã trình bày ở mục 2.2.3 — định hình toàn bộ phần còn lại của kiến trúc. Hệ thống tách thành bốn tầng xếp chồng, cộng thêm một tầng AI nằm tách biệt sang một bên:

> **[HÌNH 4.1 — Hình Dạng Kiến Trúc Khi Áp Dụng Ba Ràng Buộc]** *Sơ đồ 4 tầng xếp dọc (Data ở đáy → Core Engine → Manager → UI), tầng AI nằm tách biệt bên phải, nối thẳng xuống Core Engine — bỏ qua hoàn toàn Manager và UI. Mũi tên phụ thuộc chỉ đi một chiều từ trên xuống. Màu xanh lá đánh dấu mã thuần C#; vàng đánh dấu MonoBehaviour; xám đánh dấu dữ liệu thuần.*

Vị trí của tầng AI là điểm mấu chốt: nó không nằm "trên cùng" hay "dưới cùng" trong tháp — nó đứng tách hẳn ra, với một đường nối duy nhất đi thẳng xuống Core Engine. Đó chính là hiện thân vật lý của ràng buộc headless: AI không cần — và không được phép cần — bất kỳ thứ gì nằm phía trên lớp luật chơi thuần túy.

Ranh giới quan trọng nhất xuyên suốt cả bốn tầng là ranh giới giữa **MonoBehaviour** và **Plain C#**: bất kỳ lớp nào chỉ chứa logic thuần — tính toán, ra quyết định, xử lý dữ liệu — đều là một lớp C# bình thường, có thể tạo ra bằng `new` ở bất cứ đâu, kể cả trong một tiến trình chưa từng mở một cảnh game nào. Chỉ những lớp thực sự cần vòng đời của Unity (`Awake`, `Update`, sự kiện input từ người chơi) mới kế thừa `MonoBehaviour`. Đường ranh giới này không phải một quy ước phong cách — nó là điều kiện vật lý quyết định AI training có thể tồn tại hay không.

---

## 4.3 Khi Kỹ Năng Can Thiệp Lẫn Nhau — Ba Câu Hỏi Không Có Trong Sách Giáo Khoa

Mục 2.3.3 đã đặt ra ba câu hỏi kiến trúc mở khi đem mô hình Trigger-Target-Effect ra khỏi trang giấy: **thứ tự**, **tính đệ quy**, và **đối tượng động**. Không phải vì những khái niệm này khó hiểu về mặt lý thuyết — mà vì không một giáo trình nào buộc người đọc phải trả lời chúng bằng code thực sự chạy được, hàng nghìn lần một phút, không được phép treo máy. Đây là nơi cả ba nhận được câu trả lời cụ thể — từ chính những tình huống mà mục 3.6 đã mô tả ở góc nhìn người chơi.

### 4.3.1 Thứ Tự — Ai Phản Ứng Trước, Ai Phản Ứng Sau?

Một sự kiện đơn lẻ — "một unit vừa chết" — có thể đồng thời là tín hiệu cho *nhiều* kỹ năng khác nhau: kỹ năng "khi chính mình chết" của unit đó, và kỹ năng "khi đồng minh chết" của mọi unit còn sống xung quanh. Engine cần một quy tắc rõ ràng cho việc ai lên tiếng trước.

Câu trả lời được hiện thực hóa bằng hai luồng phân phối tách biệt: một luồng "tự kích hoạt" luôn xử lý phản ứng của chính chủ thể trước tiên, và chỉ sau khi luồng đó hoàn tất, một luồng "phát sự kiện cho đồng đội" mới quét qua toàn bộ đồng minh còn sống — theo đúng thứ tự vị trí trên sân, bỏ qua chính chủ thể, và (theo mặc định) chỉ thông báo cho những unit cùng bộ tộc, trừ khi kỹ năng của unit đó được đánh dấu rõ ràng là "phản ứng với bất kỳ ai" (như trường hợp Anubis ở mục 3.3.2 — phản ứng với cái chết của *bất kỳ* đồng minh nào, không riêng gì Niles).

Thứ tự này không phải một lựa chọn tùy tiện — nó là điều kiện để chuỗi phản ứng Reborn ở mục 3.6.6 diễn ra đúng như mô tả: kỹ năng "khi chết" của Horus phải hoàn tất buff cho toàn đội *trước khi* cơ hội hồi sinh xuất hiện — nếu thứ tự đảo ngược, buff sẽ được tính trên một bộ chỉ số không còn đúng nữa, và toàn bộ chuỗi nhân-quả mà người chơi quan sát được sẽ trở nên vô nghĩa.

### 4.3.2 Tính Đệ Quy — Khi Một Kỹ Năng Tự Kích Hoạt Chính Nó

Mục 3.7.3 đã hé lộ câu chuyện: kỹ năng của Gilgamesh — "tự cộng thêm chỉ số vĩnh viễn mỗi khi nhận một buff vĩnh viễn từ bên ngoài" — trông hoàn toàn vô hại trên giấy. Nó chỉ là một phản ứng đơn giản với một loại sự kiện.

Vấn đề lộ ra khi nhìn kỹ hơn một bước: hành động "tự cộng thêm chỉ số vĩnh viễn" — chính là *bản thân nó* — cũng tạo ra một sự kiện "nhận buff vĩnh viễn". Nếu engine xử lý nó theo đúng quy tắc thông thường, Gilgamesh sẽ phản ứng với chính phản ứng của mình, rồi lại phản ứng với phản ứng đó, không hồi kết — một vòng lặp vô hạn sẽ đóng băng trận đấu ngay lập tức (hoặc, tệ hơn, treo cứng phiên huấn luyện ở trận thứ vài chục nghìn trong đêm).

Lời giải là một "cánh cổng tự khóa": ngay trước khi phát sự kiện "nhận buff vĩnh viễn", engine dựng lên một cờ báo hiệu "đang xử lý sự kiện này rồi — đừng vào nữa", và đảm bảo cờ đó luôn được hạ xuống khi xong việc, kể cả khi có lỗi xảy ra giữa chừng. Trong lúc cờ đang dựng, mọi nỗ lực phát lại đúng loại sự kiện ấy đều bị bỏ qua trong im lặng — chuỗi phản ứng vẫn hoàn tất, Gilgamesh vẫn nhận được phần thưởng của nó, nhưng cánh cửa dẫn ngược về chính nó đã bị khóa lại đúng một lần. Đây chính là "chiếc lever cân bằng nằm trong chính kiến trúc" mà mục 3.7.3 đã nhắc tới như một minh chứng sống cho bài toán đệ quy nêu ở mục 2.3.3 — không phải một nỗi lo lý thuyết, mà là một ràng buộc kỹ thuật có thật, đòi hỏi một giải pháp cụ thể.

### 4.3.3 Đối Tượng Động — Khi Một Đối Tượng Vừa Biến Mất Vừa Phải Được Ghi Nhớ

Sekhmet "nuốt" đồng minh để hấp thụ sức mạnh — và ở đây, engine gặp phải một loại đối tượng kỳ lạ: một unit đã biến mất khỏi sân, nhưng chưa thực sự "kết thúc vai trò" của nó trong trận đấu. Mã định danh của unit bị nuốt được lưu lại trong một danh sách riêng — để nếu chính Sekhmet sau đó gục ngã, toàn bộ những unit ấy có thể được triệu hồi trở lại y hệt như chưa từng biến mất. Engine phải theo dõi một thực thể tồn tại đồng thời ở hai trạng thái: "đã rời khỏi sân" và "vẫn còn nợ một lần xuất hiện".

Một cánh cổng tự khóa thứ hai xuất hiện ngay tại đây: Sekhmet bị chặn hoàn toàn khỏi việc "nuốt" bất kỳ ai trong giai đoạn Shop — cơ chế này được thiết kế như một phần của combat, không phải một cách để âm thầm xóa vĩnh viễn lá bài khỏi tay người chơi chỉ vì Sekhmet đang đứng trên sân.

Một quan sát thú vị nảy sinh từ chính vấn đề này: engine thực ra dùng *hai cấu trúc dữ liệu khác nhau* cho hai "danh sách chờ" trông có vẻ tương tự nhau. Những cái chết đang chờ xử lý xếp vào một ngăn xếp — phần tử vào sau ra trước; còn những lượt triệu hồi đang chờ lại xếp vào một hàng đợi — phần tử vào trước ra trước. Sự khác biệt này không phải ngẫu nhiên: nó là câu trả lời cho chính câu hỏi mà mục tiếp theo xoay quanh.

---

## 4.4 Vấn Đề Khó Nhất — Khi Nhiều Cái Chết Xảy Ra Cùng Lúc

Mục 3.6.5 đã mô tả hiện tượng từ góc nhìn người chơi — một chuỗi phản ứng dây chuyền luôn được giải quyết trọn vẹn trước khi trận đấu tiếp tục — và đã hứa hẹn: *cơ chế cụ thể đứng sau nó, cùng câu chuyện về quá trình thử–sai để đi đến lời giải cuối cùng, được trình bày ở Chương 4*. Đây là câu chuyện đó.

**Cách tiếp cận đầu tiên — và lý do nó thất bại:** Phương án trực giác nhất là xử lý cái chết *ngay khi nó xảy ra* — ngay giữa lúc đang giải quyết một đòn tấn công, gỡ unit khỏi sân, kích hoạt kỹ năng "khi chết" của nó, rồi lập tức phát sự kiện cho đồng đội. Cách này đổ vỡ theo hai hướng cùng lúc. Thứ nhất, việc gỡ một phần tử khỏi danh sách trong khi engine vẫn đang duyệt qua chính danh sách đó để xử lý hàng đợi tấn công làm hỏng toàn bộ vòng lặp đang chạy — một lỗi kinh điển của lập trình. Thứ hai, và nguy hiểm hơn nhiều: khi cái chết của unit A ngay lập tức kéo theo cái chết của unit B, rồi B kéo theo C — và đặc biệt khi một Clash đồng thời (mục 3.6.3) khiến *cả hai* unit cùng gục ngã trong cùng một khoảnh khắc — engine rất dễ đánh mất dấu vết: phản ứng nào đã kích hoạt, phản ứng nào còn đang chờ, không còn gì là chắc chắn nữa.

**Lời giải — trì hoãn thay vì xử lý ngay:** Thay vì xử lý tức khắc, mỗi khi chỉ số máu của một unit chạm 0, nó chỉ đơn giản được *đánh dấu* (để không bao giờ bị đưa vào danh sách chờ hai lần) và xếp vào một ngăn xếp chờ — Death Stack. Lượt giao tranh hiện tại tiếp tục diễn ra bình thường; chỉ sau khi nó kết thúc, một quy trình riêng mới rút cạn toàn bộ ngăn xếp đó, theo đúng thứ tự, từ đầu đến cuối, trước khi bất kỳ điều gì khác được phép xảy ra.

**Vì sao là Ngăn xếp (LIFO) chứ không phải Hàng đợi (FIFO) — lý do thực sự:** Đây là quyết định có chủ đích, không phải ngẫu nhiên. Khi việc giải quyết một cái chết làm phát sinh một cái chết *hoàn toàn mới* ngay giữa chừng — ví dụ Anubis ban Reborn cho một đồng minh, đồng minh đó hồi sinh, kỹ năng nhân bội của Osiris kích hoạt trên nó, và chính hành động đó vô tình khiến một unit thứ ba gục ngã — chuỗi mới phát sinh ấy cần được giải quyết *trọn vẹn* trước khi engine quay lại tiếp tục chuỗi ban đầu. Ngăn xếp đảm bảo đúng tính chất đó một cách tự nhiên: chuỗi được mở ra gần đây nhất luôn là chuỗi được khép lại đầu tiên. Đây chính là điều kiện kỹ thuật đứng sau trải nghiệm mà người chơi quan sát được ở mục 3.6.5 — "luôn thấy được toàn bộ hệ quả của một cái chết theo đúng trật tự nhân–quả, không bao giờ bị cắt ngang giữa chừng".

Quy trình rút ngăn xếp, nhìn từ trên xuống, vận hành theo một chu trình lặp: lấy ra cái chết gần nhất → kích hoạt kỹ năng "khi chết" của chính nó → quét lại toàn bộ sân để phát hiện bất kỳ cái chết mới nào vừa phát sinh (và xếp chúng vào ngăn xếp) → phát sự kiện "khi đồng minh chết" cho từng đồng minh còn sống theo đúng thứ tự đã nêu ở mục 4.3.1 → quét lại lần nữa → lặp lại cho đến khi ngăn xếp trống hoàn toàn. Chỉ sau đó, engine mới chuyển sang bước "dọn sân": với từng unit đã được đánh dấu chết, áp dụng Reborn nếu còn quyền hồi sinh, hoặc gỡ hẳn khỏi sân nếu không. Và chỉ sau khi sân đã sạch, một lượt triệu hồi đang chờ (nếu có) mới được giải phóng — đúng một lượt mỗi lần — để nếu unit vừa xuất hiện lại trở thành nạn nhân của một phản ứng dây chuyền khác (chẳng hạn bị Sekhmet nuốt ngay khi vừa xuất hiện), toàn bộ chu trình phía trên lại được kích hoạt lại từ đầu cho đến khi mọi thứ thực sự lắng xuống. Một bộ đếm an toàn — giới hạn 500 vòng lặp — tồn tại như tuyến phòng thủ cuối cùng: không phải một phần của thiết kế, mà là một tấm lưới bắt lỗi, ghi nhận cảnh báo thay vì để cả trò chơi treo cứng, phòng khi một sự kết hợp kỹ năng nào đó tạo ra một chuỗi mà không ai lường trước được khi thiết kế.

> **[HÌNH 4.2 — Quy Trình Rút Ngăn Xếp Cái Chết]** *Sơ đồ luồng dạng vòng lặp lồng nhau: "lấy cái chết gần nhất" → "kích hoạt phản ứng của chính nó" → "quét cái chết mới" → "phát sự kiện cho đồng minh" → quay lại nếu ngăn xếp còn phần tử; khi trống → "áp Reborn / gỡ khỏi sân" → "giải phóng một lượt triệu hồi đang chờ" → quay lại đầu chu trình nếu có biến động mới. Mũi tên màu đỏ cho nhánh loại bỏ, xanh lam cho nhánh hồi sinh.*

---

## 4.5 Kiểm Chứng Tính Headless — Và Một Dấu Chấm Hỏi Nhỏ

Yêu cầu "headless-compatible" nghe có vẻ trừu tượng — cho đến khi nó va chạm với một tình huống rất cụ thể: khi một kỹ năng kiểu "tấn công thì nhận coin" kích hoạt, engine kỹ năng cần cộng tiền vào ví của người chơi. Trong một trận đấu thật, điều đó có nghĩa là gọi đến `GameManager` — nơi duy nhất nắm giữ trạng thái ví tiền. Nhưng trong một phiên huấn luyện, `GameManager` *không tồn tại* — chưa từng có cảnh nào được dựng lên, chưa từng có singleton nào được khởi tạo. Một lệnh gọi trực tiếp sẽ ném ra lỗi tham chiếu rỗng và làm sập cả tiến trình — có thể là phiên mô phỏng thứ năm mươi nghìn của đêm hôm đó.

Có hai hướng để giải quyết: rải khắp nơi những câu kiểm tra "nếu `GameManager` tồn tại thì mới gọi" (dài dòng, và chỉ cần quên một chỗ là đủ để sập), hoặc — hướng đã được chọn — dựa hẳn vào một toán tử duy nhất. Toán tử điều kiện rỗng biến toàn bộ lệnh gọi thành một hành động "không làm gì cả" trong im lặng khi đối tượng không tồn tại — không nhánh rẽ, không ngoại lệ, không cần xử lý đặc biệt. Một dấu chấm hỏi (`?`) chính là toàn bộ cây cầu nối giữa "đang chạy trong một trận đấu thật" và "đang chạy trong một hộp cát chưa từng được báo cho biết rằng có một trò chơi đang tồn tại".

Trong huấn luyện, kinh tế của bot không hề bị bỏ qua — nó được một hệ thống kinh tế độc lập, thuộc sở hữu riêng của từng `BotAgent`, quản lý hoàn toàn trong một không gian khép kín, tách biệt khỏi mọi trạng thái toàn cục. Cùng một bộ luật chi tiêu chi phối cả người chơi thật lẫn bot — chỉ khác ở chỗ bot không bao giờ cần đến `GameManager` thật để thực thi những luật đó.

---

## 4.6 Từ Mã Nguồn Đến Năm Chiến Binh — Pipeline Huấn Luyện Khép Kín

Mọi quyết định ở các mục trên hội tụ lại thành một quy trình duy nhất, vận hành hoàn toàn tự động từ đầu đến cuối. Một kịch bản dòng lệnh khởi động Unity ở chế độ không giao diện — không cửa sổ, không bộ dựng hình, chỉ thuần xử lý — và gọi vào điểm khởi đầu huấn luyện: nơi `GATrainer` được dựng lên, điều khiển `GameSimulator` chạy qua hàng trăm nghìn trận đấu (432.000), tiến hóa năm quần thể độc lập theo năm triết lý chơi khác biệt, và cuối cùng ghi lại những cá thể sống sót tốt nhất vào một file kết quả. Lần tiếp theo trò chơi thật khởi động, `AIManager` đọc file đó và đưa năm `BotAgent` đã được huấn luyện đầy đủ vào cuộc sống.

> **[HÌNH 4.3 — Pipeline Huấn Luyện Khép Kín]** *Sơ đồ ngang sáu bước: kịch bản khởi động → Unity chế độ không giao diện → điểm vào huấn luyện → [GATrainer điều khiển GameSimulator × 432.000 trận] → file kết quả huấn luyện → AIManager nạp 5 BotAgent vào game thật. Phần headless đặt trong khung xám; phần game runtime tô xanh nhạt.*

Pipeline này chính là khoản "trả lại" cho mọi ràng buộc đã được tôn trọng dọc suốt năm mục trước: chỉ cần một trong số chúng bị phá vỡ — dữ liệu hóa, tính headless, phụ thuộc một chiều, nguyên tắc tính-toán-trước-trình-diễn-sau, hay những cánh cổng tự khóa chống đệ quy — toàn bộ chuỗi này sẽ đứt gãy ở đâu đó, và 432.000 trận đấu sẽ không bao giờ chạy trọn vẹn được.

---

## 4.7 Nhìn Lại Hành Trình

Quay lại ba ràng buộc đặt ra ở mục 4.1 — mỗi ràng buộc, khi đối chiếu với những gì vừa được kể, hóa ra không phải một khẩu hiệu trừu tượng mà là một sợi chỉ xuyên suốt mọi quyết định:

**Data-driven** trở thành hiện thực qua việc định nghĩa lá bài và kỹ năng đều là dữ liệu thuần — không một dòng code riêng nào cho "Anubis" hay "Gilgamesh"; thay đổi một con số trong file dữ liệu là đủ để cân bằng lại cả một bộ tộc.

**Headless-compatible** trở thành hiện thực qua ranh giới rõ ràng giữa mã thuần và mã gắn với engine, qua nguyên tắc tính-toán-trước-trình-diễn-sau biến `TurnRecord` thành một cây cầu tùy chọn chứ không phải một phần bắt buộc của luật chơi, và qua một toán tử nhỏ bé đủ sức ngăn cả hệ thống sụp đổ khi chạy trong một thế giới không có giao diện.

**Phụ thuộc một chiều** trở thành hiện thực qua việc tầng luật chơi không bao giờ "biết" đến sự tồn tại của tầng quản lý hay giao diện — và nhờ vậy, tầng AI có thể đứng tách biệt, dùng đúng những luật chơi mà người chơi thật trải nghiệm, mà không cần venture vào bất kỳ thứ gì nằm phía trên nó.

Nhìn lại, kỹ thuật ở đây chưa bao giờ là một giai đoạn "đến sau" thiết kế — nó là một cuộc thương lượng liên tục giữa điều mà trò chơi *muốn trở thành* và điều có thể thực sự được xây dựng, vận hành, và huấn luyện hàng trăm nghìn lần mỗi đêm. Toàn bộ hành trình ấy — ba ràng buộc, ba câu hỏi kiến trúc, một vấn đề khó nhất, và một pipeline khép kín — tồn tại vì một lý do duy nhất: để "năm chiến binh" nhắc đến ở mục 4.6 có một sân chơi đáng để học. Câu chuyện về việc chúng học như thế nào — và học được gì — được kể tiếp ở Chương 5.

---

*[Kết thúc Chương 4 — Tiếp theo: Chương 5 — Thuật Toán Di Truyền Và Hệ Thống AI]*
