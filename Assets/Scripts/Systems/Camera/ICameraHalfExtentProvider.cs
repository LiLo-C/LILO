using UnityEngine;

namespace Lilo.Systems.Camera
{
    /// <summary>
    /// Seam for the camera's current visible ground-plane half-extent. Implemented for real by
    /// movement-and-camera/004's CameraRigController; until then an interim implementation
    /// computes it directly from orthographic size/aspect (spec FR-006, Assumptions).
    /// </summary>
    public interface ICameraHalfExtentProvider
    {
        Vector2 GetGroundHalfExtent();
    }
}
