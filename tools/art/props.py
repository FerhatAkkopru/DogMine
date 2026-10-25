"""Bahçe kareleri, eşyalar, ikonlar ve arayüz parçaları."""
import math
import random
from sketch import Svg, INK, ellipse_pts, arc_pts, offset
import style as S

WHITE = '#ffffff'


def rounded_rect_pts(x, y, w, h, r, per_corner=5):
    """Yuvarlatılmış dikdörtgenin kenarından geçen noktalar (saat yönünde)."""
    pts = []
    corners = [(x + w - r, y + r, -90), (x + w - r, y + h - r, 0), (x + r, y + h - r, 90), (x + r, y + r, 180)]
    for cx, cy, a0 in corners:
        for i in range(per_corner):
            a = math.radians(a0 + 90 * i / (per_corner - 1))
            pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


# ---------- Bahçe ----------

def grass_tile(svg, variant, light):
    """Çimli kare. variant: çim tutamı düzeni, light: dama tahtası için açık ton."""
    size = svg.w
    base = '#B4D49C' if light else S.LAWN
    edge = '#97BD80' if light else S.LAWN_DARK
    m = size * 0.035
    svg.blob(rounded_rect_pts(m, m, size - 2 * m, size - 2 * m, size * 0.2, 7), edge, jitter=1.0, smooth=True)
    svg.blob(rounded_rect_pts(m, m, size - 2 * m, size - 2 * m - size * 0.06, size * 0.2, 7), base, jitter=1.0, smooth=True)
    rnd = random.Random(variant * 31 + (7 if light else 0))
    s = size / 256
    spots = [[(70, 96), (168, 170)], [(84, 160), (176, 84)], [(120, 120)]][variant % 3]
    for (cx, cy) in spots:
        cx, cy = cx * s + rnd.uniform(-8, 8) * s, cy * s + rnd.uniform(-8, 8) * s
        for k, (ox, h, lean) in enumerate([(-12, 26, -8), (0, 34, 2), (12, 24, 9)]):
            svg.brush([(cx + ox * s, cy + 14 * s), (cx + (ox + lean * 0.5) * s, cy - h * 0.4 * s), (cx + (ox + lean) * s, cy - h * s * 0.8)],
                      7 * s, S.LAWN_INK, taper=0.85, opacity=0.75)
    if variant == 2:  # küçük papatya
        fx, fy = 182 * s, 178 * s
        for i in range(5):
            a = i * 2 * math.pi / 5
            svg.circle(fx + math.cos(a) * 11 * s, fy + math.sin(a) * 11 * s, 8 * s, S.WHITE)
        svg.circle(fx, fy, 6 * s, S.MUSTARD)


def soil_tile(svg, variant):
    """Kazılmış toprak: gömük görünsün diye üst kenarı koyu."""
    size = svg.w
    m = size * 0.035
    svg.blob(rounded_rect_pts(m, m, size - 2 * m, size - 2 * m, size * 0.2, 7), S.SAND_DARK, jitter=0.8)
    svg.blob(rounded_rect_pts(m, m + size * 0.05, size - 2 * m, size - 2 * m - size * 0.05, size * 0.2, 7), S.SAND, jitter=0.8)
    rnd = random.Random(variant * 13 + 5)
    s = size / 256
    for _ in range(4):
        x, y = rnd.uniform(40, 216) * s, rnd.uniform(60, 220) * s
        if 80 * s < x < 176 * s and 80 * s < y < 180 * s:
            continue  # ortası rakam için boş kalsın
        svg.ellipse(x, y, rnd.uniform(5, 9) * s, rnd.uniform(3, 6) * s, S.SAND_DARK, rot=rnd.uniform(0, 180))


def board_bed(svg):
    """Bahçe yatağı (9 parçalı): koyu çim zemin, mürekkep kontur."""
    w = svg.w
    pts = rounded_rect_pts(10, 10, w - 20, w - 20, 44, 6)
    svg.blob(pts, S.LAWN_DARK, jitter=1.0)
    svg.brush(pts, 7, INK, closed=True, jitter=0.8, wobble=0.15)


# ---------- Eşyalar ----------

