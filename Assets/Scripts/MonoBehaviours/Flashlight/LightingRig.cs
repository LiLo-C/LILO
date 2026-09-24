using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.Systems.Flashlight;
using UnityEngine;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>
    /// Thin per-frame adapter: drains the installed battery (002), derives state/target radius
    /// (001), eases the displayed radius (003), and applies it to the player flashlight and shadow
    /// toggle (005). No gameplay math of its own.
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
        public Light PlayerFlashlight => flashlightLight;

        private void Start()
        {
            ResolveConfig();
            if (config == null)
                return;

            ConfigurePlayerFlashlight();
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
