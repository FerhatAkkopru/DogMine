using System;
using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Klasik mod: sonsuz bahçeler. Her bahçede 3 can; bahçe bitince sıradaki (biraz daha büyük)
    // bahçe gelir. Can kaybetmeden biten bahçeler seriyi büyütür, seri bahçe bonusunu katlar.
    // Bir bahçede tüm canlar giderse koşu biter ve en yüksek skor kaydedilir.
    public sealed class ClassicMode : IGameMode
    {
        public const int HeartsPerGarden = 3;
        public const int PointsPerCell = 10;
        public const int BaseGardenBonus = 100;
        public const int GardenBonusStep = 25;
        public const string FirstGardenHint = "Dokun: kaz  ·  Basılı tut: mama kabı";

        readonly PlayerProgress progress;
        int bestBeforeRun;

        public int Garden { get; private set; } = 1;
        public int RunScore { get; private set; }
        public int Streak { get; private set; }

        public ClassicMode(PlayerProgress progress)
        {
            this.progress = progress ?? throw new ArgumentNullException(nameof(progress));
            StartRun();
        }

        public string Name => "Klasik";
        public RoundAction ExitAction => RoundAction.ExitToMenu;

        public void StartRun()
        {
            Garden = 1;
            RunScore = 0;
            Streak = 0;
            bestBeforeRun = progress.ClassicBestScore;
        }

        public RoundSetup CreateRound() => new RoundSetup(
            $"Bahçe {Garden}",
            ClassicDifficulty.ForGarden(Garden),
            HeartsPerGarden,
            new FinishGardenGoal(),
            Garden == 1 ? FirstGardenHint : null);

        public int LiveScore(IRoundState state) => RunScore + state.RevealedSafeCells * PointsPerCell;

        public string GetStatusLine(IRoundState state) =>
            Streak > 0 ? $"Puan {LiveScore(state)}  ·  Seri ×{Streak}" : $"Puan {LiveScore(state)}";

        public static int GardenBonus(int garden, int streak) =>
            (BaseGardenBonus + GardenBonusStep * (Math.Max(1, garden) - 1)) * Math.Max(1, streak);

        public RoundReport CompleteRound(RoundOutcome outcome)
        {
            int digPoints = outcome.RevealedSafeCells * PointsPerCell;
            return outcome.Won ? CompleteWin(outcome, digPoints) : CompleteLoss(outcome, digPoints);
        }

        public void Abandon(IRoundState state)
        {
            RunScore += state.RevealedSafeCells * PointsPerCell;
            RecordBest(Garden);
            StartRun();
        }

        RoundReport CompleteWin(RoundOutcome outcome, int digPoints)
        {
            bool perfect = outcome.HeartsLost == 0;
            bool streakBroken = !perfect && Streak > 0;
            Streak = perfect ? Streak + 1 : 0;
            int bonus = GardenBonus(Garden, Streak);
            RunScore += digPoints + bonus;
            RecordBest(Garden);
            Garden++;

            var lines = new List<string>
            {
                perfect ? "Hiçbir köpek uyanmadı!" : $"{outcome.HeartsLost} köpek söylendi ama tekrar uyudu.",
                $"Kazı puanı  +{digPoints}",
                Streak > 1 ? $"Bahçe bonusu  +{bonus}  (seri ×{Streak})" : $"Bahçe bonusu  +{bonus}",
                $"Toplam: {RunScore}"
            };
            if (streakBroken) lines.Insert(1, "Seri sıfırlandı.");

            return new RoundReport(true, "Bahçe tamam!", string.Join("\n", lines),
                "Sonraki bahçe", RoundAction.PlayCurrent,
                "Menü", RoundAction.ExitToMenu);
        }

        RoundReport CompleteLoss(RoundOutcome outcome, int digPoints)
        {
            RunScore += digPoints;
            int reached = Garden;
            RecordBest(reached);
            bool record = RunScore > bestBeforeRun;

            var lines = new List<string>
            {
                $"{outcome.MaxHearts} köpek uyanınca bahçe ayaklandı.",
                $"Ulaşılan bahçe: {reached}",
                $"Puan: {RunScore}",
                record ? "Yeni rekor!" : $"En iyi: {progress.ClassicBestScore}"
            };
            StartRun();

            return new RoundReport(false, "Herkes uyandı!", string.Join("\n", lines),
                "Yeniden başla", RoundAction.PlayCurrent,
                "Menü", RoundAction.ExitToMenu);
        }

        // Her bahçeden sonra kaydedilir; uygulama kapansa bile skor kaybolmaz.
        void RecordBest(int garden)
        {
            progress.ClassicBestGarden = Math.Max(progress.ClassicBestGarden, garden);
            progress.ClassicBestScore = Math.Max(progress.ClassicBestScore, RunScore);
        }
    }
}
