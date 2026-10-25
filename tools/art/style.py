"""Uyuyan Köpekler sanat yönü: sınırlı palet + mürekkep çizgi.

Referans: düz renkli, elle çizilmiş köpek illüstrasyonları (hardal, mercan, lacivert,
pembe, nane, krem) ve gevşek siyah mürekkep çizgiler, gövde içinde karalama dokular.
Bu dosyadaki renkler Unity'deki Palette.cs ile aynı tutulmalı.
"""

PAPER = '#F8F1E4'      # arka plan kâğıdı
PAPER_DARK = '#EDE2CC' # kâğıt gölgesi, kilitli şeyler
INK = '#211D1A'        # bütün çizgiler
INK_SOFT = '#6B5F55'

MUSTARD = '#DDB65A'
MUSTARD_DARK = '#B98F33'
MUSTARD_LIGHT = '#F2DC9C'
CORAL = '#EE6A4C'
CORAL_DARK = '#C94B31'
CORAL_LIGHT = '#F8A891'
NAVY = '#2E3C5A'
NAVY_LIGHT = '#6E81A8'
PINK = '#F3C3CD'
PINK_DARK = '#DE98A8'
PINK_LIGHT = '#FCE3E8'
MINT = '#A7D8CE'
MINT_DARK = '#7DBDB0'
MINT_LIGHT = '#D3EEE8'
CREAM = '#F7E3C3'
CREAM_DARK = '#E2C79B'
WHITE = '#FFFCF5'

LAWN = '#A9C98F'
LAWN_DARK = '#86AE6E'
LAWN_INK = '#4D7444'
SAND = '#EEDDB8'
SAND_DARK = '#D9C293'
SOIL_EDGE = '#8A6A4A'

# Köpek kürkleri: (gövde, doku, kulak/leke, doku türü)
COATS = {
    'hardal':  dict(body=MUSTARD, texture=MUSTARD_LIGHT, ear=MUSTARD_DARK, patch=None, pattern='loops'),
    'lacivert': dict(body=NAVY, texture=NAVY_LIGHT, ear=CORAL, patch=None, pattern='strokes'),
    'mercan':  dict(body=CORAL, texture=CORAL_LIGHT, ear=CORAL_DARK, patch=None, pattern='stripes'),
    'pembe':   dict(body=PINK, texture=WHITE, ear=PINK_DARK, patch=None, pattern='loops'),
    'nane':    dict(body=MINT, texture=PINK, ear=MINT_DARK, patch=None, pattern='dashes'),
    'krem':    dict(body=CREAM, texture=CREAM_DARK, ear=MUSTARD, patch=MUSTARD, pattern='strokes'),
}
COAT_ORDER = ['hardal', 'lacivert', 'mercan', 'pembe', 'nane', 'krem']
