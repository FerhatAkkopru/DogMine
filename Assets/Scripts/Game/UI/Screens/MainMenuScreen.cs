using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Açılış ekranı: nefes alan uyuyan köpek logosu, Klasik ve Macera düğmeleri,
    // altta Koleksiyon ve Ayarlar.
    public sealed class MainMenuScreen : UiScreen
    {
        const float ZzzEverySeconds = 1.3f;

        RectTransform logoDog;
        Text classicInfo;
        Text adventureInfo;
        readonly List<Text> zzzPool = new List<Text>();
        int nextZzz;
        float zzzTimer;

        public event Action ClassicPressed;
        public event Action AdventurePressed;
        public event Action CollectionPressed;
        public event Action SettingsPressed;

        public void SetInfo(string classic, string adventure)
        {
            classicInfo.text = classic;
            adventureInfo.text = adventure;
        }

        protected override void BuildBackground(RectTransform root) => Ui.Background(root, Ui.Art.MenuBackground);

        protected override void BuildContent(RectTransform safe)
        {
            logoDog = Ui.Picture(safe, "Logo köpek", Ui.Art.DogSleeping, Color.white).rectTransform;
            UiFactory.Place(logoDog, new Vector2(0.5f, 0.81f), Vector2.zero, new Vector2(420f, 420f));

            for (int i = 0; i < 3; i++)
            {
                var z = Ui.Label(safe, "Z", "Z", 64, Palette.Cream);
                UiFactory.AddOutline(z, Palette.Ink, 3f);
                UiFactory.Place(z.rectTransform, new Vector2(0.5f, 0.81f), Vector2.zero, new Vector2(120f, 120f));
                z.gameObject.SetActive(false);
                zzzPool.Add(z);
            }

            var title = Ui.Label(safe, "Başlık", "Uyuyan\nKöpekler", 116, Palette.Cream);
            title.lineSpacing = 0.9f;
            UiFactory.AddOutline(title, Palette.Ink, 6f);
            UiFactory.AddShadow(title, Palette.Shadow, new Vector2(0f, -12f));
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 0.635f), Vector2.zero, new Vector2(1000f, 290f));

            classicInfo = ModeButton(safe, "Klasik", Palette.Orange, 0.485f, () => ClassicPressed?.Invoke());
            adventureInfo = ModeButton(safe, "Macera", Palette.Teal, 0.36f, () => AdventurePressed?.Invoke());

            SmallButton(safe, "Koleksiyon", Ui.Art.Bone, Color.white, -170f, () => CollectionPressed?.Invoke());
            SmallButton(safe, "Ayarlar", Ui.Art.IconGear, Palette.Ink, 170f, () => SettingsPressed?.Invoke());
        }

        // Büyük mod düğmesi: üstte mod adı, altta küçük bilgi satırı. Bilgi yazısını döner.
        Text ModeButton(RectTransform parent, string label, Color color, float height, Action onClick)
        {
            var button = Ui.TextButton(parent, label, label, color, 78);
            UiFactory.Place((RectTransform)button.transform, new Vector2(0.5f, height), Vector2.zero, new Vector2(760f, 190f));
            UiFactory.Stretch(button.GetComponentInChildren<Text>().rectTransform, 16f, 66f, 16f, 6f);

            var info = Ui.Label(button.transform, "Bilgi", "", 36, Palette.WithAlpha(Color.white, 0.92f));
            UiFactory.Stretch(info.rectTransform, 16f, 16f, 16f, 118f);
            button.onClick.AddListener(() => onClick());
            return info;
        }

        void SmallButton(RectTransform parent, string label, Sprite icon, Color iconColor, float x, Action onClick)
        {
            var anchor = new Vector2(0.5f, 0.13f);
            var button = Ui.IconButton(parent, label, icon, Palette.Cream, iconColor, 0.62f);
            UiFactory.Place((RectTransform)button.transform, anchor, new Vector2(x, 24f), new Vector2(170f, 170f));
            button.onClick.AddListener(() => onClick());

            var text = Ui.Label(parent, label + " yazısı", label, 38, Palette.Cream);
            UiFactory.AddOutline(text, Palette.Ink, 3f);
            UiFactory.Place(text.rectTransform, anchor, new Vector2(x, -96f), new Vector2(320f, 60f));
        }

        protected override void OnShown()
        {
            // Ekran kapanırken yarım kalan "Z"ler takılı kalmasın.
            foreach (var z in zzzPool) z.gameObject.SetActive(false);
            zzzTimer = 0f;
        }

        void Update()
        {
            // Nefes alma: hafifçe şişip inen logo.
            float breath = Mathf.Sin(Time.unscaledTime * 2.2f) * 0.03f;
            logoDog.localScale = new Vector3(1f + breath * 0.4f, 1f + breath, 1f);

            zzzTimer += Time.unscaledDeltaTime;
            if (zzzTimer >= ZzzEverySeconds)
            {
                zzzTimer = 0f;
                var z = zzzPool[nextZzz];
                nextZzz = (nextZzz + 1) % zzzPool.Count;
                StartCoroutine(FloatZ(z));
            }
        }

        // Köpeğin kafasından süzülüp kaybolan "Z".
        IEnumerator FloatZ(Text z)
        {
            z.gameObject.SetActive(true);
            var rt = z.rectTransform;
            var start = logoDog.anchoredPosition + new Vector2(150f, 130f);
            yield return Tween.Run(2.4f, t =>
            {
                rt.anchoredPosition = start + new Vector2(Mathf.Sin(t * 5f) * 24f + t * 60f, t * 220f);
                float s = Mathf.Lerp(0.5f, 1.2f, t);
                rt.localScale = new Vector3(s, s, 1f);
                z.color = Palette.WithAlpha(Palette.Cream, Tween.Bump(t));
            });
            z.gameObject.SetActive(false);
        }
    }
}
