using System;

namespace UyuyanKopekler.Core
{
    // Oyuncunun ilerlemesi. Alanlar public çünkü JSON olarak kaydediliyor.
    [Serializable]
    public sealed class PlayerProgress
    {
        public int ClassicBestScore;
        public int ClassicBestGarden;
        public int AdventureCompletedLevel;

        // Aynı nesneyi kullanan modlar etkilensin diye yerinde sıfırlanır.
        public void Clear()
        {
            ClassicBestScore = 0;
            ClassicBestGarden = 0;
            AdventureCompletedLevel = 0;
        }
    }
}
