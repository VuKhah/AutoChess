"""
Tao Hinh 2.1 - 2.7 (CHUONG 2 - CO SO LY THUYET) cho tieu luan AutoChess.

Ca 7 hinh trong chuong nay deu la so do/bieu do minh hoa khai niem - khong
phu thuoc du lieu training thuc te - nen duoc ve truc tiep bang matplotlib
theo dac ta (prompt) trong Document/03_Figures/Hinh.md.

Chay:   python generate_figures_ch2.py
Ket qua: Document/03_Figures/hinh_2_1.png ... hinh_2_7.png
"""

import os
import numpy as np
import matplotlib
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch
from mpl_toolkits.mplot3d import Axes3D  # noqa: F401  (kich hoat projection="3d")

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
# HINH 2.1 - Tim kiem don-diem va tim kiem theo quan the
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_1():
    fig, axes = plt.subplots(1, 2, figsize=(11.5, 4.7), facecolor=BG)
    x = np.linspace(0, 10, 400)
    # "be mat fitness" gom 3 cuc tri ro rang: mot cuc tri dia phuong gan diem
    # xuat phat (x~2), mot cuc tri thap hon o giua (x~5), va cuc tri TOAN CUC
    # nam o xa, ben phai (x~8) - noi ca the don-diem khong bao gio toi duoc.
    f = (2.2 * np.exp(-((x - 2.0) ** 2) / (2 * 0.65 ** 2))
         + 1.5 * np.exp(-((x - 5.0) ** 2) / (2 * 0.90 ** 2))
         + 3.0 * np.exp(-((x - 8.0) ** 2) / (2 * 0.60 ** 2))
         + 0.4)
    global_x = x[np.argmax(f)]
    local_x = 2.0

    titles = ["(a) Tìm kiếm đơn-điểm (gradient / hill-climbing)",
              "(b) Tìm kiếm theo quần thể (Genetic Algorithm)"]
    for ax, title in zip(axes, titles):
        ax.set_facecolor(BG)
        ax.plot(x, f, color="#555", lw=2, zorder=2)
        ax.fill_between(x, f, f.min() - 0.3, color="#cfd8dc", alpha=0.4, zorder=1)
        ax.set_xlim(0, 10)
        ax.set_ylim(f.min() - 0.9, f.max() + 1.0)
        ax.set_xticks([])
        ax.set_yticks([])
        ax.set_xlabel("Không gian lời giải  x", fontsize=9)
        ax.set_ylabel("f(x)  —  fitness", fontsize=9)
        ax.set_title(title, fontsize=10.5, pad=10)
        ax.spines[["top", "right"]].set_visible(False)

    # (a) mot diem don di theo gradient, leo len cuc tri dia phuong GAN NHAT
    #     va mac ket o do - khong bao gio "thay" duoc cuc tri toan cuc o xa
    axL = axes[0]
    yi = f[np.argmin(np.abs(x - local_x))]
    path_x = np.array([0.6, 1.1, 1.6, 2.0])
    path_y = [f[np.argmin(np.abs(x - p))] for p in path_x]
    axL.plot(path_x, path_y, "o-", color=C_RED, ms=6, lw=1.8, zorder=5)
    axL.annotate("Mắc kẹt ở\ncực trị địa phương\ngần điểm khởi đầu",
                 xy=(local_x, yi), xytext=(local_x + 1.1, yi + 0.55),
                 arrowprops=dict(arrowstyle="->", color=C_RED, lw=1.1),
                 fontsize=8.3, color=C_RED, ha="left")
    axL.scatter([global_x], [f.max()], marker="*", s=170, color=C_GRAY,
                edgecolor="white", lw=0.8, zorder=4)
    axL.annotate("Cực trị toàn cục\n— ở xa, ngoài tầm \"nhìn thấy\"\ncủa một điểm đơn lẻ",
                 xy=(global_x, f.max()), xytext=(global_x - 2.0, f.max() - 1.3),
                 arrowprops=dict(arrowstyle="->", color=C_GRAY, lw=1.0),
                 fontsize=8, color=C_GRAY, ha="right")

    # (b) nhieu diem xuat phat song song, hoi tu qua cac the he
    axR = axes[1]
    rng = np.random.default_rng(7)
    starts = rng.uniform(0.6, 9.4, 6)
    colors = [C_BLUE, C_GREEN, C_ORANGE, C_PURPLE, "#00897B", C_PINK]
    for sx, c in zip(starts, colors):
        gx = np.array([sx,
                       sx + (global_x - sx) * 0.45,
                       sx + (global_x - sx) * 0.80,
                       global_x + rng.normal(0, 0.12)])
        gy = np.array([f[np.argmin(np.abs(x - p))] for p in gx])
        alphas = [0.3, 0.5, 0.75, 1.0]
        sizes = [22, 32, 42, 60]
        for j in range(3):
            axR.plot(gx[j:j + 2], gy[j:j + 2], "-", color=c, lw=1.3,
                     alpha=alphas[j], zorder=3)
        for j in range(4):
            axR.scatter([gx[j]], [gy[j]], s=sizes[j], color=c, alpha=alphas[j],
                        zorder=4, edgecolor="white", lw=0.4)
    axR.scatter([global_x], [f.max()], marker="*", s=190, color=C_RED,
                edgecolor="white", lw=0.9, zorder=6)
    axR.annotate("Quần thể hội tụ dần\nvề vùng cực trị tốt hơn",
                 xy=(global_x, f.max()), xytext=(global_x - 4.8, f.max() - 0.05),
                 arrowprops=dict(arrowstyle="->", color=C_RED, lw=1.1),
                 fontsize=8.3, color=C_RED, ha="left")
    axR.text(0.2, f.min() - 0.75,
             "● = một cá thể trong quần thể   —   đường nối = đường đi qua các thế hệ kế tiếp",
             fontsize=7.6, color="#666")

    fig.suptitle("Hình 2.1 — Tìm Kiếm Đơn-Điểm Và Tìm Kiếm Theo Quần Thể",
                 fontsize=13, y=1.04)
    fig.tight_layout()
    save(fig, "hinh_2_1.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 2.2 - Vong lap tien hoa tong quat (flowchart)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_2():
    fig, ax = plt.subplots(figsize=(7.2, 9.8), facecolor=BG)
    ax.set_facecolor(BG)
    ax.set_xlim(0, 10)
    ax.set_ylim(0, 12.4)
    ax.axis("off")

    steps = [
        ("Khởi tạo quần thể",                 5, 11.4, "#E8F5E9", C_GREEN),
        ("Đánh giá fitness\ntừng cá thể",     5, 9.7,  "#E3F2FD", C_BLUE),
        ("Chọn lọc\n(Selection)",             5, 8.0,  "#E3F2FD", C_BLUE),
        ("Lai ghép\n(Crossover)",             5, 6.3,  "#E3F2FD", C_BLUE),
        ("Đột biến\n(Mutation)",              5, 4.6,  "#E3F2FD", C_BLUE),
        ("Thay thế thế hệ\n(+ Elitism)",      5, 2.9,  "#E3F2FD", C_BLUE),
    ]
    for label, cx, cy, fc, ec in steps:
        draw_box(ax, cx, cy, 3.3, 1.15, label, fc=fc, ec=ec, fs=9.3)
    coords = [(cx, cy) for _, cx, cy, _, _ in steps]
    for (x0, y0), (x1, y1) in zip(coords, coords[1:]):
        draw_arrow(ax, x0, y0 - 0.62, x1, y1 + 0.62)

    # hinh thoi dieu kien dung
    dcx, dcy = 5, 1.05
    diamond = plt.Polygon([(dcx, dcy + 0.8), (dcx + 1.85, dcy),
                           (dcx, dcy - 0.8), (dcx - 1.85, dcy)],
                          closed=True, fc="#FFF3E0", ec=C_ORANGE, lw=1.5, zorder=3)
    ax.add_patch(diamond)
    ax.text(dcx, dcy, "Đạt điều kiện\ndừng?", ha="center", va="center",
            fontsize=8.6, zorder=4)
    draw_arrow(ax, 5, 2.9 - 0.62, dcx, dcy + 0.8)

    # nhanh "chua dat" -> quay lai danh gia fitness
    loop_x = 1.0
    ax.plot([dcx - 1.85, loop_x], [dcy, dcy], color=C_GRAY, lw=1.4, zorder=2)
    ax.plot([loop_x, loop_x], [dcy, 9.7], color=C_GRAY, lw=1.4, zorder=2)
    draw_arrow(ax, loop_x, 9.7, 5 - 1.65, 9.7)
    ax.text(loop_x - 0.45, 5.3, "chưa đạt\n→ lặp lại", fontsize=8.2, color=C_GRAY,
            rotation=90, ha="center", va="center")

    # nhanh "dat" -> Ket thuc
    draw_arrow(ax, dcx + 1.85, dcy, 8.5, dcy)
    draw_box(ax, 8.85, dcy, 2.0, 1.05, "Kết thúc\n(trả về cá thể\ntốt nhất)",
             fc="#FCE4EC", ec=C_PINK, fs=8.4, bold=True)
    ax.text(7.2, dcy + 0.32, "đạt", fontsize=8.2, color=C_GRAY, ha="center")

    ax.set_title("Hình 2.2 — Vòng Lặp Tiến Hóa Tổng Quát Của Một GA Chuẩn",
                 fontsize=12.5, pad=16)
    save(fig, "hinh_2_2.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 2.3 - Hoi tu som vs tien hoa lanh manh
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_3():
    fig, axes = plt.subplots(1, 2, figsize=(11.5, 4.6), facecolor=BG)
    gens = np.arange(0, 60)

    div_prem = np.exp(-gens / 6.0)
    fit_prem = 3.0 + 2.0 * (1 - np.exp(-gens / 5.0))          # plateau thap (~5)

    div_heal = np.exp(-gens / 35.0)
    fit_heal = 3.0 + 6.0 * (1 - np.exp(-gens / 28.0))         # tang dan toi ~9

    panels = [
        (axes[0], "(a) Hội tụ sớm  (premature convergence)", div_prem, fit_prem,
         "Đa dạng mất sớm —\nfitness mắc kẹt ở mức thấp"),
        (axes[1], "(b) Tiến hóa lành mạnh", div_heal, fit_heal,
         "Đa dạng giảm chậm và đều —\nfitness tăng ổn định"),
    ]
    handles, labels = None, None
    for ax, title, div, fit, note in panels:
        ax.set_facecolor(BG)
        ax2 = ax.twinx()
        l1, = ax.plot(gens, div, color=C_BLUE, lw=2.2, zorder=3)
        l2, = ax2.plot(gens, fit, color=C_RED, lw=2.2, zorder=3)
        if handles is None:
            handles = [l1, l2]
            labels = ["Độ lệch chuẩn quần thể (đa dạng)", "Best fitness"]
        ax.set_ylim(0, 1.15)
        ax2.set_ylim(2, 10)
        ax.set_xlabel("Thế hệ", fontsize=9)
        ax.set_ylabel("Đa dạng (Std Dev)", fontsize=8.6, color=C_BLUE)
        ax2.set_ylabel("Best fitness", fontsize=8.6, color=C_RED)
        ax.tick_params(axis="y", labelcolor=C_BLUE, labelsize=8)
        ax2.tick_params(axis="y", labelcolor=C_RED, labelsize=8)
        ax.tick_params(axis="x", labelsize=8)
        ax.set_title(title, fontsize=10.8, pad=10)
        ax.spines["top"].set_visible(False)
        ax2.spines["top"].set_visible(False)
        ax.grid(axis="x", color=C_GRID, lw=0.6, alpha=0.6)
        ax.text(0.97, 0.06, note, transform=ax.transAxes, fontsize=8.1,
                ha="right", va="bottom", color="#555",
                bbox=dict(fc="white", ec="#cccccc", boxstyle="round,pad=0.35"))

    fig.legend(handles=handles, labels=labels, loc="upper center", ncol=2,
               fontsize=9, frameon=False, bbox_to_anchor=(0.5, 1.02))
    fig.suptitle("Hình 2.3 — Hội Tụ Sớm Và Tiến Hóa Lành Mạnh: Hai Kịch Bản Đối Chiếu",
                 fontsize=12.5, y=1.13)
    fig.tight_layout(rect=[0, 0, 1, 0.90])
    save(fig, "hinh_2_3.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 2.4 - Ke thua sau vs Component hoa
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_4():
    fig, axes = plt.subplots(1, 2, figsize=(12, 5.8), facecolor=BG)

    # ─── (a) cay ke thua sau ──────────────────────────────────────────────
    axL = axes[0]
    axL.set_facecolor(BG)
    axL.set_xlim(0, 10)
    axL.set_ylim(0, 10)
    axL.axis("off")
    axL.set_title("(a) Kế thừa sâu — bùng nổ tổ hợp lớp", fontsize=10.8, pad=8)

    nodes = {
        "Card":             (5.0, 9.2),
        "MeleeCard":        (3.0, 7.2),
        "RangedCard":       (7.0, 7.2),
        "TauntMelee":       (1.7, 5.0),
        "RebornMelee":      (3.95, 5.0),
        "TauntRanged":      (6.05, 5.0),
        "RebornRanged":     (8.3, 5.0),
        "TauntRebornMeleeA": (1.0, 2.6),
        "...x?": (2.6, 2.6),
        "..x??": (4.2, 2.6),
        "...x???": (5.8, 2.6),
        "..x????": (7.4, 2.6),
        "TauntRebornRangedB": (9.0, 2.6),
    }
    edges = [("Card", "MeleeCard"), ("Card", "RangedCard"),
             ("MeleeCard", "TauntMelee"), ("MeleeCard", "RebornMelee"),
             ("RangedCard", "TauntRanged"), ("RangedCard", "RebornRanged"),
             ("TauntMelee", "TauntRebornMeleeA"), ("TauntMelee", "...x?"),
             ("RebornMelee", "..x??"),
             ("TauntRanged", "...x???"), ("TauntRanged", "..x????"),
             ("RebornRanged", "TauntRebornRangedB")]
    for a, b in edges:
        x0, y0 = nodes[a]
        x1, y1 = nodes[b]
        axL.plot([x0, x1], [y0 - 0.38, y1 + 0.38], color="#bbbbbb", lw=1.0, zorder=1)
    for name, (x, y) in nodes.items():
        leaf = y < 4
        fc = "#FFEBEE" if leaf else "#E3F2FD"
        ec = C_RED if leaf else C_BLUE
        w = 1.45 if leaf else 1.95
        draw_box(axL, x, y, w, 0.66, name, fc=fc, ec=ec, fs=6.0 if leaf else 6.2)
    axL.text(5, 0.7,
             "→ thêm 1 đặc điểm mới (vd. \"Safeguard\") buộc nhân đôi\nsố lớp lá đã tồn tại — chi phí tăng theo cấp số nhân",
             ha="center", fontsize=8.2, color=C_RED)

    # ─── (b) component hoa ────────────────────────────────────────────────
    axR = axes[1]
    axR.set_facecolor(BG)
    axR.set_xlim(0, 10)
    axR.set_ylim(0, 10)
    axR.axis("off")
    axR.set_title("(b) Component hóa — lắp ráp hành vi từ các phần độc lập",
                  fontsize=10.8, pad=8)

    comps = [
        ("StatsComponent\n(ATK, HP)",         (5.0, 8.5)),
        ("RenderComponent\n(sprite, anim)",   (8.5, 6.6)),
        ("AbilityComponent\n(TTE skill)",     (8.5, 3.4)),
        ("CombatComponent\n(target, dmg)",    (5.0, 1.5)),
        ("AIWeightComponent\n(gene mapping)", (1.5, 3.4)),
        ("DataRefComponent\n(card định nghĩa)", (1.5, 6.6)),
    ]
    for label, (x, y) in comps:
        axR.plot([x, 5], [y, 5], color=C_GREEN, lw=1.3, alpha=0.6, zorder=1)
    draw_box(axR, 5, 5, 2.7, 1.5, "Entity\n(Card instance)",
             fc="#FFF8E1", ec=C_ORANGE, fs=10, bold=True)
    for label, (x, y) in comps:
        draw_box(axR, x, y, 2.65, 1.05, label, fc="#E8F5E9", ec=C_GREEN, fs=7.4)
    axR.text(5, 0.15,
             "→ thêm 1 đặc điểm mới = gắn thêm 1 component —\nkhông phải sửa hay nhân bản bất kỳ lớp nào đã có",
             ha="center", fontsize=8.2, color=C_GREEN)

    fig.suptitle("Hình 2.4 — Kế Thừa Sâu Và Component Hóa", fontsize=13, y=1.02)
    fig.tight_layout()
    save(fig, "hinh_2_4.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 2.5 - Hai mo hinh to chuc vong lap game
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_5():
    fig, axes = plt.subplots(1, 2, figsize=(12, 5.2), facecolor=BG)

    # ─── (a) dan xen ──────────────────────────────────────────────────────
    axL = axes[0]
    axL.set_facecolor(BG)
    axL.set_xlim(0, 10)
    axL.set_ylim(0, 10)
    axL.axis("off")
    axL.set_title("(a) Tính toán và trình diễn đan xen trong cùng vòng lặp",
                  fontsize=10.3, pad=8)
    seq = [("Tính bước 1", 2.4, 8.6, "#E3F2FD", C_BLUE),
           ("Trình diễn bước 1", 7.6, 7.2, "#FCE4EC", C_PINK),
           ("Tính bước 2", 2.4, 5.8, "#E3F2FD", C_BLUE),
           ("Trình diễn bước 2", 7.6, 4.4, "#FCE4EC", C_PINK),
           ("Tính bước 3", 2.4, 3.0, "#E3F2FD", C_BLUE),
           ("Trình diễn bước 3", 7.6, 1.6, "#FCE4EC", C_PINK)]
    for label, x, y, fc, ec in seq:
        draw_box(axL, x, y, 3.0, 0.95, label, fc=fc, ec=ec, fs=8.0)
    coords = [(x, y) for _, x, y, _, _ in seq]
    for (x0, y0), (x1, y1) in zip(coords, coords[1:]):
        draw_arrow(axL, x0, y0, x1, y1, color="#999999", lw=1.2)
    axL.text(5, 0.35,
             "mỗi bước phải đợi bước trước \"trình diễn xong\"\n→ không thể tách rời để chạy độc lập, khó kiểm thử, khó headless",
             ha="center", fontsize=8.0, color="#555555")

    # ─── (b) tach biet ────────────────────────────────────────────────────
    axR = axes[1]
    axR.set_facecolor(BG)
    axR.set_xlim(0, 10)
    axR.set_ylim(0, 10)
    axR.axis("off")
    axR.set_title("(b) Tính toán hoàn tất trước, trình diễn độc lập sau",
                  fontsize=10.3, pad=8)
    draw_box(axR, 2.7, 7.7, 4.0, 1.7,
             "TÍNH TOÁN\n(đồng bộ, tức thời)\nchạy trọn vẹn N bước liên tiếp",
             fc="#E3F2FD", ec=C_BLUE, fs=8.2)
    draw_box(axR, 2.7, 4.7, 4.0, 1.25,
             "Nhật ký hành động\n(TurnRecord — chuỗi sự kiện có thứ tự)",
             fc="#FFF8E1", ec=C_ORANGE, fs=8.0, bold=True)
    draw_box(axR, 2.7, 1.7, 4.0, 1.5,
             "TRÌNH DIỄN\n(bất đồng bộ, theo thời gian thực)\nphát lại hoạt ảnh / âm thanh / UI",
             fc="#FCE4EC", ec=C_PINK, fs=8.0)
    draw_arrow(axR, 2.7, 7.7 - 0.9, 2.7, 4.7 + 0.68)
    draw_arrow(axR, 2.7, 4.7 - 0.68, 2.7, 1.7 + 0.8)
    draw_arrow(axR, 2.7 + 2.05, 4.7, 7.45, 4.7, color=C_GREEN)
    draw_box(axR, 8.45, 4.7, 2.3, 1.5,
             "Headless / Batch:\nbỏ qua nhánh trình diễn,\nlặp lại hàng trăm nghìn lần",
             fc="#E8F5E9", ec=C_GREEN, fs=7.0)
    axR.text(5, 0.25,
             "cùng MỘT lõi tính toán — chạy có người xem (game thật)\nhoặc không (huấn luyện AI), không có nguy cơ phân kỳ logic",
             ha="center", fontsize=8.0, color="#555555")

    fig.suptitle("Hình 2.5 — Hai Mô Hình Tổ Chức Vòng Lặp Game", fontsize=13, y=1.02)
    fig.tight_layout()
    save(fig, "hinh_2_5.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 2.6 - Khuon mau Trigger - Target - Effect (khong gian to hop 3 chieu)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_6():
    fig = plt.figure(figsize=(8.8, 7.4), facecolor=BG)
    ax = fig.add_subplot(111, projection="3d")
    ax.set_facecolor(BG)
    try:
        ax.xaxis.pane.set_facecolor(BG)
        ax.yaxis.pane.set_facecolor(BG)
        ax.zaxis.pane.set_facecolor(BG)
    except Exception:
        pass

    triggers = ["StartBattle", "OnDeath", "OnAttack", "OnTakeDmg", "EndTurnShop", "OnDeploy"]
    targets  = ["Self", "RandomAlly", "WeakestEnemy", "AllAllies", "AllEnemies"]
    effects  = ["AddStats", "Summon", "DealDmg", "GainCoin", "GiveBuff"]
    nt, nx, ne = len(triggers), len(targets), len(effects)

    xs, ys, zs = np.meshgrid(range(nt), range(nx), range(ne), indexing="ij")
    ax.scatter(xs.ravel(), ys.ravel(), zs.ravel(),
               color="#90A4AE", s=14, alpha=0.30, depthshade=False, zorder=1)

    # diem duoc lam noi bat = mot ky nang cu the (Anubis: OnDeath / Self / AddStats)
    hi, hj, hk = 1, 0, 0
    ax.scatter([hi], [hj], [hk], color=C_RED, s=210, edgecolor="white",
               linewidth=1.3, zorder=10)
    ax.text(hi, hj, hk + 1.15,
            "Một điểm = một kỹ năng cụ thể\nVD: Anubis = (OnDeath, Self, AddStats)",
            color=C_RED, fontsize=8.2, ha="center", zorder=11)

    ax.set_xticks(range(nt))
    ax.set_xticklabels(triggers, fontsize=6.6, rotation=18, ha="right")
    ax.set_yticks(range(nx))
    ax.set_yticklabels(targets, fontsize=6.6)
    ax.set_zticks(range(ne))
    ax.set_zticklabels(effects, fontsize=6.6)
    ax.set_xlabel("\nTrigger — Khi nào?", fontsize=9.5, labelpad=10)
    ax.set_ylabel("\nTarget — Nhắm vào ai?", fontsize=9.5, labelpad=10)
    ax.set_zlabel("Effect — Làm gì?", fontsize=9.5, labelpad=2)
    ax.set_title("Hình 2.6 — Khuôn Mẫu Trigger – Target – Effect\n"
                 "Như Một Không Gian Tổ Hợp Ba Chiều", fontsize=12, pad=0)
    ax.view_init(elev=16, azim=-58)
    fig.tight_layout()
    save(fig, "hinh_2_6.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 2.7 - Danh doi Tempo va Economy theo boi canh
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_7():
    fig, ax = plt.subplots(figsize=(8.4, 5.2), facecolor=BG)
    ax.set_facecolor(BG)
    x = np.linspace(0, 1, 240)

    y_lead   = np.exp(-((x - 0.70) ** 2) / (2 * 0.17 ** 2))   # dang dan dau -> co the dau tu dai han
    y_danger = np.exp(-((x - 0.25) ** 2) / (2 * 0.15 ** 2))   # dang nguy cap -> can manh ngay

    ax.plot(x, y_lead, color=C_BLUE, lw=2.4,
            label="Đang dẫn đầu  —  \"có thể chờ khoản đầu tư phát huy\"")
    ax.plot(x, y_danger, color=C_RED, lw=2.4,
            label="Đang nguy cấp  —  \"phải mạnh ngay, chưa chắc còn tương lai\"")
    ax.fill_between(x, y_lead, alpha=0.08, color=C_BLUE)
    ax.fill_between(x, y_danger, alpha=0.08, color=C_RED)

    ann_specs = [
        (y_lead,   C_BLUE, 0.70, (0.70, 1.16), "center"),
        (y_danger, C_RED,  0.25, (0.42, 0.74), "left"),
    ]
    for y, c, xm, (tx, ty), ha in ann_specs:
        ax.scatter([xm], [y.max()], color=c, s=85, zorder=5, edgecolor="white", lw=1.1)
        ax.annotate("điểm lựa chọn\ntối ưu", xy=(xm, y.max()), xytext=(tx, ty),
                    fontsize=7.8, color=c, ha=ha,
                    arrowprops=dict(arrowstyle="->", color=c, lw=1.0))

    ax.annotate("", xy=(1.0, -0.16), xytext=(0.0, -0.16),
                arrowprops=dict(arrowstyle="-|>", color="#888888"))
    ax.text(0.0, -0.24, "ưu tiên Tempo\n(mạnh tức thời)", fontsize=8.2, ha="left", color="#666666")
    ax.text(1.0, -0.24, "ưu tiên Economy\n(đầu tư dài hạn)", fontsize=8.2, ha="right", color="#666666")

    ax.set_xlim(-0.02, 1.02)
    ax.set_ylim(-0.34, 1.32)
    ax.set_xticks([])
    ax.set_ylabel("Xác suất sống sót / thắng cuộc", fontsize=9.5)
    ax.set_xlabel("Mức độ ưu tiên đầu tư dài hạn", fontsize=9.5, labelpad=30)
    ax.set_title("Hình 2.7 — Đánh Đổi Tempo Và Economy Theo Bối Cảnh", fontsize=12.5, pad=12)
    ax.legend(loc="upper left", fontsize=8.4, frameon=False)
    ax.spines[["top", "right"]].set_visible(False)
    fig.tight_layout()
    save(fig, "hinh_2_7.png")


# ══════════════════════════════════════════════════════════════════════════════
if __name__ == "__main__":
    hinh_2_1()
    hinh_2_2()
    hinh_2_3()
    hinh_2_4()
    hinh_2_5()
    hinh_2_6()
    hinh_2_7()
    print("\n[DONE] 7 hinh Chuong 2 da duoc luu vao:", OUT)
