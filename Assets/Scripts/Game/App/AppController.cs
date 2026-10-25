using UyuyanKopekler.Anim;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Ekranlar arası akış: menü, harita, oyun, ayarlar, koleksiyon ve pencereler.
    // Ekranlar yalnızca "şuna basıldı" der; ne olacağına burası karar verir.
    public sealed class AppController
    {
        readonly UiRoot ui;
        readonly ClipLibrary clips;
        readonly RoundController round;
        readonly ClassicMode classic;
        readonly AdventureMode adventure;
        readonly PlayerProgress progress;
        readonly Inventory inventory;
        readonly GameSettings settings;
        readonly ISaveStore store;
        readonly Haptics haptics;

        IGameMode currentMode;

        public AppController(UiRoot ui, RoundController round, ClassicMode classic, AdventureMode adventure,
            PlayerProgress progress, Inventory inventory, GameSettings settings, ISaveStore store, Haptics haptics,
            BackButton backButton)
        {
            this.ui = ui;
            clips = ui.Factory.Clips;
            this.round = round;
            this.classic = classic;
            this.adventure = adventure;
            this.progress = progress;
            this.inventory = inventory;
            this.settings = settings;
            this.store = store;
            this.haptics = haptics;

            ui.Menu.ClassicPressed += StartClassic;
            ui.Menu.AdventurePressed += ShowMap;
            ui.Menu.CollectionPressed += ShowCollection;
            ui.Menu.SettingsPressed += ShowSettings;

            ui.Map.BackPressed += ShowMenu;
            ui.Map.LevelSelected += OpenLevel;

            ui.Game.PausePressed += OpenPause;

            ui.Settings.BackPressed += ShowMenu;
            ui.Settings.VibrationChanged += OnVibrationChanged;
            ui.Settings.SoundChanged += OnSoundChanged;
            ui.Settings.PawHintsChanged += OnPawHintsChanged;
            ui.Settings.ResetPressed += ConfirmReset;

            ui.Collection.BackPressed += ShowMenu;

            round.Finished += OnRoundFinished;
            backButton.Pressed += OnBack;
        }

        public void Start() => ShowMenu();

        public void SaveAll()
        {
            SaveProgress();
            SaveSettings();
        }

        // ---------- Ekranlar ----------

        void ShowMenu()
        {
            string classicInfo = progress.ClassicBestScore > 0
                ? $"En iyi: {progress.ClassicBestScore}  ·  Bahçe {progress.ClassicBestGarden}"
                : "Sonsuz bahçeler";
            string adventureInfo = adventure.CompletedCount >= adventure.LevelCount
                ? "Bütün bölümler bitti!"
                : $"Bölüm {adventure.HighestUnlocked} / {adventure.LevelCount}";
            ui.Menu.SetInfo(classicInfo, adventureInfo);
            ShowScreen(ui.Menu);
        }

        void ShowMap()
        {
            ui.Map.Refresh(adventure.GetLevelInfos(), adventure.CompletedCount);
            ShowScreen(ui.Map);
        }

        void ShowSettings()
        {
            ui.Settings.Bind(settings);
            ShowScreen(ui.Settings);
        }

        void ShowCollection()
        {
            ui.Collection.SetCounts(inventory);
            ShowScreen(ui.Collection);
        }

        void ShowScreen(UiScreen screen)
        {
            round.Hide();
            ui.Show(screen);
        }

        // ---------- Oyun ----------

        void StartClassic()
        {
            classic.StartRun();
            Play(classic);
        }

        void OpenLevel(int number)
        {
            if (!adventure.IsUnlocked(number)) return;
            var level = adventure.GetLevel(number);
            ui.Dialog.Show(WithDog(number, Clips.SitWag, new DialogContent
            {
                Title = $"Bölüm {number}",
                Body = $"Hedef: {level.Goal.Title}\n{level.Board.Columns}×{level.Board.Rows} bahçe  ·  {level.Board.Dogs} köpek  ·  {level.Hearts} can",
                PrimaryLabel = "Başla",
                PrimaryColor = Palette.Coral,
                OnPrimary = () =>
                {
                    adventure.SelectLevel(number);
                    Play(adventure);
                },
                SecondaryLabel = "Vazgeç"
            }));
        }

        void Play(IGameMode mode)
        {
            currentMode = mode;
            ui.Show(ui.Game);
            round.Start(mode);
        }

        void OpenPause()
        {
            if (!round.HasActiveRound) return;
            round.Pause();
            string warning = currentMode == classic
                ? "Çıkarsan bu koşu biter; puanın rekora sayılır."
                : "Çıkarsan bu bölüme baştan başlarsın.";
            ui.Dialog.Show(WithDog(3, Clips.Sleep, new DialogContent
            {
                Title = "Mola",
                Body = $"Köpekler hâlâ uyuyor.\n{warning}",
                PrimaryLabel = "Devam et",
                OnPrimary = round.Resume,
                SecondaryLabel = "Çık",
                OnSecondary = ExitRound,
                OnBack = round.Resume
            }));
        }

        void ExitRound()
        {
            round.Abandon();
            SaveProgress();
            Handle(currentMode.ExitAction);
        }

        void OnRoundFinished(RoundReport report)
        {
            SaveProgress();
            var dog = report.IsVictory ? Clips.Happy : Clips.Bark;
            ui.Dialog.Show(WithDog(report.IsVictory ? 0 : 2, dog, new DialogContent
            {
                Title = report.Title,
                Body = report.Body,
                PrimaryLabel = report.PrimaryLabel,
                PrimaryColor = report.IsVictory ? Palette.Mint : Palette.Coral,
                OnPrimary = () => Handle(report.PrimaryAction),
                SecondaryLabel = report.SecondaryLabel,
                OnSecondary = () => Handle(report.SecondaryAction)
            }));
        }

        void Handle(RoundAction action)
        {
            switch (action)
            {
                case RoundAction.PlayCurrent: Play(currentMode); break;
                case RoundAction.ExitToMap: ShowMap(); break;
                default: ShowMenu(); break;
            }
        }

        // ---------- Ayarlar ----------

        void OnVibrationChanged(bool on)
        {
            settings.Vibration = on;
            SaveSettings();
            if (on) haptics.Buzz();
        }

        void OnSoundChanged(bool on)
        {
            settings.Sound = on;
            SaveSettings();
        }

        void OnPawHintsChanged(bool on)
        {
            settings.PawHints = on; // bir sonraki bahçede geçerli olur
            SaveSettings();
        }

        void ConfirmReset()
        {
            ui.Dialog.Show(WithDog(1, Clips.Gasp, new DialogContent
            {
                Title = "Emin misin?",
                Body = "Rekorların, açtığın bölümler ve topladığın eşyalar silinecek.",
                PrimaryLabel = "Sıfırla",
                PrimaryColor = Palette.Coral,
                OnPrimary = ResetProgress,
                SecondaryLabel = "Vazgeç"
            }));
        }

        void ResetProgress()
        {
            progress.Clear();
            classic.StartRun();
            SaveProgress();
        }

        // ---------- Geri tuşu ----------

        void OnBack()
        {
            if (ui.Dialog.IsOpen)
            {
                ui.Dialog.Back();
                return;
            }
            if (ui.Current == ui.Game)
            {
                OpenPause();
                return;
            }
            if (ui.Current != ui.Menu) ShowMenu();
        }

        // Pencerenin üstünde oynayan köpek (kuyruk sallar, havlar, uyur...).
        DialogContent WithDog(int coat, Clips.Clip<DogFrame> clip, DialogContent content)
        {
            content.IconFrames = clips.Dog(coat, clip);
            content.IconFps = clip.Fps;
            return content;
        }

        void SaveProgress() => store.Save(SaveKeys.Progress, progress);
        void SaveSettings() => store.Save(SaveKeys.Settings, settings);
    }
}
