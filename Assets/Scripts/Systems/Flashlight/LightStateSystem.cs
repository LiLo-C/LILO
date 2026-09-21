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
            FlashlightLightState state = GetState(charge, config);
            float compactRadius = config.flashlightNormalRadius * config.compactDarknessRadius;

            switch (state)
            {
                case FlashlightLightState.Normal:
                case FlashlightLightState.Flickering:
                    return config.flashlightNormalRadius;

                case FlashlightLightState.CompactDarkness:
                    return compactRadius;

                case FlashlightLightState.Critical:
                default:
                    // Continuous narrowing: normalRadius at charge==criticalStart, compactRadius as charge->0 (FR-006).
                    float t = config.lightStateCriticalStart > 0f ? charge / config.lightStateCriticalStart : 0f;
                    return Mathf.Lerp(compactRadius, config.flashlightNormalRadius, t);
            }
        }

        public static float GetTargetIntensity(float chargeFraction, GameConfig config)
        {
            float charge = Mathf.Clamp01(chargeFraction);
            float normal = config.flashlightNormalIntensity;
            float compact = config.flashlightCompactDarknessIntensity;

            return Mathf.Lerp(compact, normal, charge);
        }
    }
}
