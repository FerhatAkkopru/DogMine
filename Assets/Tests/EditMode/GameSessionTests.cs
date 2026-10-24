using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class GameSessionTests
    {
        [Test]
        public void WakingDogs_CostsHearts_AndLosesAtZero()
        {
            var session = TestBoards.Session(TestBoards.Fixed(4, 4, (3, 3), (3, 0), (0, 3)));

            session.Dig(new Cell(3, 3));
            Assert.AreEqual(2, session.Hearts);
            Assert.AreEqual(RoundStatus.Playing, session.Status);

            session.Dig(new Cell(3, 0));
            session.Dig(new Cell(0, 3));

            Assert.AreEqual(0, session.Hearts);
            Assert.AreEqual(RoundStatus.Lost, session.Status);
            Assert.AreEqual(LossReason.OutOfHearts, session.LossReason);
        }

        [Test]
        public void ClearingTheBoard_Wins_AndFlagsRemainingDogs()
        {
            var session = TestBoards.Session(TestBoards.Fixed(4, 4, (3, 3)));

            var move = session.Dig(new Cell(0, 0));

            Assert.AreEqual(RoundStatus.Won, session.Status);
            Assert.AreEqual(1, move.AutoFlagged.Count);
            Assert.AreEqual(0, session.Board.DogsLeft);
        }

        [Test]
        public void Moves_CountDigsAndChords_ButNotFlagsOrNoOps()
        {
            var session = TestBoards.Session(TestBoards.Fixed(4, 4, (3, 3)));

            session.Dig(new Cell(2, 2));       // 1 hamle
            session.ToggleFlag(new Cell(3, 3)); // hamle değil
            session.Dig(new Cell(3, 3));       // mama kabı: hamle değil
            session.Dig(new Cell(2, 2));       // çevreyi aç: 1 hamle

            Assert.AreEqual(2, session.Moves);
            Assert.AreEqual(RoundStatus.Won, session.Status);
        }

        [Test]
        public void AfterTheRoundEnds_MovesAreIgnored()
        {
            var session = TestBoards.Session(TestBoards.Fixed(4, 4, (3, 3)));
            session.Dig(new Cell(0, 0));

            Assert.IsFalse(session.Dig(new Cell(3, 3)).HasEffect);
            Assert.IsFalse(session.ToggleFlag(new Cell(3, 3)));
        }

        [Test]
        public void NoHeartLossGoal_FailsOnFirstWokenDog()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3), (0, 3));
            var session = TestBoards.Session(board, 3, new NoHeartLossGoal());

            session.Dig(new Cell(3, 3));

            Assert.AreEqual(RoundStatus.Lost, session.Status);
            Assert.AreEqual(LossReason.GoalFailed, session.LossReason);
            Assert.AreEqual(2, session.Hearts);
        }

        [Test]
        public void MoveLimitGoal_FailsWhenMovesRunOut()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            var session = TestBoards.Session(board, 3, new MoveLimitGoal(1));

            session.Dig(new Cell(2, 2));

            Assert.AreEqual(RoundStatus.Lost, session.Status);
            Assert.AreEqual(LossReason.GoalFailed, session.LossReason);
        }

        [Test]
        public void MoveLimitGoal_WinsWhenClearedOnLastMove()
        {
            var board = TestBoards.Fixed(4, 4, (3, 3));
            var session = TestBoards.Session(board, 3, new MoveLimitGoal(1));

            session.Dig(new Cell(0, 0));

            Assert.AreEqual(RoundStatus.Won, session.Status);
        }

        [Test]
        public void Outcome_ReflectsTheRound()
        {
            var session = TestBoards.Session(TestBoards.Fixed(4, 4, (3, 3)));
            session.Dig(new Cell(3, 3));
            session.Dig(new Cell(0, 0));

            var outcome = session.ToOutcome();

            Assert.IsTrue(outcome.Won);
            Assert.AreEqual(1, outcome.HeartsLost);
            Assert.AreEqual(2, outcome.Moves);
            Assert.AreEqual(15, outcome.RevealedSafeCells);
        }
    }
}
