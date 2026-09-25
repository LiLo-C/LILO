using UnityEngine;

namespace Lilo.MonoBehaviours.Interaction
{
    /// <summary>
    /// Explicitly authored tabletop spawn point for the floor's access key.
    /// Only desks with this component are considered by <see cref="AccessKeyBootstrap"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AccessKeySpawnTable : MonoBehaviour
    {
        [SerializeField] private Transform surfacePoint;
        [SerializeField, Min(0.02f)] private float keyClearance = 0.12f;
        [SerializeField, Min(0.25f)] private float approachSearchRadius = 2f;
        [SerializeField] private Collider[] tabletopCollisionProxies = System.Array.Empty<Collider>();

        public Transform SurfacePoint => surfacePoint;
        public float KeyClearance => Mathf.Max(0.02f, keyClearance);
        public float ApproachSearchRadius => Mathf.Max(0.25f, approachSearchRadius);

        public bool IsTabletopCollisionProxy(Collider candidate)
        {
            if (candidate == null || tabletopCollisionProxies == null) return false;
            for (int i = 0; i < tabletopCollisionProxies.Length; i++)
                if (tabletopCollisionProxies[i] == candidate) return true;
            return false;
        }

        public bool TryGetKeyPosition(out Vector3 position)
        {
            if (surfacePoint == null)
            {
                position = default;
                return false;
            }

            position = surfacePoint.position + Vector3.up * KeyClearance;
            return true;
        }

        public void Configure(Transform authoredSurfacePoint, float clearance = 0.12f, Collider[] collisionProxies = null)
        {
            surfacePoint = authoredSurfacePoint;
            keyClearance = Mathf.Max(0.02f, clearance);
            tabletopCollisionProxies = collisionProxies ?? System.Array.Empty<Collider>();
        }
    }
}
