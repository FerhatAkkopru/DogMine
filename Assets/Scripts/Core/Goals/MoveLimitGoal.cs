using System;

namespace UyuyanKopekler.Core
{
    // Bahçeyi en fazla N hamlede bitir.
    public sealed class MoveLimitGoal : IRoundGoal
    {
        public int Limit { get; }

        public MoveLimitGoal(int limit)
        {
            Limit = Math.Max(1, limit);
        }

        public string Title => $"{Limit} hamlede bitir";
        public string FailureText => "Hamleler bitti.";

        public string Progress(IRoundState state) => $"Hamle: {state.Moves}/{Limit}";

        public GoalStatus Evaluate(IRoundState state)
        {
            if (state.IsBoardCleared) return GoalStatus.Completed;
            return state.Moves >= Limit ? GoalStatus.Failed : GoalStatus.InProgress;
        }
    }
}
