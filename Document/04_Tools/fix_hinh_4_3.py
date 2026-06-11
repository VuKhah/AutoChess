"""
Fix Hinh 4.3 - Pipeline Huan Luyen Khep Kin (so do ngang 6 buoc).

Ban cu (generate_figures_ch4.py) co figsize=(16, 6.2) - rat rong (~2.6:1).
Khi chen vao trang Word (rong ~6.3in), ty le thu nho chi con ~0.39 lan,
khien fontsize 8.0-8.2 chi con ~3.1-3.2pt - khong doc duoc.

Fix (chi tang chu, KHONG doi bo cuc 6-buoc-mot-hang theo yeu cau):
  - Giam figsize tu (16,6.2) xuong (11.5,4.3) -> ty le thu nho khi chen vao
    doc tang tu ~0.39 len ~0.55 lan.
  - Tang fontsize: noi dung hop 8.0->10.5, nhan khung 8.2->10.5,
    chu thich cuoi 7.6->10, tieu de 12.5->14.5.
  - Vie lai xuong dong cho cac dong chu dai (hop 1/2/3/6) de khop voi
    chieu rong hop moi; tang nhe chieu rong hop 5 va 6 (2.6->3.0, 2.8->3.1)
    de "(AI_Library.json)" va "BotAgent vao game that" khong bi tran.
  - Hieu ung: chu hien thi trong doc tang tu ~3.1pt len ~5.7pt
    (tieu de ~8pt).

Chay:   python fix_hinh_4_3.py
Ket qua: ghi de Document/03_Figures/hinh_4_3.png
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
C_GRAY   = "#888888"
C_ORANGE = "#F0A030"
C_GREEN  = "#2E7D32"


def draw_box(ax, cx, cy, w, h, text, fc, ec, fs, bold=False):
    rect = FancyBboxPatch((cx - w / 2, cy - h / 2), w, h,
                          boxstyle="round,pad=0.04", fc=fc, ec=ec, lw=1.5, zorder=3)
    ax.add_patch(rect)
    ax.text(cx, cy, text, ha="center", va="center", fontsize=fs,
            fontweight="bold" if bold else "normal", zorder=4, linespacing=1.35)


def draw_arrow(ax, x0, y0, x1, y1, color=C_GRAY, lw=1.5):
    arrow = FancyArrowPatch((x0, y0), (x1, y1), arrowstyle="-|>", color=color,
                            lw=lw, mutation_scale=13, zorder=2)
    ax.add_patch(arrow)


def hinh_4_3():
    fig, ax = plt.subplots(figsize=(11.5, 4.3))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    y = 3.0
    steps = [
        dict(cx=1.2,  w=2.4, h=1.7,
             text="①  Kịch bản\nkhởi động\n(command-line\nscript)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=4.1,  w=2.4, h=1.7,
             text="②  Unity chạy\nở chế độ\nkhông giao diện\n(headless)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=7.0,  w=2.4, h=1.7,
             text="③  Điểm vào\nhuấn luyện\n(training entry\npoint)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=10.2, w=3.0, h=1.95,
             text="④  GATrainer\nđiều khiển\nGameSimulator\n× 432.000 trận đấu",
             fc="#FCEFD0", ec=C_ORANGE, bold=True),
        dict(cx=13.7, w=3.0, h=1.7,
             text="⑤  File kết quả\nhuấn luyện\n(AI_Library.json)",
             fc="#ECECEC", ec=C_GRAY, bold=False),
        dict(cx=17.25, w=3.1, h=1.95,
             text="⑥  AIManager nạp 5\nBotAgent vào\ngame thật\n(runtime)",
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
            fontsize=10.5, color="#666666", ha="center", style="italic", fontweight="bold")
    ax.text(steps[5]["cx"], y + frame_h / 2 + 0.35, "PHẦN GAME RUNTIME",
            fontsize=10.5, color=C_GREEN, ha="center", style="italic", fontweight="bold")

    for i, s in enumerate(steps):
        draw_box(ax, s["cx"], y, s["w"], s["h"], s["text"],
                 fc=s["fc"], ec=s["ec"], fs=10.5, bold=s["bold"])
        if i < len(steps) - 1:
            nxt = steps[i + 1]
            draw_arrow(ax, s["cx"] + s["w"] / 2 + 0.06, y, nxt["cx"] - nxt["w"] / 2 - 0.06, y,
                       color=C_GRAY, lw=1.5)

    ax.text((frame_left + frame_right) / 2, y - frame_h / 2 - 0.55,
            "432.000 trận: tiến hóa 5 quần thể độc lập theo 5 triết lý chơi khác nhau,\n"
            "ghi lại các cá thể sống sót tốt nhất vào file kết quả",
            fontsize=10, color="#777777", ha="center", style="italic")

    ax.set_xlim(frame_left - 0.7, steps[5]["cx"] + steps[5]["w"] / 2 + 0.7)
    ax.set_ylim(y - frame_h / 2 - 1.5, y + frame_h / 2 + 0.95)
    ax.axis("off")
    ax.set_title("Hình 4.3 — Pipeline Huấn Luyện Khép Kín: Từ Mã Nguồn Đến Năm Chiến Binh",
                 fontsize=14.5, pad=10, fontweight="bold")
    fig.tight_layout()

    path = os.path.join(OUT, "hinh_4_3.png")
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


if __name__ == "__main__":
    hinh_4_3()
