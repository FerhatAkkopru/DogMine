namespace UyuyanKopekler.Core
{
    // Bir oyun modu: turları nasıl kurduğunu, puanı/ilerlemeyi nasıl tuttuğunu bilir.
    // RoundController hangi modun oynandığını bilmeden bu arayüzle çalışır.
    public interface IGameMode
    {
        string Name { get; }

        // Oyun sırasında "Çık" denince nereye dönülecek.
        RoundAction ExitAction { get; }

        RoundSetup CreateRound();

        // Üst barda başlığın altında görünen satır (puan, hedef ilerlemesi...).
        string GetStatusLine(IRoundState state);

        // Tur bitince çağrılır: puanı/ilerlemeyi günceller ve sonuç penceresini tarif eder.
        RoundReport CompleteRound(RoundOutcome outcome);

        // Oyuncu tur ortasında çıkarsa çağrılır. state, ilk kazıdan önce çıkılırsa da doludur.
        void Abandon(IRoundState state);
    }
}
