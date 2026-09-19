using UnityEngine;

namespace Lilo.Systems.Collision
{
    public readonly struct CollisionResult
    {
        public Vector2 Position { get; }
        public bool Collided { get; }
        public int ResolutionCount { get; }

        public CollisionResult(Vector2 position, bool collided, int resolutionCount = 0)
        {
            Position = position;
            Collided = collided;
            ResolutionCount = resolutionCount;
        }
    }
}