def bowl(svg):
    """Mama kabı: tepeleme mama dolu mercan kap, önünde küçük kemik, mürekkep kontur."""
    k = svg.w / 256
    def P(pts):
        return [(x * k, y * k) for x, y in pts]
    svg.ellipse(128 * k, 212 * k, 96 * k, 11 * k, INK, opacity=0.1)
    # Kap gövdesi (aşağı doğru daralır)
    body = [(30, 118), (226, 118), (206, 190), (180, 206), (76, 206), (50, 190)]
    svg.blob(P(body), S.CORAL, jitter=0.8)
    svg.blob(P([(46, 160), (210, 160), (204, 182), (52, 182)]), S.CORAL_DARK, opacity=0.35)
    # Mama tepesi
    for cx, cy in [(78, 106), (104, 92), (132, 86), (160, 92), (182, 106), (92, 116), (120, 108), (148, 108), (172, 116), (132, 120)]:
        svg.blob(P(ellipse_pts(cx, cy, 17, 13, 8, rot=cx % 30)), '#A8683A', jitter=1)
        svg.brush(P(arc_pts(cx, cy - 2, 10, 200, 320, 5)), 3 * k, '#D29A63', taper=0.5)
    # Ağız kenarı
    svg.blob(P([(24, 116), (128, 132), (232, 116), (226, 130), (128, 146), (30, 130)]), S.CORAL_LIGHT, jitter=0.6)
    # Kemik amblemi
    for x0, x1 in [(104, 152)]:
        svg.brush(P([(x0, 172), (x1, 172)]), 9 * k, S.WHITE, taper=0)
        for x in (x0, x1):
            svg.circle(x * k, 166 * k, 7 * k, S.WHITE)
            svg.circle(x * k, 178 * k, 7 * k, S.WHITE)
    # Mürekkep
    svg.brush(P([(26, 118), (48, 190), (76, 208), (180, 208), (208, 190), (230, 118)]), 7 * k, INK, taper=0.35)
    svg.brush(P([(24, 117), (128, 133), (232, 117)]), 6 * k, INK, taper=0.4)
    svg.brush(P([(62, 106), (88, 84), (132, 74), (176, 84), (200, 104)]), 5 * k, INK, taper=0.7, opacity=0.9)


def heart(svg, full=True):
    k = svg.w / 256
    pts = [(128, 216), (60, 160), (34, 104), (52, 58), (98, 48), (128, 82), (158, 48), (204, 58), (222, 104), (196, 160)]
    pts = [(x * k, y * k) for x, y in pts]
    svg.blob(pts, S.CORAL if full else S.PAPER_DARK, jitter=1)
    if full:
        svg.brush([(70 * k, 92 * k), (82 * k, 72 * k), (100 * k, 66 * k)], 9 * k, S.WHITE, taper=0.5, opacity=0.8)
    svg.brush(offset(pts, 3 * k, -2 * k), 9 * k, INK, closed=True, wobble=0.12)


def bone(svg, angle=-30):
    k = svg.w / 256
    c = (128 * k, 128 * k)
    def R(pts):
        a = math.radians(angle)
        return [(c[0] + (x * k - c[0]) * math.cos(a) - (y * k - c[1]) * math.sin(a),
                 c[1] + (x * k - c[0]) * math.sin(a) + (y * k - c[1]) * math.cos(a)) for x, y in pts]
    shape = [(70, 112), (52, 86), (30, 92), (26, 118), (44, 128), (26, 140), (32, 166), (56, 170), (72, 144),
             (184, 144), (200, 170), (226, 166), (230, 140), (212, 128), (230, 118), (226, 92), (202, 86), (186, 112)]
    pts = R(shape)
    svg.blob(pts, S.CREAM, jitter=0.8)
    svg.brush(R([(80, 124), (176, 124)]), 6 * k, S.WHITE, taper=0.6, opacity=0.9)
    svg.brush(offset(pts, 3 * k, -2 * k), 8 * k, INK, closed=True, wobble=0.12)


def star(svg):
    k = svg.w / 256
    pts = []
    for i in range(10):
        a = math.radians(-90 + i * 36)
        r = (100 if i % 2 == 0 else 46) * k
        pts.append((128 * k + r * math.cos(a), 136 * k + r * math.sin(a)))
    svg.blob(pts, S.MUSTARD, jitter=1)
    svg.brush([(104 * k, 108 * k), (114 * k, 96 * k)], 8 * k, S.WHITE, taper=0.4, opacity=0.8)
    svg.brush(offset(pts, 3 * k, -2 * k), 8 * k, INK, closed=True, wobble=0.1)


