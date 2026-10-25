using System.Collections;
using UnityEngine;
using UyuyanKopekler.Anim;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Bahçedeki tek bir karenin görüntüsü: zemin (çim/toprak), ikon (mama kabı/köpek),
    // işaret (yanlış kap çarpısı) ve ipucu (pati izleri ya da rakam). Köpekler kare kare oynar.
    // Her kare konumuna göre sabit bir "görünüm" seçer: çim deseni, altındaki köpeğin kürkü
    // ve köpeğin hangi yöne baktığı.
    public sealed class TileView : MonoBehaviour
    {
        public const int BaseOrder = 0;
        public const int IconOrder = 1;
        public const int MarkOrder = 2;
        public const int LabelOrder = 3;

        const float BowlScale = 0.8f;
        const float AwakeDogScale = 1.3f; // komşu karelere biraz taşar: yerinden fırlamış gibi
        const float SleepingDogScale = 1.2f;
        const float HintScale = 0.84f;
        const float PressedScale = 0.92f;

        IGameArt art;
        ClipLibrary clips;
        SpriteAnimator dogAnimator;
        SpriteRenderer hint;
        Transform body;
        SpriteRenderer ground;
        SpriteRenderer icon;
        SpriteRenderer mark;
        TextMesh label;
        Coroutine motion;
        Coroutine iconMotion;
        float iconScale = 1f;
        float motionScale = 1f;
        float pressScale = 1f;
        Color groundColor;
        int look; // bu karenin sabit görünüm numarası

        public Cell Cell { get; private set; }

        // Köpeğin kürkü ve baktığı yön (yan görünüm kareleri sağa bakar).
        public int Coat => look % art.DogCoatCount;
        public bool FacesLeft => (look / 3) % 2 == 0;

        public static TileView Create(Transform parent, ClipLibrary clips)
        {
            var go = new GameObject("Kare");
            go.transform.SetParent(parent, false);
            var tile = go.AddComponent<TileView>();
            tile.clips = clips;
            tile.art = clips.Art;
            var art = tile.art;

            tile.body = new GameObject("Gövde").transform;
            tile.body.SetParent(go.transform, false);
            tile.ground = AddRenderer(tile.body, "Zemin", BaseOrder);
            tile.icon = AddRenderer(tile.body, "İkon", IconOrder);
            tile.mark = AddRenderer(tile.body, "İşaret", MarkOrder);
            tile.hint = AddRenderer(tile.body, "Pati izi", LabelOrder);
            tile.hint.transform.localScale = new Vector3(HintScale, HintScale, 1f);
            tile.dogAnimator = SpriteAnimator.For(tile.icon);

            var labelGo = new GameObject("Rakam");
            labelGo.transform.SetParent(tile.body, false);
            labelGo.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            tile.label = labelGo.AddComponent<TextMesh>();
            tile.label.font = art.DisplayFont;
            tile.label.fontSize = 72;
            tile.label.characterSize = 0.1f;
            tile.label.anchor = TextAnchor.MiddleCenter;
            tile.label.alignment = TextAlignment.Center;
            tile.label.fontStyle = FontStyle.Normal;
            var labelRenderer = labelGo.GetComponent<MeshRenderer>();
            labelRenderer.sharedMaterial = art.DisplayFont.material;
            labelRenderer.sortingOrder = LabelOrder;
            return tile;
        }

        static SpriteRenderer AddRenderer(Transform parent, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        public void Place(Cell cell, Vector3 localPosition)
        {
            Cell = cell;
            look = ((cell.X * 73856093) ^ (cell.Y * 19349663)) & 0x7fffffff;
            name = $"Kare {cell.X},{cell.Y}";
            transform.localPosition = localPosition;
            StopMotion();
            StopIconMotion();
            pressScale = 1f;
            ApplyScale();
        }

        // ---------- Görünüm durumları ----------

        // light: dama tahtası için açık ton.
        public void ShowGrass(bool light, bool bowl)
        {
            SetGround(art.GrassTile(look / 7, light), Color.white);
            SetIcon(bowl ? art.Bowl : null, BowlScale, Color.white);
            ClearMarks();
        }

        // pawHints: rakam yerine o kadar pati izi göster.
        public void ShowSoil(int nearbyDogs, bool pawHints)
        {
            SetGround(art.SoilTile(look / 5), Color.white);
            SetIcon(null, 1f, Color.white);
            ClearMarks();
            if (nearbyDogs <= 0) return;
            if (pawHints)
            {
                hint.sprite = art.PawHint(nearbyDogs);
            }
            else
            {
                label.text = nearbyDogs.ToString();
                label.color = Palette.Numbers[Mathf.Clamp(nearbyDogs, 0, Palette.Numbers.Length - 1)];
            }
        }

        // Uyanmış köpek: ayakta, kuyruk sallıyor. wakeUp ise önce uyanma animasyonu oynar.
        public void ShowAwakeDog(bool wakeUp)
        {
            SetGround(art.SoilTile(look / 5), Palette.WokenSoil);
            SetIcon(null, AwakeDogScale, Color.white);
            ClearMarks();
            icon.flipX = FacesLeft;
            var wag = clips.Dog(Coat, Clips.Wag);
            if (wakeUp)
                dogAnimator.Play(clips.Dog(Coat, Clips.Wake), Clips.Wake.Fps, false,
                    () => dogAnimator.Play(wag, Clips.Wag.Fps, true));
            else
                dogAnimator.PlayFromRandomFrame(wag, Clips.Wag.Fps);
        }

        // Kazanınca: hiç uyanmayan köpekler kıvrılmış, nefes alarak uyur.
        public void ShowSleepingDog()
        {
            SetGround(art.SoilTile(look / 5), Color.white);
            SetIcon(null, SleepingDogScale, Color.white);
            ClearMarks();
            icon.flipX = FacesLeft;
            dogAnimator.PlayFromRandomFrame(clips.Dog(Coat, Clips.Sleep), Clips.Sleep.Fps);
        }

        // Kedi gelince: köpek çimin üstünde kediye doğru havlar.
        public void ShowBarkingDog(bool light, bool faceLeft)
        {
            SetGround(art.GrassTile(look / 7, light), Color.white);
            SetIcon(null, AwakeDogScale, Color.white);
            ClearMarks();
            icon.flipX = faceLeft;
            dogAnimator.Play(clips.Dog(Coat, Clips.Bark), Clips.Bark.Fps, true);
        }

        // Köpek kaçıp gittikten sonra kalan boş çukur.
        public void ShowEmptyHole()
        {
            SetGround(art.SoilTile(look / 5), Palette.WokenSoil);
            SetIcon(null, 1f, Color.white);
            ClearMarks();
        }

        // Kaybedince: köpek olmayan yere konmuş mama kabı.
        public void ShowWrongBowl(bool light)
        {
            SetGround(art.GrassTile(look / 7, light), Palette.WrongBowl);
            SetIcon(art.Bowl, BowlScale, Palette.WithAlpha(Color.white, 0.5f));
            ClearMarks();
            SetMark(art.IconCross);
        }

        public void SetPressed(bool pressed)
        {
            pressScale = pressed ? PressedScale : 1f;
            ground.color = pressed ? groundColor * Palette.Pressed : groundColor;
            ApplyScale();
        }

        // ---------- Animasyonlar ----------

        // Kazılınca küçükten büyüyerek yerine oturur.
        public void Pop(float delay) => Play(Animate(delay, 0.22f, t => Mathf.LerpUnclamped(0.55f, 1f, Tween.OutBack(t))));

        // Kutlama dalgası: hafifçe zıplar.
        public void Bump(float delay) => Play(Animate(delay, 0.3f, t => 1f + Tween.Bump(t) * 0.12f));

        // Mama kabı ya da köpek "pop" yaparak belirir.
        public void PopIcon(float delay, float overshoot = 1f)
        {
            StopIconMotion();
            if (isActiveAndEnabled) iconMotion = StartCoroutine(AnimateIcon(delay, overshoot));
        }

        IEnumerator Animate(float delay, float seconds, System.Func<float, float> scaleAt)
        {
            if (delay > 0f)
            {
                motionScale = scaleAt(0f);
                ApplyScale();
            }
            yield return Tween.Run(seconds, t =>
            {
                motionScale = scaleAt(t);
                ApplyScale();
            }, delay);
            motionScale = 1f;
            ApplyScale();
        }

        IEnumerator AnimateIcon(float delay, float overshoot)
        {
            SetIconScale(0f);
            yield return Tween.Run(0.3f, t =>
            {
                SetIconScale(Mathf.LerpUnclamped(0f, 1f, Tween.OutBack(t)) * (1f + Tween.Bump(t) * 0.25f * overshoot));
            }, delay);
            SetIconScale(1f);
            iconMotion = null;
        }

        void Play(IEnumerator routine)
        {
            StopMotion();
            if (isActiveAndEnabled) motion = StartCoroutine(routine);
        }

        void StopIconMotion()
        {
            if (iconMotion != null) StopCoroutine(iconMotion);
            iconMotion = null;
            SetIconScale(1f);
        }

        void StopMotion()
        {
            if (motion != null) StopCoroutine(motion);
            motion = null;
            motionScale = 1f;
            ApplyScale();
        }

        // ---------- Yardımcılar ----------

        void SetGround(Sprite sprite, Color color)
        {
            ground.sprite = sprite;
            groundColor = color;
            ground.color = color;
        }

        void SetIcon(Sprite sprite, float scale, Color color)
        {
            StopIconMotion();
            dogAnimator.Stop();
            icon.flipX = false;
            icon.sprite = sprite;
            icon.color = color;
            iconScale = scale;
            SetIconScale(1f);
        }

        // factor: animasyon çarpanı (1 = normal boy).
        void SetIconScale(float factor)
        {
            float s = iconScale * factor;
            icon.transform.localScale = new Vector3(s, s, 1f);
        }

        void ClearMarks()
        {
            SetMark(null);
            hint.sprite = null;
            label.text = "";
        }

        void SetMark(Sprite sprite)
        {
            mark.sprite = sprite;
            mark.color = Palette.CoralDark;
            mark.transform.localScale = new Vector3(0.62f, 0.62f, 1f);
        }

        void ApplyScale()
        {
            float s = motionScale * pressScale;
            body.localScale = new Vector3(s, s, 1f);
        }
    }
}
