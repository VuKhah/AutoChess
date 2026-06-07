"""
Script tạo Hình 2.4, 2.5, 2.6 cho tiểu luận chuyên ngành Auto Chess.
Output: Document/03_Figures/hinh_2_4.png, hinh_2_5.png, hinh_2_6.png
"""

import json
import os
import numpy as np
import pandas as pd
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyArrowPatch

matplotlib.rcParams["font.family"] = "DejaVu Sans"
matplotlib.rcParams["axes.unicode_minus"] = False

# ─── Đường dẫn ────────────────────────────────────────────────────────────────
BASE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(BASE, "..", ".."))

TRAIN_CSV  = os.path.join(ROOT, "Document", "02_Data", "Train",
                          "training_20260604_214338.csv")
AI_JSON    = os.path.join(ROOT, "Assets", "Resources", "AI_Library.json")
OUT_DIR    = os.path.join(ROOT, "Document", "03_Figures")
os.makedirs(OUT_DIR, exist_ok=True)

# ─── Màu chủ đạo ──────────────────────────────────────────────────────────────
C_BEST  = "#E04040"   # đỏ
C_AVG   = "#F07020"   # cam
C_STD   = "#4080D0"   # xanh lam
C_GRID  = "#DDDDDD"
BG      = "#FAFAFA"

BOT_COLORS = {
    "hardBot":      "#E04040",
    "babylonBot":   "#F0A030",
    "nileBot":      "#3090D0",
    "summonerBot":  "#8040C0",
    "resilientBot": "#40A860",
}
BOT_LABELS = {
    "hardBot":      "hardBot",
    "babylonBot":   "babylonBot",
    "nileBot":      "nileBot",
    "summonerBot":  "summonerBot",
    "resilientBot": "resilientBot",
}

