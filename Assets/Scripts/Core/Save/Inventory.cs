using System;

namespace UyuyanKopekler.Core
{
    // Oyuncunun eşya cüzdanı: kemik, yıldız, inci, altın tasma. Değerler PlayerProgress'te tutulur.
    public sealed class Inventory
    {
        readonly PlayerProgress progress;

        public event Action Changed;

        public Inventory(PlayerProgress progress)
        {
            this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public int Count(ItemType type)
        {
            switch (type)
            {
                case ItemType.Bone: return progress.Bones;
                case ItemType.Star: return progress.Stars;
                case ItemType.Pearl: return progress.Pearls;
                case ItemType.GoldCollar: return progress.GoldCollars;
                default: return 0;
            }
        }

        public void Add(ItemType type, int amount)
        {
            if (amount <= 0 || type == ItemType.None) return;
            Set(type, Count(type) + amount);
        }

        // Yeterliyse harcar ve true döner; yetmiyorsa hiçbir şey değişmez.
        public bool TrySpend(ItemType type, int amount)
        {
            if (amount < 0 || Count(type) < amount) return false;
            Set(type, Count(type) - amount);
            return true;
        }

        void Set(ItemType type, int value)
        {
            switch (type)
            {
                case ItemType.Bone: progress.Bones = value; break;
                case ItemType.Star: progress.Stars = value; break;
                case ItemType.Pearl: progress.Pearls = value; break;
                case ItemType.GoldCollar: progress.GoldCollars = value; break;
                default: return;
            }
            Changed?.Invoke();
        }
    }
}
