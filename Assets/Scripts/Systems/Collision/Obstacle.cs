using UnityEngine;

namespace Lilo.Systems.Collision
{
    /// <summary>A plain-data, axis-aligned obstacle projected onto the XZ movement plane.</summary>
    public readonly struct Obstacle
    {
        public Vector2 Min { get; }
        public Vector2 Max { get; }

        public bool IsValid =>
            IsFinite(Min.x) && IsFinite(Min.y) &&
            IsFinite(Max.x) && IsFinite(Max.y) &&
            Min.x < Max.x && Min.y < Max.y;

        public Obstacle(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
