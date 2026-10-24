using System.Collections;
using UnityEngine;
using UyuyanKopekler.Art;
using UyuyanKopekler.Core;
using UyuyanKopekler.UI;

namespace UyuyanKopekler.Game
{
    // Bahçedeki tek bir karenin görüntüsü: zemin (çim/toprak), ikon (mama kabı/köpek),
    // işaret (yanlış kap çarpısı) ve rakam. Kendi küçük animasyonlarını oynatır.
    public sealed class TileView : MonoBehaviour
    {
        public const int BaseOrder = 0;
        public const int IconOrder = 1;
        public const int MarkOrder = 2;
        public const int LabelOrder = 3;

        const float BowlScale = 0.84f;
        const float DogScale = 1.0f;
        const float PressedScale = 0.92f;

        IGameArt art;
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

        public Cell Cell { get; private set; }

        public static TileView Create(Transform parent, IGameArt art)
        {
            var go = new GameObject("Kare");
            go.transform.SetParent(parent, false);
            var tile = go.AddComponent<TileView>();
            tile.art = art;

            tile.body = new GameObject("Gövde").transform;
            tile.body.SetParent(go.transform, false);
            tile.ground = AddRenderer(tile.body, "Zemin", BaseOrder);
            tile.icon = AddRenderer(tile.body, "İkon", IconOrder);
            tile.mark = AddRenderer(tile.body, "İşaret", MarkOrder);

            var labelGo = new GameObject("Rakam");
            labelGo.transform.SetParent(tile.body, false);
            labelGo.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            tile.label = labelGo.AddComponent<TextMesh>();
            tile.label.font = art.Font;
            tile.label.fontSize = 64;
            tile.label.characterSize = 0.1f;
            tile.label.anchor = TextAnchor.MiddleCenter;
            tile.label.alignment = TextAlignment.Center;
            tile.label.fontStyle = FontStyle.Bold;
            var labelRenderer = labelGo.GetComponent<MeshRenderer>();
            labelRenderer.sharedMaterial = art.Font.material;
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
            name = $"Kare {cell.X},{cell.Y}";
            transform.localPosition = localPosition;
            StopMotion();
            StopIconMotion();
            pressScale = 1f;
            ApplyScale();
        }

        // ---------- Görünüm durumları ----------

        public void ShowGrass(bool alternate, bool bowl)
        {
            SetGround(art.TileGrass, alternate ? Palette.GrassB : Palette.GrassA);
            SetIcon(bowl ? art.Bowl : null, BowlScale, Color.white);
            SetMark(null);
            label.text = "";
        }

        public void ShowSoil(bool alternate, int nearbyDogs)
        {
            SetGround(art.TileSoil, alternate ? Palette.SoilB : Palette.SoilA);
            SetIcon(null, 1f, Color.white);
            SetMark(null);
            label.text = nearbyDogs > 0 ? nearbyDogs.ToString() : "";
            label.color = Palette.Numbers[Mathf.Clamp(nearbyDogs, 0, Palette.Numbers.Length - 1)];
        }

        public void ShowWokenDog()
        {
            SetGround(art.TileSoil, Palette.WokenSoil);
            SetIcon(art.DogAwake, DogScale, Color.white);
            SetMark(null);
            label.text = "";
        }

        // Kaybedince: köpek olmayan yere konmuş mama kabı.
        public void ShowWrongBowl()
        {
            SetGround(art.TileGrass, Palette.WrongBowl);
            SetIcon(art.Bowl, BowlScale, Palette.WithAlpha(Color.white, 0.45f));
            SetMark(art.IconCross);
            label.text = "";
        }

        public void SetPressed(bool pressed)
        {
            pressScale = pressed ? PressedScale : 1f;
            ground.color = pressed ? Color.Lerp(groundColor, Color.white, 0.25f) : groundColor;
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

        void SetMark(Sprite sprite)
        {
            mark.sprite = sprite;
            mark.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        }

        void ApplyScale()
        {
            float s = motionScale * pressScale;
            body.localScale = new Vector3(s, s, 1f);
        }
    }
}
