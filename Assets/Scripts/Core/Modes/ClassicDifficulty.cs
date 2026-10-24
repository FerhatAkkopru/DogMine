using System;

namespace UyuyanKopekler.Core
{
    // Klasik modda bahçeler sırayla büyür ve köpekler sıklaşır.
    public static class ClassicDifficulty
    {
        public const int StartColumns = 8;
        public const int StartRows = 10;
        public const int MaxColumns = 10;
        public const int MaxRows = 14;
        public const double StartDensity = 0.15;
        public const double DensityStep = 0.01;
        public const double MaxDensity = 0.22;

        public static BoardConfig ForGarden(int garden)
        {
            int n = Math.Max(1, garden) - 1;
            int columns = Math.Min(StartColumns + n / 3, MaxColumns);
            int rows = Math.Min(StartRows + n / 2, MaxRows);
            double density = Math.Min(StartDensity + DensityStep * n, MaxDensity);
            int dogs = (int)Math.Round(columns * rows * density, MidpointRounding.AwayFromZero);
            return new BoardConfig(columns, rows, dogs);
        }
    }
}
