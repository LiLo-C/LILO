using Lilo.Config;
using Lilo.MonoBehaviours.Input;
using Lilo.Systems.Movement;
using UnityEngine;

namespace Lilo.MonoBehaviours.Player
{
    /// <summary>
    /// Thin per-frame adapter: reads the joystick, calls the pure MovementSystem, applies the
    /// resulting velocity to the PlayerCharacter's Transform. World mapping per ROADMAP §0:
    /// 2D (x, y) -> 3D (x, 0, y).
    /// </summary>
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private JoystickInputAdapter joystick;

        private Animator animator;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");

        private void Awake()
        {
            // The character prefab is a child of PlayerCharacter, so keep animation wiring
            // here instead of coupling the movement system to a specific model hierarchy.
            animator = GetComponentInChildren<Animator>();
        }

        /// <summary>Read-only for other systems (noise emission, audio mixing) — spec FR-011/US2.4.</summary>
        public bool IsSprinting { get; private set; }

        private void Update()
        {
            if (config == null || joystick == null)
            {
                SetAnimationSpeed(0f);
                return;
            }

            MovementInput input = joystick.CurrentInput;
            MovementResult result = MovementSystem.Compute(input, config);
            IsSprinting = result.IsSprinting;

            float movementMagnitude = result.Velocity.magnitude;
            SetAnimationSpeed(movementMagnitude <= 0f ? 0f : result.IsSprinting ? 45f : 5f);

            Vector3 delta = new Vector3(result.Velocity.x, 0f, result.Velocity.y) * Time.deltaTime;
            transform.position += delta;
        }

        private void SetAnimationSpeed(float speed)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(SpeedHash, speed);
            animator.SetFloat(MotionSpeedHash, speed > 0f ? 1f : 0f);
        }
    }
}
