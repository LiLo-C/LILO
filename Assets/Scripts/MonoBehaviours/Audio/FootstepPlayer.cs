using UnityEngine;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// Plays custom footsteps at the animation's foot contacts. Characters without
    /// authored footstep events use distance-based strides as a fallback.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class FootstepPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip[] stepClips = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float volume = 0.3f;
        [SerializeField] private float strideLength = 0.5f;
        [SerializeField] private float minMoveSpeed = 0.3f;
        [SerializeField, Min(0.1f)] private float minStepInterval = 0.55f;

        private AudioSource _source;
        private CharacterController _controller;
        private StarterAssets.ThirdPersonController[] _animationControllers;
        private bool _usesAnimationEvents;
        private Vector3 _lastPosition;
        private float _distance;
        private float _nextStepTime;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animationControllers = GetComponentsInChildren<StarterAssets.ThirdPersonController>(true);
            foreach (var controller in _animationControllers)
            {
                var animator = controller.GetComponent<Animator>();
                if (animator == null || animator.runtimeAnimatorController == null) continue;
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                    foreach (var animationEvent in clip.events)
                        if (animationEvent.functionName == "OnFootstep") _usesAnimationEvents = true;
            }

            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.pitch = 1f;
            // Apply the Inspector gain directly so the boosted clips retain their level.
            _source.volume = Mathf.Clamp01(volume);
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            foreach (var controller in _animationControllers)
                controller.Footstep += PlayAnimationStep;
            _lastPosition = transform.position;
            _distance = 0f;
            _nextStepTime = 0f;
        }

        private void OnDisable()
        {
            foreach (var controller in _animationControllers)
                if (controller != null) controller.Footstep -= PlayAnimationStep;
            if (_source != null) _source.Stop();
        }

        private void PlayAnimationStep(AnimationEvent animationEvent)
        {
            Vector3 velocity = _controller.velocity;
            velocity.y = 0f;
            if (Time.deltaTime <= 0f || !_controller.isGrounded
                || velocity.magnitude < minMoveSpeed || Time.time < _nextStepTime)
                return;

            if (PlayStep())
                // Reject duplicate blend events without skipping real foot contacts.
                _nextStepTime = Time.time + 0.08f;
        }

        private void LateUpdate()
        {
            Vector3 position = transform.position;
            float moved = Vector3.Distance(new Vector3(position.x, 0f, position.z),
                new Vector3(_lastPosition.x, 0f, _lastPosition.z));
            _lastPosition = position;

            if (_usesAnimationEvents) return;

            bool grounded = _controller == null || _controller.isGrounded;
            float stride = Mathf.Max(0.1f, strideLength);
            if (!grounded || Time.deltaTime <= 0f || moved > stride * 4f
                || moved / Time.deltaTime < minMoveSpeed)
            {
                _distance = 0f;
                return;
            }

            _distance += moved;
            if (_distance < stride || Time.time < _nextStepTime) return;

            // Discard overdue strides instead of replaying a burst after a hitch
            // or respawn. Walking still follows distance; sprinting has a cadence cap.
            _distance %= stride;
            if (PlayStep())
                _nextStepTime = Time.time + Mathf.Max(minStepInterval, _source.clip.length + 0.04f);
        }

        private bool PlayStep()
        {
            if (stepClips == null || stepClips.Length == 0) return false;
            AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
            if (clip == null) return false;
            // One source owns the step; the quiet tail yields to the next contact.
            _source.clip = clip;
            _source.Play();
            return true;
        }
    }
}
