#!/usr/bin/env python3
"""
Ve lai Hinh 5.5 -- Su Mo Rong Cua Ham Fitness Qua Cac Vong Tinh Chinh
(standalone, khong phu thuoc generate_figures_ch5.py)

Chay tu bat ky dau trong repo:
    python Document/04_Tools/regen_hinh_5_5.py
Ket qua: Document/03_Figures/hinh_5_5.png  (ghi de ban cu)
"""

import os
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.colors as mc
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch

matplotlib.rcParams["font.family"] = "DejaVu Sans"
matplotlib.rcParams["axes.unicode_minus"] = False

BASE = os.path.dirname(os.path.abspath(__file__))
OUT  = os.path.normpath(os.path.join(BASE, "..", "03_Figures"))
os.makedirs(OUT, exist_ok=True)

BG       = "#FAFAFA"
C_BLUE   = "#1565C0"
C_RED    = "#E04040"
C_GREEN  = "#2E7D32"
C_ORANGE = "#EF6C00"
C_GRAY   = "#888888"


def tint(color, a=0.65):
    r, g, b = mc.to_rgb(color)
    return (1-(1-r)*a, 1-(1-g)*a, 1-(1-b)*a)


def save(fig, name):
    path = os.path.join(OUT, name)
    fig.savefig(path, dpi=150, bbox_inches="tight", facecolor=fig.get_facecolor())
    plt.close(fig)
    print("[OK]", path)


def arrow(ax, x0, y0, x1, y1, color=C_GRAY, lw=1.5):
    ax.add_patch(FancyArrowPatch(
        (x0, y0), (x1, y1),
        arrowstyle="-|>", color=color, lw=lw,
        connectionstyle="arc3,rad=0.0", mutation_scale=14, zorder=5))


def rbox(ax, cx, cy, w, h, text, fc, ec, fs=9.0, bold=False,
         color="#222222", radius=0.04, lw=1.8):
    ax.add_patch(FancyBboxPatch(
        (cx-w/2, cy-h/2), w, h,
        boxstyle=f"round,pad={radius}",
        fc=fc, ec=ec, lw=lw, zorder=3))
    ax.text(cx, cy, text, ha="center", va="center", fontsize=fs,
            color=color, fontweight="bold" if bold else "normal",
            multialignment="center", zorder=4)


# ─────────────────────────────────────────────────────────────────────────────

