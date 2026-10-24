namespace UyuyanKopekler.Core
{
    public enum LossReason
    {
        None,
        OutOfHearts, // canlar bitti
        GoalFailed   // bölüm hedefi imkânsız hale geldi (ör. hamleler bitti)
    }
}
