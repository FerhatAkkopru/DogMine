using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Macera bölümleri. Bölümler giderek büyür; hedefler sırayla değişir.
    // Hamle sınırları güvenli kare sayısının yaklaşık yarısıdır; oynayarak ayarlanacak.
    public static class LevelCatalog
    {
        public const int DefaultHearts = 3;

        public static IReadOnlyList<LevelDefinition> CreateDefault()
        {
            var finish = new FinishGardenGoal();
            var noLoss = new NoHeartLossGoal();

            return new List<LevelDefinition>
            {
                Level(1, 6, 8, 6, finish),
                Level(2, 7, 9, 8, finish),
                Level(3, 7, 9, 9, new MoveLimitGoal(30)),
                Level(4, 8, 10, 11, noLoss),
                Level(5, 8, 10, 12, finish),
                Level(6, 8, 11, 14, new MoveLimitGoal(40)),
                Level(7, 9, 11, 16, noLoss),
                Level(8, 9, 12, 18, finish),
                Level(9, 9, 12, 19, new MoveLimitGoal(48)),
                Level(10, 10, 13, 24, noLoss),
                Level(11, 10, 13, 25, finish),
                Level(12, 10, 14, 28, new MoveLimitGoal(60)),
            };
        }

        static LevelDefinition Level(int number, int columns, int rows, int dogs, IRoundGoal goal) =>
            new LevelDefinition(number, new BoardConfig(columns, rows, dogs), DefaultHearts, goal);
    }
}
