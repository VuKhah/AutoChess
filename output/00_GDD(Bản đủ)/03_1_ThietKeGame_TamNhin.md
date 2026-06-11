# CHƯƠNG 3: THIẾT KẾ GAME — GAME DESIGN DOCUMENT

---

## 3.1 Tầm Nhìn Và Concept

### 3.1.1 Giới Thiệu Tựa Game

*AutoChess: Pantheon* (working title) là một tựa game auto battler một người chơi trên PC, phát triển trên Unity Engine 2022+ bằng C#. Ở trạng thái prototype hiện tại, game đã hoàn chỉnh đủ để người chơi trải nghiệm toàn bộ vòng lặp gameplay từ đầu đến cuối — từ màn chọn độ khó, qua các pha chuẩn bị và chiến đấu, đến màn kết thúc ván.

Đây là một tựa game chiến thuật xây dựng đội hình trong đó người chơi không trực tiếp điều khiển chiến đấu — thay vào đó, họ đưa ra các quyết định *trước* trận đấu: mua lá bài nào, sắp xếp đội hình ra sao, khi nào nên tiết kiệm, khi nào nên đầu tư. Sau pha chuẩn bị đó, hai đội tự động giao chiến theo cơ chế được lập trình sẵn, và kết quả phụ thuộc hoàn toàn vào chất lượng chiến lược của người chơi — không phải tốc độ bấm hay phản xạ tay.

Sự kết hợp này — **chiến lược dài hạn với chiến thuật theo lượt** — là điểm phân biệt cốt lõi của thể loại Auto Chess so với RTS (đòi hỏi micro-management thời gian thực) hay card game thuần túy (thiếu yếu tố chiến đấu tự động trực quan). Game được thiết kế để mỗi ván đấu kéo dài 15–25 phút, vừa đủ để cảm nhận sự phát triển chiến lược mà không quá dài để gây mệt mỏi.

---

### 3.1.2 Thế Giới Và Chủ Đề — Ba Nền Văn Minh Cổ Đại

Nếu thể loại Auto Chess là *cái gì*, thì chủ đề là *tại sao người chơi quan tâm*. Thay vì dùng chủ đề fantasy phương Tây phổ biến hay sci-fi tương lai, game này chọn hướng khai thác **thần thoại cổ đại từ ba nền văn minh** — mỗi nền văn minh tương ứng với một bộ tộc (tribe) trong game:

**Bộ tộc Babylon — Thần thoại Lưỡng Hà (Mesopotamia):**
Từ đồng bằng màu mỡ giữa sông Tigris và Euphrates, đế chế Babylon sinh ra những vị thần quyền uy nhất trong lịch sử nhân loại. Gilgamesh — vị vua huyền thoại hai phần ba là thần; Enki — thần trí tuệ và vực sâu; Ereskigal — nữ hoàng địa ngục; Nanna — thần mặt trăng; Pazuzu — ác thần gió… Trong game, bộ tộc Babylon được thiết kế xoay quanh **triết lý kinh tế và tích lũy**: các unit Babylon mạnh lên qua cơ chế hấp thụ, bán đồng minh, và trao chỉ số cho nhau — phản chiếu bản sắc của một nền văn minh xây dựng nên hệ thống thương mại và luật pháp đầu tiên của loài người.

**Bộ tộc Olympus — Thần thoại Hy Lạp:**
Từ đỉnh Olympus nhìn xuống thế gian, các vị thần Hy Lạp cai trị bằng sức mạnh thô thuần và ánh sáng huy hoàng. Olympus là bộ tộc của **tấn công thuần túy**: synergy bonus của bộ tộc này khuếch đại ATK toàn đội, biến mỗi đơn vị trên sân thành một cỗ máy gây sát thương. Đây là bộ tộc cho người chơi thích chiến lược aggro — tung đòn mạnh và kết thúc ván đấu sớm thay vì kéo dài đến giai đoạn late-game.

**Bộ tộc Niles — Thần thoại Ai Cập:**
Bên dòng sông Nile huyền bí, người Ai Cập cổ đại tin rằng cái chết không phải là kết thúc mà là bước chuyển tiếp sang một trạng thái tồn tại khác. Anubis dẫn đường sang thế giới bên kia; Osiris cai trị cõi chết; Sekhmet nuốt chửng linh hồn; Upamaki hấp thụ sức mạnh của kẻ bị tiêu diệt. Trong game, bộ tộc Niles được xây dựng hoàn toàn quanh **chu trình chết và tái sinh**: các unit Niles có synergy buff HP theo số lượng đồng minh, kết hợp với Reborn, SummonConsumed chain và OnAllyDeath triggers để tạo ra những trận chiến không bao giờ thực sự kết thúc cho đến lần chết cuối cùng.

