using UnityEngine;

namespace UyuyanKopekler.Art
{
    // İşaretli uzaklık fonksiyonları (signed distance): şeklin içinde negatif, dışında pozitif.
    // PixelCanvas bunlarla kenarları yumuşak (anti-aliased) şekiller çizer.
    // Formüllerin çoğu Inigo Quilez'in 2D SDF derlemesinden uyarlanmıştır.
    public static class Sdf
    {
        public static float Circle(Vector2 p, Vector2 c, float r) => (p - c).magnitude - r;

        public static float Ellipse(Vector2 p, Vector2 c, Vector2 r)
        {
            var q = p - c;
            float k0 = new Vector2(q.x / r.x, q.y / r.y).magnitude;
            float k1 = new Vector2(q.x / (r.x * r.x), q.y / (r.y * r.y)).magnitude;
            if (k1 < 1e-6f) return -Mathf.Min(r.x, r.y);
            return k0 * (k0 - 1f) / k1;
        }

        public static float Box(Vector2 p, Vector2 c, Vector2 half, float radius)
        {
            var q = new Vector2(Mathf.Abs(p.x - c.x), Mathf.Abs(p.y - c.y)) - half + new Vector2(radius, radius);
            var outside = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f)).magnitude;
            return outside + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
        }

        // Yuvarlak uçlu çizgi (kapsül).
        public static float Segment(Vector2 p, Vector2 a, Vector2 b, float r)
        {
            var pa = p - a;
            var ba = b - a;
            float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba));
            return (pa - ba * h).magnitude - r;
        }

        public static float Ring(Vector2 p, Vector2 c, float r, float thickness) =>
            Mathf.Abs((p - c).magnitude - r) - thickness * 0.5f;

        // Alt yarım halka: kapalı göz gibi "u" şekli.
        public static float LowerArc(Vector2 p, Vector2 c, float r, float thickness)
        {
            if (p.y <= c.y) return Ring(p, c, r, thickness);
            float left = (p - new Vector2(c.x - r, c.y)).magnitude;
            float right = (p - new Vector2(c.x + r, c.y)).magnitude;
            return Mathf.Min(left, right) - thickness * 0.5f;
        }

        // Üst yarım halka: kilit halkası gibi "∩" şekli.
        public static float UpperArc(Vector2 p, Vector2 c, float r, float thickness) =>
            LowerArc(new Vector2(p.x, 2f * c.y - p.y), c, r, thickness);

        // Kalp; c merkez, size toplam yükseklik.
        public static float Heart(Vector2 p, Vector2 c, float size)
        {
            const float naturalHeight = 1.104f;
            float s = size / naturalHeight;
            var q = (p - c) / s + new Vector2(0f, naturalHeight * 0.5f);
            q.x = Mathf.Abs(q.x);

            float d;
            if (q.y + q.x > 1f)
            {
                d = (q - new Vector2(0.25f, 0.75f)).magnitude - 0.35355339f;
            }
            else
            {
                float a = (q - new Vector2(0f, 1f)).sqrMagnitude;
                float m = 0.5f * Mathf.Max(q.x + q.y, 0f);
                float b = (q - new Vector2(m, m)).sqrMagnitude;
                d = Mathf.Sqrt(Mathf.Min(a, b)) * Mathf.Sign(q.x - q.y);
            }
            return d * s;
        }

        // Beş köşeli yıldız; r dış yarıçap, inner 0..1 iç girinti oranı.
        public static float Star(Vector2 p, Vector2 c, float r, float inner)
        {
            var k1 = new Vector2(0.809016994f, -0.587785252f);
            var k2 = new Vector2(-k1.x, k1.y);
            var q = p - c;
            q.x = Mathf.Abs(q.x);
            q -= 2f * Mathf.Max(Vector2.Dot(k1, q), 0f) * k1;
            q -= 2f * Mathf.Max(Vector2.Dot(k2, q), 0f) * k2;
            q.x = Mathf.Abs(q.x);
            q.y -= r;
            var ba = inner * new Vector2(-k1.y, k1.x) - new Vector2(0f, 1f);
            float h = Mathf.Clamp(Vector2.Dot(q, ba) / Vector2.Dot(ba, ba), 0f, r);
            return (q - ba * h).magnitude * Mathf.Sign(q.y * ba.x - q.x * ba.y);
        }

        // Noktayı c etrafında -degrees döndürür; döndürülmüş şekil çizmek için kullanılır.
        public static Vector2 Rotate(Vector2 p, Vector2 c, float degrees)
        {
            float rad = -degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
            var q = p - c;
            return c + new Vector2(q.x * cos - q.y * sin, q.x * sin + q.y * cos);
        }

        public static float Union(float a, float b) => Mathf.Min(a, b);
        public static float Subtract(float a, float b) => Mathf.Max(a, -b);
        public static float Intersect(float a, float b) => Mathf.Max(a, b);
    }
}
