using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Monster;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.MonoBehaviours.UI
{
    /// <summary>Shows a mobile action to install the carried spare when charge is low.</summary>
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
    public sealed class BatterySwapButton : MonoBehaviour
    {
        private const float ShowBelowChargeFraction = 0.2f;
        private const float RefreshInterval = 0.1f;

        private Button _button;
        private CanvasGroup _canvasGroup;
        private Text _label;
        private float _refreshTimer;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _label = GetComponentInChildren<Text>(true);
            _button.onClick.AddListener(TrySwapBattery);
            Refresh();
        }

        private void Update()
        {
            _refreshTimer -= Time.unscaledDeltaTime;
            if (_refreshTimer <= 0f)
            {
                _refreshTimer = RefreshInterval;
                Refresh();
            }
        }

        private void Refresh()
        {
            GameManager manager = GameManager.Instance;
            GameConfig config = manager != null ? manager.Config : null;
            var state = manager != null ? manager.State : null;
            bool hasBatteryDuration = config != null && config.batteryDuration > 0f;
            float chargeFraction = hasBatteryDuration && state != null
                ? state.InstalledBatteryCharge / config.batteryDuration
                : 1f;
            bool lowCharge = chargeFraction < ShowBelowChargeFraction;
            bool hasSpare = state != null && state.SpareBatterySlotOccupied;

            _canvasGroup.alpha = lowCharge ? 1f : 0f;
            _canvasGroup.interactable = lowCharge && hasSpare;
            _canvasGroup.blocksRaycasts = lowCharge;
            if (_label != null)
                _label.text = hasSpare ? "CHANGE BATTERY" : "NO SPARE BATTERY";
        }

        private void TrySwapBattery()
        {
            GameManager manager = GameManager.Instance;
            GameConfig config = manager != null ? manager.Config : null;
            var state = manager != null ? manager.State : null;
            if (config == null || state == null || config.batteryDuration <= 0f)
                return;

            float chargeFraction = state.InstalledBatteryCharge / config.batteryDuration;
            if (chargeFraction >= ShowBelowChargeFraction || !state.SpareBatterySlotOccupied)
                return;

            state.InstallSpareBattery(config.batteryDuration);

            MonsterAIController monster = FindFirstObjectByType<MonsterAIController>();
            if (monster != null)
                monster.EmitPulse(transform.position, config.noiseBatterySwap);

            Refresh();
            Debug.Log("[Battery] Installed spare battery from the mobile HUD.");
        }
    }
}
