namespace UyuyanKopekler.Core
{
    // Bir modun yeni tur için verdiği bilgiler.
    public sealed class RoundSetup
    {
        public string Title { get; }
        public BoardConfig Board { get; }
        public int Hearts { get; }
        public IRoundGoal Goal { get; }

        // Tur başında ipucu balonunda gösterilecek metin (boş olabilir).
        public string IntroHint { get; }

        public RoundSetup(string title, BoardConfig board, int hearts, IRoundGoal goal, string introHint)
        {
            Title = title;
            Board = board;
            Hearts = hearts;
            Goal = goal;
            IntroHint = introHint;
        }
    }
}
