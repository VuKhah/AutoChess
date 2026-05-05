# (2026-04-22)

**Đề tài:** Phát triển Hệ thống Trí tuệ Nhân tạo bằng Giải thuật Di truyền (Genetic Algorithm) trong môi trường Giả lập Auto-Battler "Case Battle"

**Giai đoạn báo cáo:** Hoàn tất Kiến trúc Lõi (Core Framework), Động cơ Kỹ năng (TTE Engine) và Chuẩn bị Môi trường Huấn luyện (Headless Simulation).

---

## 1. TỔNG QUAN DỰ ÁN VÀ MỤC TIÊU CỐT LÕI

Dự án "Case Battle" không nhằm mục đích tạo ra một trò chơi thương mại thuần túy, mà sử dụng gameplay như một (sandbox) để huấn luyện AI. Bài toán đặt ra là dạng **"Decision Under Constraint"** (Quyết định dưới các ràng buộc tài nguyên):

- AI phải đưa ra toàn bộ quyết định chiến thuật (mua/bán lính, ép bài phép, sắp xếp đội hình 6 ô) trong **Pha Chuẩn Bị**.
- Khi bước sang **Pha Chiến Đấu**, toàn bộ quá trình sẽ diễn ra tự động và tất định (Deterministic). AI học cách tối ưu hóa các quyết định trước đó để giành chiến thắng.

## 2. HOÀN THIỆN LUẬT CHƠI VÀ KIẾN TRÚC MÔ PHỎNG

Đến thời điểm hiện tại, hệ thống luật chơi cốt lõi đã được lập trình hoàn thiện trên Unity, đảm bảo tính chặt chẽ tuyệt đối (Bug-free) để phục vụ việc huấn luyện hàng vạn trận đấu của AI:

- **Vòng lặp Kinh tế (Economy Loop):** Cố định tài nguyên (Reset về 10 Coin mỗi Turn) kết hợp với các Unit mang kỹ năng Kinh tế (tự động đào Coin đầu hiệp). Cửa hàng có giới hạn (Cost = 3) và mở khóa bài (Tier) theo tiến trình Lượt (Lượt/2).
- **Chiến đấu Tất định (Deterministic Combat):**
    - Hệ thống xác định mục tiêu chuẩn xác: Ưu tiên lính có kỹ năng khiêu khích (`Taunt`), đánh đối diện, hoặc lan ra lân cận (Ripple Search).
    - Cơ chế sát thương đồng thời (Simultaneous Target Damage), đảm bảo công bằng.
    - Điều kiện phân định Thắng/Thua/Hòa rõ ràng dựa trên lượng lính sống sót cuối trận, tác động trực tiếp đến Cup (Điểm số) và HP của người chơi/AI.

## 3. ĐỘT PHÁ KỸ THUẬT: ĐỘNG CƠ KỸ NĂNG "TTE ENGINE"

Biến tiếng triển trong giai đoạn này là việc **đập bỏ hệ thống Hardcode (gắn cứng mã) để chuyển sang kiến trúc Hướng dữ liệu (Data-Driven)**. Em đã xây dựng thành công **TTE Engine (Trigger - Target - Effect)**:

- **Logic:** Mọi kỹ năng của thẻ bài giờ đây được định nghĩa bằng các bộ số nguyên trong file `JSON` thay vì viết lệnh `if/else` cứng trong mã nguồn C#.
    - `TriggerType`: OnTurnStart, OnTakeDamage, OnDeath...
    - `TargetType`: Self, Enemy, RandomAlly, AllAllies...
    - `EffectType`: AddStats, Heal, DealDamage, Reborn, GainCoin...
- **Kết quả:** Hệ thống hiện tại có thể vận hành 60 lá bài (40 Units và 20 Bài Phép) với hiệu ứng phức tạp (Phản sát thương, Hồi sinh, Lan truyền sức mạnh khi chết...) mà không cần phải viết thêm logic can thiệp riêng lẻ. AI có thể đọc trực tiếp các thông số này để tự chấm điểm lá bài.

## 4. TỐI ƯU HÓA VÀ GỠ LỖI HỆ THỐNG (DEBUGGING)

Để đảm bảo nguyên lý "Garbage In, Garbage Out" không xảy ra (AI học nhầm chiến thuật do game có bug), em đã thực hiện các phương pháp kiểm chứng phần mềm nâng cao:

- Test thủ công để kiểm tra các luồng kinh tế, logic kích hoạt Taunt, tính toán sát thương mà không cần render đồ họa.
- **Xử lý lỗi Đồng bộ (Desync) UI và Logic:** Khắc phục triệt để lỗi "Phantom Combat" (Unit đã chết nhưng vẫn nhận sát thương ở lượt sau do bất đồng bộ giữa Log và Render). Di dời hàm kiểm tra sự sống (`OnDeath`) vào sâu trong Pipeline chiến đấu.
- **Xử lý biến logic (Reverse Damage):** Gỡ lỗi luồng gán sát thương chéo giữa Kẻ công và Kẻ thủ, đảm bảo môi trường chiến đấu hoạt động chuẩn xác 100%.

## 5. TIẾN ĐỘ AI

Hệ thống lõi đã sẵn sàng, em đang chuyển trọng tâm sang lớp AI. Các thành phần đã hoàn thành bao gồm:

- **Cấu trúc Não bộ AI (`Chromosome`):** AI đã sở hữu bộ Gen để đánh giá (Evaluate) bài trong Shop. Thay vì đọc tên kỹ năng, AI giờ đây tự động phân tích ma trận TTE (Ví dụ: Thấy bài có `OnDeath` + `AddStats` sẽ tự nhận diện đây là bài hiến tế và tăng điểm ưu tiên nếu Gen của nó thiên về chiến thuật này).
- **Tác nhân AI (`BotAgent`):** Đã hoàn thiện logic mua bán cho đến khi hết tiền hoặc hết chỗ trên bàn cờ.

## **6. Lộ trình Giai đoạn 4 (Sắp tới):**

1. Design lại giao diện cho đẹp hơn, dể nhìn hơn. thêm hiệu ứng hoạt ảnh và rung cho một số sự kiện như va chạm của lá bài, hiệu ứng vỡ, Reborn..
2. Sẵn sàng data sạch cho 60 lá bài
3. **Thiết lập Headless Simulation:** Cho hàng ngàn cá thể AI giao đấu ngầm (không render đồ họa UI) để tiết kiệm tài nguyên hệ thống.
4. **Định nghĩa Hàm Thích nghi (Fitness Function):** Đánh giá các cá thể dựa trên tỷ lệ thắng (Win Rate / Cups) và máu bảo toàn.
5. **Vòng lặp Tiến hóa (Evolutionary Loop):** Cài đặt phép Lai ghép (Crossover) để trao đổi trọng số Gen giữa các cá thể xuất sắc, và Đột biến (Mutation) để tìm ra Meta game mới.
6. **Xuất dữ liệu:** Lọc ra bộ Gen Boss (`AI_Library.json`) siêu việt nhất để tích hợp vào Gameplay thực tế cho người chơi đấu lại.

---

Dự án đã vượt qua giai đoạn khó khăn nhất về mặt đồng bộ hóa kiến trúc logic hệ thống. Việc sử dụng TTE Engine và Data-driven approach giúp dự án có khả năng mở rộng quy mô (Scale-up) cực tốt. Môi trường Giả lập hiện tại đã hoàn toàn "sạch lỗi" và sẵn sàng 90% cho thuật toán Di truyền chạy với cường độ cao.

---