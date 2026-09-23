using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.Systems.Flashlight;
using UnityEngine;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>
    /// Thin per-frame adapter: drains the installed battery (002), derives state/target radius
    /// (001), eases the displayed radius (003), and applies it to the real flashlight Light plus
    /// the readability fill (006) and shadow toggle (005). No gameplay math of its own.
    /// </summary>
    public class LightingRig : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Light flashlightLight;
        [SerializeField] private Light readabilityFillLight;

        private float _displayedRadius;
        private float _standaloneCharge;

        public FlashlightLightState CurrentState { get; private set; }
        public float ChargeFraction { get; private set; }
        public float CurrentIntensity { get; private set; }
        public float DisplayedRadius => _displayedRadius;

        private void Start()
        {
            ResolveConfig();
            if (config == null)
                return;

            _standaloneCharge = config.batteryDuration;
            _displayedRadius = config.flashlightNormalRadius;
            CurrentIntensity = LightStateSystem.GetTargetIntensity(1f, config);
            ApplyLights();
        }

        private void Update()
        {
            ResolveConfig();
            var gm = GameManager.Instance;
            if (config == null)
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
                // 008 (auto variant): lamp empty + spare carried refills immediately
                // so taken batteries always extend light. No action button needed.
                if (newCharge <= 0f && gm.State.SpareBatterySlotOccupied)
                {
                    gm.State.InstallSpareBattery(config.batteryDuration);
                    newCharge = config.batteryDuration;
                    Debug.Log("[Battery] Spare installed — charge refilled.");
                }
                gm.State.SetInstalledBatteryCharge(newCharge);
            }
            else
                _standaloneCharge = newCharge;

            ChargeFraction = config.batteryDuration > 0f ? newCharge / config.batteryDuration : 0f;

            // 001: derive state + target radius from the fraction alone.
            CurrentState = LightStateSystem.GetState(ChargeFraction, config);
            float target = LightStateSystem.GetTargetRadius(ChargeFraction, config);
            CurrentIntensity = LightStateSystem.GetTargetIntensity(ChargeFraction, config);

            // 003: ease the displayed radius toward that target — never snap.
            _displayedRadius = RadiusEasingSystem.Ease(_displayedRadius, target, config.lightRadiusEaseRate, Time.deltaTime);

            ApplyLights();
        }

        private void ResolveConfig()
        {
            if (config == null && GameManager.Instance != null)
                config = GameManager.Instance.Config;
        }

        private void ApplyLights()
        {
            if (config == null)
                return;

            if (flashlightLight != null)
            {
                flashlightLight.range = _displayedRadius;
                flashlightLight.intensity = CurrentIntensity;
                flashlightLight.shadows = config.shadowsEnabled ? LightShadows.Soft : LightShadows.None;
            }

            if (readabilityFillLight != null)
                readabilityFillLight.intensity = Mathf.Max(0f, config.readabilityFillIntensity);
        }
    }
}
