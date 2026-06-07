"""
Tao Hinh 3.1, 3.3, 3.7, 3.8, 3.9, 3.10, 3.11, 3.12 (CHUONG 3 - THIET KE GAME)
cho tieu luan AutoChess.

8 hinh trong file nay gom 7 so do khai niem (ve truc tiep theo dac ta trong
Document/03_Figures/Hinh.md, khong phu thuoc du lieu) va 1 bieu do du lieu
thuc (Hinh 3.12 - phan phoi 68 la bai theo tier, trich xuat tu
Assets/Resources/CardsData.json).

Cac hinh con lai cua chuong 3 (3.2, 3.4, 3.5, 3.6, 3.13, 3.14) la anh chup
man hinh thuc te trong game - ngoai pham vi script nay.

Chay:   python generate_figures_ch3.py
Ket qua: Document/03_Figures/hinh_3_1.png, hinh_3_3.png, hinh_3_7.png,
         hinh_3_8.png, hinh_3_9.png, hinh_3_10.png, hinh_3_11.png, hinh_3_12.png
"""

import os
import numpy as np
import matplotlib
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch

matplotlib.rcParams["font.family"] = "DejaVu Sans"
matplotlib.rcParams["axes.unicode_minus"] = False

# ─── Duong dan ────────────────────────────────────────────────────────────────
BASE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(BASE, "..", ".."))
OUT  = os.path.join(ROOT, "Document", "03_Figures")
os.makedirs(OUT, exist_ok=True)

# ─── Bang mau dung chung ──────────────────────────────────────────────────────
BG       = "#FAFAFA"
C_GRID   = "#DDDDDD"
C_BLUE   = "#1565C0"
C_RED    = "#E04040"
C_ORANGE = "#F0A030"
C_GREEN  = "#2E7D32"
C_PURPLE = "#8040C0"
C_GRAY   = "#888888"
C_PINK   = "#D81B60"


def save(fig, name):
    path = os.path.join(OUT, name)
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


def draw_box(ax, cx, cy, w, h, text, fc="#E3F2FD", ec=C_BLUE, fs=9,
             bold=False, radius=0.04):
    rect = FancyBboxPatch((cx - w / 2, cy - h / 2), w, h,
                          boxstyle=f"round,pad={radius}",
                          fc=fc, ec=ec, lw=1.5, zorder=3)
    ax.add_patch(rect)
    ax.text(cx, cy, text, ha="center", va="center", fontsize=fs,
            fontweight="bold" if bold else "normal", zorder=4)
    return rect


