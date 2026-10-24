using System;
using UnityEngine;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Tek bir turun akışı: dokunuşları oturuma iletir, sonucu görünüme ve arayüze yansıtır,
    // tur bitince modun sonuç raporunu yayınlar. Hangi modun oynandığını bilmez.
    public sealed class RoundController
    {
        const float WinDelaySeconds = 1.2f;
        const float LossDelaySeconds = 1.7f;
        const float HintSeconds = 2.5f;
        const string BowlHint = "Bir mama kabı boş bir karede. Birini kaldırıp kaz.";
        const string FlagBeforeDigHint = "Önce bir kare kaz; mama kabını sonra koyarsın.";

        enum HintKind { None, Intro, Bowl, Message }

        readonly BoardView view;
        readonly BoardInput input;
        readonly BoardCamera boardCamera;
        readonly GameScreen hud;
        readonly Haptics haptics;
        readonly System.Random random;
        readonly MonoBehaviour coroutineHost;

        IGameMode mode;
        GameSession session;
        Cell lastCell;
        Coroutine finishing;
        HintKind hint;

        public event Action<RoundReport> Finished;

        public bool HasActiveRound => session != null && !session.IsOver;

        public RoundController(BoardView view, BoardInput input, BoardCamera boardCamera, GameScreen hud,
            Haptics haptics, System.Random random, MonoBehaviour coroutineHost)
        {
            this.view = view;
            this.input = input;
            this.boardCamera = boardCamera;
            this.hud = hud;
            this.haptics = haptics;
            this.random = random;
            this.coroutineHost = coroutineHost;

            input.enabled = false;
            input.Tapped += OnTapped;
            input.LongPressed += OnLongPressed;
            input.PressChanged += view.SetPressed;
        }

        public void Start(IGameMode gameMode)
        {
            StopFinishing();
            mode = gameMode;
            var setup = mode.CreateRound();
            session = new GameSession(new BoardModel(setup.Board, random), setup.Hearts, setup.Goal);

            view.SetVisible(true);
            view.Build(session.Board);
            boardCamera.SetContent(view.WorldSize);

            hud.SetTitle(setup.Title);
            hud.ResetHearts();
            RefreshHud();
            if (string.IsNullOrEmpty(setup.IntroHint)) HideHint();
            else ShowHint(setup.IntroHint, 0f, HintKind.Intro);

            input.enabled = true;
        }

        public void Pause() => input.enabled = false;
        public void Resume() => input.enabled = HasActiveRound;

        // Oyuncu tur ortasında çıkarsa.
        public void Abandon()
        {
            StopFinishing();
            input.enabled = false;
            HideHint();
            if (session != null && !session.IsOver) mode.Abandon(session);
            session = null;
        }

        public void Hide()
        {
            input.enabled = false;
            view.SetVisible(false);
        }

        // ---------- Girdi ----------

        void OnTapped(Cell cell)
        {
            if (!HasActiveRound) return;
            var move = session.Dig(cell);
            if (!move.HasEffect) return;

            lastCell = cell;
            view.PlayMove(move);
            RefreshHud();

            if (move.WokeDog)
            {
                haptics.Buzz();
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

        // ---------- Arayüz ----------

        void RefreshHud()
        {
            hud.SetHearts(session.Hearts, session.MaxHearts);
            hud.SetDogsLeft(session.Board.DogsLeft);
            hud.SetStatus(mode.GetStatusLine(session));
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
            bool won = session.Status == RoundStatus.Won;
            if (won) view.PlayWin(lastCell);
            else view.PlayLoss(lastCell);

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
