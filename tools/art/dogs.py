"""Köpek çizimleri. Bütün çizimler 512×512 tasarım alanında, y aşağı doğru."""
from sketch import Svg, INK, ellipse_pts, arc_pts, offset, mirror_x
import style as S


def _texture(svg, coat, box, seed):
    x0, y0, x1, y1 = box
    p = coat['pattern']
    if p == 'loops':
        svg.scribble_loops(x0, y0, x1, y1, coat['texture'], width=4, rows=3, loop=20, seed=seed)
    elif p == 'strokes':
        svg.scribble_strokes(x0, y0, x1, y1, coat['texture'], count=9, length=46, angle=-25, width=4, seed=seed)
    elif p == 'stripes':
        for i in range(5):
            x = x0 + (x1 - x0) * (i + 0.5) / 5
            svg.brush([(x - 8, y0), (x + 6, (y0 + y1) / 2), (x - 4, y1)], 5, coat['texture'], taper=0.5)
    elif p == 'dashes':
        svg.scribble_strokes(x0, y0, x1, y1, coat['texture'], count=12, length=22, angle=10, width=6, seed=seed)


def sleeping_dog(svg, coat_name, ears='floppy', seed=1, shadow=True):
    """Başını ön patilerine koymuş, yan yatıp uyuyan köpek (sola bakar)."""
    coat = S.COATS[coat_name]
    body_c, ear_c = coat['body'], coat['ear']
    dx, dy = 4, -3  # kayık mürekkep

    if shadow:
        svg.ellipse(270, 448, 225, 14, INK, opacity=0.08)

    # Kuyruk (gövdenin arkasında)
    tail = [(440, 372), (478, 392), (496, 424), (472, 444), (418, 448)]
    svg.brush(tail, 30, body_c, taper=0.7, wobble=0.04)
    svg.brush(offset([(456, 392), (488, 412), (490, 436), (462, 450)], dx, dy), 5, taper=0.7)

    # Gövde
    body = [(178, 300), (232, 272), (320, 262), (402, 270), (452, 306), (464, 362), (452, 410),
            (420, 442), (300, 446), (200, 444), (164, 410), (160, 350)]
    dense = svg.blob(body, body_c, jitter=1.5)
    with svg.clip(dense):
        _texture(svg, coat, (240, 280, 450, 400), seed)
        if coat['patch']:
            svg.blob(ellipse_pts(340, 288, 72, 40, 10), coat['patch'], jitter=4)

    # Arka bacak: but kavsi ve öne uzanan ayak
    svg.brush([(338, 424), (334, 362), (366, 322), (420, 318), (456, 350)], 6, taper=0.6)
    svg.blob(ellipse_pts(352, 438, 46, 13, 10), body_c, jitter=1)
    svg.brush([(310, 444), (350, 452), (396, 444)], 5, taper=0.7)

    # Ön bacaklar: öne uzanmış, patiler burnun altından görünür
    for (y, x_end) in [(410, 52), (426, 34)]:
        leg = [(214, y - 10), (150, y - 11), (x_end + 18, y - 9), (x_end, y + 3), (x_end + 14, y + 15), (150, y + 16), (214, y + 14)]
        svg.blob(leg, body_c, jitter=1)
        svg.brush(offset([(200, y - 11), (140, y - 12), (x_end + 14, y - 9), (x_end - 1, y + 3), (x_end + 12, y + 15)], dx, dy), 5, taper=0.55)
        svg.brush([(x_end + 14, y - 2), (x_end + 22, y + 6)], 3.5, taper=0.3)

    # Kafa: kafatası + uzun burun, çene patilerin üstünde
    head = [(92, 334), (114, 282), (162, 250), (214, 262), (238, 304), (232, 356), (206, 392),
            (152, 404), (96, 402), (58, 390), (42, 366), (56, 344)]
    svg.blob(head, body_c, jitter=1)

    # Kulak
    if ears == 'floppy':
        ear = [(168, 256), (216, 268), (230, 314), (218, 376), (194, 390), (170, 366), (160, 304)]
        svg.blob(ear, ear_c, jitter=1.5)
        svg.brush([(166, 262), (162, 312), (174, 366), (194, 388)], 5, taper=0.6)
    else:  # dik kulak
        ear = [(168, 264), (186, 196), (204, 188), (226, 240), (226, 282)]
        svg.blob(ear, ear_c, jitter=1.5)
        svg.blob([(186, 252), (196, 210), (212, 244)], body_c)
        svg.brush(offset([(168, 266), (186, 196), (204, 188), (228, 244)], dx, dy), 5, taper=0.5)

    # Yüz: kapalı göz, burun, ağız, kaş
    svg.brush(arc_pts(128, 314, 15, 15, 165, 7), 6, taper=0.35)
    svg.brush([(116, 294), (130, 289), (142, 292)], 3.5, taper=0.6)
    svg.blob(ellipse_pts(46, 362, 16, 12, 8), INK)
    svg.circle(41, 357, 3.5, S.WHITE, opacity=0.7)
    svg.brush([(52, 382), (70, 390), (92, 388)], 4.5, taper=0.6)

    # Kayık mürekkep kontur
    svg.brush(offset([(162, 352), (178, 300), (232, 272), (320, 262), (402, 270), (452, 306), (464, 362), (452, 410)], dx, dy), 6, taper=0.5)
    svg.brush(offset([(56, 344), (92, 334), (114, 282), (162, 250), (214, 262), (238, 304)], dx, dy), 6, taper=0.5)
    svg.brush(offset([(42, 368), (58, 390), (96, 402)], dx, dy), 5, taper=0.7)
    svg.brush([(180, 372), (172, 386)], 3.5, taper=0.5)
    svg.brush([(192, 380), (186, 394)], 3.5, taper=0.5)


