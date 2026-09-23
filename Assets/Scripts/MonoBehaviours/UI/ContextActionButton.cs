using UnityEngine;
using UnityEngine.UI;
using Lilo.MonoBehaviours.Hiding;
using Lilo.Systems.Hiding;

namespace Lilo.MonoBehaviours.Hud
{
    /// <summary>
    /// Context action button for iOS hiding — shows Hide/Exit near a hiding
    /// spot, hidden otherwise. Battery Take is proximity auto-pickup (no button).
    /// </summary>
    public class ContextActionButton : MonoBehaviour
    {
        [SerializeField] private HidingController hidingController;
        [SerializeField] private Button actionButton;
        [SerializeField] private Text label;
        [SerializeField] private CanvasGroup canvasGroup;

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
        }

        private void OnHidingStateChanged(HidingState _)
        {
            Refresh();
        }

        private void Update()
        {
            if (hidingController == null) return;

            _checkTimer -= Time.deltaTime;
            if (_checkTimer <= 0f)
            {
                _checkTimer = 0.15f;
                Refresh();
            }
        }

        private void Refresh()
        {
            string actionLabel = hidingController != null ? hidingController.CurrentActionLabel : string.Empty;
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
            if (hidingController != null)
                hidingController.TriggerContextAction();
            Refresh();
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
