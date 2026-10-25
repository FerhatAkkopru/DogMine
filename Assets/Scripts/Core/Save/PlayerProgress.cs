using System;

namespace UyuyanKopekler.Core
{
    // Oyuncunun ilerlemesi. Alanlar public çünkü JSON olarak kaydediliyor.
    [Serializable]
    public sealed class PlayerProgress
    {
        public const int StartingBones = 30; // yeni oyuncu 3 kedi alabilsin

        public int ClassicBestScore;
        public int ClassicBestGarden;
        public int AdventureCompletedLevel;

        // Çimin altından toplananlar (eski kayıtlarda bu alanlar yoksa başlangıç değerleri kalır).
        public int Bones = StartingBones;
        public int Stars;
        public int Pearls;
        public int GoldCollars;

        // Aynı nesneyi kullanan modlar etkilensin diye yerinde sıfırlanır.
        public void Clear()
        {
            ClassicBestScore = 0;
            ClassicBestGarden = 0;
            AdventureCompletedLevel = 0;
            Bones = StartingBones;
            Stars = 0;
            Pearls = 0;
            GoldCollars = 0;
        }
    }
}
