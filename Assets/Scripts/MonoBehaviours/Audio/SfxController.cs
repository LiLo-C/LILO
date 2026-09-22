using UnityEngine;
using Lilo.Systems.GameLoop;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// One-shot SFX player for event-driven sounds (movement start, chase start).
    /// Each clip plays once per event; no looping, no stacking.
    /// </summary>
    public class SfxController : MonoBehaviour
    {
        [SerializeField] private AudioClip behindYouClip;
        [SerializeField] private AudioClip horrorChaseClip;
        [SerializeField] private AudioClip batteryPickupClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

        private AudioSource _source;
        private AudioClip _generatedBatteryPickupClip;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f; // 2D
        }

        public void PlayBehindYou()
        {
            if (behindYouClip != null)
                _source.PlayOneShot(behindYouClip, volume * SoundSettingsStore.Effects);
        }

        public void PlayHorrorChase()
        {
            if (horrorChaseClip != null)
                _source.PlayOneShot(horrorChaseClip, volume * SoundSettingsStore.Effects);
        }

        public void PlayBatteryPickup()
        {
            AudioClip clip = batteryPickupClip != null ? batteryPickupClip : GetGeneratedBatteryPickupClip();
            _source.PlayOneShot(clip, volume * SoundSettingsStore.Effects);
        }

        private AudioClip GetGeneratedBatteryPickupClip()
        {
            if (_generatedBatteryPickupClip != null)
                return _generatedBatteryPickupClip;

            const int sampleRate = 44100;
            const float duration = 0.18f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            _generatedBatteryPickupClip = AudioClip.Create("BatteryPickupBeep", sampleCount, 1, sampleRate, false);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = 1f - i / (float)sampleCount;
                samples[i] = Mathf.Sin(2f * Mathf.PI * 880f * t) * envelope * 0.35f;
            }
            _generatedBatteryPickupClip.SetData(samples, 0);
            return _generatedBatteryPickupClip;
        }
    }
}
