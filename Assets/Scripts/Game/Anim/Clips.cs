using UyuyanKopekler.Art;

namespace UyuyanKopekler.Anim
{
    // Köpek ve kedi animasyonları: hangi kareler hangi sırayla ve hangi hızda oynar.
    public static class Clips
    {
        public sealed class Clip<T>
        {
            public readonly T[] Frames;
            public readonly float Fps;
            public Clip(float fps, params T[] frames)
            {
                Fps = fps;
                Frames = frames;
            }
        }

        // Uyurken nefes alır, arada bir kulağı kıpırdar.
        public static readonly Clip<DogFrame> Sleep = new Clip<DogFrame>(2.6f,
            DogFrame.Sleep0, DogFrame.Sleep1, DogFrame.Sleep0, DogFrame.Sleep1, DogFrame.Sleep0, DogFrame.SleepTwitch);

        // Kazılınca: gözünü açar, kafasını kaldırır, fırlar ve havlar.
        public static readonly Clip<DogFrame> Wake = new Clip<DogFrame>(9f,
            DogFrame.Sleep0, DogFrame.Wake, DogFrame.Wake, DogFrame.Alert, DogFrame.Bark, DogFrame.Alert, DogFrame.Bark, DogFrame.Alert);

        public static readonly Clip<DogFrame> Bark = new Clip<DogFrame>(6f, DogFrame.Bark, DogFrame.Alert);
        public static readonly Clip<DogFrame> Wag = new Clip<DogFrame>(5f, DogFrame.Wag0, DogFrame.Wag1);
        public static readonly Clip<DogFrame> Run = new Clip<DogFrame>(14f, DogFrame.Run0, DogFrame.Run1, DogFrame.Run2, DogFrame.Run3);

        // Önden oturup kuyruk sallar, arada göz kırpar.
        public static readonly Clip<DogFrame> SitWag = new Clip<DogFrame>(7f,
            DogFrame.Sit0, DogFrame.Sit1, DogFrame.Sit2, DogFrame.Sit1, DogFrame.Sit0, DogFrame.Sit1, DogFrame.Sit2, DogFrame.Sit1,
            DogFrame.Blink, DogFrame.Sit1);

        public static readonly Clip<DogFrame> Happy = new Clip<DogFrame>(5f, DogFrame.Happy, DogFrame.Sit0, DogFrame.Happy, DogFrame.Sit2);
        // Şaşkın: gözler kocaman, ağzı açık (ör. "Emin misin?" penceresi).
        public static readonly Clip<DogFrame> Gasp = new Clip<DogFrame>(1f, DogFrame.Gasp);

        public static readonly Clip<CatFrame> CatWalk = new Clip<CatFrame>(9f, CatFrame.Walk0, CatFrame.Walk1, CatFrame.Walk2, CatFrame.Walk3);
        public static readonly Clip<CatFrame> CatRun = new Clip<CatFrame>(16f, CatFrame.Run0, CatFrame.Run1, CatFrame.Run2, CatFrame.Run3);
        public static readonly Clip<CatFrame> CatSit = new Clip<CatFrame>(3f, CatFrame.Sit0, CatFrame.Sit1);
        public static readonly Clip<CatFrame> CatStartle = new Clip<CatFrame>(1f, CatFrame.Startle);
    }
}
