using UnityEngine;

namespace Lilo.Systems.Camera
{
    /// <summary>
    /// Pure, engine-lifecycle-independent follow + clamp math (spec FR-008). No MonoBehaviour/
    /// Component/scene dependency — CameraFollowController is the only place a real Camera or
    /// Transform is ever touched.
    /// </summary>
    public static class CameraFollowSystem
    {
        /// <summary>Frame-rate-independent smoothing so lerpFactor means the same thing at any fps (FR-001).</summary>
        public static Vector2 Lerp(Vector2 current, Vector2 target, float lerpFactor, float deltaTime)
        {
            float t = 1f - Mathf.Exp(-lerpFactor * deltaTime * 60f);
            return Vector2.Lerp(current, target, Mathf.Clamp01(t));
        }

        /// <summary>Per-axis clamp; centers on an axis too narrow for the viewport; skips a malformed axis (FR-004/FR-007).</summary>
        public static Vector2 ClampToBounds(Vector2 position, Vector2 halfExtent, LevelBounds bounds, float boundsInset)
        {
            float x = ClampAxis(position.x, halfExtent.x, bounds.Min.x, bounds.Max.x, boundsInset, bounds.IsValidOnAxis(0));
            float y = ClampAxis(position.y, halfExtent.y, bounds.Min.y, bounds.Max.y, boundsInset, bounds.IsValidOnAxis(1));
            return new Vector2(x, y);
        }

        private static float ClampAxis(float value, float halfExtent, float min, float max, float inset, bool boundsValid)
        {
            if (!boundsValid)
            {
                return value; // malformed data — leave this axis unclamped rather than produce NaN/garbage
            }

            float reach = halfExtent + inset;
            float span = max - min;

            if (span < reach * 2f)
            {
                return (min + max) * 0.5f; // level narrower than the viewport on this axis — center instead
            }

            return Mathf.Clamp(value, min + reach, max - reach);
        }

        public static Vector2 Resolve(Vector2 currentPosition, Vector2 targetPosition, float lerpFactor, float deltaTime,
            Vector2 halfExtent, LevelBounds bounds, float boundsInset)
        {
            Vector2 eased = Lerp(currentPosition, targetPosition, lerpFactor, deltaTime);
            return ClampToBounds(eased, halfExtent, bounds, boundsInset);
        }

        /// <summary>Bypasses the lerp entirely for respawn/floor-load moments (FR-009).</summary>
        public static Vector2 SnapTo(Vector2 targetPosition, Vector2 halfExtent, LevelBounds bounds, float boundsInset)
        {
            return ClampToBounds(targetPosition, halfExtent, bounds, boundsInset);
        }
    }
}
