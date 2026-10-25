using System;

namespace UyuyanKopekler.Core
{
    [Serializable]
    public sealed class GameSettings
    {
        public bool Vibration = true;
        public bool Sound = true;
        public bool PawHints = true; // açık rakam yerine pati izi göster
    }
}
