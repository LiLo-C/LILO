using UnityEngine;

namespace Lilo.Systems.Flashlight
{
    /// <summary>
    /// Pure exponential-decay ease toward a (possibly moving) target radius (spec 003 FR-002/FR-007).
    /// Never sets the displayed value directly to the target except as convergence's natural result.
    /// </summary>
    public static class RadiusEasingSystem
    {
        public static float Ease(float currentDisplayedRadius, float targetRadius, float easeRate, float deltaTime)
        {
            float dt = Mathf.Max(0f, deltaTime);
            float rate = Mathf.Max(0f, easeRate);
            float t = 1f - Mathf.Exp(-rate * dt); // in [0,1) for finite rate*dt; -> 1 as dt grows large
            return Mathf.Lerp(currentDisplayedRadius, targetRadius, t);
        }
    }
}
