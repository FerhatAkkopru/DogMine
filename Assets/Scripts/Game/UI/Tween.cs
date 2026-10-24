using System;
using System.Collections;
using UnityEngine;

namespace UyuyanKopekler.UI
{
    // Coroutine tabanlı küçük animasyon yardımcıları. Time.timeScale'den etkilenmez.
    public static class Tween
    {
        // step(t) 0'dan 1'e kadar duration boyunca çağrılır; en sonda kesin olarak 1 ile çağrılır.
        public static IEnumerator Run(float duration, Action<float> step, float delay = 0f)
        {
            if (delay > 0f)
            {
                float start = Time.unscaledTime;
                while (Time.unscaledTime - start < delay) yield return null;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                step(elapsed / duration);
                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
            step(1f);
        }

        public static IEnumerator Wait(float seconds, Action then)
        {
            float start = Time.unscaledTime;
            while (Time.unscaledTime - start < seconds) yield return null;
            then();
        }

        public static float OutCubic(float t)
        {
            float u = 1f - t;
            return 1f - u * u * u;
        }

        // Hedefi biraz aşıp geri gelir ("pop" hissi).
        public static float OutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }

        // 0 → 1 → 0 yumuşak tümsek (zıplama, sallanma için).
        public static float Bump(float t) => Mathf.Sin(t * Mathf.PI);
    }
}
