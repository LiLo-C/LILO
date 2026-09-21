using Lilo.MonoBehaviours.Input;
using Lilo.Config;
using Lilo.MonoBehaviours;
using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Lilo.MonoBehaviours.Input
{
    /// <summary>
    /// Forwards the shared virtual joystick and jump button to the Starter Assets
    /// character used by OfficeLevel1.
    /// </summary>
    public sealed class MobileStarterAssetsBridge : MonoBehaviour
    {
        [SerializeField] private JoystickInputAdapter joystick;
        [SerializeField] private StarterAssetsInputs playerInput;
        [SerializeField] private GameConfig config;
        [SerializeField] private GameplaySpeedSettings speedSettings;
        [SerializeField] private bool enableKeyboardInEditor = true;

        private ThirdPersonController _characterController;

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

            GameObject jumpButton = GameObject.Find("JumpButton");
            if (jumpButton != null && playerInput != null)
            {
                MobileJumpButton jump = jumpButton.GetComponent<MobileJumpButton>();
                if (jump == null)
                    jump = jumpButton.AddComponent<MobileJumpButton>();
                jump.SetTarget(playerInput);
            }
        }

        private void Update()
        {
            if (playerInput == null)
                return;

            var movement = joystick != null
                ? joystick.CurrentInput
                : Lilo.Systems.Movement.MovementInput.Zero;
            bool keyboardJump = false;
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

                keyboardJump = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
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

            // Starter Assets normally treats a stick as digital unless analogMovement is enabled.
            // Keep the analog magnitude and switch to the sprint speed only near full deflection.
            playerInput.analogMovement = true;
            bool keyboardSprint = false;
#if ENABLE_INPUT_SYSTEM
            keyboardSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
#endif
            playerInput.SprintInput(keyboardSprint || movement.Magnitude >= sprintThreshold);
            playerInput.MoveInput(movement.Direction * movement.Magnitude);
            if (keyboardJump)
                playerInput.JumpInput(true);
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

    public sealed class MobileJumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private StarterAssetsInputs _target;

        public void SetTarget(StarterAssetsInputs target)
        {
            _target = target;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_target != null)
                _target.JumpInput(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_target != null)
                _target.JumpInput(false);
        }
    }
}
