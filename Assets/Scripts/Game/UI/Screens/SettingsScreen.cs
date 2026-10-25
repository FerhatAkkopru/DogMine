using System;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Anim;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.UI
{
    // Ayarlar: titreşim, ses, pati izi ipuçları ve ilerlemeyi sıfırlama.
    public sealed class SettingsScreen : UiScreen
    {
        const float CardWidth = 960f;
        const float CardHeight = 760f;
        const float RowStep = 160f;

        PillToggle vibration;
        PillToggle sound;
        PillToggle pawHints;

        public event Action BackPressed;
        public event Action<bool> VibrationChanged;
        public event Action<bool> SoundChanged;
        public event Action<bool> PawHintsChanged;
        public event Action ResetPressed;

        public void Bind(GameSettings settings)
        {
            vibration.SetValue(settings.Vibration, notify: false);
            sound.SetValue(settings.Sound, notify: false);
            pawHints.SetValue(settings.PawHints, notify: false);
            vibration.Snap();
            sound.Snap();
            pawHints.Snap();
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

            float top = -56f;
            vibration = Row(body, "Titreşim", "Köpek uyanınca telefon titrer.", top);
            vibration.Changed += value => VibrationChanged?.Invoke(value);
            sound = Row(body, "Ses", "Kazı, havlama ve miyav sesleri.", top -= RowStep);
            sound.Changed += value => SoundChanged?.Invoke(value);
            pawHints = Row(body, "Pati izleri", "Kapalıysa ipuçları rakamla gösterilir.", top -= RowStep);
            pawHints.Changed += value => PawHintsChanged?.Invoke(value);

            var line = Ui.Picture(body, "Çizgi", Ui.Art.Underline, Palette.WithAlpha(Palette.Ink, 0.18f));
            line.preserveAspect = false;
            UiFactory.Place(line.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, top - 164f), new Vector2(CardWidth - 140f, 16f));

            var reset = Ui.TextButton(body, "Sıfırla", "İlerlemeyi sıfırla", Palette.Coral, 46);
            UiFactory.Place((RectTransform)reset.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 64f), new Vector2(620f, 124f));
            reset.onClick.AddListener(() => ResetPressed?.Invoke());

            var dog = Ui.AnimatedPicture(safe, "Uyuyan köpek", Ui.Clips.Dog(4, Clips.Sleep), Clips.Sleep.Fps);
            UiFactory.Place((RectTransform)dog.transform, new Vector2(0.5f, 0f), new Vector2(0f, 320f), new Vector2(480f, 480f));

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
