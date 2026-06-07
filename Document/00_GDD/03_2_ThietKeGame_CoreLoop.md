## 3.2 Vòng Lặp Gameplay Cốt Lõi (Core Loop)

### 3.2.1 Cấu Trúc Một Ván Đấu

Người chơi bắt đầu với 7 HP, 0 cup. Mục tiêu: tích đủ 10 cup (mỗi trận thắng +1) trước khi HP về 0, trong tối đa 20 lượt.

```
┌──────────────────────────────────────────────────────────────────┐
│                    VÒNG LẶP MỘT VÁN ĐẤU                         │
│                                                                  │
│   [BẮT ĐẦU] HP = 7, Cup = 0, Lượt = 1                           │
│       │                                                          │
│       ▼                                                          │
│   ╔═══════════════╗                                              │
│   ║  SHOP PHASE   ║  ← nhận 10 coin, shop mới, quyết định       │
│   ╚═══════╤═══════╝                                              │
│           │  [Nhấn Fight]                                        │
│           ▼                                                      │
│   ╔═══════════════╗                                              │
│   ║ COMBAT PHASE  ║  ← chiến đấu tự động, tối đa 50 bước        │
│   ╚═══════╤═══════╝                                              │
│           │                                                      │
│     ┌─────┴──────┐                                               │
│  Thắng?      Thua?      Hòa?                                     │
│  Cup +1      HP −1     (không đổi)                               │
│     │          │           │                                     │
│     └────┬─────┴───────────┘                                     │
│          ▼                                                       │
│   ┌──────────────────────────────────────┐                       │
│   │ Cup ≥ 10? → Thắng ván                │                       │
│   │ HP ≤ 0?  → Thua ván                  │                       │
│   │ Lượt > 20? → Thắng ván (vượt thời hạn)                      │
│   │ Còn lại  → Lượt + 1, quay lại Shop  │                       │
│   └──────────────────────────────────────┘                       │
└──────────────────────────────────────────────────────────────────┘
```

> **[HÌNH 3.3 — Sơ Đồ Vòng Lặp Một Ván Đấu]** *Flowchart hoàn chỉnh dựa trên sơ đồ ASCII bên trên, màu phân biệt Shop Phase (xanh lá), Combat Phase (đỏ), điều kiện kết thúc (vàng).*

---

### 3.2.2 Pha Chuẩn Bị — Shop Phase

Đầu mỗi lượt, người chơi nhận coin theo công thức:

```
CurrentCoin = 10 + bonusNextTurn + permanentIncomeBonus
```

Thu nhập cơ sở **10 coin/lượt** cố định — không có streak bonus hay interest. Shop tự động làm mới 7 lá (5 unit + 2 spell) theo shop tier tăng tự động:

```
shopTier = clamp( ⌊(currentTurn + 1) / 2⌋, 1, 6 )
```

| Lượt | Shop Tier | Ý nghĩa |
|------|-----------|---------|
| 1–2  | Tier 1    | Unit tier thấp, chi phí 1 coin |
| 3–4  | Tier 2    | Bắt đầu thấy unit tier 2 |
| 5–6  | Tier 3    | Pool đa dạng, unit tier 3 phổ biến |
| 7–8  | Tier 4    | Unit mạnh bắt đầu xuất hiện |
| 9–10 | Tier 5    | Gần đến tier 5–6 |
| 11+  | Tier 6    | Pool đầy đủ, mọi tier |

Tier shop tăng tự động (không tốn coin) — khác biệt so với TFT/Hearthstone Battlegrounds, đặt focus hoàn toàn vào quyết định *mua gì* thay vì *lên level khi nào*.

**7 hành động trong Shop Phase:**

1. **Mua + đặt unit lên sân** — trigger `OnDeploy`, nhận tribe synergy ngay
2. **Giữ unit trong tay** — chờ merge, vẫn nhận global tribe buff
3. **Bán unit → +1 coin** bất kể giá mua hay mergeLevel (loại bỏ sunk cost)
4. **Reroll → 1 coin** — lấy 7 lá mới, hủy freeze
5. **Freeze → 0 coin** — giữ shop sang lượt sau, chỉ điền ô trống
6. **Mua + dùng spell** — tác động ngay, biến mất
7. **Nhấn Fight** — trigger `EndTurnShop` toàn sân trước combat

> **[HÌNH 3.4 — Giao Diện Shop Phase]** *Ảnh chụp màn hình Shop Phase: 7 ô shop, sân 7 slot, hand, HUD với HP/Cup/Coin, nút Reroll/Lock/BẮT ĐẦU.*

---

### 3.2.3 Pha Chiến Đấu — Combat Phase

**Nguyên tắc cốt lõi — tính toán trước, trình diễn sau:** Toàn bộ một lượt chiến đấu được tính toán đầy đủ và tức thời thành một chuỗi sự kiện xác định, sau đó mới được "phát lại" tuần tự cho người chơi quan sát bằng hoạt ảnh (khoảng 0.1 giây/hành động, tổng cộng 5–15 giây mỗi trận). Đây chính là sự áp dụng trực tiếp nguyên tắc tách biệt tính toán và trình diễn đã giới thiệu ở mục 2.2.3 — và là điều cho phép cùng một bộ luật chiến đấu vừa vận hành trong một trận đấu thực sự, vừa chạy lặp lại hàng nghìn lần liên tiếp ở chế độ huấn luyện AI mà không cần viết thêm bất kỳ phiên bản nào khác (trình bày chi tiết ở Chương 4).

Một lượt chiến đấu trải qua ba giai đoạn nối tiếp:
- *Thiết lập:* Toàn bộ unit kích hoạt các kỹ năng "đầu trận"
- *Vòng lặp giao tranh (tối đa 50 round):* Các unit lần lượt tấn công theo hàng đợi (mục 3.6.2), chọn mục tiêu theo thứ tự ưu tiên (mục 3.6.4); mỗi khi có unit gục ngã, toàn bộ chuỗi phản ứng dây chuyền phát sinh từ đó được giải quyết trọn vẹn trước khi giao tranh tiếp tục (mục 3.6.5)
- *Kết thúc:* Một phía bị xóa sổ hoàn toàn → phân định thắng/thua; hết 50 round mà cả hai còn unit → hòa

> **[HÌNH 3.5 — Giao Diện Combat Phase]** *Ảnh chụp Combat Phase: sân người chơi (dưới) và sân đối thủ (trên) đối mặt, HUD hiển thị Cup thay Coin.*

---

### 3.2.4 Kết Quả Và Điều Kiện Kết Thúc

```
Thắng trận:  player còn unit, enemy bị xóa  →  Cup +1
Thua trận:   player bị xóa, enemy còn unit  →  HP  −1
Hòa:         cả hai còn unit sau 50 round   →  không đổi gì
```

| Điều kiện   | Khởi đầu | Ngưỡng kết thúc ván | Thay đổi/lượt |
|-------------|---------|---------------------|---------------|
| playerCups  | 0       | ≥ 10 → thắng        | +1 khi thắng  |
| playerHP    | 7       | ≤ 0 → thua          | −1 khi thua   |
| currentTurn | 1       | > 20 → thắng        | +1 mỗi lượt   |

Hòa không trừ HP — cả hai bên đều chơi tốt khi duy trì đủ lâu qua 50 round. Sau combat, sân được khôi phục về snapshot trước khi nhấn Fight — các unit triệu hồi trong battle biến mất, đội hình trở về đúng trạng thái ban đầu.

---

*[Tiếp theo: Mục 3.3 — Hệ Thống Bài (Card System)]*
