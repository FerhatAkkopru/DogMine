using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Arayüz parçalarını aynı stille kodla kurar (düğme, panel, yazı, üst bar...).
    // Ölçüler 1080×1920 referans ekrana göredir.
    public sealed class UiFactory
    {
        public const int UiLayer = 5;
        public const float TopBarHeight = 160f;

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

        // ---------- Efektler ----------

        public static void AddShadow(Graphic graphic, Color color, Vector2 distance)
        {
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
        }

        public static void AddOutline(Graphic graphic, Color color, float distance)
        {
            var outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
        }

        // ---------- Parçalar ----------

        public Image Picture(Transform parent, string name, Sprite sprite, Color color)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        // Yuvarlatılmış köşeli düz kutu. cornerScale büyüdükçe köşeler küçülür.
        public Image RoundedBox(Transform parent, string name, Color color, float cornerScale = 1f)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = Art.RoundedRect;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = cornerScale;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        // Gölgeli krem kart.
        public Image Panel(Transform parent, string name, Color color, float cornerScale = 1f)
        {
            var image = RoundedBox(parent, name, color, cornerScale);
            AddShadow(image, Palette.Shadow, new Vector2(0f, -10f));
            return image;
        }

        // Ekranı en-boy oranını bozmadan kaplayan arka plan resmi.
        public Image Background(Transform parent, Sprite sprite)
        {
            var holder = Stretch(CreateRect("Arka plan", parent));
            var image = Picture(holder, "Resim", sprite, Color.white);
            image.preserveAspect = false;
            var fitter = image.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
            return image;
        }

        public Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var label = CreateRect(name, parent).gameObject.AddComponent<Text>();
            label.font = Art.Font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = FontStyle.Bold;
            label.color = color;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        // Renkli, alt kenarı koyu ("şeker") düğme; yazı ortada.
        public Button TextButton(Transform parent, string name, string text, Color color, int fontSize = 60)
        {
            var image = RoundedBox(parent, name, color);
            image.raycastTarget = true;
            AddShadow(image, Palette.Darker(color, 0.3f), new Vector2(0f, -12f));

            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.targetGraphic = image;
            image.gameObject.AddComponent<ButtonFeedback>();

            var label = Label(image.transform, "Yazı", text, fontSize, Color.white);
            Stretch(label.rectTransform, 16f, 8f, 16f, 8f);
            AddShadow(label, new Color(0f, 0f, 0f, 0.22f), new Vector2(0f, -3f));
            return button;
        }

        // Yuvarlak, içinde ikon olan düğme.
        public Button IconButton(Transform parent, string name, Sprite icon, Color color, Color iconColor, float iconScale = 0.6f)
        {
            var image = Picture(parent, name, Art.Circle, color);
            image.raycastTarget = true;
            AddShadow(image, Palette.Darker(color, 0.3f), new Vector2(0f, -8f));

            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.targetGraphic = image;
            image.gameObject.AddComponent<ButtonFeedback>();

            var iconImage = Picture(image.transform, "İkon", icon, iconColor);
            iconImage.rectTransform.anchorMin = new Vector2(0.5f - iconScale * 0.5f, 0.5f - iconScale * 0.5f);
            iconImage.rectTransform.anchorMax = new Vector2(0.5f + iconScale * 0.5f, 0.5f + iconScale * 0.5f);
            iconImage.rectTransform.offsetMin = Vector2.zero;
            iconImage.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        // Geri düğmeli, başlıklı üst bar (harita, ayarlar, koleksiyon ekranları için).
        public RectTransform TopBar(Transform parent, string title, out Button back)
        {
            var bar = Panel(parent, "Üst bar", Palette.Cream).rectTransform;
            TopStrip(bar, TopBarHeight, 24f, 20f);

            back = IconButton(bar, "Geri", Art.IconBack, Palette.Orange, Color.white);
            Place(back.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(90f, 0f), new Vector2(116f, 116f));

            var label = Label(bar, "Başlık", title, 60, Palette.Ink);
            Stretch(label.rectTransform, 180f, 0f, 180f, 0f);
            return bar;
        }
    }
}
