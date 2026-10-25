using UnityEngine;
using UyuyanKopekler.Anim;
using UyuyanKopekler.Art;
using UyuyanKopekler.Audio;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Sahnedeki tek bileşen. Oyunun bütün parçalarını kurar ve birbirine bağlar
    // (composition root). Başka hiçbir sınıf parçaları kendisi aramaz ya da kurmaz.
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Tooltip("0 = her oyunda farklı bahçe. Başka bir sayı = hep aynı bahçeler (test için).")]
        [SerializeField] int randomSeed = 0;

        [Tooltip("Mama kabı koymak için parmağın kaç saniye basılı kalması gerektiği.")]
        [SerializeField, Range(0.2f, 1f)] float longPressSeconds = 0.35f;

        AppController app;

        void Awake()
        {
            Application.targetFrameRate = 60; // mobilde varsayılan 30

            IGameArt art = new IllustratedArt();
            ISaveStore store = new PlayerPrefsSaveStore();
            var progress = store.Load<PlayerProgress>(SaveKeys.Progress);
            var settings = store.Load<GameSettings>(SaveKeys.Settings);
            var inventory = new Inventory(progress);
            var clips = new ClipLibrary(art);

            var sound = gameObject.AddComponent<SoundService>();
            sound.Init(settings);

            var mainCamera = SetUpCamera();
            var boardView = BoardView.Create(clips, sound, transform);
            boardView.SetVisible(false);
            var boardInput = boardView.gameObject.AddComponent<BoardInput>();
            boardInput.Init(boardView, mainCamera, longPressSeconds);

            var ui = UiRoot.Create(clips, sound, transform);
            var boardCamera = mainCamera.gameObject.AddComponent<BoardCamera>();
            boardCamera.Init(() => ui.Game.BoardInsetsPixels, boardView.Backdrop);

            var haptics = new Haptics(settings);
            var random = randomSeed == 0 ? new System.Random() : new System.Random(randomSeed);
            var round = new RoundController(boardView, boardInput, boardCamera, mainCamera, ui.Game,
                inventory, settings, sound, haptics, random, this);

            var classic = new ClassicMode(progress);
            var adventure = new AdventureMode(LevelCatalog.CreateDefault(), progress);
            var backButton = gameObject.AddComponent<BackButton>();

            app = new AppController(ui, round, classic, adventure, progress, inventory, settings, store, haptics, backButton);
            app.Start();
        }

        // Uygulama arka plana atılınca (telefon kilitlenince) kaydet.
        void OnApplicationPause(bool paused)
        {
            if (paused) app?.SaveAll();
        }

        void OnApplicationQuit() => app?.SaveAll();

        static Camera SetUpCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = new GameObject("Main Camera").AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
            }
            mainCamera.orthographic = true;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Palette.Paper;
            return mainCamera;
        }
    }
}
