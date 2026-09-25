using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.Systems.Flashlight;
using Lilo.Systems.GameLoop;
using UnityEngine;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>
    /// Drains the installed battery, derives the light level, and applies synchronized
    /// random lamp flickers with one audio cue for each visible dip.
    /// </summary>
    public class LightingRig : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Light flashlightLight;
        [SerializeField] private Light readabilityFillLight;
        [SerializeField] private AudioClip flickerClip;

        private float _displayedRadius;
        private float _standaloneCharge;
        private float _steadyIntensity;
        private float _flickerMultiplier = 1f;
        private float _secondsUntilFlicker;
        private float _flickerElapsed;
        private float _flickerDepth;
        private AudioSource _flickerAudioSource;
        private bool _flickerActive;
        private bool _initialized;

        public FlashlightLightState CurrentState { get; private set; }
        public float ChargeFraction { get; private set; }
        public float CurrentIntensity => _steadyIntensity * _flickerMultiplier;
        public float DisplayedRadius => _displayedRadius;
        public Light PlayerFlashlight => flashlightLight;

        private void Start()
        {
            InitializeLighting();
        }

        /// <summary>Sets up the held lamp before the intro suspends gameplay behaviours.</summary>
        public void InitializeForCinematic()
        {
            InitializeLighting();
        }

        private void InitializeLighting()
        {
            if (_initialized)
                return;

            ResolveConfig();
            if (config == null)
                return;

            ConfigurePlayerFlashlight();
            ConfigureFlickerAudio();
            _standaloneCharge = config.batteryDuration;
            _displayedRadius = config.flashlightNormalRadius;
            _steadyIntensity = LightStateSystem.GetTargetIntensity(1f, config);
            ChargeFraction = 1f;
            ApplyLights();
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized)
                InitializeLighting();
            var gm = GameManager.Instance;
            if (!_initialized)
            {
                return;
            }

            // 002: drain in real wall-clock time, no player-action parameter.
            float currentCharge = gm != null && gm.State != null
                ? gm.State.InstalledBatteryCharge
                : _standaloneCharge;
            float newCharge = BatteryDrainSystem.Drain(currentCharge, Time.deltaTime, config.batteryDuration);
            if (gm != null && gm.State != null)
            {
                gm.State.SetInstalledBatteryCharge(newCharge);
            }
            else
                _standaloneCharge = newCharge;

            ChargeFraction = config.batteryDuration > 0f ? newCharge / config.batteryDuration : 0f;

            // 001: derive state + target radius from the fraction alone.
            CurrentState = LightStateSystem.GetState(ChargeFraction, config);
            float target = LightStateSystem.GetTargetRadius(ChargeFraction, config);
            _steadyIntensity = LightStateSystem.GetTargetIntensity(ChargeFraction, config);

            // 003: ease the displayed radius toward that target — never snap.
            _displayedRadius = RadiusEasingSystem.Ease(_displayedRadius, target, config.lightRadiusEaseRate, Time.deltaTime);

            UpdateFlicker();
            ApplyLights();
        }

        private void OnDisable()
        {
            CancelFlicker();
            _secondsUntilFlicker = 0f;
            ApplyLights();
        }

        private void ResolveConfig()
        {
            if (config == null && GameManager.Instance != null)
                config = GameManager.Instance.Config;
        }

        private void ConfigurePlayerFlashlight()
        {
            if (flashlightLight == null)
                return;

            // Bind the light to the animated hand so its glow follows the held lamp instead of
            // staying at a fixed offset on the PlayerCharacter root.
            Animator animator = GetComponent<Animator>();
            Transform hand = null;
            if (animator != null && animator.avatar != null && animator.avatar.isHuman)
            {
                hand = animator.GetBoneTransform(HumanBodyBones.LeftHand)
                    ?? animator.GetBoneTransform(HumanBodyBones.RightHand);
            }

            if (hand != null)
            {
                flashlightLight.transform.SetParent(hand, false);
                flashlightLight.transform.localPosition = new Vector3(0f, -0.03f, 0.05f);
                flashlightLight.transform.localRotation = Quaternion.identity;
            }

            // The lamp is a local point source around the held lamp prop, not a forward-facing beam.
            flashlightLight.type = LightType.Point;
        }

        private void ConfigureFlickerAudio()
        {
            if (flickerClip == null)
                flickerClip = Resources.Load<AudioClip>("Audio/light-flicker-single");
            if (flickerClip == null)
            {
                Debug.LogWarning("[LightingRig] Flicker audio is missing; lamp flicker is disabled.", this);
                return;
            }
            if (flashlightLight == null)
                return;

            _flickerAudioSource = gameObject.AddComponent<AudioSource>();
            _flickerAudioSource.playOnAwake = false;
            _flickerAudioSource.loop = false;
            _flickerAudioSource.spatialBlend = 0f;
            _flickerAudioSource.clip = flickerClip;
        }

        private void UpdateFlicker()
        {
            // One audio event starts with one visible dip. If either half is unavailable,
            // keep the lamp steady and stop any sound already in progress.
            if (Time.timeScale <= 0f || ChargeFraction <= 0f || flashlightLight == null ||
                flickerClip == null || _flickerAudioSource == null)
            {
                CancelFlicker();
                _secondsUntilFlicker = 0f;
                return;
            }

            if (_flickerActive)
            {
                if (!_flickerAudioSource.isPlaying || _flickerElapsed >= flickerClip.length)
                {
                    CancelFlicker();
                    ScheduleNextFlicker();
                    return;
                }

                _flickerElapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(_flickerElapsed / flickerClip.length);
                float recovery = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.45f, 1f, progress));
                _flickerMultiplier = 1f - _flickerDepth * (1f - 0.9f * recovery);
                return;
            }

            if (_secondsUntilFlicker <= 0f)
                ScheduleNextFlicker();
            _secondsUntilFlicker -= Time.deltaTime;
            if (_secondsUntilFlicker > 0f)
                return;

            float depletion = 1f - Mathf.Clamp01(ChargeFraction);
            _flickerDepth = Mathf.Lerp(config.flickerDimFullBattery, config.flickerDimLowBattery, depletion);
            _flickerElapsed = 0f;
            _flickerMultiplier = 1f - _flickerDepth;
            _flickerAudioSource.volume = Mathf.Lerp(0.45f, 0.85f, depletion) * SoundSettingsStore.Effects;
            _flickerAudioSource.Play();
            _flickerActive = true;
        }

        private void ScheduleNextFlicker()
        {
            if (config == null)
                return;

            float depletion = 1f - Mathf.Clamp01(ChargeFraction);
            float frequencyRamp = depletion * depletion;
            float minimum = Mathf.Lerp(config.flickerIntervalFullBattery.x,
                config.flickerIntervalLowBattery.x, frequencyRamp);
            float maximum = Mathf.Lerp(config.flickerIntervalFullBattery.y,
                config.flickerIntervalLowBattery.y, frequencyRamp);
            _secondsUntilFlicker = Random.Range(minimum, maximum);
        }

        private void CancelFlicker()
        {
            _flickerActive = false;
            _flickerMultiplier = 1f;
            if (_flickerAudioSource != null && _flickerAudioSource.isPlaying)
                _flickerAudioSource.Stop();
        }

        private void ApplyLights()
        {
            if (config == null)
                return;

            if (flashlightLight != null)
            {
                flashlightLight.enabled = true;
                flashlightLight.range = _displayedRadius;
                flashlightLight.intensity = CurrentIntensity;
                flashlightLight.shadows = config.shadowsEnabled ? LightShadows.Soft : LightShadows.None;
            }

            // Keep one gameplay light source. The directional readability fill washed the whole
            // character and room evenly, bypassing battery-driven falloff.
            if (readabilityFillLight != null)
                readabilityFillLight.enabled = false;
        }
    }
}
