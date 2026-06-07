"""
build_doc.py — Ghép 20 file GDD thành master.md rồi dùng Pandoc tạo output.docx
Chạy: python build_doc.py
"""
import subprocess, sys, os

BASE   = "Assets/Document/00_GDD"
OUT_MD = "output/master.md"
OUT_DOCX = "output/TieuLuan_AutoChess_GA.docx"
REF_DOCX = "output/reference.docx"

# ── Thứ tự file và nhóm chương ────────────────────────────────────────────────
# Phần tử là (filepath, page_break_before)
# page_break_before=True → chèn \newpage trước file (đầu chương mới)
FILES = [
    ("00_MoDau.md",                    False),  # Mở đầu — không break (đầu tài liệu)
    ("01_TongQuanLinhVuc.md",          True),   # Chương 1
    ("02_1_CoSoLyThuyet_GA.md",        True),   # Chương 2 — bắt đầu
    ("02_2_CoSoLyThuyet_Unity.md",     False),  # Chương 2 tiếp
    ("02_3_CoSoLyThuyet_TTE.md",       False),
    ("02_4_CoSoLyThuyet_Economy.md",   False),
    ("03_1_ThietKeGame_TamNhin.md",    True),   # Chương 3
    ("03_2_ThietKeGame_CoreLoop.md",   False),
    ("03_3_ThietKeGame_CardSystem.md", False),
    ("03_4_ThietKeGame_Shop.md",       False),
    ("03_5_ThietKeGame_Economy.md",    False),
    ("03_6_ThietKeGame_Combat.md",     False),
    ("03_7_ThietKeGame_Balancing.md",  False),
    ("03_8_ThietKeGame_UIUX.md",       False),
    ("04_KienTrucHeThong.md",          True),   # Chương 4
    ("05_1_GA_TongQuan_Chromosome.md", True),   # Chương 5
    ("05_2_BotAgent_Simulator_Trainer.md", False),
    ("06_KetQua_DanhGia.md",           True),   # Chương 6
    ("07_KetLuan.md",                  True),   # Kết luận
    ("08_PhuLuc.md",                   True),   # Phụ lục
]

YAML_HEADER = "---\nlang: vi\ntoc: true\ntoc-depth: 3\n---\n\n"

PAGE_BREAK = "\n\n" + chr(92) + "newpage\n\n"   # literal \newpage cho pandoc

def build_master():
    os.makedirs("output", exist_ok=True)
    with open(OUT_MD, "w", encoding="utf-8") as out:
        out.write(YAML_HEADER)
        for i, (fname, break_before) in enumerate(FILES):
            path = os.path.join(BASE, fname)
            with open(path, encoding="utf-8-sig") as f:
                text = f.read().strip()
            if break_before and i > 0:
                out.write(PAGE_BREAK)
            else:
                out.write("\n\n")
            out.write(text)
    with open(OUT_MD, encoding="utf-8") as f:
        content = f.read()
    print(f"[1/3] master.md done ({len(content):,} chars, {content.count(chr(92)+'newpage')} page breaks)")


def make_reference():
    """Tạo reference.docx từ pandoc default rồi patch bằng python-docx."""
    # Lấy default reference.docx từ pandoc
    result = subprocess.run(
        ["pandoc", "--print-default-data-file", "reference.docx"],
        capture_output=True
    )
    if result.returncode != 0:
        print("[WARN] Không lấy được default reference.docx — bỏ qua template")
        return False

    with open(REF_DOCX, "wb") as f:
        f.write(result.stdout)

    # Patch bằng python-docx
    from docx import Document
    from docx.shared import Pt, Cm, RGBColor
    from docx.enum.text import WD_ALIGN_PARAGRAPH

    doc = Document(REF_DOCX)

    # ── Kích thước trang A4, lề học thuật VN ────────────────────────────────
    for sec in doc.sections:
        sec.page_width   = Cm(21)
        sec.page_height  = Cm(29.7)
        sec.top_margin    = Cm(3.0)
        sec.bottom_margin = Cm(2.5)
        sec.left_margin   = Cm(3.5)
        sec.right_margin  = Cm(2.0)

    # ── Helper patch style ───────────────────────────────────────────────────
    def patch(name, size, bold=False, italic=False,
              align=WD_ALIGN_PARAGRAPH.LEFT, space_before=12, space_after=6,
              font_name="Times New Roman", color=None):
        try:
            st = doc.styles[name]
        except KeyError:
            return
        st.font.name       = font_name
        st.font.size       = Pt(size)
        st.font.bold       = bold
        st.font.italic     = italic
        if color:
            st.font.color.rgb = RGBColor(*color)
        pf = st.paragraph_format
        pf.alignment    = align
        pf.space_before = Pt(space_before)
        pf.space_after  = Pt(space_after)
        pf.line_spacing = Pt(size * 1.5)

    patch("Heading 1", 14, bold=True,
          align=WD_ALIGN_PARAGRAPH.CENTER, space_before=18, space_after=12)
    patch("Heading 2", 13, bold=True,
          space_before=12, space_after=6)
    patch("Heading 3", 12, bold=True, italic=True,
          space_before=10, space_after=4)
    patch("Normal",    13,
          space_before=0, space_after=6)
    patch("Body Text", 13,
          space_before=0, space_after=6)

    # Body text: cách dòng 1.5 và thụt đầu dòng
    try:
        st = doc.styles["Normal"]
        st.paragraph_format.first_line_indent = Cm(1.27)
        st.paragraph_format.line_spacing      = Pt(19.5)
    except Exception:
        pass

    # Code block: Courier New 10pt không in đậm
    for cname in ["Source Code", "Verbatim Char", "Compact"]:
        try:
            st = doc.styles[cname]
            st.font.name = "Courier New"
            st.font.size = Pt(10)
            st.font.bold = False
        except KeyError:
            pass

    doc.save(REF_DOCX)
    print(f"[2/3] reference.docx đã tạo và patch xong")
    return True


def run_pandoc(has_ref):
    cmd = [
        "pandoc", OUT_MD,
        "-o", OUT_DOCX,
        "--toc", "--toc-depth=3",
        "-V", "lang=vi",
        "--highlight-style=kate",
        "-f", "markdown+smart+tex_math_dollars",
        "--wrap=none",
    ]
    if has_ref:
        cmd += ["--reference-doc", REF_DOCX]

    print(f"[3/3] Chạy pandoc...")
    result = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8")
    if result.returncode == 0:
        size = os.path.getsize(OUT_DOCX) // 1024
        print(f"      OK → {OUT_DOCX}  ({size} KB)")
    else:
        print(f"      LỖI:\n{result.stderr}")
        sys.exit(1)


if __name__ == "__main__":
    build_master()
    has_ref = make_reference()
    run_pandoc(has_ref)
    print("\nHoàn tất. Mở output/TieuLuan_AutoChess_GA.docx để kiểm tra.")
