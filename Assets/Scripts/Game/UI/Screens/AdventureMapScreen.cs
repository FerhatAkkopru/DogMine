using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.UI
{
    // Macera haritası: aşağıdan yukarı kıvrılan bir patika üzerinde bölüm düğmeleri.
    // Bitenler yeşil + tik, sıradaki turuncu ve nabız gibi atar, kilitliler gri + kilit.
    public sealed class AdventureMapScreen : UiScreen
    {
        const float NodeSize = 170f;
        const float NodeSpacing = 250f;
        const float PathAmplitude = 250f;
        const float BottomPadding = 190f;
        const float TopPadding = 230f;
        const int DotsBetweenNodes = 4;

        ScrollRect scroll;
        RectTransform content;
        Text progress;
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
        }

        protected override void BuildBackground(RectTransform root) => Ui.Background(root, Ui.Art.MapBackground);

        protected override void BuildContent(RectTransform safe)
        {
            var bar = Ui.TopBar(safe, "Macera", out var back);
            back.onClick.AddListener(() => BackPressed?.Invoke());

            var check = Ui.Picture(bar, "Tik", Ui.Art.IconCheck, Palette.Green);
            UiFactory.Place(check.rectTransform, new Vector2(1f, 0.5f), new Vector2(-190f, 0f), new Vector2(54f, 54f));
            progress = Ui.Label(bar, "İlerleme", "", 44, Palette.InkSoft, TextAnchor.MiddleLeft);
            UiFactory.Place(progress.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(140f, 80f));

            var viewport = UiFactory.Stretch(UiFactory.CreateRect("Görünüm", safe), 0f, 0f, 0f, UiFactory.TopBarHeight + 30f);
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

            // Önce patika noktaları, sonra düğmeler (düğmeler üstte kalsın).
            for (int i = 0; i + 1 < count; i++)
            {
                for (int k = 1; k <= DotsBetweenNodes; k++)
                {
                    var at = Vector2.Lerp(NodePosition(i), NodePosition(i + 1), k / (DotsBetweenNodes + 1f));
                    var dot = Ui.Picture(content, "Patika", Ui.Art.Circle, Palette.WithAlpha(Palette.Cream, 0.85f));
                    UiFactory.Place(dot.rectTransform, new Vector2(0.5f, 0f), at, new Vector2(24f, 24f));
                }
            }
            for (int i = 0; i < count; i++)
            {
                int number = i + 1;
                var node = new Node(Ui, content, NodePosition(i));
                node.Button.onClick.AddListener(() => LevelSelected?.Invoke(number));
                nodes.Add(node);
            }
        }

        static Vector2 NodePosition(int index) =>
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
            // Sıradaki bölüm nabız gibi atar.
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 4f) * 0.06f;
            for (int i = 0; i < nodes.Count; i++)
            {
                float s = i == focusIndex && nodes[i].IsCurrent ? pulse : 1f;
                nodes[i].Root.localScale = new Vector3(s, s, 1f);
            }
        }

        // Haritadaki tek bir bölüm düğmesi.
        sealed class Node
        {
            public readonly Button Button;
            public readonly RectTransform Root;
            readonly Image circle;
            readonly Text number;
            readonly Text goal;
            readonly Image badge;
            readonly Image badgeIcon;

            public bool IsCurrent { get; private set; }

            public Node(UiFactory ui, RectTransform parent, Vector2 position)
            {
                Button = ui.TextButton(parent, "Bölüm", "", Palette.Orange, 72);
                Root = (RectTransform)Button.transform;
                UiFactory.Place(Root, new Vector2(0.5f, 0f), position, new Vector2(NodeSize, NodeSize));
                circle = Button.image;
                circle.sprite = ui.Art.Circle;
                circle.type = Image.Type.Simple;
                number = Button.GetComponentInChildren<Text>();
                UiFactory.AddOutline(number, new Color(0f, 0f, 0f, 0.25f), 2f);

                goal = ui.Label(Root, "Hedef", "", 30, Color.white);
                UiFactory.AddOutline(goal, Palette.Ink, 3f);
                UiFactory.Place(goal.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(380f, 80f));

                badge = ui.Picture(Root, "Rozet", ui.Art.Circle, Palette.Cream);
                UiFactory.Place(badge.rectTransform, new Vector2(1f, 1f), new Vector2(-14f, -14f), new Vector2(68f, 68f));
                UiFactory.AddShadow(badge, Palette.Shadow, new Vector2(0f, -4f));
                badgeIcon = ui.Picture(badge.transform, "Rozet ikonu", ui.Art.IconCheck, Palette.Green);
                UiFactory.Stretch(badgeIcon.rectTransform, 12f, 12f, 12f, 12f);
            }

            public void Show(LevelInfo info, IGameArt art)
            {
                IsCurrent = info.State == LevelState.Current;
                number.text = info.Number.ToString();
                goal.text = info.GoalTitle;

                var color = info.State switch
                {
                    LevelState.Completed => Palette.Green,
                    LevelState.Current => Palette.Orange,
                    _ => Palette.Grey
                };
                circle.color = color;
                Button.GetComponent<Shadow>().effectColor = Palette.Darker(color, 0.3f);
                Button.interactable = info.State != LevelState.Locked;

                bool locked = info.State == LevelState.Locked;
                number.color = locked ? Palette.WithAlpha(Color.white, 0.7f) : Color.white;
                goal.color = locked ? Palette.WithAlpha(Color.white, 0.75f) : Color.white;

                badge.gameObject.SetActive(info.State != LevelState.Current);
                badgeIcon.sprite = locked ? art.IconLock : art.IconCheck;
                badgeIcon.color = locked ? Palette.Lock : Palette.Green;
            }
        }
    }
}
