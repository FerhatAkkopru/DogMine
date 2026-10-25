using System;
using UnityEngine;
using UyuyanKopekler.Art;

namespace UyuyanKopekler.UI
{
    // DialogPanel'de gösterilecek içerik: mola, sonuç, bölüm girişi, onay...
    public sealed class DialogContent
    {
        public Sprite Icon { get; set; }

        // Doluysa ikon yerine bu kareler oynar (ör. kuyruk sallayan köpek).
        public Sprite[] IconFrames { get; set; }
        public float IconFps { get; set; } = 6f;
        public string Title { get; set; }
        public string Body { get; set; }

        public string PrimaryLabel { get; set; }
        public Color PrimaryColor { get; set; } = Palette.Mint;
        public Action OnPrimary { get; set; }

        // Boş bırakılırsa ikinci düğme gizlenir.
        public string SecondaryLabel { get; set; }
        public Action OnSecondary { get; set; }

        // Android geri tuşu. Boşsa ikinci düğme (o da yoksa birinci düğme) seçilmiş sayılır.
        public Action OnBack { get; set; }
    }
}
