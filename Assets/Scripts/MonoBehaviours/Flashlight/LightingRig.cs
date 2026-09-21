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

        public FlashlightLightState CurrentState { get; private set; }
        public float ChargeFraction { get; private set; }
        public float CurrentIntensity { get; private set; }
        public float DisplayedRadius => _displayedRadius;

        private void Start()
        {
            _displayedRadius = config.flashlightNormalRadius;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || config == null)
            {
                return;
            }

            // 002: drain in real wall-clock time, no player-action parameter.
            float newCharge = BatteryDrainSystem.Drain(gm.State.InstalledBatteryCharge, Time.deltaTime, config.batteryDuration);
            gm.State.SetInstalledBatteryCharge(newCharge);

            ChargeFraction = config.batteryDuration > 0f ? newCharge / config.batteryDuration : 0f;

            // 001: derive state + target radius from the fraction alone.
            CurrentState = LightStateSystem.GetState(ChargeFraction, config);
            float target = LightStateSystem.GetTargetRadius(ChargeFraction, config);
            CurrentIntensity = LightStateSystem.GetTargetIntensity(ChargeFraction, config);

            // 003: ease the displayed radius toward that target — never snap.
            _displayedRadius = RadiusEasingSystem.Ease(_displayedRadius, target, config.lightRadiusEaseRate, Time.deltaTime);

            if (flashlightLight != null)
            {
                flashlightLight.range = _displayedRadius;
                flashlightLight.intensity = CurrentIntensity;
                flashlightLight.shadows = config.shadowsEnabled ? LightShadows.Soft : LightShadows.None;
            }

            if (readabilityFillLight != null)
            {
                readabilityFillLight.intensity = config.readabilityFillIntensity;
            }
        }
    }
}
