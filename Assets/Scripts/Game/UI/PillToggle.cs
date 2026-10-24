using System;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Açık/kapalı anahtarı: yeşil (açık) / gri (kapalı) hap içinde kayan beyaz topuz.
    public sealed class PillToggle : MonoBehaviour
    {
        const float KnobInset = 8f;
        const float SlideSpeed = 14f;

        Image track;
        RectTransform knob;
        float knobPosition; // 0 = solda (kapalı), 1 = sağda (açık)

        public event Action<bool> Changed;
        public bool Value { get; private set; }

        public static PillToggle Create(UiFactory ui, Transform parent, string name)
        {
            var track = ui.RoundedBox(parent, name, Palette.Grey, 1.4f);
            track.raycastTarget = true;

            var toggle = track.gameObject.AddComponent<PillToggle>();
            toggle.track = track;

            var knobImage = ui.Picture(track.transform, "Topuz", ui.Art.Circle, Color.white);
            UiFactory.AddShadow(knobImage, Palette.Shadow, new Vector2(0f, -4f));
            toggle.knob = knobImage.rectTransform;
            toggle.knob.sizeDelta = new Vector2(64f, 64f);

            var button = track.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(toggle.Flip);

            toggle.Render(0f);
            return toggle;
        }

        public void SetValue(bool value, bool notify)
        {
            Value = value;
            track.color = value ? Palette.Green : Palette.Grey;
            if (notify) Changed?.Invoke(value);
        }

        // Ekran açılırken animasyonsuz yerine oturt.
        public void Snap() => Render(Value ? 1f : 0f);

        void Flip() => SetValue(!Value, notify: true);

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
            knob.anchoredPosition = new Vector2(Mathf.Lerp(KnobInset, -KnobInset, position), 0f);
        }
    }
}
