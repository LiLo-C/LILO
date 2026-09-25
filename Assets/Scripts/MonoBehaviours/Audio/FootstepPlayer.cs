using UnityEngine;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// Distance-driven footstep player: every strideLength meters walked, plays
    /// one random step clip at full 2D volume. Replaces the stock
    /// ThirdPersonController footstep path (3D at feet, eaten by camera
    /// distance) — keep that array empty while this is active.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FootstepPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] stepClips = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [Tooltip("Meters walked per step. Sprint covers more ground, steps faster.")]
        [SerializeField] private float strideLength = 0.7f;
        [Tooltip("Below this speed no steps play.")]
        [SerializeField] private float minMoveSpeed = 0.3f;

        private AudioSource _source;
        private CharacterController _controller;
        private Vector3 _lastPosition;
        private float _distance;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.volume = volume;

            _lastPosition = transform.position;
        }

        private void Update()
        {
            Vector3 pos = transform.position;
            float moved = Vector3.Distance(
                new Vector3(pos.x, 0f, pos.z),
                new Vector3(_lastPosition.x, 0f, _lastPosition.z));
            _lastPosition = pos;

            bool grounded = _controller == null || _controller.isGrounded;
            if (!grounded || moved / Mathf.Max(Time.deltaTime, 0.0001f) < minMoveSpeed)
            {
                _distance = 0f;
                return;
            }

            _distance += moved;
            while (_distance >= strideLength)
            {
                _distance -= strideLength;
                PlayStep();
            }
        }

        private void PlayStep()
        {
            if (stepClips == null || stepClips.Length == 0) return;

            var clip = stepClips[Random.Range(0, stepClips.Length)];
            if (clip == null) return;

            _source.volume = volume;
            _source.PlayOneShot(clip);
        }
    }
}
