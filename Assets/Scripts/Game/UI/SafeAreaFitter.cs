using UnityEngine;

namespace UyuyanKopekler.UI
{
    // RectTransform'u telefonun güvenli alanına (çentik ve alt çubuk dışı) sığdırır.
    // Ebeveyni tam ekran olmalı.
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        Rect lastSafeArea;
        Vector2Int lastScreen;

        void OnEnable() => Apply();

        void Update()
        {
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreen.x || Screen.height != lastScreen.y)
                Apply();
        }

        void Apply()
        {
            lastSafeArea = Screen.safeArea;
            lastScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;

            var size = new Vector2(Screen.width, Screen.height);
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(lastSafeArea.xMin / size.x, lastSafeArea.yMin / size.y);
            rt.anchorMax = new Vector2(lastSafeArea.xMax / size.x, lastSafeArea.yMax / size.y);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
