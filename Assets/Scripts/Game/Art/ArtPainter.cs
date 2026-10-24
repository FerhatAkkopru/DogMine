using System;
using UnityEngine;

namespace UyuyanKopekler.Art
{
    // Oyunun bütün geçici görsellerini kodla çizer. Her metot yeni bir PixelCanvas döner.
    // Gerçek çizimler geldiğinde bu sınıf yerine içe aktarılmış sprite'lar kullanılacak.
    public static class ArtPainter
    {
        static Vector2 V(float x, float y) => new Vector2(x, y);
        static Color Grey(float v, float a = 1f) => new Color(v, v, v, a);

        // Bir şekli önce kalın (kontur rengi), sonra normal (dolgu rengi) çizer.
        static void Outlined(PixelCanvas c, Func<Vector2, float> sdf, float width, Color outline, Color fill)
        {
            c.Fill(p => sdf(p) - width, outline);
            c.Fill(sdf, fill);
        }

        // ---------- Bahçe ----------

        // Çimli kare: kabarık görünsün diye altta koyu bir kenar, üstte açık bir parlama.
        // Beyaz-gri çizilir; oyunda çim yeşiline boyanır.
        public static PixelCanvas TileGrass(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.47f), V(0.46f, 0.45f), 0.16f), Grey(0.72f));
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.53f), V(0.46f, 0.43f), 0.16f), Grey(0.92f));
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.82f), V(0.32f, 0.06f), 0.06f), Grey(1f, 0.55f), 3f);

            var blade = Grey(0.80f);
            foreach (var b in new[] { V(0.30f, 0.30f), V(0.66f, 0.56f), V(0.40f, 0.66f) })
            {
                var root = b;
                c.Fill(p => Sdf.Union(
                    Sdf.Segment(p, root, root + V(-0.045f, 0.08f), 0.014f),
                    Sdf.Segment(p, root, root + V(0.045f, 0.08f), 0.014f)), blade);
            }
            return c;
        }

        // Kazılmış toprak: üst kenarı gölgeli, içeri gömük görünür.
        public static PixelCanvas TileSoil(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.5f), V(0.46f, 0.46f), 0.14f), Grey(0.84f));
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.475f), V(0.45f, 0.435f), 0.13f), Grey(1f));
            c.Fill(p => Sdf.Ellipse(p, V(0.2f, 0.18f), V(0.035f, 0.025f)), Grey(0.88f));
            return c;
        }

        // Bahçenin arkasındaki tahta çerçeve (9 parçalı / sliced).
        public static PixelCanvas Frame(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.5f), V(0.49f, 0.49f), 0.34f), Grey(1f));
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.5f), V(0.40f, 0.40f), 0.26f), Grey(0.82f));
            return c;
        }

        // Çim biçilmiş gibi yatay şeritli arka plan.
        public static PixelCanvas Garden(int width, int height)
        {
            var c = new PixelCanvas(width, height);
            float top = c.Top;
            c.Fill(_ => -1f, p =>
            {
                var col = Color.Lerp(Palette.GardenBottom, Palette.GardenTop, p.y / top);
                bool stripe = Mathf.FloorToInt(p.y / top * 12f) % 2 == 0;
                return stripe ? Color.Lerp(col, Color.white, 0.05f) : col;
            });
            return c;
        }

        // ---------- Köpekler ----------

        static Func<Vector2, float> Ear(Vector2 center, Vector2 radii, float degrees) =>
            p => Sdf.Ellipse(Sdf.Rotate(p, center, degrees), center, radii);

        static void DogBase(PixelCanvas c, bool awake)
        {
            float earY = awake ? 0.63f : 0.53f;
            float earTilt = awake ? 38f : 18f;
            var leftEar = Ear(V(0.21f, earY), V(0.10f, 0.20f), -earTilt);
            var rightEar = Ear(V(0.79f, earY), V(0.10f, 0.20f), earTilt);
            Func<Vector2, float> head = p => Sdf.Ellipse(p, V(0.5f, 0.5f), V(0.31f, 0.285f));
            Func<Vector2, float> silhouette = p => Sdf.Union(head(p), Sdf.Union(leftEar(p), rightEar(p)));

            c.Fill(p => Sdf.Ellipse(p, V(0.5f, 0.12f), V(0.30f, 0.05f)), Palette.Shadow, 3f);
            c.Fill(p => silhouette(p) - 0.022f, Palette.DogOutline);
            c.Fill(leftEar, Palette.DogFurDark);
            c.Fill(rightEar, Palette.DogFurDark);
            c.Fill(head, p => Color.Lerp(Palette.DogFur, Color.white, Mathf.Clamp01((p.y - 0.5f) * 0.5f)));
            c.Fill(p => Sdf.Ellipse(p, V(0.63f, 0.61f), V(0.10f, 0.09f)), Palette.DogFurDark);
            c.Fill(p => Sdf.Ellipse(p, V(0.5f, 0.38f), V(0.165f, 0.115f)), Palette.DogMuzzle);
            c.Fill(p => Sdf.Circle(p, V(0.33f, 0.45f), 0.042f), Palette.Cheek, 4f);
            c.Fill(p => Sdf.Circle(p, V(0.67f, 0.45f), 0.042f), Palette.Cheek, 4f);
            c.Fill(p => Sdf.Ellipse(p, V(0.5f, 0.45f), V(0.058f, 0.04f)), Palette.DogNose);
            c.Fill(p => Sdf.Circle(p, V(0.483f, 0.462f), 0.012f), Grey(1f, 0.7f));
        }

        // Uyuyan köpek: kapalı "u" gözler, sakin ağız.
        public static PixelCanvas DogSleeping(int size)
        {
            var c = new PixelCanvas(size);
            DogBase(c, awake: false);
            var ink = Palette.DogNose;
            c.Fill(p => Sdf.LowerArc(p, V(0.39f, 0.565f), 0.045f, 0.018f), ink);
            c.Fill(p => Sdf.LowerArc(p, V(0.61f, 0.565f), 0.045f, 0.018f), ink);
            c.Fill(p => Sdf.Segment(p, V(0.5f, 0.42f), V(0.5f, 0.375f), 0.007f), ink);
            c.Fill(p => Sdf.LowerArc(p, V(0.465f, 0.375f), 0.035f, 0.013f), ink);
            c.Fill(p => Sdf.LowerArc(p, V(0.535f, 0.375f), 0.035f, 0.013f), ink);
            return c;
        }

        // Uyanmış köpek: açık gözler, kalkık kulaklar, açık ağız ve dil.
        public static PixelCanvas DogAwake(int size)
        {
            var c = new PixelCanvas(size);
            DogBase(c, awake: true);
            var ink = Palette.DogNose;
            foreach (float x in new[] { 0.39f, 0.61f })
            {
                float ex = x;
                c.Fill(p => Sdf.Ellipse(p, V(ex, 0.575f), V(0.058f, 0.068f)), Color.white);
                c.Fill(p => Sdf.Circle(p, V(ex + 0.006f, 0.565f), 0.035f), ink);
                c.Fill(p => Sdf.Circle(p, V(ex + 0.018f, 0.582f), 0.012f), Color.white);
            }
            c.Fill(p => Sdf.Segment(p, V(0.33f, 0.665f), V(0.43f, 0.69f), 0.012f), Palette.DogOutline);
            c.Fill(p => Sdf.Segment(p, V(0.57f, 0.69f), V(0.67f, 0.665f), 0.012f), Palette.DogOutline);
            c.Fill(p => Sdf.Segment(p, V(0.5f, 0.42f), V(0.5f, 0.385f), 0.007f), ink);
            c.Fill(p => Sdf.Ellipse(p, V(0.5f, 0.335f), V(0.075f, 0.06f)), Palette.Mouth);
            c.Fill(p => Sdf.Intersect(
                Sdf.Ellipse(p, V(0.5f, 0.305f), V(0.05f, 0.04f)),
                Sdf.Ellipse(p, V(0.5f, 0.335f), V(0.075f, 0.06f))), Palette.Tongue);
            return c;
        }

        // ---------- Eşyalar ----------

        // Mama kabı: içinde mama taneleri olan kırmızı kap, önünde küçük beyaz kemik.
        public static PixelCanvas Bowl(int size)
        {
            var c = new PixelCanvas(size);
            var rimCenter = V(0.5f, 0.5f);
            var rimRadii = V(0.40f, 0.085f);
            Func<Vector2, float> body = p => Sdf.Intersect(Sdf.Ellipse(p, rimCenter, V(0.40f, 0.32f)), p.y - rimCenter.y);
            Func<Vector2, float> rimBand = p => Mathf.Abs(Sdf.Ellipse(p, rimCenter, rimRadii)) - 0.022f;

            c.Fill(p => Sdf.Ellipse(p, V(0.5f, 0.17f), V(0.36f, 0.06f)), Palette.Shadow, 3f);
            c.Fill(p => Sdf.Ellipse(p, rimCenter, rimRadii), Palette.BowlRedDark);
            c.Fill(p => Sdf.Intersect(rimBand(p), rimCenter.y - p.y), Palette.BowlRim);

            var kibble = new[] { V(0.36f, 0.56f), V(0.5f, 0.6f), V(0.64f, 0.56f), V(0.43f, 0.53f), V(0.57f, 0.53f) };
            foreach (var k in kibble)
            {
                var kk = k;
                Outlined(c, p => Sdf.Circle(p, kk, 0.07f), 0.014f, Palette.Darker(Palette.Kibble, 0.35f), Palette.Kibble);
            }

            c.Fill(body, p => Color.Lerp(Palette.BowlRedDark, Palette.BowlRed, Mathf.Clamp01((p.y - 0.18f) / 0.3f)));
            c.Fill(p => Sdf.Intersect(rimBand(p), p.y - rimCenter.y), Palette.BowlRim);
            c.Fill(p => Sdf.Segment(p, V(0.2f, 0.42f), V(0.27f, 0.3f), 0.02f), Grey(1f, 0.45f));

            Func<Vector2, float> emblem = p => BoneShape(p, V(0.5f, 0.32f), 0.16f, 0f);
            c.Fill(emblem, Palette.Bone);
            return c;
        }

        // Kemik şekli: merkez, uzunluk, açı.
        static float BoneShape(Vector2 p, Vector2 center, float length, float degrees)
        {
            var q = Sdf.Rotate(p, center, degrees);
            float half = length * 0.5f;
            float r = length * 0.16f;
            float knob = length * 0.19f;
            float d = Sdf.Segment(q, center + V(-half, 0f), center + V(half, 0f), r);
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sy in new[] { -1f, 1f })
                    d = Sdf.Union(d, Sdf.Circle(q, center + V(sx * half, sy * knob * 0.8f), knob));
            return d;
        }

        public static PixelCanvas Bone(int size)
        {
            var c = new PixelCanvas(size);
            Func<Vector2, float> bone = p => BoneShape(p, V(0.5f, 0.5f), 0.5f, 30f);
            c.Fill(p => bone(p) - 0.03f, Palette.Darker(Palette.BoneShade, 0.3f));
            c.Fill(bone, p => Color.Lerp(Palette.BoneShade, Palette.Bone, Mathf.Clamp01((p.y - 0.3f) / 0.35f)));
            return c;
        }

        public static PixelCanvas Heart(int size, bool full)
        {
            var c = new PixelCanvas(size);
            var fill = full ? Palette.Heart : Palette.HeartEmpty;
            c.Fill(p => Sdf.Heart(p, V(0.5f, 0.5f), 0.76f) - 0.035f, Palette.Darker(fill, 0.3f));
            c.Fill(p => Sdf.Heart(p, V(0.5f, 0.5f), 0.76f), fill);
            if (full)
                c.Fill(p => Sdf.Ellipse(Sdf.Rotate(p, V(0.33f, 0.66f), -40f), V(0.33f, 0.66f), V(0.07f, 0.04f)), Grey(1f, 0.6f), 2f);
            return c;
        }

        public static PixelCanvas Star(int size)
        {
            var c = new PixelCanvas(size);
            Func<Vector2, float> star = p => Sdf.Star(p, V(0.5f, 0.47f), 0.38f, 0.5f) - 0.03f;
            c.Fill(p => star(p) - 0.03f, Palette.StarShade);
            c.Fill(star, p => Color.Lerp(Palette.StarShade, Palette.Star, Mathf.Clamp01((p.y - 0.2f) / 0.45f)));
            c.Fill(p => Sdf.Circle(p, V(0.42f, 0.6f), 0.04f), Grey(1f, 0.7f), 2f);
            return c;
        }

        public static PixelCanvas Pearl(int size)
        {
            var c = new PixelCanvas(size);
            var center = V(0.5f, 0.5f);
            c.Fill(p => Sdf.Circle(p, center, 0.38f), Palette.PearlShade);
            c.Fill(p => Sdf.Circle(p, center, 0.355f), p =>
                Color.Lerp(Palette.Pearl, Palette.PearlShade, Mathf.Clamp01((p - V(0.4f, 0.6f)).magnitude / 0.55f)));
            c.Fill(p => Sdf.Circle(p, V(0.38f, 0.63f), 0.07f), Grey(1f, 0.9f), 5f);
            return c;
        }

        // Altın tasma: altın bir halka, mavi taşlar ve asılı bir künye.
        public static PixelCanvas Collar(int size)
        {
            var c = new PixelCanvas(size);
            var center = V(0.5f, 0.56f);
            var radii = V(0.35f, 0.25f);
            Func<Vector2, float> band = p => Mathf.Abs(Sdf.Ellipse(p, center, radii)) - 0.055f;
            c.Fill(p => band(p) - 0.02f, Palette.GoldDark);
            c.Fill(band, p => Color.Lerp(Palette.GoldDark, Palette.Gold, Mathf.Clamp01((p.y - 0.3f) / 0.4f)));
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f + Mathf.PI / 8f;
                var stud = center + new Vector2(Mathf.Cos(a) * radii.x, Mathf.Sin(a) * radii.y);
                c.Fill(p => Sdf.Circle(p, stud, 0.026f), Palette.CollarBlue);
            }
            c.Fill(p => Sdf.Ring(p, V(0.5f, 0.3f), 0.035f, 0.016f), Palette.GoldDark);
            Outlined(c, p => Sdf.Circle(p, V(0.5f, 0.2f), 0.09f), 0.02f, Palette.GoldDark, Palette.Gold);
            c.Fill(p => Sdf.Circle(p, V(0.47f, 0.23f), 0.025f), Grey(1f, 0.6f), 2f);
            return c;
        }

        // Pati izi (beyaz; arayüzde istenen renge boyanır).
        public static PixelCanvas Paw(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p =>
            {
                float d = Sdf.Ellipse(p, V(0.5f, 0.34f), V(0.2f, 0.16f));
                d = Sdf.Union(d, Sdf.Ellipse(Sdf.Rotate(p, V(0.26f, 0.58f), 25f), V(0.26f, 0.58f), V(0.075f, 0.1f)));
                d = Sdf.Union(d, Sdf.Ellipse(p, V(0.41f, 0.72f), V(0.075f, 0.1f)));
                d = Sdf.Union(d, Sdf.Ellipse(p, V(0.59f, 0.72f), V(0.075f, 0.1f)));
                d = Sdf.Union(d, Sdf.Ellipse(Sdf.Rotate(p, V(0.74f, 0.58f), -25f), V(0.74f, 0.58f), V(0.075f, 0.1f)));
                return d;
            }, Color.white);
            return c;
        }

        // ---------- Arayüz ikonları (beyaz; arayüzde boyanır) ----------

        public static PixelCanvas RoundedRect(int size, float radius)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Box(p, V(0.5f, 0.5f), V(0.495f, 0.495f), radius), Color.white);
            return c;
        }

        public static PixelCanvas Circle(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Circle(p, V(0.5f, 0.5f), 0.49f), Color.white);
            return c;
        }

        public static PixelCanvas Gear(int size)
        {
            var c = new PixelCanvas(size);
            var center = V(0.5f, 0.5f);
            c.Fill(p =>
            {
                float d = Sdf.Circle(p, center, 0.27f);
                for (int i = 0; i < 8; i++)
                {
                    var q = Sdf.Rotate(p, center, i * 45f);
                    d = Sdf.Union(d, Sdf.Box(q, center + V(0f, 0.31f), V(0.065f, 0.09f), 0.025f));
                }
                return Sdf.Subtract(d, Sdf.Circle(p, center, 0.11f));
            }, Color.white);
            return c;
        }

        public static PixelCanvas Pause(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Union(
                Sdf.Box(p, V(0.37f, 0.5f), V(0.075f, 0.24f), 0.045f),
                Sdf.Box(p, V(0.63f, 0.5f), V(0.075f, 0.24f), 0.045f)), Color.white);
            return c;
        }

        public static PixelCanvas Back(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Union(
                Sdf.Segment(p, V(0.6f, 0.76f), V(0.36f, 0.5f), 0.065f),
                Sdf.Segment(p, V(0.36f, 0.5f), V(0.6f, 0.24f), 0.065f)), Color.white);
            return c;
        }

        public static PixelCanvas Lock(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p =>
            {
                float shackle = Sdf.Union(
                    Sdf.UpperArc(p, V(0.5f, 0.56f), 0.16f, 0.075f),
                    Sdf.Union(
                        Sdf.Segment(p, V(0.34f, 0.56f), V(0.34f, 0.45f), 0.0375f),
                        Sdf.Segment(p, V(0.66f, 0.56f), V(0.66f, 0.45f), 0.0375f)));
                float body = Sdf.Box(p, V(0.5f, 0.35f), V(0.25f, 0.19f), 0.06f);
                float keyhole = Sdf.Union(
                    Sdf.Circle(p, V(0.5f, 0.38f), 0.045f),
                    Sdf.Segment(p, V(0.5f, 0.38f), V(0.5f, 0.28f), 0.02f));
                return Sdf.Union(shackle, Sdf.Subtract(body, keyhole));
            }, Color.white);
            return c;
        }

        public static PixelCanvas Check(int size)
        {
            var c = new PixelCanvas(size);
            c.Fill(p => Sdf.Union(
                Sdf.Segment(p, V(0.25f, 0.52f), V(0.43f, 0.33f), 0.075f),
                Sdf.Segment(p, V(0.43f, 0.33f), V(0.77f, 0.7f), 0.075f)), Color.white);
            return c;
        }

        // Yanlış mama kabı işareti: beyaz konturlu kırmızı çarpı.
        public static PixelCanvas Cross(int size)
        {
            var c = new PixelCanvas(size);
            Func<Vector2, float> x = p => Sdf.Union(
                Sdf.Segment(p, V(0.28f, 0.28f), V(0.72f, 0.72f), 0.06f),
                Sdf.Segment(p, V(0.28f, 0.72f), V(0.72f, 0.28f), 0.06f));
            Outlined(c, x, 0.04f, Color.white, Palette.Red);
            return c;
        }

        // ---------- Arka planlar ----------

        // Açılış ekranı: gökyüzü, güneş, bulutlar, tepeler, beyaz çit ve çiçekler.
        public static PixelCanvas MenuBackground(int width, int height)
        {
            var c = new PixelCanvas(width, height);
            float top = c.Top;
            c.VerticalGradient(Palette.SkyBottom, Palette.SkyTop);

            var sun = V(0.8f, top - 0.2f);
            c.Fill(p => Sdf.Circle(p, sun, 0.16f), Palette.WithAlpha(Palette.Sun, 0.35f), 40f);
            c.Fill(p => Sdf.Circle(p, sun, 0.085f), Palette.Sun);

            Cloud(c, V(0.2f, top - 0.33f), 1f);
            Cloud(c, V(0.88f, top - 0.47f), 0.75f);

            c.Fill(p => Sdf.Circle(p, V(0.12f, -0.62f), 1.25f), Palette.HillBack);
            c.Fill(p => Sdf.Circle(p, V(1.05f, -0.95f), 1.42f), Palette.Darker(Palette.HillBack, 0.06f));

            // Çit
            float fenceY = 0.5f;
            var rail = new Rect(0f, fenceY - 0.12f, 1f, 0.24f);
            for (float x = 0.03f; x < 1.05f; x += 0.085f)
            {
                float px = x;
                var area = new Rect(px - 0.05f, fenceY - 0.12f, 0.1f, 0.24f);
                c.Fill(p => Sdf.Box(p, V(px, fenceY), V(0.028f, 0.1f), 0.022f),
                    p => Color.Lerp(Palette.FenceShade, Palette.Fence, Mathf.Clamp01((p.y - fenceY + 0.1f) / 0.2f)), area);
            }
            c.Fill(p => Sdf.Box(p, V(0.5f, fenceY + 0.045f), V(0.6f, 0.012f), 0.01f), Palette.Fence, rail);
            c.Fill(p => Sdf.Box(p, V(0.5f, fenceY - 0.035f), V(0.6f, 0.012f), 0.01f), Palette.FenceShade, rail);

            c.Fill(p => Sdf.Ellipse(p, V(0.5f, -0.08f), V(1.0f, 0.5f)), Palette.HillFront);

            var random = new System.Random(7);
            Color[] petals = { Color.white, Palette.Tongue, Palette.Sun };
            for (int i = 0; i < 16; i++)
            {
                var at = V((float)random.NextDouble(), 0.05f + (float)random.NextDouble() * 0.28f);
                Flower(c, at, 0.012f + (float)random.NextDouble() * 0.006f, petals[i % petals.Length]);
            }
            for (int i = 0; i < 22; i++)
            {
                var at = V((float)random.NextDouble(), 0.03f + (float)random.NextDouble() * 0.33f);
                Tuft(c, at, 0.03f, Palette.Darker(Palette.HillFront, 0.18f));
            }
            return c;
        }

        // Macera haritası: çayır, çimen tutamları ve çiçekler.
        public static PixelCanvas MapBackground(int width, int height)
        {
            var c = new PixelCanvas(width, height);
            float top = c.Top;
            c.VerticalGradient(Palette.MeadowBottom, Palette.MeadowTop);

            var random = new System.Random(11);
            for (int i = 0; i < 40; i++)
            {
                var at = V((float)random.NextDouble(), (float)random.NextDouble() * top);
                Tuft(c, at, 0.028f, Palette.Darker(Palette.MeadowBottom, 0.15f));
            }
            Color[] petals = { Color.white, Palette.Sun, Palette.Tongue };
            for (int i = 0; i < 26; i++)
            {
                var at = V((float)random.NextDouble(), (float)random.NextDouble() * top);
                Flower(c, at, 0.011f, petals[i % petals.Length]);
            }
            return c;
        }

        static void Cloud(PixelCanvas c, Vector2 at, float scale)
        {
            var area = new Rect(at.x - 0.2f * scale, at.y - 0.1f * scale, 0.4f * scale, 0.22f * scale);
            c.Fill(p =>
            {
                float d = Sdf.Circle(p, at, 0.07f * scale);
                d = Sdf.Union(d, Sdf.Circle(p, at + V(-0.08f, -0.02f) * scale, 0.055f * scale));
                d = Sdf.Union(d, Sdf.Circle(p, at + V(0.085f, -0.025f) * scale, 0.05f * scale));
                d = Sdf.Union(d, Sdf.Box(p, at + V(0f, -0.045f) * scale, V(0.13f, 0.03f) * scale, 0.03f * scale));
                return d;
            }, Grey(1f, 0.92f), area);
        }

        static void Flower(PixelCanvas c, Vector2 at, float r, Color petal)
        {
            var area = new Rect(at.x - r * 3f, at.y - r * 3f, r * 6f, r * 6f);
            c.Fill(p =>
            {
                float d = float.MaxValue;
                for (int i = 0; i < 5; i++)
                {
                    float a = i * Mathf.PI * 2f / 5f;
                    d = Sdf.Union(d, Sdf.Circle(p, at + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * 1.2f, r));
                }
                return d;
            }, petal, area);
            c.Fill(p => Sdf.Circle(p, at, r * 0.7f), Palette.Gold, area);
        }

        static void Tuft(PixelCanvas c, Vector2 at, float h, Color color)
        {
            var area = new Rect(at.x - h, at.y - h * 0.3f, h * 2f, h * 1.5f);
            c.Fill(p => Sdf.Union(
                Sdf.Segment(p, at, at + V(-0.4f, 1f) * h, h * 0.12f),
                Sdf.Union(
                    Sdf.Segment(p, at, at + V(0f, 1.15f) * h, h * 0.12f),
                    Sdf.Segment(p, at, at + V(0.4f, 1f) * h, h * 0.12f))), color, area);
        }
    }
}
