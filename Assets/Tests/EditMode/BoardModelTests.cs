using System;
using System.Linq;
using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class BoardModelTests
    {
        [Test]
        public void Config_ClampsDogsSoBoardIsAlwaysWinnable()
        {
            var config = new BoardConfig(4, 4, 100);
            Assert.AreEqual(16 - BoardConfig.SafeZoneSize, config.Dogs);
        }

        [Test]
        public void Config_ClampsTooSmallSides()
        {
            var config = new BoardConfig(1, 2, 1);
            Assert.AreEqual(BoardConfig.MinSide, config.Columns);
            Assert.AreEqual(BoardConfig.MinSide, config.Rows);
        }

        [Test]
        public void FirstReveal_IsAlwaysSafe_AndKeepsNeighboursDogFree()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var board = new BoardModel(new BoardConfig(8, 10, 20), new Random(seed));
                var tap = new Cell(seed % 8, seed % 10);

                var result = board.Reveal(tap);

                Assert.AreEqual(0, result.WokenDogs.Count, $"seed {seed}");
                Assert.IsFalse(board.HasDog(tap));
                Assert.IsTrue(board.Neighbors(tap).All(n => !board.HasDog(n)), $"seed {seed}");
                Assert.AreEqual(20, board.AllCells().Count(board.HasDog));
            }
        }

        [Test]
        public void DenseBoard_FirstTapInCentre_ClearsTheSafeZone()
        {
            var board = new BoardModel(new BoardConfig(5, 5, 16), new Random(3));

            var result = board.Reveal(new Cell(2, 2));

            Assert.AreEqual(9, result.Revealed.Count);
            Assert.IsTrue(board.IsCleared);
        }

        [Test]
        public void NearbyCounts_MatchFixedLayout()
        {
            var board = TestBoards.Fixed(4, 4, (0, 0), (1, 0));

            Assert.AreEqual(2, board.GetNearbyDogs(new Cell(0, 1)));
            Assert.AreEqual(2, board.GetNearbyDogs(new Cell(1, 1)));
            Assert.AreEqual(1, board.GetNearbyDogs(new Cell(2, 0)));
            Assert.AreEqual(0, board.GetNearbyDogs(new Cell(3, 3)));
        }

        [Test]
        public void Reveal_FloodsEmptyArea_WithIncreasingDepth()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));

            var result = board.Reveal(new Cell(0, 0));

            Assert.AreEqual(15, result.Revealed.Count);
            Assert.IsTrue(board.IsCleared);
            Assert.AreEqual(0, result.Revealed.First(r => r.Cell == new Cell(0, 0)).Depth);
            Assert.AreEqual(2, result.Revealed.First(r => r.Cell == new Cell(2, 2)).Depth);
        }

        [Test]
        public void Reveal_OnNumber_OpensOnlyThatCell()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));

            var result = board.Reveal(new Cell(2, 2));

            Assert.AreEqual(1, result.Revealed.Count);
            Assert.AreEqual(CellState.Hidden, board.GetState(new Cell(0, 0)));
        }

        [Test]
        public void Reveal_Dog_WakesIt()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));

            var result = board.Reveal(new Cell(3, 3));

            Assert.AreEqual(1, result.WokenDogs.Count);
            Assert.AreEqual(1, board.WokenDogCount);
            Assert.AreEqual(CellState.Revealed, board.GetState(new Cell(3, 3)));
        }

        [Test]
        public void Flag_BlocksFlood_AndIsNotDug()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.ToggleFlag(new Cell(1, 1));

            board.Reveal(new Cell(0, 0));

            Assert.AreEqual(CellState.Flagged, board.GetState(new Cell(1, 1)));
            Assert.IsTrue(board.Reveal(new Cell(1, 1)).IsEmpty);
        }

        [Test]
        public void ToggleFlag_BeforeFirstDig_DoesNothing()
        {
            var board = new BoardModel(new BoardConfig(8, 10, 12), new Random(1));

            Assert.IsFalse(board.ToggleFlag(new Cell(0, 0)));
            Assert.AreEqual(0, board.FlagCount);
        }

        [Test]
        public void ToggleFlag_CountsUpAndDown()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            var cell = new Cell(3, 3);

            Assert.IsTrue(board.ToggleFlag(cell));
            Assert.AreEqual(1, board.FlagCount);
            Assert.AreEqual(0, board.DogsLeft);
            Assert.IsTrue(board.ToggleFlag(cell));
            Assert.AreEqual(0, board.FlagCount);
        }

        [Test]
        public void Chord_WithMatchingFlags_OpensNeighbours()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.Reveal(new Cell(2, 2));
            board.ToggleFlag(new Cell(3, 3));

            var result = board.Chord(new Cell(2, 2));

            Assert.AreEqual(0, result.WokenDogs.Count);
            Assert.IsTrue(board.IsCleared);
            Assert.IsTrue(result.Revealed.All(r => r.Depth >= 1));
        }

        [Test]
        public void Chord_WithoutEnoughFlags_DoesNothing()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.Reveal(new Cell(2, 2));

            Assert.IsTrue(board.Chord(new Cell(2, 2)).IsEmpty);
        }

        [Test]
        public void Chord_WithWrongFlag_WakesTheDog()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.Reveal(new Cell(2, 2));
            board.ToggleFlag(new Cell(3, 2));

            var result = board.Chord(new Cell(2, 2));

            CollectionAssert.Contains(result.WokenDogs.ToList(), new Cell(3, 3));
        }

        [Test]
        public void Chord_CountsWokenDogsAsMarked()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.Reveal(new Cell(3, 3));
            board.Reveal(new Cell(2, 2));

            var result = board.Chord(new Cell(2, 2));

            Assert.AreEqual(0, result.WokenDogs.Count);
            Assert.IsTrue(board.IsCleared);
        }

        [Test]
        public void FlagRemainingDogs_BringsDogsLeftToZero()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3), (0, 3));
            board.Reveal(new Cell(0, 0));

            var flagged = board.FlagRemainingDogs();

            Assert.AreEqual(2, flagged.Count);
            Assert.AreEqual(0, board.DogsLeft);
        }

        [Test]
        public void HasDiggableSafeCell_IsFalse_WhenOnlyABowlCoversASafeCell()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            board.Reveal(new Cell(2, 2));
            board.ToggleFlag(new Cell(0, 0));
            board.ToggleFlag(new Cell(3, 3));
            foreach (var c in board.AllCells().ToList())
                if (c != new Cell(0, 0) && c != new Cell(3, 3)) board.Reveal(c);

            Assert.IsFalse(board.IsCleared);
            Assert.IsFalse(board.HasDiggableSafeCell());
        }

        [Test]
        public void PlaceDogs_WithWrongCount_Throws()
        {
            var board = new BoardModel(new BoardConfig(4, 4, 2), new Random(1));

            Assert.Throws<ArgumentException>(() => board.PlaceDogs(new[] { new Cell(0, 0) }));
        }

        [Test]
        public void PlaceDogs_Twice_Throws()
        {
            var board = TestBoards.Fixed(4, 4, (0, 0));

            Assert.Throws<InvalidOperationException>(() => board.PlaceDogs(new Cell(2, 2)));
        }
    }
}
