using System;
using System.Collections.Generic;
using UnityEngine;

namespace UyuyanKopekler.Art
{
    // IGameArt'ın kodla çizilen uygulaması. Her sprite ilk istendiğinde çizilir ve saklanır.
    public sealed class ProceduralArt : IGameArt
    {
        const int IconSize = 128;
        const int FaceSize = 256;
        const float UiPixelsPerUnit = 100f;

        readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public Font Font { get; } = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public Sprite TileGrass => Icon(nameof(TileGrass), () => ArtPainter.TileGrass(IconSize));
        public Sprite TileSoil => Icon(nameof(TileSoil), () => ArtPainter.TileSoil(IconSize));
        public Sprite BoardFrame => Get(nameof(BoardFrame), () => Make(ArtPainter.Frame(96), nameof(BoardFrame), 96f, 36f));
        public Sprite GardenBackground => Get(nameof(GardenBackground), () => Make(ArtPainter.Garden(64, 256), nameof(GardenBackground), 64f, 0f));

        public Sprite DogSleeping => Icon(nameof(DogSleeping), () => ArtPainter.DogSleeping(FaceSize));
        public Sprite DogAwake => Icon(nameof(DogAwake), () => ArtPainter.DogAwake(FaceSize));
        public Sprite Bowl => Icon(nameof(Bowl), () => ArtPainter.Bowl(IconSize));
        public Sprite Heart => Icon(nameof(Heart), () => ArtPainter.Heart(IconSize, true));
        public Sprite HeartEmpty => Icon(nameof(HeartEmpty), () => ArtPainter.Heart(IconSize, false));
        public Sprite Bone => Icon(nameof(Bone), () => ArtPainter.Bone(IconSize));
        public Sprite Star => Icon(nameof(Star), () => ArtPainter.Star(IconSize));
        public Sprite Pearl => Icon(nameof(Pearl), () => ArtPainter.Pearl(IconSize));
        public Sprite Collar => Icon(nameof(Collar), () => ArtPainter.Collar(IconSize));

        public Sprite RoundedRect => Get(nameof(RoundedRect), () => Make(ArtPainter.RoundedRect(128, 0.375f), nameof(RoundedRect), UiPixelsPerUnit, 50f));
        public Sprite Circle => Icon(nameof(Circle), () => ArtPainter.Circle(IconSize));
        public Sprite Paw => Icon(nameof(Paw), () => ArtPainter.Paw(IconSize));
        public Sprite IconPause => Icon(nameof(IconPause), () => ArtPainter.Pause(IconSize));
        public Sprite IconBack => Icon(nameof(IconBack), () => ArtPainter.Back(IconSize));
        public Sprite IconGear => Icon(nameof(IconGear), () => ArtPainter.Gear(IconSize));
        public Sprite IconLock => Icon(nameof(IconLock), () => ArtPainter.Lock(IconSize));
        public Sprite IconCheck => Icon(nameof(IconCheck), () => ArtPainter.Check(IconSize));
        public Sprite IconCross => Icon(nameof(IconCross), () => ArtPainter.Cross(IconSize));

        public Sprite MenuBackground => Get(nameof(MenuBackground), () => Make(ArtPainter.MenuBackground(360, 640), nameof(MenuBackground), UiPixelsPerUnit, 0f));
        public Sprite MapBackground => Get(nameof(MapBackground), () => Make(ArtPainter.MapBackground(360, 640), nameof(MapBackground), UiPixelsPerUnit, 0f));

        // Kare ikon: dünyada 1 birim boyunda olsun diye piksel/birim = genişlik.
        Sprite Icon(string key, Func<PixelCanvas> paint) =>
            Get(key, () =>
            {
                var canvas = paint();
                return Make(canvas, key, canvas.Width, 0f);
            });

        Sprite Get(string key, Func<Sprite> create)
        {
            if (!cache.TryGetValue(key, out var sprite))
            {
                sprite = create();
                cache[key] = sprite;
            }
            return sprite;
        }

        // border > 0 ise 9 parçalı (sliced) kullanılabilir sprite üretir.
        static Sprite Make(PixelCanvas canvas, string name, float pixelsPerUnit, float border)
        {
            var texture = canvas.ToTexture(name);
            var sprite = Sprite.Create(texture, new Rect(0, 0, canvas.Width, canvas.Height), new Vector2(0.5f, 0.5f),
                pixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
            sprite.name = name;
            return sprite;
        }
    }
}
