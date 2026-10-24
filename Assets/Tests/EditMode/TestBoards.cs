using System;
using System.Linq;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Tests
{
    // Testlerde köpekleri belirli karelere koyan yardımcılar.
    static class TestBoards
    {
        public static BoardModel Fixed(int columns, int rows, params (int x, int y)[] dogs)
        {
            var board = new BoardModel(new BoardConfig(columns, rows, dogs.Length), new Random(1));
            board.PlaceDogs(dogs.Select(d => new Cell(d.x, d.y)));
            return board;
        }

        public static GameSession Session(BoardModel board, int hearts = 3, IRoundGoal goal = null) =>
            new GameSession(board, hearts, goal ?? new FinishGardenGoal());
    }
}
