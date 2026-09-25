using Lilo.Config;
using UnityEngine;

namespace Lilo.Systems.Monster
{
    /// <summary>
    /// Pure player-noise math (GDD Ch. 7.1, spec 002 FR-012). Movement noise is
    /// continuous; interaction/swap noise enters the brain as one-shot pulses.
    /// </summary>
    public static class MonsterNoise
    {
        public static float MovementRadius(GameConfig config, bool isHiding, bool isMoving, bool isSprinting)
        {
            if (config == null || isHiding || !isMoving)
                return 0f;

            float multiplier = isSprinting ? config.noiseSprint : config.noiseWalk;
            return config.noiseBaseRadius * multiplier;
        }

        /// <summary>True when a sound source falls inside the monster's rear blind cone.</summary>
        public static bool IsInRearBlindSpot(
            Vector3 observerPosition,
            Vector3 observerForward,
            Vector3 sourcePosition,
            float halfAngleDegrees)
        {
            Vector3 toSource = Vector3.ProjectOnPlane(sourcePosition - observerPosition, Vector3.up);
            Vector3 forward = Vector3.ProjectOnPlane(observerForward, Vector3.up);
            if (toSource.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f)
                return false;

            return Vector3.Angle(-forward.normalized, toSource.normalized)
                <= Mathf.Clamp(halfAngleDegrees, 0f, 180f);
        }
    }
}
