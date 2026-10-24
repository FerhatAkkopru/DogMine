using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Game
{
    // Dokunmaları bahçe hareketlerine çevirir:
    // kısa dokunuş = kaz, basılı tutma (ya da sağ tık) = mama kabı, parmak kayarsa iptal.
    public sealed class BoardInput : MonoBehaviour
    {
        const float DragCancelInches = 0.12f;
        const float MinDragPixels = 24f;

        BoardView view;
        Camera viewCamera;
        float longPressSeconds = 0.35f;

        bool pressing;
        bool longPressDone;
        float pressStartTime;
        Vector2 pressPosition;

        public event Action<Cell> Tapped;
        public event Action<Cell> LongPressed;
        public event Action<Cell?> PressChanged; // null = basılı kare yok

        public void Init(BoardView boardView, Camera camera, float longPress)
        {
            view = boardView;
            viewCamera = camera;
            longPressSeconds = longPress;
        }

        float DragThreshold => Mathf.Max(MinDragPixels, Screen.dpi * DragCancelInches);

        void OnDisable() => CancelPress();

        void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || view == null) return;

            // Bilgisayarda sağ tık = mama kabı
            var mouse = Mouse.current;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            {
                if (view.TryGetCell(mouse.position.ReadValue(), viewCamera, out var cell)) LongPressed?.Invoke(cell);
                return;
            }

            if (pointer.press.wasPressedThisFrame)
            {
                pressing = true;
                longPressDone = false;
                pressStartTime = Time.unscaledTime;
                pressPosition = pointer.position.ReadValue();
                PressChanged?.Invoke(view.TryGetCell(pressPosition, viewCamera, out var pressedCell) ? pressedCell : (Cell?)null);
            }
            if (!pressing) return;

            if (pointer.press.isPressed && !longPressDone)
            {
                if ((pointer.position.ReadValue() - pressPosition).magnitude > DragThreshold)
                {
                    CancelPress(); // parmak kaydı: kazma
                    return;
                }
                if (Time.unscaledTime - pressStartTime >= longPressSeconds)
                {
                    longPressDone = true;
                    PressChanged?.Invoke(null);
                    if (view.TryGetCell(pressPosition, viewCamera, out var longCell)) LongPressed?.Invoke(longCell);
                }
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                bool wasLongPress = longPressDone;
                CancelPress();
                if (!wasLongPress && view.TryGetCell(pressPosition, viewCamera, out var tapCell)) Tapped?.Invoke(tapCell);
            }
        }

        void CancelPress()
        {
            if (pressing) PressChanged?.Invoke(null);
            pressing = false;
        }
    }
}