def awake_dog(svg, coat_name, ears='floppy', seed=1, shadow=True):
    """Uyanmış, dört ayak üstünde havlayan köpek (sola bakar)."""
    coat = S.COATS[coat_name]
    body_c, ear_c = coat['body'], coat['ear']
    dx, dy = 4, -3
    ground = 452

    if shadow:
        svg.ellipse(290, ground + 2, 170, 13, INK, opacity=0.08)

    # Kuyruk: havada sallanıyor
    svg.brush([(418, 276), (452, 238), (470, 196), (462, 168)], 28, body_c, taper=0.75, wobble=0.04)
    svg.brush(offset([(426, 268), (458, 232), (474, 196), (466, 166)], dx, dy), 5, taper=0.6)
    for a in [(486, 214, 506, 204), (488, 186, 508, 172)]:
        svg.brush([(a[0], a[1]), (a[2], a[3])], 4, taper=0.4)

    def leg(x_top, x_bottom, shade):
        pts = [(x_top - 18, 320), (x_top + 18, 320), (x_bottom + 14, ground - 16), (x_bottom + 22, ground - 2),
               (x_bottom - 20, ground), (x_bottom - 14, ground - 16)]
        svg.blob(pts, shade, jitter=1, smooth=True)
        svg.brush(offset([(x_top + 16, 330), (x_bottom + 14, ground - 14), (x_bottom + 22, ground)], dx, dy), 5, taper=0.6)

    # Arkadaki bacaklar (biraz koyu), sonra gövde, sonra öndeki bacaklar
    leg(395, 410, coat['ear'] if coat_name != 'lacivert' else '#24304a')
    leg(222, 206, coat['ear'] if coat_name != 'lacivert' else '#24304a')

    body = [(176, 252), (252, 232), (340, 230), (404, 244), (436, 282), (430, 332), (396, 352),
            (300, 356), (212, 352), (176, 324)]
    dense = svg.blob(body, body_c, jitter=1.5)
    with svg.clip(dense):
        _texture(svg, coat, (220, 248, 420, 340), seed)
        if coat['patch']:
            svg.blob(ellipse_pts(330, 256, 64, 34, 10), coat['patch'], jitter=4)

    leg(370, 380, body_c)
    leg(196, 174, body_c)

    # Boyun ve göğüs: kafayı gövdeye bağlar
    svg.blob([(146, 232), (200, 214), (240, 248), (236, 310), (196, 334), (164, 300)], body_c, jitter=1)
    svg.brush(offset([(150, 252), (160, 292), (184, 330)], dx, dy), 5, taper=0.6)
    svg.brush([(172, 300), (166, 314)], 3.5, taper=0.4)
    svg.brush([(184, 306), (180, 320)], 3.5, taper=0.4)

    # Ağız içi ve dil (çenelerin arkasında)
    svg.blob([(116, 204), (66, 214), (62, 240), (84, 262), (124, 240)], '#7a2e2a')
    svg.blob([(78, 236), (100, 230), (112, 246), (96, 262), (78, 256)], S.CORAL_LIGHT if coat_name == 'mercan' else S.CORAL)

    # Alt çene (açık)
    svg.blob([(122, 236), (84, 256), (70, 268), (86, 276), (132, 262)], body_c, jitter=0.8)

    # Kafa ve üst çene
    head = [(124, 248), (104, 214), (66, 210), (40, 198), (44, 176), (76, 166), (104, 158), (122, 120),
            (166, 104), (210, 118), (230, 158), (224, 204), (196, 240), (160, 258)]
    svg.blob(head, body_c, jitter=1)

    # Kulak
    if ears == 'floppy':  # havaya kalkmış sarkık kulak
        ear = [(172, 112), (206, 92), (250, 96), (268, 120), (240, 128), (208, 140)]
        svg.blob(ear, ear_c, jitter=1.5)
        svg.brush(offset([(172, 112), (206, 92), (250, 96), (268, 120)], dx, dy), 5, taper=0.5)
    else:
        ear = [(150, 112), (164, 50), (182, 42), (204, 96), (198, 128)]
        svg.blob(ear, ear_c, jitter=1.5)
        svg.blob([(166, 104), (174, 66), (190, 102)], body_c)
        svg.brush(offset([(150, 114), (164, 50), (182, 42), (206, 100)], dx, dy), 5, taper=0.5)

    # Göz: şaşkın, açık
    svg.circle(146, 160, 17, S.WHITE)
    svg.circle(140, 162, 9, INK)
    svg.circle(137, 158, 3, S.WHITE)
    svg.brush([(124, 128), (142, 120), (162, 126)], 5, taper=0.5)

    # Burun
    svg.blob(ellipse_pts(44, 184, 15, 11, 8), INK)
    svg.circle(40, 180, 3.5, S.WHITE, opacity=0.7)

    # Kayık mürekkep kontur
    svg.brush(offset([(44, 176), (76, 166), (104, 158), (122, 120), (166, 104), (210, 118), (230, 158), (224, 204)], dx, dy), 6, taper=0.5)
    svg.brush(offset([(40, 198), (66, 210), (104, 214)], dx, dy), 4.5, taper=0.6)
    svg.brush(offset([(84, 256), (70, 268), (88, 278), (132, 264)], dx, dy), 4.5, taper=0.6)
    svg.brush(offset([(212, 238), (252, 232), (340, 230), (404, 244), (436, 282), (430, 332), (396, 352)], dx, dy), 6, taper=0.5)
    svg.brush(offset([(236, 354), (300, 358), (350, 354)], dx, dy), 5, taper=0.7)

    # Havlama çizgileri
    for pts in [[(28, 150), (8, 136)], [(22, 176), (0, 174)], [(28, 204), (8, 216)]]:
        svg.brush(pts, 5, taper=0.4)


