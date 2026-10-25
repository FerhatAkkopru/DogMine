using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Açılış ekranı: el yazısı başlık, köpek yatağında uyuyan üç köpek (nefes alırlar, Zzz çıkar),
    // Klasik ve Macera düğmeleri, altta Koleksiyon ve Ayarlar.
    public sealed class MainMenuScreen : UiScreen
    {
        const float ZzzEverySeconds = 1.1f;
        static readonly Vector2 HeroSize = new Vector2(1000f, 560f);

        // Yataktaki köpekler: kürk, poz, konum (yatak merkezine göre), boy, aynalı mı, nefes evresi
        static readonly (int coat, bool curled, Vector2 position, float size, bool flip, float phase)[] Sleepers =
        {
            (1, false, new Vector2(-170f, 46f), 480f, false, 0f),   // lacivert, başını patisine koymuş
            (0, true, new Vector2(208f, 58f), 400f, true, 1.7f),    // hardal, kıvrılmış
            (3, true, new Vector2(22f, -6f), 270f, false, 3.1f),    // pembe yavru, öndeki
        };

        // "Z"lerin çıktığı yerler (yatak merkezine göre): köpeklerin kafalarının üstü
        static readonly Vector2[] ZzzSources = { new Vector2(-262f, 70f), new Vector2(300f, 92f), new Vector2(-30f, 40f) };

        RectTransform hero;
        readonly List<(RectTransform rt, float phase)> breathers = new List<(RectTransform, float)>();
        readonly List<Text> zzzPool = new List<Text>();
        Text classicInfo;
        Text adventureInfo;
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

        protected override void BuildBackground(RectTransform root)
        {
            Ui.Background(root);
            // Köşeden geçen soluk pati izleri
            var paw = Palette.WithAlpha(Palette.Ink, 0.07f);
            var trail = new[] { (0.06f, 0.70f, 150f), (0.13f, 0.665f, 170f), (0.08f, 0.62f, 150f), (0.16f, 0.585f, 175f),
                                (0.86f, 0.30f, -20f), (0.94f, 0.265f, -5f), (0.89f, 0.22f, -20f) };
            foreach (var (x, y, rot) in trail)
                Ui.Doodle(root, Ui.Art.Paw, paw, new Vector2(x, y), Vector2.zero, 70f, rot);
        }

        protected override void BuildContent(RectTransform safe)
        {
            BuildTitle(safe);
            BuildHero(safe);

            classicInfo = ModeButton(safe, "Klasik", Palette.Coral, 0.335f, () => ClassicPressed?.Invoke());
            adventureInfo = ModeButton(safe, "Macera", Palette.Mustard, 0.225f, () => AdventurePressed?.Invoke());

            SmallButton(safe, "Koleksiyon", Ui.Art.Bone, Color.white, Palette.Pink, -150f, 0.6f, () => CollectionPressed?.Invoke());
            SmallButton(safe, "Ayarlar", Ui.Art.IconGear, Palette.Ink, Palette.Mint, 150f, 0.5f, () => SettingsPressed?.Invoke());
        }

        void BuildTitle(RectTransform safe)
        {
            var group = UiFactory.Place(UiFactory.CreateRect("Başlık", safe), new Vector2(0.5f, 0.84f), Vector2.zero, new Vector2(1000f, 330f));
            group.localRotation = Quaternion.Euler(0f, 0f, 3f);

            var top = Ui.Label(group, "Uyuyan", "Uyuyan", 108, Palette.Ink, TextAnchor.MiddleCenter, display: true);
            UiFactory.Place(top.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-40f, 0f), new Vector2(900f, 140f));

            var underline = Ui.Picture(group, "Fırça", Ui.Art.Underline, Palette.Mustard);
            underline.preserveAspect = false;
            UiFactory.Place(underline.rectTransform, new Vector2(0.5f, 0f), new Vector2(30f, 36f), new Vector2(700f, 92f));

            var bottom = Ui.Label(group, "Köpekler", "Köpekler", 150, Palette.Coral, TextAnchor.MiddleCenter, display: true);
            UiFactory.Place(bottom.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(30f, 20f), new Vector2(1000f, 190f));
        }

        void BuildHero(RectTransform safe)
        {
            hero = UiFactory.Place(UiFactory.CreateRect("Köpek yatağı", safe), new Vector2(0.5f, 0.565f), Vector2.zero, HeroSize);

            var back = Ui.Picture(hero, "Yatak arkası", Ui.Art.BedBack, Color.white);
            UiFactory.Stretch(back.rectTransform);

            foreach (var s in Sleepers)
            {
                var sprite = s.curled ? Ui.Art.CurledDog(s.coat) : Ui.Art.SleepingDog(s.coat);
                var dog = Ui.Picture(hero, "Köpek", sprite, Color.white);
                var rt = dog.rectTransform;
                // Nefes alırken karnından yukarı doğru şişsin diye pivot alta yakın.
                UiFactory.Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.2f),
                    s.position + new Vector2(0f, -s.size * 0.3f), new Vector2(s.size, s.size));
                rt.localScale = new Vector3(s.flip ? -1f : 1f, 1f, 1f);
                breathers.Add((rt, s.phase));
            }

            var front = Ui.Picture(hero, "Yatak önü", Ui.Art.BedFront, Color.white);
            UiFactory.Stretch(front.rectTransform);

            for (int i = 0; i < 4; i++)
            {
                var z = Ui.Label(hero, "Z", "z", 64, Palette.Ink, TextAnchor.MiddleCenter, display: true);
                UiFactory.Place(z.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
                z.gameObject.SetActive(false);
                zzzPool.Add(z);
            }
        }

        // Büyük mod düğmesi: üstte mod adı, altta küçük bilgi satırı. Bilgi yazısını döner.
        Text ModeButton(RectTransform parent, string label, Color color, float height, Action onClick)
        {
            var button = Ui.TextButton(parent, label, label, color, out var card, 74);
            UiFactory.Place((RectTransform)button.transform, new Vector2(0.5f, height), Vector2.zero, new Vector2(800f, 176f));
            UiFactory.Stretch(button.GetComponentInChildren<Text>().rectTransform, 20f, 62f, 20f, 8f);

            var info = Ui.Label(card.Body, "Bilgi", "", 34, Palette.WithAlpha(Palette.Ink, 0.75f));
            UiFactory.Stretch(info.rectTransform, 20f, 18f, 20f, 112f);
            button.onClick.AddListener(() => onClick());
            return info;
        }

        void SmallButton(RectTransform parent, string label, Sprite icon, Color iconColor, Color fill, float x, float iconScale, Action onClick)
        {
            var anchor = new Vector2(0.5f, 0.1f);
            var button = Ui.IconButton(parent, label, icon, fill, iconColor, iconScale);
            UiFactory.Place((RectTransform)button.transform, anchor, new Vector2(x, 26f), new Vector2(150f, 150f));
            button.onClick.AddListener(() => onClick());

            var text = Ui.Label(parent, label + " yazısı", label, 34, Palette.Ink);
            UiFactory.Place(text.rectTransform, anchor, new Vector2(x, -82f), new Vector2(300f, 50f));
        }

        protected override void OnShown()
        {
            foreach (var z in zzzPool) z.gameObject.SetActive(false);
            zzzTimer = 0f;
        }

        void Update()
        {
            // Nefes: her köpek kendi temposunda hafifçe şişip iner.
            float t = Time.unscaledTime;
            foreach (var (rt, phase) in breathers)
            {
                float breath = Mathf.Sin(t * 1.9f + phase);
                float flip = Mathf.Sign(rt.localScale.x);
                rt.localScale = new Vector3(flip * (1f + breath * 0.008f), 1f + breath * 0.025f, 1f);
            }

            zzzTimer += Time.unscaledDeltaTime;
            if (zzzTimer >= ZzzEverySeconds)
            {
                zzzTimer = 0f;
                var z = zzzPool[nextZzz % zzzPool.Count];
                var source = ZzzSources[nextZzz % ZzzSources.Length];
                nextZzz++;
                StartCoroutine(FloatZ(z, source));
            }
        }

        // Köpeğin kafasından süzülüp kaybolan "z".
        IEnumerator FloatZ(Text z, Vector2 start)
        {
            z.gameObject.SetActive(true);
            var rt = z.rectTransform;
            float drift = UnityEngine.Random.Range(30f, 70f);
            yield return Tween.Run(2.6f, p =>
            {
                rt.anchoredPosition = start + new Vector2(Mathf.Sin(p * 5f) * 18f + p * drift, p * 200f);
                float s = Mathf.Lerp(0.45f, 1.15f, p);
                rt.localScale = new Vector3(s, s, 1f);
                rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(p * 4f) * 12f);
                z.color = Palette.WithAlpha(Palette.Ink, Tween.Bump(p) * 0.85f);
            });
            z.gameObject.SetActive(false);
        }
    }
}
