"""Oyun seslerini hazırlar ve Unity'ye koyar.

- Arayüz/oyun sesleri: uisfx paketinden (CC0, https://github.com/romainsimon/uisfx) seçilen .ogg dosyaları.
  Paket npm'den indirilir: npm pack uisfx@0.4.0 (ya da tarball'ı açıp yolunu --uisfx ile verin).
- Havlama ve miyavlama: bu betikte sentezlenir (kaynak + formant süzgeci).

Çalıştırma: python3 tools/audio/build.py --uisfx /yol/uisfx/package
Çıktı: Assets/Resources/Audio/*.ogg|wav (+ .meta)
Gereken: pip install numpy soundfile
"""
import argparse
import hashlib
import os
import shutil

import numpy as np
import soundfile as sf

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'Resources', 'Audio')
SR = 44100

# Oyundaki ses adı (Sfx enum'u ile aynı) → uisfx paketi/ipucu
PICKS = {
    'tap': 'rubber/press',
    'dig': 'organic/press',
    'bigdig': 'organic/swipe',
    'bowl': 'rubber/check',
    'bowloff': 'rubber/uncheck',
    'collect': 'rubber/reward',
    'rare': 'organic/achievement',
    'heartlost': 'organic/error',
    'win': 'organic/complete',
    'lose': 'organic/warning',
    'whoosh': 'organic/swipe',
    'toggle': 'rubber/toggle-on',
    'open': 'rubber/open',
    'catdrop': 'organic/drop',
}


def guid_for(path):
    rel = os.path.relpath(path, ROOT).replace(os.sep, '/')
    return hashlib.md5(('uyuyan-kopekler:' + rel).encode()).hexdigest()


def write_meta(path):
    with open(path + '.meta', 'w') as f:
        f.write(f'fileFormatVersion: 2\nguid: {guid_for(path)}\n')


def resonator(x, freqs, bw):
    """Zamanla değişen ikinci derece rezonatör (formant). freqs: örnek başına frekans dizisi."""
    y = np.zeros_like(x)
    y1 = y2 = 0.0
    for i in range(len(x)):
        r = np.exp(-np.pi * bw / SR)
        c = 2 * r * np.cos(2 * np.pi * freqs[i] / SR)
        y0 = (1 - r) * x[i] + c * y1 - r * r * y2
        y[i] = y0
        y2, y1 = y1, y0
    return y


def voiced(f0, rng, breath=0.15):
    """Ses telleri kaynağı: f0 eğrisine göre testere dişi + biraz nefes gürültüsü."""
    phase = np.cumsum(f0 / SR)
    saw = 2 * (phase % 1.0) - 1
    return saw + breath * rng.normal(0, 1, len(f0))


def envelope(n, attack, decay):
    t = np.arange(n) / SR
    env = np.minimum(1.0, t / attack) * np.exp(-np.maximum(0, t - attack) / decay)
    return env


def finish(y, peak=0.55):
    y = y - np.mean(y)
    fade = min(len(y), int(0.01 * SR))
    y[-fade:] *= np.linspace(1, 0, fade)
    return (y / (np.abs(y).max() + 1e-9) * peak).astype(np.float32)


def bark(seed=1, pitch=1.0):
    """Kısa, tok bir "hav!"."""
    rng = np.random.default_rng(seed)
    n = int(0.24 * SR)
    t = np.arange(n) / SR
    f0 = pitch * (260 + 300 * np.exp(-t / 0.05) * np.minimum(1, t / 0.012))
    src = voiced(f0, rng, breath=0.25)
    f1 = 650 + 150 * np.exp(-t / 0.06)
    f2 = 1250 + 250 * np.exp(-t / 0.05)
    f3 = np.full(n, 2600.0)
    y = resonator(src, f1, 160) * 1.0 + resonator(src, f2, 220) * 0.6 + resonator(src, f3, 380) * 0.25
    onset = rng.normal(0, 1, n) * np.exp(-t / 0.012) * 0.4
    y = (y + resonator(onset, np.full(n, 900.0), 600)) * envelope(n, 0.008, 0.075)
    return finish(y)


def meow():
    """Yükselip alçalan "miyav"."""
    rng = np.random.default_rng(5)
    n = int(0.6 * SR)
    t = np.arange(n) / SR
    shape = np.sin(np.pi * np.clip(t / 0.6, 0, 1))
    f0 = 430 + 300 * shape - 60 * (t / 0.6) + 8 * np.sin(2 * np.pi * 6 * t)
    src = voiced(f0, rng, breath=0.08)
    k = np.clip(t / 0.6, 0, 1)
    f1 = np.interp(k, [0, 0.35, 1], [320, 820, 380])
    f2 = np.interp(k, [0, 0.35, 1], [2300, 1350, 850])
    y = resonator(src, f1, 120) + resonator(src, f2, 180) * 0.7 + resonator(src, np.full(n, 3000.0), 400) * 0.2
    env = np.minimum(1, t / 0.05) * np.minimum(1, (0.6 - t) / 0.12)
    return finish(y * env, peak=0.36)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--uisfx', required=True, help='uisfx paket klasörü (içinde sounds/ olan)')
    args = ap.parse_args()

    os.makedirs(OUT, exist_ok=True)
    if not os.path.exists(OUT + '.meta'):
        with open(OUT + '.meta', 'w') as f:
            f.write(f'fileFormatVersion: 2\nguid: {guid_for(OUT)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')

    for name, cue in PICKS.items():
        src = os.path.join(args.uisfx, 'sounds', cue + '.ogg')
        dst = os.path.join(OUT, name + '.ogg')
        shutil.copyfile(src, dst)
        write_meta(dst)

    for name, data in [('bark', bark(1, 1.0)), ('bark2', bark(2, 1.12)), ('meow', meow())]:
        dst = os.path.join(OUT, name + '.wav')
        sf.write(dst, data, SR, subtype='PCM_16')
        write_meta(dst)

    lic = os.path.join(OUT, 'LICENSE-AUDIO.txt')
    with open(lic, 'w') as f:
        f.write('Ogg sesler: uisfx (https://github.com/romainsimon/uisfx) — CC0 1.0 Universal (kamu malı).\n'
                'bark*.wav, meow.wav: tools/audio/build.py ile sentezlendi (projeye ait).\n')
    write_meta(lic)
    print('sesler yazıldı →', os.path.relpath(OUT, ROOT))


if __name__ == '__main__':
    main()
