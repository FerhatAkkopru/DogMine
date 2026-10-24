using System;
using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Macera modu: harita üzerinde sırayla açılan bölümler, her birinin bir hedefi var.
    public sealed class AdventureMode : IGameMode
    {
        readonly IReadOnlyList<LevelDefinition> levels;
        readonly PlayerProgress progress;

        public LevelDefinition Current { get; private set; }

        public AdventureMode(IReadOnlyList<LevelDefinition> levels, PlayerProgress progress)
        {
            if (levels == null || levels.Count == 0) throw new ArgumentException("En az bir bölüm gerekli.", nameof(levels));
            this.levels = levels;
            this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public string Name => "Macera";
        public RoundAction ExitAction => RoundAction.ExitToMap;

        public int LevelCount => levels.Count;
        public int CompletedCount => Math.Min(progress.AdventureCompletedLevel, levels.Count);
        public int HighestUnlocked => Math.Min(CompletedCount + 1, levels.Count);

        public LevelDefinition GetLevel(int number) => levels[number - 1];
        public bool IsUnlocked(int number) => number >= 1 && number <= HighestUnlocked;

        public LevelState GetState(int number)
        {
            if (number <= CompletedCount) return LevelState.Completed;
            return number == HighestUnlocked ? LevelState.Current : LevelState.Locked;
        }

        public IReadOnlyList<LevelInfo> GetLevelInfos()
        {
            var infos = new List<LevelInfo>(levels.Count);
            foreach (var level in levels)
                infos.Add(new LevelInfo(level.Number, level.Goal.Title, GetState(level.Number)));
            return infos;
        }

        public bool SelectLevel(int number)
        {
            if (!IsUnlocked(number)) return false;
            Current = GetLevel(number);
            return true;
        }

        public RoundSetup CreateRound()
        {
            if (Current == null) SelectLevel(HighestUnlocked);
            return new RoundSetup(
                $"Bölüm {Current.Number}",
                Current.Board,
                Current.Hearts,
                Current.Goal,
                $"Hedef: {Current.Goal.Title}");
        }

        public string GetStatusLine(IRoundState state) => Current.Goal.Progress(state);

        public RoundReport CompleteRound(RoundOutcome outcome) =>
            outcome.Won ? CompleteWin() : CompleteLoss(outcome);

        public void Abandon(IRoundState state) { }

        RoundReport CompleteWin()
        {
            int finished = Current.Number;
            bool firstTime = finished > progress.AdventureCompletedLevel;
            if (firstTime) progress.AdventureCompletedLevel = finished;

            string goal = $"Hedef tamam: {Current.Goal.Title}";
            bool hasNext = finished < levels.Count;
            if (!hasNext)
            {
                return new RoundReport(true, $"Bölüm {finished} tamam!", $"{goal}\nBütün bölümleri bitirdin!",
                    "Harita", RoundAction.ExitToMap,
                    "Menü", RoundAction.ExitToMenu);
            }

            Current = GetLevel(finished + 1);
            string body = firstTime ? $"{goal}\nBölüm {finished + 1} açıldı!" : goal;
            return new RoundReport(true, $"Bölüm {finished} tamam!", body,
                "Sonraki bölüm", RoundAction.PlayCurrent,
                "Harita", RoundAction.ExitToMap);
        }

        RoundReport CompleteLoss(RoundOutcome outcome)
        {
            string reason = outcome.LossReason == LossReason.GoalFailed
                ? Current.Goal.FailureText
                : $"{outcome.MaxHearts} köpek uyanınca bahçe ayaklandı.";

            return new RoundReport(false, "Olmadı!", $"{reason}\nHedef: {Current.Goal.Title}",
                "Tekrar dene", RoundAction.PlayCurrent,
                "Harita", RoundAction.ExitToMap);
        }
    }
}
