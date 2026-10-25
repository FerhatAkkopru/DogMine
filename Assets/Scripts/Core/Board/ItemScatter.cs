using System;
using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Bir bahçede çimin altına kaç eşya saklanacağına karar verir.
    public static class ItemScatter
    {
        public const double BonesPerSafeCell = 0.08;
        public const double StarChance = 0.6;
        public const double PearlChance = 0.12;
        public const double GoldCollarChance = 0.03;

        public static IReadOnlyDictionary<ItemType, int> Roll(BoardConfig config, Random random)
        {
            return new Dictionary<ItemType, int>
            {
                [ItemType.Bone] = Math.Max(1, (int)Math.Round(config.SafeCellCount * BonesPerSafeCell)),
                [ItemType.Star] = random.NextDouble() < StarChance ? 1 : 0,
                [ItemType.Pearl] = random.NextDouble() < PearlChance ? 1 : 0,
                [ItemType.GoldCollar] = random.NextDouble() < GoldCollarChance ? 1 : 0,
            };
        }
    }
}