def draw_arrow(ax, x0, y0, x1, y1, color=C_GRAY, lw=1.4, style="-|>",
               connectionstyle="arc3,rad=0.0"):
    arrow = FancyArrowPatch((x0, y0), (x1, y1),
                            arrowstyle=style, color=color, lw=lw,
                            connectionstyle=connectionstyle,
                            mutation_scale=13, zorder=2)
    ax.add_patch(arrow)
    return arrow


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.1 - Ba bo toc va triet ly choi (infographic 3 cot)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_1():
    fig, ax = plt.subplots(figsize=(12, 6.6))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    tribes = [
        dict(cx=2.0, name="BABYLON", color=C_BLUE, fc="#E3F2FD",
             keyword="ACCUMULATION\n— snowball dài hạn",
             mechanic="Buff lẫn nhau qua sự kiện\ndeploy / sell, hấp thụ chỉ số\ntích lũy vĩnh viễn",
             style="Yếu đầu game,\nbùng nổ cuối game —\ncàng kéo dài càng mạnh",
             example="VD: Utu, Ashur,\nLamashtu–Uridimmu"),
        dict(cx=5.5, name="OLYMPUS", color=C_PURPLE, fc="#F1E9FB",
             keyword="AGGRESSION\n— áp đảo sớm",
             mechanic="ATK synergy qua\ncác sự kiện combat\n(combat events)",
             style="Gây sức ép ngay\ntừ đầu trận — dồn ép\nđối thủ trước khi ổn định",
             example="(thiết kế dự kiến —\nchưa có unit thực)"),
        dict(cx=9.0, name="NILES", color=C_GREEN, fc="#E5F2E8",
             keyword="REACTION\n— mạnh bất ngờ qua chuỗi trigger",
             mechanic="Chu trình chết – tái sinh,\nchuỗi phản ứng OnAllyDeath\nlan tỏa toàn đội hình",
             style="Đội hình dễ vỡ nhưng\nmỗi cái chết có thể kích hoạt\ncả chuỗi buff bất ngờ",
             example="VD: Anubis, Osiris,\nSobek, Thoth"),
    ]

    col_w  = 3.05
    top_y  = 5.65
    rows_y = [4.55, 3.35, 2.05, 0.85]
    row_h  = [0.95, 1.35, 1.35, 0.95]
    row_fs = [8.6, 7.6, 7.6, 7.4]
    row_lbl = ["Triết lý cốt lõi", "Cơ chế đặc trưng", "Phong cách chơi", "Unit tiêu biểu"]

    for tb in tribes:
        cx, color, fc = tb["cx"], tb["color"], tb["fc"]
        # khung cot bao quanh ca cot
        outer = FancyBboxPatch((cx - col_w / 2 - 0.12, 0.25),
                               col_w + 0.24, 5.95,
                               boxstyle="round,pad=0.04",
                               fc="white", ec=color, lw=1.2, alpha=0.55, zorder=1)
        ax.add_patch(outer)
        # header — ten bo toc
        draw_box(ax, cx, top_y, col_w, 0.95, tb["name"], fc=color, ec=color,
                 fs=13.5, bold=True, radius=0.06)
        ax.text(cx, top_y, tb["name"], ha="center", va="center", fontsize=13.5,
                fontweight="bold", color="white", zorder=5)
        # 4 hang noi dung
        contents = [tb["keyword"], tb["mechanic"], tb["style"], tb["example"]]
        for ry, rh, fs, lbl, content in zip(rows_y, row_h, row_fs, row_lbl, contents):
            draw_box(ax, cx, ry, col_w - 0.25, rh, "", fc=fc, ec=color, fs=fs, radius=0.05)
            ax.text(cx, ry + rh / 2 - 0.20, lbl, ha="center", va="top",
                    fontsize=7.0, color=color, fontweight="bold", style="italic", zorder=5)
            ax.text(cx, ry - 0.10, content, ha="center", va="center",
                    fontsize=fs, color="#333333", zorder=5)

    ax.set_xlim(0, 11)
    ax.set_ylim(0, 6.7)
    ax.axis("off")
    ax.set_title("Hình 3.1 — Ba Bộ Tộc Và Triết Lý Chơi", fontsize=13, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_3_1.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.3 - So do vong lap mot van dau
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_3():
    fig, ax = plt.subplots(figsize=(8.6, 10.5))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cx = 5.0
    # cac khoi chinh tren truc doc
    y_start  = 13.4
    y_shop   = 11.6
    y_combat = 9.5
    y_branch = 7.6
    y_decide = 5.5
    y_win    = 3.55
    y_lose   = 2.15

    draw_box(ax, cx, y_start, 4.4, 1.0,
             "[BẮT ĐẦU VÁN ĐẤU]\nHP = 7   ·   Cup = 0   ·   Lượt = 1",
             fc="#ECECEC", ec=C_GRAY, fs=9.2, bold=True)

    draw_arrow(ax, cx, y_start - 0.5, cx, y_shop + 0.62, color=C_GRAY)

    draw_box(ax, cx, y_shop, 5.0, 1.25,
             "SHOP PHASE\nnhận 10 coin · làm mới shop\nmua / bán / xếp đội hình / reroll",
             fc="#E5F2E8", ec=C_GREEN, fs=8.8, bold=True)

    draw_arrow(ax, cx, y_shop - 0.625, cx, y_combat + 0.65, color=C_GRAY)
    ax.text(cx + 0.18, (y_shop - 0.625 + y_combat + 0.65) / 2, "[Nhấn Fight]",
            fontsize=8.0, color="#555555", ha="left", style="italic")

    draw_box(ax, cx, y_combat, 5.0, 1.3,
             "COMBAT PHASE\nchiến đấu tự động (auto-battler)\ntối đa 50 round / trận",
             fc="#FBE3E3", ec=C_RED, fs=8.8, bold=True)

    # 3 nhanh ket qua
    bx = [2.55, 5.0, 7.45]
    blabels = ["THẮNG\nCup +1", "THUA\nHP −1", "HÒA\n(không đổi)"]
    bcolors = [C_GREEN, C_RED, C_GRAY]
    bfc     = ["#E5F2E8", "#FBE3E3", "#ECECEC"]
    for x, lbl, c, fc in zip(bx, blabels, bcolors, bfc):
        draw_arrow(ax, cx, y_combat - 0.65, x, y_branch + 0.45, color=c, lw=1.2,
                   connectionstyle=f"arc3,rad={(x - cx) * 0.05}")
        draw_box(ax, x, y_branch, 1.95, 0.9, lbl, fc=fc, ec=c, fs=8.2, bold=True)
        draw_arrow(ax, x, y_branch - 0.45, cx, y_decide + 0.55, color=c, lw=1.2,
                   connectionstyle=f"arc3,rad={(cx - x) * -0.05}")

    draw_box(ax, cx, y_decide, 6.6, 1.85,
             "Cup ≥ 10 ?  →  Thắng ván\n"
             "HP ≤ 0 ?  →  Thua ván\n"
             "Lượt > 20 ?  →  Thắng ván (vượt thời hạn)\n"
             "Còn lại  →  Lượt += 1, quay lại Shop",
             fc="#FCEFD0", ec=C_ORANGE, fs=8.0)

    # vong lap quay lai Shop Phase (canh trai) - duong gap khuc kieu so do khoi
    loop_x = 0.75
    ax.plot([cx - 3.3, loop_x], [y_decide, y_decide], color=C_GRAY, lw=1.3,
            solid_capstyle="round", zorder=2)
    ax.plot([loop_x, loop_x], [y_decide, y_shop], color=C_GRAY, lw=1.3,
            solid_capstyle="round", zorder=2)
    draw_arrow(ax, loop_x, y_shop, cx - 2.5, y_shop, color=C_GRAY, lw=1.3)
    ax.text((cx - 3.3 + loop_x) / 2, y_decide - 1.25, "còn lại\n→ Lượt + 1, quay lại Shop",
            fontsize=7.6, color="#555555", ha="center")

    # nhanh ket thuc van
    draw_arrow(ax, cx + 1.6, y_decide - 0.45, cx + 0.55, y_win + 0.42, color=C_GREEN, lw=1.2,
               connectionstyle="arc3,rad=-0.25")
    draw_box(ax, cx + 1.7, y_win, 2.85, 0.95, "[KẾT THÚC]\nTHẮNG VÁN\n(Cup ≥ 10  /  Lượt > 20)",
             fc="#E5F2E8", ec=C_GREEN, fs=7.6, bold=True)

    draw_arrow(ax, cx - 1.6, y_decide - 0.45, cx - 0.55, y_lose + 0.42, color=C_RED, lw=1.2,
               connectionstyle="arc3,rad=0.25")
    draw_box(ax, cx - 1.7, y_lose, 2.85, 0.95, "[KẾT THÚC]\nTHUA VÁN\n(HP ≤ 0)",
             fc="#FBE3E3", ec=C_RED, fs=7.8, bold=True)

    ax.set_xlim(0, 10)
    ax.set_ylim(1.2, 14.2)
    ax.axis("off")
    ax.set_title("Hình 3.3 — Sơ Đồ Vòng Lặp Một Ván Đấu (Match Loop)",
                 fontsize=12.5, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_3_3.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.7 - So sanh ba triet ly tribe synergy (bang 3 cot)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_7():
    fig, ax = plt.subplots(figsize=(11.5, 6.6))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cols = [
        dict(cx=2.0, name="BABYLON", color=C_BLUE, fc="#E3F2FD"),
        dict(cx=5.5, name="NILES",   color=C_GREEN, fc="#E5F2E8"),
        dict(cx=9.0, name="OLYMPUS", color=C_PURPLE, fc="#F1E9FB"),
    ]
    row_labels = ["Trigger chính", "Giai đoạn mạnh nhất", "Rủi ro chính", "Unit tiêu biểu"]
    data = {
        "BABYLON": [
            "Sự kiện Deploy / Sell\n(vòng kinh tế)",
            "Late-game —\nsnowball tích lũy dài hạn",
            "Yếu đầu game,\nkhó chặn khi đã bùng nổ",
            "Utu · Ashur ·\nLamashtu–Uridimmu",
        ],
        "NILES": [
            "Summon / Reborn / Death\n(chuỗi phản ứng)",
            "Bất kỳ lúc nào —\nbùng nổ qua chuỗi trigger",
            "Đội hình dễ vỡ,\nphụ thuộc vào chuỗi kích hoạt",
            "Anubis · Osiris ·\nSobek · Thoth",
        ],
        "OLYMPUS": [
            "Combat events\n(ATK synergy)",
            "Early-game —\náp đảo ngay từ đầu",
            "Hiện chưa có unit —\nthiết kế dự kiến",
            "(chưa có — gene[19]\ngiữ chỗ mở rộng)",
        ],
    }

    top_y  = 5.55
    row_y  = [4.35, 3.15, 1.95, 0.80]
    row_h  = 1.05
    col_w  = 3.05

    for col in cols:
        cx, color, fc = col["cx"], col["color"], col["fc"]
        outer = FancyBboxPatch((cx - col_w / 2 - 0.10, 0.20), col_w + 0.20, 5.75,
                               boxstyle="round,pad=0.04", fc="white", ec=color,
                               lw=1.1, alpha=0.5, zorder=1)
        ax.add_patch(outer)
        draw_box(ax, cx, top_y, col_w, 0.85, col["name"], fc=color, ec=color, radius=0.06)
        ax.text(cx, top_y, col["name"], ha="center", va="center", fontsize=12.5,
                fontweight="bold", color="white", zorder=5)
        for ry, lbl, content in zip(row_y, row_labels, data[col["name"]]):
            draw_box(ax, cx, ry, col_w - 0.2, row_h, "", fc=fc, ec=color, radius=0.05)
            ax.text(cx, ry + row_h / 2 - 0.20, lbl, ha="center", va="top", fontsize=7.0,
                    color=color, fontweight="bold", style="italic", zorder=5)
            ax.text(cx, ry - 0.12, content, ha="center", va="center", fontsize=7.8,
                    color="#333333", zorder=5)

    ax.set_xlim(0, 11)
    ax.set_ylim(0, 6.5)
    ax.axis("off")
    ax.set_title("Hình 3.7 — So Sánh Ba Triết Lý Tribe Synergy",
                 fontsize=13, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_3_7.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.8 - So do san chien dau 7 slot (Frontline / Backline)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_8():
    fig, ax = plt.subplots(figsize=(10, 4.6))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    slot_w, slot_h = 1.15, 1.15
    xs = [0.9 + i * 1.35 for i in range(7)]
    y0 = 2.0

    for i, x in enumerate(xs):
        is_front = i <= 3
        c   = C_RED if is_front else C_BLUE
        fc  = "#FBE3E3" if is_front else "#E3F2FD"
        draw_box(ax, x, y0, slot_w, slot_h, f"Slot {i}", fc=fc, ec=c, fs=9.2, bold=True)

    # vung bao Frontline / Backline
    front_box = FancyBboxPatch((xs[0] - slot_w / 2 - 0.18, y0 - slot_h / 2 - 0.45),
                               (xs[3] - xs[0]) + slot_w + 0.36, slot_h + 0.9,
                               boxstyle="round,pad=0.05", fc="none", ec=C_RED,
                               lw=1.6, ls="--", zorder=1)
    ax.add_patch(front_box)
    back_box = FancyBboxPatch((xs[4] - slot_w / 2 - 0.18, y0 - slot_h / 2 - 0.45),
                              (xs[6] - xs[4]) + slot_w + 0.36, slot_h + 0.9,
                              boxstyle="round,pad=0.05", fc="none", ec=C_BLUE,
                              lw=1.6, ls="--", zorder=1)
    ax.add_patch(back_box)

    ax.text((xs[0] + xs[3]) / 2, y0 + slot_h / 2 + 0.62, "FRONTLINE  (slot 0 – 3)",
            ha="center", fontsize=10, color=C_RED, fontweight="bold")
    ax.text((xs[4] + xs[6]) / 2, y0 + slot_h / 2 + 0.62, "BACKLINE  (slot 4 – 6)",
            ha="center", fontsize=10, color=C_BLUE, fontweight="bold")

    # mui ten huong tan cong cua doi phuong (tu phai sang, uu tien Frontline)
    arr_y = y0 - slot_h / 2 - 1.05
    draw_arrow(ax, xs[6] + 0.7, arr_y, xs[3] - 0.1, arr_y, color=C_RED, lw=2.0)
    ax.text((xs[3] + xs[6]) / 2 + 0.3, arr_y - 0.30,
            "Đối phương luôn nhắm Frontline trước", fontsize=8.4, color=C_RED, ha="center")

    sub_y = arr_y - 0.85
    draw_arrow(ax, xs[3] + 0.55, sub_y, xs[3] - 0.1, sub_y, color=C_GRAY, lw=1.4, style="-|>")
    ax.text((xs[4] + xs[6]) / 2 + 0.3, sub_y - 0.40,
            "chỉ chuyển sang Backline\nkhi Frontline đã trống hoàn toàn",
            fontsize=7.8, color=C_GRAY, ha="center", va="center")

    ax.set_xlim(0, 10.4)
    ax.set_ylim(-1.35, 4.2)
    ax.axis("off")
    ax.set_title("Hình 3.8 — Sân Chiến Đấu 7 Slot (Frontline / Backline)",
                 fontsize=12.5, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_3_8.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.9 - Thu tu tan cong trong mot round (hai san doi mat)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_9():
    fig, ax = plt.subplots(figsize=(10.4, 5.6))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    slot_w, slot_h = 1.05, 1.05
    xs = [0.85 + i * 1.28 for i in range(7)]
    y_enemy  = 3.55
    y_player = 1.55

    # so thu tu hang doi: Slot 0 (enemy) -> Slot 0 (player) -> Slot 1 (enemy) -> ...
    order = []
    for i in range(7):
        order.append(("E", i))
        order.append(("P", i))
    queue_no = {key: idx + 1 for idx, key in enumerate(order)}

    for i, x in enumerate(xs):
        draw_box(ax, x, y_enemy, slot_w, slot_h, "", fc="#FBE3E3", ec=C_RED, fs=8)
        ax.text(x, y_enemy + 0.30, f"#{queue_no[('E', i)]}", ha="center", fontsize=8.6,
                color=C_RED, fontweight="bold")
        ax.text(x, y_enemy - 0.18, f"slot {i}", ha="center", fontsize=7.2, color="#883333")

        draw_box(ax, x, y_player, slot_w, slot_h, "", fc="#E3F2FD", ec=C_BLUE, fs=8)
        ax.text(x, y_player + 0.30, f"#{queue_no[('P', i)]}", ha="center", fontsize=8.6,
                color=C_BLUE, fontweight="bold")
        ax.text(x, y_player - 0.18, f"slot {i}", ha="center", fontsize=7.2, color="#1A4F8B")

    ax.text(xs[0] - 0.95, y_enemy, "ĐỊCH", ha="center", va="center", fontsize=10,
            color=C_RED, fontweight="bold", rotation=90)
    ax.text(xs[0] - 0.95, y_player, "BẠN", ha="center", va="center", fontsize=10,
            color=C_BLUE, fontweight="bold", rotation=90)

    ax.text((xs[0] + xs[6]) / 2, y_enemy + 1.00,
            "Hàng đợi tấn công round: Slot 0 (địch) → Slot 0 (bạn) → Slot 1 (địch) → "
            "Slot 1 (bạn) → … → Slot 6 (bạn)",
            ha="center", fontsize=8.6, color="#444444")
    ax.text((xs[0] + xs[6]) / 2, y_enemy + 0.66,
            "→ tổng cộng tối đa 14 lượt hành động mỗi round khi cả hai sân đều đầy",
            ha="center", fontsize=8.0, color="#777777", style="italic")

    # chu giai cho slot trong
    leg_x, leg_y = xs[6] + 1.05, (y_enemy + y_player) / 2
    legend_box = FancyBboxPatch((leg_x - 0.5, leg_y - 0.5), 1.0, 1.0,
                                boxstyle="round,pad=0.03", fc="#EEEEEE", ec=C_GRAY,
                                lw=1.2, hatch="////", zorder=3)
    ax.add_patch(legend_box)
    ax.text(leg_x, leg_y, "—", ha="center", va="center", fontsize=10, color=C_GRAY, zorder=4)
    ax.text(leg_x, leg_y - 0.78,
            "Slot trống hoặc\nATK = 0\n→ bị bỏ qua khỏi\nhàng đợi, không\nnhận số thứ tự",
            ha="center", va="top", fontsize=7.2, color="#666666")

    ax.set_xlim(-0.3, 11.2)
    ax.set_ylim(0.5, 5.0)
    ax.axis("off")
    ax.set_title("Hình 3.9 — Thứ Tự Tấn Công Trong Một Round",
                 fontsize=12.5, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_3_9.png")


# ══════════════════════════════════════════════════════════════════════════════
# Helper dung chung cho 3.10 / 3.11 - dong thoi gian (timeline) ngang
# ══════════════════════════════════════════════════════════════════════════════
def _draw_chain_timeline(ax, steps, color, fc, title):
    n = len(steps)
    xs = np.linspace(0.9, 0.9 + (n - 1) * 1.55, n)
    y = 1.0
    box_w, box_h = 1.40, 1.55

    for i, (x, text) in enumerate(zip(xs, steps)):
        draw_box(ax, x, y, box_w, box_h, text, fc=fc, ec=color, fs=7.3)
        ax.text(x, y + box_h / 2 + 0.27, f"Bước {i + 1}", ha="center", fontsize=8.0,
                color=color, fontweight="bold")
        if i < n - 1:
            draw_arrow(ax, x + box_w / 2 + 0.04, y, xs[i + 1] - box_w / 2 - 0.04, y,
                       color=color, lw=1.6)

    ax.text((xs[0] + xs[-1]) / 2, y - box_h / 2 - 0.65,
            "chuỗi khép lại — trận đấu tiếp tục như bình thường",
            ha="center", fontsize=7.6, color="#777777", style="italic")

    ax.set_xlim(xs[0] - box_w / 2 - 0.5, xs[-1] + box_w / 2 + 0.5)
    ax.set_ylim(y - box_h / 2 - 1.15, y + box_h / 2 + 0.95)
    ax.axis("off")
    ax.set_title(title, fontsize=12, pad=8, fontweight="bold")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.10 - Mot chuoi phan ung day chuyen dien hinh (vi du tong quat)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_10():
    fig, ax = plt.subplots(figsize=(12.5, 3.6))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    steps = [
        "Unit A\ngục ngã",
        "Kỹ năng\n\"khi đồng minh\nchết\" kích hoạt",
        "Unit B được\nban hiệu ứng\nReborn",
        "B gục ngã\n→ hồi sinh\nngay lập tức",
        "Kỹ năng nhân bội\nkích hoạt\ntrên B",
        "Chuỗi khép lại,\ntrận đấu\ntiếp tục",
    ]
    _draw_chain_timeline(ax, steps, C_PINK, "#FBE3F0",
                         "Hình 3.10 — Một Chuỗi Phản Ứng Dây Chuyền Điển Hình")
    fig.tight_layout()
    save(fig, "hinh_3_10.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.11 - Chuoi phan ung khi mot unit Reborn guc nga (vi du cu the)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_11():
    fig, ax = plt.subplots(figsize=(13.5, 3.6))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    steps = [
        "HP về 0\n(unit mang\nReborn)",
        "Kỹ năng\n\"khi chết\"\nkích hoạt\n(Horus)",
        "Hồi sinh\nngay với\nđúng 1 HP",
        "Kỹ năng\n\"khi hồi sinh\"\nkích hoạt\n(Osiris)",
        "Kỹ năng \"khi có\nđồng minh mới\"\nlan tỏa (Sobek)",
        "Unit trở lại\nhàng đợi —\nchuỗi khép lại",
    ]
    _draw_chain_timeline(ax, steps, C_GREEN, "#E5F2E8",
                         "Hình 3.11 — Chuỗi Phản Ứng Khi Một Unit Reborn Gục Ngã")
    # chu thich nhan manh: unit duoc tinh la da chet hop le truoc khi hoi sinh
    ax.text(0.5, -0.34,
            "Lưu ý thiết kế: unit vẫn được tính là \"đã chết hợp lệ\" và kích hoạt trọn vẹn kỹ năng \"khi chết\" "
            "TRƯỚC khi hồi sinh — Horus + Reborn nghĩa là phần thưởng kích hoạt hai lần chỉ từ một unit.",
            transform=ax.transAxes, ha="center", fontsize=7.6, color="#555555", style="italic")
    fig.tight_layout()
    save(fig, "hinh_3_11.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 3.12 - Phan phoi 68 la bai theo tier (DU LIEU THUC tu CardsData.json)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_3_12():
    tiers      = [1, 2, 3, 4, 5, 6]
    babylon    = [4, 3, 3, 6, 4, 3]   # tong 23
    niles      = [5, 4, 4, 4, 4, 3]   # tong 24
    spell      = [8, 6, 3, 3, 1, 0]   # tong 21
    unit_total = [b + n for b, n in zip(babylon, niles)]   # 9 7 7 10 8 6 = 47
    grand      = [u + s for u, s in zip(unit_total, spell)]  # 17 13 10 13 9 6 = 68

    fig, ax = plt.subplots(figsize=(10, 6))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    x = np.arange(len(tiers))
    bw = 0.34

    # cot Unit (stacked: Babylon + Niles), cot Spell rieng — canh nhau theo tung tier
    b_bars = ax.bar(x - bw / 2 - 0.02, babylon, width=bw, color=C_BLUE,
                    edgecolor="white", lw=0.8, label="Unit — Babylon", zorder=3)
    n_bars = ax.bar(x - bw / 2 - 0.02, niles, width=bw, bottom=babylon, color=C_GREEN,
                    edgecolor="white", lw=0.8, label="Unit — Niles", zorder=3)
    s_bars = ax.bar(x + bw / 2 + 0.02, spell, width=bw, color=C_ORANGE,
                    edgecolor="white", lw=0.8, label="Spell", zorder=3)

    # nhan tong tren dinh moi cot Unit / Spell
    for xi, ut in zip(x, unit_total):
        ax.text(xi - bw / 2 - 0.02, ut + 0.35, str(ut), ha="center", fontsize=8.4,
                color=C_BLUE, fontweight="bold")
    for xi, sp in zip(x, spell):
        if sp > 0:
            ax.text(xi + bw / 2 + 0.02, sp + 0.35, str(sp), ha="center", fontsize=8.4,
                    color=C_ORANGE, fontweight="bold")
        else:
            ax.text(xi + bw / 2 + 0.02, 0.35, "0", ha="center", fontsize=8.4,
                    color=C_GRAY)

    # tong cong moi tier o tren cung
    for xi, g, ut, sp in zip(x, grand, unit_total, spell):
        top = max(ut, sp if sp > 0 else 0) + 1.7
        ax.text(xi, top, f"Σ {g}", ha="center", fontsize=9.0, color="#444444", fontweight="bold")

    ax.set_xticks(x)
    ax.set_xticklabels([f"Tier {t}" for t in tiers], fontsize=9.5)
    ax.set_ylabel("Số lượng lá bài", fontsize=10)
    ax.set_ylim(0, 17)
    ax.set_title("Hình 3.12 — Phân Phối 68 Lá Bài Theo Tier\n"
                 "(47 unit: 23 Babylon · 24 Niles  +  21 spell  =  68)",
                 fontsize=12.5, pad=12, fontweight="bold")
    ax.legend(loc="upper right", fontsize=8.6, frameon=False)
    ax.spines[["top", "right"]].set_visible(False)
    ax.grid(axis="y", color=C_GRID, lw=0.7, zorder=0)
    ax.set_axisbelow(True)

    ax.text(0.0, -0.16,
            "Nguồn dữ liệu: Assets/Resources/CardsData.json — 47 unit card (tribe = Babylon / Niles) + 21 spell card = 68 lá",
            transform=ax.transAxes, ha="left", fontsize=7.6, color="#777777", style="italic")

    fig.tight_layout()
    save(fig, "hinh_3_12.png")


# ══════════════════════════════════════════════════════════════════════════════
if __name__ == "__main__":
    hinh_3_1()
    hinh_3_3()
    hinh_3_7()
    hinh_3_8()
    hinh_3_9()
    hinh_3_10()
    hinh_3_11()
    hinh_3_12()
    print("\n[DONE] 8 hinh Chuong 3 da duoc luu vao:", OUT)
