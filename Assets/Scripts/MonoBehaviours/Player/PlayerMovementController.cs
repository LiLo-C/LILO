using Lilo.Config;
using Lilo.MonoBehaviours.Audio;
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
        [SerializeField] private SfxController sfx;

        /// <summary>Read-only for other systems (noise emission, audio mixing) — spec FR-011/US2.4.</summary>
        public bool IsSprinting { get; private set; }

        private bool _hasMoved;

        private void Update()
        {
            if (config == null || joystick == null)
            {
                return;
            }

            var gameState = GameManager.Instance?.State;
            if (gameState != null && gameState.IsHiding)
            {
                IsSprinting = false;
                return;
            }

            MovementInput input = joystick.CurrentInput;
            MovementResult result = MovementSystem.Compute(input, config);
            IsSprinting = result.IsSprinting;

            Vector3 delta = new Vector3(result.Velocity.x, 0f, result.Velocity.y) * Time.deltaTime;
            transform.position += delta;

            if (!_hasMoved && delta.sqrMagnitude > 0.0001f)
            {
                _hasMoved = true;
                if (sfx != null) sfx.PlayBehindYou();
            }
        }
    }
}
