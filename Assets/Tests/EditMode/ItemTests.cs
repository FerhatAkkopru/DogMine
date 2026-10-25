using System;
using System.Linq;
using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class ItemTests
    {
        [Test]
        public void RandomBoards_HideItemsOnlyUnderSafeCells()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var board = new BoardModel(new BoardConfig(8, 10, 12), new Random(seed));
                board.Reveal(new Cell(4, 5));

                foreach (var c in board.AllCells())
                    if (board.GetItem(c) != ItemType.None) Assert.IsFalse(board.HasDog(c), $"seed {seed}");
            }
        }

        [Test]
        public void Scatter_AlwaysHasSomeBones()
        {
            var counts = ItemScatter.Roll(new BoardConfig(8, 10, 12), new Random(1));
            Assert.GreaterOrEqual(counts[ItemType.Bone], 1);
        }

        [Test]
        public void RevealingAnItem_ReportsItOnce()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.PlaceItem(new Cell(0, 0), ItemType.Bone);
            board.PlaceItem(new Cell(1, 2), ItemType.Star);

            var first = board.Reveal(new Cell(0, 0));

            Assert.AreEqual(2, first.Items.Count);
            Assert.AreEqual(ItemType.None, board.GetItem(new Cell(0, 0)));
            Assert.AreEqual(0, board.Reveal(new Cell(0, 0)).Items.Count);
        }

        [Test]
        public void PlaceItem_OnDog_Throws()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));

            Assert.Throws<ArgumentException>(() => board.PlaceItem(new Cell(3, 3), ItemType.Bone));
        }

        [Test]
        public void Session_CountsCollectedItems()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.PlaceItem(new Cell(0, 0), ItemType.Bone);
            board.PlaceItem(new Cell(0, 1), ItemType.Bone);
            board.PlaceItem(new Cell(1, 0), ItemType.Pearl);
            var session = TestBoards.Session(board);

            var move = session.Dig(new Cell(0, 0));

            Assert.AreEqual(3, move.Reveal.Items.Count);
            Assert.AreEqual(2, session.Collected(ItemType.Bone));
            Assert.AreEqual(1, session.Collected(ItemType.Pearl));
        }
    }
}
