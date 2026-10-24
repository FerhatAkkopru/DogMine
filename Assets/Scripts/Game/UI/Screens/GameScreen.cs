using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Oyun sırasındaki arayüz: üst bar (mola, başlık, mod bilgisi, kalpler, kalan köpek)
    // ve altta ipucu balonu. Ortası boş kalır; bahçe dünyada çizilir.
    public sealed class GameScreen : UiScreen
    {
        const float BarHeight = 250f;
        const float BarTop = 16f;
        const float BoardGap = 28f;
        const float BottomReserve = 150f;
        const float HeartSize = 66f;

        Text title;
        Text status;
        Text dogsLeft;
        RectTransform heartsRow;
        readonly List<Image> hearts = new List<Image>();
        HintToast hint;
        Canvas rootCanvas;
        int shownHearts = -1;

        public event Action PausePressed;

        // Bahçenin arayüzün altında kalmaması için üstten ve alttan bırakılacak boşluk (piksel).
        public Vector2 BoardInsetsPixels
        {
            get
            {
                float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
                var safeArea = Screen.safeArea;
                float top = Screen.height - safeArea.yMax + (BarTop + BarHeight + BoardGap) * scale;
                float bottom = safeArea.yMin + BottomReserve * scale;
                return new Vector2(top, bottom);
            }
        }

        public void SetTitle(string text) => title.text = text;
        public void SetStatus(string text) => status.text = text;
        public void SetDogsLeft(int count) => dogsLeft.text = $"× {count}";
        public void ShowHint(string text, float seconds) => hint.Show(text, seconds);
        public void HideHint() => hint.Hide();

        public void SetHearts(int current, int max)
        {
            while (hearts.Count < max)
            {
                var heart = Ui.Picture(heartsRow, "Kalp", Ui.Art.Heart, Color.white);
                var element = heart.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = HeartSize;
                element.preferredHeight = HeartSize;
                hearts.Add(heart);
            }
            for (int i = 0; i < hearts.Count; i++)
            {
                hearts[i].gameObject.SetActive(i < max);
                hearts[i].sprite = i < current ? Ui.Art.Heart : Ui.Art.HeartEmpty;
            }

            // Yeni giden kalp küçük bir sarsıntıyla griye döner.
            if (shownHearts > current && isActiveAndEnabled)
                for (int i = current; i < Math.Min(shownHearts, hearts.Count); i++)
                    StartCoroutine(LoseHeart(hearts[i].rectTransform));
            shownHearts = current;
        }

        // Yeni tur başlarken kalp animasyonları oynamasın.
        public void ResetHearts() => shownHearts = -1;

        protected override void BuildContent(RectTransform safe)
        {
            rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

            var bar = Ui.Panel(safe, "Üst bar", Palette.Cream).rectTransform;
            UiFactory.TopStrip(bar, BarHeight, 24f, BarTop);

            var pause = Ui.IconButton(bar, "Mola", Ui.Art.IconPause, Palette.Orange, Color.white, 0.5f);
            UiFactory.Place((RectTransform)pause.transform, new Vector2(0f, 0.5f), new Vector2(100f, 0f), new Vector2(130f, 130f));
            pause.onClick.AddListener(() => PausePressed?.Invoke());

            title = Ui.Label(bar, "Başlık", "", 62, Palette.Ink);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -30f), new Vector2(500f, 84f));

            status = Ui.Label(bar, "Durum", "", 40, Palette.InkSoft);
            status.resizeTextForBestFit = true;
            status.resizeTextMinSize = 24;
            status.resizeTextMaxSize = 40;
            status.verticalOverflow = VerticalWrapMode.Truncate;
            UiFactory.Place(status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-10f, 34f), new Vector2(500f, 110f));

            heartsRow = UiFactory.CreateRect("Kalpler", bar);
            UiFactory.Place(heartsRow, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-36f, -34f), new Vector2(240f, HeartSize));
            var row = heartsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleRight;
            row.spacing = 6f;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var dogIcon = Ui.Picture(bar, "Köpek ikonu", Ui.Art.DogSleeping, Color.white);
            UiFactory.Place(dogIcon.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-150f, 30f), new Vector2(90f, 90f));
            dogsLeft = Ui.Label(bar, "Kalan köpek", "", 48, Palette.Ink, TextAnchor.MiddleLeft);
            UiFactory.Place(dogsLeft.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-28f, 30f), new Vector2(122f, 90f));

            hint = HintToast.Create(Ui, safe, 960f);
        }

        IEnumerator LoseHeart(RectTransform heart)
        {
            yield return Tween.Run(0.45f, t =>
            {
                float s = 1f + Tween.Bump(t) * 0.45f;
                heart.localScale = new Vector3(s, s, 1f);
                heart.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 14f * (1f - t));
            });
        }
    }
}
