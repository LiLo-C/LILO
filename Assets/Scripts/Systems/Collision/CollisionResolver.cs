using System.Collections.Generic;
using UnityEngine;

namespace Lilo.Systems.Collision
{
    /// <summary>Deterministic circle-versus-AABB correction with no physics-engine dependency.</summary>
    public static class CollisionResolver
    {
        private const float Epsilon = 0.000001f;

        public static CollisionResult ResolveSingle(Vector2 playerPosition, float playerRadius, Obstacle obstacle)
        {
            if (!IsFinite(playerPosition) || !IsFinite(playerRadius) || playerRadius <= 0f || !obstacle.IsValid)
                return new CollisionResult(playerPosition, false);

            Vector2 closest = new Vector2(
                Mathf.Clamp(playerPosition.x, obstacle.Min.x, obstacle.Max.x),
                Mathf.Clamp(playerPosition.y, obstacle.Min.y, obstacle.Max.y));
            Vector2 separation = playerPosition - closest;
            float distanceSquared = separation.sqrMagnitude;
            float radiusSquared = playerRadius * playerRadius;

            if (distanceSquared >= radiusSquared)
                return new CollisionResult(playerPosition, false);

            if (distanceSquared > Epsilon)
            {
                Vector2 corrected = closest + separation / Mathf.Sqrt(distanceSquared) * playerRadius;
                return new CollisionResult(corrected, true, 1);
            }

            // The centre is inside or exactly on the rectangle. Choose the nearest
            // face of the Minkowski-expanded rectangle with stable tie-breaking.
            float left = playerPosition.x - obstacle.Min.x + playerRadius;
            float right = obstacle.Max.x - playerPosition.x + playerRadius;
            float bottom = playerPosition.y - obstacle.Min.y + playerRadius;
            float top = obstacle.Max.y - playerPosition.y + playerRadius;

            Vector2 result = playerPosition;
            float minimum = left;
            result.x = obstacle.Min.x - playerRadius;

            if (right < minimum)
            {
                minimum = right;
                result = new Vector2(obstacle.Max.x + playerRadius, playerPosition.y);
            }
            if (bottom < minimum)
            {
                minimum = bottom;
                result = new Vector2(playerPosition.x, obstacle.Min.y - playerRadius);
            }
            if (top < minimum)
                result = new Vector2(playerPosition.x, obstacle.Max.y + playerRadius);

            return new CollisionResult(result, true, 1);
        }

        public static CollisionResult ResolveMany(
            Vector2 playerPosition,
            float playerRadius,
            IReadOnlyList<Obstacle> obstacles,
            int maxPasses = 8)
        {
            if (obstacles == null || obstacles.Count == 0 || maxPasses <= 0)
                return new CollisionResult(playerPosition, false);

            Vector2 position = playerPosition;
            int resolutionCount = 0;

            for (int pass = 0; pass < maxPasses; pass++)
            {
                bool changed = false;
                for (int index = 0; index < obstacles.Count; index++)
                {
                    CollisionResult result = ResolveSingle(position, playerRadius, obstacles[index]);
                    if (!result.Collided)
                        continue;

                    position = result.Position;
                    resolutionCount += result.ResolutionCount;
                    changed = true;
                }

                if (!changed)
                    break;
            }

            return new CollisionResult(position, resolutionCount > 0, resolutionCount);
        }

        private static bool IsFinite(Vector2 value) => IsFinite(value.x) && IsFinite(value.y);
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
