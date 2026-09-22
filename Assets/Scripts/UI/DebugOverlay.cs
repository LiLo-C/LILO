using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Flashlight;
using Lilo.MonoBehaviours.Monster;
using Lilo.State;
using Lilo.Systems.Debug;
using Lilo.Systems.Flashlight;
using UnityEngine;
using UnityEngine.UI;

namespace Lilo.UI
{
    public sealed class DebugOverlay : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private bool visibleOnStart = true;

        private LightingRig _rig;
        private MonsterAIController _monster;
        private GameConfig _config;

        private void Awake()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            gameObject.SetActive(false);
            return;
#endif
            if (label == null) label = GetComponent<Text>();
            GameObject player = GameObject.Find("PlayerCharacter");
            if (player != null)
            {
                _rig = player.GetComponent<LightingRig>();
                _monster = FindFirstObjectByType<MonsterAIController>();
            }
            _config = GameManager.Instance != null ? GameManager.Instance.Config : null;
            if (label != null) label.enabled = visibleOnStart;
        }

        private void Update()
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return;
#else
            if (label == null || !_enabledLabel()) return;
            if (_rig == null)
            {
                GameObject player = GameObject.Find("PlayerCharacter");
                if (player != null) _rig = player.GetComponent<LightingRig>();
            }
            if (_config == null && GameManager.Instance != null)
                _config = GameManager.Instance.Config;

            GameState state = GameManager.Instance != null ? GameManager.Instance.State : null;
            var snapshot = new DebugOverlaySnapshot
            {
                LightState = _rig != null ? _rig.CurrentState : FlashlightLightState.CompactDarkness,
                LightRadius = _rig != null ? _rig.DisplayedRadius : 0f,
                LightIntensity = _rig != null ? _rig.CurrentIntensity : 0f,
                BatteryFraction = _rig != null ? _rig.ChargeFraction : 0f,
                SpareOccupied = state != null && state.SpareBatterySlotOccupied,
                SpareChargeFraction = state != null && _config != null && _config.batteryDuration > 0f
                    ? state.SpareBatteryCharge / _config.batteryDuration : 0f,
                MonsterAvailable = _monster != null && _monster.isActiveAndEnabled,
                MonsterState = _monster != null ? _monster.CurrentState : MonsterState.Patrol,
                MonsterDistance = _monster != null ? _monster.DistanceToPlayer : -1f,
                NoiseRadius = _monster != null ? _monster.CurrentNoiseRadius : 0f,
                NoiseSource = _monster != null ? (NoiseSource)_monster.CurrentNoiseSource : NoiseSource.Silent,
                IsHiding = state != null && state.IsHiding,
            };
            label.text = DebugOverlaySystem.Format(snapshot);
#endif
        }

        private bool _enabledLabel() => label != null && label.enabled;

        public void Toggle()
        {
            if (label != null) label.enabled = !label.enabled;
        }
    }
}
