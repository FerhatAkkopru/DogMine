using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.UI
{
    // Macera haritası: kâğıt üstünde aşağıdan yukarı kıvrılan noktalı bir patika ve bölüm düğmeleri.
    // Bitenler nane yeşili + tik, sıradaki mercan ve yanında uyuyan bir köpek, kilitliler soluk + kilit.
    public sealed class AdventureMapScreen : UiScreen
    {
        const float NodeSize = 160f;
        const float NodeSpacing = 270f;
        const float PathAmplitude = 240f;
        const float BottomPadding = 200f;
        const float TopPadding = 240f;
        const int DotsBetweenNodes = 6;

        ScrollRect scroll;
        RectTransform content;
        Text progress;
        RectTransform guide;
        readonly List<Node> nodes = new List<Node>();
        int focusIndex;

        public event Action BackPressed;
        public event Action<int> LevelSelected;

        public void Refresh(IReadOnlyList<LevelInfo> levels, int completed)
        {
            if (nodes.Count != levels.Count) BuildNodes(levels.Count);

            progress.text = $"{completed}/{levels.Count}";
            focusIndex = levels.Count - 1;
            for (int i = 0; i < levels.Count; i++)
            {
                nodes[i].Show(levels[i], Ui.Art);
                if (levels[i].State == LevelState.Current) focusIndex = i;
            }

            // Sıradaki bölümün yanında kıvrılmış bir köpek uyur.
            var at = NodePosition(focusIndex);
            float side = at.x > 0f ? -1f : 1f;
            UiFactory.Place(guide, new Vector2(0.5f, 0f), at + new Vector2(side * 170f, 10f), new Vector2(170f, 170f));
            guide.localScale = new Vector3(side > 0f ? -1f : 1f, 1f, 1f);
        }

        protected override void BuildBackground(RectTransform root) => Ui.Background(root);

        protected override void BuildContent(RectTransform safe)
        {
            var bar = Ui.TopBar(safe, "Macera", out var back);
            back.onClick.AddListener(() => BackPressed?.Invoke());

            var chip = Ui.Card(bar, "İlerleme", Palette.Mint, 6f, circle: false, cornerScale: 1.5f);
            UiFactory.Place(chip.Root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 4f), new Vector2(170f, 80f));
            var check = Ui.Picture(chip.Body, "Tik", Ui.Art.IconCheck, Palette.Ink);
            UiFactory.Place(check.rectTransform, new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(46f, 46f));
            progress = Ui.Label(chip.Body, "Sayı", "", 40, Palette.Ink, TextAnchor.MiddleLeft, display: true);
            UiFactory.Place(progress.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(100f, 70f));

            var viewport = UiFactory.Stretch(UiFactory.CreateRect("Görünüm", safe), 0f, 0f, 0f, UiFactory.TopBarHeight + 40f);
            var hitArea = viewport.gameObject.AddComponent<Image>();
            hitArea.color = Color.clear; // kaydırma dokunuşlarını yakalamak için
            viewport.gameObject.AddComponent<RectMask2D>();

            content = UiFactory.CreateRect("İçerik", viewport);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(1f, 0f);
            content.pivot = new Vector2(0.5f, 0f);

            scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;
        }

        protected override void OnShown() => StartCoroutine(ScrollToFocus());

        void BuildNodes(int count)
        {
            foreach (Transform child in content) Destroy(child.gameObject);
            nodes.Clear();
            content.sizeDelta = new Vector2(0f, BottomPadding + (count - 1) * NodeSpacing + TopPadding);

            // Önce noktalı patika, sonra düğmeler (düğmeler üstte kalsın).
            var dot = Palette.WithAlpha(Palette.Ink, 0.45f);
            for (int i = 0; i + 1 < count; i++)
            {
                for (int k = 1; k <= DotsBetweenNodes; k++)
                {
                    float t = k / (DotsBetweenNodes + 1f);
                    var at = PathPoint(i + t);
                    Ui.Doodle(content, Ui.Art.CircleFill, dot, new Vector2(0.5f, 0f), at, 16f, 0f);
                }
            }

            guide = Ui.Picture(content, "Uyuyan köpek", Ui.Art.CurledDog(2), Color.white).rectTransform;

            for (int i = 0; i < count; i++)
            {
                int number = i + 1;
                var node = new Node(Ui, content, NodePosition(i));
                node.Button.onClick.AddListener(() => LevelSelected?.Invoke(number));
                nodes.Add(node);
            }
        }

        static Vector2 NodePosition(int index) => PathPoint(index);

        // Patika: bölümler arasında yumuşak kıvrımlar (index kesirli olabilir).
        static Vector2 PathPoint(float index) =>
            new Vector2(Mathf.Sin(index * 1.15f) * PathAmplitude, BottomPadding + index * NodeSpacing);

        IEnumerator ScrollToFocus()
        {
            yield return null; // yerleşimin hesaplanmasını bekle
            Canvas.ForceUpdateCanvases();
            float viewHeight = scroll.viewport.rect.height;
            float contentHeight = content.rect.height;
            if (contentHeight <= viewHeight)
            {
                scroll.verticalNormalizedPosition = 0f;
                yield break;
            }
            float y = NodePosition(focusIndex).y;
            scroll.verticalNormalizedPosition = Mathf.Clamp01((y - viewHeight * 0.4f) / (contentHeight - viewHeight));
        }

        void Update()
        {
            // Sıradaki bölüm nabız gibi atar, yanındaki köpek nefes alır.
            float t = Time.unscaledTime;
            float pulse = 1f + Mathf.Sin(t * 4f) * 0.05f;
            for (int i = 0; i < nodes.Count; i++)
            {
                float s = i == focusIndex && nodes[i].IsCurrent ? pulse : 1f;
                nodes[i].Root.localScale = new Vector3(s, s, 1f);
            }
            if (guide != null)
            {
                float breath = Mathf.Sin(t * 1.9f) * 0.025f;
                guide.localScale = new Vector3(Mathf.Sign(guide.localScale.x) * (1f + breath * 0.3f), 1f + breath, 1f);
            }
        }

        // Haritadaki tek bir bölüm düğmesi.
        sealed class Node
        {
            public readonly Button Button;
            public readonly RectTransform Root;
            readonly Card card;
            readonly Text number;
            readonly Text goal;
            readonly RectTransform badge;
            readonly Image badgeIcon;

            public bool IsCurrent { get; private set; }

            public Node(UiFactory ui, RectTransform parent, Vector2 position)
            {
                Button = ui.CircleButton(parent, "Bölüm", Palette.Coral, out card);
                Root = (RectTransform)Button.transform;
                UiFactory.Place(Root, new Vector2(0.5f, 0f), position, new Vector2(NodeSize, NodeSize));

                number = ui.Label(card.Body, "Numara", "", 70, Palette.Ink, TextAnchor.MiddleCenter, display: true);
                UiFactory.Stretch(number.rectTransform, 0f, 6f, 0f, 0f);

                // Hedef yazısı: altında kâğıt renginde bir etiket, patika noktaları yazının altında kalsın.
                var tag = ui.Picture(Root, "Hedef etiketi", ui.Art.BoxFill, Palette.Paper);
                tag.type = Image.Type.Sliced;
                tag.pixelsPerUnitMultiplier = 2.5f;
                UiFactory.Place(tag.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), Vector2.zero);
                var fit = tag.gameObject.AddComponent<HorizontalLayoutGroup>();
                fit.padding = new RectOffset(16, 16, 4, 8);
                fit.childControlWidth = true;
                fit.childControlHeight = true;
                var fitter = tag.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                goal = ui.Label(tag.transform, "Hedef", "", 30, Palette.InkSoft);
                goal.horizontalOverflow = HorizontalWrapMode.Overflow;

                var badgeCard = ui.Card(Root, "Rozet", Palette.White, 4f, circle: true);
                badge = badgeCard.Root;
                UiFactory.Place(badge, new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(64f, 64f));
                badgeIcon = ui.Picture(badgeCard.Body, "Rozet ikonu", ui.Art.IconCheck, Palette.Ink);
                UiFactory.Stretch(badgeIcon.rectTransform, 14f, 14f, 14f, 14f);
            }

            public void Show(LevelInfo info, IGameArt art)
            {
                IsCurrent = info.State == LevelState.Current;
                bool locked = info.State == LevelState.Locked;
                number.text = info.Number.ToString();
                goal.text = info.GoalTitle;

                card.SetColor(info.State switch
                {
                    LevelState.Completed => Palette.Mint,
                    LevelState.Current => Palette.Coral,
                    _ => Palette.PaperDark
                });
                Button.interactable = !locked;
                number.color = locked ? Palette.WithAlpha(Palette.Ink, 0.35f) : Palette.Ink;
                goal.color = locked ? Palette.WithAlpha(Palette.InkSoft, 0.6f) : Palette.InkSoft;

                badge.gameObject.SetActive(!IsCurrent);
                badgeIcon.sprite = locked ? art.IconLock : art.IconCheck;
                badgeIcon.color = locked ? Palette.InkSoft : Palette.Ink;
            }
        }
    }
}
