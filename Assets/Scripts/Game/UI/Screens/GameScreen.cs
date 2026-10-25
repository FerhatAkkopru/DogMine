using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Oyun sırasındaki arayüz (arka planı saydam; bahçe ve kâğıt zemin dünyada çizilir):
    // solda mola, ortada başlık ve mod bilgisi, sağda kalpler ve kalan köpek, altta ipucu balonu.
    public sealed class GameScreen : UiScreen
    {
        const float HudHeight = 230f;
        const float BoardGap = 26f;
        const float BottomReserve = 150f;
        const float HeartSize = 70f;

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
                float top = Screen.height - safeArea.yMax + (HudHeight + BoardGap) * scale;
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

            // Yeni giden kalp küçük bir sarsıntıyla solar.
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

            var pause = Ui.IconButton(safe, "Mola", Ui.Art.IconPause, Palette.Cream, Palette.Ink, 0.46f);
            UiFactory.Place((RectTransform)pause.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -34f), new Vector2(120f, 120f));
            pause.onClick.AddListener(() => PausePressed?.Invoke());

            title = Ui.Label(safe, "Başlık", "", 66, Palette.Ink, TextAnchor.MiddleCenter, display: true);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-20f, -30f), new Vector2(520f, 90f));

            var pill = Ui.Card(safe, "Durum", Palette.MustardLight, 6f, circle: false, cornerScale: 1.5f);
            UiFactory.Place(pill.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-20f, -128f), new Vector2(480f, 76f));
            status = Ui.Label(pill.Body, "Yazı", "", 34, Palette.Ink);
            status.resizeTextForBestFit = true;
            status.resizeTextMinSize = 22;
            status.resizeTextMaxSize = 34;
            status.verticalOverflow = VerticalWrapMode.Truncate;
            UiFactory.Stretch(status.rectTransform, 18f, 4f, 18f, 4f);

            heartsRow = UiFactory.CreateRect("Kalpler", safe);
            UiFactory.Place(heartsRow, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -34f), new Vector2(240f, HeartSize));
            var row = heartsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleRight;
            row.spacing = 2f;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            var chip = Ui.Card(safe, "Kalan köpek", Palette.Cream, 6f, circle: false, cornerScale: 1.5f);
            UiFactory.Place(chip.Root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -124f), new Vector2(226f, 80f));
            var dogIcon = Ui.Picture(chip.Body, "Köpek", Ui.Art.CurledDog(0), Color.white);
            UiFactory.Place(dogIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(60f, 4f), new Vector2(96f, 96f));
            dogsLeft = Ui.Label(chip.Body, "Sayı", "", 40, Palette.Ink, TextAnchor.MiddleLeft, display: true);
            UiFactory.Place(dogsLeft.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(92f, 70f));

            hint = HintToast.Create(Ui, safe, 940f);
        }

        IEnumerator LoseHeart(RectTransform heart)
        {
            yield return Tween.Run(0.5f, t =>
            {
                float s = 1f + Tween.Bump(t) * 0.5f;
                heart.localScale = new Vector3(s, s, 1f);
                heart.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 16f * (1f - t));
            });
        }
    }
}
