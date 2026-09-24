using Lilo.Config;
using UnityEngine;

namespace Lilo.Systems.Flashlight
{
    /// <summary>
    /// Pure derivation: charge fraction -> FlashlightLightState + target radius. No animation, no
    /// memory, no wall-clock time (spec 001 FR-001/FR-004/FR-010). Boundaries are
    /// upper-bound-inclusive: exactly at a band's top edge belongs to that band, never the one
    /// above it (GDD 5.2).
    /// </summary>
    public static class LightStateSystem
    {
        public static FlashlightLightState GetState(float chargeFraction, GameConfig config)
        {
            float charge = Mathf.Clamp01(chargeFraction);

            if (charge <= 0f)
                return FlashlightLightState.CompactDarkness;
            if (charge <= config.lightStateCriticalStart)
                return FlashlightLightState.Critical;
            if (charge <= config.lightStateFlickerStart)
                return FlashlightLightState.Flickering;
            return FlashlightLightState.Normal;
        }

        public static float GetTargetRadius(float chargeFraction, GameConfig config)
        {
            float charge = Mathf.Clamp01(chargeFraction);
            float compactRadius = config.flashlightNormalRadius * config.compactDarknessRadius;
            // Battery loss affects the usable beam across the whole charge range, rather than
            // keeping full reach until the critical state. The compact floor only matters while
            // charge remains; a zero-charge light has no useful beam.
            return Mathf.Lerp(compactRadius, config.flashlightNormalRadius, charge);
        }

        public static float GetTargetIntensity(float chargeFraction, GameConfig config)
        {
            float charge = Mathf.Clamp01(chargeFraction);
            // Zero charge must mean zero emitted light. A nonzero darkness floor made the lamp
            // continue illuminating the player and room after the HUD reached 0%.
            return config.flashlightNormalIntensity * charge;
        }
    }
}
