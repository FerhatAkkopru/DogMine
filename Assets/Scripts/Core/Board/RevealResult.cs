using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Tek bir kazma ya da çevre açma hamlesinin sonucu.
    public sealed class RevealResult
    {
        static readonly IReadOnlyList<FoundItem> NoItems = new List<FoundItem>();
        public static readonly RevealResult Empty = new RevealResult(new List<RevealedCell>(), new List<Cell>(), NoItems);

        public IReadOnlyList<RevealedCell> Revealed { get; }
        public IReadOnlyList<Cell> WokenDogs { get; }

        // Açılan karelerin altından çıkan eşyalar (her eşya yalnızca bir kez gelir).
        public IReadOnlyList<FoundItem> Items { get; }

        public RevealResult(IReadOnlyList<RevealedCell> revealed, IReadOnlyList<Cell> wokenDogs, IReadOnlyList<FoundItem> items = null)
        {
            Revealed = revealed;
            WokenDogs = wokenDogs;
            Items = items ?? NoItems;
        }

        public bool IsEmpty => Revealed.Count == 0 && WokenDogs.Count == 0;
    }
}
