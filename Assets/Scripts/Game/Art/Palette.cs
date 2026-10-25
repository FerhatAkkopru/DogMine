using UnityEngine;

namespace UyuyanKopekler.Art
{
    // Oyunun bütün renkleri. tools/art/style.py ile aynı tutulmalı (çizimler oradan üretiliyor).
    // Sınırlı palet: kâğıt, mürekkep ve altı ana renk (hardal, mercan, lacivert, pembe, nane, krem).
    public static class Palette
    {
        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        // Temel
        public static readonly Color Paper = Hex("#F8F1E4");
        public static readonly Color PaperDark = Hex("#EDE2CC");
        public static readonly Color Ink = Hex("#211D1A");
        public static readonly Color InkSoft = Hex("#6B5F55");
        public static readonly Color White = Hex("#FFFCF5");

        // Ana renkler
        public static readonly Color Mustard = Hex("#DDB65A");
        public static readonly Color MustardLight = Hex("#F2DC9C");
        public static readonly Color Coral = Hex("#EE6A4C");
        public static readonly Color CoralDark = Hex("#C94B31");
        public static readonly Color CoralLight = Hex("#F8A891");
        public static readonly Color Navy = Hex("#2E3C5A");
        public static readonly Color Pink = Hex("#F3C3CD");
        public static readonly Color Mint = Hex("#A7D8CE");
        public static readonly Color MintDark = Hex("#7DBDB0");
        public static readonly Color Cream = Hex("#F7E3C3");

        // Gölge, karartma
        public static readonly Color Shadow = Hex("#211D1A");
        public static readonly Color Backdrop = new Color(0.13f, 0.11f, 0.10f, 0.55f);

        // Bahçe
        public static readonly Color WokenSoil = Hex("#F8C6B8");   // toprak sprite'ını bununla boyayınca pembeleşir
        public static readonly Color WrongBowl = Hex("#B9B2A6");
        public static readonly Color Pressed = new Color(0.9f, 0.9f, 0.88f, 1f);

        // Rakamlar (komşu köpek sayısı)
        public static readonly Color[] Numbers =
        {
            Color.clear,
            Hex("#3558A8"), // 1 mavi
            Hex("#3E8A55"), // 2 yeşil
            Hex("#E0553A"), // 3 mercan
            Hex("#7A4A9A"), // 4 mor
            Hex("#A8741C"), // 5 hardal koyu
            Hex("#22867E"), // 6 deniz yeşili
            Hex("#211D1A"), // 7 mürekkep
            Hex("#6B5F55")  // 8 gri
        };

        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
