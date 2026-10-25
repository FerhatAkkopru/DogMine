using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    sealed class FakeRoundState : IRoundState
    {
        public bool IsBoardCleared { get; set; }
        public int SafeCellsLeft { get; set; }
        public int RevealedSafeCells { get; set; }
        public int HeartsLost { get; set; }
        public int Moves { get; set; }
        public int Collected(ItemType type) => 0;
    }
}
