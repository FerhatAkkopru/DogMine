"""Köpek ve kedi kuklaları: parçalar (gövde, kafa, bacaklar, kuyruk, kulaklar) pozla
döndürülüp yerleştirilir. Aynı kukladan duruş, koşu, havlama, kuyruk sallama gibi
kareler üretilir. Bütün çizimler 512×512 alanda, y aşağı doğru; köpek SAĞA bakar
(sola bakan kareler oyunda aynalanır)."""
import math
from sketch import INK, ellipse_pts, arc_pts, offset
import style as S

GROUND = 452


# ---------- Geometri yardımcıları ----------

def rot(pts, cx, cy, deg):
    a = math.radians(deg)
    c, s = math.cos(a), math.sin(a)
    return [(cx + (x - cx) * c - (y - cy) * s, cy + (x - cx) * s + (y - cy) * c) for x, y in pts]


def rotp(p, cx, cy, deg):
    return rot([p], cx, cy, deg)[0]


def tr(pts, dx, dy):
    return [(x + dx, y + dy) for x, y in pts]


def scallop(pts, amount, bumps):
    """Kabarık tüy konturu: noktaları dışa doğru dalgalandırır."""
    n = len(pts)
    cx = sum(p[0] for p in pts) / n
    cy = sum(p[1] for p in pts) / n
    out = []
    for i, (x, y) in enumerate(pts):
        k = abs(math.sin(math.pi * bumps * i / n))
        dx, dy = x - cx, y - cy
        d = math.hypot(dx, dy) or 1
        out.append((x + dx / d * amount * k, y + dy / d * amount * k))
    return out


def shade(hex_color, k):
    """Rengi koyulaştır (k<1) ya da aç (k>1)."""
    r, g, b = (int(hex_color[i:i + 2], 16) for i in (1, 3, 5))
    if k < 1:
        r, g, b = (int(v * k) for v in (r, g, b))
    else:
        r, g, b = (int(v + (255 - v) * (k - 1)) for v in (r, g, b))
    return f'#{r:02x}{g:02x}{b:02x}'


# ---------- Irklar ----------

BREEDS = {
    'hardal':   dict(body=S.MUSTARD, texture=S.MUSTARD_LIGHT, ear=S.MUSTARD_DARK, muzzle=S.CREAM, collar=S.MINT,
                     ears='floppy', tail='plume', fluffy=0, legs=1.0, pattern='loops'),
    'lacivert': dict(body=S.NAVY, texture=S.NAVY_LIGHT, ear=S.CORAL, muzzle=S.CREAM, collar=S.MUSTARD,
                     ears='floppy', tail='stub', fluffy=9, legs=0.95, pattern='strokes'),
    'mercan':   dict(body=S.CORAL, texture=S.CORAL_LIGHT, ear=S.CORAL, muzzle=S.CREAM, collar=S.NAVY,
                     ears='pointy', tail='curl', fluffy=0, legs=1.0, pattern='stripes'),
    'pembe':    dict(body=S.PINK, texture=S.WHITE, ear=S.PINK_DARK, muzzle=S.PINK_LIGHT, collar=S.MINT,
                     ears='pom', tail='pom', fluffy=12, legs=1.05, pattern='loops'),
    'nane':     dict(body=S.MINT, texture=S.PINK, ear=S.MINT_DARK, muzzle=S.WHITE, collar=S.CORAL,
                     ears='folded', tail='stub_up', fluffy=0, legs=1.0, pattern='dashes'),
    'krem':     dict(body=S.CREAM, texture=S.CREAM_DARK, ear=S.MUSTARD, muzzle=S.WHITE, collar=S.CORAL,
                     ears='pointy', tail='stub', fluffy=0, legs=0.6, pattern='patch', patch=S.MUSTARD),
}


def texture(svg, b, box, seed):
    x0, y0, x1, y1 = box
    p = b['pattern']
    if p == 'loops':
        svg.scribble_loops(x0, y0, x1, y1, b['texture'], width=4, rows=2, loop=20, seed=seed)
    elif p == 'strokes':
        svg.scribble_strokes(x0, y0, x1, y1, b['texture'], count=8, length=40, angle=-25, width=4, seed=seed)
    elif p == 'stripes':
        for i in range(4):
            x = x0 + (x1 - x0) * (i + 0.5) / 4
            svg.brush([(x - 6, y0), (x + 6, (y0 + y1) / 2), (x - 3, y1)], 5, b['texture'], taper=0.5)
    elif p == 'dashes':
        svg.scribble_strokes(x0, y0, x1, y1, b['texture'], count=10, length=20, angle=10, width=6, seed=seed)
    elif p == 'patch':
        svg.blob(ellipse_pts((x0 + x1) / 2, y0 + 12, (x1 - x0) * 0.42, 34, 10), b['patch'], jitter=3)


# ---------- Yan görünüm: duruş, koşu, havlama ----------

POSES = {
    # bacak açıları: (kalça/omuz açısı, diz açısı) derece; + = ileri (sağa)
    'stand':  dict(fn=(6, -4), ff=(-2, 2), bn=(-6, 6), bf=(4, -2), bob=0, tilt=0, head=0, ears=0, tail=-35, eyes='open', mouth='smile'),
    'alert':  dict(fn=(10, -6), ff=(0, 2), bn=(-10, 8), bf=(2, -2), bob=-4, tilt=-3, head=-6, ears=-4, tail=-55, eyes='wide', mouth='smile'),
    'bark':   dict(fn=(14, -8), ff=(2, 0), bn=(-12, 8), bf=(2, -2), bob=2, tilt=-5, head=-14, ears=12, tail=-60, eyes='wide', mouth='bark'),
    'wag0':   dict(fn=(6, -4), ff=(-2, 2), bn=(-6, 6), bf=(4, -2), bob=0, tilt=0, head=-3, ears=0, tail=-70, eyes='happy', mouth='tongue'),
    'wag1':   dict(fn=(6, -4), ff=(-2, 2), bn=(-6, 6), bf=(4, -2), bob=1, tilt=0, head=0, ears=4, tail=-15, eyes='happy', mouth='tongue'),
    'run0':   dict(fn=(-28, 30), ff=(-12, 18), bn=(40, -55), bf=(28, -40), bob=-4, tilt=2, head=4, ears=10, tail=10, eyes='wide', mouth='tongue'),
    'run1':   dict(fn=(40, -10), ff=(25, -5), bn=(-20, 25), bf=(-5, 15), bob=-14, tilt=-4, head=-2, ears=24, tail=20, eyes='wide', mouth='tongue'),
    'run2':   dict(fn=(62, -4), ff=(50, -4), bn=(-58, 20), bf=(-45, 15), bob=-22, tilt=-2, head=0, ears=34, tail=28, eyes='wide', mouth='tongue'),
    'run3':   dict(fn=(8, 6), ff=(-4, 10), bn=(10, -25), bf=(22, -30), bob=-8, tilt=4, head=3, ears=18, tail=16, eyes='wide', mouth='tongue'),
}

