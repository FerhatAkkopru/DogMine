using System;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Ekranın üstüne açılan tek pencere: arka planı karartır, kart "pop" yaparak gelir.
    // Mola, sonuç, bölüm girişi ve onay pencerelerinin hepsi bunu kullanır.
    public sealed class DialogPanel : MonoBehaviour
    {
        const float CardWidth = 880f;
        const float OpenSeconds = 0.28f;
        const float CloseSeconds = 0.14f;

        CanvasGroup group;
        RectTransform card;
        Image icon;
        Text title;
        Text body;
        Button primary;
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

            card = ui.Panel(transform, "Kart", Palette.Cream).rectTransform;
            UiFactory.Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(CardWidth, 0f));
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(60, 60, 50, 60);
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            card.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            icon = ui.Picture(card, "İkon", null, Color.white);
            SetHeight(icon.gameObject, 230f);

            title = ui.Label(card, "Başlık", "", 70, Palette.Ink);
            body = ui.Label(card, "Metin", "", 40, Palette.InkSoft);
            body.lineSpacing = 1.15f;
            SetHeight(UiFactory.CreateRect("Boşluk", card).gameObject, 6f);

            primary = ui.TextButton(card, "Birincil", "", Palette.Green, 56);
            primaryLabel = primary.GetComponentInChildren<Text>();
            SetHeight(primary.gameObject, 150f);
            primary.onClick.AddListener(() => Choose(content?.OnPrimary));

            secondary = ui.TextButton(card, "İkincil", "", Palette.CreamDark, 48);
            secondaryLabel = secondary.GetComponentInChildren<Text>();
            secondaryLabel.color = Palette.Ink;
            SetHeight(secondary.gameObject, 120f);
            secondary.onClick.AddListener(() => Choose(content?.OnSecondary));

            gameObject.SetActive(false);
        }

        public void Show(DialogContent dialog)
        {
            content = dialog;
            icon.gameObject.SetActive(dialog.Icon != null);
            icon.sprite = dialog.Icon;
            title.text = dialog.Title;
            body.gameObject.SetActive(!string.IsNullOrEmpty(dialog.Body));
            body.text = dialog.Body;

            primaryLabel.text = dialog.PrimaryLabel;
            primary.image.color = dialog.PrimaryColor;
            primary.GetComponent<Shadow>().effectColor = Palette.Darker(dialog.PrimaryColor, 0.3f);
            secondary.gameObject.SetActive(!string.IsNullOrEmpty(dialog.SecondaryLabel));
            secondaryLabel.text = dialog.SecondaryLabel;

            IsOpen = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            group.blocksRaycasts = true;
            Animate(OpenSeconds, t =>
            {
                group.alpha = Mathf.Clamp01(t * 2f);
                float s = Mathf.LerpUnclamped(0.85f, 1f, Tween.OutBack(t));
                card.localScale = new Vector3(s, s, 1f);
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

        static GameObject SetHeight(GameObject go, float height)
        {
            var element = go.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
            return go;
        }
    }
}
