# PHONG CÁCH HÀNH VĂN
### Tiểu Luận Chuyên Ngành — "Phát triển Game Auto Chess với Trí tuệ Nhân tạo Dựa trên Thuật toán Di truyền"

---

## 1. Bản Sắc Cốt Lõi

Tiểu luận này không phải là tài liệu kỹ thuật (technical documentation), không phải là wiki, không phải là README. Nó là **một lập luận học thuật kéo dài** — dẫn người đọc từ câu hỏi ban đầu đến kết luận cuối, qua từng bước có chứng minh và phân tích. Mỗi chương, mỗi mục, mỗi đoạn văn đều phục vụ cho lập luận trung tâm đó.

**Người đọc lý tưởng** là sinh viên CNTT năm 3–4 hoặc giảng viên có nền tảng kỹ thuật nhưng chưa làm game — họ hiểu khái niệm lập trình cơ bản, nhưng cần được dẫn dắt qua thiết kế cụ thể và lý do đằng sau mỗi quyết định.

**Nguyên tắc bao trùm:** Bất cứ khi nào bạn viết "hệ thống X làm Y" — hãy hỏi thêm: *tại sao cần Y, và tại sao X là cách làm Y tốt hơn các cách khác?* Câu trả lời cho câu hỏi đó mới là nội dung của tiểu luận.

---

## 2. Giọng Văn Và Người Viết

### 2.1 Xưng hô

- Dùng **"chúng tôi"** khi nói về nhóm tác giả hoặc quyết định thiết kế: *"Chúng tôi chọn cách tiếp cận này vì..."*
- Dùng **thể bị động / vô nhân xưng** khi mô tả hệ thống hoạt động: *"Chromosome được khởi tạo với...", "Quần thể hội tụ sau..."*
- **Tránh** "tác giả" tự xưng về mình theo ngôi thứ ba — nghe cứng và xa cách.

### 2.2 Mức độ trang trọng

Trang trọng nhưng không khô khan. Được phép dùng:
- Câu hỏi tu từ để dẫn dắt: *"Vậy điều gì quyết định bot 'học' chiến lược nào?"*
- Mệnh đề tương phản trực tiếp: *"Không phải vì X, mà vì Y."*
- Thừa nhận trực tiếp một hạn chế: *"Cách tiếp cận này có điểm yếu rõ ràng: ..."*

**Không được phép:**
- Ngôn ngữ quảng cáo: *"hệ thống mạnh mẽ", "giải pháp tuyệt vời"*
- Câu cảm thán không có dữ liệu đi kèm
- Hedging quá mức: *"có thể nói rằng", "dường như", "đôi khi"* — hãy nói thẳng hoặc đừng nói

---

## 3. Cấu Trúc Lập Luận — Micro-Narrative

Mỗi đoạn văn, mỗi mục, mỗi chương đều có cấu trúc ba lớp:

```
[BỐI CẢNH / VẤN ĐỀ]  →  [PHÂN TÍCH / GIẢI PHÁP]  →  [HỆ QUẢ / KẾT NỐI]
```

### 3.1 Mở đầu một mục: Đặt vấn đề trước, kỹ thuật sau

❌ **Sai:** Bắt đầu ngay bằng mô tả kỹ thuật.
> "Hàm `Evaluate()` tính điểm của một card theo công thức: s = baseATK × genes[0] + ..."

✅ **Đúng:** Đặt câu hỏi hoặc vấn đề trước.
> "Trước khi BotAgent có thể quyết định *mua gì*, nó cần một thước đo để so sánh các lựa chọn. Thước đo đó không thể là một con số cố định — vì giá trị của một lá bài thay đổi tùy theo đội hình hiện tại, tình trạng kinh tế, và chiến lược đang theo đuổi. Đây là lý do `Evaluate()` tồn tại — không phải để 'tính điểm', mà để *dịch* trạng thái game thành một con số mà chromosome có thể so sánh."

### 3.2 Kết thúc một mục: Không để ý tưởng treo lơ lửng

❌ **Sai:** Kết thúc bằng mô tả cuối cùng, để đoạn tự cắt.
> "...Threshold này đảm bảo bot không bán unit cũ để mua unit mới kém hơn."

✅ **Đúng:** Tổng kết ý nghĩa hoặc kết nối sang mục tiếp theo.
> "...Threshold này đảm bảo bot không bán unit cũ để mua unit mới kém hơn. Nói cách khác, BuyUnitsPhase không chỉ là vòng lặp 'mua cái tốt nhất có thể' — nó là vòng lặp *cải thiện có điều kiện*, chỉ thực hiện khi lợi ích rõ ràng vượt quá chi phí cơ hội. Đây là triết lý kinh tế mà toàn bộ các phase sau đều kế thừa."

