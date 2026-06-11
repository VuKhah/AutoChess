"""
Fix Hinh 4.2 - Quy Trinh Rut Ngan Xep Cai Chet (Death Stack).

Ban cu (generate_figures_ch4.py) xep 10 khoi thanh MOT cot doc,
figsize=(9.6, 13.8) - rat cao (ty le ~0.7:1). Khi chen vao trang Word
(rong ~6.3in), chieu cao quy doi ~9in - VUOT QUA mot trang giay; fontsize
8.4-8.8 chi con ~5.5pt.

Fix: bo tri lai theo kieu "chu U" 2 cot x 5 hang (boustrophedon):
  - Cot trai (tren -> duoi): Bat dau, B1-B4 (vong lap xu ly tung cai chet)
  - Noi tiep ngang o hang day (B4 -> "Ngan xep con phan tu?")
  - Cot phai (duoi -> tren): Quyet dinh 1, B5, B6, Quyet dinh 2, Ket thuc
  - Vong lap trong (CON) di qua khe giua 2 cot, quay ve B1
  - Vong lap ngoai (CO) di vong qua dinh, quay ve "Bat dau"

Ket qua: anh gan vuong (~1.05:1), fontsize tang gan gap doi (12-13.5),
chieu cao quy doi khi chen vao doc con ~6in - vua mot trang.

Chay:   python fix_hinh_4_2.py
Ket qua: ghi de Document/03_Figures/hinh_4_2.png
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

BG       = "#FAFAFA"
C_RED    = "#E04040"
C_ORANGE = "#F0A030"
C_GREEN  = "#2E7D32"
C_PURPLE = "#8040C0"
C_GRAY   = "#888888"
C_BLUE   = "#1565C0"


def draw_box(ax, cx, cy, w, h, text, fc, ec, fs, bold=False):
    rect = FancyBboxPatch((cx - w / 2, cy - h / 2), w, h,
                          boxstyle="round,pad=0.05", fc=fc, ec=ec, lw=1.6, zorder=3)
    ax.add_patch(rect)
    ax.text(cx, cy, text, ha="center", va="center", fontsize=fs,
            fontweight="bold" if bold else "normal", zorder=4, linespacing=1.4)


def draw_arrow(ax, x0, y0, x1, y1, color=C_GRAY, lw=1.8):
    arrow = FancyArrowPatch((x0, y0), (x1, y1), arrowstyle="-|>", color=color,
                            lw=lw, mutation_scale=15, zorder=2)
    ax.add_patch(arrow)


def hinh_4_2():
    fig, ax = plt.subplots(figsize=(10.0, 9.5))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cx_L, cx_R = 2.6, 8.6
    bw, h = 5.0, 1.5
    row_y = [7.8, 5.85, 3.9, 1.95, 0.0]  # tren -> duoi

    # ─── Cot trai: Bat dau -> B1 -> B2 -> B3 -> B4 ──────────────────────────
    draw_box(ax, cx_L, row_y[0], bw, h,
             "Lượt giao tranh vừa kết thúc\nDeath Stack có ≥ 1 phần tử",
             fc="#ECECEC", ec=C_GRAY, fs=12.5, bold=True)
    draw_box(ax, cx_L, row_y[1], bw, h,
             "①  Lấy cái chết gần nhất\n(pop khỏi Death Stack — LIFO)",
             fc="#FBE3E3", ec=C_RED, fs=12.5)
    draw_box(ax, cx_L, row_y[2], bw, h,
             "②  Kích hoạt kỹ năng \"khi chết\"\ncủa chính unit vừa lấy ra",
             fc="#FBE3E3", ec=C_RED, fs=12.5)
    draw_box(ax, cx_L, row_y[3], bw, h,
             "③  Quét lại sân\n→ phát hiện cái chết mới, đẩy vào stack",
             fc="#FBE3E3", ec=C_RED, fs=12.5)
    draw_box(ax, cx_L, row_y[4], bw, h,
             "④  Phát sự kiện \"khi đồng minh chết\"\ncho từng đồng minh còn sống\n"
             "(đúng thứ tự vị trí — mục 4.3.1)",
             fc="#FBE3E3", ec=C_RED, fs=12)

    for a, b in zip(row_y[:-1], row_y[1:]):
        draw_arrow(ax, cx_L, a - h / 2 - 0.05, cx_L, b + h / 2 + 0.05, color=C_RED, lw=2.0)

    # ─── Cot phai: Q.dinh1 -> B5 -> B6 -> Q.dinh2 -> Ket thuc (duoi -> tren) ─
    draw_box(ax, cx_R, row_y[4], bw, h, "Ngăn xếp\ncòn phần tử?",
             fc="#FCEFD0", ec=C_ORANGE, fs=13, bold=True)
    draw_box(ax, cx_R, row_y[3], bw, h,
             "⑤  Dọn sân — mỗi unit đã đánh dấu chết:\n"
             "•  còn lượt Reborn → hồi sinh tại chỗ\n"
             "•  hết lượt → gỡ khỏi sân vĩnh viễn",
             fc="#E3F2FD", ec=C_BLUE, fs=12)
    draw_box(ax, cx_R, row_y[2], bw, h,
             "⑥  Giải phóng ĐÚNG MỘT lượt\ntriệu hồi đang chờ (nếu có)",
             fc="#E3F2FD", ec=C_BLUE, fs=12.5)
    draw_box(ax, cx_R, row_y[1], bw, h,
             "Có biến động mới phát sinh?\n(cái chết / lượt triệu hồi mới)",
             fc="#FCEFD0", ec=C_ORANGE, fs=12.5, bold=True)
    draw_box(ax, cx_R, row_y[0], bw, h,
             "[Sân đã lắng]\nTrận đấu tiếp tục\nbình thường",
             fc="#E5F2E8", ec=C_GREEN, fs=12.5, bold=True)

    # B4(trai, hang day) -> Q.dinh1 (phai, hang day): noi ngang "ran luon"
    draw_arrow(ax, cx_L + bw / 2 + 0.05, row_y[4], cx_R - bw / 2 - 0.05, row_y[4],
               color=C_RED, lw=2.0)

    # Q.dinh1 -> B5 (TRONG, len tren trong cot phai)
    draw_arrow(ax, cx_R, row_y[4] + h / 2 + 0.05, cx_R, row_y[3] - h / 2 - 0.05,
               color=C_GRAY, lw=2.0)
    ax.text(cx_R + bw / 2 + 0.15, (row_y[4] + row_y[3]) / 2 + h / 2 - 0.45,
            "TRỐNG", fontsize=11, color="#555555", ha="left", style="italic")

    # B5 -> B6 -> Q.dinh2
    draw_arrow(ax, cx_R, row_y[3] + h / 2 + 0.05, cx_R, row_y[2] - h / 2 - 0.05,
               color=C_BLUE, lw=2.0)
    draw_arrow(ax, cx_R, row_y[2] + h / 2 + 0.05, cx_R, row_y[1] - h / 2 - 0.05,
               color=C_GRAY, lw=2.0)

    # Q.dinh2 -> Ket thuc (KHONG, len tren)
    draw_arrow(ax, cx_R, row_y[1] + h / 2 + 0.05, cx_R, row_y[0] - h / 2 - 0.05,
               color=C_GREEN, lw=2.0)
    ax.text(cx_R + bw / 2 + 0.15, (row_y[1] + row_y[0]) / 2 + h / 2 - 0.45,
            "KHÔNG", fontsize=11, color="#555555", ha="left", style="italic")

    # ─── Vong lap trong (CON): Q.dinh1 -> B1, di qua khe giua 2 cot ─────────
    gap_x = (cx_L + bw / 2 + cx_R - bw / 2) / 2
    y_exit = row_y[4] - 0.3
    ax.plot([cx_R - bw / 2, gap_x], [y_exit, y_exit], color=C_RED, lw=1.8,
            solid_capstyle="round", zorder=2)
    ax.plot([gap_x, gap_x], [y_exit, row_y[1]], color=C_RED, lw=1.8,
            solid_capstyle="round", zorder=2)
    draw_arrow(ax, gap_x, row_y[1], cx_L + bw / 2 + 0.05, row_y[1], color=C_RED, lw=1.8)
    ax.text(gap_x, (y_exit + row_y[1]) / 2,
            "CÒN →\nlặp lại\nbước ①", fontsize=9.5, color=C_RED, ha="center", va="center")

    # ─── Vong lap ngoai (CO): Q.dinh2 -> Bat dau, di vong qua dinh ──────────
    outer_x, top_y = cx_R + bw / 2 + 0.5, 9.15
    ax.plot([cx_R + bw / 2, outer_x], [row_y[1], row_y[1]], color=C_PURPLE, lw=1.8,
            solid_capstyle="round", zorder=2)
    ax.plot([outer_x, outer_x], [row_y[1], top_y], color=C_PURPLE, lw=1.8,
            solid_capstyle="round", zorder=2)
    ax.plot([outer_x, cx_L], [top_y, top_y], color=C_PURPLE, lw=1.8,
            solid_capstyle="round", zorder=2)
    draw_arrow(ax, cx_L, top_y, cx_L, row_y[0] + h / 2 + 0.05, color=C_PURPLE, lw=1.8)
    ax.text((outer_x + cx_L) / 2, top_y - 0.3,
            "CÓ → toàn bộ chu trình lặp lại từ đầu, cho đến khi lắng hẳn",
            fontsize=10.5, color=C_PURPLE, ha="center", va="top")

    # ─── Ghi chu cuoi ────────────────────────────────────────────────────────
    ax.text((cx_L + cx_R) / 2, row_y[4] - h / 2 - 0.55,
            "Bộ đếm an toàn: giới hạn 500 vòng lặp — một lưới bắt lỗi, không phải một phần của thiết kế",
            fontsize=10.5, color="#999999", ha="center", va="top", style="italic")

    ax.set_xlim(-0.3, outer_x + 0.4)
    ax.set_ylim(row_y[4] - h / 2 - 1.1, top_y + 0.5)
    ax.axis("off")
    ax.set_title("Hình 4.2 — Quy Trình Rút Ngăn Xếp Cái Chết (Death Stack)",
                 fontsize=16, pad=12, fontweight="bold")
    fig.tight_layout()

    path = os.path.join(OUT, "hinh_4_2.png")
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


if __name__ == "__main__":
    hinh_4_2()
