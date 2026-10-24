using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // Arayüzün kökü: Canvas, EventSystem, bütün ekranlar ve en üstteki diyalog.
    // Aynı anda tek bir ekran görünür.
    public sealed class UiRoot : MonoBehaviour
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        RectTransform screensLayer;

        public UiFactory Factory { get; private set; }
        public MainMenuScreen Menu { get; private set; }
        public AdventureMapScreen Map { get; private set; }
        public GameScreen Game { get; private set; }
        public SettingsScreen Settings { get; private set; }
        public CollectionScreen Collection { get; private set; }
        public DialogPanel Dialog { get; private set; }
        public UiScreen Current { get; private set; }

        public static UiRoot Create(IGameArt art, Transform parent)
        {
            var go = new GameObject("Arayüz") { layer = UiFactory.UiLayer };
            go.transform.SetParent(parent, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // 1080×1920 her zaman sığar

            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem(parent);

            var root = go.AddComponent<UiRoot>();
            root.Factory = new UiFactory(art);
            root.screensLayer = UiFactory.Stretch(UiFactory.CreateRect("Ekranlar", go.transform));

            root.Menu = root.CreateScreen<MainMenuScreen>("Açılış");
            root.Map = root.CreateScreen<AdventureMapScreen>("Macera haritası");
            root.Game = root.CreateScreen<GameScreen>("Oyun");
            root.Settings = root.CreateScreen<SettingsScreen>("Ayarlar");
            root.Collection = root.CreateScreen<CollectionScreen>("Koleksiyon");

            // Diyalog her zaman ekranların üstünde.
            root.Dialog = UiFactory.CreateRect("Diyalog", go.transform).gameObject.AddComponent<DialogPanel>();
            root.Dialog.Build(root.Factory);
            return root;
        }

        public void Show(UiScreen screen)
        {
            if (Current == screen) return;
            if (Current != null) Current.Hide();
            Current = screen;
            screen.Show();
        }

        T CreateScreen<T>(string name) where T : UiScreen
        {
            var screen = UiFactory.CreateRect(name, screensLayer).gameObject.AddComponent<T>();
            screen.Build(Factory);
            return screen;
        }

        // Dokunmaların arayüze ulaşması için yeni Input System ile çalışan bir EventSystem gerekir.
        static void EnsureEventSystem(Transform parent)
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.transform.SetParent(parent, false);
            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
}