def curled_pup(svg, coat_name, seed=1, shadow=True):
    """Simit gibi kıvrılmış, burnunu kuyruğuna gömmüş köpek (sola bakar)."""
    coat = S.COATS[coat_name]
    body_c, ear_c = coat['body'], coat['ear']
    tail_tip = coat['ear']
    dx, dy = 4, -3
    if shadow:
        svg.ellipse(262, 430, 196, 15, INK, opacity=0.08)
    # Gövde: yuvarlak sırt
    body = [(92, 400), (96, 320), (150, 252), (250, 222), (350, 230), (420, 270), (446, 340), (430, 404), (330, 428), (180, 428)]
    dense = svg.blob(body, body_c, jitter=2)
    with svg.clip(dense):
        _texture(svg, coat, (220, 240, 430, 340), seed)
        if coat['patch']:
            svg.blob(ellipse_pts(330, 262, 74, 40, 10), coat['patch'], jitter=4)
    # Arka bacak kavsi ve ayak
    svg.brush([(318, 410), (316, 350), (352, 306), (412, 306), (440, 344)], 6, taper=0.6)
    # Kafa: gövdenin önünde, aşağı eğik, burun kuyruğa gömülü
    head = [(72, 372), (86, 316), (130, 276), (190, 268), (236, 300), (246, 352), (220, 392), (150, 404), (96, 400)]
    svg.blob(head, body_c, jitter=1)
    svg.brush(offset([(78, 350), (90, 312), (130, 276), (190, 266), (238, 300), (248, 350)], dx, dy), 6, taper=0.5)
    # Kulak
    ear = [(158, 276), (204, 270), (226, 306), (216, 358), (190, 368), (168, 334)]
    svg.blob(ear, ear_c, jitter=1.5)
    svg.brush([(158, 282), (166, 330), (190, 366)], 5, taper=0.6)
    # Yüz: kapalı göz, burun ucu kuyruğun arkasından görünür
    svg.brush(arc_pts(120, 334, 14, 15, 165, 7), 6, taper=0.35)
    svg.brush([(108, 312), (122, 306), (136, 310)], 3.5, taper=0.6)
    svg.blob(ellipse_pts(70, 380, 13, 10, 8), INK)
    # Kuyruk: burnun önünden dolanır, ucu koyu
    tail = [(436, 380), (380, 414), (280, 430), (170, 426), (96, 412), (60, 396)]
    svg.brush(tail, 40, body_c, taper=0.45, wobble=0.03)
    svg.brush([(110, 414), (74, 404), (52, 392)], 30, tail_tip, taper=0.5)
    svg.brush(offset([(430, 366), (380, 398), (280, 414), (170, 410), (96, 396)], 0, 0), 5, taper=0.5)
    svg.brush(offset([(436, 396), (380, 432), (280, 448), (160, 444), (84, 428), (50, 404)], dx, dy), 5, taper=0.5)
    # Sırt konturu
    svg.brush(offset([(250, 222), (350, 230), (420, 270), (446, 340), (434, 396)], dx, dy), 6, taper=0.5)