BODY = [(126, 320), (142, 264), (200, 238), (282, 236), (338, 252), (362, 300), (352, 352), (302, 386), (204, 388), (144, 370)]
BODY_C = (240, 312)
NECK = (318, 262)
HIPS = {'bn': (168, 350), 'bf': (190, 346)}
SHOULDERS = {'fn': (312, 352), 'ff': (292, 348)}
RUMP = (130, 298)


def _leg(svg, b, hip, angles, length, color, near):
    a1, a2 = angles
    l1 = 54 * length
    l2 = 50 * length
    knee = (hip[0] + l1 * math.sin(math.radians(a1)), hip[1] + l1 * math.cos(math.radians(a1)))
    foot = (knee[0] + l2 * math.sin(math.radians(a1 + a2)), knee[1] + l2 * math.cos(math.radians(a1 + a2)))
    w = 40 if near else 34
    svg.brush([hip, knee, foot], w, color, taper=0.12, wobble=0.04)
    svg.blob(ellipse_pts(foot[0] + 9, foot[1] + 4, 24, 14, 10, rot=a1 * 0.3), color, jitter=0.8)
    if near:
        edge = [(hip[0] + w * 0.42, hip[1] + 6), (knee[0] + w * 0.42, knee[1]), (foot[0] + w * 0.36, foot[1] - 6)]
        svg.brush(offset(edge, 3, -2), 5, INK, taper=0.6)
        svg.brush(offset(arc_pts(foot[0] + 9, foot[1] + 4, 24, 20, 160, 6, ry=14), 2, -1), 4.5, INK, taper=0.5)
        svg.brush([(foot[0] + 22, foot[1] + 2), (foot[0] + 24, foot[1] + 12)], 3, INK, taper=0.3)


def _tail(svg, b, base, angle, color):
    """Kuyruk: angle 0 = geriye yatay, negatif = yukarı."""
    st = b['tail']
    def P(pts):
        return rot([(base[0] + x, base[1] + y) for x, y in pts], base[0], base[1], angle)
    if st == 'plume':
        pts = P([(0, 0), (-40, -6), (-78, -2), (-104, 12)])
        svg.brush(pts, 30, color, taper=0.7, wobble=0.06)
        svg.brush(offset(P([(-10, -10), (-44, -18), (-80, -14), (-108, 4)]), 3, -2), 5, INK, taper=0.6)
        svg.brush(P([(-50, 8), (-60, 20)]), 3.5, INK, taper=0.4)
        svg.brush(P([(-74, 8), (-84, 22)]), 3.5, INK, taper=0.4)
    elif st == 'curl':
        pts = P([(0, 0), (-18, -40), (10, -74), (44, -64), (40, -38)])
        svg.brush(pts, 26, color, taper=0.55, wobble=0.04)
        svg.brush(offset(P([(-14, -6), (-30, -44), (6, -86), (52, -70), (48, -40)]), 3, -2), 5, INK, taper=0.6)
        svg.brush(P([(4, -60), (30, -56)]), 9, S.CREAM, taper=0.6, opacity=0.9)
    elif st == 'pom':
        pts = P([(0, 0), (-22, -24), (-30, -50)])
        svg.brush(pts, 12, color, taper=0.2)
        tip = P([(-34, -62)])[0]
        svg.blob(scallop(ellipse_pts(tip[0], tip[1], 30, 30, 18), 6, 9), color, jitter=1)
        svg.brush(offset(arc_pts(tip[0], tip[1], 32, 180, 360, 8), 3, -2), 5, INK, taper=0.5)
    elif st == 'stub_up':
        pts = P([(0, 0), (-8, -34), (-4, -58)])
        svg.brush(pts, 22, color, taper=0.55)
        svg.brush(offset(P([(-12, -4), (-18, -36), (-12, -62)]), 3, -2), 5, INK, taper=0.6)
    else:  # stub
        pts = P([(0, 0), (-30, -8), (-48, -2)])
        svg.brush(pts, 24, color, taper=0.6)
        svg.brush(offset(P([(-6, -10), (-32, -18), (-52, -10)]), 3, -2), 5, INK, taper=0.6)


def _ears_back(svg, b, head_xf, ear_angle):
    """Kafanın arkasında kalan kulak parçaları (dik kulakların arka yüzü)."""
    if b['ears'] in ('pointy', 'folded'):
        ear = head_xf(rot([(318, 150), (300, 66), (330, 72), (362, 130)], 330, 150, -ear_angle * 0.6))
        svg.blob(ear, shade(b['ear'], 0.85) if b['ear'] == b['body'] else b['ear'], jitter=1)


