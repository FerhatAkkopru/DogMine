namespace UyuyanKopekler.Core
{
    // Varsayılan hedef: bütün güvenli kareleri aç.
    public sealed class FinishGardenGoal : IRoundGoal
    {
        public string Title => "Bahçeyi temizle";
        public string FailureText => "Bahçe bitmeden canlar tükendi.";

        public string Progress(IRoundState state) => $"Kalan kare: {state.SafeCellsLeft}";

        public GoalStatus Evaluate(IRoundState state) =>
            state.IsBoardCleared ? GoalStatus.Completed : GoalStatus.InProgress;
    }
}
