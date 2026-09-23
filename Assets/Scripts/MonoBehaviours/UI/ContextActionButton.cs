using UnityEngine;
using UnityEngine.UI;
using Lilo.MonoBehaviours.Battery;
using Lilo.MonoBehaviours.Hiding;
using Lilo.Systems.Hiding;

namespace Lilo.MonoBehaviours.Hud
{
    /// <summary>
    /// Context action button for iOS — shows Hide/Exit near a hiding spot and
    /// Take/Hands Full near a loaded keyboard battery. Hiding needs a
    /// HidingController on the player; battery Take works with any
    /// Player-tagged transform. Proximity-driven, no keyboard.
    /// </summary>
    public class ContextActionButton : MonoBehaviour
    {
        [SerializeField] private HidingController hidingController;
        [SerializeField] private KeyboardBatteryDirector batteryDirector;
        [SerializeField] private Button actionButton;
        [SerializeField] private Text label;
        [SerializeField] private CanvasGroup canvasGroup;

        private Transform _playerTransform;
        private float _checkTimer;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void Start()
        {
            if (actionButton == null)
                actionButton = GetComponent<UnityEngine.UI.Button>();
            if (actionButton != null)
                actionButton.onClick.AddListener(OnButtonPressed);

            ResolveController();
            if (hidingController != null)
            {
                hidingController.StateChanged -= OnHidingStateChanged;
                hidingController.StateChanged += OnHidingStateChanged;
            }
            Refresh();
        }

        private void OnEnable()
        {
            if (hidingController == null)
                ResolveController();
            if (hidingController != null)
            {
                hidingController.StateChanged -= OnHidingStateChanged;
                hidingController.StateChanged += OnHidingStateChanged;
            }
        }

        private void OnDisable()
        {
            if (hidingController != null)
                hidingController.StateChanged -= OnHidingStateChanged;
        }

        private void ResolveController()
        {
            if (hidingController == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                    hidingController = player.GetComponent<HidingController>();
            }
            if (hidingController == null)
                hidingController = FindAnyObjectByType<HidingController>();
            if (batteryDirector == null)
                batteryDirector = FindAnyObjectByType<KeyboardBatteryDirector>();
        }

        private void OnHidingStateChanged(HidingState _)
        {
            Refresh();
        }

        private void Update()
        {
            _checkTimer -= Time.deltaTime;
            if (_checkTimer <= 0f)
            {
                _checkTimer = 0.15f;
                Refresh();
            }
        }

        /// <summary>Player transform for proximity checks, with or without hiding.</summary>
        private Transform PlayerTransform()
        {
            if (hidingController != null) return hidingController.transform;
            if (_playerTransform == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) _playerTransform = player.transform;
            }
            return _playerTransform;
        }

        private void Refresh()
        {
            Transform player = PlayerTransform();
            if (player == null)
            {
                SetVisible(false);
                return;
            }

            string actionLabel = hidingController != null ? hidingController.CurrentActionLabel : string.Empty;
            bool hidden = hidingController != null && hidingController.CurrentState == HidingState.Hidden;

            // A loaded battery in reach takes precedence over Hide, never over Exit.
            if (!hidden && batteryDirector != null)
            {
                var slot = batteryDirector.NearestLoadedSlot(player.position);
                if (slot != null)
                    actionLabel = batteryDirector.IsSpareFull ? "Hands Full" : "Take";
            }

            bool show = !string.IsNullOrEmpty(actionLabel);

            if (show)
            {
                if (label != null)
                    label.text = actionLabel;
                SetVisible(true);
            }
            else
            {
                SetVisible(false);
            }
        }

        public void OnButtonPressed()
        {
            if (hidingController != null && hidingController.CanExit)
                hidingController.TriggerContextAction();
            else if (!HandleBatteryPress() && hidingController != null)
                hidingController.TriggerContextAction();
            Refresh();
        }

        /// <summary>Takes a loaded battery in reach; consumes the press either way.</summary>
        private bool HandleBatteryPress()
        {
            if (batteryDirector == null) return false;

            Transform player = PlayerTransform();
            if (player == null) return false;

            var slot = batteryDirector.NearestLoadedSlot(player.position);
            if (slot == null) return false;

            batteryDirector.TryTake(slot);
            return true;
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}
