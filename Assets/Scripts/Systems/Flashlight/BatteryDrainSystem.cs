using UnityEngine;

namespace Lilo.Systems.Flashlight
{
    /// <summary>
    /// Pure real-time drain math (spec 002 FR-001/FR-002/FR-007/FR-010). No player-action
    /// parameter exists by construction — sprint/hide/interact cannot affect drain rate.
    /// </summary>
    public static class BatteryDrainSystem
    {
        /// <summary>Drains by exactly elapsedSeconds, clamped to [0, batteryDuration].</summary>
        public static float Drain(float currentChargeSeconds, float elapsedSeconds, float batteryDuration)
        {
            float next = currentChargeSeconds - Mathf.Max(0f, elapsedSeconds);
            return Mathf.Clamp(next, 0f, batteryDuration);
        }
    }
}
