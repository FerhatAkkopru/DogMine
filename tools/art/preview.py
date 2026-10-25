"""Çizimleri hızlıca görmek için temas sayfası üretir (yalnızca geliştirme)."""
import io, sys
import cairosvg
from PIL import Image, ImageDraw, ImageFont


def render(svg, size):
    png = cairosvg.svg2png(bytestring=svg.to_svg().encode(), output_width=size, output_height=size * svg.h // svg.w)
    return Image.open(io.BytesIO(png)).convert('RGBA')


def sheet(items, cell=300, cols=3, bg='#F8F1E4', path='sheet.png'):
    rows = (len(items) + cols - 1) // cols
    img = Image.new('RGBA', (cols * cell, rows * (cell + 30)), bg)
    d = ImageDraw.Draw(img)
    for i, (label, svg) in enumerate(items):
        im = render(svg, cell)
        x, y = (i % cols) * cell, (i // cols) * (cell + 30)
        img.alpha_composite(im, (x, y))
        d.text((x + 8, y + cell + 6), label, fill='#211d1a')
    img.convert('RGB').save(path)
