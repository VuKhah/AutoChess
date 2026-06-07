"""
Tao Hinh 2.1, 2.2, 2.3, 2.7 - 2.16 cho tieu luan chuyen nganh Auto Chess.
Output: Assets/Document/03_Figures/hinh_2_X.png
"""

import json, os, textwrap
import numpy as np
import matplotlib
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch
import matplotlib.patheffects as pe

matplotlib.rcParams["font.family"] = "DejaVu Sans"
matplotlib.rcParams["axes.unicode_minus"] = False

BASE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(BASE, "..", ".."))
AI_JSON = os.path.join(ROOT, "Resources", "AI_Library.json")
OUT     = os.path.join(ROOT, "Document", "03_Figures")
os.makedirs(OUT, exist_ok=True)

BG = "#FAFAFA"

# 9 mau cho 9 nhom gene
GRP_COLORS = [
    "#E57373","#FFB74D","#81C784","#64B5F6","#BA68C8",
    "#4DD0E1","#A1887F","#90A4AE","#F06292",
]
GRP_NAMES = [
    "Stat (0-3)","Keywords (4-6)","Trigger (7-12)",
    "Effect (13-17)","Tribe (18-20)","Board (21-23)",
    "Reroll (24-27)","Spell (28-31)","Aura/Ally (32-36)",
]
GENE_TO_GRP = (
    [0]*4 + [1]*3 + [2]*6 + [3]*5 + [4]*3 +
    [5]*3 + [6]*4 + [7]*4 + [8]*5
)
GENE_NAMES = [
    "wATK","wHP","wTierBonus","wCostEff",
    "wTaunt","wReborn","wSafeguard",
    "tStartBattle","tOnDeath","tOnAttack","tOnTakeDmg","tEndTurnShop","tOnDeploy",
    "eAddStats","eSummon","eDealDmg","eGainCoin","eGiveBuff",
    "sBabylon","sOlympus","sNiles",
    "wMerge","wFrontline","wSaveThreshold",
    "wRerollThresh","wRerollMax","wRerollKeep","wProactiveSell",
    "wSpellThresh","wSpellOnStrong","wSpellOnMerged","wSpellEconomy",
    "tAura","tOnSell","tOnAllyGroup","tOnAllyDeploy","tOnAllySell",
]

def save(fig, name):
    path = os.path.join(OUT, name)
    fig.savefig(path, dpi=150, bbox_inches="tight")
    plt.close(fig)
    print(f"[OK] {path}")

# ─── helper diagram ────────────────────────────────────────────────────────────
def box(ax, x, y, w, h, label, fc="#EEF4FF", ec="#5588BB",
        fontsize=9, radius=0.03, bold=False, color="#222"):
    rect = FancyBboxPatch((x-w/2, y-h/2), w, h,
        boxstyle=f"round,pad={radius}", fc=fc, ec=ec, lw=1.4, zorder=3)
    ax.add_patch(rect)
    weight = "bold" if bold else "normal"
    ax.text(x, y, label, ha="center", va="center",
            fontsize=fontsize, weight=weight, color=color, zorder=4,
            wrap=True)

def arrow(ax, x0, y0, x1, y1, color="#5588BB", lw=1.4, style="->"):
    ax.annotate("", xy=(x1,y1), xytext=(x0,y0),
        arrowprops=dict(arrowstyle=style, color=color,
                        lw=lw, connectionstyle="arc3,rad=0"))


