# KẾ HOẠCH THỰC HIỆN

Bảng dưới đây tóm tắt tiến độ thực hiện đề tài qua 15 tuần, tính từ thời điểm chọn đề tài (cuối tháng 02/2026) đến hiện tại (10/06/2026).

| STT | Thời gian thực hiện | Công việc | Ghi chú |
|---|---|---|---|
| 1 | Tuần 1 (26/02 – 04/03) | Chọn đề tài, khảo sát hướng nghiên cứu kết hợp game Auto Chess với AI dựa trên Thuật toán Di truyền (GA); bắt đầu tìm hiểu Godot Engine | |
| 2 | Tuần 2–3 (05/03 – 18/03) | Quyết định chuyển từ Godot sang Unity Engine; học lý thuyết GA (mã hóa nhiễm sắc thể, chọn lọc, lai ghép, đột biến) song song với học Unity Engine cơ bản (GameObject, Component, Scene, Prefab, UI Canvas) | Đổi engine: Godot → Unity; nền tảng cho Chương 2 |
| 3 | Tuần 4 (19/03 – 25/03) | Viết đề cương sơ bộ; phác thảo GDD: ý tưởng game, 3 chủng tộc (tribe), vòng lặp gameplay cốt lõi | |
| 4 | Tuần 5–7 (26/03 – 15/04) | Xây dựng prototype Unity đầu tiên: bàn cờ, shop, combat cơ bản, một số lá bài thử nghiệm | Prototype v1 |
| 5 | Tuần 8 (16/04 – 22/04) | Dọn dẹp dự án và viết lại core system (v2): chuyển cơ chế lượt cứng sang hệ thống Turn-Tier, sửa logic coin/ATK/DEF, bắt đầu bổ sung Card | Bắt đầu theo dõi bằng Git (17/04) |
| 6 | Tuần 9–10 (23/04 – 06/05) | Phát triển hệ thống thẻ bài: phân nhóm Magic card, bổ sung dữ liệu chủng tộc Babylon & Egypt, sửa logic shop/slot/growth | |
| 7 | Tuần 11 (07/05 – 13/05) | Xây hệ thống sự kiện Deploy/Destroy, mở rộng Ability (Taunt → thêm Reborn, Safeguard), thiết kế lại Card_Prefab, sửa bug summon | Viết note tiến độ (summary.md); lưu lại bản giao diện cũ trước khi cập nhật (11/05) |
| 8 | Tuần 12 (14/05 – 20/05) | Cải thiện giao diện: xây dựng lại layout Shop/Battle dựa trên bản cũ đã lưu, hệ thống targeting frontline/backline, sửa lỗi AI simulation và thứ tự slot | |
| 9 | Tuần 13 (21/05 – 27/05) | Refactor lớn shop/combat/spell/hiệu ứng hình ảnh; xây hệ thống AI bằng GA (chromosome 32-gene, multi-bot, script huấn luyện); hoàn thiện UI CardDetailPanel, PlayerCup, ShopTierPanel; refactor hệ thống buff chủng tộc | Mốc: GA 32-gene chạy được lần đầu |
| 10 | Tuần 14 (28/05 – 03/06) | Mở rộng chromosome lên 37-gene, cải tiến GA (2-point crossover, early stopping, chống bế tắc, thêm summonerBot), tinh chỉnh huấn luyện; bắt đầu viết tiểu luận (Chương 4, 6, Kết luận, Phụ lục) và TRAINING_GUIDE.md | Mốc: bản tiểu luận hoàn chỉnh đầu tiên (03/06) |
| 11 | Tuần 15 (04/06 – 10/06) | Sửa số liệu sai lệch khi đối chiếu code, tái cấu trúc toàn bộ dàn ý tiểu luận (Chương 2–6 + Phụ lục B/C) theo phản hồi, tạo hình minh họa, ghép bản v2 | Kế hoạch kết thúc tại đây; các cải thiện còn lại (giao diện, review tổng thể, ghép Word/PDF...) sẽ được hoàn thành ở giai đoạn sau |
