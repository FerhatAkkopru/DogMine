using UnityEngine;

namespace UyuyanKopekler.Art
{
    // Oyunun kullandığı bütün görseller ve yazı tipleri. Görünümler yalnızca bu arayüzü tanır.
    // Çizimler tools/art/ altındaki betiklerle üretilip Assets/Resources/Art'a konuyor.
    public interface IGameArt
    {
        Font DisplayFont { get; } // başlıklar, rakamlar
        Font BodyFont { get; }    // düğmeler, açıklamalar

        // Bahçe (dünya içi, 1 kare = 1 birim)
        Sprite GrassTile(int variant, bool light);
        Sprite SoilTile(int variant);
        Sprite BoardBed { get; } // 9 parçalı
        Sprite Bowl { get; }
        Sprite Paper { get; }    // döşenebilir kâğıt dokusu

        // Köpekler: aynı kürk numarası her pozda aynı köpek
        int DogCoatCount { get; }
        Sprite SleepingDog(int coat); // başını patisine koymuş
        Sprite CurledDog(int coat);   // simit gibi kıvrılmış
        Sprite AwakeDog(int coat);    // uyanmış, havlıyor

        // Eşyalar (renkli)
        Sprite Heart { get; }
        Sprite HeartEmpty { get; }
        Sprite Bone { get; }
        Sprite Star { get; }
        Sprite Pearl { get; }
        Sprite Collar { get; }

        // Beyaz çizilenler: Image.color ile istenen renge boyanır
        Sprite Paw { get; }
        Sprite IconPause { get; }
        Sprite IconBack { get; }
        Sprite IconCheck { get; }
        Sprite IconCross { get; }
        Sprite IconLock { get; }
        Sprite IconGear { get; }
        Sprite BoxFill { get; }    // 9 parçalı, elle çizilmiş köşeli kutu
        Sprite BoxLine { get; }    // aynı kutunun mürekkep konturu
        Sprite CircleFill { get; }
        Sprite CircleLine { get; }
        Sprite Underline { get; }  // başlık altı fırça darbesi

        // Açılış sahnesi: köpek yatağının arkası ve önü (köpekler aralarına konur)
        Sprite BedBack { get; }
        Sprite BedFront { get; }
    }
}
