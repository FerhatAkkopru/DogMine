namespace UyuyanKopekler.Core
{
    // Bir kare açılınca altından çıkan eşya.
    public readonly struct FoundItem
    {
        public readonly Cell Cell;
        public readonly ItemType Type;

        public FoundItem(Cell cell, ItemType type)
        {
            Cell = cell;
            Type = type;
        }
    }
}
