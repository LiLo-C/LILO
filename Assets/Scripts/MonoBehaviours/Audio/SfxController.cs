using System.Collections;
using UnityEngine;
using Lilo.Systems.GameLoop;
using Lilo.MonoBehaviours.Monster;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// One-shot SFX player for event-driven sounds (movement start, chase start).
    /// Each clip plays once per event; no looping, no stacking.
    /// </summary>
    public class SfxController : MonoBehaviour
    {
        [SerializeField] private AudioClip behindYouClip;
        [SerializeField] private AudioClip chaseBgmClip;
        // Kept for existing scenes created before chase audio moved to its own loop.
        [SerializeField, HideInInspector] private AudioClip horrorChaseClip;
        [SerializeField] private AudioClip batteryPickupClip;
        [SerializeField] private AudioClip playerCaughtClip;
        [SerializeField] private AudioClip keyPickupClip;
        [SerializeField] private AudioClip waterDispenserClip;
        [SerializeField] private AudioClip toiletFlushClip;
        [SerializeField] private AudioClip life2VoiceOverClip;
        [SerializeField] private AudioClip life1VoiceOverClip;
        [SerializeField] private AudioClip lastLifeVoiceOverClip;
        [SerializeField] private AudioClip needAwayVoiceOverClip;
        [SerializeField] private AudioClip batteryRunsOutVoiceOverClip;
        [SerializeField] private AudioClip needAccessKeyVoiceOverClip;
        [SerializeField, Min(0.05f)] private float chaseBgmFadeOutSeconds = 1.5f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.8f;
        [SerializeField, Min(0.1f)] private float monsterAudioMinDistance = 1.5f;
        [SerializeField, Min(1f)] private float monsterAudioMaxDistance = 24f;

        private AudioSource _source;
        private AudioSource _chaseBgmSource;
        private Coroutine _chaseBgmFadeRoutine;
        private AudioSource _voiceSource;
        private AudioSource _behindYouSpatialSource;
        private Transform _monsterAudioTransform;
        private AudioClip _generatedBatteryPickupClip;
        private float _lastWaterDispenserPlayTime = float.NegativeInfinity;
        private float _lastToiletFlushPlayTime = float.NegativeInfinity;

        private void Awake()
        {
            if (_source == null)
                _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f; // 2D

            if (_voiceSource == null)
                _voiceSource = gameObject.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
            _voiceSource.spatialBlend = 0f;
            _voiceSource.priority = 32;

            // Positional monster audio is kept on a child emitter so moving it
            // never moves the controller or the centered UI/VO source.
            var monsterAudioObject = new GameObject("MonsterSpatialAudio");
            monsterAudioObject.transform.SetParent(transform, false);
            _monsterAudioTransform = monsterAudioObject.transform;

            _chaseBgmSource = monsterAudioObject.AddComponent<AudioSource>();
            ConfigureMonsterAudioSource(_chaseBgmSource);
            _chaseBgmSource.loop = true;
            _chaseBgmSource.priority = 64;
            // Keep the music audible during a full chase while preserving the
            // monster's direction in 3D space. Other monster cues stay close-range.
            _chaseBgmSource.minDistance = Mathf.Max(monsterAudioMinDistance, 15f);
            _chaseBgmSource.maxDistance = Mathf.Max(_chaseBgmSource.minDistance, 60f);

            _behindYouSpatialSource = monsterAudioObject.AddComponent<AudioSource>();
            ConfigureMonsterAudioSource(_behindYouSpatialSource);
            _behindYouSpatialSource.loop = false;
        }

        private void ConfigureMonsterAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = monsterAudioMinDistance;
            source.maxDistance = Mathf.Max(monsterAudioMinDistance, monsterAudioMaxDistance);
            source.dopplerLevel = 0f;
        }

        public void PlayBehindYou()
        {
            var monster = Object.FindAnyObjectByType<MonsterAIController>();
            if (monster != null)
            {
                PlayBehindYou(monster.transform.position);
                return;
            }

            if (behindYouClip != null)
                _source.PlayOneShot(behindYouClip, volume * SoundSettingsStore.Effects);
        }

        public void PlayBehindYou(Vector3 monsterPosition)
        {
            SetMonsterAudioPosition(monsterPosition);
            if (behindYouClip != null && _behindYouSpatialSource != null)
                _behindYouSpatialSource.PlayOneShot(behindYouClip, volume * SoundSettingsStore.Effects);
        }

        public void PlayChaseBgm()
        {
            AudioClip clip = chaseBgmClip != null ? chaseBgmClip : horrorChaseClip;
            if (clip == null || _chaseBgmSource == null)
                return;

            if (_chaseBgmFadeRoutine != null)
            {
                StopCoroutine(_chaseBgmFadeRoutine);
                _chaseBgmFadeRoutine = null;
            }

            if (_chaseBgmSource.clip != clip)
                _chaseBgmSource.clip = clip;

            _chaseBgmSource.volume = SoundSettingsStore.Music;
            if (!_chaseBgmSource.isPlaying)
                _chaseBgmSource.Play();
        }

        public void StopChaseBgm()
        {
            if (_chaseBgmSource == null || !_chaseBgmSource.isPlaying || _chaseBgmFadeRoutine != null)
                return;

            _chaseBgmFadeRoutine = StartCoroutine(FadeOutChaseBgm());
        }

        public void PlayHorrorChase() => PlayChaseBgm();

        public void StopHorrorChase() => StopChaseBgm();

        private IEnumerator FadeOutChaseBgm()
        {
            float startVolume = _chaseBgmSource.volume;
            float duration = Mathf.Max(0.05f, chaseBgmFadeOutSeconds);
            float elapsed = 0f;

            while (elapsed < duration && _chaseBgmSource != null && _chaseBgmSource.isPlaying)
            {
                elapsed += Time.unscaledDeltaTime;
                _chaseBgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            if (_chaseBgmSource != null && _chaseBgmSource.isPlaying)
            {
                _chaseBgmSource.volume = 0f;
                _chaseBgmSource.Stop();
            }

            _chaseBgmFadeRoutine = null;
        }

        private void SetMonsterAudioPosition(Vector3 position)
        {
            if (_monsterAudioTransform != null)
                _monsterAudioTransform.position = position;
        }

        public void PlayBatteryPickup()
        {
            AudioClip clip = batteryPickupClip != null ? batteryPickupClip : GetGeneratedBatteryPickupClip();
            _source.PlayOneShot(clip, volume * SoundSettingsStore.Effects);
        }

        public void PlayPlayerCaught()
        {
            if (playerCaughtClip != null)
                _source.PlayOneShot(playerCaughtClip, volume * SoundSettingsStore.Effects);
        }
        public void PlayKeyPickup()
        {
            AudioClip clip = keyPickupClip != null ? keyPickupClip : batteryPickupClip;
            if (clip != null)
                _source.PlayOneShot(clip, volume * SoundSettingsStore.Effects);
        }

        public void PlayWaterDispenserPassBy()
        {
            if (waterDispenserClip == null || Time.unscaledTime - _lastWaterDispenserPlayTime < 2f)
                return;

            _lastWaterDispenserPlayTime = Time.unscaledTime;
            _source.PlayOneShot(waterDispenserClip, volume * SoundSettingsStore.Effects);
        }

        public void PlayToiletFlushPassBy()
        {
            if (toiletFlushClip == null || Time.unscaledTime - _lastToiletFlushPlayTime < 2f)
                return;

            _lastToiletFlushPlayTime = Time.unscaledTime;
            _source.PlayOneShot(toiletFlushClip, volume * SoundSettingsStore.Effects);
        }

        public bool PlayLifeRemainingVoiceOver(int livesRemaining)
        {
            AudioClip clip = livesRemaining switch
            {
                2 => life2VoiceOverClip,
                1 => life1VoiceOverClip,
                _ => lastLifeVoiceOverClip,
            };
            return PlayVoiceOver(clip);
        }

        public bool PlayLifeVoiceOverForSubtitle(string subtitle)
        {
            int livesRemaining = subtitle switch
            {
                "WAIT WHAT WAS THAT? WHAT IS HAPPENING?!" => 2,
                "I FELT IT ALL THROUGH MY SKIN OH GOD" => 1,
                "NO NO NO I DON'T WANT TO FEEL IT AGAIN" => 0,
                _ => -1,
            };

            return livesRemaining >= 0 && PlayLifeRemainingVoiceOver(livesRemaining);
        }

        public bool PlayBatteryRunsOutVoiceOver()
        {
            return PlayVoiceOver(batteryRunsOutVoiceOverClip);
        }

        public bool PlayNeedAccessKeyVoiceOver()
        {
            return PlayVoiceOver(needAccessKeyVoiceOverClip);
        }

        public bool PlayNeedAwayVoiceOver()
        {
            return PlayVoiceOver(needAwayVoiceOverClip);
        }

        private bool PlayVoiceOver(AudioClip clip)
        {
            if (clip == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
                return false;

            if (_voiceSource == null)
            {
                _voiceSource = gameObject.AddComponent<AudioSource>();
                _voiceSource.playOnAwake = false;
                _voiceSource.spatialBlend = 0f;
                _voiceSource.priority = 32;
            }

            float effectsVolume = SoundSettingsStore.Effects;
            if (effectsVolume <= 0f || !_voiceSource.enabled)
                return false;

            _voiceSource.PlayOneShot(clip, volume * effectsVolume);
            return true;
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
