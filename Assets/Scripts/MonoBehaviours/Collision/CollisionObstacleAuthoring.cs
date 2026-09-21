using Lilo.Systems.Collision;
using UnityEngine;

namespace Lilo.MonoBehaviours.Collision
{
    /// <summary>
    /// Marks a static scene BoxCollider as authored collision data. The runtime
    /// resolver consumes the cached plain-data projection, never PhysX callbacks.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CollisionObstacleAuthoring : MonoBehaviour
    {
        private BoxCollider source;

        public Obstacle ToObstacle()
        {
            if (source == null)
                source = GetComponent<BoxCollider>();

            Bounds bounds = source.bounds;
            return new Obstacle(
                new Vector2(bounds.min.x, bounds.min.z),
                new Vector2(bounds.max.x, bounds.max.z));
        }
    }
}
