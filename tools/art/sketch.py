"""Elle çizilmiş görünümlü vektör çizimler için küçük bir kütüphane.

Şekiller nokta listeleriyle tarif edilir; noktalar Catmull-Rom eğrisiyle yumuşatılır.
- blob():  kapalı, dolgulu şekil (köpek gövdesi, kafa, kulak...)
- brush(): uçları incelen mürekkep çizgisi (konturlar, bacaklar, yüz çizgileri)
- clip():  bir şeklin içine kırpılmış doku karalamaları
Çıktı SVG'dir; cairosvg ile PNG'ye çevrilir.
"""
import math
import random

INK = '#211d1a'


def _catmull(pts, closed, samples):
    """Noktalardan geçen yumuşak eğriyi yoğun nokta listesi olarak döner."""
    n = len(pts)
    if n < 2:
        return list(pts)
    out = []
    segs = n if closed else n - 1
    for i in range(segs):
        p0 = pts[(i - 1) % n] if closed or i > 0 else pts[0]
        p1 = pts[i]
        p2 = pts[(i + 1) % n]
        p3 = pts[(i + 2) % n] if closed or i + 2 < n else pts[-1]
        for s in range(samples):
            t = s / samples
            t2, t3 = t * t, t * t * t
            out.append(tuple(
                0.5 * ((2 * p1[k]) + (-p0[k] + p2[k]) * t + (2 * p0[k] - 5 * p1[k] + 4 * p2[k] - p3[k]) * t2
                       + (-p0[k] + 3 * p1[k] - 3 * p2[k] + p3[k]) * t3) for k in (0, 1)))
    if not closed:
        out.append(tuple(pts[-1]))
    return out


def _jitter(pts, amount, seed):
    if amount <= 0:
        return [tuple(p) for p in pts]
    rnd = random.Random(seed)
    return [(x + rnd.uniform(-amount, amount), y + rnd.uniform(-amount, amount)) for x, y in pts]


def _fmt(pts):
    return ' '.join(f'{x:.1f},{y:.1f}' for x, y in pts)


def ellipse_pts(cx, cy, rx, ry, n=10, rot=0.0, start=0.0):
    a0 = math.radians(rot)
    pts = []
    for i in range(n):
        t = start + 2 * math.pi * i / n
        x, y = rx * math.cos(t), ry * math.sin(t)
        pts.append((cx + x * math.cos(a0) - y * math.sin(a0), cy + x * math.sin(a0) + y * math.cos(a0)))
    return pts


def arc_pts(cx, cy, r, a0, a1, n=8, ry=None):
    ry = r if ry is None else ry
    return [(cx + r * math.cos(math.radians(a0 + (a1 - a0) * i / (n - 1))),
             cy + ry * math.sin(math.radians(a0 + (a1 - a0) * i / (n - 1)))) for i in range(n)]


def offset(pts, dx, dy):
    return [(x + dx, y + dy) for x, y in pts]


def scale_pts(pts, s, cx=0, cy=0):
    return [(cx + (x - cx) * s, cy + (y - cy) * s) for x, y in pts]


def mirror_x(pts, axis):
    return [(2 * axis - x, y) for x, y in pts]


