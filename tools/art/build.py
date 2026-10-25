"""Bütün oyun görsellerini üretir ve Unity'ye koyar.

Çalıştırma (proje kökünden):  python3 tools/art/build.py
Çıktı: Assets/Resources/Art/**.png (+ .meta içe aktarma ayarları)
       Assets/Resources/Fonts/*.ttf
Gereken: pip install cairosvg pillow numpy
"""
import hashlib
import io
import os
import shutil
import sys

import cairosvg
import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'Resources', 'Art')
FONTS_OUT = os.path.join(ROOT, 'Assets', 'Resources', 'Fonts')

from sketch import Svg  # noqa: E402
import dogs as D  # noqa: E402
import props as P  # noqa: E402
import style as S  # noqa: E402

# Her kürk için kulak tipi sabit: aynı renk köpek her pozda aynı köpek gibi görünsün.
EARS = {'hardal': 'floppy', 'lacivert': 'floppy', 'mercan': 'pointy', 'pembe': 'floppy', 'nane': 'pointy', 'krem': 'pointy'}
ASCII = {'hardal': 'mustard', 'lacivert': 'navy', 'mercan': 'coral', 'pembe': 'pink', 'nane': 'mint', 'krem': 'cream'}


def guid_for(path):
    """Yol başına sabit GUID: dosya yeniden üretilince Unity referansları bozulmasın."""
    rel = os.path.relpath(path, ROOT).replace(os.sep, '/')
    return hashlib.md5(('uyuyan-kopekler:' + rel).encode()).hexdigest()


TEXTURE_META = '''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: {mips}
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: {wrap}
    wrapV: {wrap}
    wrapW: {wrap}
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 2
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'''

FOLDER_META = '''fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'''

FONT_META = '''fileFormatVersion: 2
guid: {guid}
TrueTypeFontImporter:
  externalObjects: {{}}
  serializedVersion: 4
  fontSize: 16
  forceTextureCase: -2
  characterSpacing: 0
  characterPadding: 1
  includeFontData: 1
  fontNames:
  - {name}
  fallbackFontReferences: []
  customCharacters: 
  fontRenderingMode: 0
  ascentCalculationMode: 1
  useLegacyBoundsCalculation: 0
  shouldRoundAdvanceValue: 1
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'''

written = []


def ensure_folder_meta(folder):
    """Klasör ve üst klasörleri için .meta yazar (Assets'e kadar)."""
    folder = os.path.abspath(folder)
    assets = os.path.join(ROOT, 'Assets')
    while folder != assets and folder.startswith(assets):
        meta = folder + '.meta'
        if not os.path.exists(meta):
            with open(meta, 'w') as f:
                f.write(FOLDER_META.format(guid=guid_for(folder)))
        folder = os.path.dirname(folder)


