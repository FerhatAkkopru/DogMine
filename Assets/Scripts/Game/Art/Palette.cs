using UnityEngine;

namespace UyuyanKopekler.Art
{
    // Oyunun bütün renkleri tek yerde. Tema değişikliği buradan yapılır.
    public static class Palette
    {
        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        // Bahçe
        public static readonly Color GrassA = Hex("#8CCB5E");
        public static readonly Color GrassB = Hex("#7FBF52");
        public static readonly Color GrassPressed = Hex("#A5DB7C");
        public static readonly Color SoilA = Hex("#F2DDB0");
        public static readonly Color SoilB = Hex("#EAD19C");
        public static readonly Color WokenSoil = Hex("#F4B9A6");
        public static readonly Color WrongBowl = Hex("#B9B4AA");
        public static readonly Color BoardFrame = Hex("#6B4A2E");
        public static readonly Color GardenTop = Hex("#5E9E48");
        public static readonly Color GardenBottom = Hex("#3F7A34");

        public static readonly Color[] Numbers =
        {
            Color.clear,
            Hex("#2F6FDB"), // 1
            Hex("#2B8A3E"), // 2
            Hex("#D6402B"), // 3
            Hex("#6B40B5"), // 4
            Hex("#A12E2E"), // 5
            Hex("#0E8585"), // 6
            Hex("#2B2620"), // 7
            Hex("#6B6355")  // 8
        };

        // Köpek
        public static readonly Color DogFur = Hex("#C98A4B");
        public static readonly Color DogFurDark = Hex("#8E5A2E");
        public static readonly Color DogMuzzle = Hex("#F6E3C4");
        public static readonly Color DogNose = Hex("#3A2418");
        public static readonly Color DogOutline = Hex("#4A2E1A");
        public static readonly Color Cheek = new Color(1f, 0.55f, 0.55f, 0.55f);
        public static readonly Color Tongue = Hex("#F07A8C");
        public static readonly Color Mouth = Hex("#7A2E2E");

        // Eşyalar
        public static readonly Color BowlRed = Hex("#E2483D");
        public static readonly Color BowlRedDark = Hex("#A82E26");
        public static readonly Color BowlRim = Hex("#F07A6E");
        public static readonly Color Kibble = Hex("#9A5E2E");
        public static readonly Color Heart = Hex("#EF4B5C");
        public static readonly Color HeartEmpty = Hex("#D9CFC4");
        public static readonly Color Bone = Hex("#FFF4DE");
        public static readonly Color BoneShade = Hex("#E5D3B0");
        public static readonly Color Star = Hex("#FFC93C");
        public static readonly Color StarShade = Hex("#F29A1F");
        public static readonly Color Pearl = Hex("#F3EEFA");
        public static readonly Color PearlShade = Hex("#C9BDE0");
        public static readonly Color Gold = Hex("#F5B931");
        public static readonly Color GoldDark = Hex("#B9801A");
        public static readonly Color CollarBlue = Hex("#3C7DD9");

        // Arayüz
        public static readonly Color Cream = Hex("#FFF8EC");
        public static readonly Color CreamDark = Hex("#F1E3CB");
        public static readonly Color Ink = Hex("#4A3426");
        public static readonly Color InkSoft = Hex("#8A6F5A");
        public static readonly Color White = Color.white;
        public static readonly Color Shadow = new Color(0.16f, 0.09f, 0.04f, 0.28f);
        public static readonly Color Backdrop = new Color(0.08f, 0.06f, 0.04f, 0.6f);
        public static readonly Color HintBack = new Color(0.17f, 0.12f, 0.08f, 0.82f);

        public static readonly Color Orange = Hex("#F59A30");
        public static readonly Color Teal = Hex("#2FA6A0");
        public static readonly Color Green = Hex("#4CAF50");
        public static readonly Color Red = Hex("#E25545");
        public static readonly Color Grey = Hex("#A9A39B");
        public static readonly Color Lock = Hex("#7D756C");

        public static readonly Color SkyTop = Hex("#8FD3F4");
        public static readonly Color SkyBottom = Hex("#DDF3FB");
        public static readonly Color HillBack = Hex("#8CC860");
        public static readonly Color HillFront = Hex("#6DB24A");
        public static readonly Color Sun = Hex("#FFE27A");
        public static readonly Color Fence = Hex("#FFF4E0");
        public static readonly Color FenceShade = Hex("#E3CFAE");
        public static readonly Color MeadowTop = Hex("#A8DA7A");
        public static readonly Color MeadowBottom = Hex("#6FB24B");

        // Bir rengi koyulaştırır (düğme gölgesi için).
        public static Color Darker(Color c, float amount = 0.25f) =>
            new Color(c.r * (1f - amount), c.g * (1f - amount), c.b * (1f - amount), c.a);

        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
