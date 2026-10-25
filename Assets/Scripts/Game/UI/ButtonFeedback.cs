using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UyuyanKopekler.UI
{
    // Düğmeye basılınca yüzü sert gölgesinin üstüne doğru "çöker", bırakınca geri kalkar.
    public sealed class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        const float Speed = 30f;

        Selectable selectable;
        RectTransform body;
        float depth;
        float target;

        public void Init(Selectable owner, RectTransform pressedPart, float pressDepth)
        {
            selectable = owner;
            body = pressedPart;
            depth = pressDepth;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable == null || selectable.interactable) target = depth;
        }

        public void OnPointerUp(PointerEventData eventData) => target = 0f;
        public void OnPointerExit(PointerEventData eventData) => target = 0f;

        void OnDisable()
        {
            target = 0f;
            if (body != null) body.anchoredPosition = Vector2.zero;
        }

        void Update()
        {
            if (body == null) return;
            var position = body.anchoredPosition;
            float y = Mathf.Lerp(position.y, -target, 1f - Mathf.Exp(-Speed * Time.unscaledDeltaTime));
            body.anchoredPosition = new Vector2(0f, y);
        }
    }
}
