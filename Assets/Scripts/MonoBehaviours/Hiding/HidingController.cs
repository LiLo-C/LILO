using System;
using UnityEngine;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Monster;
using Lilo.MonoBehaviours.Player;
using Lilo.Systems.GameLoop;
using Lilo.Systems.Hiding;
using StarterAssets;

namespace Lilo.MonoBehaviours.Hiding
{
    /// <summary>
    /// Thin MonoBehaviour adapter over HidingSystem (spec 001 FR-001–FR-004, US1).
    /// Detects candidate HidingSpots via proximity, syncs state to GameState,
    /// positions player at anchors, and exposes a context action for iOS UI buttons.
    /// </summary>
    public class HidingController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField, Min(1f)] private float maxHideSeconds = 20f;
        [SerializeField, Min(0f)] private float heartbeatStartSeconds = 5f;
        [SerializeField, Min(0f)] private float heartbeatMaxNoiseRadius = 12f;

        private HidingSystem _system;
        private HidingSpot _nearestSpot;
        private HidingSpot[] _allSpots = Array.Empty<HidingSpot>();
        private float _spotScanTimer;
        private HidingSpot _activeSpot;
        private MonsterAIController _monster;
        private CharacterController _characterController;
        private ThirdPersonController _thirdPersonController;
        private bool _thirdPersonWasEnabled;
        private Renderer[] _renderers;
        private bool[] _rendererStates;
        private Light[] _lights;
        private bool[] _lightStates;
        private AudioSource _heartbeatSource;
        private AudioClip _heartbeatClip;
        private float _hiddenSeconds;
        private float _heartbeatCountdown;
        private bool _presentationHidden;

        /// <summary>Global hiding flag read by MonsterAIController when GameManager is absent.</summary>
        public static bool IsPlayerHiding { get; private set; }
        public static bool IsPlayerExposed { get; private set; }
        public static bool IsPlayerMovementLocked { get; private set; }

        public HidingState CurrentState => _system != null ? _system.CurrentState : HidingState.Visible;
        public HidingSpot NearestSpot => _nearestSpot;
        public bool CanHide => CurrentState == HidingState.Visible && _nearestSpot != null && _nearestSpot.IsAvailable;
        public bool CanExit => CurrentState == HidingState.Hidden;
        public float HiddenSeconds => _hiddenSeconds;

        public string CurrentActionLabel
        {
            get
            {
                if (CanHide) return "HIDE";
                if (CanExit) return IsPlayerExposed
                    ? "FOUND - EXIT"
                    : $"EXIT  {Mathf.CeilToInt(Mathf.Max(0f, maxHideSeconds - _hiddenSeconds))}s";
                return string.Empty;
            }
        }

        public event Action<HidingState> StateChanged;

        private void Awake()
        {
            IsPlayerHiding = false;
            IsPlayerExposed = false;
            IsPlayerMovementLocked = false;
            if (config == null)
                config = GameManager.Instance?.Config;
            _system = new HidingSystem(config);
            _system.StateChanged += OnSystemStateChanged;
            _characterController = GetComponent<CharacterController>();
            _thirdPersonController = GetComponent<ThirdPersonController>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            _rendererStates = new bool[_renderers.Length];
            _lights = GetComponentsInChildren<Light>(true);
            _lightStates = new bool[_lights.Length];
            _heartbeatSource = gameObject.AddComponent<AudioSource>();
            _heartbeatSource.playOnAwake = false;
            _heartbeatSource.spatialBlend = 0f;
        }

        private void Start()
        {
            RefreshSpots();
        }

        private void OnDestroy()
        {
            if (_system != null)
            {
                _system.StateChanged -= OnSystemStateChanged;
                _system.ForceExit();
            }
            IsPlayerHiding = false;
            IsPlayerExposed = false;
            IsPlayerMovementLocked = false;
            if (_heartbeatClip != null)
                Destroy(_heartbeatClip);
        }

        private void Update()
        {
            if (_system == null) return;

            _system.Tick(Time.deltaTime);

            if (CurrentState == HidingState.Hidden)
                UpdateHeartbeat(Time.deltaTime);

            _spotScanTimer -= Time.deltaTime;
            if (_spotScanTimer <= 0f)
            {
                _spotScanTimer = 0.2f;
                UpdateNearestSpot();
            }
        }

        /// <summary>
        /// Called by iOS action button or UI. No keyboard dependency.
        /// </summary>
        public void TriggerContextAction()
        {
            if (CanHide)
                TryEnterNearest();
            else if (CanExit)
                TryExit();
        }

        public bool TryEnterNearest()
        {
            if (!CanHide || _nearestSpot == null) return false;

            // Return to the actual approach point, which was known to be reachable.
            var spotData = new HidingSpotData(_nearestSpot.HidePosition, transform.position);
            var starterInput = GetComponent<StarterAssetsInputs>();
            var movement = GetComponent<PlayerMovementController>();
            bool rushedEntry = (starterInput != null && starterInput.sprint)
                || (movement != null && movement.IsSprinting);
            var result = _system.TryEnter(spotData, anchorsValid: true);
            if (result.Success)
            {
                _activeSpot = _nearestSpot;
                _nearestSpot.SetOccupied(true);
                if (rushedEntry)
                {
                    if (_monster == null)
                        _monster = FindFirstObjectByType<MonsterAIController>();
                    if (_monster != null)
                    {
                        float radius = config != null
                            ? config.noiseBaseRadius * config.noiseSprint
                            : 12f;
                        _monster.EmitPulse(_activeSpot.HidePosition, radius, 1f);
                    }
                }
            }
            return result.Success;
        }

        public bool TryExit()
        {
            if (!CanExit) return false;

            var result = _system.TryExit(anchorsValid: true);
            return result.Success;
        }

        public void ForceExit()
        {
            _system?.ForceExit();
        }

        private void OnSystemStateChanged(HidingState state)
        {
            IsPlayerHiding = state == HidingState.Hidden;
            IsPlayerMovementLocked = state != HidingState.Visible;
            if (state != HidingState.Hidden)
                IsPlayerExposed = false;

            var gameState = GameManager.Instance?.State;
            if (gameState != null)
                gameState.SetHiding(IsPlayerHiding);

            if (state == HidingState.Hidden)
            {
                _hiddenSeconds = 0f;
                _heartbeatCountdown = 0f;
                SetPresentationHidden(true);
                transform.position = _system.CurrentPlayerPosition;
            }
            else if (state == HidingState.Visible)
            {
                if (_activeSpot != null)
                    transform.position = _system.CurrentPlayerPosition;
                SetPresentationHidden(false);
                if (_activeSpot != null)
                    _activeSpot.SetOccupied(false);
                _activeSpot = null;
                _hiddenSeconds = 0f;
            }

            StateChanged?.Invoke(state);
        }

        private void RefreshSpots()
        {
            _allSpots = FindObjectsByType<HidingSpot>();
        }

        private void UpdateNearestSpot()
        {
            if (CurrentState != HidingState.Visible) return;

            if (_allSpots == null || _allSpots.Length == 0)
                RefreshSpots();

            HidingSpot bestSpot = null;
            float bestDistance = float.MaxValue;
            Vector3 pos = transform.position;

            foreach (var spot in _allSpots)
            {
                if (spot == null || !spot.IsAvailable) continue;

                Vector3 delta = pos - spot.transform.position;
                float dist = new Vector2(delta.x, delta.z).magnitude;
                if (dist <= spot.InteractionRadius && dist < bestDistance)
                {
                    bestDistance = dist;
                    bestSpot = spot;
                }
            }

            _nearestSpot = bestSpot;
        }

        private void UpdateHeartbeat(float deltaTime)
        {
            _hiddenSeconds += deltaTime;
            if (!IsPlayerExposed && _hiddenSeconds >= maxHideSeconds)
            {
                IsPlayerExposed = true;
                Debug.Log($"[Hiding] Heartbeat revealed the player's desk after {maxHideSeconds:0} seconds.");
            }

            if (_hiddenSeconds < heartbeatStartSeconds)
                return;

            _heartbeatCountdown -= deltaTime;
            if (_heartbeatCountdown > 0f)
                return;

            float urgency = Mathf.InverseLerp(heartbeatStartSeconds, maxHideSeconds, _hiddenSeconds);
            _heartbeatCountdown = Mathf.Lerp(1.5f, 0.5f, urgency);
            if (_heartbeatClip == null)
                _heartbeatClip = CreateHeartbeatClip();
            _heartbeatSource.PlayOneShot(_heartbeatClip, Mathf.Lerp(0.25f, 0.9f, urgency) * SoundSettingsStore.Effects);

            if (_monster == null)
                _monster = FindFirstObjectByType<MonsterAIController>();
            if (_monster != null && _activeSpot != null)
            {
                float radius = Mathf.Lerp(1f, heartbeatMaxNoiseRadius, urgency);
                _monster.EmitPulse(_activeSpot.HidePosition, radius, _heartbeatCountdown,
                    Lilo.Systems.Monster.NoiseSource.Heartbeat);
            }
        }

        private void SetPresentationHidden(bool hidden)
        {
            if (_presentationHidden == hidden)
                return;
            _presentationHidden = hidden;
            if (hidden)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    _rendererStates[i] = _renderers[i].enabled;
                    _renderers[i].enabled = false;
                }
                for (int i = 0; i < _lights.Length; i++)
                {
                    _lightStates[i] = _lights[i].enabled;
                    _lights[i].enabled = false;
                }
                if (_characterController != null)
                    _characterController.enabled = false;
                if (_thirdPersonController != null)
                {
                    _thirdPersonWasEnabled = _thirdPersonController.enabled;
                    _thirdPersonController.enabled = false;
                }
            }
            else
            {
                if (_characterController != null)
                    _characterController.enabled = true;
                if (_thirdPersonController != null)
                    _thirdPersonController.enabled = _thirdPersonWasEnabled;
                for (int i = 0; i < _renderers.Length; i++)
                    if (_renderers[i] != null) _renderers[i].enabled = _rendererStates[i];
                for (int i = 0; i < _lights.Length; i++)
                    if (_lights[i] != null) _lights[i].enabled = _lightStates[i];
            }
        }

        private static AudioClip CreateHeartbeatClip()
        {
            const int sampleRate = 22050;
            int count = Mathf.RoundToInt(sampleRate * 0.36f);
            var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float first = Beat(t, 0f);
                float second = Beat(t, 0.18f);
                samples[i] = (first + 0.8f * second)
                    * (Mathf.Sin(2f * Mathf.PI * 65f * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * 130f * t))
                    * 0.35f;
            }
            AudioClip clip = AudioClip.Create("HidingHeartbeat", count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float Beat(float time, float start)
        {
            float t = (time - start) / 0.12f;
            return t >= 0f && t <= 1f ? Mathf.Sin(Mathf.PI * t) * (1f - t) : 0f;
        }
    }
}
