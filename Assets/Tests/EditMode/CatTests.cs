using System;
using System.Linq;
using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class CatTests
    {
        [Test]
        public void Cat_FindsDogsAround_AndDigsTheSafeCell()
        {
            // Köpekler (2,2) ve (3,3): kedi (2,3)'e gider
            var session = TestBoards.Session(TestBoards.Fixed(5, 5, (2, 2), (3, 3)));

            var move = session.SendCat(new Cell(2, 3));

            Assert.AreEqual(MoveKind.Cat, move.Kind);
            CollectionAssert.AreEquivalent(new[] { new Cell(2, 2), new Cell(3, 3) }, move.CatFoundDogs.ToList());
            Assert.AreEqual(CellState.Flagged, session.Board.GetState(new Cell(2, 2)));
            Assert.AreEqual(CellState.Revealed, session.Board.GetState(new Cell(2, 3)));
            Assert.AreEqual(3, session.Hearts);
            Assert.AreEqual(1, session.Moves);
            Assert.AreEqual(0, session.Board.DogsLeft);
        }

        [Test]
        public void Cat_OnADog_RevealsItWithoutLosingAHeart()
        {
            var session = TestBoards.Session(TestBoards.Fixed(5, 5, (2, 2)));

            var move = session.SendCat(new Cell(2, 2));

            Assert.AreEqual(1, move.CatFoundDogs.Count);
            Assert.AreEqual(CellState.Flagged, session.Board.GetState(new Cell(2, 2)));
            Assert.AreEqual(0, session.HeartsLost);
            Assert.IsFalse(move.WokeDog);
        }

        [Test]
        public void Cat_AsFirstMove_IsSafe()
        {
            var board = new BoardModel(new BoardConfig(8, 10, 20), new Random(4));
            var session = TestBoards.Session(board);

            var move = session.SendCat(new Cell(4, 4));

            Assert.IsTrue(board.DogsPlaced);
            Assert.AreEqual(0, move.CatFoundDogs.Count);
            Assert.AreEqual(CellState.Revealed, board.GetState(new Cell(4, 4)));
        }

        [Test]
        public void Cat_CanFinishTheGarden()
        {
            var session = TestBoards.Session(TestBoards.Fixed(4, 4, (3, 3)));
            session.Dig(new Cell(2, 2));

            session.SendCat(new Cell(0, 0));

            Assert.AreEqual(RoundStatus.Won, session.Status);
        }

        [Test]
        public void Cat_IgnoresOpenOrBowlCells()
        {
            var session = TestBoards.Session(TestBoards.Fixed(4, 4, (3, 3)));
            session.Dig(new Cell(2, 2));
            session.ToggleFlag(new Cell(3, 3));

            Assert.IsFalse(session.SendCat(new Cell(2, 2)).HasEffect);
            Assert.IsFalse(session.SendCat(new Cell(3, 3)).HasEffect);
        }
    }
}