def save_png(img, rel, mips=True, wrap=1):
    path = os.path.join(OUT, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    ensure_folder_meta(os.path.dirname(path))
    img.save(path, optimize=True)
    with open(path + '.meta', 'w') as f:
        f.write(TEXTURE_META.format(guid=guid_for(path), mips=1 if mips else 0, wrap=wrap))
    written.append(rel)


def render(svg, width=None, height=None):
    png = cairosvg.svg2png(bytestring=svg.to_svg().encode('utf-8'), output_width=width or svg.w, output_height=height or svg.h)
    return Image.open(io.BytesIO(png)).convert('RGBA')


def bleed(img):
    """Saydam piksellere komşu rengini ver: küçültürken kenarda koyu hale olmasın."""
    a = np.asarray(img).astype(np.float32)
    rgb, alpha = a[..., :3], a[..., 3:4]
    known = alpha[..., 0] > 0
    out = rgb.copy()
    for _ in range(4):
        acc = np.zeros_like(out)
        cnt = np.zeros(known.shape, np.float32)
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                k = np.roll(np.roll(known, dy, 0), dx, 1)
                acc += np.roll(np.roll(out, dy, 0), dx, 1) * k[..., None]
                cnt += k
        fill = (~known) & (cnt > 0)
        out[fill] = acc[fill] / cnt[fill][:, None]
        known = known | fill
    res = np.concatenate([out, alpha], axis=-1).clip(0, 255).astype(np.uint8)
    return Image.fromarray(res, 'RGBA')


def build(fn, size, *args, seed=3, height=None, **kw):
    svg = Svg(size, height or size, seed=seed)
    fn(svg, *args, **kw)
    return bleed(render(svg))


def paper_texture(size=512, seed=4):
    """Döşenebilir kâğıt dokusu: ince tane + hafif lekeler."""
    rnd = np.random.default_rng(seed)
    base = np.array([int(S.PAPER[i:i + 2], 16) for i in (1, 3, 5)], np.float32)
    grain = rnd.normal(0, 2.6, (size, size))
    blotch = rnd.normal(0, 1, (size // 16, size // 16))
    blotch = np.kron(blotch, np.ones((16, 16)))
    for _ in range(6):  # kenarları sararak bulanıklaştır → döşenebilir
        blotch = (blotch + np.roll(blotch, 8, 0) + np.roll(blotch, -8, 0) + np.roll(blotch, 8, 1) + np.roll(blotch, -8, 1)) / 5
    v = grain + blotch * 2.2
    img = np.clip(base[None, None, :] + v[..., None] * np.array([1.0, 0.95, 0.85]), 0, 255).astype(np.uint8)
    rgba = np.concatenate([img, np.full((size, size, 1), 255, np.uint8)], axis=-1)
    return Image.fromarray(rgba, 'RGBA')


def main():
    if os.path.isdir(OUT):
        shutil.rmtree(OUT)

    # Köpekler
    for i, coat in enumerate(S.COAT_ORDER):
        name, ears = ASCII[coat], EARS[coat]
        save_png(build(D.sleeping_dog, 512, coat, ears=ears, seed=i + 1), f'Dogs/sleep_{name}.png')
        save_png(build(D.curled_pup, 512, coat, seed=i + 1), f'Dogs/curl_{name}.png')
        save_png(build(D.awake_dog, 512, coat, ears=ears, seed=i + 1), f'Dogs/awake_{name}.png')

    # Bahçe
    for v in range(3):
        save_png(build(P.grass_tile, 256, v, False, seed=v + 10), f'Board/grass_{v}_a.png')
        save_png(build(P.grass_tile, 256, v, True, seed=v + 20), f'Board/grass_{v}_b.png')
    for v in range(2):
        save_png(build(P.soil_tile, 256, v, seed=v + 30), f'Board/soil_{v}.png')
    save_png(build(P.board_bed, 160, seed=40), 'Board/bed.png', mips=False)
    save_png(build(P.bowl, 256, seed=41), 'Board/bowl.png')

    # Eşyalar
    save_png(build(P.heart, 256, True, seed=50), 'Items/heart.png')
    save_png(build(P.heart, 256, False, seed=51), 'Items/heart_empty.png')
    save_png(build(P.bone, 256, seed=52), 'Items/bone.png')
    save_png(build(P.star, 256, seed=53), 'Items/star.png')
    save_png(build(P.pearl, 256, seed=54), 'Items/pearl.png')
    save_png(build(P.collar, 256, seed=55), 'Items/collar.png')
    save_png(build(P.paw, 128, seed=56), 'Items/paw.png')

    # İkonlar (beyaz)
    for name, fn in [('pause', P.icon_pause), ('back', P.icon_back), ('check', P.icon_check),
                     ('cross', P.icon_cross), ('lock', P.icon_lock), ('gear', P.icon_gear)]:
        save_png(build(fn, 128, seed=60), f'Icons/{name}.png')

    # Arayüz parçaları (beyaz, 9 parçalı)
    save_png(build(P.ui_box_fill, 128, seed=70), 'UI/box_fill.png', mips=False)
    save_png(build(P.ui_box_line, 128, seed=71), 'UI/box_line.png', mips=False)
    save_png(build(P.ui_circle_fill, 128, seed=72), 'UI/circle_fill.png')
    save_png(build(P.ui_circle_line, 128, seed=73), 'UI/circle_line.png')
    save_png(build(P.underline, 512, seed=74, height=128), 'UI/underline.png', mips=False)
    save_png(paper_texture(), 'UI/paper.png', mips=False, wrap=0)

    # Açılış sahnesi
    save_png(build(P.bed_back, 1000, seed=80, height=560), 'Hero/bed_back.png', mips=False)
    save_png(build(P.bed_front, 1000, seed=81, height=560), 'Hero/bed_front.png', mips=False)

    # Yazı tipi
    os.makedirs(FONTS_OUT, exist_ok=True)
    ensure_folder_meta(FONTS_OUT)
    for src, dst in [('ShantellSans-ExtraBold.ttf', 'ShantellSans-ExtraBold.ttf'), ('ShantellSans-Bold.ttf', 'ShantellSans-Bold.ttf')]:
        s = os.path.join(HERE, 'fonts', src)
        d = os.path.join(FONTS_OUT, dst)
        shutil.copyfile(s, d)
        with open(d + '.meta', 'w') as f:
            f.write(FONT_META.format(guid=guid_for(d), name='Shantell Sans'))
    shutil.copyfile(os.path.join(HERE, 'fonts', 'OFL.txt'), os.path.join(FONTS_OUT, 'OFL.txt'))
    with open(os.path.join(FONTS_OUT, 'OFL.txt.meta'), 'w') as f:
        f.write(f'fileFormatVersion: 2\nguid: {guid_for(os.path.join(FONTS_OUT, "OFL.txt"))}\nTextScriptImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')

    print(f'{len(written)} görsel yazıldı → {os.path.relpath(OUT, ROOT)}')


if __name__ == '__main__':
    main()
