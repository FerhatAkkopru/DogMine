using System;
using UnityEngine;

namespace UyuyanKopekler.Game
{
    // Kamerayı bahçe, üst bar ve alt ipucu alanı arasına tam sığacak şekilde ayarlar.
    // Ekran boyutu, güvenli alan ya da bahçe değişince kendiliğinden yeniden sığdırır.
    [RequireComponent(typeof(Camera))]
    public sealed class BoardCamera : MonoBehaviour
    {
        Camera viewCamera;
        Func<Vector2> insetsPixels; // (üst, alt) boşluk, piksel
        SpriteRenderer backdrop;
        Vector2 content = new Vector2(10f, 12f);

        bool dirty = true;
        Vector2Int lastScreen;
        Vector2 lastInsets;

        public void Init(Func<Vector2> insets, SpriteRenderer background)
        {
            viewCamera = GetComponent<Camera>();
            viewCamera.orthographic = true;
            insetsPixels = insets;
            backdrop = background;
        }

        public void SetContent(Vector2 worldSize)
        {
            content = worldSize;
            dirty = true;
            Fit();
        }

        void LateUpdate()
        {
            if (insetsPixels == null) return;
            var insets = insetsPixels();
            if (dirty || insets != lastInsets || Screen.width != lastScreen.x || Screen.height != lastScreen.y)
                Fit();
        }

        void Fit()
        {
            if (viewCamera == null || insetsPixels == null || Screen.width <= 0 || Screen.height <= 0) return;

            var insets = insetsPixels();
            lastInsets = insets;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            dirty = false;

            float width = Screen.width;
            float height = Screen.height;
            float available = Mathf.Max(1f, height - insets.x - insets.y);

            // Bir dünya biriminin kaç piksel olacağı: bahçe hem enine hem boyuna sığmalı.
            float pixelsPerUnit = Mathf.Min(width / content.x, available / content.y);
            viewCamera.orthographicSize = height / (2f * pixelsPerUnit);

            // Bahçenin merkezi, üst ve alt boşlukların arasındaki alanın ortasına gelsin.
            float offsetPixels = (insets.y - insets.x) * 0.5f;
            transform.position = new Vector3(0f, -offsetPixels / pixelsPerUnit, -10f);

            FitBackdrop();
        }

        // Kâğıt zemin kameranın gördüğü alanı tamamen kaplasın.
        void FitBackdrop()
        {
            if (backdrop == null || backdrop.sprite == null) return;
            float viewHeight = viewCamera.orthographicSize * 2f * 1.02f;
            float viewWidth = viewHeight * viewCamera.aspect;
            backdrop.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            if (backdrop.drawMode == SpriteDrawMode.Simple)
            {
                var spriteSize = backdrop.sprite.bounds.size;
                backdrop.transform.localScale = new Vector3(viewWidth / spriteSize.x, viewHeight / spriteSize.y, 1f);
            }
            else
            {
                backdrop.transform.localScale = Vector3.one;
                backdrop.size = new Vector2(viewWidth, viewHeight); // döşenir, gerilmez
            }
        }
    }
}
