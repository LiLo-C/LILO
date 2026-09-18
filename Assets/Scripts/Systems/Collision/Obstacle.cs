using System;
using UnityEngine;

namespace Lilo.Systems.Collision
{
    /// <summary>Author-authored, axis-aligned solid obstacle on the horizontal plane.</summary>
    [Serializable]
    public struct Obstacle
    {
        public Vector2 min;
        public Vector2 max;

        public Obstacle(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsValid => min.x < max.x && min.y < max.y;
    }
}
