using System;

namespace UyuyanKopekler.Core
{
    // Bir bahçenin boyutu ve köpek sayısı. Geçersiz değerler güvenli aralığa kısılır,
    // böylece Inspector'a yanlış sayı girilse bile oyun her zaman bitirilebilir kalır.
    public readonly struct BoardConfig
    {
        public const int MinSide = 4;
        public const int MaxSide = 20;
        public const int SafeZoneSize = 9; // ilk dokunulan kare ve 8 komşusu köpeksiz

        public readonly int Columns;
        public readonly int Rows;
        public readonly int Dogs;

        public BoardConfig(int columns, int rows, int dogs)
        {
            Columns = Math.Clamp(columns, MinSide, MaxSide);
            Rows = Math.Clamp(rows, MinSide, MaxSide);
            Dogs = Math.Clamp(dogs, 1, Columns * Rows - SafeZoneSize);
        }

        public int CellCount => Columns * Rows;
        public int SafeCellCount => CellCount - Dogs;

        public override string ToString() => $"{Columns}×{Rows}, {Dogs} köpek";
    }
}
