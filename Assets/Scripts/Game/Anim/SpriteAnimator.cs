using System;
using UnityEngine;
using UnityEngine.UI;

namespace UyuyanKopekler.Anim
{
    // Kare kare animasyon oynatır: SpriteRenderer (dünya) ya da Image (arayüz) üzerinde çalışır.
    public sealed class SpriteAnimator : MonoBehaviour
    {
        Action<Sprite> apply;
        Sprite[] frames;
        float fps;
        bool loop;
        float clock;
        int index;
        Action done;

        public bool IsPlaying => frames != null;

        public static SpriteAnimator For(SpriteRenderer renderer)
        {
            // Not: Unity nesnelerinde ?? kullanılmaz (editörde "sahte null" döner).
            if (!renderer.TryGetComponent<SpriteAnimator>(out var animator)) animator = renderer.gameObject.AddComponent<SpriteAnimator>();
            animator.apply = s => renderer.sprite = s;
            return animator;
        }

        public static SpriteAnimator For(Image image)
        {
            if (!image.TryGetComponent<SpriteAnimator>(out var animator)) animator = image.gameObject.AddComponent<SpriteAnimator>();
            animator.apply = s => image.sprite = s;
            return animator;
        }

        // Kareleri baştan oynatır. loop değilse son karede durur ve onDone çağrılır.
        public void Play(Sprite[] clip, float framesPerSecond, bool repeat, Action onDone = null)
        {
            frames = clip != null && clip.Length > 0 ? clip : null;
            fps = Mathf.Max(0.1f, framesPerSecond);
            loop = repeat;
            done = onDone;
            clock = 0f;
            index = 0;
            if (frames != null) apply(frames[0]);
        }

        // Rastgele bir kareden başlatır (yan yana duran köpekler aynı anda nefes almasın).
        public void PlayFromRandomFrame(Sprite[] clip, float framesPerSecond)
        {
            Play(clip, framesPerSecond, true);
            if (frames == null) return;
            index = UnityEngine.Random.Range(0, frames.Length);
            clock = UnityEngine.Random.value / fps;
            apply(frames[index]);
        }

        public void Stop()
        {
            frames = null;
            done = null;
        }

        void Update()
        {
            if (frames == null) return;
            clock += Time.deltaTime;
            float step = 1f / fps;
            while (clock >= step && frames != null)
            {
                clock -= step;
                if (index + 1 < frames.Length)
                {
                    index++;
                    apply(frames[index]);
                }
                else if (loop)
                {
                    index = 0;
                    apply(frames[0]);
                }
                else
                {
                    var callback = done;
                    frames = null;
                    done = null;
                    callback?.Invoke();
                }
            }
        }
    }
}