def _head(svg, b, head_xf, pose, seed):
    ears = b['ears']
    ear_angle = pose['ears']
    body_c = b['body']
    # Kafatası
    skull = [(282, 214), (288, 162), (326, 128), (378, 122), (420, 150), (432, 196), (418, 246), (372, 274), (318, 268)]
    if b['fluffy']:
        skull = scallop(skull, b['fluffy'] * 0.6, 7)
    dense = svg.blob(head_xf(skull), body_c, jitter=1)
    # Burun bölgesi
    muzzle = head_xf([(398, 196), (448, 190), (478, 206), (482, 236), (462, 262), (412, 266), (390, 236)])
    svg.blob(muzzle, b['muzzle'], jitter=0.8)
    # Göğüs/çene tüyü (kabarıklar için)
    if ears == 'pom':
        top = head_xf([(312, 150)])[0]
        svg.blob(scallop(ellipse_pts(top[0] + 34, top[1] - 16, 54, 40, 16), 7, 8), body_c, jitter=1)
        svg.brush(offset(arc_pts(top[0] + 34, top[1] - 16, 56, 190, 350, 8, ry=42), 3, -2), 5, INK, taper=0.5)
    # Göz
    eye = head_xf([(398, 184)])[0]
    style = pose['eyes']
    if style == 'closed':
        svg.brush(arc_pts(eye[0], eye[1], 13, 15, 165, 7), 6, INK, taper=0.35)
    elif style == 'happy':
        svg.brush(arc_pts(eye[0], eye[1] + 6, 13, 195, 345, 7), 6, INK, taper=0.35)
    else:
        r = 19 if style == 'wide' else 16
        if style == 'wide':
            svg.circle(eye[0], eye[1], r + 6, S.WHITE)
        svg.blob(ellipse_pts(eye[0], eye[1], r * 0.72, r, 10), INK)
        svg.circle(eye[0] + 3, eye[1] - 5, r * 0.3, S.WHITE)
    brow = head_xf([(380, 156), (396, 150), (412, 154)])
    if style == 'wide':
        svg.brush(tr(brow, 0, -8), 5, INK, taper=0.5)
    # Yanak
    cheek = head_xf([(404, 232)])[0]
    svg.ellipse(cheek[0], cheek[1], 15, 9, S.CORAL_LIGHT, opacity=0.55)
    # Burun
    nose = head_xf([(476, 214)])[0]
    svg.blob(ellipse_pts(nose[0], nose[1], 15, 12, 8), INK)
    svg.circle(nose[0] - 3, nose[1] - 4, 3.5, S.WHITE, opacity=0.7)
    # Ağız
    mouth = pose['mouth']
    if mouth == 'bark':
        jaw = head_xf([(420, 252), (476, 246), (470, 290), (430, 296)])
        svg.blob(head_xf([(414, 244), (480, 238), (474, 288), (424, 300), (404, 270)]), '#7a2e2a')
        svg.blob(head_xf([(426, 276), (462, 270), (462, 294), (432, 298)]), S.CORAL)
        svg.blob(jaw[:1] + head_xf([(470, 296), (474, 306), (420, 308), (402, 286)]), b['muzzle'])
        svg.brush(head_xf([(402, 290), (420, 308), (474, 306)]), 5, INK, taper=0.6)
        svg.brush(head_xf([(414, 244), (480, 238)]), 5, INK, taper=0.6)
    elif mouth == 'tongue':
        svg.brush(head_xf([(430, 252), (450, 258), (470, 250)]), 5, INK, taper=0.6)
        tongue = head_xf([(440, 254), (462, 254), (466, 282), (448, 290), (436, 278)])
        svg.blob(tongue, S.CORAL, jitter=0.6)
        svg.brush(head_xf([(450, 260), (452, 280)]), 3, S.CORAL_DARK, taper=0.4)
        svg.brush(offset(tongue[1:4], 2, -1), 3.5, INK, taper=0.5)
    else:
        svg.brush(head_xf([(476, 228), (474, 244), (458, 254), (436, 250)]), 5, INK, taper=0.6)
    # Kulak (öndeki)
    if ears == 'floppy':
        ear = head_xf(rot([(318, 140), (350, 134), (360, 176), (350, 236), (326, 256), (304, 230), (304, 178)], 330, 140, ear_angle))
        svg.blob(ear, b['ear'], jitter=1.2)
        svg.brush(offset(ear[:5], 3, -2), 5, INK, taper=0.55)
    elif ears == 'pom':
        ear = head_xf(rot([(318, 150)], 330, 150, ear_angle))[0]
        svg.blob(scallop(ellipse_pts(ear[0] - 4, ear[1] + 56, 34, 58, 16), 7, 8), b['ear'], jitter=1)
        svg.brush(offset(ellipse_pts(ear[0] - 4, ear[1] + 56, 36, 60, 10)[0:6], 3, -2), 5, INK, taper=0.5)
    else:
        tip_fold = ears == 'folded'
        pts = [(330, 150), (322, 64), (346, 68), (384, 136)]
        ear = head_xf(rot(pts, 340, 150, -ear_angle * 0.6))
        svg.blob(ear, b['ear'] if b['ear'] != b['body'] else shade(b['body'], 0.9), jitter=1)
        inner = head_xf(rot([(338, 140), (334, 90), (362, 132)], 340, 150, -ear_angle * 0.6))
        svg.blob(inner, S.PINK, jitter=0.6)
        if tip_fold:
            fold = head_xf(rot([(322, 64), (346, 68), (352, 96), (330, 92)], 340, 150, -ear_angle * 0.6))
            svg.blob(fold, shade(b['ear'], 0.82), jitter=0.5)
        svg.brush(offset(ear, 3, -2), 5, INK, taper=0.5)
    # Kafa konturu (kayık)
    svg.brush(offset(head_xf([(286, 196), (296, 150), (330, 126), (380, 120), (422, 148), (434, 192)]), 4, -3), 6, INK, taper=0.5)
    svg.brush(offset(head_xf([(448, 190), (480, 206), (484, 236)]), 3, -2), 5, INK, taper=0.6)


def dog_side(svg, coat, pose_name, seed=1, shadow=True):
    b = BREEDS[coat]
    pose = POSES[pose_name]
    legs = b['legs']
    leg_drop = (1 - legs) * 104  # kısa bacaklılarda gövde aşağı iner
    bob = pose['bob'] + leg_drop
    tilt = pose['tilt']
    far = shade(b['body'], 0.82)

    def body_xf(pts):
        return tr(rot(pts, BODY_C[0], BODY_C[1], tilt), 0, bob)

    neck = body_xf([NECK])[0]

    def head_xf(pts):
        moved = tr(pts, neck[0] - NECK[0], neck[1] - NECK[1])
        return rot(moved, neck[0], neck[1], pose['head'] + tilt)

    if shadow:
        lift = max(0.4, 1 + pose['bob'] / 40)
        svg.ellipse(250, GROUND + 6, 150 * lift, 12, INK, opacity=0.09)

    # Arkadaki bacaklar
    _leg(svg, b, body_xf([HIPS['bf']])[0], pose['bf'], legs, far, False)
    _leg(svg, b, body_xf([SHOULDERS['ff']])[0], pose['ff'], legs, far, False)
    # Kuyruk
    _tail(svg, b, body_xf([RUMP])[0], pose['tail'] - tilt * 0, b['body'])
    # Arkadaki kulak
    _ears_back(svg, b, head_xf, pose['ears'])
    # Gövde
    body = body_xf(BODY)
    if b['fluffy']:
        body = scallop(body, b['fluffy'], 9)
    dense = svg.blob(body, b['body'], jitter=1.5)
    with svg.clip(dense):
        tbox = body_xf([(170, 262), (330, 350)])
        texture(svg, b, (tbox[0][0], tbox[0][1], tbox[1][0], tbox[1][1]), seed)
    svg.brush(offset(body_xf([(130, 330), (142, 266), (200, 238), (282, 236)]), 4, -3), 6, INK, taper=0.5)
    svg.brush(offset(body_xf([(206, 390), (300, 388), (340, 368)]), 4, -3), 5, INK, taper=0.6)
    # Öndeki bacaklar
    _leg(svg, b, body_xf([HIPS['bn']])[0], pose['bn'], legs, b['body'], True)
    _leg(svg, b, body_xf([SHOULDERS['fn']])[0], pose['fn'], legs, b['body'], True)
    # Arka but kavsi
    svg.brush(body_xf([(150, 360), (156, 316), (190, 296), (222, 304)]), 5, INK, taper=0.6)
    # Kafa
    _head(svg, b, head_xf, pose, seed)
    # Tasma
    c0 = head_xf([(300, 250)])[0]
    c1 = head_xf([(352, 274)])[0]
    svg.brush([c0, ((c0[0] + c1[0]) / 2, (c0[1] + c1[1]) / 2 + 8), c1], 16, b['collar'], taper=0.1)
    tag = ((c0[0] + c1[0]) / 2 + 6, (c0[1] + c1[1]) / 2 + 20)
    svg.circle(tag[0], tag[1], 9, S.MUSTARD if b['collar'] != S.MUSTARD else S.CORAL)
    svg.brush(offset(ellipse_pts(tag[0], tag[1], 9, 9, 8), 1, -1), 3, INK, closed=True)


