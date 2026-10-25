using System.Collections.Generic;
using UnityEngine;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.Anim
{
    // Animasyon tanımlarını (Clips) gerçek sprite dizilerine çevirir ve saklar.
    public sealed class ClipLibrary
    {
        readonly IGameArt art;
        readonly Dictionary<(int, object), Sprite[]> dogCache = new Dictionary<(int, object), Sprite[]>();
        readonly Dictionary<object, Sprite[]> catCache = new Dictionary<object, Sprite[]>();
        Sprite[] puff;

        public ClipLibrary(IGameArt art)
        {
            this.art = art;
        }

        public IGameArt Art => art;

        public Sprite[] Dog(int coat, Clips.Clip<DogFrame> clip)
        {
            var key = (coat, (object)clip);
            if (!dogCache.TryGetValue(key, out var sprites))
            {
                sprites = new Sprite[clip.Frames.Length];
                for (int i = 0; i < sprites.Length; i++) sprites[i] = art.Dog(coat, clip.Frames[i]);
                dogCache[key] = sprites;
            }
            return sprites;
        }

        public Sprite[] Cat(Clips.Clip<CatFrame> clip)
        {
            if (!catCache.TryGetValue(clip, out var sprites))
            {
                sprites = new Sprite[clip.Frames.Length];
                for (int i = 0; i < sprites.Length; i++) sprites[i] = art.Cat(clip.Frames[i]);
                catCache[clip] = sprites;
            }
            return sprites;
        }

        public Sprite[] Puff => puff ??= new[] { art.Puff(0), art.Puff(1), art.Puff(2) };
    }
}
