namespace UyuyanKopekler.Core
{
    // Tur bitince sonuç penceresinde gösterilecek her şey. Arayüz bunu olduğu gibi çizer;
    // hangi düğmenin ne yapacağına mod karar verir.
    public sealed class RoundReport
    {
        public bool IsVictory { get; }
        public string Title { get; }
        public string Body { get; }
        public string PrimaryLabel { get; }
        public RoundAction PrimaryAction { get; }
        public string SecondaryLabel { get; }
        public RoundAction SecondaryAction { get; }

        public RoundReport(bool isVictory, string title, string body,
            string primaryLabel, RoundAction primaryAction,
            string secondaryLabel, RoundAction secondaryAction)
        {
            IsVictory = isVictory;
            Title = title;
            Body = body;
            PrimaryLabel = primaryLabel;
            PrimaryAction = primaryAction;
            SecondaryLabel = secondaryLabel;
            SecondaryAction = secondaryAction;
        }
    }
}
