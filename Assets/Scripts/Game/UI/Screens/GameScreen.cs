using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;
using UyuyanKopekler.Audio;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.UI
{
    // Oyun sırasındaki arayüz (arka planı saydam; bahçe ve kâğıt zemin dünyada çizilir):
    // üstte mola, başlık, mod bilgisi, kalpler ve kalan köpek; altta kedi düğmesi ve kemik sayacı;
    // aralarında ipucu balonu. Toplanan eşyalar kareden sayaca uçar.
    public sealed class GameScreen : UiScreen
    {
        const float HudHeight = 230f;
        const float BoardGap = 26f;
        const float BarHeight = 124f;
        const float BarBottom = 30f;
        const float BottomReserve = BarBottom + BarHeight + 130f; // alt bar + ipucu balonu
        const float HeartSize = 70f;

        Text title;
        Text status;
        Text dogsLeft;
        Text bones;
        RectTransform bonesIcon;
        RectTransform bonesChip;
        Card catCard;
        Text catLabel;
        RectTransform catPrice;
        CanvasGroup catGroup;
        bool catArmed;
        RectTransform heartsRow;
        RectTransform effects;
        readonly List<Image> hearts = new List<Image>();
        HintToast hint;
        Canvas rootCanvas;
        int shownHearts = -1;

        public event Action PausePressed;
        public event Action CatPressed;

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
        public void SetBones(int count) => bones.text = count.ToString();
        public void ShowHint(string text, float seconds) => hint.Show(text, seconds);
        public void HideHint() => hint.Hide();

        // armed: oyuncu kediyi bırakacağı kareyi seçiyor. affordable: kemik yetiyor mu.
        public void SetCatState(bool armed, bool affordable)
        {
            catArmed = armed;
            catCard.SetColor(armed ? Palette.Coral : Palette.Mint);
            catLabel.text = armed ? "Kare seç" : "Kedi";
            UiFactory.Stretch(catLabel.rectTransform, 132f, armed ? 10f : 52f, 10f, 10f);
            catPrice.gameObject.SetActive(!armed);
            catGroup.alpha = affordable || armed ? 1f : 0.5f;
            if (!armed) catCard.Root.localScale = Vector3.one;
        }

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

        // Çimin altından çıkan eşya ekrandaki yerinden (piksel) fırlar ve uçar: kemik sayaca,
        // diğerleri büyüyüp yukarı süzülerek kaybolur. Varınca onArrive çağrılır.
        public void FlyItem(ItemType type, Vector2 screenPosition, float delay, Action onArrive)
        {
            if (!isActiveAndEnabled)
            {
                onArrive?.Invoke();
                return;
            }
            StartCoroutine(Fly(type, screenPosition, delay, onArrive));
        }

        Sprite ItemSprite(ItemType type)
        {
            switch (type)
            {
                case ItemType.Star: return Ui.Art.Star;
                case ItemType.Pearl: return Ui.Art.Pearl;
                case ItemType.GoldCollar: return Ui.Art.Collar;
                default: return Ui.Art.Bone;
            }
        }

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
            var dogIcon = Ui.Picture(chip.Body, "Köpek", Ui.Art.Dog(0, DogFrame.Sleep0), Color.white);
            UiFactory.Place(dogIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(60f, 4f), new Vector2(100f, 100f));
            dogsLeft = Ui.Label(chip.Body, "Sayı", "", 40, Palette.Ink, TextAnchor.MiddleLeft, display: true);
            UiFactory.Place(dogsLeft.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(92f, 70f));

            BuildBottomBar(safe);
            hint = HintToast.Create(Ui, safe, 940f, BarBottom + BarHeight + 26f);

            effects = UiFactory.Stretch(UiFactory.CreateRect("Efektler", safe));
        }

        void BuildBottomBar(RectTransform safe)
        {
            var cat = Ui.TextButton(safe, "Kedi", "", Palette.Mint, out catCard, 44);
            UiFactory.Place(catCard.Root, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(34f, BarBottom), new Vector2(330f, BarHeight));
            cat.onClick.AddListener(() => CatPressed?.Invoke());
            catGroup = catCard.Root.gameObject.AddComponent<CanvasGroup>();

            var icon = Ui.Picture(catCard.Body, "Kedi ikonu", Ui.Art.CatIcon, Color.white);
            UiFactory.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(70f, 4f), new Vector2(110f, 110f));
            catLabel = cat.GetComponentInChildren<Text>();
            catLabel.alignment = TextAnchor.MiddleLeft;
            catLabel.resizeTextForBestFit = true;
            catLabel.resizeTextMinSize = 26;
            catLabel.resizeTextMaxSize = 44;

            catPrice = UiFactory.CreateRect("Fiyat", catCard.Body);
            UiFactory.Stretch(catPrice, 132f, 10f, 10f, 66f);
            var price = Ui.Label(catPrice, "Fiyat yazısı", CatHelper.BonePrice.ToString(), 34, Palette.Ink, TextAnchor.MiddleLeft);
            UiFactory.Stretch(price.rectTransform, 0f, 0f, 90f, 0f);
            var bone = Ui.Picture(catPrice, "Kemik", Ui.Art.Bone, Color.white);
            UiFactory.Place(bone.rectTransform, new Vector2(0f, 0.5f), new Vector2(76f, 2f), new Vector2(54f, 54f));

            var bonesCard = Ui.Card(safe, "Kemikler", Palette.Cream, 8f, circle: false, cornerScale: 1.3f);
            bonesChip = bonesCard.Root;
            UiFactory.Place(bonesChip, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-34f, BarBottom + 8f), new Vector2(236f, BarHeight - 16f));
            var bonesImage = Ui.Picture(bonesCard.Body, "Kemik ikonu", Ui.Art.Bone, Color.white);
            bonesIcon = bonesImage.rectTransform;
            UiFactory.Place(bonesIcon, new Vector2(0f, 0.5f), new Vector2(64f, 2f), new Vector2(92f, 92f));
            bones = Ui.Label(bonesCard.Body, "Kemik sayısı", "0", 48, Palette.Ink, TextAnchor.MiddleLeft, display: true);
            UiFactory.Stretch(bones.rectTransform, 116f, 0f, 10f, 0f);
        }

        protected override void OnShown()
        {
            // Ekran yarıda kapandıysa uçuş ve sallanma animasyonlarından kalanları temizle.
            foreach (Transform child in effects) Destroy(child.gameObject);
            bonesChip.localScale = Vector3.one;
            foreach (var heart in hearts)
            {
                heart.rectTransform.localScale = Vector3.one;
                heart.rectTransform.localRotation = Quaternion.identity;
            }
        }

        void Update()
        {
            // Kedi seçimdeyken düğme nabız gibi atar.
            if (!catArmed) return;
            float s = 1f + Mathf.Sin(Time.unscaledTime * 8f) * 0.04f;
            catCard.Root.localScale = new Vector3(s, s, 1f);
        }

        IEnumerator Fly(ItemType type, Vector2 screenPosition, float delay, Action onArrive)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            bool toBones = type == ItemType.Bone;
            bool rare = type == ItemType.Pearl || type == ItemType.GoldCollar;
            Ui.Sound.Play(rare ? Sfx.Rare : Sfx.Collect);

            var image = Ui.Picture(effects, "Uçan eşya", ItemSprite(type), Color.white);
            var rt = image.rectTransform;
            UiFactory.Place(rt, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 110f));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(effects, screenPosition, null, out var start);
            Vector2 target;
            if (toBones)
            {
                var bonesScreen = RectTransformUtility.WorldToScreenPoint(null, bonesIcon.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(effects, bonesScreen, null, out target);
            }
            else
            {
                target = new Vector2(0f, effects.rect.height * 0.5f - 120f);
            }

            // 1) kareden fırlar
            float popScale = toBones ? 1.2f : 2.2f;
            yield return Tween.Run(toBones ? 0.22f : 0.5f, t =>
            {
                rt.anchoredPosition = start + new Vector2(0f, Tween.OutCubic(t) * 70f);
                float s = Mathf.LerpUnclamped(0f, popScale, Tween.OutBack(t));
                rt.localScale = new Vector3(s, s, 1f);
                rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 9f) * 12f);
            });
            if (!toBones) yield return new WaitForSecondsRealtime(0.35f);

            // 2) kavis çizerek hedefe uçar
            var from = rt.anchoredPosition;
            var control = Vector2.Lerp(from, target, 0.5f) + new Vector2(0f, 260f);
            yield return Tween.Run(0.55f, t =>
            {
                float e = t * t * (3f - 2f * t);
                var a = Vector2.Lerp(from, control, e);
                var b = Vector2.Lerp(control, target, e);
                rt.anchoredPosition = Vector2.Lerp(a, b, e);
                float s = Mathf.Lerp(popScale, 0.55f, e);
                rt.localScale = new Vector3(s, s, 1f);
                if (!toBones) image.color = Palette.WithAlpha(Color.white, 1f - e * e);
            });
            Destroy(image.gameObject);
            onArrive?.Invoke();
            if (toBones) yield return Tween.Run(0.2f, t =>
            {
                float s = 1f + Tween.Bump(t) * 0.18f;
                bonesChip.localScale = new Vector3(s, s, 1f);
            });
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