# ---------- Kıvrılmış uyku (nefes ve uyanma kareleri) ----------

def dog_curl(svg, coat, breath=0.0, awake=False, twitch=0.0, seed=1, shadow=True):
    """Simit gibi kıvrılmış köpek (sağa bakar). Başı ön patilerinin üstünde.
    breath 0..1 karın şişmesi, awake: kafa kalkık ve gözler açık, twitch: kulak kıpırdaması."""
    b = BREEDS[coat]
    body_c, ear_c = b['body'], b['ear']
    cx, base = 250, 438
    sx, sy = 1 + 0.015 * breath, 1 + 0.05 * breath
    def bxf(pts):
        return [(cx + (x - cx) * sx, base + (y - base) * sy) for x, y in pts]
    if shadow:
        svg.ellipse(262, 444, 200, 13, INK, opacity=0.09)
    # Gövde (arkada, solda)
    body = bxf([(352, 432), (358, 330), (318, 268), (236, 244), (150, 252), (86, 292), (64, 356), (84, 418), (190, 440)])
    if b['fluffy']:
        body = scallop(body, b['fluffy'], 9)
    dense = svg.blob(body, body_c, jitter=2)
    with svg.clip(dense):
        tb = bxf([(110, 268), (320, 350)])
        texture(svg, b, (tb[0][0], tb[0][1], tb[1][0], tb[1][1]), seed)
    svg.brush(bxf([(206, 420), (204, 360), (168, 318), (110, 320), (80, 356)]), 6, INK, taper=0.6)
    svg.brush(offset(bxf([(318, 268), (236, 244), (150, 252), (86, 292), (64, 356), (80, 412)]), 4, -3), 6, INK, taper=0.5)
    # Kuyruk: gövdenin önünden dolanır
    tail = bxf([(80, 410), (140, 436), (230, 446), (300, 444)])
    svg.brush(tail, 34, body_c, taper=0.45, wobble=0.03)
    tip_c = ear_c if b['tail'] in ('plume', 'pom', 'curl') else shade(body_c, 0.85)
    svg.brush(bxf([(262, 446), (300, 444), (330, 436)]), 28, tip_c, taper=0.6)
    svg.brush(tr(bxf([(88, 398), (140, 420), (230, 428), (290, 428)]), 0, 0), 4.5, INK, taper=0.5)
    svg.brush(offset(bxf([(84, 424), (140, 452), (230, 462), (320, 452)]), 3, -2), 5, INK, taper=0.5)
    # Ön patiler (kafanın altında, öne uzanmış)
    for px, py in [(404, 428), (452, 432)]:
        svg.blob(ellipse_pts(px, py, 30, 15, 10), body_c, jitter=0.8)
        svg.brush(offset(arc_pts(px, py, 30, 20, 160, 6, ry=15), 2, -1), 4.5, INK, taper=0.5)
        svg.brush([(px + 16, py - 4), (px + 18, py + 8)], 3, INK, taper=0.3)
    # Kafa
    head_rot = -26 if awake else 0
    head_dy = -34 if awake else 0
    pivot = (330, 400)
    def hxf(pts):
        return rot(tr(pts, 0, head_dy), pivot[0], pivot[1] + head_dy, head_rot)
    if awake:  # boyun
        svg.blob(hxf([(300, 330), (380, 330), (390, 420), (310, 430)]), body_c)
    skull = ellipse_pts(356, 344, 84, 70, 14)
    if b['fluffy']:
        skull = scallop(skull, b['fluffy'] * 0.6, 8)
    svg.blob(hxf(skull), body_c, jitter=1)
    svg.blob(hxf([(414, 352), (462, 352), (480, 378), (464, 404), (418, 406)]), b['muzzle'], jitter=0.8)
    svg.brush(offset(hxf(arc_pts(356, 344, 86, 115, 330, 12, ry=72)), 4, -3), 6, INK, taper=0.45)
    svg.brush(offset(hxf([(462, 352), (482, 378), (466, 404)]), 3, -2), 5, INK, taper=0.6)
    nose = hxf([(476, 368)])[0]
    svg.blob(ellipse_pts(nose[0], nose[1], 14, 11, 8), INK)
    svg.circle(nose[0] - 3, nose[1] - 4, 3.5, S.WHITE, opacity=0.7)
    svg.brush(hxf([(474, 386), (462, 398), (444, 396)]), 4.5, INK, taper=0.6)
    eye = hxf([(398, 330)])[0]
    if awake:
        svg.circle(eye[0], eye[1], 21, S.WHITE)
        svg.blob(ellipse_pts(eye[0], eye[1], 12, 15, 10), INK)
        svg.circle(eye[0] + 3, eye[1] - 5, 4.5, S.WHITE)
        svg.brush(hxf([(380, 298), (398, 292), (414, 298)]), 5, INK, taper=0.5)
    else:
        svg.brush(arc_pts(eye[0], eye[1], 14, 15, 165, 7), 6, INK, taper=0.35)
        svg.brush(hxf([(386, 312), (400, 307), (412, 311)]), 3.5, INK, taper=0.6)
    cheek = hxf([(420, 382)])[0]
    svg.ellipse(cheek[0], cheek[1], 14, 8, S.CORAL_LIGHT, opacity=0.55)
    # Kulak
    ears = b['ears']
    if ears in ('floppy', 'pom'):
        ang = (-30 if awake else 0) + twitch * 16
        ear = hxf(rot([(332, 280), (300, 284), (286, 326), (294, 384), (318, 398), (340, 362), (344, 316)], 326, 282, ang))
        if ears == 'pom':
            ear = scallop(ear, 6, 6)
        svg.blob(ear, ear_c, jitter=1.2)
        svg.brush(offset(ear[:5], 3, -2), 5, INK, taper=0.55)
    else:
        ang = (6 if awake else 38) - twitch * 18
        pts = [(330, 292), (320, 210), (346, 208), (376, 282)]
        ear = hxf(rot(pts, 340, 292, ang))
        svg.blob(ear, ear_c if ear_c != body_c else shade(body_c, 0.9), jitter=1)
        svg.blob(hxf(rot([(338, 284), (334, 230), (358, 280)], 340, 292, ang)), S.PINK, jitter=0.5)
        if ears == 'folded':
            svg.blob(hxf(rot([(320, 210), (346, 208), (350, 236), (328, 232)], 340, 292, ang)), shade(ear_c, 0.82))
        svg.brush(offset(ear, 3, -2), 5, INK, taper=0.5)


