namespace UyuyanKopekler.Core
{
    // Biten bir turun özeti. Modlar puanı, seriyi ve kilitleri bundan hesaplar.
    public readonly struct RoundOutcome
    {
        public readonly bool Won;
        public readonly LossReason LossReason;
        public readonly int RevealedSafeCells;
        public readonly int HeartsLost;
        public readonly int MaxHearts;
        public readonly int Moves;

        public RoundOutcome(bool won, LossReason lossReason, int revealedSafeCells, int heartsLost, int maxHearts, int moves)
        {
            Won = won;
            LossReason = lossReason;
            RevealedSafeCells = revealedSafeCells;
            HeartsLost = heartsLost;
            MaxHearts = maxHearts;
            Moves = moves;
        }
    }
}
