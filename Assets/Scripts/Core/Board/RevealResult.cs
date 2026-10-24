using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Tek bir kazma ya da çevre açma hamlesinin sonucu.
    public sealed class RevealResult
    {
        public static readonly RevealResult Empty = new RevealResult(new List<RevealedCell>(), new List<Cell>());

        public IReadOnlyList<RevealedCell> Revealed { get; }
        public IReadOnlyList<Cell> WokenDogs { get; }

        public RevealResult(IReadOnlyList<RevealedCell> revealed, IReadOnlyList<Cell> wokenDogs)
        {
            Revealed = revealed;
            WokenDogs = wokenDogs;
        }

        public bool IsEmpty => Revealed.Count == 0 && WokenDogs.Count == 0;
    }
}
