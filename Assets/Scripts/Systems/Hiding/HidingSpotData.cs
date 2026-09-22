using UnityEngine;

namespace Lilo.Systems.Hiding
{
    /// <summary>
    /// Readonly representation of a hiding spot's geometric anchors and occupation status (spec 001 FR-001, FR-003).
    /// </summary>
    public readonly struct HidingSpotData
    {
        public readonly bool IsValid;
        public readonly Vector3 HideAnchor;
        public readonly Vector3 ExitAnchor;
        public readonly bool IsOccupied;

        public HidingSpotData(Vector3 hideAnchor, Vector3 exitAnchor, bool isOccupied = false)
        {
            IsValid = true;
            HideAnchor = hideAnchor;
            ExitAnchor = exitAnchor;
            IsOccupied = isOccupied;
        }

        public HidingSpotData WithOccupied(bool isOccupied)
        {
            return new HidingSpotData(HideAnchor, ExitAnchor, isOccupied);
        }
    }
}
