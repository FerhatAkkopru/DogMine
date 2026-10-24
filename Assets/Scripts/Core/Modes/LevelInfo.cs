namespace UyuyanKopekler.Core
{
    // Harita ekranının bir bölüm düğmesini çizmek için ihtiyaç duyduğu bilgiler.
    public readonly struct LevelInfo
    {
        public readonly int Number;
        public readonly string GoalTitle;
        public readonly LevelState State;

        public LevelInfo(int number, string goalTitle, LevelState state)
        {
            Number = number;
            GoalTitle = goalTitle;
            State = state;
        }
    }
}
