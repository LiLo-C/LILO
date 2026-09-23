using Lilo.MonoBehaviours.Input;
using Lilo.Config;
using Lilo.MonoBehaviours;
using StarterAssets;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Lilo.MonoBehaviours.Input
{
    /// <summary>
    /// Forwards the shared virtual joystick to the Starter Assets character used
    /// by OfficeLevel1.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class MobileStarterAssetsBridge : MonoBehaviour
    {
        [SerializeField] private JoystickInputAdapter joystick;
        [SerializeField] private StarterAssetsInputs playerInput;
        [SerializeField] private GameConfig config;
        [SerializeField] private GameplaySpeedSettings speedSettings;
        [SerializeField] private bool enableKeyboardInEditor = true;

        private ThirdPersonController _characterController;

        private void Start()
        {
            DisableJump();
        }

        private void Awake()
        {
            if (joystick == null)
                joystick = FindFirstObjectByType<JoystickInputAdapter>();

            if (speedSettings == null)
                speedSettings = FindFirstObjectByType<GameplaySpeedSettings>();

            GameObject player = GameObject.Find("PlayerCharacter");
            if (player != null)
            {
                if (playerInput == null)
                    playerInput = player.GetComponent<StarterAssetsInputs>();
                if (_characterController == null)
                    _characterController = player.GetComponent<ThirdPersonController>();
            }

        }

        private void Update()
        {
            if (joystick == null)
                joystick = FindFirstObjectByType<JoystickInputAdapter>();

            if (playerInput == null)
                return;

            var gameState = GameManager.Instance?.State;
            if (gameState != null && gameState.IsHiding)
            {
                playerInput.MoveInput(Vector2.zero);
                playerInput.SprintInput(false);
                return;
            }

            var movement = joystick != null
                ? joystick.CurrentInput
                : Lilo.Systems.Movement.MovementInput.Zero;
#if ENABLE_INPUT_SYSTEM
            if (enableKeyboardInEditor && (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer))
            {
                Vector2 keyboardDirection = ReadKeyboardDirection();
                if (keyboardDirection.sqrMagnitude > 0.001f)
                {
                    float magnitude = keyboardDirection.magnitude;
                    movement = new Lilo.Systems.Movement.MovementInput(keyboardDirection.normalized, magnitude);
                }

            }
#endif
            GameConfig activeConfig = config != null ? config : GameManager.Instance?.Config;
            if (speedSettings != null && _characterController != null)
            {
                _characterController.MoveSpeed = speedSettings.playerWalkSpeed;
                _characterController.SprintSpeed = speedSettings.playerSprintSpeed;
            }
            else if (activeConfig != null)
            {
                config = activeConfig;
                ApplyConfiguredPlayerSpeed(activeConfig);
            }
            float sprintThreshold = activeConfig != null ? activeConfig.sprintJoystickThreshold : 0.9f;

            // Any nonzero joystick deflection keeps the configured walk speed as the minimum.
            // A near-full pull selects the faster sprint speed; the animator receives that same speed.
            playerInput.analogMovement = false;
            bool keyboardSprint = false;
#if ENABLE_INPUT_SYSTEM
            keyboardSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
#endif
            playerInput.SprintInput(movement.Magnitude > 0f
                && (keyboardSprint || movement.Magnitude >= sprintThreshold));
            playerInput.MoveInput(movement.Direction * movement.Magnitude);
            playerInput.JumpInput(false);
        }

        private void DisableJump()
        {
            if (_characterController != null)
                _characterController.JumpHeight = 0f;
#if ENABLE_INPUT_SYSTEM
            GameObject player = GameObject.Find("PlayerCharacter");
            var input = player != null ? player.GetComponentInChildren<PlayerInput>(true) : null;
            input?.actions?.FindAction("Jump")?.Disable();
#endif
            playerInput?.JumpInput(false);
        }

#if ENABLE_INPUT_SYSTEM
        private static Vector2 ReadKeyboardDirection()
        {
            if (Keyboard.current == null)
                return Vector2.zero;

            Vector2 direction = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) direction.y += 1f;
            if (Keyboard.current.sKey.isPressed) direction.y -= 1f;
            if (Keyboard.current.dKey.isPressed) direction.x += 1f;
            if (Keyboard.current.aKey.isPressed) direction.x -= 1f;
            return Vector2.ClampMagnitude(direction, 1f);
        }
#endif

        private void ApplyConfiguredPlayerSpeed(GameConfig activeConfig)
        {
            if (_characterController == null)
                return;

            _characterController.MoveSpeed = activeConfig.walkSpeed;
            _characterController.SprintSpeed = activeConfig.walkSpeed * activeConfig.sprintMultiplier;
        }
    }
}
