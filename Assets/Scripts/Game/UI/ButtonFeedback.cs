using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UyuyanKopekler.UI
{
    // Düğmeye basılınca hafifçe küçülür, bırakınca geri büyür.
    public sealed class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        const float PressedScale = 0.93f;
        const float Speed = 18f;

        Selectable selectable;
        float target = 1f;

        void Awake() => selectable = GetComponent<Selectable>();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable == null || selectable.interactable) target = PressedScale;
        }

        public void OnPointerUp(PointerEventData eventData) => target = 1f;
        public void OnPointerExit(PointerEventData eventData) => target = 1f;

        void OnDisable()
        {
            target = 1f;
            transform.localScale = Vector3.one;
        }

        void Update()
        {
            float s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-Speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
