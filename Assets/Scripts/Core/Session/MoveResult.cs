using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Bir dokunuşun sonucu: görünüm bunu kullanarak animasyon oynatır.
    public sealed class MoveResult
    {
        static readonly IReadOnlyList<Cell> NoCells = new List<Cell>();
        public static readonly MoveResult None = new MoveResult(MoveKind.None, RevealResult.Empty, NoCells);

        public MoveKind Kind { get; }
        public RevealResult Reveal { get; }

        // Tur kazanıldığında kalan köpeklerin üstüne otomatik konan mama kapları.
        public IReadOnlyList<Cell> AutoFlagged { get; }

        public MoveResult(MoveKind kind, RevealResult reveal, IReadOnlyList<Cell> autoFlagged)
        {
            Kind = kind;
            Reveal = reveal;
            AutoFlagged = autoFlagged ?? NoCells;
        }

        public bool HasEffect => Kind != MoveKind.None;
        public bool WokeDog => Reveal.WokenDogs.Count > 0;
    }
}
