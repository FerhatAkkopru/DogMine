using UnityEngine;

namespace UyuyanKopekler.Art
{
    // Oyunun kullandığı bütün görseller. Görünümler yalnızca bu arayüzü tanır;
    // gerçek çizimler geldiğinde ProceduralArt yerine başka bir uygulama takılır.
    public interface IGameArt
    {
        Font Font { get; }

        // Bahçe (dünya içi, 1 kare = 1 birim)
        Sprite TileGrass { get; }
        Sprite TileSoil { get; }
        Sprite BoardFrame { get; }
        Sprite GardenBackground { get; }

        // Karakterler ve eşyalar
        Sprite DogSleeping { get; }
        Sprite DogAwake { get; }
        Sprite Bowl { get; }
        Sprite Heart { get; }
        Sprite HeartEmpty { get; }
        Sprite Bone { get; }
        Sprite Star { get; }
        Sprite Pearl { get; }
        Sprite Collar { get; }

        // Arayüz (beyaz çizilir, Image.color ile boyanır)
        Sprite RoundedRect { get; }
        Sprite Circle { get; }
        Sprite Paw { get; }
        Sprite IconPause { get; }
        Sprite IconBack { get; }
        Sprite IconGear { get; }
        Sprite IconLock { get; }
        Sprite IconCheck { get; }
        Sprite IconCross { get; }

        // Ekran arka planları
        Sprite MenuBackground { get; }
        Sprite MapBackground { get; }
    }
}
