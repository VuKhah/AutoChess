"""
Fix Hinh 3.10 - Mot Chuoi Phan Ung Day Chuyen Dien Hinh.

Ban cu (trong generate_figures_ch3.py) ve 6 hop xep thanh MOT hang ngang,
figsize=(12.5, 3.6) - ty le rat det (~3.5:1). Khi chen vao trang Word (rong
~6.3in), anh bi thu nho ~2 lan, khien chu (fontsize 7.3) chi con ~3.6pt -
khong doc duoc.

Fix: xep lai thanh LUOI 2 hang x 3 cot (kieu "ran luon") - anh vuong vuc hon
(~9.5 x 6.4), it bi thu nho hon khi chen vao doc (~0.66 lan thay vi ~0.50
lan), dong thoi tang fontsize hop/nhan/tieu de len gan gap doi. Hieu ung:
chu hien thi trong doc ~8-9pt thay vi ~3.6pt.

Chay:   python fix_hinh_3_10.py
Ket qua: ghi de Document/03_Figures/hinh_3_10.png
"""

import os
import matplotlib
import matplotlib.pyplot as plt
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch

matplotlib.rcParams["font.family"] = "DejaVu Sans"
matplotlib.rcParams["axes.unicode_minus"] = False

BASE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(BASE, "..", ".."))
OUT  = os.path.join(ROOT, "Document", "03_Figures")

BG     = "#FAFAFA"
C_PINK = "#D81B60"
FC_PINK = "#FBE3F0"


def draw_box(ax, cx, cy, w, h, text, fc, ec, fs, bold=False, radius=0.06):
    rect = FancyBboxPatch((cx - w / 2, cy - h / 2), w, h,
                          boxstyle=f"round,pad={radius}",
                          fc=fc, ec=ec, lw=1.8, zorder=3)
    ax.add_patch(rect)
    ax.text(cx, cy, text, ha="center", va="center", fontsize=fs,
            fontweight="bold" if bold else "normal", zorder=4)


def draw_arrow(ax, x0, y0, x1, y1, color, lw=2.0):
    arrow = FancyArrowPatch((x0, y0), (x1, y1), arrowstyle="-|>", color=color,
                            lw=lw, mutation_scale=18, zorder=2)
    ax.add_patch(arrow)


def hinh_3_10():
    steps = [
        "Unit A\ngục ngã",
        "Kỹ năng \"khi đồng minh\nchết\" của Anubis\n→ ban Reborn cho B",
        "Unit B (đồng minh\nyếu nhất) nhận\nhiệu ứng Reborn",
        "B gục ngã →\nhồi sinh ngay\nvới 1 HP",
        "Kỹ năng \"khi đồng minh\nhồi sinh\" của Osiris\n→ nhân bội chỉ số B",
        "Chuỗi khép lại,\ntrận đấu tiếp tục",
    ]

    fig, ax = plt.subplots(figsize=(9.5, 6.4))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    box_w, box_h = 2.85, 1.85
    col_x = [1.85, 5.0, 8.15]
    row_y = [4.6, 1.6]
    positions = [(col_x[0], row_y[0]), (col_x[1], row_y[0]), (col_x[2], row_y[0]),
                 (col_x[0], row_y[1]), (col_x[1], row_y[1]), (col_x[2], row_y[1])]

    for i, ((x, y), text) in enumerate(zip(positions, steps)):
        draw_box(ax, x, y, box_w, box_h, text, fc=FC_PINK, ec=C_PINK, fs=13)
        ax.text(x, y + box_h / 2 + 0.32, f"Bước {i + 1}", ha="center", fontsize=14,
                color=C_PINK, fontweight="bold")

    # Hang 1: Buoc 1 -> 2 -> 3
    for i in range(2):
        x0 = positions[i][0] + box_w / 2 + 0.05
        x1 = positions[i + 1][0] - box_w / 2 - 0.05
        draw_arrow(ax, x0, row_y[0], x1, row_y[0], color=C_PINK)

    # Buoc 3 -> 4: duong gap khuc xuong - sang trai - xuong (kieu "ran luon"),
    # di qua khoang trong giua 2 hang de khong de len Hop 2/Hop 5. Diem rot
    # vao Hop 4 lech sang phai tam hop de khong de len nhan "Buoc 4".
    mid_y = ((row_y[0] - box_h / 2) + (row_y[1] + box_h / 2)) / 2
    x3, y3 = positions[2][0], row_y[0] - box_h / 2 - 0.05
    x4, y4 = positions[3][0] + 0.7, row_y[1] + box_h / 2 + 0.05
    ax.plot([x3, x3], [y3, mid_y], color=C_PINK, lw=2.0, solid_capstyle="round", zorder=2)
    ax.plot([x3, x4], [mid_y, mid_y], color=C_PINK, lw=2.0, solid_capstyle="round", zorder=2)
    draw_arrow(ax, x4, mid_y, x4, y4, color=C_PINK)

    # Hang 2: Buoc 4 -> 5 -> 6
    for i in range(3, 5):
        x0 = positions[i][0] + box_w / 2 + 0.05
        x1 = positions[i + 1][0] - box_w / 2 - 0.05
        draw_arrow(ax, x0, row_y[1], x1, row_y[1], color=C_PINK)

    ax.text((col_x[0] + col_x[2]) / 2, row_y[1] - box_h / 2 - 0.45,
            "chuỗi khép lại — trận đấu tiếp tục như bình thường\n"
            "Anubis, Osiris: ví dụ minh họa cho hai loại kỹ năng phản ứng (chi tiết: Phụ lục A)",
            ha="center", va="top", fontsize=11, color="#777777", style="italic")

    ax.set_xlim(0, 10)
    ax.set_ylim(-1.0, 6.5)
    ax.axis("off")
    ax.set_title("Hình 3.10 — Một Chuỗi Phản Ứng Dây Chuyền Điển Hình",
                 fontsize=16, pad=12, fontweight="bold")
    fig.tight_layout()

    path = os.path.join(OUT, "hinh_3_10.png")
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


if __name__ == "__main__":
    hinh_3_10()
