using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class ClassicModeTests
    {
        static RoundOutcome Win(int cells, int heartsLost = 0) =>
            new RoundOutcome(true, LossReason.None, cells, heartsLost, 3, 5);

        static RoundOutcome Loss(int cells) =>
            new RoundOutcome(false, LossReason.OutOfHearts, cells, 3, 3, 5);

        [Test]
        public void FirstGarden_IsTheOriginalBoard()
        {
            var setup = new ClassicMode(new PlayerProgress()).CreateRound();

            Assert.AreEqual(8, setup.Board.Columns);
            Assert.AreEqual(10, setup.Board.Rows);
            Assert.AreEqual(12, setup.Board.Dogs);
            Assert.AreEqual(ClassicMode.HeartsPerGarden, setup.Hearts);
            Assert.IsNotNull(setup.IntroHint);
        }

        [Test]
        public void Difficulty_GrowsAndCaps()
        {
            var late = ClassicDifficulty.ForGarden(100);

            Assert.AreEqual(ClassicDifficulty.MaxColumns, late.Columns);
            Assert.AreEqual(ClassicDifficulty.MaxRows, late.Rows);
            Assert.AreEqual(31, late.Dogs);
            Assert.Greater(ClassicDifficulty.ForGarden(4).Dogs, ClassicDifficulty.ForGarden(1).Dogs);
        }

        [Test]
        public void PerfectWin_GrowsStreak_AndScores()
        {
            var mode = new ClassicMode(new PlayerProgress());

            var report = mode.CompleteRound(Win(cells: 68));

            Assert.IsTrue(report.IsVictory);
            Assert.AreEqual(1, mode.Streak);
            Assert.AreEqual(2, mode.Garden);
            Assert.AreEqual(680 + 100, mode.RunScore);
            Assert.AreEqual(RoundAction.PlayCurrent, report.PrimaryAction);
        }

        [Test]
        public void Streak_MultipliesGardenBonus()
        {
            var mode = new ClassicMode(new PlayerProgress());
            mode.CompleteRound(Win(cells: 0));

            mode.CompleteRound(Win(cells: 0));

            Assert.AreEqual(2, mode.Streak);
            Assert.AreEqual(100 + (100 + 25) * 2, mode.RunScore);
        }

        [Test]
        public void LosingAHeart_ResetsStreak()
        {
            var mode = new ClassicMode(new PlayerProgress());
            mode.CompleteRound(Win(cells: 0));

            mode.CompleteRound(Win(cells: 0, heartsLost: 1));

            Assert.AreEqual(0, mode.Streak);
        }

        [Test]
        public void Loss_SavesBestScore_AndRestartsRun()
        {
            var progress = new PlayerProgress();
            var mode = new ClassicMode(progress);
            mode.CompleteRound(Win(cells: 10));

            var report = mode.CompleteRound(Loss(cells: 5));

            Assert.IsFalse(report.IsVictory);
            Assert.AreEqual(100 + 100 + 50, progress.ClassicBestScore);
            Assert.AreEqual(2, progress.ClassicBestGarden);
            StringAssert.Contains("Yeni rekor!", report.Body);
            Assert.AreEqual(1, mode.Garden);
            Assert.AreEqual(0, mode.RunScore);
        }

        [Test]
        public void Loss_BelowBest_IsNotARecord()
        {
            var progress = new PlayerProgress { ClassicBestScore = 5000 };
            var mode = new ClassicMode(progress);

            var report = mode.CompleteRound(Loss(cells: 5));

            StringAssert.DoesNotContain("Yeni rekor!", report.Body);
            Assert.AreEqual(5000, progress.ClassicBestScore);
        }

        [Test]
        public void Abandon_KeepsBestScore()
        {
            var progress = new PlayerProgress();
            var mode = new ClassicMode(progress);
            mode.CompleteRound(Win(cells: 10));

            mode.Abandon(new FakeRoundState { RevealedSafeCells = 3 });

            Assert.AreEqual(100 + 100 + 30, progress.ClassicBestScore);
            Assert.AreEqual(1, mode.Garden);
        }
    }
}
