using UnityEngine;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>Plays a prop sound when the player passes close by its collider.</summary>
    [DisallowMultipleComponent]
    public sealed class PropPassBySfx : MonoBehaviour
    {
        public enum SoundType
        {
            WaterDispenser,
            ToiletPartition,
        }

        [SerializeField] private SoundType soundType;
        [SerializeField] private Collider proximityCollider;
        [SerializeField, Min(0.1f)] private float enterDistance = 1.25f;
        [SerializeField, Min(0.1f)] private float exitDistance = 1.8f;

        private Transform _player;
        private SfxController _sfx;
        private bool _wasNear;
        private bool _hasInitializedProximity;

        private void Awake()
        {
            if (proximityCollider == null)
                proximityCollider = GetComponent<Collider>();
            ResolveReferences();
        }

        private void Update()
        {
            if (_player == null || _sfx == null)
                ResolveReferences();
            if (_player == null || proximityCollider == null || _sfx == null)
                return;

            Vector3 nearestPoint = proximityCollider.ClosestPoint(_player.position);
            float threshold = _wasNear ? exitDistance : enterDistance;
            bool isNear = (_player.position - nearestPoint).sqrMagnitude <= threshold * threshold;

            // Scene entry can place the player inside a prop's pass-by radius.
            // Treat that initial overlap as the starting state, not as a pass-by;
            // the sound will play after the player leaves and re-enters the radius.
            if (!_hasInitializedProximity)
            {
                _wasNear = isNear;
                _hasInitializedProximity = true;
                return;
            }

            if (isNear && !_wasNear)
            {
                if (soundType == SoundType.WaterDispenser)
                    _sfx.PlayWaterDispenserPassBy();
                else
                    _sfx.PlayToiletFlushPassBy();
            }

            _wasNear = isNear;
        }

        private void ResolveReferences()
        {
            if (_player == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player == null)
                    player = GameObject.Find("PlayerCharacter");
                if (player != null)
                    _player = player.transform;
            }

            if (_sfx == null)
                _sfx = GameObject.Find("SfxController")?.GetComponent<SfxController>();
        }
    }
}