# ---------- Önden oturan köpek (kuyruk sallama, göz kırpma) ----------

FRONT_POSES = {
    'sit0':  dict(tail=-55, eyes='open', mouth='smile', head=-5),
    'sit1':  dict(tail=0, eyes='open', mouth='smile', head=0),
    'sit2':  dict(tail=50, eyes='open', mouth='smile', head=5),
    'blink': dict(tail=0, eyes='closed', mouth='smile', head=0),
    'happy': dict(tail=20, eyes='happy', mouth='tongue', head=-4),
    'gasp':  dict(tail=40, eyes='wide', mouth='open', head=6),
}


def dog_front(svg, coat, pose_name, seed=1, shadow=True):
    b = BREEDS[coat]
    pose = FRONT_POSES[pose_name]
    body_c, ear_c = b['body'], b['ear']
    legs = b['legs']
    drop = (1 - legs) * 60
    if shadow:
        svg.ellipse(256, 448, 150, 13, INK, opacity=0.09)
    # Kuyruk (arkada, sağdan görünür)
    base = (330, 420)
    tail_pts = rot([(330, 420), (382, 400), (418, 360), (424, 318)], base[0], base[1], pose['tail'])
    svg.brush(tail_pts, 26 if b['tail'] != 'stub' else 22, body_c, taper=0.6)
    svg.brush(offset(tail_pts, 3, -2), 5, INK, taper=0.6)
    # Gövde (armut) + arka butlar
    body = tr([(204, 262), (308, 262), (348, 330), (360, 420), (256, 446), (152, 420), (164, 330)], 0, drop * 0.3)
    if b['fluffy']:
        body = scallop(body, b['fluffy'], 9)
    dense = svg.blob(body, body_c, jitter=1.5)
    with svg.clip(dense):
        texture(svg, b, (170, 330, 340, 420), seed)
    for hx in (176, 336):
        hb = ellipse_pts(hx, 416, 54, 36, 12)
        svg.blob(hb, body_c, jitter=1)
        svg.brush(offset(arc_pts(hx, 416, 56, 190 if hx < 256 else 200, 340 if hx < 256 else 350, 8, ry=38), 3, -2), 5, INK, taper=0.5)
    svg.blob(ellipse_pts(256, 318, 46, 52, 12), b['muzzle'], jitter=1)
    # Ön bacaklar
    for lx in (226, 286):
        svg.brush([(lx, 320), (lx - 2 if lx < 256 else lx + 2, 440)], 36, body_c, taper=0.1)
        svg.blob(ellipse_pts(lx - 2 if lx < 256 else lx + 2, 442, 24, 13, 10), body_c)
        svg.brush(offset([(lx + 17 if lx > 256 else lx - 17, 330), (lx + 18 if lx > 256 else lx - 18, 436)], 2, -1), 5, INK, taper=0.6)
        svg.brush(arc_pts(lx - 2 if lx < 256 else lx + 2, 442, 24, 20, 160, 6, ry=13), 4.5, INK, taper=0.5)
    svg.brush(offset([(204, 266), (166, 330), (150, 410)], 3, -2), 6, INK, taper=0.5)
    svg.brush(offset([(308, 266), (348, 330), (362, 410)], 3, -2), 6, INK, taper=0.5)
    # Kafa
    hc = (256, 172 + drop * 0.3)
    def hxf(pts):
        return rot(pts, hc[0], hc[1] + 60, pose['head'])
    ears = b['ears']
    # arkadaki dik kulaklar
    if ears in ('pointy', 'folded'):
        for sgn in (-1, 1):
            pts = [(hc[0] + sgn * 40, hc[1] - 70), (hc[0] + sgn * 70, hc[1] - 150), (hc[0] + sgn * 96, hc[1] - 140), (hc[0] + sgn * 98, hc[1] - 50)]
            ear = hxf(pts)
            svg.blob(ear, ear_c if ear_c != body_c else shade(body_c, 0.9), jitter=1)
            inner = hxf([(hc[0] + sgn * 56, hc[1] - 74), (hc[0] + sgn * 72, hc[1] - 126), (hc[0] + sgn * 86, hc[1] - 70)])
            svg.blob(inner, S.PINK, jitter=0.5)
            if ears == 'folded':
                svg.blob(hxf([(hc[0] + sgn * 70, hc[1] - 150), (hc[0] + sgn * 96, hc[1] - 140), (hc[0] + sgn * 92, hc[1] - 112), (hc[0] + sgn * 72, hc[1] - 118)]), shade(ear_c, 0.82))
            svg.brush(offset(ear, 3, -2), 5, INK, taper=0.5)
    skull = ellipse_pts(hc[0], hc[1], 104, 94, 16)
    if b['fluffy']:
        skull = scallop(skull, b['fluffy'] * 0.7, 9)
    svg.blob(hxf(skull), body_c, jitter=1)
    if ears == 'pom':
        svg.blob(hxf(scallop(ellipse_pts(hc[0], hc[1] - 92, 62, 38, 16), 7, 8)), body_c, jitter=1)
        svg.brush(offset(hxf(arc_pts(hc[0], hc[1] - 92, 64, 190, 350, 8, ry=40)), 3, -2), 5, INK, taper=0.5)
    svg.brush(offset(hxf(arc_pts(hc[0], hc[1], 106, 200, 340, 10, ry=96)), 4, -3), 6, INK, taper=0.4)
    # burun bölgesi
    svg.blob(hxf(ellipse_pts(hc[0], hc[1] + 44, 54, 38, 12)), b['muzzle'], jitter=0.8)
    nose = hxf([(hc[0], hc[1] + 24)])[0]
    svg.blob(ellipse_pts(nose[0], nose[1], 22, 15, 10), INK)
    svg.circle(nose[0] - 6, nose[1] - 5, 4.5, S.WHITE, opacity=0.7)
    m = pose['mouth']
    if m == 'tongue':
        svg.blob(hxf([(hc[0] - 14, hc[1] + 50), (hc[0] + 14, hc[1] + 50), (hc[0] + 16, hc[1] + 80), (hc[0], hc[1] + 88), (hc[0] - 16, hc[1] + 80)]), S.CORAL)
        svg.brush(hxf([(hc[0], hc[1] + 56), (hc[0], hc[1] + 76)]), 3, S.CORAL_DARK, taper=0.4)
    elif m == 'open':
        svg.blob(hxf(ellipse_pts(hc[0], hc[1] + 64, 20, 18, 10)), '#7a2e2a')
        svg.blob(hxf(ellipse_pts(hc[0], hc[1] + 72, 13, 9, 8)), S.CORAL)
    svg.brush(hxf(arc_pts(hc[0] - 15, hc[1] + 40, 15, 15, 165, 7)), 5, INK, taper=0.4)
    svg.brush(hxf(arc_pts(hc[0] + 15, hc[1] + 40, 15, 15, 165, 7)), 5, INK, taper=0.4)
    svg.brush(hxf([(hc[0], hc[1] + 34), (hc[0], hc[1] + 44)]), 4, INK, taper=0.3)
    # gözler
    for sgn in (-1, 1):
        e = hxf([(hc[0] + sgn * 44, hc[1] - 10)])[0]
        st = pose['eyes']
        if st == 'closed':
            svg.brush(arc_pts(e[0], e[1], 13, 15, 165, 7), 6, INK, taper=0.35)
        elif st == 'happy':
            svg.brush(arc_pts(e[0], e[1] + 6, 13, 195, 345, 7), 6, INK, taper=0.35)
        else:
            r = 19 if st == 'wide' else 16
            if st == 'wide':
                svg.circle(e[0], e[1], r + 6, S.WHITE)
            svg.blob(ellipse_pts(e[0], e[1], r * 0.78, r, 10), INK)
            svg.circle(e[0] + 4, e[1] - 6, r * 0.3, S.WHITE)
        cheek = hxf([(hc[0] + sgn * 66, hc[1] + 30)])[0]
        svg.ellipse(cheek[0], cheek[1], 15, 9, S.CORAL_LIGHT, opacity=0.55)
    # sarkık / ponpon kulaklar (önde)
    if ears in ('floppy', 'pom'):
        for sgn in (-1, 1):
            pts = [(hc[0] + sgn * 70, hc[1] - 74), (hc[0] + sgn * 108, hc[1] - 56), (hc[0] + sgn * 124, hc[1] + 10),
                   (hc[0] + sgn * 112, hc[1] + 70), (hc[0] + sgn * 86, hc[1] + 72), (hc[0] + sgn * 80, hc[1] + 6)]
            ear = hxf(pts)
            if ears == 'pom':
                ear = scallop(ear, 7, 6)
            svg.blob(ear, ear_c, jitter=1.2)
            svg.brush(offset(ear[:5], 3, -2), 5, INK, taper=0.5)
    # tasma
    cy = hc[1] + 92
    svg.brush([(196, cy - 4), (256, cy + 12), (316, cy - 4)], 16, b['collar'], taper=0.1)
    svg.circle(256, cy + 22, 10, S.MUSTARD if b['collar'] != S.MUSTARD else S.CORAL)
    svg.brush(offset(ellipse_pts(256, cy + 22, 10, 10, 8), 1, -1), 3, INK, closed=True)


