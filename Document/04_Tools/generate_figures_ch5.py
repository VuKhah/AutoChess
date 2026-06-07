"""
Tao Hinh 5.1 - 5.8 (CHUONG 5 - THUAT TOAN DI TRUYEN VA HE THONG AI) cho tieu luan AutoChess.

8 hinh, 2 nhom:
  - So do khai niem (ve truc tiep theo dac ta trong Hinh.md):
      5.1 Kien truc tong the he thong AI (4 thanh phan + luong du lieu)
      5.2 Bieu do 37 gene phan nhom mau sac (9 nhom chuc nang)
      5.4 Flowchart 7-phase DecidePrepPhase
      5.5 Su mo rong cua ham fitness qua cac vong tinh chinh (doi chieu 2 phien ban)
  - Bieu do du lieu thuc (gene 5 bot trich tu AI_Library.json tai commit 6e1c9f6
    "Tune GA training output" - DUNG ban tuong ung voi training_20260601_213435.csv
    duoc trich dan trong essay, KHONG dung AI_Library.json hien tai tren disk vi
    no la ket qua cua mot lan train sau, fitness khac han so noi dung 5.7):
      5.3 Bang 37 gene + gia tri 5 bot (heatmap + bieu do cot trung binh theo nhom)
      5.6 Duong cong fitness qua 180 the he (CSV that)
      5.7 Phan phoi tribe qua cac the he (CSV that, area chart xep chong)
      5.8 Radar chart 8 truc cho 5 bot (gene 5 bot, rut gon tu 9 nhom -> 8 truc)

Chay:   python generate_figures_ch5.py
Ket qua: Document/03_Figures/hinh_5_1.png .. hinh_5_8.png
"""

import os
import csv
import numpy as np
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.colors as mcolors
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch

matplotlib.rcParams["font.family"] = "DejaVu Sans"
matplotlib.rcParams["axes.unicode_minus"] = False

# ─── Duong dan ────────────────────────────────────────────────────────────────
BASE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(BASE, "..", ".."))
OUT  = os.path.join(ROOT, "Document", "03_Figures")
CSV_PATH = os.path.join(ROOT, "Document", "02_Data", "Train", "training_20260601_213435.csv")
os.makedirs(OUT, exist_ok=True)

# ─── Bang mau dung chung (dong bo voi generate_figures_ch2/3/4.py) ────────────
BG       = "#FAFAFA"
C_GRID   = "#DDDDDD"
C_BLUE   = "#1565C0"
C_RED    = "#E04040"
C_ORANGE = "#F0A030"
C_GREEN  = "#2E7D32"
C_PURPLE = "#8040C0"
C_GRAY   = "#888888"
C_PINK   = "#D81B60"
C_TEAL   = "#00897B"
C_BROWN  = "#8D6E63"
C_OLIVE  = "#AFB42B"


def save(fig, name):
    path = os.path.join(OUT, name)
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


