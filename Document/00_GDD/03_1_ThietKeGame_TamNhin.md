# CHƯƠNG 3: THIẾT KẾ GAME — GAME DESIGN DOCUMENT

---

## 3.1 Tầm Nhìn Và Concept

**Tên tựa game:** AutoChess: Pantheon *(working title)*  
**Thể loại:** Auto Battler / Auto Chess  
**Nền tảng:** PC (Windows), Unity 2022+, C#  
**Chế độ chơi:** Single-player PvE, tối đa 20 lượt  
**Trạng thái:** Prototype hoàn chỉnh

Người chơi không điều khiển chiến đấu trực tiếp — họ đưa ra quyết định *trước* trận: mua bài nào, sắp xếp đội hình, khi nào tiết kiệm/đầu tư. Sau pha chuẩn bị, hai đội tự động giao chiến; kết quả phụ thuộc hoàn toàn vào chất lượng chiến lược.

---

### 3.1.1 Ba Bộ Tộc — Ba Triết Lý Chơi

| Bộ tộc | Nguồn gốc | Cơ chế cốt lõi | Phong cách chơi |
|--------|-----------|----------------|-----------------|
| **Babylon** | Thần thoại Lưỡng Hà | Buff qua deploy/sell, hấp thụ chỉ số | Accumulation — snowball dài hạn |
| **Olympus** | Thần thoại Hy Lạp | ATK synergy qua combat events | Aggression — áp đảo sớm |
| **Niles** | Thần thoại Ai Cập | Chu trình chết–tái sinh, OnAllyDeath chain | Reaction — mạnh bất ngờ qua chuỗi trigger |

Người chơi có thể đi theo một bộ tộc, kết hợp hai, hoặc xây đội hình hybrid — đây là chiều sâu chiến thuật mà thiết kế hướng tới.

> **[HÌNH 3.1 — Ba Bộ Tộc Và Triết Lý Chơi]** *Infographic ba cột: Babylon (Accumulation), Olympus (Aggression), Niles (Death & Rebirth). Mỗi cột kèm từ khóa chiến lược và unit tiêu biểu.*

Core fantasy: *"Tôi là chiến lược gia sắp đặt để các thần linh cổ đại chiến đấu theo ý muốn của tôi."* Niềm vui đến từ *sự tiên liệu* — combo Anubis → Reborn → Osiris được lên kế hoạch ba lượt trước là khoảnh khắc đặc trưng nhất của game.

---

### 3.1.2 Đặc Trưng Phân Biệt

**TTE Engine data-driven:** Toàn bộ 68 lá bài định nghĩa qua JSON (trigger/target/effect), không có code C# riêng cho bất kỳ lá bài nào. Thiết kế viên có thể thêm card mới chỉ bằng sửa JSON, không cần lập trình viên.

**AI được sinh ra, không được lập trình:** Mọi hành vi bot — đánh giá card, quyết định reroll, ưu tiên tribe, bán unit — phát sinh từ 37 số thực được Genetic Algorithm tìm kiếm qua hàng chục nghìn trận mô phỏng. Không có một dòng if-else quy tắc nào được viết tay.

**Ba cấp độ với phong cách chơi thực sự khác nhau:** Easy/Medium/Hard tương ứng với các archetype bot được train riêng biệt (babylonBot, nileBot, resilientBot…) — không chỉ khó hơn mà còn chơi *khác hơn*.

> **[HÌNH 3.2 — Screenshot Màn Hình Chọn Độ Khó]** *Ảnh chụp UI chọn Easy / Medium / Hard với mô tả từng cấp độ.*

---

*[Tiếp theo: Mục 3.2 — Vòng Lặp Gameplay Cốt Lõi (Core Loop)]*
