"""
Fix Hinh 3.11 - Chuoi Phan Ung Khi Mot Unit Reborn Guc Nga.

Cung van de nhu Hinh 3.10: ban cu xep 6 hop thanh MOT hang ngang,
figsize=(13.5, 3.6) - rat det (~3.75:1), khi chen vao trang Word chu
(fontsize 7.3) bi thu nho con ~3.5pt - khong doc duoc. Ghi chu thiet ke
o cuoi cung qua nho (fontsize 7.6) va dat o vi tri co the bi cat.

Fix: ap dung dung bo cuc luoi 2 hang x 3 cot "ran luon" da dung cho Hinh
3.10 (cung mau xanh la - Niles), tang fontsize hop/nhan/tieu de gan gap
doi, va dua dong ghi chu thiet ke ve ngay duoi cung, day du 2 dong, fontsize
lon hon.

Chay:   python fix_hinh_3_11.py
Ket qua: ghi de Document/03_Figures/hinh_3_11.png
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

BG      = "#FAFAFA"
C_GREEN = "#2E7D32"
FC_GREEN = "#E5F2E8"


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


def hinh_3_11():
    steps = [
        "HP về 0\n(unit mang\nReborn)",
        "Kỹ năng \"khi\nchết\" kích hoạt\n(Horus)",
        "Hồi sinh ngay\nvới đúng\n1 HP",
        "Kỹ năng \"khi\nhồi sinh\" kích\nhoạt (Osiris)",
        "Kỹ năng \"khi có\nđồng minh mới\"\nlan tỏa (Sobek)",
        "Unit trở lại\nhàng đợi —\nchuỗi khép lại",
    ]

    fig, ax = plt.subplots(figsize=(9.5, 7.0))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    box_w, box_h = 2.85, 1.85
    col_x = [1.85, 5.0, 8.15]
    row_y = [4.6, 1.6]
    positions = [(col_x[0], row_y[0]), (col_x[1], row_y[0]), (col_x[2], row_y[0]),
                 (col_x[0], row_y[1]), (col_x[1], row_y[1]), (col_x[2], row_y[1])]

    for i, ((x, y), text) in enumerate(zip(positions, steps)):
        draw_box(ax, x, y, box_w, box_h, text, fc=FC_GREEN, ec=C_GREEN, fs=13)
        ax.text(x, y + box_h / 2 + 0.32, f"Bước {i + 1}", ha="center", fontsize=14,
                color=C_GREEN, fontweight="bold")

    # Hang 1: Buoc 1 -> 2 -> 3
    for i in range(2):
        x0 = positions[i][0] + box_w / 2 + 0.05
        x1 = positions[i + 1][0] - box_w / 2 - 0.05
        draw_arrow(ax, x0, row_y[0], x1, row_y[0], color=C_GREEN)

    # Buoc 3 -> 4: duong gap khuc xuong - sang trai - xuong (kieu "ran luon"),
    # diem rot vao Hop 4 lech sang phai tam hop de khong de len nhan "Buoc 4"
    mid_y = ((row_y[0] - box_h / 2) + (row_y[1] + box_h / 2)) / 2
    x3, y3 = positions[2][0], row_y[0] - box_h / 2 - 0.05
    x4, y4 = positions[3][0] + 0.7, row_y[1] + box_h / 2 + 0.05
    ax.plot([x3, x3], [y3, mid_y], color=C_GREEN, lw=2.0, solid_capstyle="round", zorder=2)
    ax.plot([x3, x4], [mid_y, mid_y], color=C_GREEN, lw=2.0, solid_capstyle="round", zorder=2)
    draw_arrow(ax, x4, mid_y, x4, y4, color=C_GREEN)

    # Hang 2: Buoc 4 -> 5 -> 6
    for i in range(3, 5):
        x0 = positions[i][0] + box_w / 2 + 0.05
        x1 = positions[i + 1][0] - box_w / 2 - 0.05
        draw_arrow(ax, x0, row_y[1], x1, row_y[1], color=C_GREEN)

    ax.text((col_x[0] + col_x[2]) / 2, row_y[1] - box_h / 2 - 0.45,
            "chuỗi khép lại — trận đấu tiếp tục như bình thường",
            ha="center", va="top", fontsize=11, color="#777777", style="italic")

    # Ghi chu thiet ke - day du, 2 dong, dat ngay duoi cung
    ax.text(5.0, row_y[1] - box_h / 2 - 1.05,
            "Lưu ý thiết kế: unit vẫn được tính là \"đã chết hợp lệ\" và kích hoạt trọn vẹn kỹ năng \"khi chết\"\n"
            "TRƯỚC khi hồi sinh — Horus + Reborn nghĩa là phần thưởng kích hoạt hai lần chỉ từ một unit.",
            ha="center", va="top", fontsize=10.5, color="#555555", style="italic")

    ax.set_xlim(0, 10)
    ax.set_ylim(-1.7, 6.5)
    ax.axis("off")
    ax.set_title("Hình 3.11 — Chuỗi Phản Ứng Khi Một Unit Reborn Gục Ngã",
                 fontsize=16, pad=12, fontweight="bold")
    fig.tight_layout()

    path = os.path.join(OUT, "hinh_3_11.png")
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


if __name__ == "__main__":
    hinh_3_11()
