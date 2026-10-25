using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Bir dokunuşun sonucu: görünüm bunu kullanarak animasyon oynatır.
    public sealed class MoveResult
    {
        static readonly IReadOnlyList<Cell> NoCells = new List<Cell>();
        public static readonly MoveResult None = new MoveResult(MoveKind.None, RevealResult.Empty, NoCells, NoCells);

        public MoveKind Kind { get; }
        public RevealResult Reveal { get; }

        // Tur kazanıldığında kalan köpeklerin üstüne otomatik konan mama kapları.
        public IReadOnlyList<Cell> AutoFlagged { get; }

        // Kedinin ortaya çıkardığı köpekler (yalnızca Kind == Cat iken dolu).
        public IReadOnlyList<Cell> CatFoundDogs { get; }

        public MoveResult(MoveKind kind, RevealResult reveal, IReadOnlyList<Cell> autoFlagged, IReadOnlyList<Cell> catFoundDogs = null)
        {
            Kind = kind;
            Reveal = reveal;
            AutoFlagged = autoFlagged ?? NoCells;
            CatFoundDogs = catFoundDogs ?? NoCells;
        }

        public bool HasEffect => Kind != MoveKind.None;
        public bool WokeDog => Reveal.WokenDogs.Count > 0;
    }
}
