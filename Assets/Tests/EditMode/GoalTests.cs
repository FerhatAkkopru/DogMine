using NUnit.Framework;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    public class GoalTests
    {
        [Test]
        public void FinishGarden_CompletesOnlyWhenCleared()
        {
            var goal = new FinishGardenGoal();

            Assert.AreEqual(GoalStatus.InProgress, goal.Evaluate(new FakeRoundState()));
            Assert.AreEqual(GoalStatus.Completed, goal.Evaluate(new FakeRoundState { IsBoardCleared = true }));
        }

        [Test]
        public void NoHeartLoss_FailsWhenAHeartIsLost()
        {
            var goal = new NoHeartLossGoal();

            Assert.AreEqual(GoalStatus.Failed, goal.Evaluate(new FakeRoundState { HeartsLost = 1, IsBoardCleared = true }));
        }

        [Test]
        public void MoveLimit_ShowsProgress_AndFailsAtTheLimit()
        {
            var goal = new MoveLimitGoal(20);

            Assert.AreEqual("Hamle: 7/20", goal.Progress(new FakeRoundState { Moves = 7 }));
            Assert.AreEqual(GoalStatus.InProgress, goal.Evaluate(new FakeRoundState { Moves = 19 }));
            Assert.AreEqual(GoalStatus.Failed, goal.Evaluate(new FakeRoundState { Moves = 20 }));
            Assert.AreEqual(GoalStatus.Completed, goal.Evaluate(new FakeRoundState { Moves = 20, IsBoardCleared = true }));
        }
    }
}
