using UnityEngine;

namespace Lilo.Systems.Collision
{
    public readonly struct CollisionResult
    {
        public CollisionResult(Vector2 correctedPosition, bool didResolve)
        {
            CorrectedPosition = correctedPosition;
            DidResolve = didResolve;
        }

        public Vector2 CorrectedPosition { get; }
        public bool DidResolve { get; }
    }
}
