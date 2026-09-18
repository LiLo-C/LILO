using UnityEngine;

namespace Lilo.Systems.Camera
{
    /// <summary>Per-level authored rectangular extent (spec FR-005 — not a shared GameConfig value).</summary>
    public readonly struct LevelBounds
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        public LevelBounds(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>False when Min exceeds Max on that axis (malformed data — Edge Case).</summary>
        public bool IsValidOnAxis(int axisIndex)
        {
            return axisIndex == 0 ? Min.x <= Max.x : Min.y <= Max.y;
        }
    }
}
