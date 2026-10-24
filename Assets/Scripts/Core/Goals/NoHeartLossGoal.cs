namespace UyuyanKopekler.Core
{
    // Bahçeyi hiçbir köpeği uyandırmadan bitir.
    public sealed class NoHeartLossGoal : IRoundGoal
    {
        public string Title => "Hiç can kaybetmeden bitir";
        public string FailureText => "Bir köpek uyandı.";

        public string Progress(IRoundState state) => "Hiçbir köpeği uyandırma!";

        public GoalStatus Evaluate(IRoundState state)
        {
            if (state.HeartsLost > 0) return GoalStatus.Failed;
            return state.IsBoardCleared ? GoalStatus.Completed : GoalStatus.InProgress;
        }
    }
}