# ══════════════════════════════════════════════════════════════════════════════
# HÌNH 2.4 — Elitism và Fitness Không Giảm
# ══════════════════════════════════════════════════════════════════════════════
def plot_hinh_2_4(df):
    fig, ax = plt.subplots(figsize=(9, 4.5), facecolor=BG)
    ax.set_facecolor(BG)

    gen  = df["gen"].values
    best = df["best"].values

    # Vùng tô dưới đường best
    ax.fill_between(gen, best, alpha=0.12, color=C_BEST)
    ax.plot(gen, best, color=C_BEST, linewidth=2.2, label="Best Fitness")

    # Chú thích tính monotone
    ax.annotate(
        "Không bao giờ giảm\n(nhờ Elitism)",
        xy=(gen[6], best[6]),
        xytext=(gen[6] + 18, best[6] - 2200),
        arrowprops=dict(arrowstyle="->", color="#888", lw=1.2),
        fontsize=9, color="#555",
    )

    ax.set_xlabel("Thế hệ", fontsize=11)
    ax.set_ylabel("Best Fitness", fontsize=11)
    ax.set_title("Hình 2.4 — Elitism và Fitness Không Giảm", fontsize=13, pad=10)
    ax.set_xlim(gen[0], gen[-1])
    ax.yaxis.set_major_formatter(matplotlib.ticker.FuncFormatter(
        lambda x, _: f"{int(x):,}"))
    ax.grid(axis="y", color=C_GRID, linewidth=0.8)
    ax.spines[["top", "right"]].set_visible(False)
    ax.legend(fontsize=10)

    fig.tight_layout()
    out = os.path.join(OUT_DIR, "hinh_2_4.png")
    fig.savefig(out, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print(f"[OK] {out}")


# ══════════════════════════════════════════════════════════════════════════════
# HÌNH 2.5 — Premature Convergence vs. Healthy Evolution
# ══════════════════════════════════════════════════════════════════════════════
def plot_hinh_2_5(df):
    fig, ax1 = plt.subplots(figsize=(10, 5), facecolor=BG)
    ax1.set_facecolor(BG)
    ax2 = ax1.twinx()

    gen    = df["gen"].values
    best   = df["best"].values
    avg    = df["avg"].values
    stddev = df["std_dev"].values

    # Trục trái: best & avg
    l1, = ax1.plot(gen, best,   color=C_BEST, lw=2.2, label="Best Fitness")
    l2, = ax1.plot(gen, avg,    color=C_AVG,  lw=1.8, label="Avg Fitness",  linestyle="--")

    # Trục phải: std_dev
    l3, = ax2.plot(gen, stddev, color=C_STD,  lw=1.6, label="Std Dev",      linestyle=":")
    ax2.fill_between(gen, stddev, alpha=0.08, color=C_STD)

    # Vùng "Healthy Evolution" (std_dev vẫn còn cao → đa dạng)
    healthy_end = int(len(gen) * 0.25)
    ax1.axvspan(0, gen[healthy_end], alpha=0.06, color="green",
                label="Healthy Diversity Zone")

    ax1.set_xlabel("Thế hệ", fontsize=11)
    ax1.set_ylabel("Fitness", fontsize=11, color="#444")
    ax2.set_ylabel("Std Dev (Độ đa dạng)", fontsize=11, color=C_STD)
    ax2.tick_params(axis="y", colors=C_STD)

    ax1.set_title("Hình 2.5 — Premature Convergence vs. Healthy Evolution",
                  fontsize=13, pad=10)
    ax1.set_xlim(gen[0], gen[-1])
    ax1.yaxis.set_major_formatter(matplotlib.ticker.FuncFormatter(
        lambda x, _: f"{int(x):,}"))
    ax1.grid(axis="y", color=C_GRID, linewidth=0.8)
    ax1.spines[["top"]].set_visible(False)
    ax2.spines[["top"]].set_visible(False)

    lines = [l1, l2, l3]
    labels = [l.get_label() for l in lines]
    ax1.legend(lines, labels, fontsize=10, loc="lower right")

    fig.tight_layout()
    out = os.path.join(OUT_DIR, "hinh_2_5.png")
    fig.savefig(out, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print(f"[OK] {out}")


# ══════════════════════════════════════════════════════════════════════════════
# HÌNH 2.6 — Radar Chart So Sánh 5 Bot Archetype
# ══════════════════════════════════════════════════════════════════════════════

# 9 nhóm gene và các chỉ số gene thuộc mỗi nhóm
GENE_GROUPS = {
    "Stat\n(0–3)":       [0, 1, 2, 3],
    "Keywords\n(4–6)":   [4, 5, 6],
    "Trigger\n(7–12)":   [7, 8, 9, 10, 11, 12],
    "Effect\n(13–17)":   [13, 14, 15, 16, 17],
    "Tribe\n(18–20)":    [18, 19, 20],
    "Board\n(21–23)":    [21, 22, 23],
    "Reroll\n(24–27)":   [24, 25, 26, 27],
    "Spell\n(28–31)":    [28, 29, 30, 31],
    "Aura/Ally\n(32–36)":[32, 33, 34, 35, 36],
}

def bot_group_means(genes):
    return [np.mean([genes[i] for i in idxs])
            for idxs in GENE_GROUPS.values()]

def plot_hinh_2_6(ai_data):
    labels = list(GENE_GROUPS.keys())
    N = len(labels)
    angles = np.linspace(0, 2 * np.pi, N, endpoint=False).tolist()
    angles += angles[:1]   # đóng vòng

    fig, ax = plt.subplots(figsize=(7, 7),
                           subplot_kw=dict(polar=True),
                           facecolor=BG)
    ax.set_facecolor(BG)

    for bot_key, data in ai_data.items():
        vals = bot_group_means(data["genes"])
        vals += vals[:1]
        color = BOT_COLORS[bot_key]
        ax.plot(angles, vals, color=color, linewidth=2, label=BOT_LABELS[bot_key])
        ax.fill(angles, vals, color=color, alpha=0.10)

    # Ticks & labels
    ax.set_xticks(angles[:-1])
    ax.set_xticklabels(labels, fontsize=9)
    ax.set_yticks([0.2, 0.4, 0.6, 0.8, 1.0])
    ax.set_yticklabels(["0.2", "0.4", "0.6", "0.8", "1.0"], fontsize=7, color="#999")
    ax.set_ylim(0, 1)
    ax.grid(color=C_GRID, linewidth=0.9)
    ax.spines["polar"].set_visible(False)

    ax.set_title("Hình 2.6 — Radar Chart So Sánh 5 Bot Archetype",
                 fontsize=13, pad=22)
    ax.legend(loc="upper right", bbox_to_anchor=(1.32, 1.12), fontsize=10)

    fig.tight_layout()
    out = os.path.join(OUT_DIR, "hinh_2_6.png")
    fig.savefig(out, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print(f"[OK] {out}")


# ─── Main ─────────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    df = pd.read_csv(TRAIN_CSV)
    with open(AI_JSON, encoding="utf-8") as f:
        ai_data = json.load(f)

    plot_hinh_2_4(df)
    plot_hinh_2_5(df)
    plot_hinh_2_6(ai_data)

    print(f"\nĐã lưu 3 hình vào: {OUT_DIR}")
