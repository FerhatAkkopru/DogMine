using UnityEngine;

namespace UyuyanKopekler.UI
{
    // Bütün ekranların ortak tabanı: kurulum, yumuşak açılıp kapanma, güvenli alan.
    public abstract class UiScreen : MonoBehaviour
    {
        const float FadeSeconds = 0.18f;

        CanvasGroup group;
        Coroutine fade;

        protected UiFactory Ui { get; private set; }

        // Çentik ve alt çubuk dışında kalan alan; içerik buraya kurulur.
        protected RectTransform Safe { get; private set; }

        public bool IsVisible => gameObject.activeSelf && group.interactable;

        public void Build(UiFactory ui)
        {
            Ui = ui;
            group = gameObject.AddComponent<CanvasGroup>();
            var root = UiFactory.Stretch((RectTransform)transform);

            BuildBackground(root);
            Safe = UiFactory.Stretch(UiFactory.CreateRect("Güvenli alan", root));
            Safe.gameObject.AddComponent<SafeAreaFitter>();
            BuildContent(Safe);

            group.alpha = 0f;
            gameObject.SetActive(false);
        }

        // Tam ekran (güvenli alanın dışına taşan) arka plan.
        protected virtual void BuildBackground(RectTransform root) { }

        protected abstract void BuildContent(RectTransform safe);

        protected virtual void OnShown() { }

        public void Show()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            group.interactable = true;
            group.blocksRaycasts = true;
            OnShown();
            FadeTo(1f, deactivateAtEnd: false);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            group.interactable = false;
            group.blocksRaycasts = false;
            FadeTo(0f, deactivateAtEnd: true);
        }

        void FadeTo(float target, bool deactivateAtEnd)
        {
            if (fade != null) StopCoroutine(fade);
            float from = group.alpha;
            fade = StartCoroutine(Tween.Run(FadeSeconds, t =>
            {
                group.alpha = Mathf.Lerp(from, target, t);
                if (t >= 1f && deactivateAtEnd) gameObject.SetActive(false);
            }));
        }
    }
}
