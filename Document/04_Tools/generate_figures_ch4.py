"""
Tao Hinh 4.1, 4.2, 4.3 (CHUONG 4 - KIEN TRUC HE THONG) cho tieu luan AutoChess.

Ca 3 hinh trong file nay deu la so do khai niem (ve truc tiep theo dac ta
trong Document/03_Figures/Hinh.md, khong phu thuoc so lieu thuc nghiem):
  - Hinh 4.1: hinh dang kien truc 4 tang + nhanh AI tach rieng
  - Hinh 4.2: quy trinh rut ngan xep cai chet (Death Stack)
  - Hinh 4.3: pipeline huan luyen khep kin (6 buoc)

Chay:   python generate_figures_ch4.py
Ket qua: Document/03_Figures/hinh_4_1.png, hinh_4_2.png, hinh_4_3.png
"""

import os
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
# HINH 4.1 - Hinh dang kien truc khi ap dung ba rang buoc (4 tang + nhanh AI)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_4_1():
    fig, ax = plt.subplots(figsize=(11.4, 8.8))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cx = 4.0
    w_layer = 6.6
    h_layer = 1.35

    layers = [
        dict(y=8.7, text="TẦNG GIAO DIỆN (UI)\n"
                         "CardView · BoardView · ShopUI · phát lại TurnRecord bằng hoạt ảnh",
             fc="#FCEFD0", ec=C_ORANGE),
        dict(y=6.85, text="TẦNG QUẢN LÝ (Manager)\n"
                          "GameManager · AIManager — vòng đời Unity, điều phối trạng thái ván đấu",
             fc="#FCEFD0", ec=C_ORANGE),
        dict(y=5.0, text="CORE ENGINE\n"
                         "CombatResolver · SkillEngine · GameSimulator\n"
                         "luật chơi thuần — tạo bằng new() được ở bất cứ đâu",
             fc="#E5F2E8", ec=C_GREEN),
        dict(y=3.05, text="TẦNG DỮ LIỆU (Data)\n"
                         "CardsData.json · Chromosome · tham số cân bằng — data-driven, không hard-code",
             fc="#ECECEC", ec=C_GRAY),
    ]

    for L in layers:
        draw_box(ax, cx, L["y"], w_layer, h_layer, L["text"],
                 fc=L["fc"], ec=L["ec"], fs=8.4)

    # mui ten phu thuoc mot chieu, tu tren xuong duoi
    for a, b in zip(layers[:-1], layers[1:]):
        draw_arrow(ax, cx, a["y"] - h_layer / 2 - 0.04, cx, b["y"] + h_layer / 2 + 0.04,
                   color=C_GRAY, lw=1.5)

    ax.text(cx - w_layer / 2 - 0.3, (layers[0]["y"] + layers[-1]["y"]) / 2,
            "phụ thuộc một chiều\n(trên → dưới;\ntầng dưới không\nbiết đến tầng trên)",
            fontsize=7.4, color="#555555", ha="right", va="center", style="italic")

    # nhanh AI tach rieng — noi thang xuong Core Engine, bo qua Manager & UI
    ai_cx, ai_y = cx + w_layer / 2 + 2.7, 7.75
    draw_box(ax, ai_cx, ai_y, 3.1, 1.7,
             "TẦNG AI (đứng tách biệt)\nGATrainer · BotAgent ·\nGameSimulator (headless)",
             fc="#E5F2E8", ec=C_GREEN, fs=8.4, bold=True)

    line_x = ai_cx
    core_y = layers[2]["y"]
    core_right = cx + w_layer / 2
    ax.plot([line_x, line_x], [ai_y - 0.85, core_y], color=C_GREEN, lw=1.6,
            solid_capstyle="round", zorder=2)
    draw_arrow(ax, line_x, core_y, core_right + 0.06, core_y, color=C_GREEN, lw=1.6)
    ax.text(line_x + 0.18, (ai_y - 0.85 + core_y) / 2,
            "kết nối thẳng — bỏ qua\nManager & UI hoàn toàn\n(hiện thân của ràng buộc headless)",
            fontsize=7.4, color=C_GREEN, ha="left", va="center", style="italic")

    # chu thich mau (legend)
    legend = [
        (C_GREEN, "#E5F2E8", "Mã C# thuần (Plain C#) — logic, tính toán, ra quyết định"),
        (C_ORANGE, "#FCEFD0", "MonoBehaviour — gắn với vòng đời Unity (Awake / Update / Input)"),
        (C_GRAY, "#ECECEC", "Dữ liệu thuần (Data) — tham số, cấu hình; không chứa logic"),
    ]
    lx, ly0, dy = 0.55, 1.15, 0.46
    for i, (ec, fcc, lbl) in enumerate(legend):
        ly = ly0 - i * dy
        rect = FancyBboxPatch((lx, ly - 0.1), 0.5, 0.32, boxstyle="round,pad=0.02",
                              fc=fcc, ec=ec, lw=1.3, zorder=3)
        ax.add_patch(rect)
        ax.text(lx + 0.66, ly + 0.06, lbl, fontsize=7.6, color="#444444",
                ha="left", va="center")

    ax.set_xlim(-2.7, ai_cx + 2.0)
    ax.set_ylim(0.05, 9.95)
    ax.axis("off")
    ax.set_title("Hình 4.1 — Hình Dạng Kiến Trúc Khi Áp Dụng Ba Ràng Buộc",
                 fontsize=12.5, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_4_1.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 4.2 - Quy trinh rut ngan xep cai chet (Death Stack)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_4_2():
    fig, ax = plt.subplots(figsize=(9.6, 13.8))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cx = 4.6
    bw = 5.7

    y_start = 15.0
    y1      = 13.3
    y2      = 11.7
    y3      = 10.1
    y4      = 8.15
    y_dec1  = 6.35
    y5      = 4.5
    y6      = 2.8
    y_dec2  = 1.05
    y_end   = -0.7

    draw_box(ax, cx, y_start, bw, 1.05,
             "Lượt giao tranh vừa kết thúc\nDeath Stack có ≥ 1 phần tử",
             fc="#ECECEC", ec=C_GRAY, fs=8.6, bold=True)
    draw_arrow(ax, cx, y_start - 0.55, cx, y1 + 0.55, color=C_GRAY)

    draw_box(ax, cx, y1, bw, 1.05,
             "①  Lấy cái chết gần nhất\n(pop khỏi Death Stack — LIFO)",
             fc="#FBE3E3", ec=C_RED, fs=8.6)
    draw_arrow(ax, cx, y1 - 0.55, cx, y2 + 0.55, color=C_RED)

    draw_box(ax, cx, y2, bw, 1.05,
             "②  Kích hoạt kỹ năng 'khi chết'\ncủa chính unit vừa lấy ra",
             fc="#FBE3E3", ec=C_RED, fs=8.6)
    draw_arrow(ax, cx, y2 - 0.55, cx, y3 + 0.55, color=C_RED)

    draw_box(ax, cx, y3, bw, 1.05,
             "③  Quét lại sân\n→ phát hiện cái chết mới → đẩy vào stack",
             fc="#FBE3E3", ec=C_RED, fs=8.6)
    draw_arrow(ax, cx, y3 - 0.55, cx, y4 + 0.65, color=C_RED)

    draw_box(ax, cx, y4, bw, 1.3,
             "④  Phát sự kiện 'khi đồng minh chết'\ncho từng đồng minh còn sống\n"
             "(đúng thứ tự vị trí — mục 4.3.1)",
             fc="#FBE3E3", ec=C_RED, fs=8.4)
    draw_arrow(ax, cx, y4 - 0.65, cx, y_dec1 + 0.55, color=C_GRAY)

    dec1_w = bw - 0.9
    draw_box(ax, cx, y_dec1, dec1_w, 1.05, "Ngăn xếp còn phần tử?",
             fc="#FCEFD0", ec=C_ORANGE, fs=8.8, bold=True)

    # vong lap trong: con phan tu -> quay lai buoc 1 (canh trai, ngan)
    loop_x1 = cx - bw / 2 - 1.15
    ax.plot([cx - dec1_w / 2, loop_x1], [y_dec1, y_dec1], color=C_RED, lw=1.3,
            solid_capstyle="round", zorder=2)
    ax.plot([loop_x1, loop_x1], [y_dec1, y1], color=C_RED, lw=1.3,
            solid_capstyle="round", zorder=2)
    draw_arrow(ax, loop_x1, y1, cx - bw / 2, y1, color=C_RED, lw=1.3)
    ax.text(loop_x1 - 0.2, (y_dec1 + y1) / 2, "CÒN\n→ lặp lại\nbước ①",
            fontsize=7.2, color=C_RED, ha="right", va="center")

    # nhanh "trong" — tiep tuc xuong duoi
    draw_arrow(ax, cx, y_dec1 - 0.55, cx, y5 + 0.85, color=C_GRAY)
    ax.text(cx + 0.28, (y_dec1 - 0.55 + y5 + 0.85) / 2, "TRỐNG",
            fontsize=7.4, color="#555555", ha="left", style="italic")

    # buoc 5: don san — hai nhanh mau (Reborn xanh lam / loai bo do)
    ax.text(cx, y5 + 0.78,
            "⑤  Dọn sân — áp dụng cho từng unit đã được đánh dấu chết",
            fontsize=7.8, color="#555555", ha="center", style="italic")
    bx5      = [cx - 1.7, cx + 1.7]
    blabels5 = ["Còn lượt Reborn\n→ hồi sinh trở lại sân", "Hết lượt hồi sinh\n→ gỡ khỏi sân vĩnh viễn"]
    bcolors5 = [C_BLUE, C_RED]
    bfc5     = ["#E3F2FD", "#FBE3E3"]
    for x, lbl, c, fcc in zip(bx5, blabels5, bcolors5, bfc5):
        draw_box(ax, x, y5, 2.75, 1.05, lbl, fc=fcc, ec=c, fs=7.8)
        draw_arrow(ax, x, y5 - 0.55, cx, y6 + 0.55, color=c, lw=1.2,
                   connectionstyle=f"arc3,rad={(cx - x) * -0.07}")

    draw_box(ax, cx, y6, bw, 1.05,
             "⑥  Giải phóng ĐÚNG MỘT lượt\ntriệu hồi đang chờ (nếu có)",
             fc="#E3F2FD", ec=C_BLUE, fs=8.6)
    draw_arrow(ax, cx, y6 - 0.55, cx, y_dec2 + 0.55, color=C_GRAY)

    dec2_w = bw - 0.5
    draw_box(ax, cx, y_dec2, dec2_w, 1.05,
             "Có biến động mới phát sinh?\n(cái chết / lượt triệu hồi mới)",
             fc="#FCEFD0", ec=C_ORANGE, fs=8.4, bold=True)

    # vong lap ngoai: co bien dong moi -> quay lai dau chu trinh (canh phai, dai)
    loop_x2 = cx + bw / 2 + 1.25
    ax.plot([cx + dec2_w / 2, loop_x2], [y_dec2, y_dec2], color=C_PURPLE, lw=1.3,
            solid_capstyle="round", zorder=2)
    ax.plot([loop_x2, loop_x2], [y_dec2, y_start], color=C_PURPLE, lw=1.3,
            solid_capstyle="round", zorder=2)
    draw_arrow(ax, loop_x2, y_start, cx + bw / 2, y_start, color=C_PURPLE, lw=1.3)
    ax.text(loop_x2 + 0.2, (y_dec2 + y_start) / 2,
            "CÓ\n→ toàn bộ chu trình\nlặp lại từ đầu\ncho đến khi lắng hẳn",
            fontsize=7.2, color=C_PURPLE, ha="left", va="center")

    # nhanh "khong" — ket thuc
    draw_arrow(ax, cx, y_dec2 - 0.55, cx, y_end + 0.5, color=C_GRAY)
    ax.text(cx + 0.28, (y_dec2 - 0.55 + y_end + 0.5) / 2, "KHÔNG",
            fontsize=7.4, color="#555555", ha="left", style="italic")
    draw_box(ax, cx, y_end, bw, 0.95,
             "[Sân đã lắng — trận đấu tiếp tục bình thường]",
             fc="#E5F2E8", ec=C_GREEN, fs=8.4, bold=True)

    ax.text(cx, y_end - 0.95,
            "Bộ đếm an toàn: giới hạn 500 vòng lặp — một lưới bắt lỗi, không phải một phần của thiết kế",
            fontsize=7.0, color="#999999", ha="center", style="italic")

    ax.set_xlim(loop_x1 - 1.9, loop_x2 + 3.0)
    ax.set_ylim(y_end - 1.7, y_start + 1.0)
    ax.axis("off")
    ax.set_title("Hình 4.2 — Quy Trình Rút Ngăn Xếp Cái Chết (Death Stack)",
                 fontsize=12.5, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_4_2.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 4.3 - Pipeline huan luyen khep kin (so do ngang 6 buoc)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_4_3():
    fig, ax = plt.subplots(figsize=(16, 6.2))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    y = 3.0
    steps = [
        dict(cx=1.4,  w=2.4, h=1.7,
             text="①  Kịch bản khởi động\n(command-line script)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=4.3,  w=2.4, h=1.7,
             text="②  Unity chạy ở chế độ\nkhông giao diện (headless)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=7.2,  w=2.4, h=1.7,
             text="③  Điểm vào huấn luyện\n(training entry point)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=10.4, w=3.0, h=1.95,
             text="④  GATrainer điều khiển\nGameSimulator\n× 432.000 trận đấu",
             fc="#FCEFD0", ec=C_ORANGE, bold=True),
        dict(cx=13.7, w=2.6, h=1.7,
             text="⑤  File kết quả\nhuấn luyện\n(AI_Library.json)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=16.9, w=2.8, h=1.95,
             text="⑥  AIManager nạp 5\nBotAgent vào game thật\n(runtime)",
             fc="#E5F2E8", ec=C_GREEN, bold=True),
    ]

    # khung xam bao quanh phan headless (buoc 1-5)
    frame_left  = steps[0]["cx"] - steps[0]["w"] / 2 - 0.45
    frame_right = steps[4]["cx"] + steps[4]["w"] / 2 + 0.45
    frame_h     = 2.55
    frame = FancyBboxPatch((frame_left, y - frame_h / 2), frame_right - frame_left, frame_h,
                           boxstyle="round,pad=0.05", fc="#EFEFEF", ec=C_GRAY,
                           lw=1.4, ls="--", zorder=1)
    ax.add_patch(frame)
    ax.text((frame_left + frame_right) / 2, y + frame_h / 2 + 0.35,
            "PHẦN HEADLESS — Unity chạy không cửa sổ, không bộ dựng cảnh, không một khung hình nào được vẽ",
            fontsize=8.2, color="#666666", ha="center", style="italic", fontweight="bold")
    ax.text(steps[5]["cx"], y + frame_h / 2 + 0.35, "PHẦN GAME RUNTIME",
            fontsize=8.2, color=C_GREEN, ha="center", style="italic", fontweight="bold")

    for i, s in enumerate(steps):
        draw_box(ax, s["cx"], y, s["w"], s["h"], s["text"],
                 fc=s["fc"], ec=s["ec"], fs=8.0, bold=s["bold"])
        if i < len(steps) - 1:
            nxt = steps[i + 1]
            draw_arrow(ax, s["cx"] + s["w"] / 2 + 0.06, y, nxt["cx"] - nxt["w"] / 2 - 0.06, y,
                       color=C_GRAY, lw=1.5)

    ax.text((frame_left + frame_right) / 2, y - frame_h / 2 - 0.55,
            "432.000 trận: tiến hóa 5 quần thể độc lập theo 5 triết lý chơi khác nhau,\n"
            "ghi lại các cá thể sống sót tốt nhất vào file kết quả",
            fontsize=7.6, color="#777777", ha="center", style="italic")

    ax.set_xlim(frame_left - 0.7, steps[5]["cx"] + steps[5]["w"] / 2 + 0.7)
    ax.set_ylim(y - frame_h / 2 - 1.5, y + frame_h / 2 + 0.95)
    ax.axis("off")
    ax.set_title("Hình 4.3 — Pipeline Huấn Luyện Khép Kín: Từ Mã Nguồn Đến Năm Chiến Binh",
                 fontsize=12.5, pad=10, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_4_3.png")


# ══════════════════════════════════════════════════════════════════════════════
if __name__ == "__main__":
    hinh_4_1()
    hinh_4_2()
    hinh_4_3()