Ba bộ tộc này không chỉ là nhãn phân loại — chúng định nghĩa ba **triết lý chơi** hoàn toàn khác nhau: Babylon xây dựng giá trị qua thời gian, Olympus áp đảo bằng sức mạnh tức thời, Niles tồn tại qua sự hỗn loạn.

> **[HÌNH 3.1 — Ba Bộ Tộc Và Triết Lý Chơi]** *Infographic ba cột: Babylon (biểu tượng Lưỡng Hà, màu vàng đồng — Economy & Accumulation), Olympus (biểu tượng Hy Lạp, màu xanh trời — Pure Aggression), Niles (biểu tượng Ai Cập, màu xanh lam đậm — Death & Rebirth). Mỗi cột kèm từ khóa chiến lược và ví dụ card tiêu biểu.* Người chơi có thể chọn đi theo một bộ tộc, kết hợp hai, hoặc xây dựng đội hình hybrid — đây là chiều sâu chiến thuật mà thiết kế hướng tới.

Về mặt thẩm mỹ, ba nền văn minh này chia sẻ một đặc điểm quan trọng: tất cả đều là **huyền thoại cổ đại đã tuyệt chủng** — không còn được thờ phụng trong thế giới hiện đại, nhưng vẫn giữ được sức hút văn hóa và tính biểu tượng mạnh mẽ. Điều này cho phép thiết kế tự do sáng tạo nhân vật mà không bị ràng buộc bởi sự nhạy cảm tôn giáo đương đại, đồng thời tạo ra cảm giác *huyền bí và sử thi* mà các chủ đề giả tưởng generic thường thiếu.

---

### 3.1.3 Core Fantasy — Lời Hứa Trải Nghiệm

Mỗi game thành công đều có thể trả lời được câu hỏi: *khi chơi game này, người chơi cảm thấy như thể họ đang làm gì?* Câu trả lời ngắn gọn đó — gọi là **core fantasy** — định hướng mọi quyết định thiết kế từ cơ chế gameplay đến giao diện người dùng.

Core fantasy của game này là:

> **"Tôi là một chiến lược gia triệu hồi và điều phối các vị thần cổ đại. Không phải tôi chiến đấu — tôi *sắp đặt* để các thần linh chiến đấu theo ý muốn của tôi."**

Lời hứa này bao gồm nhiều lớp. Lớp thứ nhất là **quyền năng vắng mặt**: bạn không cầm kiếm, nhưng mọi chiến thắng đều là của bạn — vì đội hình đó do bạn xây dựng, cách sắp xếp đó do bạn quyết định. Đây là cảm giác của một vị tướng nhìn từ xa trong khi quân đội thực thi chiến thuật của mình. Lớp thứ hai là **sự kết hợp bất ngờ**: khi Anubis ban Reborn cho đồng minh vừa chết, đồng minh đó hồi sinh ngay trước mắt Osiris đang chờ để nhân đôi chỉ số của nó — đó là một *combo* mà người chơi đã lên kế hoạch ba lượt trước. Niềm vui không đến từ phản xạ, mà đến từ *sự tiên liệu*. Lớp thứ ba là **câu chuyện của từng ván đấu**: mỗi ván là một hành trình riêng — đôi khi bạn đánh bạc theo bộ tộc Babylon và thành công; đôi khi shop ngẫu nhiên lại mở ra cơ hội Niles summon chain không ngờ tới. Không có hai ván nào giống nhau.

Để core fantasy này thành hiện thực, thiết kế phải đảm bảo hai điều kiện. Một là **tính dễ đọc của board**: ở bất kỳ thời điểm nào trong combat, người chơi phải có thể nhìn vào sân và hiểu được điều gì đang xảy ra — unit nào đang tấn công ai, ai sắp chết, ability nào vừa kích hoạt. Hai là **cảm giác agency trong chaos**: dù shop là ngẫu nhiên và combat là tự động, người chơi phải cảm thấy rằng mình *có thể ảnh hưởng đến kết quả* bằng các quyết định tốt hơn — không phải chỉ là may mắn.

---

### 3.1.4 Đối Tượng Người Chơi Mục Tiêu

Game hướng đến hai nhóm đối tượng chính, khác nhau về kinh nghiệm nhưng tìm kiếm cùng loại trải nghiệm:

