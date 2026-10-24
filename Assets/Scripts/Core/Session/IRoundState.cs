namespace UyuyanKopekler.Core
{
    // Hedeflerin ve modların bir tur hakkında okuyabildiği bilgiler (salt okunur).
    public interface IRoundState
    {
        bool IsBoardCleared { get; }
        int SafeCellsLeft { get; }
        int RevealedSafeCells { get; }
        int HeartsLost { get; }
        int Moves { get; }
    }
}