# ---------- Kedi (smokin kedi: siyah gövde, krem göğüs/pati, nane gözler) ----------

CAT = dict(body='#2B2725', light='#4A4440', chest=S.CREAM, eye=S.MINT, nose=S.PINK, collar=S.CORAL)

CAT_POSES = {
    'walk0': dict(fn=(14, -6), ff=(-10, 4), bn=(-12, 6), bf=(12, -6), bob=0, tilt=0, head=0, tail=-60, arch=0, eyes='open'),
    'walk1': dict(fn=(4, 0), ff=(2, -2), bn=(-2, 2), bf=(2, -2), bob=-3, tilt=0, head=1, tail=-55, arch=0, eyes='open'),
    'walk2': dict(fn=(-10, 4), ff=(14, -6), bn=(12, -6), bf=(-12, 6), bob=0, tilt=0, head=0, tail=-60, arch=0, eyes='open'),
    'walk3': dict(fn=(2, -2), ff=(4, 0), bn=(2, -2), bf=(-2, 2), bob=-3, tilt=0, head=-1, tail=-65, arch=0, eyes='open'),
    'startle': dict(fn=(-4, 2), ff=(-8, 4), bn=(8, -2), bf=(12, -4), bob=-30, tilt=0, head=12, tail=-75, arch=1.5, eyes='wide'),
    'run0': dict(fn=(-30, 30), ff=(-14, 20), bn=(42, -55), bf=(30, -42), bob=-4, tilt=2, head=4, tail=-20, arch=0, eyes='wide'),
    'run1': dict(fn=(42, -10), ff=(28, -6), bn=(-22, 25), bf=(-6, 15), bob=-14, tilt=-4, head=-2, tail=-10, arch=0, eyes='wide'),
    'run2': dict(fn=(66, -4), ff=(54, -4), bn=(-60, 20), bf=(-48, 15), bob=-22, tilt=-2, head=0, tail=0, arch=0, eyes='wide'),
    'run3': dict(fn=(8, 6), ff=(-4, 10), bn=(12, -25), bf=(24, -30), bob=-8, tilt=4, head=3, tail=-12, arch=0, eyes='wide'),
}


def _cat_leg(svg, hip, angles, color, near, paw):
    a1, a2 = angles
    l1, l2 = 58, 56
    knee = (hip[0] + l1 * math.sin(math.radians(a1)), hip[1] + l1 * math.cos(math.radians(a1)))
    foot = (knee[0] + l2 * math.sin(math.radians(a1 + a2)), knee[1] + l2 * math.cos(math.radians(a1 + a2)))
    svg.brush([hip, knee, foot], 28 if near else 24, color, taper=0.15, wobble=0.03)
    svg.blob(ellipse_pts(foot[0] + 6, foot[1] + 3, 17, 10, 10), paw if near else color)
    if near:
        svg.brush(offset(arc_pts(foot[0] + 6, foot[1] + 3, 17, 20, 160, 6, ry=10), 2, -1), 4, INK, taper=0.5)