# ══════════════════════════════════════════════════════════════════════════════
# 2.1 — Chromosome 37 Gene Structure
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_1():
    with open(AI_JSON) as f:
        ai = json.load(f)
    hard_genes = ai["hardBot"]["genes"]

    fig, ax = plt.subplots(figsize=(10, 8), facecolor=BG)
    ax.set_facecolor(BG)

    for i in range(37):
        g   = GENE_TO_GRP[i]
        val = hard_genes[i]
        col = GRP_COLORS[g]
        ax.barh(36-i, val, color=col, alpha=0.85, height=0.72, zorder=3)
        ax.barh(36-i, 1.0, color=col, alpha=0.10, height=0.72, zorder=2)
        ax.text(-0.02, 36-i, f"[{i:02d}] {GENE_NAMES[i]}",
                ha="right", va="center", fontsize=7.5)
        ax.text(val + 0.02, 36-i, f"{val:.2f}",
                ha="left", va="center", fontsize=7, color="#444")

    # Group labels on right
    grp_rows = {}
    for i, g in enumerate(GENE_TO_GRP):
        grp_rows.setdefault(g, []).append(36-i)
    for g, rows in grp_rows.items():
        mid = (max(rows)+min(rows))/2
        ax.text(1.10, mid, GRP_NAMES[g], ha="left", va="center",
                fontsize=8.5, color=GRP_COLORS[g], weight="bold")
        ax.plot([1.06,1.08],[min(rows)-0.35,max(rows)+0.35],
                color=GRP_COLORS[g], lw=3, solid_capstyle="round")

    ax.set_xlim(-0.25, 1.30)
    ax.set_ylim(-0.7, 36.7)
    ax.set_xlabel("Gia tri gene (hardBot)", fontsize=11)
    ax.set_title("Hinh 2.1 — Cau Truc Chromosome 37 Gene", fontsize=13, pad=10)
    ax.set_yticks([])
    ax.spines[["top","right","left"]].set_visible(False)
    ax.axvline(0, color="#bbb", lw=0.8)
    ax.grid(axis="x", color="#DDDDDD", lw=0.7, zorder=0)
    fig.tight_layout()
    save(fig, "hinh_2_1.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.2 — GA Evolution Loop Flowchart
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_2():
    fig, ax = plt.subplots(figsize=(6, 10), facecolor=BG)
    ax.set_facecolor(BG)
    ax.set_xlim(0, 6); ax.set_ylim(0, 11)
    ax.axis("off")

    steps = [
        (3, 10.0, 5.2, 0.7, "Khoi tao quan the P\n(ngau nhien + seeding 5 archetype)", "#C8E6C9","#388E3C"),
        (3,  8.7, 5.2, 0.7, "Danh gia fitness(c)\ncho moi c thuoc P",                 "#E3F2FD","#1565C0"),
        (3,  7.4, 5.2, 0.7, "Sap xep P theo fitness giam dan",                         "#E3F2FD","#1565C0"),
        (3,  5.8, 5.2, 1.5, "Tao the he moi P':\n+ Clone elite (top 10%)\n+ Tournament x2 -> Crossover\n+ Mutate + Immigrate", "#FFF9C4","#F57F17"),
        (3,  4.2, 5.2, 0.7, "P <- P'\n(the he ke tiep)",                               "#E3F2FD","#1565C0"),
        (3,  2.8, 3.8, 0.7, "Dat plateau?",                                            "#FCE4EC","#C62828"),
        (3,  1.4, 5.2, 0.7, "Tra ve 5 specialist bot\nthu duoc",                       "#C8E6C9","#2E7D32"),
    ]

    for (x, y, w, h, lbl, fc, ec) in steps:
        box(ax, x, y, w, h, lbl, fc=fc, ec=ec, fontsize=8.5)

    # Arrows between steps
    pairs = [(10.0,8.7),(8.7,7.4),(7.4,5.8+0.75),(5.8-0.75,4.2),(4.2,2.8)]
    for y0, y1 in pairs:
        arrow(ax, 3, y0-0.35, 3, y1+0.35)

    # Decision diamond-like: plateau branch
    arrow(ax, 3, 2.8-0.35, 3, 1.4+0.35, color="#C62828")  # Yes -> end
    ax.text(3.15, 2.1, "Co (plateau)", fontsize=8, color="#C62828")
    # No -> back to evaluate
    ax.annotate("", xy=(0.6, 8.7), xytext=(0.6, 2.8),
        arrowprops=dict(arrowstyle="->", color="#1565C0", lw=1.4))
    ax.plot([3-5.2/2, 0.6], [2.8, 2.8], color="#1565C0", lw=1.4)
    ax.plot([0.6, 0.6+5.2/2-0.0], [8.7, 8.7], color="#1565C0", lw=1.4)
    ax.text(0.1, 5.7, "Khong\n(lap lai)", fontsize=8, color="#1565C0", ha="center")

    ax.set_title("Hinh 2.2 — Vong Lap Tien Hoa GA", fontsize=13, pad=10)
    fig.tight_layout()
    save(fig, "hinh_2_2.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.3 — Gaussian Mutation N(0, 0.12)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_3():
    sigma = 0.12
    x = np.linspace(-0.5, 0.5, 1000)
    y = (1/(sigma * np.sqrt(2*np.pi))) * np.exp(-0.5*(x/sigma)**2)

    fig, ax = plt.subplots(figsize=(8, 4.5), facecolor=BG)
    ax.set_facecolor(BG)

    # 2-sigma region
    mask2 = np.abs(x) <= 2*sigma
    ax.fill_between(x, y, where=mask2, alpha=0.25, color="#4CAF50", label=f"Ben trong ±2σ = ±{2*sigma:.2f} (~95%)")
    # 1-sigma region
    mask1 = np.abs(x) <= sigma
    ax.fill_between(x, y, where=mask1, alpha=0.40, color="#2196F3", label=f"Ben trong ±1σ = ±{sigma:.2f} (~68%)")

    ax.plot(x, y, color="#1565C0", lw=2.2)

    # Vertical lines
    for s, lbl, col in [(sigma,"1σ","#2196F3"), (2*sigma,"2σ","#4CAF50")]:
        for sign in [-1, 1]:
            ax.axvline(sign*s, color=col, lw=1.4, ls="--")
        ax.text( s+0.01, max(y)*0.5, f"+{lbl}", fontsize=9, color=col)
        ax.text(-s-0.01, max(y)*0.5, f"-{lbl}", fontsize=9, color=col, ha="right")

    ax.set_xlabel("Delta gene (cong vao gia tri hien tai)", fontsize=11)
    ax.set_ylabel("Xac suat", fontsize=11)
    ax.set_title("Hinh 2.3 — Gaussian Mutation  N(0, σ=0.12)", fontsize=13, pad=10)
    ax.legend(fontsize=9)
    ax.spines[["top","right"]].set_visible(False)
    ax.set_xlim(-0.45, 0.45)
    fig.tight_layout()
    save(fig, "hinh_2_3.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.7 — Component-Entity Model
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_7():
    fig, ax = plt.subplots(figsize=(7, 4.5), facecolor=BG)
    ax.set_facecolor(BG); ax.set_xlim(0,10); ax.set_ylim(0,6); ax.axis("off")

    # GameObject
    box(ax, 5, 4.8, 3.6, 0.8, "Card  (GameObject)", fc="#E8F5E9", ec="#2E7D32",
        fontsize=11, bold=True, color="#1B5E20")

    # 4 Components
    comps = [
        (1.5, 2.5, "CardUI\n(render card art,\nstats text)"),
        (4.0, 2.5, "CardDraggable\n(drag & drop,\nslot detection)"),
        (6.5, 2.5, "CardVisuals\n(animations,\nVFX, highlights)"),
        (9.0, 2.5, "CardSlot\n(slot index,\nboard position)"),
    ]
    comp_colors = ["#E3F2FD","#FFF3E0","#FCE4EC","#EDE7F6"]
    comp_ec     = ["#1565C0","#E65100","#880E4F","#4527A0"]
    for i,(x,y,lbl) in enumerate(comps):
        box(ax, x, y, 2.6, 1.4, lbl, fc=comp_colors[i], ec=comp_ec[i], fontsize=8.5)
        arrow(ax, x, 3.9, x, y+0.7, color=comp_ec[i])

    # MonoBehaviour label
    ax.text(5, 1.4, "Moi component ke thua MonoBehaviour — phat trien doc lap",
            ha="center", va="center", fontsize=9, color="#555",
            style="italic")

    ax.set_title("Hinh 2.7 — Component-Entity Model trong Unity", fontsize=13, pad=6)
    fig.tight_layout()
    save(fig, "hinh_2_7.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.8 — Partial Class GameManager
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_8():
    fig, ax = plt.subplots(figsize=(8, 4), facecolor=BG)
    ax.set_facecolor(BG); ax.set_xlim(0,10); ax.set_ylim(0,5); ax.axis("off")

    files = [
        (1.2, 3.5, "GameManager.cs\nCore state: HP, coins,\nboard, Singleton,\nExecuteNextTurn"),
        (3.7, 3.5, "GameManager.Shop.cs\nRefreshShop, BuyCard,\nRoll, Lock,\nMergeHints"),
        (6.2, 3.5, "GameManager.Combat.cs\nStartCombatPhase,\nCombatSequence,\nVisualizeAction"),
        (8.7, 3.5, "GameManager.Board.cs\nSyncBoards,\nSnapshot, Restore,\nSpawnUI"),
    ]
    file_ec = ["#1565C0","#2E7D32","#B71C1C","#4527A0"]
    file_fc = ["#E3F2FD","#E8F5E9","#FCE4EC","#EDE7F6"]

    for i,(x,y,lbl) in enumerate(files):
        box(ax, x, y, 2.3, 1.5, lbl, fc=file_fc[i], ec=file_ec[i], fontsize=7.8)

    # Central merged class
    box(ax, 5, 1.3, 5.5, 0.9,
        "public partial class GameManager : MonoBehaviour",
        fc="#FFFDE7", ec="#F57F17", fontsize=9.5, bold=True)

    # Arrows
    for i,(x,y,_) in enumerate(files):
        arrow(ax, x, y-0.75, 5, 1.3+0.45, color=file_ec[i], lw=1.3)

    ax.text(5, 0.35, "partial keyword => 4 file = 1 class duy nhat khi bien dich",
            ha="center", va="center", fontsize=9, color="#555", style="italic")
    ax.set_title("Hinh 2.8 — Partial Class GameManager", fontsize=13, pad=6)
    fig.tight_layout()
    save(fig, "hinh_2_8.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.9 — MonoBehaviour vs Plain C# Boundary
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_9():
    fig, ax = plt.subplots(figsize=(9, 5), facecolor=BG)
    ax.set_facecolor(BG); ax.set_xlim(0,10); ax.set_ylim(0,6); ax.axis("off")

    # Top layer: Unity-dependent
    ax.add_patch(FancyBboxPatch((0.3,3.4),9.4,2.2, boxstyle="round,pad=0.1",
        fc="#E3F2FD", ec="#1565C0", lw=2, alpha=0.5))
    ax.text(5, 5.35, "Tang Unity-dependent  (MonoBehaviour — can scene)",
            ha="center", va="center", fontsize=10, color="#1565C0", weight="bold")

    unity_items = ["GameManager","CardDatabase","UIManager","AudioManager"]
    for i,name in enumerate(unity_items):
        x = 1.5 + i*2.2
        box(ax, x, 4.4, 1.9, 0.7, name, fc="#BBDEFB", ec="#1565C0", fontsize=9)

    # Bottom layer: Plain C#
    ax.add_patch(FancyBboxPatch((0.3,0.4),9.4,2.5, boxstyle="round,pad=0.1",
        fc="#E8F5E9", ec="#2E7D32", lw=2, alpha=0.5))
    ax.text(5, 2.65, "Tang Headless-compatible  (Plain C# — khong can scene)",
            ha="center", va="center", fontsize=10, color="#2E7D32", weight="bold")

    plain_items = ["Chromosome","BotAgent","CombatResolver","GameSimulator","EconomyManager"]
    for i,name in enumerate(plain_items):
        x = 1.0 + i*2.0
        box(ax, x, 1.5, 1.75, 0.7, name, fc="#C8E6C9", ec="#2E7D32", fontsize=8.5)

    # Arrows downward (Unity can call Plain, not reverse)
    for xa in [2.5, 5.0, 7.5]:
        arrow(ax, xa, 3.4, xa, 2.9, color="#555", lw=1.2)
    ax.text(5.3, 3.15, "Mot chieu (Unity goi Plain)", fontsize=8.5, color="#555", style="italic")

    ax.set_title("Hinh 2.9 — Ranh Gioi MonoBehaviour va Plain C#", fontsize=13, pad=6)
    fig.tight_layout()
    save(fig, "hinh_2_9.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.10 — TTE Space 3D Cube
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_10():
    from mpl_toolkits.mplot3d import Axes3D
    from mpl_toolkits.mplot3d.art3d import Poly3DCollection

    fig = plt.figure(figsize=(7, 6), facecolor=BG)
    ax = fig.add_subplot(111, projection="3d")
    ax.set_facecolor(BG)

    # Draw a unit cube scaled to 14 x 12 x 13
    Tx, Ty, Tz = 14, 12, 13  # Trigger, Target, Effect
    verts_faces = [
        [(0,0,0),(Tx,0,0),(Tx,Ty,0),(0,Ty,0)],   # bottom
        [(0,0,Tz),(Tx,0,Tz),(Tx,Ty,Tz),(0,Ty,Tz)], # top
        [(0,0,0),(0,Ty,0),(0,Ty,Tz),(0,0,Tz)],   # left
        [(Tx,0,0),(Tx,Ty,0),(Tx,Ty,Tz),(Tx,0,Tz)], # right
        [(0,0,0),(Tx,0,0),(Tx,0,Tz),(0,0,Tz)],   # front
        [(0,Ty,0),(Tx,Ty,0),(Tx,Ty,Tz),(0,Ty,Tz)], # back
    ]
    face_colors = [
        "#64B5F640","#64B5F620",
        "#81C78430","#81C78420",
        "#FFB74D40","#FFB74D20",
    ]
    poly = Poly3DCollection(verts_faces, alpha=0.25)
    poly.set_facecolor(face_colors)
    poly.set_edgecolor("#5588BB")
    ax.add_collection3d(poly)

    # Axis labels
    ax.set_xlabel("Trigger (14)", fontsize=10, labelpad=8)
    ax.set_ylabel("Target (12)", fontsize=10, labelpad=8)
    ax.set_zlabel("Effect (13)", fontsize=10, labelpad=8)
    ax.set_xlim(0, Tx); ax.set_ylim(0, Ty); ax.set_zlim(0, Tz)
    ax.set_xticks([0, Tx]); ax.set_yticks([0, Ty]); ax.set_zticks([0, Tz])

    # Example labels inside cube
    examples = [
        (7, 6, 6.5, "OnDeath\n+Summon\n(Horus)"),
        (2, 3, 3,   "StartBattle\n+AddStats\n(Marduk)"),
        (10, 9, 10, "Aura\n+GiveBuff\n(Osiris)"),
    ]
    for (x,y,z,lbl) in examples:
        ax.scatter([x],[y],[z], s=80, color="#E57373", zorder=5)
        ax.text(x+0.5, y+0.5, z+0.5, lbl, fontsize=7.5, color="#B71C1C")

    # Total combos text
    ax.text2D(0.5, -0.02, f"14 × 12 × 13 = 2,184 to hop co ban",
              transform=ax.transAxes, ha="center", fontsize=10, color="#444")

    ax.set_title("Hinh 2.10 — Khong Gian To Hop TTE", fontsize=13, pad=14)
    ax.view_init(elev=22, azim=-55)
    fig.tight_layout()
    save(fig, "hinh_2_10.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.11 — TriggerAbility() Flowchart
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_11():
    fig, ax = plt.subplots(figsize=(7, 11), facecolor=BG)
    ax.set_facecolor(BG); ax.set_xlim(0,8); ax.set_ylim(0,13); ax.axis("off")

    EXEC  = "#E3F2FD"; EXEC_E  = "#1565C0"
    CHECK = "#FCE4EC"; CHECK_E = "#C62828"
    END   = "#C8E6C9"; END_E   = "#2E7D32"
    SKIP_COL = "#BDBDBD"

    nodes = [
        (4, 12.3, 6.0, 0.75, "TriggerAbility(context, source, ...)", "#EDE7F6","#4527A0", True),
        (4, 11.2, 5.5, 0.65, "Source co abilities?",                  CHECK, CHECK_E, False),
        (4, 10.1, 5.5, 0.65, "ability.trigger == context?",           CHECK, CHECK_E, False),
        (4,  9.0, 5.5, 0.65, "Da dat triggerLimit?",                  CHECK, CHECK_E, False),
        (4,  7.9, 5.5, 0.65, "conditionCount / tribe filter OK?",     CHECK, CHECK_E, False),
        (4,  6.8, 5.5, 0.65, "FindTargets() -> danh sach target",     EXEC,  EXEC_E,  False),
        (4,  5.7, 5.5, 0.65, "targets rong && triggerLimit > 0?",     CHECK, CHECK_E, False),
        (4,  4.6, 5.5, 0.65, "ApplyGrowth() hoac ExecuteEffect()\nvoi moi target", EXEC, EXEC_E, False),
        (4,  3.4, 5.5, 0.65, "isEscalating?\n-> tang escalationBonus", CHECK, CHECK_E, False),
        (4,  2.2, 4.0, 0.65, "Ket thuc",                              END,   END_E,   True),
    ]

    for (x,y,w,h,lbl,fc,ec,bold) in nodes:
        box(ax, x, y, w, h, lbl, fc=fc, ec=ec, fontsize=8.5, bold=bold)

    # Main flow arrows
    ys = [n[1] for n in nodes]
    hs = [n[3] for n in nodes]
    for i in range(len(nodes)-1):
        arrow(ax, nodes[i][0], ys[i]-hs[i]/2, nodes[i+1][0], ys[i+1]+hs[i+1]/2)

    # Skip arrows (to the right)
    skip_targets = [
        (1, 2.2, "Khong"),  # no abilities -> exit
        (2, 2.2, "Khong"),  # wrong trigger
        (3, 2.2, "Dat roi"),
        (4, 2.2, "Khong hop le"),
        (6, 2.2, "Co: skip"),
    ]
    for src_idx, target_y, lbl in skip_targets:
        sx, sy = nodes[src_idx][0]+nodes[src_idx][2]/2, nodes[src_idx][1]
        ax.annotate("", xy=(7.2, target_y), xytext=(sx, sy),
            arrowprops=dict(arrowstyle="->", color=SKIP_COL, lw=1.1,
                            connectionstyle="arc3,rad=-0.3"))
        ax.text(sx+0.1, sy-0.05, lbl, fontsize=7.5, color=SKIP_COL)

    # Side line from all skips to exit
    ax.plot([7.2,7.2],[2.2,11.2], color=SKIP_COL, lw=1.2, ls="--")
    arrow(ax, 7.2, 2.2, nodes[-1][0]+nodes[-1][2]/2, 2.2, color=SKIP_COL, lw=1.2)

    ax.set_title("Hinh 2.11 — Luong TriggerAbility()", fontsize=13, pad=6)
    fig.tight_layout()
    save(fig, "hinh_2_11.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.12 — 14 Trigger Types with 4 Color Groups
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_12():
    groups = [
        ("Combat ca nhan", "#BBDEFB","#1565C0", [
            ("OnAttack",    "Sau khi tan cong va con song",   "Ra-messe (ATK tang)"),
            ("OnTakeDamage","Khi nhan damage va con song",    "Set (phan don)"),
            ("OnDeath",     "Khi chet (qua death stack)",     "Horus, Sekhmet"),
            ("StartOfBattle","Mot lan dau tran",              "Marduk (growth)"),
        ]),
        ("Shop phase", "#DCEDC8","#33691E", [
            ("OnDeploy",    "Khi keo len san",                "Thoth (draw)"),
            ("OnSell",      "Khi bi ban",                     "Isis (buff allies)"),
            ("EndTurnShop", "Dau luot tiep theo",             "Nephthys (coin)"),
        ]),
        ("Phan ung dong minh", "#FFE0B2","#E65100", [
            ("OnAllyDeath",  "Dong minh cung tribe chet",     "Anubis (buff stack)"),
            ("OnAllySummon", "Dong minh duoc trieu hoi",      "Sobek (counter)"),
            ("OnAllyReborn", "Dong minh hoi sinh",            "Osiris (scale)"),
            ("OnAllyDeploy", "Dong minh deploy",              "Hathor"),
            ("OnAllySell",   "Dong minh bi ban",              "Bastet"),
        ]),
        ("Dac biet", "#F8BBD0","#880E4F", [
            ("Aura",        "Dau tran -> AllAlliesExceptSelf","Atlas (passive ATK)"),
            ("OnStatGain",  "Khi nhan chi so vinh vien",      "Chain reaction guard"),
        ]),
    ]

    fig, ax = plt.subplots(figsize=(10, 7.5), facecolor=BG)
    ax.set_facecolor(BG); ax.axis("off")
    ax.set_xlim(0,10); ax.set_ylim(0,9)

    # Headers
    ax.text(5, 8.6, "Hinh 2.12 — Phan Loai 14 Trigger Types", ha="center",
            fontsize=13, weight="bold")

    y_cursor = 8.1
    for grp_name, fc, ec, items in groups:
        # Group header
        ax.add_patch(FancyBboxPatch((0.2, y_cursor-0.35), 9.6, 0.48,
            boxstyle="round,pad=0.05", fc=ec, ec=ec, alpha=0.85))
        ax.text(5, y_cursor-0.10, grp_name, ha="center", va="center",
                fontsize=10, weight="bold", color="white")
        y_cursor -= 0.6

        # Column headers (first row)
        for col_x, col_lbl in [(1.5,"Trigger"),(4.5,"Mo ta"),(8.0,"Vi du card")]:
            ax.text(col_x, y_cursor, col_lbl, ha="center", va="center",
                    fontsize=8.5, weight="bold", color="#444")
        y_cursor -= 0.42

        for trig, desc, card in items:
            ax.add_patch(FancyBboxPatch((0.2, y_cursor-0.28), 9.6, 0.46,
                boxstyle="round,pad=0.03", fc=fc, ec="#DDD", alpha=0.6))
            ax.text(1.5, y_cursor-0.04, trig,  ha="center", va="center", fontsize=8.5,
                    weight="bold", color=ec)
            ax.text(4.5, y_cursor-0.04, desc,  ha="center", va="center", fontsize=8)
            ax.text(8.0, y_cursor-0.04, card,  ha="center", va="center", fontsize=8,
                    color="#555", style="italic")
            y_cursor -= 0.50

        y_cursor -= 0.15

    fig.tight_layout()
    save(fig, "hinh_2_12.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.13 — Pending Summon Queue & Death Stack Sequence Diagram
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_13():
    fig, ax = plt.subplots(figsize=(11, 6.5), facecolor=BG)
    ax.set_facecolor(BG); ax.set_xlim(0,12); ax.set_ylim(0,8); ax.axis("off")

    # Actors
    actors = ["Unit A\n(HP=0)", "FlushDeathStack", "CombatResolver\nTriggerAbility", "PendingSummons\nQueue", "Unit B\n(summoned)", "Unit C\n(enqueued)"]
    xs = [1.0, 3.2, 5.5, 7.8, 10.0, 11.2]
    actor_colors = ["#FCE4EC","#E3F2FD","#FFF9C4","#E8F5E9","#EDE7F6","#EDE7F6"]
    actor_ec     = ["#C62828","#1565C0","#F57F17","#2E7D32","#7B1FA2","#7B1FA2"]

    Y_TOP = 7.2; Y_BOT = 0.5

    for i,(x,lbl) in enumerate(zip(xs,actors)):
        box(ax, x, Y_TOP, 1.55, 0.65, lbl, fc=actor_colors[i], ec=actor_ec[i], fontsize=7.8)
        ax.plot([x,x],[Y_TOP-0.33,Y_BOT], color="#BBB", lw=1.2, ls="--", zorder=1)

    # Events (y from top down)
    events = [
        # (from_x, to_x, y, label, color, style)
        (xs[0], xs[1], 6.3, "HP <= 0: masuk ke deathStack", "#C62828", "->"),
        (xs[1], xs[2], 5.6, "TriggerOnDeath(unitA)", "#1565C0", "->"),
        (xs[2], xs[4], 4.9, "SummonUnit(B) -- lang tuc", "#2E7D32", "->"),
        (xs[2], xs[3], 4.2, "Enqueue(C) vao queue", "#F57F17", "->"),
        (xs[1], xs[2], 3.5, "BroadcastOnAllyDeath", "#1565C0", "->"),
        (xs[1], xs[0], 2.8, "CleanupBoard: xoa A", "#C62828", "<-"),
        (xs[1], xs[3], 2.1, "ProcessNextPendingSummon", "#F57F17", "->"),
        (xs[3], xs[5], 1.5, "pop C -> SummonUnit(C)", "#7B1FA2", "->"),
    ]

    for x0,x1,y,lbl,col,style in events:
        direction = 1 if x1 > x0 else -1
        ax.annotate("", xy=(x1, y), xytext=(x0, y),
            arrowprops=dict(arrowstyle=style, color=col, lw=1.5))
        mid_x = (x0+x1)/2
        ax.text(mid_x, y+0.13, lbl, ha="center", va="bottom", fontsize=7.8,
                color=col, weight="bold" if col=="#C62828" else "normal")

    ax.set_title("Hinh 2.13 — Pending Summon Queue va Death Stack", fontsize=13, y=0.98)
    fig.tight_layout()
    save(fig, "hinh_2_13.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.14 — Drop Rate by Shop Tier (Stacked Bar)
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_14():
    # Full 6×6 table from Ch 3.4
    data = np.array([
        [100, 0,  0,  0,  0,  0],
        [70,  30, 0,  0,  0,  0],
        [50,  35, 15, 0,  0,  0],
        [25,  40, 25, 10, 0,  0],
        [15,  25, 35, 15, 10, 0],
        [10,  15, 20, 25, 20, 10],
    ])
    tier_labels = ["Shop Lv 1","Shop Lv 2","Shop Lv 3","Shop Lv 4","Shop Lv 5","Shop Lv 6"]
    card_tiers  = ["Card Tier 1","Card Tier 2","Card Tier 3","Card Tier 4","Card Tier 5","Card Tier 6"]
    bar_colors  = ["#90CAF9","#A5D6A7","#FFCC80","#F48FB1","#CE93D8","#80DEEA"]

    fig, ax = plt.subplots(figsize=(9, 5.5), facecolor=BG)
    ax.set_facecolor(BG)

    bottoms = np.zeros(6)
    for t in range(6):
        vals = data[:, t]
        bars = ax.bar(range(6), vals, bottom=bottoms, color=bar_colors[t],
                      label=card_tiers[t], width=0.65, edgecolor="white", lw=0.8)
        for j, (b, v) in enumerate(zip(bottoms, vals)):
            if v > 0:
                ax.text(j, b + v/2, f"{v}%", ha="center", va="center",
                        fontsize=8.5, color="#222", weight="bold")
        bottoms += vals

    ax.set_xticks(range(6)); ax.set_xticklabels(tier_labels, fontsize=10)
    ax.set_ylabel("Phan tram xuat hien (%)", fontsize=11)
    ax.set_ylim(0, 110)
    ax.set_title("Hinh 2.14 — Drop Rate Theo Shop Tier", fontsize=13, pad=10)
    ax.legend(loc="upper right", fontsize=9, ncol=2)
    ax.spines[["top","right"]].set_visible(False)
    ax.grid(axis="y", color="#DDDDDD", lw=0.7)
    fig.tight_layout()
    save(fig, "hinh_2_14.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.15 — Tempo vs Economy Trade-off
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_15():
    x = np.linspace(0, 1, 300)

    # Win probability as concave function of spending, shifted by HP
    def curve(peak, width=0.22):
        return np.exp(-((x - peak)**2) / (2*width**2))

    high_hp = curve(0.35) * 0.85   # optimal at moderate spending
    low_hp  = curve(0.70) * 0.90   # optimal at high spending

    fig, ax = plt.subplots(figsize=(8, 4.5), facecolor=BG)
    ax.set_facecolor(BG)

    ax.plot(x, high_hp, color="#1565C0", lw=2.5, label="HP cao (du tru duoc)")
    ax.plot(x, low_hp,  color="#C62828", lw=2.5, label="HP thap (can tan cong ngay)")
    ax.fill_between(x, high_hp, alpha=0.10, color="#1565C0")
    ax.fill_between(x, low_hp,  alpha=0.10, color="#C62828")

    # Optimal points
    for curve_y, col, lbl, yo in [(high_hp,"#1565C0","Toi uu HP cao",0.08),
                                   (low_hp, "#C62828","Toi uu HP thap",0.06)]:
        peak_x = x[np.argmax(curve_y)]
        peak_y = max(curve_y)
        ax.axvline(peak_x, color=col, lw=1.3, ls="--", alpha=0.7)
        ax.annotate(lbl, xy=(peak_x, peak_y),
            xytext=(peak_x+0.07, peak_y+yo),
            arrowprops=dict(arrowstyle="->", color=col, lw=1.2),
            fontsize=9, color=col)

    ax.set_xlabel("Muc do chi tieu som (Economy -> Tempo)", fontsize=11)
    ax.set_ylabel("Xac suat thang uoc tinh", fontsize=11)
    ax.set_title("Hinh 2.15 — Danh Doi Tempo va Economy", fontsize=13, pad=10)
    ax.set_xticks([0, 0.5, 1])
    ax.set_xticklabels(["Tiet kiem toi da\n(Economy)", "Can bang", "Chi tieu toi da\n(Tempo)"])
    ax.set_yticks([])
    ax.legend(fontsize=10)
    ax.spines[["top","right","left"]].set_visible(False)
    ax.grid(axis="x", color="#DDDDDD", lw=0.7)
    fig.tight_layout()
    save(fig, "hinh_2_15.png")


# ══════════════════════════════════════════════════════════════════════════════
# 2.16 — Fitness Structure
# ══════════════════════════════════════════════════════════════════════════════
def hinh_2_16():
    # 3 scenarios
    scenarios = {
        "Thang som\nHP cao": {"base":120, "margin":6*6-0*3, "speed":(20-6)*2},
        "Thang muon\nHP vua":{"base":120, "margin":3*6-2*3, "speed":(20-16)*2},
        "Hoa\nPhong thu":    {"base":70,  "margin":3*6-3*3, "speed":0},
        "Thua sat\nHP can":  {"base":35,  "margin":1*6-5*3, "speed":0},
    }
    labels = list(scenarios.keys())
    bases   = [v["base"]   for v in scenarios.values()]
    margins = [v["margin"] for v in scenarios.values()]
    speeds  = [v["speed"]  for v in scenarios.values()]
    totals  = [b+m+s for b,m,s in zip(bases,margins,speeds)]

    fig, ax = plt.subplots(figsize=(8, 5), facecolor=BG)
    ax.set_facecolor(BG)

    x = np.arange(len(labels))
    w = 0.55

    b1 = ax.bar(x, bases,   width=w, color="#42A5F5", label="Ket qua co ban (thang/hoa/thua)",
                edgecolor="white", lw=0.8)
    b2 = ax.bar(x, margins, width=w, bottom=bases,    color="#66BB6A", label="Bien thang (hpA*6 - hpB*3)",
                edgecolor="white", lw=0.8)
    b3 = ax.bar(x, speeds,  width=w, bottom=[b+m for b,m in zip(bases,margins)],
                color="#FFA726", label="Toc do (MaxTurns-turns)*2",
                edgecolor="white", lw=0.8)

    # Value labels
    for i,(b,m,s,tot) in enumerate(zip(bases,margins,speeds,totals)):
        ax.text(i, b/2,    f"+{b}",  ha="center", va="center", fontsize=9, weight="bold", color="white")
        if abs(m) > 2:
            ax.text(i, b+m/2,  f"{m:+}", ha="center", va="center", fontsize=9, weight="bold", color="white")
        if s > 0:
            ax.text(i, b+m+s/2,f"+{s}", ha="center", va="center", fontsize=9, weight="bold", color="white")
        ax.text(i, tot+3, f"= {tot}", ha="center", va="bottom", fontsize=10, weight="bold", color="#333")

    ax.set_xticks(x); ax.set_xticklabels(labels, fontsize=10)
    ax.set_ylabel("Diem Fitness", fontsize=11)
    ax.set_title("Hinh 2.16 — Cau Truc Ham Fitness", fontsize=13, pad=10)
    ax.legend(fontsize=9, loc="upper right")
    ax.spines[["top","right"]].set_visible(False)
    ax.grid(axis="y", color="#DDDDDD", lw=0.7)
    ax.set_ylim(-20, 250)
    fig.tight_layout()
    save(fig, "hinh_2_16.png")


# ─── Main ─────────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    hinh_2_1()
    hinh_2_2()
    hinh_2_3()
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
    print("Done: 13 hinh da luu.")
