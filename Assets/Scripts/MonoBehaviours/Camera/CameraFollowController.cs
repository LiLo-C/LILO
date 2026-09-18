using Lilo.Config;
using Lilo.Systems.Camera;
using UnityEngine;

namespace Lilo.MonoBehaviours.Camera
{
    /// <summary>
    /// Thin adapter: calls CameraFollowSystem.Resolve every LateUpdate (after player movement's
    /// Update, per FR-010) and applies the ground-plane result to the camera rig's root position.
    /// </summary>
    public class CameraFollowController : MonoBehaviour, ICameraHalfExtentProvider
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Transform player;
        [SerializeField] private Transform cameraRig; // the yaw root; pitch/offset are its children
        [SerializeField] private UnityEngine.Camera trackedCamera;
        [SerializeField] private LevelBoundsAuthoring levelBounds;

        private Vector2 _currentPosition;

        private void Start()
        {
            _currentPosition = new Vector2(player.position.x, player.position.z);
        }

        // INTERIM — replace with movement-and-camera/004's CameraRigController once it exists.
        public Vector2 GetGroundHalfExtent()
        {
            float halfHeight = trackedCamera.orthographicSize;
            float halfWidth = halfHeight * trackedCamera.aspect;
            return new Vector2(halfWidth, halfHeight);
        }

        private void LateUpdate()
        {
            Vector2 target = new Vector2(player.position.x, player.position.z);
            LevelBounds bounds = levelBounds != null ? levelBounds.Bounds : new LevelBounds(Vector2.negativeInfinity, Vector2.positiveInfinity);

            _currentPosition = CameraFollowSystem.Resolve(
                _currentPosition, target, config.cameraFollowLerpFactor, Time.deltaTime,
                GetGroundHalfExtent(), bounds, config.cameraBoundsInset);

            ApplyToRig(_currentPosition);
        }

        /// <summary>Bypasses the lerp for respawn/floor-load moments (FR-009).</summary>
        public void SnapTo(Vector2 targetPosition)
        {
            LevelBounds bounds = levelBounds != null ? levelBounds.Bounds : new LevelBounds(Vector2.negativeInfinity, Vector2.positiveInfinity);
            _currentPosition = CameraFollowSystem.SnapTo(targetPosition, GetGroundHalfExtent(), bounds, config.cameraBoundsInset);
            ApplyToRig(_currentPosition);
        }

        private void ApplyToRig(Vector2 groundPosition)
        {
            Vector3 pos = cameraRig.position;
            cameraRig.position = new Vector3(groundPosition.x, pos.y, groundPosition.y);
        }
    }
}
