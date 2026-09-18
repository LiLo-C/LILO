using Lilo.Systems.Camera;
using UnityEngine;

namespace Lilo.MonoBehaviours.Camera
{
    /// <summary>Per-scene authored level bounds (spec FR-005 — each floor's footprint differs).</summary>
    public class LevelBoundsAuthoring : MonoBehaviour
    {
        [SerializeField] private Vector2 min = new Vector2(-10, -10);
        [SerializeField] private Vector2 max = new Vector2(10, 10);

        public LevelBounds Bounds => new LevelBounds(min, max);

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3((min.x + max.x) * 0.5f, 0f, (min.y + max.y) * 0.5f);
            Vector3 size = new Vector3(max.x - min.x, 0.1f, max.y - min.y);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
