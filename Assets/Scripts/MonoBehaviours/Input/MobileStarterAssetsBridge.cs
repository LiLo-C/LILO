using Lilo.MonoBehaviours.Input;
using Lilo.Config;
using Lilo.MonoBehaviours;
using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;

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

        private void Awake()
        {
            if (joystick == null)
                joystick = FindFirstObjectByType<JoystickInputAdapter>();

            if (playerInput == null)
            {
                GameObject player = GameObject.Find("PlayerCharacter");
                if (player != null)
                    playerInput = player.GetComponent<StarterAssetsInputs>();
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
            if (joystick == null || playerInput == null)
                return;

            var movement = joystick.CurrentInput;
            GameConfig activeConfig = config != null ? config : GameManager.Instance?.Config;
            float sprintThreshold = activeConfig != null ? activeConfig.sprintJoystickThreshold : 0.9f;

            // Starter Assets normally treats a stick as digital unless analogMovement is enabled.
            // Keep the analog magnitude and switch to the sprint speed only near full deflection.
            playerInput.analogMovement = true;
            playerInput.SprintInput(movement.Magnitude >= sprintThreshold);
            playerInput.MoveInput(movement.Direction * movement.Magnitude);
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