class Svg:
    def __init__(self, w, h, seed=1):
        self.w, self.h = w, h
        self.defs = []
        self.items = []
        self.stack = [self.items]
        self._ids = 0
        self.seed = seed

    def _next_seed(self):
        self.seed += 7919
        return self.seed

    def _add(self, s):
        self.stack[-1].append(s)

    # ---------- Şekiller ----------

    def blob(self, pts, fill, jitter=0.0, opacity=1.0, smooth=True):
        p = _jitter(pts, jitter, self._next_seed())
        dense = _catmull(p, True, 14) if smooth else p
        self._add(f'<polygon points="{_fmt(dense)}" fill="{fill}" fill-opacity="{opacity}"/>')
        return dense

    def holed(self, outer, inner, fill, opacity=1.0):
        """İçinde delik olan şekil (ör. dişli çark)."""
        d = 'M ' + ' L '.join(f'{x:.1f},{y:.1f}' for x, y in outer) + ' Z M ' + ' L '.join(f'{x:.1f},{y:.1f}' for x, y in inner) + ' Z'
        self._add(f'<path d="{d}" fill="{fill}" fill-opacity="{opacity}" fill-rule="evenodd"/>')

    def poly(self, pts, fill, opacity=1.0):
        self._add(f'<polygon points="{_fmt(pts)}" fill="{fill}" fill-opacity="{opacity}"/>')

    def circle(self, cx, cy, r, fill, opacity=1.0):
        self._add(f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="{r:.1f}" fill="{fill}" fill-opacity="{opacity}"/>')

    def ellipse(self, cx, cy, rx, ry, fill, rot=0.0, opacity=1.0):
        self._add(f'<ellipse cx="{cx:.1f}" cy="{cy:.1f}" rx="{rx:.1f}" ry="{ry:.1f}" fill="{fill}" '
                  f'fill-opacity="{opacity}" transform="rotate({rot:.1f} {cx:.1f} {cy:.1f})"/>')

    def rect(self, x, y, w, h, fill, rx=0, opacity=1.0):
        self._add(f'<rect x="{x:.1f}" y="{y:.1f}" width="{w:.1f}" height="{h:.1f}" rx="{rx:.1f}" '
                  f'fill="{fill}" fill-opacity="{opacity}"/>')

    # Uçları incelen fırça çizgisi. taper: uçlardaki incelme oranı (0 = yok).
    def brush(self, pts, width, color=INK, taper=0.6, closed=False, jitter=0.0, opacity=1.0, wobble=0.12):
        p = _jitter(pts, jitter, self._next_seed())
        dense = _catmull(p, closed, 16)
        if closed:
            dense = dense + [dense[0]]
        n = len(dense)
        if n < 2:
            return
        rnd = random.Random(self._next_seed())
        phase = rnd.uniform(0, 6.28)
        left, right = [], []
        for i, (x, y) in enumerate(dense):
            a = dense[max(i - 1, 0)]
            b = dense[min(i + 1, n - 1)]
            dx, dy = b[0] - a[0], b[1] - a[1]
            ln = math.hypot(dx, dy) or 1.0
            nx, ny = -dy / ln, dx / ln
            t = i / (n - 1)
            profile = 1.0 if closed else (1 - taper) + taper * math.sin(math.pi * t) ** 0.6
            w = width * profile * (1 + wobble * math.sin(phase + t * 9.0))
            left.append((x + nx * w / 2, y + ny * w / 2))
            right.append((x - nx * w / 2, y - ny * w / 2))
        outline = left + right[::-1]
        self._add(f'<polygon points="{_fmt(outline)}" fill="{color}" fill-opacity="{opacity}"/>')
        # Yuvarlak uçlar
        if not closed:
            w0 = width * (1 - taper) * 0.5
            self._add(f'<circle cx="{dense[0][0]:.1f}" cy="{dense[0][1]:.1f}" r="{w0:.1f}" fill="{color}" fill-opacity="{opacity}"/>')
            self._add(f'<circle cx="{dense[-1][0]:.1f}" cy="{dense[-1][1]:.1f}" r="{w0:.1f}" fill="{color}" fill-opacity="{opacity}"/>')

    # ---------- Kırpma ve gruplar ----------

    def clip(self, dense_pts):
        """Verilen şeklin içine kırpılmış bir grup açar (with ifadesiyle)."""
        svg = self

        class _Clip:
            def __enter__(self_inner):
                svg._ids += 1
                cid = f'c{svg._ids}'
                svg.defs.append(f'<clipPath id="{cid}"><polygon points="{_fmt(dense_pts)}"/></clipPath>')
                svg.stack.append([])
                self_inner.cid = cid
                return svg

            def __exit__(self_inner, *a):
                body = svg.stack.pop()
                svg._add(f'<g clip-path="url(#{self_inner.cid})">' + ''.join(body) + '</g>')
        return _Clip()

    def group(self, transform='', opacity=1.0):
        svg = self

        class _Group:
            def __enter__(self_inner):
                svg.stack.append([])
                return svg

            def __exit__(self_inner, *a):
                body = svg.stack.pop()
                svg._add(f'<g transform="{transform}" opacity="{opacity}">' + ''.join(body) + '</g>')
        return _Group()

    # ---------- Dokular ----------

    def scribble_loops(self, x0, y0, x1, y1, color, width=3.0, rows=4, loop=18, seed=0, opacity=0.9):
        """Referanstaki gibi gevşek, düzensiz ilmekli karalamalar (kısa parçalar halinde)."""
        rnd = random.Random(seed)
        for r in range(rows):
            y = y0 + (y1 - y0) * (r + 0.5) / rows + rnd.uniform(-8, 8)
            x = x0 + rnd.uniform(0, loop * 2)
            while x < x1:
                count = rnd.randint(2, 4)  # bir parçadaki ilmek sayısı
                pts = []
                for _ in range(count):
                    s = loop * rnd.uniform(0.75, 1.3)
                    by = y + rnd.uniform(-3, 3)
                    pts += [(x, by), (x + s * 0.6, by - s * 0.7), (x + s * 0.2, by - s * 0.95), (x + s * 0.4, by + s * 0.05)]
                    x += s * rnd.uniform(0.75, 1.0)
                if len(pts) > 3:
                    self.brush(pts, width, color, taper=0.5, opacity=opacity, wobble=0.08)
                x += loop * rnd.uniform(1.2, 2.6)  # parçalar arasında boşluk

    def scribble_strokes(self, x0, y0, x1, y1, color, count=6, length=40, angle=-20, width=3.0, seed=0, opacity=0.9):
        """Kısa, hafif kavisli tüy çizgileri."""
        rnd = random.Random(seed)
        for _ in range(count):
            x, y = rnd.uniform(x0, x1), rnd.uniform(y0, y1)
            a = math.radians(angle + rnd.uniform(-15, 15))
            L = length * rnd.uniform(0.7, 1.2)
            bend = rnd.uniform(-0.25, 0.25) * L
            pts = [(x, y), (x + math.cos(a) * L * 0.5 - math.sin(a) * bend, y + math.sin(a) * L * 0.5 + math.cos(a) * bend),
                   (x + math.cos(a) * L, y + math.sin(a) * L)]
            self.brush(pts, width, color, taper=0.8, opacity=opacity)

    def dots(self, x0, y0, x1, y1, color, count=10, r=3.0, seed=0, opacity=0.9):
        rnd = random.Random(seed)
        for _ in range(count):
            self.circle(rnd.uniform(x0, x1), rnd.uniform(y0, y1), r * rnd.uniform(0.7, 1.3), color, opacity)

    # ---------- Çıktı ----------

    def to_svg(self):
        return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{self.w}" height="{self.h}" '
                f'viewBox="0 0 {self.w} {self.h}"><defs>{"".join(self.defs)}</defs>{"".join(self.items)}</svg>')

    def save_png(self, path, width=None, height=None):
        import cairosvg
        cairosvg.svg2png(bytestring=self.to_svg().encode('utf-8'), write_to=path,
                         output_width=width or self.w, output_height=height or self.h)
