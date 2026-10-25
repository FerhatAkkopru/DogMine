using System;
using UnityEngine;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Koleksiyon (iskelet): çimin altından çıkacak eşyalar. Sayılar 2. adımda bağlanacak.
    public sealed class CollectionScreen : UiScreen
    {
        const float CardWidth = 470f;
        const float CardHeight = 520f;
        const float CardGap = 34f;

        public event Action BackPressed;

        protected override void BuildBackground(RectTransform root) => Ui.Background(root);

        protected override void BuildContent(RectTransform safe)
        {
            Ui.TopBar(safe, "Koleksiyon", out var back);
            back.onClick.AddListener(() => BackPressed?.Invoke());

            float firstRow = -(UiFactory.TopBarHeight + 70f);
            float secondRow = firstRow - CardHeight - CardGap;
            float x = (CardWidth + CardGap) * 0.5f;

            Item(safe, "Kemik", "Yardımcı almak için", Ui.Art.Bone, Palette.Cream, new Vector2(-x, firstRow), -2f);
            Item(safe, "Yıldız", "Macera hedefleri için", Ui.Art.Star, Palette.MustardLight, new Vector2(x, firstRow), 1.5f);
            Item(safe, "İnci", "Nadir bir hazine", Ui.Art.Pearl, Palette.Pink, new Vector2(-x, secondRow), 1f);
            Item(safe, "Altın tasma", "Çok nadir!", Ui.Art.Collar, Palette.Mint, new Vector2(x, secondRow), -1.5f);

            var footer = Ui.Label(safe, "Not", "Hepsi çimin altından çıkacak. Yakında!", 36, Palette.InkSoft);
            UiFactory.Place(footer.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, secondRow - CardHeight - 50f), new Vector2(960f, 70f));
        }

        // Hafif eğik duran, elle yapıştırılmış gibi kart.
        void Item(Transform parent, string name, string description, Sprite icon, Color color, Vector2 position, float tilt)
        {
            var card = Ui.Card(parent, name, color, 12f);
            UiFactory.Place(card.Root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, new Vector2(CardWidth, CardHeight));
            card.Root.localRotation = Quaternion.Euler(0f, 0f, tilt);
            var body = card.Body;

            var picture = Ui.Picture(body, "Resim", icon, Color.white);
            UiFactory.Place(picture.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(200f, 200f));

            var title = Ui.Label(body, "Ad", name, 50, Palette.Ink, TextAnchor.MiddleCenter, display: true);
            UiFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(CardWidth - 40f, 70f));

            var count = Ui.Label(body, "Sayı", "0", 58, Palette.Coral, TextAnchor.MiddleCenter, display: true);
            UiFactory.Place(count.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -372f), new Vector2(CardWidth - 40f, 76f));

            var note = Ui.Label(body, "Açıklama", description, 30, Palette.InkSoft);
            UiFactory.Place(note.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -446f), new Vector2(CardWidth - 40f, 56f));
        }
    }
}
