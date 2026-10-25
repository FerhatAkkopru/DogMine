using System;
using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Tek bir bahçe turu: canlar, hamleler, hedef ve kazanma/kaybetme kararı.
    // Hangi modda oynandığını bilmez; hedef dışarıdan verilir.
    public sealed class GameSession : IRoundState
    {
        readonly Dictionary<ItemType, int> collected = new Dictionary<ItemType, int>();

        public BoardModel Board { get; }
        public IRoundGoal Goal { get; }
        public int MaxHearts { get; }
        public int Hearts { get; private set; }
        public int Moves { get; private set; }
        public RoundStatus Status { get; private set; } = RoundStatus.NotStarted;
        public LossReason LossReason { get; private set; } = LossReason.None;

        public bool IsOver => Status == RoundStatus.Won || Status == RoundStatus.Lost;

        // IRoundState
        public bool IsBoardCleared => Board.IsCleared;
        public int SafeCellsLeft => Board.SafeCellsLeft;
        public int RevealedSafeCells => Board.RevealedSafeCount;
        public int HeartsLost => MaxHearts - Hearts;
        public int Collected(ItemType type) => collected.TryGetValue(type, out int n) ? n : 0;

        public GameSession(BoardModel board, int hearts, IRoundGoal goal)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Goal = goal ?? new FinishGardenGoal();
            MaxHearts = Math.Max(1, hearts);
            Hearts = MaxHearts;
        }

        // Çimli kareye dokunmak kazar; açık rakama dokunmak çevresini açar.
        public MoveResult Dig(Cell cell)
        {
            if (IsOver || !Board.Contains(cell)) return MoveResult.None;

            MoveKind kind;
            RevealResult reveal;
            switch (Board.GetState(cell))
            {
                case CellState.Hidden:
                    kind = MoveKind.Dig;
                    reveal = Board.Reveal(cell);
                    break;
                case CellState.Revealed:
                    kind = MoveKind.Chord;
                    reveal = Board.Chord(cell);
                    break;
                default:
                    return MoveResult.None; // mama kabı olan kare kazılmaz
            }
            if (reveal.IsEmpty) return MoveResult.None;

            Status = RoundStatus.Playing;
            Moves++;
            Hearts = Math.Max(0, Hearts - reveal.WokenDogs.Count);
            Count(reveal);

            Evaluate();
            var autoFlagged = Status == RoundStatus.Won ? Board.FlagRemainingDogs() : null;
            return new MoveResult(kind, reveal, autoFlagged);
        }

        // Kediyi çimli bir kareye gönderir: 3×3 alandaki köpekler ortaya çıkar, can gitmez.
        // Bir hamle sayılır. Kedinin bedeli (kemik) bu sınıfın işi değil.
        public MoveResult SendCat(Cell cell)
        {
            if (IsOver || !Board.Contains(cell) || Board.GetState(cell) != CellState.Hidden) return MoveResult.None;

            var result = Board.SendCat(cell);
            if (result.IsEmpty) return MoveResult.None;

            Status = RoundStatus.Playing;
            Moves++;
            Count(result.Reveal);

            Evaluate();
            var autoFlagged = Status == RoundStatus.Won ? Board.FlagRemainingDogs() : null;
            return new MoveResult(MoveKind.Cat, result.Reveal, autoFlagged, result.FoundDogs);
        }

        void Count(RevealResult reveal)
        {
            foreach (var item in reveal.Items)
                collected[item.Type] = Collected(item.Type) + 1;
        }

        public bool ToggleFlag(Cell cell) => !IsOver && Board.ToggleFlag(cell);

        public RoundOutcome ToOutcome() =>
            new RoundOutcome(Status == RoundStatus.Won, LossReason, RevealedSafeCells, HeartsLost, MaxHearts, Moves);

        void Evaluate()
        {
            if (Hearts <= 0)
            {
                End(RoundStatus.Lost, LossReason.OutOfHearts);
                return;
            }

            switch (Goal.Evaluate(this))
            {
                case GoalStatus.Completed: End(RoundStatus.Won, LossReason.None); break;
                case GoalStatus.Failed: End(RoundStatus.Lost, LossReason.GoalFailed); break;
            }
        }

        void End(RoundStatus status, LossReason reason)
        {
            Status = status;
            LossReason = reason;
        }
    }
}
