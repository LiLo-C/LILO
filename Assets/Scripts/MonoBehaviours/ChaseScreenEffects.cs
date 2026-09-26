using Lilo.MonoBehaviours.Monster;
using UnityEngine;

namespace Lilo.UI
{
    /// <summary>Additive camera shake for office gameplay.</summary>
    [DisallowMultipleComponent]
    public sealed class ChaseScreenEffects : MonoBehaviour
    {
        private const float ImpactDuration = 0.42f;
        private const float ImpactShake = 0.22f;
        private const float SustainedShake = 0.035f;
        private MonsterAIController _monster;
        private UnityEngine.Camera _camera;
        private bool _wasChasing;
        private float _impactShakeRemaining;

        public Vector3 CameraPositionOffset { get; private set; }

        private void Update()
        {
            if (_monster == null)
                _monster = FindAnyObjectByType<MonsterAIController>();
            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            bool chasing = _monster != null && _monster.CurrentState == Lilo.State.MonsterState.Chase;
            if (chasing && !_wasChasing)
                _impactShakeRemaining = ImpactDuration;
            _wasChasing = chasing;

            float dt = Time.unscaledDeltaTime;
            _impactShakeRemaining = Mathf.Max(0f, _impactShakeRemaining - dt);
            float impact = ImpactShake * Mathf.SmoothStep(0f, 1f, _impactShakeRemaining / ImpactDuration);
            float shakeStrength = (chasing ? SustainedShake : 0f) + impact;
            float t = Time.unscaledTime * 28f;
            Vector2 noise = new Vector2(
                Mathf.PerlinNoise(t, 0.37f) - 0.5f,
                Mathf.PerlinNoise(0.73f, t) - 0.5f) * 2f;
            CameraPositionOffset = _camera != null
                ? (_camera.transform.right * noise.x + _camera.transform.up * noise.y) * shakeStrength
                : Vector3.zero;

        }
    }
}
