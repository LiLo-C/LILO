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
        private Collider[] _chairCollisionProxies;

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

        /// <summary>Coarse furniture collision boxes are not walls between a hand and this tabletop.</summary>
        public bool IsPickupReachProxy(Collider candidate)
        {
            if (IsTabletopCollisionProxy(candidate)) return true;
            if (candidate == null || transform.parent == null) return false;
            if (_chairCollisionProxies == null)
            {
                var chairs = new System.Collections.Generic.List<Collider>();
                // ChairCollider is the dedicated box proxy authored inside this
                // cubicle. Do not ignore walls or the cubicle's other colliders.
                foreach (Collider collider in transform.parent.GetComponentsInChildren<Collider>(true))
                    if (collider is BoxCollider && collider.name == "ChairCollider")
                        chairs.Add(collider);
                _chairCollisionProxies = chairs.ToArray();
            }
            foreach (Collider proxy in _chairCollisionProxies)
                if (candidate == proxy) return true;
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