def pearl(svg):
    k = svg.w / 256
    pts = ellipse_pts(128 * k, 132 * k, 84 * k, 84 * k, 12)
    svg.blob(pts, S.PINK_LIGHT, jitter=1)
    svg.blob(ellipse_pts(146 * k, 150 * k, 56 * k, 56 * k, 10), S.PINK, opacity=0.6)
    svg.blob(ellipse_pts(100 * k, 100 * k, 22 * k, 16 * k, 8, rot=-30), S.WHITE)
    svg.brush(offset(pts, 3 * k, -2 * k), 8 * k, INK, closed=True, wobble=0.1)


def collar(svg):
    k = svg.w / 256
    band = ellipse_pts(128 * k, 112 * k, 92 * k, 62 * k, 14)
    svg.brush(band, 30 * k, S.MUSTARD, closed=True, wobble=0.03)
    for i in range(8):
        a = math.radians(i * 45 + 22)
        svg.circle(128 * k + math.cos(a) * 92 * k, 112 * k + math.sin(a) * 62 * k, 7 * k, S.WHITE)
    svg.brush(ellipse_pts(128 * k, 112 * k, 108 * k, 77 * k, 14), 6 * k, INK, closed=True, wobble=0.1)
    svg.brush(ellipse_pts(128 * k, 112 * k, 76 * k, 47 * k, 14), 6 * k, INK, closed=True, wobble=0.1)
    tag = ellipse_pts(128 * k, 204 * k, 30 * k, 30 * k, 10)
    svg.brush([(128 * k, 172 * k), (128 * k, 182 * k)], 6 * k, INK, taper=0)
    svg.blob(tag, S.CORAL, jitter=0.8)
    svg.brush(offset(tag, 2 * k, -2 * k), 6 * k, INK, closed=True)


def paw(svg):
    """Pati izi (beyaz; kodda boyanır)."""
    k = svg.w / 256
    svg.blob([(x * k, y * k) for x, y in [(128, 132), (176, 160), (186, 206), (150, 222), (128, 212), (106, 222), (70, 206), (80, 160)]], WHITE, jitter=1)
    for cx, cy, r, rot in [(66, 116, 22, -20), (104, 72, 22, -8), (152, 72, 22, 8), (190, 116, 22, 20)]:
        svg.blob(ellipse_pts(cx * k, cy * k, r * k, r * 1.25 * k, 10, rot=rot), WHITE, jitter=0.8)


# ---------- İkonlar (beyaz çizgi; kodda mürekkep ya da krem renge boyanır) ----------

def _icon(svg, strokes, width=20):
    k = svg.w / 128
    for pts in strokes:
        svg.brush([(x * k, y * k) for x, y in pts], width * k / 1.6, WHITE, taper=0.25, wobble=0.08)


def icon_pause(svg):
    _icon(svg, [[(48, 34), (47, 64), (48, 94)], [(80, 34), (81, 64), (80, 94)]], 26)


def icon_back(svg):
    _icon(svg, [[(78, 30), (44, 64), (78, 98)]], 24)


def icon_check(svg):
    _icon(svg, [[(28, 66), (52, 90), (100, 38)]], 24)


def icon_cross(svg):
    _icon(svg, [[(34, 34), (64, 64), (94, 94)], [(94, 34), (64, 64), (34, 94)]], 22)


def icon_lock(svg):
    k = svg.w / 128
    svg.brush([(x * k, y * k) for x, y in arc_pts(64, 56, 22, 180, 360, 8)], 10 * k, WHITE, taper=0.1)
    svg.brush([(42 * k, 56 * k), (42 * k, 64 * k)], 10 * k, WHITE, taper=0)
    svg.brush([(86 * k, 56 * k), (86 * k, 64 * k)], 10 * k, WHITE, taper=0)
    svg.blob([(x * k, y * k) for x, y in rounded_rect_pts(30, 60, 68, 50, 12, 4)], WHITE, jitter=0.8)