def cat_side(svg, pose_name, shadow=True):
    c = CAT
    pose = CAT_POSES[pose_name]
    bob, tilt, arch = pose['bob'], pose['tilt'], pose['arch']
    center = (246, 318)
    def bxf(pts):
        return tr(rot(pts, center[0], center[1], tilt), 0, bob)
    if shadow:
        svg.ellipse(250, GROUND + 6, 140 * max(0.4, 1 + pose['bob'] / 40), 11, INK, opacity=0.1)
    far = c['light']
    _cat_leg(svg, bxf([(174, 340)])[0], pose['bf'], far, False, far)
    _cat_leg(svg, bxf([(316, 338)])[0], pose['ff'], far, False, far)
    # Kuyruk: uzun, S kıvrımlı
    base = bxf([(132, 300)])[0]
    tail = rot([(base[0], base[1]), (base[0] - 50, base[1] - 10), (base[0] - 78, base[1] - 50), (base[0] - 66, base[1] - 96), (base[0] - 40, base[1] - 118)],
               base[0], base[1], pose['tail'] + 60)
    svg.brush(tail, 26 if arch else 20, c['body'], taper=0.35, wobble=0.05)
    # Gövde (kamburlaşınca sırt yükselir)
    hump = -40 * arch
    body = bxf([(128, 318), (150, 276), (214, 262 + hump), (300, 262 + hump * 0.6), (354, 282), (366, 320), (340, 356), (250, 364), (160, 358)])
    if arch:
        body = scallop(body, 8, 11)
    svg.blob(body, c['body'], jitter=1.2)
    svg.blob(bxf([(330, 300), (362, 304), (360, 350), (326, 356)]), c['chest'], jitter=1)
    svg.brush(bxf([(168, 350), (172, 318), (200, 300), (230, 306)]), 4, c['light'], taper=0.6)
    _cat_leg(svg, bxf([(186, 344)])[0], pose['bn'], c['body'], True, c['chest'])
    _cat_leg(svg, bxf([(330, 344)])[0], pose['fn'], c['body'], True, c['chest'])
    # Kafa
    neck = bxf([(340, 288)])[0]
    def hxf(pts):
        return rot(tr(pts, neck[0] - 340, neck[1] - 288), neck[0], neck[1], pose['head'] + tilt)
    ear_back = 22 if arch else 0
    for ex in ((334, 214), (382, 206)):
        ear = hxf(rot([(ex[0] - 18, ex[1] + 14), (ex[0] + 2, ex[1] - 50), (ex[0] + 26, ex[1] + 6)], ex[0], ex[1] + 10, ear_back))
        svg.blob(ear, c['body'], jitter=0.8)
        svg.blob(hxf(rot([(ex[0] - 6, ex[1] + 8), (ex[0] + 3, ex[1] - 30), (ex[0] + 16, ex[1] + 6)], ex[0], ex[1] + 10, ear_back)), c['nose'])
    skull = ellipse_pts(372, 252, 66, 58, 14)
    svg.blob(hxf(skull), c['body'], jitter=0.8)
    svg.blob(hxf(ellipse_pts(414, 276, 32, 22, 10)), c['chest'], jitter=0.6)
    eye = hxf([(398, 240)])[0]
    r = 16 if pose['eyes'] == 'wide' else 13
    svg.blob(ellipse_pts(eye[0], eye[1], r, r * 1.05, 10), c['eye'])
    svg.blob(ellipse_pts(eye[0] + 2, eye[1], r * 0.32, r * 0.85, 8), INK)
    svg.circle(eye[0] + 5, eye[1] - 5, 3.5, S.WHITE)
    nose = hxf([(438, 266)])[0]
    svg.poly([(nose[0] - 8, nose[1] - 5), (nose[0] + 6, nose[1] - 5), (nose[0] - 1, nose[1] + 5)], c['nose'])
    svg.brush(hxf([(436, 276), (428, 286), (414, 284)]), 4, INK, taper=0.6)
    for dy, ang in ((-4, -8), (6, 6)):
        w0 = hxf([(424, 276 + dy)])[0]
        svg.brush([w0, (w0[0] + 52, w0[1] + ang)], 2.6, S.CREAM, taper=0.7, opacity=0.9)
    # Tasma ve zil
    c0, c1 = hxf([(326, 286)])[0], hxf([(366, 306)])[0]
    svg.brush([c0, c1], 13, c['collar'], taper=0.1)
    bell = ((c0[0] + c1[0]) / 2 + 4, (c0[1] + c1[1]) / 2 + 16)
    svg.circle(bell[0], bell[1], 9, S.MUSTARD)
    svg.brush([(bell[0] - 6, bell[1] + 2), (bell[0] + 6, bell[1] + 2)], 2.5, INK, taper=0)


def cat_front(svg, frame=0, shadow=True):
    """Önden oturan kedi; frame kuyruğun konumu (0/1)."""
    c = CAT
    if shadow:
        svg.ellipse(256, 450, 120, 12, INK, opacity=0.1)
    tail_ang = (-20, 25)[frame % 2]
    base = (300, 430)
    tail = rot([base, (360, 420), (400, 380), (392, 320), (370, 300)], base[0], base[1], tail_ang)
    svg.brush(tail, 20, c['body'], taper=0.35)
    body = [(206, 270), (306, 270), (338, 340), (342, 430), (256, 448), (170, 430), (174, 340)]
    svg.blob(body, c['body'], jitter=1.2)
    svg.blob(ellipse_pts(256, 330, 40, 60, 12), c['chest'], jitter=1)
    for lx in (228, 284):
        svg.brush([(lx, 340), (lx, 440)], 26, c['body'], taper=0.1)
        svg.blob(ellipse_pts(lx, 444, 19, 11, 10), c['chest'])
    hc = (256, 196)
    for sgn in (-1, 1):
        ear = [(hc[0] + sgn * 30, hc[1] - 60), (hc[0] + sgn * 64, hc[1] - 126), (hc[0] + sgn * 84, hc[1] - 40)]
        svg.blob(ear, c['body'])
        svg.blob([(hc[0] + sgn * 44, hc[1] - 60), (hc[0] + sgn * 62, hc[1] - 102), (hc[0] + sgn * 72, hc[1] - 52)], c['nose'])
    svg.blob(ellipse_pts(hc[0], hc[1], 86, 76, 16), c['body'], jitter=0.8)
    svg.blob(ellipse_pts(hc[0], hc[1] + 36, 46, 30, 12), c['chest'], jitter=0.6)
    for sgn in (-1, 1):
        e = (hc[0] + sgn * 36, hc[1] - 6)
        svg.blob(ellipse_pts(e[0], e[1], 16, 18, 10), c['eye'])
        svg.blob(ellipse_pts(e[0], e[1], 5, 15, 8), INK)
        svg.circle(e[0] + 5, e[1] - 6, 4, S.WHITE)
        for dy, ang in ((26, -6), (36, 4)):
            svg.brush([(hc[0] + sgn * 30, hc[1] + dy), (hc[0] + sgn * 96, hc[1] + dy + ang)], 2.6, S.CREAM, taper=0.7, opacity=0.9)
    svg.poly([(hc[0] - 9, hc[1] + 20), (hc[0] + 9, hc[1] + 20), (hc[0], hc[1] + 30)], c['nose'])
    svg.brush(arc_pts(hc[0] - 9, hc[1] + 34, 9, 15, 165, 6), 4, S.CREAM, taper=0.4)
    svg.brush(arc_pts(hc[0] + 9, hc[1] + 34, 9, 15, 165, 6), 4, S.CREAM, taper=0.4)
    svg.brush([(196, hc[1] + 72), (256, hc[1] + 86), (316, hc[1] + 72)], 13, c['collar'], taper=0.1)
    svg.circle(256, hc[1] + 98, 10, S.MUSTARD)


