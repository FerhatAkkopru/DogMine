using System;
using UnityEngine;

namespace UyuyanKopekler.Art
{
    // Kod içinde resim çizmek için küçük bir tuval.
    // Koordinatlar genişliğe göre normalize: x 0..1, y 0..(yükseklik/genişlik), (0,0) sol alt.
    public sealed class PixelCanvas
    {
        readonly Color[] pixels;
        readonly float pixelSize;

        public int Width { get; }
        public int Height { get; }
        public float Top => Height / (float)Width;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            pixelSize = 1f / width;
            pixels = new Color[width * height];
        }

        public PixelCanvas(int size) : this(size, size) { }

        public Rect Bounds => new Rect(0f, 0f, 1f, Top);

        public Color GetPixel(int x, int y) => pixels[y * Width + x];

        // sdf ile tarif edilen şekli renkle doldurur. feather > 1 kenarı daha yumuşak yapar.
        public void Fill(Func<Vector2, float> sdf, Color color, float feather = 1f) =>
            Fill(sdf, _ => color, Bounds, feather);

        // Yalnızca verilen dikdörtgenin içini tarar (küçük şekillerde çok daha hızlı).
        public void Fill(Func<Vector2, float> sdf, Color color, Rect area, float feather = 1f) =>
            Fill(sdf, _ => color, area, feather);

        // Şekli, konuma göre değişen bir renkle (degrade, gölge) doldurur.
        public void Fill(Func<Vector2, float> sdf, Func<Vector2, Color> colorAt, float feather = 1f) =>
            Fill(sdf, colorAt, Bounds, feather);

        public void Fill(Func<Vector2, float> sdf, Func<Vector2, Color> colorAt, Rect area, float feather = 1f)
        {
            float edge = pixelSize * Mathf.Max(0.01f, feather);
            int x0 = Mathf.Clamp(Mathf.FloorToInt(area.xMin / pixelSize) - 2, 0, Width);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(area.xMax / pixelSize) + 2, 0, Width);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(area.yMin / pixelSize) - 2, 0, Height);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(area.yMax / pixelSize) + 2, 0, Height);

            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++)
                {
                    var p = new Vector2((x + 0.5f) * pixelSize, (y + 0.5f) * pixelSize);
                    float coverage = Mathf.Clamp01(0.5f - sdf(p) / edge);
                    if (coverage <= 0f) continue;
                    var c = colorAt(p);
                    c.a *= coverage;
                    Blend(y * Width + x, c);
                }
            }
        }

        // Bütün tuvali dikey bir degrade ile boyar.
        public void VerticalGradient(Color bottom, Color top)
        {
            for (int y = 0; y < Height; y++)
            {
                var c = Color.Lerp(bottom, top, Height == 1 ? 0f : y / (float)(Height - 1));
                for (int x = 0; x < Width; x++) Blend(y * Width + x, c);
            }
        }

        public Texture2D ToTexture(string name)
        {
            BleedTransparentEdges();
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.SetPixels(pixels);
            tex.Apply(false, true); // GPU'ya gönder, CPU kopyasını bırak
            return tex;
        }

        // Normal alfa harmanlama ("over").
        void Blend(int index, Color src)
        {
            var dst = pixels[index];
            float outA = src.a + dst.a * (1f - src.a);
            if (outA <= 0f) return;
            float k = dst.a * (1f - src.a);
            pixels[index] = new Color(
                (src.r * src.a + dst.r * k) / outA,
                (src.g * src.a + dst.g * k) / outA,
                (src.b * src.a + dst.b * k) / outA,
                outA);
        }

        // Tamamen saydam piksellere komşularının rengini verir; böylece doku büyütülüp
        // küçültülürken kenarlarda koyu bir hale oluşmaz.
        void BleedTransparentEdges()
        {
            var known = new bool[pixels.Length];
            for (int i = 0; i < pixels.Length; i++) known[i] = pixels[i].a > 0f;

            for (int pass = 0; pass < 3; pass++)
            {
                var filled = new bool[pixels.Length];
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = y * Width + x;
                        if (known[i]) continue;

                        float r = 0f, g = 0f, b = 0f;
                        int n = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                int nx = x + dx, ny = y + dy;
                                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height) continue;
                                int j = ny * Width + nx;
                                if (!known[j]) continue;
                                r += pixels[j].r; g += pixels[j].g; b += pixels[j].b; n++;
                            }
                        }
                        if (n == 0) continue;
                        pixels[i] = new Color(r / n, g / n, b / n, 0f);
                        filled[i] = true;
                    }
                }
                for (int i = 0; i < pixels.Length; i++) known[i] |= filled[i];
            }
        }
    }
}