### 3.3 Giải thích một quyết định thiết kế

Mẫu chuẩn: **Vấn đề → Phương án → Chọn gì → Tại sao → Hệ quả**

> "Một câu hỏi thiết kế không tầm thường là: khi 3 bản sao cần merge, giữ lại bản nào? Câu trả lời đơn giản nhất là giữ bản đầu tiên, hoặc bản có base stat cao nhất — cả hai đều dễ cài đặt. Nhưng chúng tôi chọn tiêu chí khác: giữ lại bản sao có *tổng bonus tích lũy lớn nhất* (permanentATKBonus + growthATKBonus + ...). Lý do: các bonus này là kết quả của nhiều lượt đầu tư — spell cast vào unit đó, nhiều lần StartOfBattle trigger. Bỏ đi unit đó là bỏ đi toàn bộ lịch sử đầu tư. Tiêu chí này đảm bảo merge không bao giờ trở thành hành động phá hủy giá trị đã tích lũy."

---

## 4. Trình Bày Nội Dung Kỹ Thuật

### 4.1 Code block: không dump — dẫn dắt

Mỗi code block phải được bao bởi văn xuôi giải thích *trước* và *sau*. Văn xuôi trước cho biết người đọc cần chú ý điều gì; văn xuôi sau giải thích ý nghĩa của những gì vừa thấy.

❌ **Sai:**
> "Hàm khởi tạo quần thể như sau:"
> ```csharp
> [block code 40 dòng]
> ```
> "Như trên, quần thể được chia thành 5 nhóm."

✅ **Đúng:**
> "Thay vì khởi tạo ngẫu nhiên hoàn toàn, quần thể được chia thành 5 nhóm, mỗi nhóm được 'trỏ hướng' về một archetype khác nhau. Cơ chế này giải quyết một vấn đề thực tiễn quan trọng: trong không gian 37 chiều, chromosome Summoner — đòi hỏi đồng thời nhiều gene cụ thể ở mức cao — cực kỳ khó tìm ra bằng ngẫu nhiên thuần túy. Seeding giải quyết điều đó:"
> ```csharp
> case 2: // Summoner seed — chú ý genes[14] (Summon) và genes[27] (ProactiveSell thấp)
>     c.genes[14] = Random.Range(0.75f, 1.0f);
>     c.genes[27] = Random.Range(0.00f, 0.15f);
>     // ...
> ```
> "Hai gene được highlight ở đây không phải ngẫu nhiên: genes[14] cao bảo bot ưu tiên unit có Summon ability; genes[27] thấp ngăn bot bán đi những 'shell unit' trông yếu nhưng cần thiết cho chuỗi triệu hồi. Thiếu một trong hai, archetype Summoner không thành hình — đây là ràng buộc gene không thể tách rời."

### 4.2 Bảng số liệu: không bỏ ngỏ — hãy đọc thay cho người đọc

❌ **Sai:**
> "Kết quả training được trình bày trong bảng sau:"
> | Gen | Best | Avg | Niles% |
> | 0 | 19392 | 8553 | 52.8% |
> | 199 | 20275 | 10835 | 30.3% |

✅ **Đúng:**
> "Bảng dưới đây cho thấy hai điểm đáng chú ý: best fitness chỉ cải thiện 4,6% (từ 19.392 lên 20.275) trong khi avg cải thiện tới 26,7% — khoảng cách này phản ánh rằng warm-start đã cung cấp một chromosome gần-tối-ưu từ đầu, và phần lớn 200 thế hệ sau đó dành cho việc 'nâng sàn' toàn quần thể. Điểm thứ hai: Niles từ 52,8% (gen 0) xuống 30,3% (gen 199) — dấu hiệu cho thấy với scoring ưu tiên thắng/thua tuyệt đối, chromosome generalist (nhóm Other) tự nhiên cạnh tranh được tốt hơn so với specialist."
> | Gen | Best | Avg | Niles% | Other% |
> | 0 | 19.392 | 8.553 | 52,8% | 15,9% |
> | 199 | 20.275 | 10.835 | 30,3% | 34,7% |

### 4.3 Công thức và gene: luôn giải thích ý nghĩa, không chỉ cú pháp

❌ **Sai:**
> "progressScore = bestEver + avgEma × 0.45 + avgLate × 0.08 + avgCard × 0.035"

✅ **Đúng:**
> "Thay vì theo dõi chỉ một con số (best fitness), hệ thống plateau detection dùng `progressScore` — một chỉ số tổng hợp phản ánh *toàn diện hơn* sức khỏe của quá trình training: `bestEver` là kết quả tốt nhất đạt được; `avgEma` (exponential moving average) là xu hướng chất lượng trung bình; `avgLate` và `avgCard` đo sức mạnh board và tay bài. Hệ số nhân (0.45, 0.08, 0.035) phản ánh mức độ quan trọng tương đối: avg quan trọng gần bằng best, còn board power và card quality chỉ là tín hiệu phụ."