def draw():
    fig, (axL, axR) = plt.subplots(1, 2, figsize=(15.8, 8.4))
    fig.patch.set_facecolor(BG)

    for ax in (axL, axR):
        ax.set_facecolor(BG)
        ax.set_xlim(0, 10)
        ax.set_ylim(-1.85, 11.4)
        ax.axis("off")

    # ═══════════════════════════════════════════════════════════════════════
    # PANEL TRAI — Phien ban dau: phan phoi roi rac 3 gia tri
    # ═══════════════════════════════════════════════════════════════════════
    axL.text(5, 11.0, "Phiên bản đầu — chỉ thắng / thua",
             fontsize=12.5, ha="center", fontweight="bold", color="#1A1A1A")
    axL.text(5, 10.3,
             '"Trung thực" nhất với luật chơi — nhưng chỉ phân biệt được\n'
             'KẾT QUẢ, không phân biệt được CÁCH đi đến kết quả đó',
             fontsize=8.2, ha="center", color="#666666", style="italic",
             linespacing=1.5)

    BASE_Y = 1.5
    SCALE  = 0.52        # val=10 → cao 5.2 đơn vị
    BAR_W  = 1.55
    bars = [
        ("THẮNG", 10, C_GREEN, 2.1),
        ("HÒA",    2, C_GRAY,  5.0),
        ("THUA",   0, C_RED,   7.9),
    ]

    # Duong tham chieu ngang tai muc 10 va 2
    for val, clr in ((10, C_GREEN), (2, C_GRAY)):
        y_ref = BASE_Y + val * SCALE
        axL.plot([0.6, 9.4], [y_ref, y_ref],
                 color=clr, lw=0.9, ls="--", alpha=0.22, zorder=1)

    for lbl, val, clr, x in bars:
        h = max(0.16, val * SCALE)
        axL.add_patch(FancyBboxPatch(
            (x-BAR_W/2, BASE_Y), BAR_W, h,
            boxstyle="round,pad=0.03",
            fc=tint(clr, 0.62), ec=clr, lw=2.0, zorder=3))
        # Gia tri phia tren
        axL.text(x, BASE_Y + h + 0.30, str(val),
                 fontsize=14, ha="center", fontweight="bold", color=clr, zorder=4)
        # Ten ket qua phia duoi
        axL.text(x, BASE_Y - 0.44, lbl,
                 fontsize=10.5, ha="center", color="#333333", fontweight="bold")

    # Badge "roi rac"
    axL.text(5, BASE_Y + 10*SCALE + 0.76,
             "→ Phân phối rời rạc: chỉ 3 giá trị khả dĩ",
             fontsize=8.4, ha="center", color="#555555",
             bbox=dict(boxstyle="round,pad=0.34", fc="#EFEFEF", ec="#CCCCCC", lw=1.0))

    # Mui ten xuong ket luan
    arrow(axL, 5, BASE_Y - 0.82, 5, -1.08, color=C_RED, lw=2.4)

    # Hop ket luan
    rbox(axL, 5, -1.42, 9.2, 0.80,
         "Quần thể hội tụ nhanh về một chiến lược RUSH một chiều\n"
         "(áp đảo early-game — sụp đổ trước đối thủ biết tích lũy & scale)",
         fc=tint(C_RED, 0.88), ec=C_RED, fs=8.3, bold=True, color="#B71C1C")

    # ═══════════════════════════════════════════════════════════════════════
    # PANEL PHAI — Phien ban hien hanh: 6 tin hieu, 3 nhom co trong so
    # ═══════════════════════════════════════════════════════════════════════
    axR.text(5, 11.0, "Phiên bản hiện hành — sáu tín hiệu, ba nhóm",
             fontsize=12.5, ha="center", fontweight="bold", color="#1A1A1A")
    axR.text(5, 10.3,
             'Mỗi nhóm đo một khía cạnh khác nhau của "chơi tốt" — cộng dồn\n'
             'thành MỘT điểm fitness, với trọng số cố ý bất đối xứng',
             fontsize=8.2, ha="center", color="#666666", style="italic",
             linespacing=1.5)

    # Cot xep chong can giua (rong hon ban cu)
    BAR_CX  = 3.0
    BAR_W2  = 3.4
    BAR_L   = BAR_CX - BAR_W2/2      # = 1.3
    BASE_Y2 = 1.5

    # Chieu cao ti le: nhom 1 ap dao, nhom 2 vua, nhom 3 nho
    # Blue/Orange tang them mot chut de callout text khong bi chen
    SEG_H = [4.20, 1.90, 1.20]
    SEG_COLORS = [C_GREEN, C_BLUE, C_ORANGE]

    seg_cy = []
    y2 = BASE_Y2
    for h, clr in zip(SEG_H, SEG_COLORS):
        axR.add_patch(FancyBboxPatch(
            (BAR_L, y2), BAR_W2, h,
            boxstyle="round,pad=0.03",
            fc=tint(clr, 0.62), ec=clr, lw=2.0, zorder=3))
        seg_cy.append(y2 + h/2)
        y2 += h

    # Dau "+" tai cac ranh gioi doan — dung badge tron
    for y_join in (BASE_Y2+SEG_H[0], BASE_Y2+SEG_H[0]+SEG_H[1]):
        axR.text(BAR_CX, y_join, "+",
                 fontsize=12, ha="center", va="center",
                 color="#FFFFFF", fontweight="bold", zorder=6,
                 bbox=dict(boxstyle="circle,pad=0.20", fc=C_GRAY, ec="none", alpha=0.80))

    # Nhan phia tren va phia duoi cot
    total_h2 = sum(SEG_H)
    axR.text(BAR_CX, BASE_Y2 + total_h2 + 0.40,
             "= ScoreFromA( … )",
             fontsize=9.2, ha="center", va="bottom",
             color="#444444", style="italic", fontweight="bold")
    axR.text(BAR_CX, BASE_Y2 - 0.44,
             "điểm một trận đấu",
             fontsize=8.2, ha="center", va="top", color="#888888", style="italic")

    # ── Callout labels ben phai cot ──────────────────────────────────────
    # Moi callout: mui ten ngang + title (dam) + cong thuc + chu thich ngan

    CALL_SRC = BAR_L + BAR_W2          # mat phai cot = 4.7
    CALL_DST = CALL_SRC + 0.40         # dau mui ten   = 5.1
    TXT_X    = CALL_DST + 0.15         # bat dau chu   = 5.25

    # (color, title, formula, note)
    # Orange va Blue gan nhau — giu title 1 dong, note 1 dong ngan
    # Green co nhieu khong gian — note 2 dong
    callouts = [
        (C_GREEN,
         "① Kết quả trận đấu",
         "base = 300 / 100 / 10",
         "Tín hiệu áp đảo — chiếm phần lớn biên độ điểm;\n"
         "GA ưu tiên thắng trước, mọi thứ khác sau"),
        (C_BLUE,
         "② Biên độ HP tức thời",
         "hpA×8 − hpB×4",
         "Giữ mạng được thưởng × 2 so với gây thiệt hại"),
        (C_ORANGE,
         "③–⑥  Bàn cờ & chất lượng đội hình",
         "0.06 / 0.04 / 0.035 / 0.025",
         "Lấy mẫu nửa sau trận — tuyệt đối + tương đối"),
    ]

    for (clr, title, formula, note), cy_seg in zip(callouts, seg_cy):
        # Mui ten ngang tu mat phai cot
        axR.annotate(
            "", xy=(CALL_DST, cy_seg), xytext=(CALL_SRC, cy_seg),
            arrowprops=dict(arrowstyle="-|>", color=clr, lw=1.5,
                            mutation_scale=11, shrinkA=0, shrinkB=0))
        # Title (dam, mau nhom)
        axR.text(TXT_X, cy_seg + 0.22, title,
                 fontsize=8.8, ha="left", va="bottom",
                 color=clr, fontweight="bold", zorder=4)
        # Cong thuc (khong dung monospace de tranh loi glyph tieng Viet)
        axR.text(TXT_X, cy_seg - 0.05, formula,
                 fontsize=8.2, ha="left", va="top",
                 color="#1A1A1A", zorder=4)
        # Chu thich (nghieng, xam)
        axR.text(TXT_X, cy_seg - 0.42, note,
                 fontsize=7.4, ha="left", va="top",
                 color="#666666", style="italic", zorder=4, linespacing=1.35)

    # Mui ten xuong ket luan
    arrow(axR, BAR_CX, BASE_Y2 - 0.82, BAR_CX, -1.08, color=C_GREEN, lw=2.4)

    # Hop ket luan
    rbox(axR, 5, -1.42, 9.2, 0.80,
         "Áp lực chọn lọc đa chiều — phân biệt được CÁCH một bot\n"
         "đi đến chiến thắng, chứ không chỉ CÓ thắng được hay không",
         fc=tint(C_GREEN, 0.88), ec=C_GREEN, fs=8.3, bold=True, color="#1B5E20")

    # ── Tieu de hinh ──────────────────────────────────────────────────────
    fig.suptitle(
        "Hình 5.5 — Sự Mở Rộng Của Hàm Fitness Qua Các Vòng Tinh Chỉnh",
        fontsize=13.5, fontweight="bold", y=0.988)
    fig.tight_layout(rect=[0, 0, 1, 0.972])
    save(fig, "hinh_5_5.png")


if __name__ == "__main__":
    draw()