def belly_up_dog(svg, coat_name, seed=1, shadow=True):
    """Sırt üstü yatmış, patileri havada, dili dışarıda mışıl mışıl uyuyan köpek."""
    coat = S.COATS[coat_name]
    body_c, ear_c = coat['body'], coat['ear']
    dx, dy = 4, -3
    if shadow:
        svg.ellipse(256, 430, 210, 15, INK, opacity=0.08)
    # Havadaki bacaklar (gövdenin arkasında)
    legs = [[(206, 326), (190, 252), (176, 214)], [(250, 318), (252, 246), (262, 208)],
            [(326, 326), (346, 256), (356, 222)], [(370, 334), (402, 274), (420, 246)]]
    for i, pts in enumerate(legs):
        svg.brush(pts, 34, body_c if i % 2 == 0 else ear_c, taper=0.15, wobble=0.04)
        svg.brush(offset(pts, dx + 14, dy), 5, taper=0.5)
        tip = pts[-1]
        svg.blob(ellipse_pts(tip[0], tip[1], 20, 16, 8), body_c if i % 2 == 0 else ear_c)
    # Gövde (göbek yukarıda)
    body = [(150, 400), (154, 344), (214, 306), (316, 300), (404, 318), (440, 360), (430, 404), (330, 424), (220, 424)]
    dense = svg.blob(body, body_c, jitter=1.5)
    with svg.clip(dense):
        svg.blob(ellipse_pts(300, 336, 92, 30, 10), S.WHITE, opacity=0.55)
        _texture(svg, coat, (200, 360, 420, 410), seed)
    # Kafa: solda, ters (çene yukarı)
    head = [(66, 410), (52, 362), (78, 318), (130, 306), (176, 330), (186, 380), (160, 420), (110, 430)]
    svg.blob(head, body_c, jitter=1)
    ear = [(70, 410), (34, 418), (20, 444), (52, 452), (96, 430)]
    svg.blob(ear, ear_c, jitter=1.5)
    # Yüz: kapalı gözler (ters), dil dışarıda
    svg.brush(arc_pts(100, 380, 12, 195, 345, 7), 5.5, taper=0.35)
    svg.brush(arc_pts(140, 386, 12, 195, 345, 7), 5.5, taper=0.35)
    svg.blob(ellipse_pts(120, 322, 14, 10, 8), INK)
    svg.blob([(146, 330), (170, 318), (182, 330), (168, 344)], S.CORAL)
    svg.brush([(140, 334), (160, 340)], 4, taper=0.5)
    # Kontur
    svg.brush(offset([(150, 400), (154, 344), (214, 306), (316, 300), (404, 318), (440, 360), (430, 404)], dx, dy), 6, taper=0.5)
    svg.brush(offset([(52, 362), (78, 318), (130, 306), (176, 330)], dx, dy), 6, taper=0.5)
    # Kuyruk
    svg.brush([(436, 392), (476, 404), (498, 388)], 22, body_c, taper=0.7)
