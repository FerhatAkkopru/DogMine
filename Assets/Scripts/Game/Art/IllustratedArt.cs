using System.Collections.Generic;
using UnityEngine;

namespace UyuyanKopekler.Art
{
    // IGameArt'ın Resources/Art altındaki çizimlerden okunan uygulaması.
    // Sprite'lar kodda kurulur: böylece birim boyu (pixels per unit) ve 9 parçalı kenarlar tek yerde.
    public sealed class IllustratedArt : IGameArt
    {
        const string Root = "Art/";
        const float UiPixelsPerUnit = 100f;
        const int SheetCell = 320;      // kare sayfalarında bir karenin boyu (tools/art/build.py CELL)
        const int DogSheetColumns = 5;  // rig.DOG_SHEET_COLS
        const int CatSheetColumns = 4;  // rig.CAT_SHEET_COLS
        static readonly string[] Coats = { "mustard", "navy", "coral", "pink", "mint", "cream" };

        readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public IllustratedArt()
        {
            DisplayFont = LoadFont("Fonts/ShantellSans-ExtraBold");
            BodyFont = LoadFont("Fonts/ShantellSans-Bold");
        }

        public Font DisplayFont { get; }
        public Font BodyFont { get; }

        public Sprite GrassTile(int variant, bool light) =>
            World($"Board/grass_{Mathf.Abs(variant) % 3}_{(light ? "b" : "a")}");
        public Sprite SoilTile(int variant) => World($"Board/soil_{Mathf.Abs(variant) % 2}");
        public Sprite BoardBed => Sliced("Board/bed", 160f, 56f);
        public Sprite Bowl => World("Board/bowl");
        public Sprite Paper => Get("UI/paper", 100f, 0f);

        public int DogCoatCount => Coats.Length;
        public Sprite Dog(int coat, DogFrame frame) => Frame("Dogs/dog_" + Coat(coat), (int)frame, DogSheetColumns, SheetCell);
        public Sprite Cat(CatFrame frame) => Frame("Dogs/cat", (int)frame, CatSheetColumns, SheetCell);
        public Sprite CatIcon => World("Icons/cat");

        public Sprite PawHint(int count) => World($"Board/hint_{Mathf.Clamp(count, 1, 8)}");
        public Sprite Puff(int frame) => Frame("Board/puff", Mathf.Clamp(frame, 0, 2), 3, 128);

        public Sprite Heart => World("Items/heart");
        public Sprite HeartEmpty => World("Items/heart_empty");
        public Sprite Bone => World("Items/bone");
        public Sprite Star => World("Items/star");
        public Sprite Pearl => World("Items/pearl");
        public Sprite Collar => World("Items/collar");

        public Sprite Paw => World("Items/paw");
        public Sprite IconPause => World("Icons/pause");
        public Sprite IconBack => World("Icons/back");
        public Sprite IconCheck => World("Icons/check");
        public Sprite IconCross => World("Icons/cross");
        public Sprite IconLock => World("Icons/lock");
        public Sprite IconGear => World("Icons/gear");
        public Sprite BoxFill => Sliced("UI/box_fill", UiPixelsPerUnit, 52f);
        public Sprite BoxLine => Sliced("UI/box_line", UiPixelsPerUnit, 52f);
        public Sprite CircleFill => World("UI/circle_fill");
        public Sprite CircleLine => World("UI/circle_line");
        public Sprite Underline => Get("UI/underline", UiPixelsPerUnit, 0f);

        public Sprite BedBack => Get("Hero/bed_back", UiPixelsPerUnit, 0f);
        public Sprite BedFront => Get("Hero/bed_front", UiPixelsPerUnit, 0f);

        static string Coat(int coat) => Coats[((coat % Coats.Length) + Coats.Length) % Coats.Length];

        // Dünyada 1 birim genişliğinde olsun diye piksel/birim = doku genişliği.
        Sprite World(string path) => Get(path, -1f, 0f);

        Sprite Sliced(string path, float pixelsPerUnit, float border) => Get(path, pixelsPerUnit, border);

        // Kare sayfasından tek bir kare (soldan sağa, yukarıdan aşağı sıralı). Dünyada 1 birim.
        Sprite Frame(string path, int index, int columns, int cell)
        {
            string key = path + "#" + index;
            if (cache.TryGetValue(key, out var sprite)) return sprite;

            var texture = LoadTexture(path);
            int col = index % columns;
            int row = index / columns;
            var rect = new Rect(col * cell, texture.height - (row + 1) * cell, cell, cell);
            if (rect.yMin < 0 || rect.xMax > texture.width) rect = new Rect(0, 0, texture.width, texture.height);
            sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), cell, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            cache[key] = sprite;
            return sprite;
        }

        static Texture2D LoadTexture(string path)
        {
            var texture = Resources.Load<Texture2D>(Root + path);
            if (texture != null) return texture;
            Debug.LogError($"Görsel bulunamadı: Resources/{Root}{path}.png — tools/art/build.py çalıştırıldı mı?");
            return Texture2D.whiteTexture;
        }

        Sprite Get(string path, float pixelsPerUnit, float border)
        {
            if (cache.TryGetValue(path, out var sprite)) return sprite;

            var texture = LoadTexture(path);
            float ppu = pixelsPerUnit > 0f ? pixelsPerUnit : texture.width;
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                ppu, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            sprite.name = path;
            cache[path] = sprite;
            return sprite;
        }

        static Font LoadFont(string path)
        {
            var font = Resources.Load<Font>(path);
            if (font != null) return font;
            Debug.LogError($"Yazı tipi bulunamadı: Resources/{path}.ttf");
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
