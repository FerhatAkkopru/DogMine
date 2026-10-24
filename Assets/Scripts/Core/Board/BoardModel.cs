using System;
using System.Collections.Generic;

namespace UyuyanKopekler.Core
{
    // Bahçenin kuralları: köpeklerin yeri, açılan kareler, mama kapları, komşu sayıları.
    // Can, hamle ve hedef gibi tur bilgilerini bilmez; onlar GameSession'ın işidir.
    public sealed class BoardModel
    {
        readonly bool[,] dogs;
        readonly CellState[,] states;
        readonly int[,] nearby;
        readonly Random random;

        public BoardConfig Config { get; }
        public int Columns => Config.Columns;
        public int Rows => Config.Rows;

        public bool DogsPlaced { get; private set; }
        public int RevealedSafeCount { get; private set; }
        public int FlagCount { get; private set; }
        public int WokenDogCount { get; private set; }

        public bool IsCleared => RevealedSafeCount == Config.SafeCellCount;
        public int SafeCellsLeft => Config.SafeCellCount - RevealedSafeCount;
        public int DogsLeft => Math.Max(0, Config.Dogs - FlagCount - WokenDogCount);

        public BoardModel(BoardConfig config, Random random)
        {
            Config = config;
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            dogs = new bool[config.Columns, config.Rows];
            states = new CellState[config.Columns, config.Rows];
            nearby = new int[config.Columns, config.Rows];
        }

        // ---------- Sorgular ----------

        public bool Contains(Cell c) => c.X >= 0 && c.X < Columns && c.Y >= 0 && c.Y < Rows;
        public CellState GetState(Cell c) => states[c.X, c.Y];
        public bool HasDog(Cell c) => dogs[c.X, c.Y];
        public int GetNearbyDogs(Cell c) => nearby[c.X, c.Y];

