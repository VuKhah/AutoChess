"""
Fix Hinh 4.1 - Hinh Dang Kien Truc Khi Ap Dung Ba Rang Buoc.

Ban cu (generate_figures_ch4.py) co figsize=(11.4, 8.8) - khi chen vao trang
Word (rong ~6.3in) bi thu nho ~0.55 lan, fontsize noi dung 8.4 chi con
~4.6pt, chu thich phu (7.4) chi con ~4.1pt - kho doc.

Fix:
  - Tach moi tang thanh "tieu de tang" (in dam, mau, fontsize lon) +
    "noi dung" (chu thuong, fontsize vua) thay vi gop chung mot khoi chu.
  - Tang toan bo fontsize len ~1.4-1.6 lan.
  - Dat 2 chu thich phu ("phu thuoc mot chieu", "ket noi thang") vao trong
    khung net dut nho - ro rang hon la chu in nghieng troi noi.
  - Thu gon khoang trong thua, giam figsize tong the (~9.5x8.25) de ty le
    thu nho khi chen vao doc nhe hon (~0.66 lan thay vi ~0.55 lan).

Chay:   python fix_hinh_4_1.py
Ket qua: ghi de Document/03_Figures/hinh_4_1.png
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
C_BLUE   = "#1565C0"
C_ORANGE = "#F0A030"
C_GREEN  = "#2E7D32"
C_GRAY   = "#888888"


def draw_box(ax, cx, cy, w, h, fc, ec, dashed=False, lw=1.8):
    style = "round,pad=0.05"
    rect = FancyBboxPatch((cx - w / 2, cy - h / 2), w, h,
                          boxstyle=style, fc=fc, ec=ec, lw=lw,
                          ls="--" if dashed else "-", zorder=3)
    ax.add_patch(rect)


def draw_arrow(ax, x0, y0, x1, y1, color=C_GRAY, lw=1.8):
    arrow = FancyArrowPatch((x0, y0), (x1, y1), arrowstyle="-|>", color=color,
                            lw=lw, mutation_scale=16, zorder=2)
    ax.add_patch(arrow)


def hinh_4_1():
    fig, ax = plt.subplots(figsize=(9.5, 8.25))
    fig.patch.set_facecolor(BG)
    ax.set_facecolor(BG)

    cx = 3.5
    w_layer = 6.2
    h_layer = 1.7

    layers = [
        dict(y=8.4, title="TẦNG GIAO DIỆN (UI)",
             content="CardView · BoardView · ShopUI ·\nphát lại TurnRecord bằng hoạt ảnh",
             fc="#FCEFD0", ec=C_ORANGE, n_content=2),
        dict(y=6.35, title="TẦNG QUẢN LÝ (Manager)",
             content="GameManager · AIManager — vòng đời Unity,\nđiều phối trạng thái ván đấu",
             fc="#FCEFD0", ec=C_ORANGE, n_content=2),
        dict(y=4.3, title="CORE ENGINE",
             content="CombatResolver · SkillEngine · GameSimulator\n"
                     "luật chơi thuần — tạo bằng new() được ở bất cứ đâu",
             fc="#E5F2E8", ec=C_GREEN, n_content=2),
        dict(y=2.25, title="TẦNG DỮ LIỆU (Data)",
             content="CardsData.json · Chromosome ·\ntham số cân bằng — data-driven, không hard-code",
             fc="#ECECEC", ec=C_GRAY, n_content=2),
    ]

    for L in layers:
        draw_box(ax, cx, L["y"], w_layer, h_layer, fc=L["fc"], ec=L["ec"])
        title_y = L["y"] + h_layer / 2 - 0.40
        ax.text(cx, title_y, L["title"], ha="center", va="center", fontsize=14.5,
                fontweight="bold", color=L["ec"], zorder=4)
        content_y = title_y - 0.34 - (L["n_content"] - 1) * 0.30
        ax.text(cx, content_y, L["content"], ha="center", va="center", fontsize=12,
                color="#333333", zorder=4, linespacing=1.5)

    # mui ten phu thuoc mot chieu, tu tren xuong duoi
    for a, b in zip(layers[:-1], layers[1:]):
        draw_arrow(ax, cx, a["y"] - h_layer / 2 - 0.05, cx, b["y"] + h_layer / 2 + 0.05,
                   color=C_GRAY, lw=2.0)

    # chu thich "phu thuoc mot chieu" - dat trong khung net dut ben trai
    note_l_cx, note_l_cy = -0.85, (layers[1]["y"] + layers[2]["y"]) / 2
    draw_box(ax, note_l_cx, note_l_cy, 1.85, 1.95, fc="#FFFFFF", ec="#AAAAAA", dashed=True, lw=1.4)
    ax.text(note_l_cx, note_l_cy,
            "Phụ thuộc\nmột chiều\n(trên → dưới)\n\nTầng dưới\nkhông biết\nđến tầng trên",
            fontsize=11, color="#555555", ha="center", va="center", style="italic",
            linespacing=1.35, zorder=4)

    # nhanh AI tach rieng — noi thang xuong Core Engine, bo qua Manager & UI
    ai_cx, ai_y = 7.7, 7.4
    draw_box(ax, ai_cx, ai_y, 2.9, 2.0, fc="#E5F2E8", ec=C_GREEN)
    ax.text(ai_cx, ai_y + 0.55, "TẦNG AI\n(đứng tách biệt)", ha="center", va="center",
            fontsize=14.5, fontweight="bold", color=C_GREEN, zorder=4, linespacing=1.3)
    ax.text(ai_cx, ai_y - 0.45, "GATrainer · BotAgent ·\nGameSimulator (headless)",
            ha="center", va="center", fontsize=12, color="#333333", zorder=4, linespacing=1.4)

    core_y = layers[2]["y"]
    core_right = cx + w_layer / 2
    line_x = ai_cx
    ai_bottom = ai_y - 1.0
    ax.plot([line_x, line_x], [ai_bottom, core_y], color=C_GREEN, lw=2.0,
            solid_capstyle="round", zorder=2)
    draw_arrow(ax, line_x, core_y, core_right + 0.06, core_y, color=C_GREEN, lw=2.0)

    # chu thich "ket noi thang" - khung net dut ben phai, duoi Core Engine
    # (dat thap hon de khong de len duong noi/mui ten ben tren)
    note_r_cx, note_r_cy = 8.0, 3.0
    draw_box(ax, note_r_cx, note_r_cy, 2.4, 1.7, fc="#FFFFFF", ec=C_GREEN, dashed=True, lw=1.4)
    ax.text(note_r_cx, note_r_cy,
            "kết nối thẳng — bỏ qua\nManager & UI hoàn toàn\n(hiện thân của ràng buộc\nheadless)",
            fontsize=11, color=C_GREEN, ha="center", va="center", style="italic",
            linespacing=1.35, zorder=4)

    # chu thich mau (legend) - hang ngang phia duoi Tang Du lieu
    legend = [
        (C_GREEN, "#E5F2E8", "Mã C# thuần (Plain C#)\nlogic, tính toán, ra quyết định"),
        (C_ORANGE, "#FCEFD0", "MonoBehaviour\ngắn với vòng đời Unity"),
        (C_GRAY, "#ECECEC", "Dữ liệu thuần (Data)\ntham số, cấu hình"),
    ]
    leg_y = 0.55
    leg_xs = [0.85, 3.5, 6.15]
    for (ec, fcc, lbl), lx in zip(legend, leg_xs):
        rect = FancyBboxPatch((lx - 0.28, leg_y - 0.18), 0.56, 0.36, boxstyle="round,pad=0.02",
                              fc=fcc, ec=ec, lw=1.5, zorder=3)
        ax.add_patch(rect)
        ax.text(lx, leg_y - 0.55, lbl, fontsize=11.5, color="#444444",
                ha="center", va="top", linespacing=1.3, zorder=4)

    ax.set_xlim(-1.95, 9.5)
    ax.set_ylim(-0.65, 9.55)
    ax.axis("off")
    ax.set_title("Hình 4.1 — Hình Dạng Kiến Trúc Khi Áp Dụng Ba Ràng Buộc",
                 fontsize=17, pad=14, fontweight="bold")
    fig.tight_layout()

    path = os.path.join(OUT, "hinh_4_1.png")
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


if __name__ == "__main__":
    hinh_4_1()
