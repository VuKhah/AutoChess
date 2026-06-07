"""
Ve hinh 2.7 den 2.16 cho tieu luan Auto Chess.
Chay: python gen_2_7_to_2_16.py
Ket qua luu vao thu muc OUTPUT_DIR ben duoi.
"""

import json, os, sys
import numpy as np
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch

matplotlib.rcParams["font.family"] = "DejaVu Sans"
matplotlib.rcParams["axes.unicode_minus"] = False

# ── CAU HINH DUONG DAN ──────────────────────────────────────────────────────
# Chinh 2 dong nay neu can:
AI_JSON    = r"D:\workspaces\specialized-essays\AutoChess\Assets\Resources\AI_Library.json"
OUTPUT_DIR = r"D:\workspaces\specialized-essays\AutoChess\Assets\Document\03_Figures"
# ────────────────────────────────────────────────────────────────────────────

os.makedirs(OUTPUT_DIR, exist_ok=True)
BG = "#FAFAFA"

def save(fig, name):
    path = os.path.join(OUTPUT_DIR, name)
    fig.savefig(path, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print("[OK]", path)

# ── HELPER ──────────────────────────────────────────────────────────────────
def draw_box(ax, cx, cy, w, h, text,
             fc="#E3F2FD", ec="#1565C0", fs=9, bold=False, radius=0.03):
    rect = FancyBboxPatch((cx - w/2, cy - h/2), w, h,
                          boxstyle=f"round,pad={radius}",
                          fc=fc, ec=ec, lw=1.5, zorder=3)
    ax.add_patch(rect)
    ax.text(cx, cy, text, ha="center", va="center", fontsize=fs,
            fontweight="bold" if bold else "normal", zorder=4)

def draw_arrow(ax, x0, y0, x1, y1, color="#555", lw=1.4, style="->"):
    ax.annotate("", xy=(x1, y1), xytext=(x0, y0),
                arrowprops=dict(arrowstyle=style, color=color, lw=lw))


# ════════════════════════════════════════════════════════════════════════════
# 2.7  Component-Entity Model
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_7():
    fig, ax = plt.subplots(figsize=(10, 5), facecolor=BG)
    ax.set_facecolor(BG); ax.axis("off")
    ax.set_xlim(0, 10); ax.set_ylim(0, 6)

    # GameObject root
    draw_box(ax, 5, 5, 4.2, 0.8, "Card  (GameObject)",
             fc="#E8F5E9", ec="#2E7D32", fs=12, bold=True)

    # 4 Components
    comps = [
        (1.3, 2.7, "CardUI",         "render card art\nstats, text",    "#E3F2FD", "#1565C0"),
        (3.7, 2.7, "CardDraggable",  "drag & drop\nslot detection",     "#FFF3E0", "#E65100"),
        (6.3, 2.7, "CardVisuals",    "animations\nVFX, highlights",     "#FCE4EC", "#880E4F"),
        (8.7, 2.7, "CardSlot",       "slot index\nboard position",      "#EDE7F6", "#4527A0"),
    ]
    for (cx, cy, title, desc, fc, ec) in comps:
        draw_box(ax, cx, cy + 0.35, 2.2, 0.6, title, fc=fc, ec=ec, fs=10, bold=True)
        draw_box(ax, cx, cy - 0.45, 2.2, 0.7, desc,  fc=fc, ec=ec, fs=8.5)
        # connect group
        ax.plot([cx - 1.1, cx + 1.1], [cy + 0.05, cy + 0.05], color=ec, lw=0.8, ls="--")
        draw_arrow(ax, cx, cy + 0.65, cx, cy - 0.08, color=ec, lw=1.3)
        draw_arrow(ax, cx, 4.6,        cx, cy + 0.65, color=ec, lw=1.3)

    ax.text(5, 1.0,
            "Moi Component ke thua MonoBehaviour — phat trien & tai su dung doc lap",
            ha="center", va="center", fontsize=9.5, color="#555", style="italic")

    ax.set_title("Hinh 2.7 — Component-Entity Model trong Unity",
                 fontsize=13, pad=8)
    fig.tight_layout()
    save(fig, "hinh_2_7.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.8  Partial Class GameManager
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_8():
    fig, ax = plt.subplots(figsize=(11, 5), facecolor=BG)
    ax.set_facecolor(BG); ax.axis("off")
    ax.set_xlim(0, 11); ax.set_ylim(0, 5.5)

    files = [
        (1.4,  "GameManager.cs",        "Core state\nHP, coins, board\nSingleton\nExecuteNextTurn", "#E3F2FD","#1565C0"),
        (3.8,  "GameManager.Shop.cs",   "RefreshShop\nBuyCard, Roll\nLock, MergeHints",             "#E8F5E9","#2E7D32"),
        (6.2,  "GameManager.Combat.cs", "StartCombatPhase\nCombatSequence\nVisualizeAction",        "#FCE4EC","#B71C1C"),
        (8.6,  "GameManager.Board.cs",  "SyncBoards\nSnapshot, Restore\nSpawnUI",                  "#EDE7F6","#4527A0"),
    ]

    for (cx, fname, body, fc, ec) in files:
        # file header
        draw_box(ax, cx, 4.35, 2.3, 0.55, fname,  fc=ec,  ec=ec,  fs=8,   bold=True)
        ax.texts[-1].set_color("white")
        # body
        draw_box(ax, cx, 3.2,  2.3, 1.55, body,   fc=fc,  ec=ec,  fs=8.2)
        draw_arrow(ax, cx, 2.42, cx, 1.75, color=ec, lw=1.5)

    # merged class box
    draw_box(ax, 5, 1.15, 8.0, 0.85,
             "public partial class GameManager : MonoBehaviour",
             fc="#FFFDE7", ec="#F57F17", fs=10.5, bold=True)

    ax.text(5, 0.35,
            "partial keyword  =>  4 file bien dich thanh 1 class duy nhat",
            ha="center", va="center", fontsize=9, color="#777", style="italic")

    ax.set_title("Hinh 2.8 — Partial Class GameManager", fontsize=13, pad=8)
    fig.tight_layout()
    save(fig, "hinh_2_8.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.9  MonoBehaviour vs Plain C# Boundary
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_9():
    fig, ax = plt.subplots(figsize=(11, 5.5), facecolor=BG)
    ax.set_facecolor(BG); ax.axis("off")
    ax.set_xlim(0, 11); ax.set_ylim(0, 6.5)

    # ── Top layer ──
    ax.add_patch(FancyBboxPatch((0.3, 3.6), 10.4, 2.5,
                                boxstyle="round,pad=0.12",
                                fc="#DBEAFE", ec="#1D4ED8", lw=2, alpha=0.55))
    ax.text(5.5, 5.8,
            "Tang Unity-dependent   (MonoBehaviour — can Scene de chay)",
            ha="center", va="center", fontsize=10.5,
            color="#1D4ED8", fontweight="bold")

    unity = ["GameManager", "CardDatabase", "UIManager", "AudioManager"]
    for i, name in enumerate(unity):
        draw_box(ax, 1.5 + i * 2.55, 4.55, 2.2, 0.72,
                 name, fc="#BFDBFE", ec="#1D4ED8", fs=9.5)

    # ── Bottom layer ──
    ax.add_patch(FancyBboxPatch((0.3, 0.4), 10.4, 2.8,
                                boxstyle="round,pad=0.12",
                                fc="#DCFCE7", ec="#15803D", lw=2, alpha=0.55))
    ax.text(5.5, 2.95,
            "Tang Headless-compatible   (Plain C# — new ClassName() — khong can Scene)",
            ha="center", va="center", fontsize=10.5,
            color="#15803D", fontweight="bold")

    plain = ["Chromosome", "BotAgent", "CombatResolver", "GameSimulator", "EconomyManager"]
    for i, name in enumerate(plain):
        draw_box(ax, 1.0 + i * 2.2, 1.65, 1.95, 0.72,
                 name, fc="#BBF7D0", ec="#15803D", fs=9)

    # ── Arrows ──
    for xa in [2.5, 5.5, 8.5]:
        draw_arrow(ax, xa, 3.6, xa, 3.2, color="#374151", lw=1.5)
    ax.text(6.6, 3.4, "mot chieu (Unity goi Plain,\nkhong chieu nguoc lai)",
            ha="left", va="center", fontsize=8.5, color="#555", style="italic")

    # ── Note ──
    ax.text(5.5, 0.18,
            "GATrainer (MonoBehaviour) goi GameSimulator.EvaluateMatch() "
            "hang nghin lan — khong tao Scene moi",
            ha="center", va="center", fontsize=8.5, color="#666", style="italic")

    ax.set_title("Hinh 2.9 — Ranh Gioi MonoBehaviour va Plain C#",
                 fontsize=13, pad=8)
    fig.tight_layout()
    save(fig, "hinh_2_9.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.10  TTE Space 3D Cube
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_10():
    from mpl_toolkits.mplot3d.art3d import Poly3DCollection

    fig = plt.figure(figsize=(7, 6), facecolor=BG)
    ax = fig.add_subplot(111, projection="3d")
    ax.set_facecolor(BG)

    T, Ta, E = 14, 12, 13   # Trigger, Target, Effect

    faces = [
        [(0,0,0),(T,0,0),(T,Ta,0),(0,Ta,0)],
        [(0,0,E),(T,0,E),(T,Ta,E),(0,Ta,E)],
        [(0,0,0),(0,Ta,0),(0,Ta,E),(0,0,E)],
        [(T,0,0),(T,Ta,0),(T,Ta,E),(T,0,E)],
        [(0,0,0),(T,0,0),(T,0,E),(0,0,E)],
        [(0,Ta,0),(T,Ta,0),(T,Ta,E),(0,Ta,E)],
    ]
    poly = Poly3DCollection(faces, alpha=0.18,
                            facecolor=["#90CAF9","#90CAF9",
                                       "#A5D6A7","#A5D6A7",
                                       "#FFCC80","#FFCC80"],
                            edgecolor="#5588BB", linewidth=0.9)
    ax.add_collection3d(poly)

    ax.set_xlabel("Trigger  (14)", fontsize=10, labelpad=10)
    ax.set_ylabel("Target   (12)", fontsize=10, labelpad=10)
    ax.set_zlabel("Effect   (13)", fontsize=10, labelpad=10)
    ax.set_xlim(0, T); ax.set_ylim(0, Ta); ax.set_zlim(0, E)
    ax.set_xticks([0, T]); ax.set_yticks([0, Ta]); ax.set_zticks([0, E])

    examples = [
        (8, 5, 1,  "OnDeath + Summon\n(Horus)"),
        (1, 2, 4,  "StartBattle + AddStats\n(Marduk)"),
        (13,10, 9, "Aura + GiveBuff\n(Osiris)"),
    ]
    for x, y, z, lbl in examples:
        ax.scatter([x],[y],[z], s=70, color="#EF5350", zorder=5)
        ax.text(x+0.4, y+0.4, z+0.5, lbl, fontsize=7.5, color="#B71C1C")

    ax.text2D(0.5, -0.04,
              "14  x  12  x  13  =  2,184 to hop co ban",
              transform=ax.transAxes, ha="center", fontsize=10.5, color="#333")

    ax.set_title("Hinh 2.10 — Khong Gian To Hop TTE",
                 fontsize=13, pad=16)
    ax.view_init(elev=24, azim=-52)
    fig.tight_layout()
    save(fig, "hinh_2_10.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.11  TriggerAbility() Flowchart
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_11():
    fig, ax = plt.subplots(figsize=(8, 13), facecolor=BG)
    ax.set_facecolor(BG); ax.axis("off")
    ax.set_xlim(0, 9); ax.set_ylim(0, 14)

    PROC = ("#E3F2FD", "#1565C0")
    COND = ("#FCE4EC", "#C62828")
    ACT  = ("#E8F5E9", "#2E7D32")
    CALL = ("#FFF9C4", "#F57F17")
    END  = ("#C8E6C9", "#1B5E20")

    nodes = [
        # (cx, cy, w, h, label, style, bold)
        (4.5, 13.2, 7.5, 0.7,  "TriggerAbility(context, source, ...)",    PROC, True),
        (4.5, 12.0, 7.0, 0.65, "Source co abilities khong?",               COND, False),
        (4.5, 10.8, 7.0, 0.65, "ability.trigger == context?",              COND, False),
        (4.5,  9.6, 7.0, 0.65, "Da dat triggerLimit?",                     COND, False),
        (4.5,  8.4, 7.0, 0.65, "conditionCount & tribe filter OK?",        COND, False),
        (4.5,  7.2, 7.0, 0.65, "FindTargets()  ->  List<CardInstance>",    CALL, False),
        (4.5,  6.0, 7.0, 0.65, "targets rong  &&  triggerLimit > 0?",      COND, False),
        (4.5,  4.8, 7.0, 0.9,  "ApplyGrowth()  hoac  ExecuteEffect()\nvoi moi target trong danh sach", ACT, False),
        (4.5,  3.5, 7.0, 0.65, "isEscalating?\n-> tang escalationBonus",   COND, False),
        (4.5,  2.2, 5.0, 0.65, "Ket thuc (lap sang ability tiep theo)",    END,  True),
    ]

    for (cx, cy, w, h, lbl, style, bold) in nodes:
        draw_box(ax, cx, cy, w, h, lbl, fc=style[0], ec=style[1], fs=9, bold=bold)

    # Main flow
    for i in range(len(nodes)-1):
        _, y0, _, h0, *_ = nodes[i]
        _, y1, _, h1, *_ = nodes[i+1]
        draw_arrow(ax, 4.5, y0-h0/2, 4.5, y1+h1/2, color="#444")

    # Skip branches -> right side exit
    skips = [
        (1, "Khong\n(thoat)"),
        (2, "Sai trigger\n(bo qua)"),
        (3, "Da dat limit\n(bo qua)"),
        (4, "Khong hop le\n(bo qua)"),
        (6, "Co: khong dem\nlimit (bo qua)"),
    ]
    SIDE_X = 8.5
    side_ys = []
    for idx, lbl in skips:
        cx, cy, w, h, *_ = nodes[idx]
        sx = cx + w/2
        side_ys.append(cy)
        ax.annotate("", xy=(SIDE_X, cy), xytext=(sx, cy),
                    arrowprops=dict(arrowstyle="->", color="#BDBDBD", lw=1.2,
                                    connectionstyle="arc3,rad=-0.15"))
        ax.text(sx + 0.1, cy + 0.18, lbl, fontsize=7.5, color="#888")

    # Vertical line on right side -> exit box
    ax.plot([SIDE_X, SIDE_X], [min(side_ys), max(side_ys)],
            color="#BDBDBD", lw=1.2, ls="--")
    draw_arrow(ax, SIDE_X, min(side_ys), 4.5 + 5/2, nodes[-1][1],
               color="#BDBDBD", lw=1.2)

    ax.set_title("Hinh 2.11 — Luong TriggerAbility()", fontsize=13, pad=8)
    fig.tight_layout()
    save(fig, "hinh_2_11.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.12  14 Trigger Types — 4 colour groups
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_12():
    groups = [
        ("Combat ca nhan", "#1D4ED8", "#DBEAFE", [
            ("OnAttack",     "Sau khi tan cong va con song",       "Ra-messe  (ATK tang theo so lan tan cong)"),
            ("OnTakeDamage", "Khi nhan damage va con song",        "Set  (phan don ngay lap tuc)"),
            ("OnDeath",      "Khi chet  (xu ly qua death stack)",  "Horus, Sekhmet, Osiris"),
            ("StartOfBattle","Mot lan dau moi tran",               "Marduk  (tich luy growth)"),
        ]),
        ("Shop phase", "#15803D", "#DCFCE7", [
            ("OnDeploy",    "Khi keo card len san",                "Thoth  (rut them card)"),
            ("OnSell",      "Khi ban card",                        "Isis  (buff dong minh con lai)"),
            ("EndTurnShop", "Dau luot tiep theo voi moi unit san", "Nephthys  (nhan them coin)"),
        ]),
        ("Phan ung dong minh", "#C2410C", "#FFEDD5", [
            ("OnAllyDeath",  "Dong minh cung tribe chet",          "Anubis  (stack ATK vinh vien)"),
            ("OnAllySummon", "Dong minh duoc trieu hoi combat",    "Sobek  (tang counter)"),
            ("OnAllyReborn", "Dong minh hoi sinh Reborn",          "Osiris  (nhan doi chi so)"),
            ("OnAllyDeploy", "Dong minh deploy trong shop phase",  "Hathor"),
            ("OnAllySell",   "Dong minh bi ban",                   "Bastet"),
        ]),
        ("Dac biet", "#7E22CE", "#F3E8FF", [
            ("Aura",        "Dau tran -> AllAlliesExceptSelf",     "Atlas  (passive ATK buff)"),
            ("OnStatGain",  "Khi nhan chi so vinh vien",           "Chain reaction  (guard flag)"),
        ]),
    ]

    fig, ax = plt.subplots(figsize=(13, 9), facecolor=BG)
    ax.set_facecolor(BG); ax.axis("off")
    ax.set_xlim(0, 13); ax.set_ylim(0, 10.5)
    ax.set_title("Hinh 2.12 — Phan Loai 14 Trigger Types",
                 fontsize=14, fontweight="bold", pad=6)

    COL_X = [1.5, 5.5, 10.5]
    COL_W = [2.6, 6.5, 4.5]
    for cx, lbl in zip(COL_X, ["Trigger", "Mo ta", "Vi du card"]):
        ax.text(cx, 9.9, lbl, ha="center", va="center",
                fontsize=10, fontweight="bold", color="#333")

    y = 9.55
    for grp_name, ec, fc, items in groups:
        # Group header bar
        ax.add_patch(FancyBboxPatch((0.2, y - 0.28), 12.6, 0.50,
                                    boxstyle="round,pad=0.05",
                                    fc=ec, ec=ec))
        ax.text(6.5, y - 0.02, grp_name, ha="center", va="center",
                fontsize=10, fontweight="bold", color="white")
        y -= 0.62

        for trig, desc, card in items:
            ax.add_patch(FancyBboxPatch((0.2, y - 0.26), 12.6, 0.45,
                                        boxstyle="round,pad=0.03",
                                        fc=fc, ec="#DDD", alpha=0.7))
            ax.text(COL_X[0], y - 0.02, trig, ha="center", va="center",
                    fontsize=9, fontweight="bold", color=ec)
            ax.text(COL_X[1], y - 0.02, desc, ha="center", va="center",
                    fontsize=8.5)
            ax.text(COL_X[2], y - 0.02, card, ha="center", va="center",
                    fontsize=8, color="#555", style="italic")
            y -= 0.50
        y -= 0.18

    fig.tight_layout()
    save(fig, "hinh_2_12.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.13  Pending Summon Queue & Death Stack Sequence Diagram
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_13():
    fig, ax = plt.subplots(figsize=(13, 7), facecolor=BG)
    ax.set_facecolor(BG); ax.axis("off")
    ax.set_xlim(0, 14); ax.set_ylim(0, 8.5)
    ax.set_title("Hinh 2.13 — Pending Summon Queue va Death Stack",
                 fontsize=13, pad=8)

    actors = [
        (1.1,  "Unit A\n(HP = 0)",       "#FCE4EC","#C62828"),
        (3.4,  "FlushDeathStack",         "#E3F2FD","#1565C0"),
        (6.2,  "TriggerAbility /\nCombatResolver", "#FFF9C4","#F57F17"),
        (9.0,  "PendingSummons\nQueue",   "#E8F5E9","#2E7D32"),
        (11.5, "Unit B\n(summon ngay)",   "#EDE7F6","#7B1FA2"),
        (13.0, "Unit C\n(enqueue)",       "#EDE7F6","#7B1FA2"),
    ]
    xs = [a[0] for a in actors]
    Y_HEAD = 8.0; Y_BOT = 0.7

    for (x, lbl, fc, ec) in actors:
        draw_box(ax, x, Y_HEAD, 1.9, 0.7, lbl, fc=fc, ec=ec, fs=8.5)
        ax.plot([x, x], [Y_HEAD - 0.35, Y_BOT], color="#CCC", lw=1.2, ls="--", zorder=1)

    msgs = [
        # (from_x_idx, to_x_idx, y, label, color)
        (0, 1, 7.0, "HP <= 0  ->  masuk deathStack",     "#C62828"),
        (1, 2, 6.2, "TriggerOnDeath( unitA )",            "#1565C0"),
        (2, 4, 5.4, "SummonUnit( B )  --  lang tuc",      "#7B1FA2"),
        (2, 5, 4.7, "Enqueue( C )  vao queue",            "#2E7D32"),
        (1, 2, 4.0, "BroadcastOnAllyDeath",               "#1565C0"),
        (1, 0, 3.3, "CleanupBoard:  xoa Unit A khoi san", "#C62828"),
        (1, 3, 2.6, "ProcessNextPendingSummon()",         "#F57F17"),
        (3, 5, 1.9, "pop C  ->  SummonUnit( C )",         "#7B1FA2"),
    ]

    for fi, ti, y, lbl, col in msgs:
        x0, x1 = xs[fi], xs[ti]
        style = "<-" if x1 < x0 else "->"
        ax.annotate("", xy=(x1, y), xytext=(x0, y),
                    arrowprops=dict(arrowstyle=style, color=col, lw=1.6))
        mx = (x0 + x1) / 2
        ax.text(mx, y + 0.14, lbl, ha="center", va="bottom",
                fontsize=8, color=col, fontweight="bold")

    fig.tight_layout()
    save(fig, "hinh_2_13.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.14  Drop Rate by Shop Tier
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_14():
    data = np.array([
        [100, 0,  0,  0,  0,  0],
        [70,  30, 0,  0,  0,  0],
        [50,  35, 15, 0,  0,  0],
        [25,  40, 25, 10, 0,  0],
        [15,  25, 35, 15, 10, 0],
        [10,  15, 20, 25, 20, 10],
    ])
    shop_labels = [f"Shop Lv {i}" for i in range(1, 7)]
    tier_labels = [f"Card Tier {i}" for i in range(1, 7)]
    colors = ["#90CAF9","#A5D6A7","#FFCC80","#F48FB1","#CE93D8","#80DEEA"]

    fig, ax = plt.subplots(figsize=(10, 5.5), facecolor=BG)
    ax.set_facecolor(BG)

    bottoms = np.zeros(6)
    for t in range(6):
        vals = data[:, t]
        ax.bar(range(6), vals, bottom=bottoms, color=colors[t],
               label=tier_labels[t], width=0.65, edgecolor="white", lw=0.8)
        for j, (b, v) in enumerate(zip(bottoms, vals)):
            if v >= 5:
                ax.text(j, b + v/2, f"{v}%", ha="center", va="center",
                        fontsize=9, fontweight="bold", color="#222")
        bottoms += vals

    ax.set_xticks(range(6)); ax.set_xticklabels(shop_labels, fontsize=10)
    ax.set_ylabel("Phan tram xuat hien (%)", fontsize=11)
    ax.set_ylim(0, 115)
    ax.set_title("Hinh 2.14 — Drop Rate Theo Shop Tier", fontsize=13, pad=10)
    ax.legend(loc="upper right", fontsize=9, ncol=2)
    ax.spines[["top","right"]].set_visible(False)
    ax.grid(axis="y", color="#DDD", lw=0.7)
    fig.tight_layout()
    save(fig, "hinh_2_14.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.15  Tempo vs Economy Trade-off
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_15():
    x = np.linspace(0, 1, 400)

    def bell(peak, sigma=0.22, scale=1.0):
        return scale * np.exp(-((x - peak)**2) / (2 * sigma**2))

    high_hp = bell(0.32, 0.20, 0.82)
    low_hp  = bell(0.68, 0.20, 0.88)

    fig, ax = plt.subplots(figsize=(9, 5), facecolor=BG)
    ax.set_facecolor(BG)

    ax.fill_between(x, high_hp, alpha=0.10, color="#1D4ED8")
    ax.fill_between(x, low_hp,  alpha=0.10, color="#DC2626")
    ax.plot(x, high_hp, color="#1D4ED8", lw=2.5, label="HP cao  (con nhieu buffer)")
    ax.plot(x, low_hp,  color="#DC2626", lw=2.5, label="HP thap  (can tempo ngay)")

    for curve, col, yo in [(high_hp,"#1D4ED8", 0.09), (low_hp,"#DC2626",-0.12)]:
        px = x[np.argmax(curve)]
        py = curve.max()
        ax.axvline(px, color=col, lw=1.4, ls="--", alpha=0.6)
        ax.annotate("Diem toi uu",
                    xy=(px, py),
                    xytext=(px + 0.09, py + yo),
                    arrowprops=dict(arrowstyle="->", color=col, lw=1.2),
                    fontsize=9, color=col)

    ax.annotate("", xy=(0.68, 0.12), xytext=(0.32, 0.12),
                arrowprops=dict(arrowstyle="->", color="#888", lw=1.3))
    ax.text(0.50, 0.09, "Khi HP giam,\ndiem toi uu dich sang phai",
            ha="center", fontsize=8.5, color="#666", style="italic")

    ax.set_xlim(0, 1); ax.set_ylim(0, 1.05)
    ax.set_xlabel("Muc do chi tieu som   [ Economy <──> Tempo ]", fontsize=11)
    ax.set_ylabel("Xac suat thang (uoc tinh)", fontsize=11)
    ax.set_title("Hinh 2.15 — Danh Doi Tempo va Economy", fontsize=13, pad=10)
    ax.set_xticks([0, 0.5, 1])
    ax.set_xticklabels(["Tiet kiem toi da\n(Economy)", "Can bang", "Chi tieu toi da\n(Tempo)"])
    ax.set_yticks([])
    ax.legend(fontsize=10, loc="upper center")
    ax.spines[["top","right","left"]].set_visible(False)
    fig.tight_layout()
    save(fig, "hinh_2_15.png")


# ════════════════════════════════════════════════════════════════════════════
# 2.16  Fitness Structure
# ════════════════════════════════════════════════════════════════════════════
def hinh_2_16():
    # score = base + hpA*6 - hpB*3 + (thang)*(MaxTurns-turns)*2
    MAX_T = 20
    scenarios = [
        ("Thang som\nHP cao",  120, 6*6, 0*3, (MAX_T-6)*2),
        ("Thang muon\nHP vua", 120, 3*6, 2*3, (MAX_T-16)*2),
        ("Hoa\nPhong thu",      70, 3*6, 3*3, 0),
        ("Thua sat\nHP can",    35, 1*6, 5*3, 0),
    ]
    labels   = [s[0] for s in scenarios]
    bases    = [s[1] for s in scenarios]
    hp_plus  = [s[2] for s in scenarios]
    hp_minus = [-s[3] for s in scenarios]
    speeds   = [s[4] for s in scenarios]
    totals   = [b+p+m+sp for b,p,m,sp in zip(bases,hp_plus,hp_minus,speeds)]

    x = np.arange(len(labels))
    w = 0.55

    fig, ax = plt.subplots(figsize=(9, 5.5), facecolor=BG)
    ax.set_facecolor(BG)

    ax.bar(x, bases,    width=w, color="#42A5F5", label="Ket qua co ban  (120/70/35)",      edgecolor="w")
    ax.bar(x, hp_plus,  width=w, bottom=bases,   color="#66BB6A", label="hpA x 6",          edgecolor="w")
    ax.bar(x, hp_minus, width=w,
           bottom=[b+p for b,p in zip(bases,hp_plus)],
           color="#EF9A9A", label="- hpB x 3",   edgecolor="w")
    ax.bar(x, speeds,   width=w,
           bottom=[b+p+m for b,p,m in zip(bases,hp_plus,hp_minus)],
           color="#FFA726", label="(MaxTurns-turns) x 2", edgecolor="w")

    for i, (b,p,m,sp,tot) in enumerate(zip(bases,hp_plus,hp_minus,speeds,totals)):
        ax.text(i, b/2,              f"+{b}",   ha="center", va="center", fontsize=9.5, fontweight="bold", color="white")
        if p > 0:
            ax.text(i, b+p/2,        f"+{p}",  ha="center", va="center", fontsize=9,   fontweight="bold", color="white")
        if abs(m) > 0:
            ax.text(i, b+p+m/2,      f"{m}",   ha="center", va="center", fontsize=9,   fontweight="bold", color="white")
        if sp > 0:
            ax.text(i, b+p+m+sp/2,   f"+{sp}", ha="center", va="center", fontsize=9,   fontweight="bold", color="white")
        ax.text(i, tot + 4, f"= {tot}", ha="center", va="bottom", fontsize=11, fontweight="bold", color="#222")

    ax.set_xticks(x); ax.set_xticklabels(labels, fontsize=10)
    ax.set_ylabel("Diem Fitness", fontsize=11)
    ax.set_ylim(-25, 240)
    ax.set_title("Hinh 2.16 — Cau Truc Ham Fitness", fontsize=13, pad=10)
    ax.legend(fontsize=9, loc="upper right")
    ax.spines[["top","right"]].set_visible(False)
    ax.grid(axis="y", color="#DDD", lw=0.7)
    fig.tight_layout()
    save(fig, "hinh_2_16.png")


# ── MAIN ────────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    hinh_2_7()
    hinh_2_8()
    hinh_2_9()
    hinh_2_10()
    hinh_2_11()
    hinh_2_12()
    hinh_2_13()
    hinh_2_14()
    hinh_2_15()
    hinh_2_16()
    print("Done: 10 figures saved to", OUTPUT_DIR)
