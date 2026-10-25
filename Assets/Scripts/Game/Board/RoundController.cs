using System;
using UnityEngine;
using UyuyanKopekler.Audio;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Tek bir turun akışı: dokunuşları oturuma iletir, sonucu görünüme ve arayüze yansıtır,
    // tur bitince modun sonuç raporunu yayınlar. Hangi modun oynandığını bilmez.
    // Kedi yardımcısı: düğmeye basılınca "kare seç" moduna girer, seçilen çimli kareye kedi gider.
    public sealed class RoundController
    {
        const float WinDelaySeconds = 1.2f;
        const float LossDelaySeconds = 2.2f;
        const float HintSeconds = 2.5f;
        const float ItemDelaySeconds = 0.2f;
        const string BowlHint = "Bir mama kabı boş bir karede. Birini kaldırıp kaz.";
        const string FlagBeforeDigHint = "Önce bir kare kaz; mama kabını sonra koyarsın.";
        const string CatBeforeDigHint = "Önce bir kare kaz; kediyi sonra çağırırsın.";
        const string CatPickHint = "Kediyi göndereceğin çimli kareye dokun.";
        const string CatNeedsGrassHint = "Kedi yalnızca çimli karelere gider.";

        enum HintKind { None, Intro, Bowl, Cat, Message }

        readonly BoardView view;
        readonly BoardInput input;
        readonly BoardCamera boardCamera;
        readonly Camera worldCamera;
        readonly GameScreen hud;
        readonly Inventory inventory;
        readonly GameSettings settings;
        readonly ISoundPlayer sound;
        readonly Haptics haptics;
        readonly System.Random random;
        readonly MonoBehaviour coroutineHost;

        IGameMode mode;
        GameSession session;
        Cell lastCell;
        Coroutine finishing;
        HintKind hint;
        bool catArmed;
        bool catBusy;
        int bonesInFlight;

        public event Action<RoundReport> Finished;

        public bool HasActiveRound => session != null && !session.IsOver;

        public RoundController(BoardView view, BoardInput input, BoardCamera boardCamera, Camera worldCamera, GameScreen hud,
            Inventory inventory, GameSettings settings, ISoundPlayer sound, Haptics haptics, System.Random random,
            MonoBehaviour coroutineHost)
        {
            this.view = view;
            this.input = input;
            this.boardCamera = boardCamera;
            this.worldCamera = worldCamera;
            this.hud = hud;
            this.inventory = inventory;
            this.settings = settings;
            this.sound = sound;
            this.haptics = haptics;
            this.random = random;
            this.coroutineHost = coroutineHost;

            input.enabled = false;
            input.Tapped += OnTapped;
            input.LongPressed += OnLongPressed;
            input.PressChanged += view.SetPressed;
            hud.CatPressed += OnCatPressed;
        }

        public void Start(IGameMode gameMode)
        {
            StopFinishing();
            mode = gameMode;
            var setup = mode.CreateRound();
            session = new GameSession(new BoardModel(setup.Board, random), setup.Hearts, setup.Goal);
            catArmed = false;
            catBusy = false;
            bonesInFlight = 0;

            view.UsePawHints = settings.PawHints;
            view.SetVisible(true);
            view.Build(session.Board);
            boardCamera.SetContent(view.WorldSize);

            hud.SetTitle(setup.Title);
            hud.ResetHearts();
            RefreshHud();
            RefreshBones();
            if (string.IsNullOrEmpty(setup.IntroHint)) HideHint();
            else ShowHint(setup.IntroHint, 0f, HintKind.Intro);

            input.enabled = true;
        }

        public void Pause() => input.enabled = false;
        public void Resume() => input.enabled = HasActiveRound && !catBusy;

        // Oyuncu tur ortasında çıkarsa.
        public void Abandon()
        {
            StopFinishing();
            input.enabled = false;
            catArmed = false;
            catBusy = false;
            HideHint();
            if (session != null && !session.IsOver) mode.Abandon(session);
            session = null;
        }

        public void Hide()
        {
            input.enabled = false;
            catArmed = false;
            view.SetVisible(false);
        }

        // ---------- Girdi ----------

        void OnTapped(Cell cell)
        {
            if (!HasActiveRound) return;
            if (catArmed)
            {
                SendCat(cell);
                return;
            }

            var move = session.Dig(cell);
            if (!move.HasEffect) return;

            lastCell = cell;
            view.PlayMove(move);
            RefreshHud();
            CollectItems(move.Reveal);

            if (move.WokeDog)
            {
                haptics.Buzz();
                sound.Play(Sfx.HeartLost);
                int count = move.Reveal.WokenDogs.Count;
                string who = count == 1 ? "Bir köpek" : $"{count} köpek";
                UpdateHint($"Hav! {who} uyandı. {session.Hearts} can kaldı.");
            }
            else
            {
                UpdateHint(null);
            }

            if (session.IsOver) EndRound();
        }

        void OnLongPressed(Cell cell)
        {
            if (!HasActiveRound) return;
            if (catArmed)
            {
                // Basılı tutmak kedi seçimini iptal eder (mama kabı konmaz).
                SetCatArmed(false);
                return;
            }
            if (!session.Board.DogsPlaced)
            {
                ShowHint(FlagBeforeDigHint, HintSeconds, HintKind.Message);
                return;
            }
            if (!session.ToggleFlag(cell)) return;
            view.PlayFlag(cell);
            RefreshHud();
            UpdateHint(null);
        }

        // ---------- Kedi ----------

        void OnCatPressed()
        {
            if (!HasActiveRound || catBusy) return;
            if (catArmed)
            {
                SetCatArmed(false);
                return;
            }
            if (!session.Board.DogsPlaced)
            {
                ShowHint(CatBeforeDigHint, HintSeconds, HintKind.Message);
                return;
            }
            if (!CatHelper.CanAfford(inventory))
            {
                ShowHint($"Kedi {CatHelper.BonePrice} kemik ister. Kazdıkça çimin altından kemik çıkar.", HintSeconds, HintKind.Message);
                return;
            }
            SetCatArmed(true);
        }

        void SetCatArmed(bool armed)
        {
            catArmed = armed;
            RefreshBones();
            if (armed)
            {
                sound.Play(Sfx.Meow);
                ShowHint(CatPickHint, 0f, HintKind.Cat);
            }
            else if (hint == HintKind.Cat)
            {
                HideHint();
                UpdateHint(null);
            }
        }

        void SendCat(Cell cell)
        {
            if (session.Board.GetState(cell) != CellState.Hidden)
            {
                ShowHint(CatNeedsGrassHint, 0f, HintKind.Cat);
                return;
            }
            if (!CatHelper.TryPay(inventory))
            {
                SetCatArmed(false);
                return;
            }

            var move = session.SendCat(cell);
            if (!move.HasEffect)
            {
                inventory.Add(ItemType.Bone, CatHelper.BonePrice); // olmadı: kemikler geri
                SetCatArmed(false);
                return;
            }

            catArmed = false;
            catBusy = true;
            input.enabled = false;
            lastCell = cell;
            HideHint();
            RefreshBones();

            var catSession = session;
            view.PlayCat(cell, move, () =>
            {
                if (session != catSession) return; // bu arada turdan çıkıldı
                catBusy = false;
                RefreshHud();
                CollectItems(move.Reveal);

                int found = move.CatFoundDogs.Count;
                string message = found == 0 ? "Kedi etrafta köpek bulamadı, kareyi kazdı."
                    : found == 1 ? "Kedi bir köpek buldu! Üstüne mama kabı kondu."
                    : $"Kedi {found} köpek buldu! Üstlerine mama kabı kondu.";
                UpdateHint(message);

                if (session.IsOver) EndRound();
                else input.enabled = true;
            });
        }

        // ---------- Eşyalar ----------

        // Açılan karelerden çıkan eşyalar hemen cüzdana eklenir (kaydedilir); ekranda kareden
        // sayaca uçarlar, kemik sayacı da her kemik varınca bir artar.
        void CollectItems(RevealResult reveal)
        {
            foreach (var item in reveal.Items)
            {
                inventory.Add(item.Type, 1);
                Vector2 screen = worldCamera.WorldToScreenPoint(view.CellWorldPosition(item.Cell));
                bool bone = item.Type == ItemType.Bone;
                if (bone) bonesInFlight++;
                hud.FlyItem(item.Type, screen, ItemDelaySeconds, () =>
                {
                    if (!bone) return;
                    bonesInFlight = Math.Max(0, bonesInFlight - 1);
                    RefreshBones();
                });

                if (item.Type == ItemType.Pearl) ShowHint("İnci buldun! Koleksiyonuna eklendi.", HintSeconds, HintKind.Message);
                else if (item.Type == ItemType.GoldCollar) ShowHint("Altın tasma! Çok nadir bir hazine.", HintSeconds, HintKind.Message);
            }
            RefreshBones();
        }

        // ---------- Arayüz ----------

        void RefreshHud()
        {
            hud.SetHearts(session.Hearts, session.MaxHearts);
            hud.SetDogsLeft(session.Board.DogsLeft);
            hud.SetStatus(mode.GetStatusLine(session));
        }

        void RefreshBones()
        {
            hud.SetBones(Math.Max(0, inventory.Count(ItemType.Bone) - bonesInFlight));
            hud.SetCatState(catArmed, CatHelper.CanAfford(inventory));
        }

        // Öncelik: tur bittiyse ipucu yok; kazılacak güvenli kare kalmadıysa mama kabı uyarısı;
        // sonra verilen mesaj; hiçbiri yoksa açılış/uyarı ipucu kapanır.
        void UpdateHint(string message)
        {
            if (session.IsOver)
            {
                HideHint();
                return;
            }
            if (session.Board.DogsPlaced && !session.Board.HasDiggableSafeCell())
            {
                ShowHint(BowlHint, 0f, HintKind.Bowl);
                return;
            }
            if (message != null)
            {
                ShowHint(message, HintSeconds, HintKind.Message);
                return;
            }
            if (hint == HintKind.Intro || hint == HintKind.Bowl) HideHint();
        }

        void ShowHint(string text, float seconds, HintKind kind)
        {
            hud.ShowHint(text, seconds);
            hint = kind;
        }

        void HideHint()
        {
            hud.HideHint();
            hint = HintKind.None;
        }

        // ---------- Bitiş ----------

        void EndRound()
        {
            input.enabled = false;
            catArmed = false;
            RefreshBones();
            bool won = session.Status == RoundStatus.Won;
            if (won) view.PlayWin(lastCell);
            else view.PlayLoss(lastCell);
            sound.Play(won ? Sfx.Win : Sfx.Lose);

            var finishedSession = session;
            finishing = coroutineHost.StartCoroutine(Tween.Wait(won ? WinDelaySeconds : LossDelaySeconds, () =>
            {
                finishing = null;
                var report = mode.CompleteRound(finishedSession.ToOutcome());
                Finished?.Invoke(report);
            }));
        }

        void StopFinishing()
        {
            if (finishing != null) coroutineHost.StopCoroutine(finishing);
            finishing = null;
        }
    }
}