# ---------- Kare sayfaları (oyundaki DogFrame / CatFrame sırasıyla AYNI olmalı) ----------

DOG_FRAMES = [
    ('side', 'stand'), ('side', 'alert'), ('side', 'bark'), ('side', 'wag0'), ('side', 'wag1'),
    ('side', 'run0'), ('side', 'run1'), ('side', 'run2'), ('side', 'run3'),
    ('curl', 0.0), ('curl', 1.0), ('curl', 'twitch'), ('curl', 'wake'),
    ('front', 'sit0'), ('front', 'sit1'), ('front', 'sit2'), ('front', 'blink'), ('front', 'happy'), ('front', 'gasp'),
]
DOG_SHEET_COLS = 5

CAT_FRAMES = ['walk0', 'walk1', 'walk2', 'walk3', 'startle', 'run0', 'run1', 'run2', 'run3', ('front', 0), ('front', 1)]
CAT_SHEET_COLS = 4


def draw_dog_frame(svg, coat, frame, seed):
    kind, arg = frame
    if kind == 'side':
        dog_side(svg, coat, arg, seed=seed)
    elif kind == 'curl':
        if arg == 'twitch':
            dog_curl(svg, coat, 0.5, twitch=1.0, seed=seed)
        elif arg == 'wake':
            dog_curl(svg, coat, 0.0, awake=True, seed=seed)
        else:
            dog_curl(svg, coat, arg, seed=seed)
    else:
        dog_front(svg, coat, arg, seed=seed)


def draw_cat_frame(svg, frame):
    if isinstance(frame, tuple):
        cat_front(svg, frame[1])
    else:
        cat_side(svg, frame)


# ---------- Pati izi ipuçları (rakam yerine) ----------

PIPS = {
    1: [(128, 128)],
    2: [(86, 86), (170, 170)],
    3: [(76, 76), (128, 128), (180, 180)],
    4: [(84, 84), (172, 84), (84, 172), (172, 172)],
    5: [(78, 78), (178, 78), (128, 128), (78, 178), (178, 178)],
    6: [(86, 66), (170, 66), (86, 128), (170, 128), (86, 190), (170, 190)],
    7: [(66, 72), (128, 72), (190, 72), (128, 128), (66, 184), (128, 184), (190, 184)],
    8: [(66, 70), (128, 70), (190, 70), (96, 128), (160, 128), (66, 186), (128, 186), (190, 186)],
}
HINT_COLORS = ['#3558A8', '#3E8A55', '#E0553A', '#7A4A9A', '#A8741C', '#22867E', '#211D1A', '#6B5F55']


def paw_shape(svg, cx, cy, size, color, angle):
    k = size / 128
    def P(pts):
        return rot([(cx + (x - 64) * k, cy + (y - 64) * k) for x, y in pts], cx, cy, angle)
    svg.blob(P([(64, 70), (90, 84), (96, 108), (78, 118), (64, 112), (50, 118), (32, 108), (38, 84)]), color, jitter=0.4)
    for tx, ty, r, a in ((30, 56, 11, -20), (50, 36, 11, -8), (78, 36, 11, 8), (98, 56, 11, 20)):
        c = P([(tx, ty)])[0]
        svg.blob(ellipse_pts(c[0], c[1], r * k, r * 1.25 * k, 8, rot=a + angle), color, jitter=0.3)


def paw_hint(svg, count):
    import random as _r
    rnd = _r.Random(count * 7)
    pips = PIPS[count]
    size = {1: 96, 2: 80, 3: 72, 4: 74, 5: 64, 6: 58, 7: 52, 8: 50}[count]
    k = svg.w / 256
    for (x, y) in pips:
        paw_shape(svg, x * k, y * k, size * k, HINT_COLORS[count - 1], rnd.uniform(-18, 18))


def puff(svg, frame):
    """Toz bulutu: koşan köpeğin arkasında (3 kare: büyür ve dağılır)."""
    k = svg.w / 128
    r = (18, 26, 30)[frame] * k
    alpha = (1.0, 0.85, 0.5)[frame]
    for dx, dy, s in ((-16, 6, 1.0), (10, 2, 0.85), (0, -12, 0.75), (20, -10, 0.6)):
        cx, cy = 64 * k + dx * k * (1 + frame * 0.6), 74 * k + dy * k * (1 + frame * 0.4)
        svg.circle(cx, cy, r * s, S.CREAM, opacity=alpha)
    if frame < 2:
        svg.brush(arc_pts(64 * k, 74 * k, r * 1.5, 200, 340, 7), 3 * k, INK, taper=0.5, opacity=0.5 * alpha)


def cat_icon(svg):
    """Kedi düğmesi ikonu: önden kedi kafası."""
    c = CAT
    k = svg.w / 256
    hc = (128 * k, 140 * k)
    for sgn in (-1, 1):
        svg.blob([(hc[0] + sgn * 30 * k, hc[1] - 52 * k), (hc[0] + sgn * 70 * k, hc[1] - 118 * k), (hc[0] + sgn * 92 * k, hc[1] - 30 * k)], c['body'])
        svg.blob([(hc[0] + sgn * 46 * k, hc[1] - 56 * k), (hc[0] + sgn * 66 * k, hc[1] - 96 * k), (hc[0] + sgn * 78 * k, hc[1] - 44 * k)], c['nose'])
    svg.blob(ellipse_pts(hc[0], hc[1], 92 * k, 80 * k, 16), c['body'], jitter=0.8)
    svg.blob(ellipse_pts(hc[0], hc[1] + 38 * k, 48 * k, 32 * k, 12), c['chest'], jitter=0.6)
    for sgn in (-1, 1):
        e = (hc[0] + sgn * 38 * k, hc[1] - 6 * k)
        svg.blob(ellipse_pts(e[0], e[1], 17 * k, 19 * k, 10), c['eye'])
        svg.blob(ellipse_pts(e[0], e[1], 5 * k, 16 * k, 8), INK)
        svg.circle(e[0] + 5 * k, e[1] - 6 * k, 4 * k, S.WHITE)
    svg.poly([(hc[0] - 10 * k, hc[1] + 22 * k), (hc[0] + 10 * k, hc[1] + 22 * k), (hc[0], hc[1] + 32 * k)], c['nose'])
    svg.brush(offset(ellipse_pts(hc[0], hc[1], 94 * k, 82 * k, 14)[8:14], 3, -2), 5 * k, INK, taper=0.5)
