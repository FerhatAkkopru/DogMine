using System;
using UnityEngine;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Koleksiyon (iskelet): çimin altından çıkacak eşyalar. Sayılar 2. adımda bağlanacak.
    public sealed class CollectionScreen : UiScreen
    {
        const float CardWidth = 470f;
        const float CardHeight = 540f;
        const float CardGap = 30f;

        public event Action BackPressed;

        protected override void BuildBackground(RectTransform root) => Ui.Background(root, Ui.Art.MapBackground);

        protected override void BuildContent(RectTransform safe)
        {
            Ui.TopBar(safe, "Koleksiyon", out var back);
            back.onClick.AddListener(() => BackPressed?.Invoke());

            float firstRow = -(UiFactory.TopBarHeight + 60f);
            float secondRow = firstRow - CardHeight - CardGap;
            float x = (CardWidth + CardGap) * 0.5f;

            Card(safe, "Kemik", "Yardımcı almak için", Ui.Art.Bone, new Vector2(-x, firstRow));
            Card(safe, "Yıldız", "Macera hedefleri için", Ui.Art.Star, new Vector2(x, firstRow));
            Card(safe, "İnci", "Nadir bir hazine", Ui.Art.Pearl, new Vector2(-x, secondRow));
            Card(safe, "Altın tasma", "Çok nadir!", Ui.Art.Collar, new Vector2(x, secondRow));

            var footer = Ui.Label(safe, "Not", "Hepsi çimin altından çıkacak. Yakında!", 38, Palette.Cream);
            UiFactory.AddOutline(footer, Palette.Ink, 3f);
            UiFactory.Place(footer.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, secondRow - CardHeight - 40f), new Vector2(960f, 80f));
        }

        void Card(Transform parent, string name, string description, Sprite icon, Vector2 position)
        {
            var card = Ui.Panel(parent, name, Palette.Cream).rectTransform;
            UiFactory.Place(card, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, new Vector2(CardWidth, CardHeight));

            var glow = Ui.Picture(card, "Hale", Ui.Art.Circle, Palette.CreamDark);
            UiFactory.Place(glow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(230f, 230f));
            var picture = Ui.Picture(card, "Resim", icon, Color.white);
            UiFactory.Place(picture.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(190f, 190f));

            var title = Ui.Label(card, "Ad", name, 50, Palette.Ink);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -320f), new Vector2(CardWidth - 40f, 64f));

            var count = Ui.Label(card, "Sayı", "0", 56, Palette.Orange);
            UiFactory.Place(count.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -390f), new Vector2(CardWidth - 40f, 70f));

            var note = Ui.Label(card, "Açıklama", description, 32, Palette.InkSoft);
            UiFactory.Place(note.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -462f), new Vector2(CardWidth - 40f, 60f));
        }
    }
}
