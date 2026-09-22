using UnityEngine;
using Lilo.Systems.Hiding;

namespace Lilo.MonoBehaviours.Hiding
{
    /// <summary>
    /// In-scene authoring component for a hiding spot (e.g., under a desk, in a locker).
    /// Holds the hide anchor (position when hidden) and exit anchor (position when visible).
    /// </summary>
    public class HidingSpot : MonoBehaviour
    {
        [Tooltip("Transform representing where the player is placed when hidden.")]
        [SerializeField] private Transform hideAnchor;

        [Tooltip("Transform representing where the player emerges upon exiting.")]
        [SerializeField] private Transform exitAnchor;

        [Tooltip("Interaction distance threshold for entering this spot.")]
        [SerializeField] private float interactionRadius = 2.0f;

        public bool IsOccupied { get; private set; }

        public float InteractionRadius => interactionRadius;

        public Vector3 HidePosition => hideAnchor != null ? hideAnchor.position : transform.position;
        public Vector3 ExitPosition => exitAnchor != null ? exitAnchor.position : transform.position + transform.forward;

        public HidingSpotData ToData()
        {
            return new HidingSpotData(HidePosition, ExitPosition, IsOccupied);
        }

        public void SetOccupied(bool occupied)
        {
            IsOccupied = occupied;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);

            if (hideAnchor != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(hideAnchor.position, 0.3f);
            }
            if (exitAnchor != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(exitAnchor.position, 0.3f);
            }
        }
    }
}
