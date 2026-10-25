using System;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Ekranın üstüne açılan tek pencere: arka planı karartır, kart "pop" yaparak gelir.
    // Kartın tepesinden bir köpek çizimi taşar. Mola, sonuç, bölüm girişi ve onay
    // pencerelerinin hepsi bunu kullanır.
    public sealed class DialogPanel : MonoBehaviour
    {
        const float CardWidth = 860f;
        const float IconSize = 360f;
        const float IconOverlap = 290f; // çizimin tepesi kartın üst kenarından bu kadar yukarıda: köpek kartın üstüne oturur
        const float OpenSeconds = 0.32f;
        const float CloseSeconds = 0.14f;

        CanvasGroup group;
        RectTransform cardRoot;
        RectOffset paddingWithIcon;
        RectOffset paddingWithoutIcon;
        VerticalLayoutGroup layout;
        Image icon;
        Text title;
        Text body;
        Button primary;
        Card primaryCard;
        Text primaryLabel;
        Button secondary;
        Text secondaryLabel;
        DialogContent content;
        Coroutine running;

        public bool IsOpen { get; private set; }

        public void Build(UiFactory ui)
        {
            UiFactory.Stretch((RectTransform)transform);
            group = gameObject.AddComponent<CanvasGroup>();

            var backdrop = ui.Picture(transform, "Karartma", null, Palette.Backdrop);
            backdrop.preserveAspect = false;
            backdrop.raycastTarget = true; // arkadaki düğmelere dokunulmasın
            UiFactory.Stretch(backdrop.rectTransform);

            var card = ui.Card(transform, "Kart", Palette.Cream, 14f);
            cardRoot = card.Root;
            UiFactory.Place(cardRoot, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(CardWidth, 0f));
            card.Shadow.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            card.Body.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            paddingWithIcon = new RectOffset(64, 64, (int)(IconSize - IconOverlap) + 40, 64);
            paddingWithoutIcon = new RectOffset(64, 64, 60, 64);
            layout = cardRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = paddingWithIcon;
            layout.spacing = 22f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            cardRoot.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Kartın tepesinden taşan çizim (düzenin dışında)
            icon = ui.Picture(cardRoot, "Çizim", null, Color.white);
            icon.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiFactory.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, IconOverlap), new Vector2(IconSize, IconSize));

            title = ui.Label(cardRoot, "Başlık", "", 72, Palette.Ink, TextAnchor.MiddleCenter, display: true);
            body = ui.Label(cardRoot, "Metin", "", 38, Palette.InkSoft);
            body.lineSpacing = 1.1f;
            SetHeight(UiFactory.CreateRect("Boşluk", cardRoot).gameObject, 10f);

            primary = ui.TextButton(cardRoot, "Birincil", "", Palette.Mint, out primaryCard, 56);
            primaryLabel = primary.GetComponentInChildren<Text>();
            SetHeight(primary.gameObject, 146f);
            primary.onClick.AddListener(() => Choose(content?.OnPrimary));

            secondary = ui.TextButton(cardRoot, "İkincil", "", Palette.Paper, 46);
            secondaryLabel = secondary.GetComponentInChildren<Text>();
            SetHeight(secondary.gameObject, 116f);
            secondary.onClick.AddListener(() => Choose(content?.OnSecondary));

            gameObject.SetActive(false);
        }

        public void Show(DialogContent dialog)
        {
            content = dialog;
            bool hasIcon = dialog.Icon != null;
            icon.gameObject.SetActive(hasIcon);
            icon.sprite = dialog.Icon;
            layout.padding = hasIcon ? paddingWithIcon : paddingWithoutIcon;
            title.text = dialog.Title;
            body.gameObject.SetActive(!string.IsNullOrEmpty(dialog.Body));
            body.text = dialog.Body;

            primaryLabel.text = dialog.PrimaryLabel;
            primaryCard.SetColor(dialog.PrimaryColor);
            secondary.gameObject.SetActive(!string.IsNullOrEmpty(dialog.SecondaryLabel));
            secondaryLabel.text = dialog.SecondaryLabel;

            IsOpen = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            group.blocksRaycasts = true;
            Animate(OpenSeconds, t =>
            {
                group.alpha = Mathf.Clamp01(t * 2.5f);
                float s = Mathf.LerpUnclamped(0.8f, 1f, Tween.OutBack(t));
                cardRoot.localScale = new Vector3(s, s, 1f);
                // çizim kartla birlikte gelir, biraz da zıplar
                float hop = Tween.Bump(Mathf.Clamp01(t * 1.3f)) * 30f;
                icon.rectTransform.anchoredPosition = new Vector2(0f, IconOverlap + hop);
            });
        }

        public void Hide()
        {
            if (!IsOpen) return;
            IsOpen = false;
            group.blocksRaycasts = false;
            Animate(CloseSeconds, t =>
            {
                group.alpha = 1f - t;
                if (t >= 1f) gameObject.SetActive(false);
            });
        }

        // Android geri tuşu: OnBack yoksa ikinci düğme ("Vazgeç" gibi) seçilmiş sayılır.
        public void Back()
        {
            if (!IsOpen) return;
            bool hasSecondary = !string.IsNullOrEmpty(content.SecondaryLabel);
            Choose(content.OnBack ?? (hasSecondary ? content.OnSecondary : content.OnPrimary));
        }

        void Choose(Action action)
        {
            if (!IsOpen) return;
            Hide();
            action?.Invoke();
        }

        void Animate(float seconds, Action<float> step)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Tween.Run(seconds, step));
        }

        static void SetHeight(GameObject go, float height)
        {
            var element = go.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
        }
    }
}