        public IEnumerable<Cell> Neighbors(Cell c)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var n = new Cell(c.X + dx, c.Y + dy);
                    if (Contains(n)) yield return n;
                }
        }

        public IEnumerable<Cell> AllCells()
        {
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    yield return new Cell(x, y);
        }

        // Kazılabilecek (çimli, mama kabı olmayan) güvenli kare kaldı mı?
        public bool HasDiggableSafeCell()
        {
            foreach (var c in AllCells())
                if (states[c.X, c.Y] == CellState.Hidden && !dogs[c.X, c.Y]) return true;
            return false;
        }

        // ---------- Köpekleri yerleştirme ----------

        // İlk dokunulan kare ve 8 komşusu köpeksiz kalacak şekilde rastgele yerleştirir.
        public void PlaceDogs(Cell safeCell)
        {
            EnsureNotPlaced();

            var candidates = new List<Cell>(Config.CellCount);
            foreach (var c in AllCells())
                if (c.StepsTo(safeCell) > 1) candidates.Add(c);

            // Kısmi Fisher–Yates karıştırma: ilk "Dogs" eleman rastgele seçilmiş olur.
            int count = Math.Min(Config.Dogs, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                int j = random.Next(i, candidates.Count);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
                dogs[candidates[i].X, candidates[i].Y] = true;
            }

            FinishPlacement();
        }

        // Köpekleri belirli karelere koyar (testler ve ileride elle tasarlanmış bölümler için).
        public void PlaceDogs(IEnumerable<Cell> dogCells)
        {
            EnsureNotPlaced();

            int placed = 0;
            foreach (var c in dogCells)
            {
                if (!Contains(c)) throw new ArgumentException($"{c} bahçenin dışında.");
                if (dogs[c.X, c.Y]) throw new ArgumentException($"{c} iki kez verildi.");
                dogs[c.X, c.Y] = true;
                placed++;
            }
            if (placed != Config.Dogs)
                throw new ArgumentException($"{Config.Dogs} köpek bekleniyordu, {placed} verildi.");

            FinishPlacement();
        }

        void EnsureNotPlaced()
        {
            if (DogsPlaced) throw new InvalidOperationException("Köpekler zaten yerleştirildi.");
        }

        void FinishPlacement()
        {
            foreach (var c in AllCells())
            {
                int n = 0;
                foreach (var nb in Neighbors(c))
                    if (dogs[nb.X, nb.Y]) n++;
                nearby[c.X, c.Y] = n;
            }
            DogsPlaced = true;
        }

        // ---------- Hamleler ----------

        // Çimli bir kareyi kazar. İlk kazıda köpekler yerleştirilir (ilk dokunuş güvenli).
        public RevealResult Reveal(Cell cell)
        {
            if (!Contains(cell) || states[cell.X, cell.Y] != CellState.Hidden) return RevealResult.Empty;
            if (!DogsPlaced) PlaceDogs(cell);

            var revealed = new List<RevealedCell>();
            var woken = new List<Cell>();
            Dig(cell, 0, revealed, woken);
            return new RevealResult(revealed, woken);
        }

        // Açık bir rakama dokunulduğunda: çevredeki mama kabı (ve uyanmış köpek) sayısı
        // rakama eşitse kalan çimli komşuların hepsini kazar.
        public RevealResult Chord(Cell cell)
        {
            if (!Contains(cell) || states[cell.X, cell.Y] != CellState.Revealed || dogs[cell.X, cell.Y])
                return RevealResult.Empty;

            int number = nearby[cell.X, cell.Y];
            if (number == 0) return RevealResult.Empty;

            int marked = 0;
            var targets = new List<Cell>();
            foreach (var n in Neighbors(cell))
            {
                var s = states[n.X, n.Y];
                if (s == CellState.Flagged || (s == CellState.Revealed && dogs[n.X, n.Y])) marked++;
                else if (s == CellState.Hidden) targets.Add(n);
            }
            if (marked != number || targets.Count == 0) return RevealResult.Empty;

            var revealed = new List<RevealedCell>();
            var woken = new List<Cell>();
            foreach (var t in targets) Dig(t, 1, revealed, woken);
            return new RevealResult(revealed, woken);
        }

        // Mama kabı koyar ya da kaldırır. İlk kazıdan önce ve açık karelerde çalışmaz.
        public bool ToggleFlag(Cell cell)
        {
            if (!DogsPlaced || !Contains(cell)) return false;

            switch (states[cell.X, cell.Y])
            {
                case CellState.Hidden:
                    states[cell.X, cell.Y] = CellState.Flagged;
                    FlagCount++;
                    return true;
                case CellState.Flagged:
                    states[cell.X, cell.Y] = CellState.Hidden;
                    FlagCount--;
                    return true;
                default:
                    return false;
            }
        }

        // Bahçe bitince açıkta kalan köpeklerin üstüne mama kabı koyar; sayaç 0'a iner.
        public IReadOnlyList<Cell> FlagRemainingDogs()
        {
            var flagged = new List<Cell>();
            foreach (var c in AllCells())
            {
                if (dogs[c.X, c.Y] && states[c.X, c.Y] == CellState.Hidden)
                {
                    states[c.X, c.Y] = CellState.Flagged;
                    FlagCount++;
                    flagged.Add(c);
                }
            }
            return flagged;
        }

        // ---------- İç işler ----------

        // Kare köpekse uyandırır; değilse boş alanı yayılarak (BFS) açar.
        void Dig(Cell start, int baseDepth, List<RevealedCell> revealed, List<Cell> woken)
        {
            if (states[start.X, start.Y] != CellState.Hidden) return;

            if (dogs[start.X, start.Y])
            {
                states[start.X, start.Y] = CellState.Revealed;
                WokenDogCount++;
                woken.Add(start);
                return;
            }

            var queue = new Queue<RevealedCell>();
            queue.Enqueue(new RevealedCell(start, baseDepth));
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var c = item.Cell;
                if (states[c.X, c.Y] != CellState.Hidden || dogs[c.X, c.Y]) continue;

                states[c.X, c.Y] = CellState.Revealed;
                RevealedSafeCount++;
                revealed.Add(item);

                if (nearby[c.X, c.Y] != 0) continue;
                foreach (var n in Neighbors(c))
                    if (states[n.X, n.Y] == CellState.Hidden)
                        queue.Enqueue(new RevealedCell(n, item.Depth + 1));
            }
        }
    }
}
