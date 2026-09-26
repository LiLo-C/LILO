using UnityEngine;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// Plays one step per distance-based stride. The Starter Assets animation
    /// footsteps are disabled on the same character hierarchy to avoid doubles.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FootstepPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] stepClips = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
        [SerializeField] private float strideLength = 0.85f;
        [SerializeField] private float minMoveSpeed = 0.3f;

        private AudioSource _source;
        private CharacterController _controller;
        private Vector3 _lastPosition;
        private float _distance;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            foreach (var controller in GetComponentsInChildren<StarterAssets.ThirdPersonController>(true))
                controller.FootstepAudioClips = System.Array.Empty<AudioClip>();

            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            // Keep the current clip gain, but halve the AudioSource output so
            // the resulting footstep level is 50% below the previous setup.
            _source.volume = volume * 0.5f;
            _lastPosition = transform.position;
        }

        private void Update()
        {
            Vector3 position = transform.position;
            float moved = Vector3.Distance(new Vector3(position.x, 0f, position.z),
                new Vector3(_lastPosition.x, 0f, _lastPosition.z));
            _lastPosition = position;

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
            AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
            if (clip == null) return;
            _source.PlayOneShot(clip, volume);
        }
    }
}
