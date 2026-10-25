using System;
using System.Collections.Generic;
using UnityEngine;
using UyuyanKopekler.Core;

namespace UyuyanKopekler.Audio
{
    // Kısa sesleri küçük bir AudioSource havuzuyla çalar. Ayarlardan ses kapalıysa susar.
    // Aynı ses çok sık tetiklenirse (ör. aynı karede 20 kare açılınca) bir kez çalar.
    public sealed class SoundService : MonoBehaviour, ISoundPlayer
    {
        const int Voices = 8;
        const float MinRepeatSeconds = 0.05f;

        readonly Dictionary<Sfx, AudioClip[]> clips = new Dictionary<Sfx, AudioClip[]>();
        readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
        AudioSource[] sources;
        GameSettings settings;
        int next;

        public void Init(GameSettings gameSettings)
        {
            settings = gameSettings;
            sources = new AudioSource[Voices];
            for (int i = 0; i < Voices; i++)
            {
                sources[i] = gameObject.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0f;
            }
            foreach (Sfx sfx in Enum.GetValues(typeof(Sfx))) clips[sfx] = Load(sfx);
        }

        public void Play(Sfx sfx)
        {
            if (settings == null || !settings.Sound || sources == null) return;
            var options = clips[sfx];
            if (options.Length == 0) return;

            float now = Time.unscaledTime;
            if (lastPlayed.TryGetValue(sfx, out float last) && now - last < MinRepeatSeconds) return;
            lastPlayed[sfx] = now;

            var source = sources[next];
            next = (next + 1) % sources.Length;
            source.pitch = Varies(sfx) ? UnityEngine.Random.Range(0.92f, 1.08f) : 1f;
            source.volume = Volume(sfx);
            source.PlayOneShot(options[UnityEngine.Random.Range(0, options.Length)]);
        }

        static bool Varies(Sfx sfx) => sfx == Sfx.Dig || sfx == Sfx.Bark || sfx == Sfx.Bowl || sfx == Sfx.Collect;

        static float Volume(Sfx sfx) => sfx == Sfx.Dig || sfx == Sfx.Tap ? 0.7f : 1f;

        // Havlamanın iki çeşidi var (bark, bark2); diğerleri tek dosya.
        static AudioClip[] Load(Sfx sfx)
        {
            string name = sfx.ToString().ToLowerInvariant();
            var list = new List<AudioClip>();
            var main = Resources.Load<AudioClip>("Audio/" + name);
            if (main != null) list.Add(main);
            var second = Resources.Load<AudioClip>("Audio/" + name + "2");
            if (second != null) list.Add(second);
            if (list.Count == 0) Debug.LogWarning($"Ses bulunamadı: Resources/Audio/{name} — tools/audio/build.py çalıştırıldı mı?");
            return list.ToArray();
        }
    }
}
