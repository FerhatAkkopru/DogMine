using System;

namespace UyuyanKopekler.Core
{
    // Macera modunda tek bir bölüm.
    public sealed class LevelDefinition
    {
        public int Number { get; }
        public BoardConfig Board { get; }
        public int Hearts { get; }
        public IRoundGoal Goal { get; }

        public LevelDefinition(int number, BoardConfig board, int hearts, IRoundGoal goal)
        {
            Number = number;
            Board = board;
            Hearts = Math.Max(1, hearts);
            Goal = goal ?? throw new ArgumentNullException(nameof(goal));
        }
    }
}
