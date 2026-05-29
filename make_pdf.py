#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
LAST BRAKE 기획서 PDF 생성 스크립트
ReportLab 사용 — NotoSansKR 폰트로 한글 완전 지원
"""

from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.lib.units import mm
from reportlab.lib.styles import ParagraphStyle
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle,
    HRFlowable, KeepTogether, PageBreak
)
from reportlab.platypus.flowables import Flowable
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.enums import TA_LEFT, TA_CENTER, TA_RIGHT
import os, sys

# ── 폰트 등록 ───────────────────────────────────────────────
FONT_PATH = "/Users/rohdaeyoung/Project File/my-code/LAST_BRAKE/Assets/TextMesh Pro/Fonts/NotoSansKR-Regular.ttf"
pdfmetrics.registerFont(TTFont("NotoKR", FONT_PATH))
pdfmetrics.registerFont(TTFont("NotoKR-B", FONT_PATH))  # bold fallback

# ── 출력 경로 ────────────────────────────────────────────────
OUT = "/Users/rohdaeyoung/Project File/my-code/LAST_BRAKE/LAST_BRAKE_기획서.pdf"

# ── 색상 ────────────────────────────────────────────────────
C_BG       = colors.HexColor("#0D0D1A")
C_CARD     = colors.HexColor("#16163A")
C_BORDER   = colors.HexColor("#2A2A5A")
C_GOLD     = colors.HexColor("#F5D66E")
C_GOLD2    = colors.HexColor("#C9A227")
C_WHITE    = colors.HexColor("#FFFFFF")
C_TEXT     = colors.HexColor("#DDE3F0")
C_MUTED    = colors.HexColor("#7A88AA")
C_RED      = colors.HexColor("#FF6B6B")
C_GREEN    = colors.HexColor("#4CAF73")
C_BLUE     = colors.HexColor("#5C85D6")
C_PURPLE   = colors.HexColor("#9C6FCE")
C_PLUS     = colors.HexColor("#6DE898")
C_MINUS    = colors.HexColor("#FF8A8A")
C_GOOD     = colors.HexColor("#6DE898")
C_NORMAL   = colors.HexColor("#8AB4F8")
C_BAD      = colors.HexColor("#FF8A8A")
C_TRUE     = colors.HexColor("#C9A0F8")
C_TINT     = colors.HexColor("#1A1A36")
C_HEADER   = colors.HexColor("#1E1E42")

W, H = A4  # 595 x 842 pt
M = 22*mm   # 좌우 여백

# ── 스타일 ───────────────────────────────────────────────────
def S(name, **kw):
    kw.setdefault("fontName", "NotoKR")
    s = ParagraphStyle(name, **kw)
    return s

ST = {
    "body"    : S("body",    fontSize=9,  leading=16, textColor=C_TEXT,  spaceBefore=2),
    "muted"   : S("muted",   fontSize=8,  leading=14, textColor=C_MUTED),
    "h1"      : S("h1",      fontSize=22, leading=30, textColor=C_WHITE, spaceAfter=4),
    "h2"      : S("h2",      fontSize=14, leading=20, textColor=C_WHITE, spaceBefore=10, spaceAfter=4),
    "h3"      : S("h3",      fontSize=10, leading=16, textColor=C_GOLD,  spaceBefore=6,  spaceAfter=3),
    "gold"    : S("gold",    fontSize=9,  leading=15, textColor=C_GOLD),
    "italic"  : S("italic",  fontSize=10, leading=18, textColor=colors.HexColor("#D4C080"),
                   spaceAfter=4, leftIndent=12, borderPad=6),
    "cover_sub": S("cover_sub", fontSize=13, leading=20, textColor=colors.HexColor("#9EA8C8"), alignment=TA_CENTER),
    "center"  : S("center",  fontSize=9,  leading=16, textColor=C_TEXT, alignment=TA_CENTER),
    "sec_num" : S("sec_num", fontSize=8,  leading=12, textColor=C_BG, alignment=TA_CENTER),
    "code"    : S("code",    fontSize=8,  leading=13, textColor=colors.HexColor("#B0BCE0"),
                   backColor=colors.HexColor("#0A0A1F"), leftIndent=8, rightIndent=8,
                   borderPad=6, fontName="NotoKR"),
    "plus"    : S("plus",    fontSize=9, leading=14, textColor=C_PLUS, alignment=TA_CENTER),
    "minus"   : S("minus",   fontSize=9, leading=14, textColor=C_MINUS, alignment=TA_CENTER),
    "zero"    : S("zero",    fontSize=9, leading=14, textColor=C_MUTED, alignment=TA_CENTER),
}

def P(text, style="body", **kw):
    s = ST[style]
    if kw:
        from copy import copy
        s = copy(s)
        for k, v in kw.items():
            setattr(s, k, v)
    return Paragraph(text, s)

def SP(n=4): return Spacer(1, n)

def HR(col=C_BORDER, t=0.5):
    return HRFlowable(width="100%", thickness=t, color=col, spaceAfter=6, spaceBefore=6)

# ── 섹션 제목 ────────────────────────────────────────────────
def sec_title(num, title):
    num_data = [[P(str(num), "sec_num")]]
    num_tbl = Table(num_data, colWidths=[7*mm], rowHeights=[7*mm])
    num_tbl.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (-1,-1), C_GOLD2),
        ("ROUNDEDCORNERS", [4]),
        ("VALIGN",     (0,0), (-1,-1), "MIDDLE"),
    ]))
    row = [[num_tbl, P(title, "h2", spaceBefore=0, spaceAfter=0)]]
    t = Table(row, colWidths=[10*mm, W - M*2 - 10*mm])
    t.setStyle(TableStyle([
        ("VALIGN",      (0,0), (-1,-1), "MIDDLE"),
        ("LEFTPADDING", (1,0), (1,0),  8),
        ("LINEBELOW",   (0,0), (-1,-1), 0.5, C_BORDER),
        ("BOTTOMPADDING",(0,0),(-1,-1), 6),
    ]))
    return [SP(10), t, SP(8)]

# ── 카드 박스 ────────────────────────────────────────────────
def card(content_rows, title=None):
    items = []
    if title:
        items.append([P(title, "h3", spaceBefore=0, spaceAfter=0)])
    items += [[c] for c in content_rows]
    t = Table(items, colWidths=[W - M*2 - 14*mm])
    ts = [
        ("BACKGROUND",   (0,0), (-1,-1), C_CARD),
        ("BOX",          (0,0), (-1,-1), 0.5, C_BORDER),
        ("ROUNDEDCORNERS", [6]),
        ("TOPPADDING",   (0,0), (-1,-1), 6),
        ("BOTTOMPADDING",(0,0), (-1,-1), 6),
        ("LEFTPADDING",  (0,0), (-1,-1), 12),
        ("RIGHTPADDING", (0,0), (-1,-1), 12),
    ]
    t.setStyle(TableStyle(ts))
    return [t, SP(8)]

# ── 일반 표 ──────────────────────────────────────────────────
def tbl(headers, rows, col_widths=None, row_colors=None):
    usable = W - M*2 - 14*mm
    if col_widths is None:
        col_widths = [usable / len(headers)] * len(headers)

    header_row = [P(h, "gold", fontSize=8, leading=12, spaceBefore=0, spaceAfter=0) for h in headers]
    data = [header_row]
    for row in rows:
        data.append([P(str(c), "body", fontSize=8.5, leading=13, spaceBefore=0, spaceAfter=0) for c in row])

    t = Table(data, colWidths=col_widths)
    ts = [
        ("BACKGROUND",    (0,0), (-1,0),  C_HEADER),
        ("LINEBELOW",     (0,0), (-1,0),  1, C_BORDER),
        ("ROWBACKGROUNDS",(0,1), (-1,-1), [C_CARD, C_TINT]),
        ("BOX",           (0,0), (-1,-1), 0.5, C_BORDER),
        ("TOPPADDING",    (0,0), (-1,-1), 5),
        ("BOTTOMPADDING", (0,0), (-1,-1), 5),
        ("LEFTPADDING",   (0,0), (-1,-1), 8),
        ("RIGHTPADDING",  (0,0), (-1,-1), 8),
        ("VALIGN",        (0,0), (-1,-1), "MIDDLE"),
    ]
    if row_colors:
        for (r, c) in row_colors:
            ts.append(("TEXTCOLOR", (c, r+1), (c, r+1),
                       C_PLUS if rows[r][c].startswith("+") else
                       C_MINUS if rows[r][c].startswith("−") else C_MUTED))
    t.setStyle(TableStyle(ts))
    return [t, SP(8)]

# ── 수치 delta 표 ────────────────────────────────────────────
def delta_tbl(rows):
    usable = W - M*2 - 14*mm
    cw = [usable*0.46, usable*0.18, usable*0.18, usable*0.18]
    hdr = ["선택지", "INT", "RISK", "ADDICT"]
    data = [[P(h, "gold", fontSize=8, leading=12, spaceBefore=0, spaceAfter=0) for h in hdr]]
    for (label, i, r, a) in rows:
        def cell(v):
            if isinstance(v, str): txt = v
            elif v > 0: txt = f"+{v}"
            elif v < 0: txt = f"−{abs(v)}"
            else: txt = "0"
            if txt.startswith("+"):
                return P(txt, "plus", fontSize=9, leading=13, spaceBefore=0, spaceAfter=0)
            elif txt.startswith("−"):
                return P(txt, "minus", fontSize=9, leading=13, spaceBefore=0, spaceAfter=0)
            else:
                return P(txt, "zero", fontSize=9, leading=13, spaceBefore=0, spaceAfter=0)
        data.append([
            P(label, "body", fontSize=8.5, leading=13, spaceBefore=0, spaceAfter=0),
            cell(i), cell(r), cell(a)
        ])
    t = Table(data, colWidths=cw)
    t.setStyle(TableStyle([
        ("BACKGROUND",    (0,0), (-1,0),  C_HEADER),
        ("LINEBELOW",     (0,0), (-1,0),  1, C_BORDER),
        ("ROWBACKGROUNDS",(0,1), (-1,-1), [C_CARD, C_TINT]),
        ("BOX",           (0,0), (-1,-1), 0.5, C_BORDER),
        ("TOPPADDING",    (0,0), (-1,-1), 5),
        ("BOTTOMPADDING", (0,0), (-1,-1), 5),
        ("LEFTPADDING",   (0,0), (-1,-1), 8),
        ("RIGHTPADDING",  (0,0), (-1,-1), 8),
        ("VALIGN",        (0,0), (-1,-1), "MIDDLE"),
        ("ALIGN",         (1,0), (-1,-1), "CENTER"),
    ]))
    return [t, SP(8)]

# ── 정보 박스 ────────────────────────────────────────────────
def info_box(text, bg=colors.HexColor("#0D1E3A"), border=colors.HexColor("#2563EB"), tc=colors.HexColor("#9AB5E8")):
    t = Table([[P(text, "body", textColor=tc, fontSize=8.5, leading=14, spaceBefore=0, spaceAfter=0)]],
              colWidths=[W - M*2 - 14*mm])
    t.setStyle(TableStyle([
        ("BACKGROUND",   (0,0), (-1,-1), bg),
        ("BOX",          (0,0), (-1,-1), 0.8, border),
        ("ROUNDEDCORNERS", [5]),
        ("TOPPADDING",   (0,0), (-1,-1), 8),
        ("BOTTOMPADDING",(0,0), (-1,-1), 8),
        ("LEFTPADDING",  (0,0), (-1,-1), 12),
        ("RIGHTPADDING", (0,0), (-1,-1), 12),
    ]))
    return [t, SP(8)]

# ── 엔딩 카드 그리드 ─────────────────────────────────────────
def ending_grid():
    usable = W - M*2 - 14*mm
    cw = [(usable - 6*mm) / 2] * 2
    def ec(emoji, title, badge_col, cond, desc):
        inner = Table([
            [P(f"{emoji}  {title}", "h3", spaceBefore=0, spaceAfter=2, textColor=badge_col)],
            [P(cond, "muted", fontSize=7.5, leading=12, spaceBefore=0, spaceAfter=3)],
            [P(desc, "body", fontSize=8, leading=13, spaceBefore=0, spaceAfter=0)],
        ], colWidths=[cw[0] - 14*mm])
        inner.setStyle(TableStyle([
            ("BACKGROUND",   (0,0), (-1,-1), C_CARD),
            ("LINEABOVE",    (0,0), (-1,0),  2, badge_col),
            ("BOX",          (0,0), (-1,-1), 0.5, C_BORDER),
            ("TOPPADDING",   (0,0), (-1,-1), 8),
            ("BOTTOMPADDING",(0,0), (-1,-1), 8),
            ("LEFTPADDING",  (0,0), (-1,-1), 10),
            ("RIGHTPADDING", (0,0), (-1,-1), 10),
        ]))
        return inner
    row1 = [
        ec("🌅", "굿 엔딩 「새벽의 선택」", C_GOOD,
           "INT ≥ 75  AND  ADDICT ≤ 40",
           "도윤이 스스로 멈출 수 있었던 결말. 회복과 새 출발."),
        ec("🌙", "노멀 엔딩 「연락처 삭제」", C_NORMAL,
           "INT ≤ 50  AND  RISK ≥ 60",
           "상처를 안고 일상으로 돌아가는 결말. 서아의 역할이 중요."),
    ]
    row2 = [
        ec("🌑", "배드 엔딩 「끝나지 않는 밤」", C_BAD,
           "ADDICT ≥ 80",
           "완전히 무너진 결말. 체포와 붕괴. 경찰 사이렌 SFX."),
        ec("👁", "트루 엔딩", C_TRUE,
           "세 엔딩 모두 클리어 후 재시작",
           "도윤이 플레이어를 직접 바라보는 4번째 벽 파괴."),
    ]
    t = Table([row1, [SP(4), SP(4)], row2], colWidths=cw, rowHeights=[None, 4, None])
    t.setStyle(TableStyle([
        ("LEFTPADDING",  (0,0), (-1,-1), 3),
        ("RIGHTPADDING", (0,0), (-1,-1), 3),
        ("TOPPADDING",   (0,0), (-1,-1), 0),
        ("BOTTOMPADDING",(0,0), (-1,-1), 0),
    ]))
    return [t, SP(8)]

# ── 1393 CTA 박스 ─────────────────────────────────────────────
def cta_box():
    content = [
        [P("게임 마지막, 플레이어에게 실제로 연결되는 선택지", "muted", alignment=TA_CENTER, spaceBefore=0, spaceAfter=4)],
        [P("☎  1393", "h1", alignment=TA_CENTER, textColor=C_RED, fontSize=32, leading=40, spaceBefore=0, spaceAfter=4)],
        [P("정신건강위기상담전화 — 24시간 무료 운영", "body", alignment=TA_CENTER, textColor=colors.HexColor("#FFB0B0"), spaceBefore=0, spaceAfter=4)],
        [P('선택지 ②  "누군가에게 연락한다"  →  tel:1393 으로 즉시 연결', "muted", fontSize=8, alignment=TA_CENTER, spaceBefore=0, spaceAfter=0)],
    ]
    t = Table(content, colWidths=[W - M*2 - 14*mm])
    t.setStyle(TableStyle([
        ("BACKGROUND",   (0,0), (-1,-1), colors.HexColor("#1A0505")),
        ("BOX",          (0,0), (-1,-1), 1.5, colors.HexColor("#E53935")),
        ("ROUNDEDCORNERS", [8]),
        ("TOPPADDING",   (0,0), (-1,-1), 10),
        ("BOTTOMPADDING",(0,0), (-1,-1), 10),
        ("LEFTPADDING",  (0,0), (-1,-1), 16),
        ("RIGHTPADDING", (0,0), (-1,-1), 16),
    ]))
    return [t, SP(8)]

# ── 커버 페이지 ──────────────────────────────────────────────
def cover_page():
    els = []
    els.append(SP(30))

    # 배지
    badge_t = Table([[P("GAME DESIGN DOCUMENT  ·  2026", "muted", alignment=TA_CENTER, fontSize=9)]],
                    colWidths=[W - M*2])
    badge_t.setStyle(TableStyle([
        ("BACKGROUND", (0,0),(-1,-1), colors.HexColor("#1A0A3A")),
        ("BOX",        (0,0),(-1,-1), 0.5, colors.HexColor("#4A3090")),
        ("ROUNDEDCORNERS", [12]),
        ("TOPPADDING",    (0,0),(-1,-1), 5),
        ("BOTTOMPADDING", (0,0),(-1,-1), 5),
    ]))
    els.append(badge_t)
    els.append(SP(20))

    # 타이틀
    els.append(P("LAST BRAKE", "h1", fontSize=40, leading=48, alignment=TA_CENTER,
                  textColor=C_GOLD, spaceBefore=0, spaceAfter=0))
    els.append(P("끝나지 않는 밤", "cover_sub", fontSize=16, spaceBefore=4, spaceAfter=20))

    # 구분선
    els.append(HRFlowable(width="40%", thickness=0.8, color=C_GOLD2,
                           hAlign="CENTER", spaceBefore=4, spaceAfter=20))

    # 핵심 메시지
    msg_t = Table([[P('"중독은 특별한 사람이 아니라,\n반복된 선택에서 시작됩니다."',
                      "italic", fontSize=11, leading=18, alignment=TA_CENTER,
                      spaceBefore=0, spaceAfter=0)]],
                  colWidths=[W - M*2 - 40*mm])
    msg_t.setStyle(TableStyle([
        ("BACKGROUND",   (0,0),(-1,-1), colors.HexColor("#1A1200")),
        ("LINEABOVE",    (0,0),(-1,0),  0, C_BG),
        ("LINEBEFORE",   (0,0),(0,-1),  2, C_GOLD2),
        ("TOPPADDING",   (0,0),(-1,-1), 10),
        ("BOTTOMPADDING",(0,0),(-1,-1), 10),
        ("LEFTPADDING",  (0,0),(-1,-1), 16),
        ("RIGHTPADDING", (0,0),(-1,-1), 16),
        ("ALIGN",        (0,0),(-1,-1), "CENTER"),
    ]))
    els.append(msg_t)
    els.append(SP(30))

    # 메타 정보
    meta = [
        ["장르",   "선택형 비주얼 노벨 (Visual Novel)"],
        ["엔진",   "Unity 6 (6000.0.73f1)  ·  URP"],
        ["플랫폼", "PC — Windows / macOS / Linux"],
        ["엔딩",   "4종 (Good · Normal · Bad · True)"],
        ["개발",   "롭대영"],
        ["마감",   "2026년 6월 5일"],
    ]
    usable = W - M*2
    meta_data = [[P(k, "gold", fontSize=9, spaceBefore=0, spaceAfter=0),
                  P(v, "body", fontSize=9, spaceBefore=0, spaceAfter=0)] for k, v in meta]
    mt = Table(meta_data, colWidths=[usable*0.22, usable*0.78])
    mt.setStyle(TableStyle([
        ("BACKGROUND",    (0,0), (-1,-1), C_CARD),
        ("ROWBACKGROUNDS",(0,0), (-1,-1), [C_CARD, C_TINT]),
        ("BOX",           (0,0), (-1,-1), 0.5, C_BORDER),
        ("LINEBELOW",     (0,0), (-1,-2), 0.3, C_BORDER),
        ("TOPPADDING",    (0,0), (-1,-1), 7),
        ("BOTTOMPADDING", (0,0), (-1,-1), 7),
        ("LEFTPADDING",   (0,0), (-1,-1), 12),
        ("RIGHTPADDING",  (0,0), (-1,-1), 12),
        ("VALIGN",        (0,0), (-1,-1), "MIDDLE"),
    ]))
    els.append(mt)
    els.append(PageBreak())
    return els

# ── 배경 그리기 (콜백) ───────────────────────────────────────
def draw_bg(canvas, doc):
    canvas.saveState()
    canvas.setFillColor(C_BG)
    canvas.rect(0, 0, W, H, fill=1, stroke=0)
    canvas.restoreState()

# ════════════════════════════════════════════════════════════
#  본문 빌드
# ════════════════════════════════════════════════════════════
def build():
    doc = SimpleDocTemplate(
        OUT, pagesize=A4,
        leftMargin=M, rightMargin=M,
        topMargin=16*mm, bottomMargin=16*mm,
        title="LAST BRAKE 기획서",
        author="rohdaeyoung",
    )

    els = []

    # ── 커버 ──────────────────────────────────────────────────
    els += cover_page()

    # ── 1. 기획 의도 ──────────────────────────────────────────
    els += sec_title(1, "기획 의도")
    els += card([
        P("<b>왜 이 주제인가</b>", "h3", spaceBefore=0, spaceAfter=4),
        P("한국 청년층의 약물 관련 사건은 해마다 증가하고 있습니다. 그러나 '약물 중독'은 여전히 "
          "사회적 금기로 취급되어, 정작 예방 교육이 가장 필요한 10~20대에게 충분히 닿지 못하고 있습니다."),
        SP(4),
        P("LAST BRAKE는 플레이어가 직접 선택의 주체가 되어 '왜 사람들이 약물에 빠져드는가'를 "
          "체험적으로 이해하도록 설계된 게임입니다. 도덕적 훈계가 아닌, "
          "반복된 작은 선택이 어떻게 큰 결과를 만드는지를 게임이라는 매체로 전달합니다."),
    ])
    els += card([
        P("<b>게임이 아닌 게임</b>", "h3", spaceBefore=0, spaceAfter=4),
        P("이 작품은 단순한 오락 게임이 아닙니다. 마지막 장면에서 플레이어는 "
          "자신의 선택 결과 수치를 직면하고, 세 가지 선택지 중 하나로 "
          "실제 정신건강위기상담전화(1393)에 연결할 수 있습니다."),
        SP(4),
        P("게임을 끝낸 플레이어가 현실의 누군가를 떠올리는 것, 그것이 목표입니다.",
          "muted"),
    ])

    # ── 2. 게임 개요 ──────────────────────────────────────────
    els += sec_title(2, "게임 개요")
    usable = W - M*2 - 14*mm
    els += tbl(
        ["항목", "내용"],
        [
            ["제목",     "LAST BRAKE: 끝나지 않는 밤"],
            ["장르",     "선택형 비주얼 노벨 (Visual Novel)"],
            ["테마",     "청년 약물 중독 인식 개선"],
            ["플랫폼",   "PC — Windows / macOS / Linux"],
            ["플레이시간","약 20~30분 (1회차 기준)"],
            ["엔딩 수",  "4종 (Good · Normal · Bad · True)"],
            ["언어",     "한국어"],
            ["엔진",     "Unity 6 (6000.0.73f1) · URP · TextMeshPro"],
            ["대상 연령","15세 이상 권장"],
        ],
        col_widths=[usable*0.25, usable*0.75],
    )

    # ── 3. 등장인물 ───────────────────────────────────────────
    els += sec_title(3, "등장인물")
    char_data = [
        ["도윤 (주인공)",   "평범한 대학생. 모든 선택이 도윤의 운명을 결정한다.\n클럽 문화에 처음 발을 들이며 이야기가 시작된다."],
        ["민재 (선배)",     "도윤을 약물 세계로 끌어들이는 인물.\n처음엔 친근하게 접근하지만 점점 요구가 커진다."],
        ["하준 (친구)",     "도윤의 친구. 위험을 경고하는 존재.\n이성적 선택지의 근거가 되는 인물."],
        ["서아",            "도윤을 걱정하며 곁에 있으려는 인물.\n노멀 엔딩에서 중요한 역할을 맡는다."],
    ]
    cw2 = [usable*0.25, usable*0.75]
    c_tbl = Table(
        [[P(n, "gold", fontSize=9, spaceBefore=0, spaceAfter=0),
          P(d, "body", fontSize=8.5, leading=14, spaceBefore=0, spaceAfter=0)]
         for n, d in char_data],
        colWidths=cw2
    )
    c_tbl.setStyle(TableStyle([
        ("ROWBACKGROUNDS",(0,0),(-1,-1),[C_CARD, C_TINT]),
        ("BOX",          (0,0),(-1,-1), 0.5, C_BORDER),
        ("LINEBELOW",    (0,0),(-1,-2), 0.3, C_BORDER),
        ("TOPPADDING",   (0,0),(-1,-1), 7),
        ("BOTTOMPADDING",(0,0),(-1,-1), 7),
        ("LEFTPADDING",  (0,0),(-1,-1), 10),
        ("RIGHTPADDING", (0,0),(-1,-1), 10),
        ("VALIGN",       (0,0),(-1,-1), "TOP"),
    ]))
    els += [c_tbl, SP(8)]

    # ── 4. 스토리 구조 ────────────────────────────────────────
    els += sec_title(4, "스토리 구조")
    els += tbl(
        ["씬 파일", "내용"],
        [
            ["01_Step1_Prologue", "프롤로그 — 민재의 클럽 초대. 첫 번째 선택의 출발점"],
            ["02_Step2_Club",     "클럽 — 약물 첫 접촉. '한 번쯤은 괜찮겠지'의 시작"],
            ["03_Step3_Morning",  "다음날 아침 — 현실 직시 vs 회피"],
            ["04_Step4_Party",    "파티 — 더 깊어지는 중독의 갈림길"],
            ["05_Step5_Collapse", "무너짐 — 최후의 선택, 엔딩 분기 결정"],
            ["07_GoodEnd",        "굿 엔딩 「새벽의 선택」"],
            ["08_NormalEnd",      "노멀 엔딩 「연락처 삭제」"],
            ["09_BadEnd",         "배드 엔딩 「끝나지 않는 밤」"],
            ["10_TrueEnd",        "트루 엔딩 — 세 엔딩 클리어 후 해금. 4번째 벽 파괴"],
        ],
        col_widths=[usable*0.35, usable*0.65],
    )

    # ── 5. 게임플레이 시스템 ──────────────────────────────────
    els += sec_title(5, "게임플레이 시스템")
    els += tbl(
        ["수치", "색상", "초기값", "의미"],
        [
            ["INT (판단력)",    "초록", "100", "높을수록 이성적 선택 가능. ADDICT 60+ 시 특정 선택지 잠금"],
            ["RISK (위험도)",   "주황",   "0", "높을수록 위험한 상황에 노출. 60+ 시 HUD 빨간색"],
            ["ADDICT (의존도)", "보라",   "0", "높을수록 약물 의존. 60+ 시 강제 선택 메커닉 발동"],
        ],
        col_widths=[usable*0.2, usable*0.1, usable*0.1, usable*0.6],
    )

    els.append(P("Step 1 — 프롤로그", "h3"))
    els += delta_tbl([
        ("집에 갈게요",      10, -5,  0),
        ("가볼까? (클럽 동행)", -10, 15,  5),
    ])

    els.append(P("Step 2 — 클럽", "h3"))
    els += delta_tbl([
        ("거절한다",        15, -5,  0),
        ("한 번만 해본다",   0,  0,  5),
        ("적극적으로 즐긴다", -20, 10, 20),
    ])

    els.append(P("Step 3 — 다음날 아침", "h3"))
    els += delta_tbl([
        ("끊겠다고 결심",   15,  0, -5),
        ("다시 연락한다",   -5,  5, 10),
        ("더 구한다",      -10, 10, 20),
    ])

    els.append(P("Step 4 — 파티", "h3"))
    els += delta_tbl([
        ("함께한다",          -10, 10, 10),
        ("더 강한 걸 요구",   -20, 15, 25),
        ("민재 말을 따른다",  -15, 10, 30),
    ])

    els.append(P("Step 5 — 무너짐 (엔딩 결정)", "h3"))
    cw5 = [usable*0.35, usable*0.2, usable*0.15, usable*0.15, usable*0.15]
    step5_hdr = ["선택지", "조건", "INT", "RISK", "ADDICT"]
    step5_rows = [
        ("경찰에 신고한다",   "INT ≥ 70 필요", 25, -20, -20),
        ("다 끝났어... (포기)", "없음",         -15,  10,   5),
        ("형 시키는 거 다 할게", "강제 배드",    -25,  20,  30),
    ]
    def delta_cell(v):
        if isinstance(v, str): return P(v, "muted", fontSize=8, spaceBefore=0, spaceAfter=0, alignment=TA_CENTER)
        if v > 0: return P(f"+{v}", "plus", fontSize=9, spaceBefore=0, spaceAfter=0)
        elif v < 0: return P(f"−{abs(v)}", "minus", fontSize=9, spaceBefore=0, spaceAfter=0)
        else: return P("0", "zero", fontSize=9, spaceBefore=0, spaceAfter=0)
    s5_data = [[P(h, "gold", fontSize=8, leading=12, spaceBefore=0, spaceAfter=0) for h in step5_hdr]]
    for (label, cond, i, r, a) in step5_rows:
        s5_data.append([
            P(label, "body", fontSize=8.5, spaceBefore=0, spaceAfter=0),
            P(cond, "muted", fontSize=8, spaceBefore=0, spaceAfter=0),
            delta_cell(i), delta_cell(r), delta_cell(a)
        ])
    s5t = Table(s5_data, colWidths=cw5)
    s5t.setStyle(TableStyle([
        ("BACKGROUND",    (0,0),(-1,0),  C_HEADER),
        ("LINEBELOW",     (0,0),(-1,0),  1, C_BORDER),
        ("ROWBACKGROUNDS",(0,1),(-1,-1), [C_CARD, C_TINT]),
        ("BOX",           (0,0),(-1,-1), 0.5, C_BORDER),
        ("TOPPADDING",    (0,0),(-1,-1), 5),
        ("BOTTOMPADDING", (0,0),(-1,-1), 5),
        ("LEFTPADDING",   (0,0),(-1,-1), 8),
        ("RIGHTPADDING",  (0,0),(-1,-1), 8),
        ("VALIGN",        (0,0),(-1,-1), "MIDDLE"),
        ("ALIGN",         (2,0),(-1,-1), "CENTER"),
    ]))
    els += [s5t, SP(8)]

    els += info_box(
        "강제 선택 메커닉: ADDICT ≥ 60이 되면 이성적 선택지가 비활성화(빨간색 틴팅)되고, "
        "1.2초 후 나쁜 선택지가 자동으로 클릭되는 애니메이션이 재생됩니다. "
        "의존도에 의해 선택권을 잃어가는 상태를 시각·청각으로 표현합니다.",
        bg=colors.HexColor("#1A0A0A"),
        border=colors.HexColor("#C84040"),
        tc=colors.HexColor("#F0A0A0"),
    )

    # ── 6. 엔딩 분기 ──────────────────────────────────────────
    els += sec_title(6, "엔딩 분기")
    els += ending_grid()

    els += tbl(
        ["단계", "내용", "대상"],
        [
            ["Phase 1 · 메시지",  "BGM 페이드 → 도윤 응시 애니메이션 → 흑백화 → 핵심 메시지 → 암전", "전 엔딩"],
            ["Phase FACE · 응시", "화면 가득 도윤 얼굴 + 글리치 + 표정 변화 → 탭으로 다음 단계",  "TrueEnd 전용"],
            ["Phase 2 · 결말",    "스탯 수치 공개 애니메이션 → '이래도 계속 하겠습니까?' → 선택지 3개", "전 엔딩"],
        ],
        col_widths=[usable*0.25, usable*0.55, usable*0.2],
    )

    # ── 7. 시각 효과 ──────────────────────────────────────────
    els += sec_title(7, "시각 효과 시스템")
    els += tbl(
        ["효과", "강도", "용도"],
        [
            ["RedWarning",       "붉은 플래시 (0.15s in / 0.45s out)",     "위험한 선택 직후 충격 연출"],
            ["Glitch (상시)",    "alpha 0.02~0.06, 미세 정적",               "ADDICT 상승 시 배경 분위기"],
            ["GlitchBurst",      "alpha 최대 0.45, 1.5~2.5초 자동 종료",   "극적 순간 번쩍임"],
            ["BlurPulse",        "3회 반복 페이드",                          "정신 혼란 · 약효 묘사"],
            ["Vignette",         "intensity 0.8 강화",                      "긴장감 고조 구간"],
            ["BreathingVignette","0.25↔0.50, 5초 주기 cos 파형",            "Phase 2 조용한 중독감 표현"],
        ],
        col_widths=[usable*0.25, usable*0.35, usable*0.4],
    )
    els += tbl(
        ["소품", "등장 상황", "SFX"],
        [
            ["스마트폰",  "민재의 문자 수신",        "SFX_Phone_Notify"],
            ["시계 07:30","다음날 아침",              "(조용함)"],
            ["약병",      "약물 등장 장면",           "SFX_Pill_Rattle"],
            ["채팅 창",   "카카오톡 형식 대화",       "SFX_Phone_Send"],
            ["서류",      "경찰 신고서",              "SFX_ReportPaper"],
        ],
        col_widths=[usable*0.2, usable*0.4, usable*0.4],
    )

    # ── 8. 사운드 디자인 ──────────────────────────────────────
    els += sec_title(8, "사운드 디자인")
    els += tbl(
        ["BGM 트랙", "분위기", "재생 씬"],
        [
            ["BGM_Normal_DarkAmbient", "어두운 앰비언트",   "기본 씬 전체"],
            ["BGM_Club_Muffled",       "먹먹한 클럽음",     "Step2 클럽 씬"],
            ["BGM_Distorted_Risk",     "왜곡된 위기음",     "RISK 고조 씬"],
        ],
        col_widths=[usable*0.38, usable*0.3, usable*0.32],
    )
    els += tbl(
        ["카테고리", "클립 (총 34개)", "재생 시점"],
        [
            ["UI",       "SFX_UI_Select / DangerConfirm / ForcedClick / Locked / StatReveal",
             "선택지 클릭 · 강제 · 잠금 · 수치 공개"],
            ["신체 반응","SFX_Heartbeat_Ramp/Slow · Tinnitus · Breath_Panic · Electric_Tremor",
             "약효 및 중독 상태 표현"],
            ["소품",     "SFX_Pill_Rattle/Open · Glass_Clink · Door_Knock · ReportPaper",
             "소품 컷인 각각 연동"],
            ["전화",     "SFX_Phone_Notify/Send · Dial1393",
             "스마트폰 컷인 · 1393 선택"],
            ["엔딩",     "SFX_Police_Siren · TrueEnd_StareLoop/TapCut · Ending_Restart/Quit",
             "배드엔딩 · 트루엔딩 · 선택지 3종"],
        ],
        col_widths=[usable*0.18, usable*0.46, usable*0.36],
    )
    els += info_box(
        "오디오 자동 연결 시스템: Unity 에디터 메뉴 [LAST BRAKE > 31. SFX 전체 연결]을 실행하면 "
        "11개 씬 전체에 37개 클립이 1클릭으로 자동 연결됩니다. "
        "[32. 오디오 연결 해제]로 무음 상태 즉시 복원 가능.",
    )

    # ── 9. 기술 구현 ──────────────────────────────────────────
    els += sec_title(9, "기술 구현")
    els += tbl(
        ["기술 포인트", "설명"],
        [
            ["DontDestroyOnLoad 싱글턴",
             "FXManager · SFXManager · BGMController · StatManager가 11개 씬 전체에서 상태 유지"],
            ["Runtime Canvas 생성",
             "Phase 2 결말 화면 전체를 코드로 생성 → 씬 의존성 제거, 레이어 충돌 없음"],
            ["anchorMax.x 수치 바",
             "Slider+LayoutGroup 충돌 해결 → Fill 이미지의 anchorMax.x를 직접 lerp"],
            ["글리치 이중 모드",
             "상시(alpha 0.02~0.06) / 버스트(alpha 0.45, 자동 종료) 분리 → 가독성 유지"],
            ["cos 파형 비네트",
             "(1−cos(t/5s×2π))×0.5 → 숨쉬듯 출렁이는 조용한 긴장감 표현"],
            ["Reflection 에디터 툴",
             "BindingFlags로 37개 AudioClip 필드를 이름 매핑, 1클릭 자동 연결/해제"],
        ],
        col_widths=[usable*0.32, usable*0.68],
    )
    els += tbl(
        ["#", "버그 증상", "원인", "해결"],
        [
            ["1", "결말 화면 뒤로 씬 배경 비침",  "배경 60% 투명도 사용", "불투명 단색 배경으로 교체"],
            ["2", "글리치가 대화 텍스트 가림",     "alpha 0.45 지속",     "상시 0.06 이하 제한, 버스트 분리"],
            ["3", "Phase 2 대화창 미표시",         "NullRef로 코루틴 중단", "null 체크 추가, alpha=1 즉시"],
            ["4", "스탯 바 카드 overflow",          "중앙 앵커 계산 오류", "상단 앵커 기준으로 전환"],
            ["5", "씬 전환 후 dangling Instance",  "OnDestroy 미처리",    "if(Instance==this) null 처리"],
        ],
        col_widths=[usable*0.05, usable*0.28, usable*0.3, usable*0.37],
    )

    # ── 10. 사회적 가치 ───────────────────────────────────────
    els += sec_title(10, "사회적 가치")
    els += card([
        P("이 게임은 약물 중독 예방을 위한 인식 개선 콘텐츠로 기획되었습니다."),
        SP(6),
        P("• 플레이어가 직접 체험하는 '반복된 선택의 결과'"),
        P("• 수치로 가시화되는 판단력 저하와 의존도 상승"),
        P("• 강제 선택 메커닉으로 자유의 상실을 체감"),
        P("• 트루 엔딩의 4번째 벽 파괴로 플레이어 자신에게 질문을 던짐"),
    ])
    els += cta_box()

    # ── 빌드 ──────────────────────────────────────────────────
    doc.build(els,
              onFirstPage=draw_bg,
              onLaterPages=draw_bg)
    print(f"✅ PDF 생성 완료: {OUT}")

if __name__ == "__main__":
    build()
