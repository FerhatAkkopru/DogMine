using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class AdventureModeTests
    {
        static readonly RoundOutcome Won = new RoundOutcome(true, LossReason.None, 10, 0, 3, 5);

        static AdventureMode Create(PlayerProgress progress = null) =>
            new AdventureMode(LevelCatalog.CreateDefault(), progress ?? new PlayerProgress());

        [Test]
        public void Initially_OnlyLevelOneIsOpen()
        {
            var mode = Create();

            Assert.AreEqual(LevelState.Current, mode.GetState(1));
            Assert.AreEqual(LevelState.Locked, mode.GetState(2));
            Assert.IsFalse(mode.SelectLevel(2));
            Assert.AreEqual("Bölüm 1", mode.CreateRound().Title);
        }

        [Test]
        public void WinningALevel_UnlocksTheNext_AndMovesToIt()
        {
            var progress = new PlayerProgress();
            var mode = Create(progress);
            mode.SelectLevel(1);

            var report = mode.CompleteRound(Won);

            Assert.AreEqual(1, progress.AdventureCompletedLevel);
            Assert.AreEqual(LevelState.Current, mode.GetState(2));
            Assert.AreEqual(2, mode.Current.Number);
            Assert.AreEqual(RoundAction.PlayCurrent, report.PrimaryAction);
        }

        [Test]
        public void ReplayingAnOldLevel_DoesNotLoseProgress()
        {
            var progress = new PlayerProgress { AdventureCompletedLevel = 5 };
            var mode = Create(progress);
            mode.SelectLevel(2);

            mode.CompleteRound(Won);

            Assert.AreEqual(5, progress.AdventureCompletedLevel);
        }

        [Test]
        public void WinningTheLastLevel_ReturnsToMap()
        {
            var mode = Create(new PlayerProgress { AdventureCompletedLevel = 11 });
            Assert.IsTrue(mode.SelectLevel(12));

            var report = mode.CompleteRound(Won);

            Assert.AreEqual(RoundAction.ExitToMap, report.PrimaryAction);
            Assert.AreEqual(LevelState.Completed, mode.GetState(12));
        }

        [Test]
        public void FailingTheGoal_ExplainsWhy()
        {
            var mode = Create(new PlayerProgress { AdventureCompletedLevel = 2 });
            mode.SelectLevel(3); // hamle sınırlı bölüm
            var outcome = new RoundOutcome(false, LossReason.GoalFailed, 10, 0, 3, 30);

            var report = mode.CompleteRound(outcome);

            StringAssert.Contains(mode.Current.Goal.FailureText, report.Body);
            Assert.AreEqual(RoundAction.PlayCurrent, report.PrimaryAction);
            Assert.AreEqual(3, mode.Current.Number);
        }

        [Test]
        public void Catalog_LevelsGrow()
        {
            var levels = LevelCatalog.CreateDefault();

            for (int i = 1; i < levels.Count; i++)
            {
                Assert.AreEqual(i + 1, levels[i].Number);
                Assert.GreaterOrEqual(levels[i].Board.CellCount, levels[i - 1].Board.CellCount);
            }
        }
    }
}
