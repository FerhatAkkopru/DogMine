using UnityEngine;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Game
{
    // Telefonu titretir (ayarlardan kapatılabilir). Bilgisayarda hiçbir şey yapmaz.
    public sealed class Haptics
    {
        readonly GameSettings settings;

        public Haptics(GameSettings settings)
        {
            this.settings = settings;
        }

        public void Buzz()
        {
            if (!settings.Vibration) return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
