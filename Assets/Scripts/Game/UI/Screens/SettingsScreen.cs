using System;
using UnityEngine;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.UI
{
    // Ayarlar: titreşim, ses ve ilerlemeyi sıfırlama.
    public sealed class SettingsScreen : UiScreen
    {
        const float CardWidth = 960f;
        const float CardHeight = 560f;

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

        protected override void BuildBackground(RectTransform root) => Ui.Background(root, Ui.Art.MapBackground);

        protected override void BuildContent(RectTransform safe)
        {
            Ui.TopBar(safe, "Ayarlar", out var back);
            back.onClick.AddListener(() => BackPressed?.Invoke());

            var card = Ui.Panel(safe, "Kart", Palette.Cream).rectTransform;
            UiFactory.Place(card, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -(UiFactory.TopBarHeight + 70f)), new Vector2(CardWidth, CardHeight));

            vibration = Row(card, "Titreşim", "Köpek uyanınca telefon titrer.", -50f);
            vibration.Changed += value => VibrationChanged?.Invoke(value);
            sound = Row(card, "Ses", "Sesler yakında geliyor.", -200f);
            sound.Changed += value => SoundChanged?.Invoke(value);

            var line = Ui.RoundedBox(card, "Çizgi", Palette.CreamDark, 6f);
            UiFactory.Place(line.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -350f), new Vector2(CardWidth - 120f, 6f));

            var reset = Ui.TextButton(card, "Sıfırla", "İlerlemeyi sıfırla", Palette.Red, 46);
            UiFactory.Place((RectTransform)reset.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 50f), new Vector2(620f, 120f));
            reset.onClick.AddListener(() => ResetPressed?.Invoke());

            var footer = Ui.Label(safe, "Sürüm", $"Uyuyan Köpekler  ·  sürüm {Application.version}", 32, Palette.Cream);
            UiFactory.AddOutline(footer, Palette.Ink, 2f);
            UiFactory.Place(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(900f, 60f));
        }

        PillToggle Row(Transform card, string title, string subtitle, float top)
        {
            var label = Ui.Label(card, title, title, 52, Palette.Ink, TextAnchor.MiddleLeft);
            UiFactory.Place(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, top), new Vector2(620f, 64f));

            var note = Ui.Label(card, title + " açıklama", subtitle, 32, Palette.InkSoft, TextAnchor.MiddleLeft);
            UiFactory.Place(note.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, top - 64f), new Vector2(620f, 50f));

            var toggle = PillToggle.Create(Ui, card, title + " anahtarı");
            UiFactory.Place((RectTransform)toggle.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-60f, top - 12f), new Vector2(150f, 84f));
            return toggle;
        }
    }
}
