using System;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.UI
{
    // Ayarlar: titreşim, ses ve ilerlemeyi sıfırlama.
    public sealed class SettingsScreen : UiScreen
    {
        const float CardWidth = 960f;
        const float CardHeight = 600f;

        PillToggle vibration;
        PillToggle sound;

        public event Action BackPressed;
        public event Action<bool> VibrationChanged;
        public event Action<bool> SoundChanged;
        public event Action ResetPressed;

        public void Bind(GameSettings settings)
        {
            vibration.SetValue(settings.Vibration, notify: false);
            sound.SetValue(settings.Sound, notify: false);
            vibration.Snap();
            sound.Snap();
        }

        protected override void BuildBackground(RectTransform root) => Ui.Background(root);

        protected override void BuildContent(RectTransform safe)
        {
            Ui.TopBar(safe, "Ayarlar", out var back);
            back.onClick.AddListener(() => BackPressed?.Invoke());

            var card = Ui.Card(safe, "Kart", Palette.Cream, 14f);
            UiFactory.Place(card.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -(UiFactory.TopBarHeight + 80f)), new Vector2(CardWidth, CardHeight));
            var body = card.Body;

            vibration = Row(body, "Titreşim", "Köpek uyanınca telefon titrer.", -56f);
            vibration.Changed += value => VibrationChanged?.Invoke(value);
            sound = Row(body, "Ses", "Sesler yakında geliyor.", -216f);
            sound.Changed += value => SoundChanged?.Invoke(value);

            var line = Ui.Picture(body, "Çizgi", Ui.Art.Underline, Palette.WithAlpha(Palette.Ink, 0.18f));
            line.preserveAspect = false;
            UiFactory.Place(line.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -380f), new Vector2(CardWidth - 140f, 16f));

            var reset = Ui.TextButton(body, "Sıfırla", "İlerlemeyi sıfırla", Palette.Coral, 46);
            UiFactory.Place((RectTransform)reset.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 64f), new Vector2(620f, 124f));
            reset.onClick.AddListener(() => ResetPressed?.Invoke());

            var dog = Ui.Picture(safe, "Uyuyan köpek", Ui.Art.SleepingDog(4), Color.white);
            UiFactory.Place(dog.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 300f), new Vector2(420f, 420f));

            var footer = Ui.Label(safe, "Sürüm", $"Uyuyan Köpekler  ·  sürüm {Application.version}", 30, Palette.InkSoft);
            UiFactory.Place(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(900f, 60f));
        }

        PillToggle Row(Transform card, string title, string subtitle, float top)
        {
            var label = Ui.Label(card, title, title, 54, Palette.Ink, TextAnchor.MiddleLeft, display: true);
            UiFactory.Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(64f, top), new Vector2(600f, 70f));

            var note = Ui.Label(card, title + " açıklama", subtitle, 30, Palette.InkSoft, TextAnchor.MiddleLeft);
            UiFactory.Place(note.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(64f, top - 70f), new Vector2(620f, 50f));

            var toggle = PillToggle.Create(Ui, card, title + " anahtarı");
            UiFactory.Place((RectTransform)toggle.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-64f, top - 16f), new Vector2(156f, 88f));
            return toggle;
        }
    }
}
