using System.Collections.Generic;
using UnityEngine;

namespace Lilo.Systems.Collision
{
    /// <summary>
    /// Pure circle-vs-axis-aligned-rectangle push-out math. It deliberately does not use
    /// Collider, Rigidbody, callbacks, or a physics step.
    /// </summary>
    public static class CollisionResolver
    {
        private const float Epsilon = 0.000001f;
        private const int MaxResolutionPasses = 4;

        public static CollisionResult ResolveSingle(Vector2 playerPosition, float playerRadius, Obstacle obstacle)
        {
            if (!obstacle.IsValid || playerRadius <= 0f || float.IsNaN(playerRadius) || float.IsInfinity(playerRadius))
                return new CollisionResult(playerPosition, false);

            Vector2 closest = new Vector2(
                Mathf.Clamp(playerPosition.x, obstacle.min.x, obstacle.max.x),
                Mathf.Clamp(playerPosition.y, obstacle.min.y, obstacle.max.y));
            Vector2 offset = playerPosition - closest;
            float distanceSquared = offset.sqrMagnitude;
            float radiusSquared = playerRadius * playerRadius;

            // Outside the rectangle: only a circle whose edge penetrates the rectangle needs push-out.
            if (distanceSquared > Epsilon)
            {
                if (distanceSquared >= radiusSquared)
                    return new CollisionResult(playerPosition, false);

                float distance = Mathf.Sqrt(distanceSquared);
                Vector2 corrected = closest + offset / distance * playerRadius;
                return new CollisionResult(corrected, true);
            }

            // The center is inside the rectangle. Pick the nearest face deterministically and put the
            // circle exactly outside it; this is the nearest valid recovery for an overlapping spawn.
            float left = playerPosition.x - obstacle.min.x;
            float right = obstacle.max.x - playerPosition.x;
            float bottom = playerPosition.y - obstacle.min.y;
            float top = obstacle.max.y - playerPosition.y;

            float minimum = left;
            int axis = 0;
            if (right < minimum) { minimum = right; axis = 1; }
            if (bottom < minimum) { minimum = bottom; axis = 2; }
            if (top < minimum) { minimum = top; axis = 3; }

            Vector2 insideCorrected = playerPosition;
            switch (axis)
            {
                case 0: insideCorrected.x = obstacle.min.x - playerRadius; break;
                case 1: insideCorrected.x = obstacle.max.x + playerRadius; break;
                case 2: insideCorrected.y = obstacle.min.y - playerRadius; break;
                default: insideCorrected.y = obstacle.max.y + playerRadius; break;
            }

            return new CollisionResult(insideCorrected, true);
        }

        public static CollisionResult ResolveMany(Vector2 playerPosition, float playerRadius, IReadOnlyList<Obstacle> obstacles)
        {
            Vector2 corrected = playerPosition;
            bool didResolve = false;
            if (obstacles == null)
                return new CollisionResult(corrected, false);

            for (int pass = 0; pass < MaxResolutionPasses; pass++)
            {
                bool passChanged = false;
                for (int i = 0; i < obstacles.Count; i++)
                {
                    CollisionResult result = ResolveSingle(corrected, playerRadius, obstacles[i]);
                    if (!result.DidResolve)
                        continue;

                    corrected = result.CorrectedPosition;
                    passChanged = true;
                    didResolve = true;
                }

                if (!passChanged)
                    break;
            }

            return new CollisionResult(corrected, didResolve);
        }
    }
}
