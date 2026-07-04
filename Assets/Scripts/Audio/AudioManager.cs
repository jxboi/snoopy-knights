using System.Collections.Generic;
using UnityEngine;

namespace SnoopyKnights.Audio
{
    public enum Sfx { Tap, Place, Complete, Arrow, Hit, Horn, Victory, Defeat, Tick, Objective }

    /// <summary>
    /// Placeholder audio: every clip is synthesized at startup (sine tones with
    /// decay), so the project ships zero audio assets. Survives scene reloads.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        const int SampleRate = 22050;

        static AudioManager instance;

        AudioSource source;
        readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
        readonly Dictionary<Sfx, float> lastPlay = new Dictionary<Sfx, float>();

        public static void Ensure()
        {
            if (instance != null) return;
            var go = new GameObject("Audio");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AudioManager>();
            instance.source = go.AddComponent<AudioSource>();
            instance.source.playOnAwake = false;
            go.AddComponent<AudioListener>(); // the camera carries none
            instance.BuildClips();
        }

        public static void Play(Sfx sfx)
        {
            if (instance != null) instance.PlayInternal(sfx);
        }

        void PlayInternal(Sfx sfx)
        {
            float minGap = sfx == Sfx.Hit ? 0.09f : 0.05f;
            if (lastPlay.TryGetValue(sfx, out float last) && Time.unscaledTime - last < minGap)
                return;
            lastPlay[sfx] = Time.unscaledTime;
            source.PlayOneShot(clips[sfx], 0.55f);
        }

        void BuildClips()
        {
            clips[Sfx.Tap] = Tone("tap", 880f, 880f, 0.05f);
            clips[Sfx.Place] = Tone("place", 190f, 150f, 0.18f);
            clips[Sfx.Complete] = Sequence("complete", (523f, 0.09f), (784f, 0.16f));
            clips[Sfx.Arrow] = Tone("arrow", 1400f, 650f, 0.07f);
            clips[Sfx.Hit] = Tone("hit", 150f, 110f, 0.09f);
            clips[Sfx.Horn] = Tone("horn", 320f, 180f, 0.7f);
            clips[Sfx.Victory] = Sequence("victory", (523f, 0.12f), (659f, 0.12f), (784f, 0.12f), (1047f, 0.3f));
            clips[Sfx.Defeat] = Sequence("defeat", (392f, 0.15f), (311f, 0.15f), (262f, 0.35f));
            clips[Sfx.Tick] = Tone("tick", 1250f, 1250f, 0.04f);
            clips[Sfx.Objective] = Sequence("objective", (659f, 0.08f), (988f, 0.18f));
        }

        static AudioClip Tone(string name, float freqStart, float freqEnd, float seconds)
        {
            var data = Synth(freqStart, freqEnd, seconds);
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Sequence(string name, params (float freq, float dur)[] notes)
        {
            var parts = new List<float[]>();
            int total = 0;
            foreach (var (freq, dur) in notes)
            {
                var d = Synth(freq, freq, dur);
                parts.Add(d);
                total += d.Length;
            }
            var all = new float[total];
            int offset = 0;
            foreach (var p in parts)
            {
                p.CopyTo(all, offset);
                offset += p.Length;
            }
            var clip = AudioClip.Create(name, all.Length, 1, SampleRate, false);
            clip.SetData(all, 0);
            return clip;
        }

        static float[] Synth(float freqStart, float freqEnd, float seconds)
        {
            int n = Mathf.Max(1, (int)(seconds * SampleRate));
            var data = new float[n];
            double phase = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float freq = Mathf.Lerp(freqStart, freqEnd, t);
                phase += 2.0 * System.Math.PI * freq / SampleRate;
                float envelope = Mathf.Exp(-4.5f * t);
                data[i] = (float)System.Math.Sin(phase) * envelope;
            }
            return data;
        }
    }
}