def draw_box(ax, cx, cy, w, h, text, fc="#E3F2FD", ec=C_BLUE, fs=9,
             bold=False, radius=0.04, color="#222222"):
    rect = FancyBboxPatch((cx - w / 2, cy - h / 2), w, h,
                          boxstyle=f"round,pad={radius}",
                          fc=fc, ec=ec, lw=1.5, zorder=3)
    ax.add_patch(rect)
    ax.text(cx, cy, text, ha="center", va="center", fontsize=fs, color=color,
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


def tint(hex_color, frac=0.80):
    """Tron mau voi mau trang -> phien ban nhat dung lam mau nen (fc) cho o/hop."""
    r, g, b = mcolors.to_rgb(hex_color)
    return (r + (1 - r) * frac, g + (1 - g) * frac, b + (1 - b) * frac)


# ─── Du lieu goc: 37 gene, 9 nhom chuc nang (doi chieu 05_1_..._Chromosome.md:71-109) ─
GENES = [
    (0, "wATK", 1), (1, "wHP", 1), (2, "wTierBonus", 1), (3, "wCostEff", 1),
    (4, "wTaunt", 2), (5, "wReborn", 2), (6, "wSafeguard", 2),
    (7, "tStartBattle", 3), (8, "tOnDeath", 3), (9, "tOnAttack", 3),
    (10, "tOnTakeDmg", 3), (11, "tEndTurnShop", 3), (12, "tOnDeploy", 3),
    (13, "eAddStats", 4), (14, "eSummon", 4), (15, "eDealDmg", 4),
    (16, "eGainCoin", 4), (17, "eGiveBuff", 4),
    (18, "sBabylon", 5), (19, "sOlympus", 5), (20, "sNiles", 5),
    (21, "wMerge", 6), (22, "wFrontline", 6), (23, "wSaveThreshold", 6),
    (24, "wRerollThresh", 7), (25, "wRerollMax", 7), (26, "wRerollKeep", 7), (27, "wProactiveSell", 7),
    (28, "wSpellThresh", 8), (29, "wSpellOnStrong", 8), (30, "wSpellOnMerged", 8), (31, "wSpellEconomy", 8),
    (32, "tAura", 9), (33, "tOnSell", 9), (34, "tOnAllyGroup", 9), (35, "tOnAllyDeploy", 9), (36, "tOnAllySell", 9),
]

GROUPS = {
    1: ("Stat cơ bản",      C_BLUE),
    2: ("Keyword",          C_PINK),
    3: ("Trigger (cha)",    C_RED),
    4: ("Effect",           C_ORANGE),
    5: ("Tribe synergy",    C_GREEN),
    6: ("Board / Merge",    C_PURPLE),
    7: ("Reroll",           C_TEAL),
    8: ("Spell",            C_BROWN),
    9: ("Trigger (con)",    C_OLIVE),
}

# ─── Du lieu that: gene cua 5 bot, trich AI_Library.json @ commit 6e1c9f6 ─────
# ("Tune GA training output", 2026-06-01 21:51) — ban duy nhat co hardBot.fitness
# = 4764.0, khop chinh xac voi training_20260601_213435.csv duoc trich dan trong
# muc 5.7 cua essay. AI_Library.json HIEN TAI tren disk la ket qua mot lan train
# sau (commit 36d9c1f, fitness ~21000) — KHONG dung cho hinh nay.
BOT_GENES = {
    "hardBot":      [0.807, 0.947, 0.448, 0.866, 0.698, 1.000, 0.764, 0.827, 0.751, 0.204,
                     0.644, 0.761, 0.126, 0.234, 0.860, 0.251, 0.870, 1.000, 0.736, 0.086,
                     0.264, 0.650, 0.446, 0.358, 0.995, 0.494, 0.727, 0.178, 0.167, 0.677,
                     0.757, 0.133, 0.154, 0.415, 1.000, 0.577, 0.611],
    "babylonBot":   [0.300, 1.000, 0.279, 0.745, 0.239, 0.715, 0.889, 0.931, 0.943, 0.207,
                     0.072, 0.132, 0.235, 0.818, 0.780, 0.319, 0.984, 0.811, 0.608, 0.441,
                     0.271, 0.656, 0.637, 0.043, 0.687, 0.138, 0.286, 0.346, 0.221, 0.741,
                     0.053, 0.501, 0.396, 0.090, 0.997, 1.000, 0.832],
    "nileBot":      [0.255, 1.000, 0.293, 0.736, 0.284, 0.701, 0.884, 0.962, 0.951, 0.207,
                     0.953, 0.177, 0.223, 1.000, 0.780, 0.139, 0.947, 0.850, 0.167, 0.345,
                     0.617, 0.902, 0.585, 0.216, 0.675, 0.161, 0.286, 0.346, 0.215, 0.673,
                     0.053, 0.431, 0.432, 0.075, 0.997, 1.000, 0.789],
    "summonerBot":  [0.130, 0.835, 0.317, 0.794, 0.254, 0.661, 0.889, 0.931, 0.943, 0.207,
                     0.072, 0.612, 0.787, 0.890, 0.952, 0.041, 0.671, 0.980, 0.167, 0.212,
                     0.786, 0.776, 0.761, 0.295, 0.940, 0.351, 0.476, 0.000, 0.386, 0.598,
                     0.265, 0.266, 0.377, 0.197, 1.000, 1.000, 0.851],
    "resilientBot": [0.020, 1.000, 0.361, 0.874, 1.000, 1.000, 0.944, 1.000, 0.996, 0.000,
                     1.000, 1.000, 0.223, 1.000, 0.631, 0.531, 1.000, 0.849, 0.421, 0.402,
                     0.704, 0.848, 0.499, 0.027, 0.693, 0.000, 0.202, 0.346, 0.230, 0.694,
                     0.079, 0.431, 0.432, 0.211, 1.000, 0.951, 0.854],
}
BOT_ORDER = ["hardBot", "babylonBot", "nileBot", "summonerBot", "resilientBot"]
BOT_LABELS = ["hardBot\n(generalist)", "babylonBot", "nileBot", "summonerBot", "resilientBot"]
BOT_COLORS = ["#37474F", "#F0A030", C_BLUE, C_PURPLE, C_GREEN]

# 8 truc radar — rut gon tu 9 nhom (nhom 9 - trigger con - gop chung vao "Trigger")
RADAR_AXES = [
    ("Stat",     [0, 1, 2, 3]),
    ("Keywords", [4, 5, 6]),
    ("Trigger",  [7, 8, 9, 10, 11, 12, 32, 33, 34, 35, 36]),
    ("Effect",   [13, 14, 15, 16, 17]),
    ("Tribe",    [18, 19, 20]),
    ("Board",    [21, 22, 23]),
    ("Reroll",   [24, 25, 26, 27]),
    ("Spell",    [28, 29, 30, 31]),
]


def load_training_csv():
    with open(CSV_PATH, encoding="utf-8-sig") as f:
        rows = list(csv.DictReader(f))
    g = lambda key, cast=float: np.array([cast(r[key]) for r in rows])
    return dict(gen=g("gen", int), best=g("best"), avg=g("avg"), worst=g("worst"),
                std=g("std_dev"), pct_b=g("pct_babylon"), pct_n=g("pct_niles"), pct_o=g("pct_other"))


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.1 - Kien truc tong the he thong AI (4 thanh phan + AI_Library.json)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_1():
    fig, ax = plt.subplots(figsize=(12.4, 11.2))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cx = 3.55
    bw, bh = 4.7, 1.5
    ys = [9.2, 6.9, 4.6, 2.3, 0.0]

    stages = [
        dict(text="Chromosome\n(class Chromosome — 37 gene thực, [0,1])",
             fc=tint(C_PURPLE), ec=C_PURPLE,
             desc="mảng 37 số thực — biểu diễn chiến lược chơi,\nbản thân không chứa bất kỳ logic nào"),
        dict(text="BotAgent\nDecidePrepPhase()",
             fc=tint(C_BLUE), ec=C_BLUE,
             desc="engine thực thi — dùng 37 gene làm \"bộ não\"\nđể quyết định mỗi lượt mua/bán/reroll"),
        dict(text="GameSimulator\nEvaluateMatch(botA, botB) → MatchResult",
             fc=tint(C_ORANGE), ec=C_ORANGE,
             desc="môi trường đánh giá — chạy trận 20 lượt\n× [DecidePrepPhase + ResolveTurn]"),
        dict(text="GATrainer\nInit → Evaluate → Select → Crossover → Mutate",
             fc=tint(C_GREEN), ec=C_GREEN,
             desc="vòng lặp tiến hóa — điều phối toàn bộ quá trình\nqua nhiều thế hệ, chọn ra 5 bot chuyên biệt"),
        dict(text="AI_Library.json\n(5 bot chuyên biệt — kết quả cuối cùng)",
             fc=tint(C_GRAY), ec=C_GRAY,
             desc="file kết quả — AIManager đọc khi game khởi động,\nminh bạch, có thể xác nhận thủ công"),
    ]

    arrow_labels = [
        "\"bộ não\" định hình chiến lược",
        "board state mỗi lượt shop",
        "fitness scores qua hàng trăm nghìn trận",
        "ghi 5 bot đã chọn vào file",
    ]

    for s, y in zip(stages, ys):
        draw_box(ax, cx, y, bw, bh, s["text"], fc=s["fc"], ec=s["ec"], fs=9.6, bold=True)
        ax.text(cx + bw / 2 + 0.35, y, s["desc"], fontsize=8.0, color="#555555",
                ha="left", va="center", style="italic")

    for (a, b), lbl in zip(zip(ys[:-1], ys[1:]), arrow_labels):
        draw_arrow(ax, cx, a - bh / 2 - 0.05, cx, b + bh / 2 + 0.05, color=C_GRAY, lw=1.6)
        ax.text(cx - bw / 2 - 0.3, (a + b) / 2, lbl, fontsize=7.6, color="#555555",
                ha="right", va="center", style="italic")

    ax.set_xlim(-3.6, cx + bw / 2 + 6.6)
    ax.set_ylim(ys[-1] - bh / 2 - 0.6, ys[0] + bh / 2 + 0.7)
    ax.axis("off")
    ax.set_title("Hình 5.1 — Kiến Trúc Tổng Thể Hệ Thống AI: Bốn Thành Phần",
                 fontsize=12.5, pad=12, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_5_1.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.2 - 37 gene phan nhom mau sac (9 hang ngang, moi hang mot nhom)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_2():
    fig, ax = plt.subplots(figsize=(13.6, 9.4))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cell_w, cell_h, gap = 1.0, 0.78, 0.10
    label_w = 3.05
    row_h = cell_h + 0.42
    n_groups = 9
    y_top = n_groups * row_h

    # nhom theo thu tu chi so gene tang dan
    by_group = {}
    for idx, name, gnum in GENES:
        by_group.setdefault(gnum, []).append((idx, name))

    for i, gnum in enumerate(range(1, n_groups + 1)):
        y = y_top - i * row_h
        gname, color = GROUPS[gnum]
        members = by_group[gnum]

        # nhan nhom ben trai (the mau + ten + so luong gene)
        sw = FancyBboxPatch((0.0, y - cell_h / 2), 0.4, cell_h,
                            boxstyle="round,pad=0.02", fc=color, ec=color, zorder=3)
        ax.add_patch(sw)
        ax.text(0.6, y, f"Nhóm {gnum} — {gname}\n({len(members)} gene)",
                fontsize=8.4, ha="left", va="center", color="#333333")

        # cac o gene cua nhom, can trai bat dau tu label_w
        for j, (idx, name) in enumerate(members):
            x = label_w + j * (cell_w + gap)
            draw_box(ax, x + cell_w / 2, y, cell_w, cell_h, f"{idx}\n{name}",
                     fc=tint(color, 0.78), ec=color, fs=6.8, radius=0.05)

    max_members = max(len(v) for v in by_group.values())
    x_right = label_w + max_members * (cell_w + gap)

    ax.text(label_w, y_top + 0.85,
            "37 ô — mỗi ô: chỉ số gene (trên) + tên viết tắt (dưới) — sắp theo thứ tự chỉ số trong từng nhóm",
            fontsize=8.2, color="#666666", ha="left", style="italic")

    ax.set_xlim(-0.3, x_right + 0.4)
    ax.set_ylim(0.0, y_top + 1.5)
    ax.axis("off")
    ax.set_title("Hình 5.2 — 37 Gene Của Chromosome, Phân Theo 9 Nhóm Chức Năng",
                 fontsize=12.5, pad=12, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_5_2.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.3 - Bang 37 gene + gia tri 5 bot (heatmap + bieu do cot trung binh nhom)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_3():
    fig = plt.figure(figsize=(11.6, 18.4))
    fig.patch.set_facecolor(BG)
    gs = fig.add_gridspec(2, 1, height_ratios=[5.1, 1.0], hspace=0.085)
    ax_hm = fig.add_subplot(gs[0])
    ax_bar = fig.add_subplot(gs[1])
    for a in (ax_hm, ax_bar):
        a.set_facecolor(BG)

    cmap = mcolors.LinearSegmentedColormap.from_list("genes", ["#FFFFFF", C_BLUE])
    n = len(GENES)
    matrix = np.array([[BOT_GENES[bot][idx] for bot in BOT_ORDER] for idx, _, _ in GENES])

    # ─ panel tren: heatmap 37 hang × 5 cot, the mau nhom ben trai ─
    for row, (idx, name, gnum) in enumerate(GENES):
        y = n - 1 - row
        gname, color = GROUPS[gnum]
        sw = FancyBboxPatch((-2.55, y - 0.46), 0.34, 0.92,
                            boxstyle="round,pad=0.015", fc=color, ec=color, zorder=3)
        ax_hm.add_patch(sw)
        ax_hm.text(-2.1, y, f"[{idx}] {name}", fontsize=6.7, ha="left", va="center",
                   color="#333333")
        for col, bot in enumerate(BOT_ORDER):
            val = BOT_GENES[bot][idx]
            ax_hm.add_patch(plt.Rectangle((col - 0.46, y - 0.46), 0.92, 0.92,
                                          fc=cmap(val), ec="white", lw=0.6, zorder=2))
            txt_color = "white" if val > 0.62 else "#333333"
            ax_hm.text(col, y, f"{val:.2f}", fontsize=6.0, ha="center", va="center",
                       color=txt_color, zorder=4)

    for col, lbl in enumerate(BOT_LABELS):
        ax_hm.text(col, n + 0.55, lbl.split("\n")[0], fontsize=9.2, ha="center", va="bottom",
                   color=BOT_COLORS[col], fontweight="bold")

    # vach phan cach giua cac nhom gene
    row = 0
    prev_gnum = GENES[0][2]
    for idx, name, gnum in GENES:
        if gnum != prev_gnum:
            y = n - row - 0.5
            ax_hm.plot([-2.55, 4.6], [y, y], color="#BBBBBB", lw=0.7, zorder=1)
            prev_gnum = gnum
        row += 1

    ax_hm.set_xlim(-2.75, 4.65)
    ax_hm.set_ylim(-0.8, n + 1.0)
    ax_hm.axis("off")
    ax_hm.set_title("Bảng giá trị 37 gene — hardBot · babylonBot · nileBot · summonerBot · resilientBot",
                    fontsize=10.6, pad=10, fontweight="bold")

    sm = plt.cm.ScalarMappable(cmap=cmap, norm=mcolors.Normalize(vmin=0, vmax=1))
    sm.set_array([])
    cb = fig.colorbar(sm, ax=ax_hm, orientation="vertical", fraction=0.022,
                      pad=0.015, shrink=0.5, aspect=24)
    cb.set_label("giá trị gene\n[0 → thấp · 1 → cao]", fontsize=7.2, color="#666666")
    cb.ax.tick_params(labelsize=6.6)

    # ─ panel duoi: bieu do cot — gia tri trung binh moi nhom (9 nhom) cho 5 bot ─
    group_nums = list(range(1, 10))
    group_names = [f"N{g}\n{GROUPS[g][0]}" for g in group_nums]
    x = np.arange(len(group_nums))
    bw = 0.155

    for i, bot in enumerate(BOT_ORDER):
        idx_by_group = {g: [gi for gi, _, gn in GENES if gn == g] for g in group_nums}
        means = [np.mean([BOT_GENES[bot][gi] for gi in idx_by_group[g]]) for g in group_nums]
        ax_bar.bar(x + (i - 2) * bw, means, width=bw * 0.92, color=BOT_COLORS[i],
                   label=BOT_LABELS[i].split("\n")[0], zorder=3)

    ax_bar.set_xticks(x)
    ax_bar.set_xticklabels(group_names, fontsize=6.8)
    ax_bar.set_ylabel("Giá trị TB / nhóm", fontsize=8.6)
    ax_bar.set_ylim(0, 1.05)
    ax_bar.set_title("Giá trị gene trung bình theo từng nhóm (9 nhóm) — góc nhìn tổng hợp đối chiếu 5 bot",
                     fontsize=9.6, pad=8)
    ax_bar.legend(loc="upper center", bbox_to_anchor=(0.5, -0.30), ncol=5, fontsize=7.6, frameon=False)
    ax_bar.spines[["top", "right"]].set_visible(False)
    ax_bar.grid(axis="y", color=C_GRID, lw=0.6, zorder=0)
    ax_bar.set_axisbelow(True)

    fig.suptitle("Hình 5.3 — Bảng 37 Gene Với Giá Trị Của 5 Bot Được Chọn",
                 fontsize=13, y=0.997, fontweight="bold")
    fig.text(0.5, 0.012,
             "Nguồn dữ liệu thực: AI_Library.json @ commit 6e1c9f6 \"Tune GA training output\" "
             "(khớp training_20260601_213435.csv, hardBot.fitness = 4764 — đúng lần training trích dẫn ở mục 5.7)",
             fontsize=7.4, color="#777777", ha="center", style="italic")
    save(fig, "hinh_5_3.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.4 - Flowchart 7-phase DecidePrepPhase (so do doc 7 buoc)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_4():
    fig, ax = plt.subplots(figsize=(11.6, 17.4))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cx = 4.6
    bw, bh = 6.8, 1.85
    step = 2.55
    ys = [16.0 - i * step for i in range(7)]

    phases = [
        dict(num="①", name="RerollPhase", color=C_BLUE,
             cond="Reroll nếu shop kém hơn board × genes[24]\ntối đa genes[25]×3+1 lần · giữ ≥ genes[26] coin",
             out="→ shop mới hoặc giữ nguyên"),
        dict(num="②", name="BuyUnitsPhase", color=C_TEAL,
             cond="Greedy mua card điểm cao nhất vượt genes[23]×3\nbán unit yếu nhất nhường slot nếu lợi nhuận rõ",
             out="→ board được cập nhật"),
        dict(num="③", name="BuySpellsPhase", color=C_PURPLE,
             cond="EvaluateSpell() theo từng EffectType\nmua nếu điểm chuẩn hóa ≥ genes[28]×3",
             out="→ spell mua + áp dụng lên board"),
        dict(num="④", name="ProactiveSellPhase", color=C_ORANGE,
             cond="Bán unit có điểm dưới genes[27]×3\ndù board chưa đầy (giữ lại unit token tạm thời)",
             out="→ giải phóng slot & coin sớm"),
        dict(num="⑤", name="TryMerge", color=C_GRAY,
             cond="logic tất định — KHÔNG phụ thuộc gene\n3 bản sao lv0→lv1 · 2 bản sao lv1→lv2",
             out="→ unit được nâng cấp, giữ bonus lớn nhất"),
        dict(num="⑥", name="RepositionPhase", color=C_PINK,
             cond="genes[22] Taunt · [8] tOnDeath → frontline\ngenes[17] eGiveBuff → backline (Aura/support)",
             out="→ đội hình sắp xếp theo PositionScore"),
        dict(num="⑦", name="FreezePhase", color=C_GREEN,
             cond="Freeze nếu (1 − genes[24]) ≥ 0.35\nVÀ có card muốn mua nhưng chưa đủ tiền",
             out="→ giữ nguyên shop cho lượt kế tiếp"),
    ]

    for i, (p, y) in enumerate(zip(phases, ys)):
        rect = FancyBboxPatch((cx - bw / 2, y - bh / 2), bw, bh,
                              boxstyle="round,pad=0.04", fc=tint(p["color"], 0.84),
                              ec=p["color"], lw=1.6, zorder=3)
        ax.add_patch(rect)
        ax.text(cx, y + 0.52, f"{p['num']}  {p['name']}", fontsize=11.6,
                ha="center", va="center", fontweight="bold", color="#222222", zorder=4)
        ax.text(cx, y - 0.32, p["cond"], fontsize=7.9, ha="center", va="center",
                color="#444444", zorder=4)
        ax.text(cx + bw / 2 + 0.35, y, p["out"], fontsize=8.4, ha="left", va="center",
                color=p["color"], style="italic", fontweight="bold")
        if i < len(phases) - 1:
            nxt_y = ys[i + 1]
            draw_arrow(ax, cx, y - bh / 2 - 0.06, cx, nxt_y + bh / 2 + 0.06,
                       color=C_GRAY, lw=1.6)

    ax.text(cx, ys[0] + bh / 2 + 0.85,
            "Bảy phase chạy tuần tự theo thứ tự ưu tiên logic — mỗi lượt chuẩn bị,\n"
            "gọi từ DecidePrepPhase(): reroll trước khi mua → mua trước khi bán →\n"
            "merge sau khi mua → sắp xếp sau khi merge → freeze sau cùng",
            fontsize=8.4, ha="center", va="bottom", color="#555555", style="italic")

    ax.text(cx, ys[-1] - bh / 2 - 0.85,
            "[Kết thúc lượt chuẩn bị — board sẵn sàng cho ResolveTurn() ở GameSimulator]",
            fontsize=8.6, ha="center", color=C_GREEN, fontweight="bold")

    ax.set_xlim(cx - bw / 2 - 0.5, cx + bw / 2 + 4.2)
    ax.set_ylim(ys[-1] - bh / 2 - 1.5, ys[0] + bh / 2 + 1.75)
    ax.axis("off")
    ax.set_title("Hình 5.4 — Bảy Phase Của DecidePrepPhase: Hành Vi Và Gene Chi Phối",
                 fontsize=12.5, pad=12, fontweight="bold")
    fig.tight_layout()
    save(fig, "hinh_5_4.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.5 - Su mo rong cua ham fitness qua cac vong tinh chinh (2 panel doi chieu)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_5():
    fig, axes = plt.subplots(1, 2, figsize=(14.4, 7.2))
    fig.patch.set_facecolor(BG)
    for ax in axes:
        ax.set_facecolor(BG)
        ax.set_xlim(0, 10)
        ax.set_ylim(-1.1, 10.4)
        ax.axis("off")

    # ── Panel (a): phien ban dau — chi thang/thua, phan phoi roi rac 3 gia tri ──
    axL = axes[0]
    axL.text(5, 9.85, "Phiên bản đầu — chỉ thắng / thua",
             fontsize=11.2, ha="center", fontweight="bold", color="#333333")
    axL.text(5, 9.1, "tín hiệu \"trung thực\" nhất với luật chơi — nhưng chỉ phân biệt\n"
                     "ĐƯỢC kết quả, không phân biệt được CÁCH đi đến kết quả đó",
             fontsize=7.8, ha="center", color="#666666", style="italic")

    base_y = 2.6
    bars = [("THẮNG", 10, C_GREEN), ("HÒA", 2, C_GRAY), ("THUA", 0, C_RED)]
    xs = [2.3, 5.0, 7.7]
    max_h = 5.4
    for (lbl, val, color), x in zip(bars, xs):
        h = max(0.12, val / 10 * max_h)
        axL.add_patch(plt.Rectangle((x - 0.75, base_y), 1.5, h, fc=tint(color, 0.55),
                                    ec=color, lw=1.6, zorder=3))
        axL.text(x, base_y + h + 0.32, str(val), fontsize=11, ha="center",
                 fontweight="bold", color=color)
        axL.text(x, base_y - 0.4, lbl, fontsize=9.4, ha="center", color="#444444",
                 fontweight="bold")

    draw_arrow(axL, 5, base_y - 0.85, 5, -0.05, color=C_RED, lw=2.0)
    draw_box(axL, 5, -0.65, 8.6, 0.95,
             "Quần thể hội tụ nhanh về một chiến lược RUSH một chiều\n"
             "(áp đảo early-game — sụp đổ trước đối thủ biết tích lũy & scale)",
             fc=tint(C_RED, 0.85), ec=C_RED, fs=8.0, bold=True, color=C_RED)

    # ── Panel (b): phien ban hien hanh — sau tin hieu cong don, ba nhom co trong so ──
    axR = axes[1]
    axR.text(5, 9.85, "Phiên bản hiện hành — sáu tín hiệu cộng dồn, ba nhóm",
             fontsize=11.2, ha="center", fontweight="bold", color="#333333")
    axR.text(5, 9.1, "mỗi nhóm đo một khía cạnh khác nhau của \"chơi tốt\" — cộng dồn\n"
                     "thành MỘT điểm fitness, với trọng số cố ý bất đối xứng",
             fontsize=7.8, ha="center", color="#666666", style="italic")

    bar_x, bar_w = 3.6, 1.9
    bar_bottom = 0.55
    seg_h = [4.55, 1.55, 0.85]   # ti le truc quan ~ "nen ap dao" vs "phan dinh tinh"
    seg_colors = [C_GREEN, C_BLUE, C_ORANGE]
    seg_labels = [
        "① Kết quả trận đấu\nnền 300 / 100 / 10\n(vẫn áp đảo biên độ điểm)",
        "② Biên độ HP tức thời\nhpA×8 − hpB×4\n(\"tự bảo tồn\" — giữ mạng × 2)",
        "③–⑥ Sức mạnh bàn cờ +\nchất lượng đội hình (nửa sau trận)\ntrọng số nhỏ 0.06/0.04/0.035/0.025\ntuyệt đối + tương đối với đối thủ",
    ]
    y = bar_bottom
    for h, color, lbl in zip(seg_h, seg_colors, seg_labels):
        axR.add_patch(plt.Rectangle((bar_x - bar_w / 2, y), bar_w, h, fc=tint(color, 0.55),
                                    ec=color, lw=1.6, zorder=3))
        axR.text(bar_x, y + h / 2, lbl, fontsize=6.7, ha="center", va="center",
                 color="#333333")
        y += h
    axR.text(bar_x, y + 0.32, "= điểm fitness\ncộng dồn", fontsize=8.0, ha="center",
             va="bottom", color="#333333", fontweight="bold")
    axR.text(bar_x, bar_bottom - 0.32, "ScoreFromA(...)", fontsize=7.6, ha="center",
             va="top", color="#777777", style="italic")

    # mui ten "+" giua cac doan (minh hoa cong don)
    for yy in (bar_bottom + seg_h[0], bar_bottom + seg_h[0] + seg_h[1]):
        axR.text(bar_x + bar_w / 2 + 0.32, yy, "+", fontsize=13, ha="left", va="center",
                 color="#999999", fontweight="bold")

    axR.text(7.85, bar_bottom + sum(seg_h) / 2,
             "phân phối điểm liên tục —\nkhông còn chỉ ba giá trị rời rạc;\n"
             "phân biệt được \"thắng đẹp\"\nvà \"thắng may\"",
             fontsize=7.6, ha="left", va="center", color="#555555", style="italic")

    draw_arrow(axR, bar_x, bar_bottom - 0.85, bar_x, -0.05, color=C_GREEN, lw=2.0)
    draw_box(axR, bar_x + 0.55, -0.65, 8.9, 0.95,
             "Áp lực chọn lọc đa chiều hơn — phân biệt được CÁCH một bot\n"
             "đi đến chiến thắng, chứ không chỉ CÓ thắng được hay không",
             fc=tint(C_GREEN, 0.85), ec=C_GREEN, fs=8.0, bold=True, color=C_GREEN)

    fig.suptitle("Hình 5.5 — Sự Mở Rộng Của Hàm Fitness Qua Các Vòng Tinh Chỉnh",
                 fontsize=13, fontweight="bold")
    fig.tight_layout(rect=[0, 0, 1, 0.96])
    save(fig, "hinh_5_5.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.6 - Duong cong fitness qua 180 the he (du lieu CSV that)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_6():
    d = load_training_csv()
    gen = d["gen"]

    fig, ax1 = plt.subplots(figsize=(12.6, 6.6))
    fig.patch.set_facecolor(BG)
    ax1.set_facecolor(BG)

    ax1.plot(gen, d["worst"], color=C_GRAY, lw=1.1, alpha=0.75, label="Worst", zorder=2)
    ax1.plot(gen, d["avg"], color=C_BLUE, lw=1.8, label="Avg", zorder=3)
    ax1.plot(gen, d["best"], color="#B71C1C", lw=2.4, label="Best", zorder=4)

    ax1.axvline(6, color="#B71C1C", lw=1.1, ls="--", alpha=0.7, zorder=1)
    ax1.annotate("Gen 6: Best đạt đỉnh 4764\n(Hall of Fame xác định rất sớm)",
                 xy=(6, 4764), xytext=(34, 4530),
                 fontsize=8.0, color="#B71C1C",
                 arrowprops=dict(arrowstyle="->", color="#B71C1C", lw=1.1))

    axvspan_lo, axvspan_hi = 18, 22
    ax1.axvspan(axvspan_lo, axvspan_hi, color=C_OLIVE, alpha=0.16, zorder=1)
    ax1.annotate("vùng pct_other tăng vọt\n(gen 19→20: 43%→48%)",
                 xy=(20, 2400), xytext=(46, 1500),
                 fontsize=8.0, color="#7C8A00",
                 arrowprops=dict(arrowstyle="->", color=C_OLIVE, lw=1.1))

    ax1.set_xlabel("Thế hệ (generation)", fontsize=10)
    ax1.set_ylabel("Điểm fitness", fontsize=10)
    ax1.set_xlim(0, 179)
    ax1.set_ylim(0, 5300)
    ax1.spines[["top"]].set_visible(False)
    ax1.grid(axis="y", color=C_GRID, lw=0.6, zorder=0)
    ax1.set_axisbelow(True)

    ax2 = ax1.twinx()
    ax2.plot(gen, d["std"], color="#F0A030", lw=1.6, ls="-.", label="Std Dev (trục phải)", zorder=3)
    ax2.set_ylabel("Std Dev", fontsize=10, color="#C8860D")
    ax2.set_ylim(0, 1100)
    ax2.tick_params(axis="y", labelcolor="#C8860D")
    ax2.spines[["top"]].set_visible(False)

    h1, l1 = ax1.get_legend_handles_labels()
    h2, l2 = ax2.get_legend_handles_labels()
    ax1.legend(h1 + h2, l1 + l2, loc="lower right", fontsize=8.6, frameon=False, ncol=2)

    ax1.set_title("Hình 5.6 — Đường Cong Fitness Qua 180 Thế Hệ (Production Run)",
                  fontsize=12.5, pad=12, fontweight="bold")
    fig.text(0.5, -0.01,
             "Nguồn dữ liệu thực: Document/02_Data/Train/training_20260601_213435.csv "
             "(population=120 · generations=180 · matches/chrom=20)",
             fontsize=7.6, color="#777777", ha="center", style="italic")
    fig.tight_layout()
    save(fig, "hinh_5_6.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.7 - Phan phoi tribe qua cac the he (area chart xep chong, du lieu CSV that)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_7():
    d = load_training_csv()
    gen = d["gen"]

    fig, ax = plt.subplots(figsize=(12.6, 6.0))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    colors = ["#F0A030", C_BLUE, "#BBBBBB"]
    labels = ["Babylon", "Niles", "Other (generalist / Olympus / mixed)"]
    ax.stackplot(gen, d["pct_b"], d["pct_n"], d["pct_o"],
                 colors=colors, alpha=0.85, edgecolor="white", lw=0.4, labels=labels, zorder=2)

    ax.set_xlabel("Thế hệ (generation)", fontsize=10)
    ax.set_ylabel("Tỉ lệ quần thể (%)", fontsize=10)
    ax.set_xlim(0, 179)
    ax.set_ylim(0, 119)
    ax.set_yticks([0, 20, 40, 60, 80, 100])
    ax.legend(loc="upper center", bbox_to_anchor=(0.5, -0.16), ncol=3, fontsize=9, frameon=False)
    ax.spines[["top", "right"]].set_visible(False)

    ax.text(0.012, 0.985,
            "Babylon: 6.7%–53.3% (TB ~30%)  ·  Niles: 22.5%–70.0% (TB ~43%)  ·  "
            "Other: 3.3%–68.3%\nKhông bộ tộc nào tuyệt chủng — elitism + immigration giữ tối thiểu cho cả ba",
            transform=ax.transAxes, fontsize=7.8, color="#444444", ha="left", va="top")

    ax.set_title("Hình 5.7 — Phân Phối Bộ Tộc (Tribe) Qua 180 Thế Hệ",
                 fontsize=12.5, pad=12, fontweight="bold")
    fig.text(0.5, 0.005,
             "Nguồn dữ liệu thực: training_20260601_213435.csv — cột pct_babylon / pct_niles / pct_other",
             fontsize=7.6, color="#777777", ha="center", style="italic")
    fig.tight_layout(rect=[0, 0.02, 1, 1])
    save(fig, "hinh_5_7.png")


# ══════════════════════════════════════════════════════════════════════════════
# HINH 5.8 - Radar chart 8 truc cho 5 bot (gene that, rut gon tu 9 nhom)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_5_8():
    labels = [a for a, _ in RADAR_AXES]
    n = len(labels)
    angles = np.linspace(0, 2 * np.pi, n, endpoint=False).tolist()
    angles += angles[:1]

    fig, ax = plt.subplots(figsize=(9.2, 9.6), subplot_kw=dict(polar=True))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    for i, bot in enumerate(BOT_ORDER):
        vals = [float(np.mean([BOT_GENES[bot][gi] for gi in idxs])) for _, idxs in RADAR_AXES]
        vals += vals[:1]
        ax.plot(angles, vals, color=BOT_COLORS[i], lw=2.1,
                label=BOT_LABELS[i].split("\n")[0], zorder=3)
        ax.fill(angles, vals, color=BOT_COLORS[i], alpha=0.07, zorder=2)

    ax.set_theta_offset(np.pi / 2)
    ax.set_theta_direction(-1)
    ax.set_xticks(angles[:-1])
    ax.set_xticklabels(labels, fontsize=10.4)
    ax.set_ylim(0, 1.0)
    ax.set_yticks([0.25, 0.5, 0.75, 1.0])
    ax.set_yticklabels(["0.25", "0.50", "0.75", "1.00"], fontsize=7.2, color="#888888")
    ax.grid(color=C_GRID, lw=0.7)
    ax.spines["polar"].set_color(C_GRID)

    ax.legend(loc="upper right", bbox_to_anchor=(1.34, 1.12), fontsize=9, frameon=False)
    ax.set_title("Hình 5.8 — Radar Chart: Hồ Sơ Gene 8-Trục Của 5 Bot Được Chọn",
                 fontsize=12.5, pad=28, fontweight="bold")
    fig.text(0.5, 0.015,
             "Mỗi trục = trung bình các gene cùng nhóm chức năng (rút gọn từ 9 nhóm — "
             "nhóm 9 \"Trigger con\" gộp vào \"Trigger\"). Nguồn: AI_Library.json @ commit 6e1c9f6",
             fontsize=7.4, color="#777777", ha="center", style="italic")
    fig.tight_layout(rect=[0, 0.03, 1, 1])
    save(fig, "hinh_5_8.png")


# ══════════════════════════════════════════════════════════════════════════════
if __name__ == "__main__":
    hinh_5_1()
    hinh_5_2()
    hinh_5_3()
    hinh_5_4()
    hinh_5_5()
    hinh_5_6()
    hinh_5_7()
    hinh_5_8()
    print("\n[DONE] 8 hinh Chuong 5 da duoc luu vao:", OUT)
