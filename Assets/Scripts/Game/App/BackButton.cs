using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UyuyanKopekler.Game
{
    // Android'in geri tuşu (bilgisayarda Esc) basılınca haber verir.
    public sealed class BackButton : MonoBehaviour
    {
        public event Action Pressed;

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) Pressed?.Invoke();
        }
    }
}
