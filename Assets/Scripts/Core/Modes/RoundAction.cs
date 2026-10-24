namespace UyuyanKopekler.Core
{
    // Sonuç ekranındaki bir düğmeye basılınca ne olacağı.
    public enum RoundAction
    {
        PlayCurrent, // modun sıradaki turunu başlat (sonraki bahçe, tekrar dene, yeniden başla)
        ExitToMenu,
        ExitToMap
    }
}