**Nhóm 1 — Người chơi chiến lược có kinh nghiệm:**
Những người đã quen với Teamfight Tactics (TFT), Hearthstone Battlegrounds hay Dota Underlords và muốn một trải nghiệm tương tự nhưng gọn hơn, ít thời gian hơn và có câu chuyện thần thoại phong phú. Nhóm này đánh giá cao độ sâu hệ thống (nhiều loại ability, tribe synergy, merge economy) và sẵn sàng dành thời gian học cơ chế để master nó.

**Nhóm 2 — Người mới với thể loại Auto Battler:**
Những người tò mò về thể loại nhưng thấy TFT hay Hearthstone Battlegrounds quá phức tạp hoặc đòi hỏi quá nhiều thời gian (các game thương mại thường kéo dài 30–45 phút mỗi ván với 7–8 người chơi). Game này cung cấp một phiên bản **PvE có kiểm soát** — không áp lực từ người chơi thực khác, không sợ mất rank, học ở tốc độ của mình và thử nghiệm chiến lược tự do.

Điểm giao thoa giữa hai nhóm là **chiều dài ván đấu**: 15–25 phút là ngưỡng tối ưu — đủ dài để cảm nhận arc chiến lược (build lên, đấu tranh, thắng hay thua), nhưng không quá dài để một ván thua trở thành cả buổi tối phí đi.

Về mặt kỹ thuật, game không yêu cầu kết nối mạng, không có hệ thống tài khoản hay monetization — đây là thiết kế có chủ ý cho ngữ cảnh nghiên cứu học thuật, đặt toàn bộ focus vào cơ chế gameplay và hệ thống AI thay vì infrastructure vận hành.

---

### 3.1.5 Đặc Trưng Phân Biệt — Điều Game Này Làm Khác

Trong bức tranh rộng hơn của thể loại Auto Chess, game này định vị mình ở một góc rõ ràng qua ba đặc trưng phân biệt:

**Thứ nhất — Hệ thống ability data-driven hoàn toàn (TTE Engine):**
Trong TFT và Hearthstone Battlegrounds, mỗi champion/minion có code riêng cho ability của nó. Trong game này, toàn bộ 68 lá bài được định nghĩa bằng JSON — không có một dòng code C# nào dành riêng cho một lá bài cụ thể. Tất cả hành vi được biểu diễn qua ba trường `trigger`, `target`, `effect` và một tập modifier. Điều này có nghĩa là một thiết kế viên game có thể thêm lá bài hoàn toàn mới chỉ bằng cách sửa file JSON — không cần lập trình viên, không cần build lại project.

**Thứ hai — AI được sinh ra, không được lập trình:**
Không có một dòng quy tắc if-else nào được viết tay cho AI bot trong game này. Mọi hành vi chiến lược của bot — từ cách đánh giá một lá bài đến khi nào reroll, ưu tiên tribe nào, bán unit nào — đều được mã hóa trong 37 số thực và được tìm kiếm bởi Genetic Algorithm qua hàng chục nghìn trận đấu mô phỏng. Người chơi không đối đầu với những quy tắc do con người đặt ra mà với **chiến lược tiến hóa qua dữ liệu**. Đây là điểm khác biệt cơ bản so với mọi Auto Chess thương mại hiện tại.

**Thứ ba — Ba cấp độ khó với phong cách chơi thực sự khác nhau:**
Khi người chơi chọn "Dễ", "Trung bình" hay "Khó", họ không chỉ nhận được một bot kém hơn hay tốt hơn theo đường thẳng. Mỗi cấp độ tương ứng với một **archetype bot khác nhau** được train riêng biệt: Easy bot có gene[24] (reroll aggressiveness) thấp và không tối ưu hóa tribe synergy; Hard bot là generalist được chọn theo fitness tuyệt đối; các bot trung gian đặc thù hóa theo Babylon hay Niles. Kết quả là mỗi cấp độ không chỉ khó hơn mà còn chơi *khác hơn* — tạo ra trải nghiệm đa dạng hơn chỉ là tăng số liệu thống kê.

> **[HÌNH 3.2 — Screenshot Màn Hình Chọn Độ Khó]** *Ảnh chụp màn hình UI chọn Easy / Medium / Hard, thể hiện thiết kế giao diện và thông tin mô tả từng cấp độ hiển thị cho người chơi.*

---

*[Tiếp theo: Mục 3.2 — Vòng Lặp Gameplay Cốt Lõi (Core Loop)]*
