namespace UyuyanKopekler.Core
{
    // Bir turun nasıl kazanılacağını söyler. Yeni hedef eklemek için bu arayüzü uygulayan
    // yeni bir sınıf yazmak yeterli (ör. ileride "5 yıldız topla").
    public interface IRoundGoal
    {
        // Haritada ve bölüm girişinde görünen kısa açıklama: "20 hamlede bitir".
        string Title { get; }

        // Oyun sırasında üst barda görünen ilerleme: "Hamle 7/20".
        string Progress(IRoundState state);

        // Hedef imkânsızlaşınca sonuç ekranında görünen açıklama.
        string FailureText { get; }

        GoalStatus Evaluate(IRoundState state);
    }
}
