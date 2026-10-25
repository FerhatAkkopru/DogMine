using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Kedinin bahçeye gönderilmesinin sonucu.
    public sealed class CatResult
    {
        public static readonly CatResult Empty = new CatResult(new List<Cell>(), RevealResult.Empty);

        // Kedinin ortaya çıkardığı (üstüne mama kabı konan) köpekler.
        public IReadOnlyList<Cell> FoundDogs { get; }

        // Kedinin bıraktığı kare güvenliyse kazıldığı için açılan kareler.
        public RevealResult Reveal { get; }

        public CatResult(IReadOnlyList<Cell> foundDogs, RevealResult reveal)
        {
            FoundDogs = foundDogs;
            Reveal = reveal;
        }

        public bool IsEmpty => FoundDogs.Count == 0 && Reveal.IsEmpty;
    }
}