def icon_gear(svg):
    k = svg.w / 128
    pts = []
    for i in range(32):
        a = 2 * math.pi * i / 32
        r = 46 if (i // 2) % 2 == 0 else 34
        pts.append((64 * k + r * k * math.cos(a), 64 * k + r * k * math.sin(a)))
    hole = ellipse_pts(64 * k, 64 * k, 15 * k, 15 * k, 16)
    svg.holed(pts, hole, WHITE)


def icon_bowl_outline(svg):
    _icon(svg, [[(24, 58), (36, 94), (92, 94), (104, 58)], [(22, 58), (106, 58)]], 16)


# ---------- Arayüz şekilleri (beyaz; 9 parçalı) ----------

def ui_box_fill(svg):
    w = svg.w
    svg.blob(rounded_rect_pts(6, 6, w - 12, w - 12, 44, 6), WHITE, jitter=0.6)


def ui_box_line(svg):
    w = svg.w
    svg.brush(rounded_rect_pts(9, 9, w - 18, w - 18, 41, 6), 7, WHITE, closed=True, jitter=0.5, wobble=0.18)


def ui_circle_fill(svg):
    w = svg.w
    svg.blob(ellipse_pts(w / 2, w / 2, w / 2 - 6, w / 2 - 6, 14), WHITE, jitter=0.6)


def ui_circle_line(svg):
    w = svg.w
    svg.brush(ellipse_pts(w / 2, w / 2, w / 2 - 9, w / 2 - 9, 16), 7 * w / 128, WHITE, closed=True, jitter=0.5, wobble=0.18)


def underline(svg):
    """Başlığın altındaki fırça darbesi."""
    w, h = svg.w, svg.h
    svg.brush([(16, h * 0.62), (w * 0.3, h * 0.42), (w * 0.62, h * 0.5), (w - 16, h * 0.34)], h * 0.34, WHITE, taper=0.7, wobble=0.1)


# ---------- Süsler ----------

# Köpek yatağı geometrisi (1000×560 tuval)
BED = dict(cx=500, cy=300, rx=470, ry=230, crx=390, cry=150, ccy=286, lrx=410, lry=140)


def _ell(cx, cy, rx, ry, a0, a1, step=6):
    import math as _m
    rng = range(a0, a1 + (1 if a1 >= a0 else -1), step if a1 >= a0 else -step)
    return [(cx + rx * _m.cos(_m.radians(a)), cy + ry * _m.sin(_m.radians(a))) for a in rng]


def bed_back(svg):
    """Köpek yatağının arka kenarı ve minderi (köpekler bunun üstüne çizilir)."""
    b = BED
    svg.ellipse(b['cx'], b['cy'] + b['ry'] - 6, b['rx'] - 20, 20, INK, opacity=0.1)
    outer = _ell(b['cx'], b['cy'], b['rx'], b['ry'], 0, 354, 12)
    svg.blob(outer, S.CORAL, jitter=1.5)
    cushion = _ell(b['cx'], b['ccy'], b['crx'], b['cry'], 0, 348, 24)
    dense = svg.blob(cushion, S.CREAM, jitter=1.5)
    with svg.clip(dense):
        svg.scribble_loops(150, 170, 850, 420, S.CREAM_DARK, width=5, rows=5, loop=26, seed=9)
    svg.brush(offset(_ell(b['cx'], b['ccy'], b['crx'], b['cry'], 170, 370, 12), 3, -2), 6, INK, wobble=0.12, opacity=0.9)
    svg.brush(offset(_ell(b['cx'], b['cy'], b['rx'], b['ry'], 175, 365, 12), 5, -4), 8, INK, wobble=0.12)
    # dikişler
    stitch = _ell(b['cx'], b['cy'] - 6, b['rx'] - 40, b['ry'] - 36, 192, 348, 8)
    for i in range(0, len(stitch) - 1, 2):
        svg.brush([stitch[i], stitch[i + 1]], 5, S.CORAL_LIGHT, taper=0.2)


def bed_front(svg):
    """Köpek yatağının ön kenarı (köpeklerin önüne çizilir)."""
    b = BED
    bottom = _ell(b['cx'], b['cy'], b['rx'], b['ry'], 0, 180, 6)
    lip = _ell(b['cx'], b['cy'], b['lrx'], b['lry'], 180, 0, 6)
    svg.poly(bottom + lip, S.CORAL)
    svg.brush(offset(_ell(b['cx'], b['cy'], b['rx'], b['ry'], -4, 184, 8), 5, -4), 8, INK, wobble=0.12)
    svg.brush(_ell(b['cx'], b['cy'], b['lrx'] + 4, b['lry'] + 4, 186, -6, 8), 7, INK, wobble=0.15)
    stitch = _ell(b['cx'], b['cy'] + 8, (b['rx'] + b['lrx']) / 2, (b['ry'] + b['lry']) / 2 + 6, 14, 166, 7)
    for i in range(0, len(stitch) - 1, 2):
        svg.brush([stitch[i], stitch[i + 1]], 5, S.CORAL_LIGHT, taper=0.2)
