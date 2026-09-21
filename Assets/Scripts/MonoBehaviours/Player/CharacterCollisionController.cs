using System.Collections.Generic;
using Lilo.Config;
using Lilo.MonoBehaviours.Collision;
using Lilo.Systems.Collision;
using UnityEngine;

namespace Lilo.MonoBehaviours.Player
{
    /// <summary>
    /// Thin scene adapter. Player movement is applied in Update; this resolves the
    /// tentative XZ position in LateUpdate before the default-order camera follows it.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class CharacterCollisionController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private List<CollisionObstacleAuthoring> authoredObstacles = new List<CollisionObstacleAuthoring>();

        private readonly List<Obstacle> obstacles = new List<Obstacle>();

        private void Awake()
        {
            CacheObstacleBounds();
        }

        private void LateUpdate()
        {
            if (config == null || config.playerRadius <= 0f || obstacles.Count == 0)
                return;

            Vector3 current = transform.position;
            CollisionResult result = CollisionResolver.ResolveMany(
                new Vector2(current.x, current.z),
                config.playerRadius,
                obstacles);

            if (result.Collided)
                transform.position = new Vector3(result.Position.x, current.y, result.Position.y);
        }

        private void CacheObstacleBounds()
        {
            obstacles.Clear();
            for (int index = 0; index < authoredObstacles.Count; index++)
            {
                CollisionObstacleAuthoring authoring = authoredObstacles[index];
                if (authoring != null)
                    obstacles.Add(authoring.ToObstacle());
            }
        }
    }
}