---

## 5. Kết Nối Giữa Các Phần

### 5.1 Câu chốt kết thúc một section (closing sentence)

Mỗi section (##) kết thúc bằng một câu hoặc đoạn ngắn tổng kết ý nghĩa, không chỉ mô tả. Có thể dùng các mẫu sau:

- **Tổng kết ý nghĩa:** *"Ba gene này cùng tạo ra một chính sách reroll đầy đủ — không chỉ 'có reroll hay không' mà là 'reroll như thế nào, bao nhiêu lần, và khi nào thì dừng lại'."*
- **Kết nối sang mục tiếp theo:** *"Nhưng dù reroll bao nhiêu lần, cuối cùng bot vẫn phải quyết định: mua card nào? Đó là bài toán của BuyUnitsPhase."*
- **Thừa nhận giới hạn:** *"Cơ chế này hoạt động tốt trong training, nhưng có một điểm mù quan trọng sẽ được phân tích ở mục 5.7."*

### 5.2 Câu chuyển tiếp giữa các section (transition sentence)

Đầu mỗi section (##) không bắt đầu ngay bằng nội dung kỹ thuật — cần một câu neo vào context trước đó:

- **Tham chiếu ngược:** *"Nếu BotAgent là người thực thi quyết định, thì GameSimulator là sân khấu nơi những quyết định đó được kiểm nghiệm."*
- **Đặt vấn đề mới:** *"Với quần thể đã khởi tạo, GA cần trả lời câu hỏi tiếp theo: chromosome nào tốt hơn — và tốt hơn theo tiêu chí gì?"*
- **Tương phản:** *"Seeding giải quyết được vấn đề khởi đầu. Nhưng nếu quần thể hội tụ quá sớm — tất cả chromosome trở nên giống nhau — thì những thế hệ sau chỉ là lãng phí. Đây là bài toán immigration."*

---

## 6. Tránh Các Lỗi Phổ Biến

### 6.1 Lỗi "Ghi chú kỹ thuật giả dạng văn xuôi"

Dấu hiệu nhận biết: đoạn văn là một danh sách các fact/feature được viết thành câu, không có lập luận nối kết chúng.

❌ **Sai:**
> "GATrainer có nhiều tính năng. Nó có thể chạy quick mode hoặc production mode. Quick mode dùng 30 pop và 40 gen. Production mode dùng 320 pop và 200 gen. Có early stopping khi plateau. Có warm-start từ library cũ. Có validation sau training."

✅ **Đúng:**
> "GATrainer hoạt động ở hai chế độ: quick mode (30 pop × 40 gen, ~2 phút) để kiểm tra logic trong quá trình phát triển, và production mode (320 pop × 200 gen, ~20–30 phút) để tạo ra bộ bot thực sự. Sự khác biệt không chỉ là quy mô — production mode bổ sung thêm ba cơ chế không có ở quick mode: warm-start từ library cũ (để không bắt đầu từ đầu mỗi lần), validation sau training (để không vô tình downgrade AI hiện có), và early stopping dựa trên composite progress score (để không lãng phí thời gian sau khi đã hội tụ). Ba cơ chế này cùng giải quyết vấn đề thực tiễn quan trọng nhất của GA trong production: *làm thế nào để mỗi lần chạy training đều tốt hơn hoặc ít nhất là không tệ hơn lần trước?*"

### 6.2 Lỗi "Con số không có ngữ cảnh"

❌ **Sai:** *"PLATEAU_PATIENCE = 28, PLATEAU_EPS = 150."*

✅ **Đúng:** *"PLATEAU_PATIENCE = 28 thế hệ — tức là nếu progressScore cải thiện ít hơn 150 điểm (PLATEAU_EPS) trong 28 thế hệ liên tiếp, training dừng lại. Con số 28 không phải tùy tiện: với 200 thế hệ tối đa và minStopGen = 75%, training không thể dừng trước thế hệ 150 dù sao — 28 thế hệ patience từ đó là khoảng 14% thời gian còn lại, đủ để phân biệt plateau thực sự với biến động ngẫu nhiên."*

### 6.3 Lỗi "Mô tả không phân biệt quan trọng và không quan trọng"

Không phải mọi chi tiết kỹ thuật đều cần cùng độ sâu giải thích. Hãy dùng nguyên tắc **tỉ lệ thuận giữa chiều sâu giải thích và tầm quan trọng của quyết định thiết kế**.

- Quyết định trung tâm (fitness function, chromosome design): giải thích đầy đủ với context và so sánh phương án
- Chi tiết cài đặt (constant values, helper functions): một câu hoặc inline comment trong code là đủ
- Công nghệ nền tảng (Unity, JSON serialization): không cần giải thích từ đầu, chỉ đề cập

### 6.4 Lỗi "Kết thúc chương không có landing"

Mỗi chương cần một đoạn kết thúc (~2–4 câu) tổng kết điều đã trình bày và dẫn sang chương tiếp theo. Không kết thúc bằng một mục kỹ thuật cuối cùng mà không có closure.

Mẫu câu kết chương:
> *"Toàn bộ hệ thống kỹ thuật mô tả trong chương này — TTE engine, combat resolver, headless trainer — đều phục vụ một mục tiêu duy nhất: tạo ra môi trường đủ trung thực để GA có thể học được chiến lược chơi thực sự, không phải chiến lược cho một game khác. Chương tiếp theo đi vào bên trong GA — phân tích từng lớp thiết kế của hệ thống AI mà môi trường này đã tạo ra."*

---

## 7. Kiểm Tra Nội Dung — Những Gì Có Thể Thiếu

Khi rà soát từng chương, đặt những câu hỏi sau:

| Câu hỏi kiểm tra | Dấu hiệu thiếu |
|---|---|
| Tại sao thiết kế này được chọn (thay vì phương án khác)? | Mô tả *cái gì* nhưng không có *tại sao* |
| Quyết định này có hệ quả gì với phần còn lại của hệ thống? | Mục đứng độc lập, không kết nối |
| Người đọc sẽ hiểu gì khác sau khi đọc mục này? | Mục chỉ mô tả lại code đã có |
| Có sự chênh lệch giữa design intention và thực tế không? | Không có bất kỳ thừa nhận hạn chế nào |
| Con số/kết quả nào là quan trọng nhất — và tại sao? | Nhiều số liệu nhưng không có câu "điều này có nghĩa là..." |

---

## 8. Những Đoạn Văn Mẫu Trong Bài — Học Từ Chính Tiểu Luận

Những đoạn sau đây trong bài **đã đạt chuẩn** và nên dùng làm thước đo:

**Mẫu 1 — Dẫn dắt kỹ thuật bằng ngữ cảnh** *(00_MoDau.md)*
> *"Đề tài này được hình thành từ câu hỏi: Liệu có thể xây dựng một hệ thống AI cho game Auto Chess mà không cần viết một dòng quy tắc cứng nào, chỉ dựa thuần túy vào quá trình tiến hóa tự nhiên?"*

**Mẫu 2 — Phân tích lý do chọn phương pháp** *(06_KetQua_DanhGia.md)*
> *"GA phù hợp với ba lý do cụ thể cho bài toán này: (1) không cần gradient; (2) khả thi trên CPU phần cứng cá nhân; (3) tự nhiên tạo ra diversity. Điều này không có nghĩa GA là lựa chọn tốt nhất về mặt tuyệt đối — mà là lựa chọn tốt nhất trong điều kiện ràng buộc cụ thể."*

**Mẫu 3 — Giải thích ý nghĩa của con số** *(05_2_BotAgent_Simulator_Trainer.md)*
> *"Best fitness không tăng sau gen 6, nhưng avg fitness tiếp tục tăng từ 2871 → ~3050 (+6.2%). Điều này cho thấy GA đang làm đúng vai trò của nó: không chỉ tìm ra cá thể tốt nhất mà nâng cao chất lượng trung bình của toàn quần thể."*

**Mẫu 4 — Thừa nhận giới hạn thẳng thắn** *(05_1_GA_TongQuan_Chromosome.md)*
> *"Gene[19] sOlympus là gene 'dead' nhất trong nhóm vì không có seeded archetype chuyên cho Olympus và không có bot được chọn theo sOlympus dominant."*

---

## 9. Checklist Trước Khi Hoàn Thiện Một Mục (##)

- [ ] Câu đầu tiên **không** bắt đầu bằng tên hàm/class/parameter
- [ ] Có ít nhất một câu giải thích *tại sao* quyết định này được đưa ra
- [ ] Code block (nếu có) được giới thiệu bằng văn xuôi và được phân tích sau
- [ ] Số liệu/hằng số (nếu có) được đặt trong ngữ cảnh ý nghĩa
- [ ] Câu cuối cùng tổng kết ý nghĩa hoặc kết nối sang mục tiếp theo
- [ ] Không có đoạn nào chỉ là danh sách tính năng viết thành câu
- [ ] Người đọc không cần đọc code để hiểu ý chính của mục

---

*File này là tài liệu tham chiếu nội bộ — không đưa vào bản in cuối.*
