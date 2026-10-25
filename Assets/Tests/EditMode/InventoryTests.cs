using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class InventoryTests
    {
        [Test]
        public void NewPlayer_StartsWithBonesForThreeCats()
        {
            var inventory = new Inventory(new PlayerProgress());

            Assert.AreEqual(PlayerProgress.StartingBones, inventory.Count(ItemType.Bone));
            Assert.IsTrue(CatHelper.CanAfford(inventory));
        }

        [Test]
        public void Spending_FailsWhenNotEnough_AndChangesNothing()
        {
            var progress = new PlayerProgress { Bones = 5 };
            var inventory = new Inventory(progress);

            Assert.IsFalse(CatHelper.TryPay(inventory));
            Assert.AreEqual(5, progress.Bones);
        }

        [Test]
        public void AddAndSpend_UpdateProgress_AndNotify()
        {
            var progress = new PlayerProgress { Bones = 0 };
            var inventory = new Inventory(progress);
            int changes = 0;
            inventory.Changed += () => changes++;

            inventory.Add(ItemType.Bone, 12);
            inventory.Add(ItemType.Pearl, 1);
            bool paid = CatHelper.TryPay(inventory);

            Assert.IsTrue(paid);
            Assert.AreEqual(2, progress.Bones);
            Assert.AreEqual(1, progress.Pearls);
            Assert.AreEqual(3, changes);
        }

        [Test]
        public void ResetProgress_RestoresStartingBones()
        {
            var progress = new PlayerProgress { Bones = 99, GoldCollars = 2 };

            progress.Clear();

            Assert.AreEqual(PlayerProgress.StartingBones, progress.Bones);
            Assert.AreEqual(0, progress.GoldCollars);
        }
    }
}
