namespace UyuyanKopekler.Core
{
    public enum CellState
    {
        Hidden,   // çim, kazılmamış
        Flagged,  // üstünde mama kabı var
        Revealed  // kazılmış (altında köpek varsa köpek uyanmıştır)
    }
}
