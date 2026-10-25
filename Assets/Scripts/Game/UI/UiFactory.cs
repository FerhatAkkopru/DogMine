using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Arayüz parçalarını aynı elle çizilmiş stille kodla kurar: kâğıt zemin, mürekkep kontur,
    // sert gölge, Shantell Sans yazı. Ölçüler 1080×1920 referans ekrana göredir.
    public sealed class UiFactory
    {
        public const int UiLayer = 5;
        public const float TopBarHeight = 150f;
        public const float ShadowDepth = 10f;

        public IGameArt Art { get; }

        public UiFactory(IGameArt art)
        {
            Art = art;
        }

        // ---------- Yerleşim yardımcıları ----------

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = UiLayer };
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        // Ebeveyni kenarlardan verilen boşluklarla doldurur.
        public static RectTransform Stretch(RectTransform rt, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        // anchor: ebeveyn içindeki bağlantı noktası (0..1), position: oradan kayma, size: boyut.
        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size) =>
            Place(rt, anchor, new Vector2(0.5f, 0.5f), position, size);

        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        // Üstte, yanlardan esneyen sabit yükseklikte bir şerit.
        public static RectTransform TopStrip(RectTransform rt, float height, float margin, float topOffset)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(margin, -topOffset - height);
            rt.offsetMax = new Vector2(-margin, -topOffset);
            return rt;
        }

        // ---------- Temel parçalar ----------

        public Image Picture(Transform parent, string name, Sprite sprite, Color color)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        // display: başlık yazı tipi (daha kalın). Yazı tipleri zaten kalın; ayrıca kalınlaştırılmaz.
        public Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter, bool display = false)
        {
            var label = CreateRect(name, parent).gameObject.AddComponent<Text>();
            label.font = display ? Art.DisplayFont : Art.BodyFont;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = FontStyle.Normal;
            label.color = color;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        // Ekranı dolduran kâğıt zemin.
        public Image Background(Transform parent)
        {
            var image = CreateRect("Kâğıt", parent).gameObject.AddComponent<Image>();
            image.sprite = Art.Paper;
            image.type = Image.Type.Tiled;
            image.color = Color.white;
            image.raycastTarget = false;
            Stretch(image.rectTransform);
            return image;
        }

        // Elle çizilmiş kart: sert gölge + renkli yüz + mürekkep kontur. İçerik card.Body'ye eklenir.
        // cornerScale büyüdükçe köşeler küçülür (ince hap şekilleri için).
        public Card Card(Transform parent, string name, Color fill, float depth = ShadowDepth, bool circle = false, float cornerScale = 1f)
        {
            var root = CreateRect(name, parent);

            var shadow = Shape(root, "Gölge", circle ? Art.CircleFill : Art.BoxFill, Palette.Shadow, circle, cornerScale);
            shadow.rectTransform.offsetMin = new Vector2(0f, -depth);
            shadow.rectTransform.offsetMax = new Vector2(0f, -depth);

            var body = Stretch(CreateRect("Gövde", root));
            var face = Shape(body, "Yüz", circle ? Art.CircleFill : Art.BoxFill, fill, circle, cornerScale);
            var line = Shape(body, "Çizgi", circle ? Art.CircleLine : Art.BoxLine, Palette.Ink, circle, cornerScale);
            return new Card(root, body, shadow, face, line, depth);
        }

        Image Shape(RectTransform parent, string name, Sprite sprite, Color color, bool circle, float cornerScale)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            if (circle)
            {
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }
            else
            {
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = cornerScale;
            }
            Stretch(image.rectTransform);
            return image;
        }

        // ---------- Düğmeler ----------

        public Button TextButton(Transform parent, string name, string text, Color fill, int fontSize = 56) =>
            TextButton(parent, name, text, fill, out _, fontSize);

        // Renkli kart düğme, ortasında başlık yazısı.
        public Button TextButton(Transform parent, string name, string text, Color fill, out Card card, int fontSize = 56)
        {
            card = Card(parent, name, fill);
            var button = MakeButton(card);
            var label = Label(card.Body, "Yazı", text, fontSize, Palette.Ink, TextAnchor.MiddleCenter, display: true);
            Stretch(label.rectTransform, 20f, 6f, 20f, 6f);
            return button;
        }

        public Button IconButton(Transform parent, string name, Sprite icon, Color fill, Color iconColor, float iconScale = 0.55f) =>
            IconButton(parent, name, icon, fill, iconColor, out _, iconScale);

        // Yuvarlak kart düğme (içine istenen şey card.Body'ye eklenir).
        public Button CircleButton(Transform parent, string name, Color fill, out Card card)
        {
            card = Card(parent, name, fill, ShadowDepth * 0.8f, circle: true);
            return MakeButton(card);
        }

        // Yuvarlak kart düğme, ortasında ikon.
        public Button IconButton(Transform parent, string name, Sprite icon, Color fill, Color iconColor, out Card card, float iconScale = 0.55f)
        {
            var button = CircleButton(parent, name, fill, out card);
            var image = Picture(card.Body, "İkon", icon, iconColor);
            var rt = image.rectTransform;
            rt.anchorMin = new Vector2(0.5f - iconScale * 0.5f, 0.5f - iconScale * 0.5f);
            rt.anchorMax = new Vector2(0.5f + iconScale * 0.5f, 0.5f + iconScale * 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return button;
        }

        static Button MakeButton(Card card)
        {
            card.Face.raycastTarget = true;
            var button = card.Root.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.targetGraphic = card.Face;
            card.Root.gameObject.AddComponent<ButtonFeedback>().Init(button, card.Body, card.ShadowDepth);
            return button;
        }

        // ---------- Hazır parçalar ----------

        // Geri düğmeli, başlıklı üst bar (harita, ayarlar, koleksiyon ekranları için).
        public RectTransform TopBar(Transform parent, string title, out Button back)
        {
            var bar = TopStrip(CreateRect("Üst bar", parent), TopBarHeight, 32f, 24f);

            back = IconButton(bar, "Geri", Art.IconBack, Palette.Cream, Palette.Ink, 0.5f);
            Place((RectTransform)back.transform, new Vector2(0f, 0.5f), new Vector2(58f, 4f), new Vector2(112f, 112f));

            var underline = Picture(bar, "Alt çizgi", Art.Underline, Palette.WithAlpha(Palette.Mustard, 0.85f));
            underline.preserveAspect = false;
            Place(underline.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -26f), new Vector2(330f, 44f));

            var label = Label(bar, "Başlık", title, 70, Palette.Ink, TextAnchor.MiddleCenter, display: true);
            Stretch(label.rectTransform, 150f, 0f, 150f, 0f);
            return bar;
        }

        // Arka plana serpiştirilen soluk süs (pati, kemik, yıldız...).
        public Image Doodle(Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 position, float size, float degrees)
        {
            var image = Picture(parent, "Süs", sprite, color);
            Place(image.rectTransform, anchor, position, new Vector2(size, size));
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, degrees);
            return image;
        }
    }
}
