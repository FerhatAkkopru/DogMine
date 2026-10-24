using System;

namespace UyuyanKopekler.Core
{
    // Izgaradaki bir karenin konumu. X sütun (soldan), Y satır (alttan).
    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly int X;
        public readonly int Y;

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Chebyshev uzaklığı: 8 yönlü komşulukta kaç adım uzakta.
        public int StepsTo(Cell other) => Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));

        public bool Equals(Cell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Cell other && Equals(other);
        public override int GetHashCode() => (X * 397) ^ Y;
        public override string ToString() => $"({X},{Y})";

        public static bool operator ==(Cell a, Cell b) => a.Equals(b);
        public static bool operator !=(Cell a, Cell b) => !a.Equals(b);
    }
}
