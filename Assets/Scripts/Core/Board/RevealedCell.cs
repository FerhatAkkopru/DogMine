namespace UyuyanKopekler.Core
{
    // Açılan bir kare ve dokunulan yere kaç adım uzakta açıldığı (dalga animasyonu ve kombo için).
    public readonly struct RevealedCell
    {
        public readonly Cell Cell;
        public readonly int Depth;

        public RevealedCell(Cell cell, int depth)
        {
            Cell = cell;
            Depth = depth;
        }
    }
}
