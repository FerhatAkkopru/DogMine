using UnityEngine;
using UnityEngine.UI;

namespace UyuyanKopekler.UI
{
    // Elle çizilmiş kart: altta sert mürekkep gölgesi, üstte renkli yüz ve mürekkep kontur.
    // İçerik Body'ye eklenir; düğmelerde basılınca Body gölgeye doğru çöker.
    public sealed class Card
    {
        public RectTransform Root { get; }
        public RectTransform Body { get; }
        public Image Shadow { get; }
        public Image Face { get; }
        public Image Line { get; }
        public float ShadowDepth { get; }

        public Card(RectTransform root, RectTransform body, Image shadow, Image face, Image line, float shadowDepth)
        {
            Root = root;
            Body = body;
            Shadow = shadow;
            Face = face;
            Line = line;
            ShadowDepth = shadowDepth;
        }

        public void SetColor(Color color) => Face.color = color;
    }
}
