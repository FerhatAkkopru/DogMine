using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Alt kısımda beliren koyu ipucu balonu. Süre verilirse kendiliğinden kaybolur.
    public sealed class HintToast : MonoBehaviour
    {
        const float FadeSeconds = 0.2f;

        CanvasGroup group;
        Text label;
        Coroutine routine;

        public static HintToast Create(UiFactory ui, Transform parent, float width)
        {
            var pill = ui.RoundedBox(parent, "İpucu", Palette.HintBack, 1.3f);
            UiFactory.Place(pill.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(width, 0f));

            var layout = pill.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(44, 44, 26, 26);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            pill.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var toast = pill.gameObject.AddComponent<HintToast>();
            toast.group = pill.gameObject.AddComponent<CanvasGroup>();
            toast.group.blocksRaycasts = false;
            toast.group.alpha = 0f;
            toast.label = ui.Label(pill.transform, "Yazı", "", 40, Palette.Cream);
            pill.gameObject.SetActive(false);
            return toast;
        }

        // seconds <= 0 ise kapatılana kadar kalır.
        public void Show(string message, float seconds)
        {
            if (string.IsNullOrEmpty(message))
            {
                Hide();
                return;
            }
            label.text = message;
            gameObject.SetActive(true);
            Restart(ShowRoutine(seconds));
        }

        public void Hide()
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return;
            }
            Restart(Fade(0f, deactivate: true));
        }

        IEnumerator ShowRoutine(float seconds)
        {
            yield return Fade(1f, deactivate: false);
            if (seconds <= 0f) yield break;
            float start = Time.unscaledTime;
            while (Time.unscaledTime - start < seconds) yield return null;
            yield return Fade(0f, deactivate: true);
        }

        IEnumerator Fade(float target, bool deactivate)
        {
            float from = group.alpha;
            yield return Tween.Run(FadeSeconds, t => group.alpha = Mathf.Lerp(from, target, t));
            if (deactivate) gameObject.SetActive(false);
        }

        void Restart(IEnumerator next)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(next);
        }
    }
}
