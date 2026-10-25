using System;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;
using UyuyanKopekler.Audio;

namespace UyuyanKopekler.UI
{
    // Açık/kapalı anahtarı: mürekkep konturlu hap, içinde kayan yuvarlak topuz.
    // Açıkken nane yeşili, kapalıyken kâğıt rengi.
    public sealed class PillToggle : MonoBehaviour
    {
        const float KnobSize = 66f;
        const float KnobInset = 8f;
        const float SlideSpeed = 12f;

        Card track;
        RectTransform knob;
        ISoundPlayer sound;
        float knobPosition; // 0 = solda (kapalı), 1 = sağda (açık)

        public event Action<bool> Changed;
        public bool Value { get; private set; }

        public static PillToggle Create(UiFactory ui, Transform parent, string name)
        {
            var track = ui.Card(parent, name, Palette.PaperDark, 6f, circle: false, cornerScale: 1.25f);
            track.Face.raycastTarget = true;

            var toggle = track.Root.gameObject.AddComponent<PillToggle>();
            toggle.track = track;
            toggle.sound = ui.Sound;

            var knobCard = ui.Card(track.Body, "Topuz", Palette.White, 4f, circle: true);
            toggle.knob = knobCard.Root;
            toggle.knob.sizeDelta = new Vector2(KnobSize, KnobSize);

            var button = track.Root.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.targetGraphic = track.Face;
            button.onClick.AddListener(toggle.Flip);

            toggle.Render(0f);
            return toggle;
        }

        public void SetValue(bool value, bool notify)
        {
            Value = value;
            track.SetColor(value ? Palette.Mint : Palette.PaperDark);
            if (notify) Changed?.Invoke(value);
        }

        // Ekran açılırken animasyonsuz yerine oturt.
        public void Snap() => Render(Value ? 1f : 0f);

        void Flip()
        {
            sound.Play(Sfx.Toggle);
            SetValue(!Value, notify: true);
        }

        void Update()
        {
            float target = Value ? 1f : 0f;
            if (Mathf.Approximately(knobPosition, target)) return;
            Render(Mathf.MoveTowards(knobPosition, target, SlideSpeed * Time.unscaledDeltaTime));
        }

        void Render(float position)
        {
            knobPosition = position;
            var anchor = new Vector2(position, 0.5f);
            knob.anchorMin = anchor;
            knob.anchorMax = anchor;
            knob.pivot = anchor;
            knob.anchoredPosition = new Vector2(Mathf.Lerp(KnobInset, -KnobInset, position), 2f);
        }
    }
}
