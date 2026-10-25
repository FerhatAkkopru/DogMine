namespace UyuyanKopekler.Audio
{
    // Ses çalan her şey bu arayüzü kullanır; nasıl çalındığını bilmez.
    public interface ISoundPlayer
    {
        void Play(Sfx sfx);
    }
}
