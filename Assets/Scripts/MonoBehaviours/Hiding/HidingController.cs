using System;
using UnityEngine;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.Systems.Hiding;

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

        private HidingSystem _system;
        private HidingSpot _nearestSpot;
        private HidingSpot[] _allSpots = Array.Empty<HidingSpot>();
        private float _spotScanTimer;

        /// <summary>Global hiding flag read by MonsterAIController when GameManager is absent.</summary>
        public static bool IsPlayerHiding { get; private set; }

        public HidingState CurrentState => _system != null ? _system.CurrentState : HidingState.Visible;
        public HidingSpot NearestSpot => _nearestSpot;
        public bool CanHide => CurrentState == HidingState.Visible && _nearestSpot != null && !_nearestSpot.IsOccupied;
        public bool CanExit => CurrentState == HidingState.Hidden;

        public string CurrentActionLabel
        {
            get
            {
                if (CanHide) return "Hide";
                if (CanExit) return "Exit";
                return string.Empty;
            }
        }

        public event Action<HidingState> StateChanged;

        private void Awake()
        {
            IsPlayerHiding = false;
            if (config == null)
                config = GameManager.Instance?.Config;
            _system = new HidingSystem(config);
            _system.StateChanged += OnSystemStateChanged;
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
        }

        private void Update()
        {
            if (_system == null) return;

            _system.Tick(Time.deltaTime);

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

            var spotData = _nearestSpot.ToData();
            var result = _system.TryEnter(spotData, anchorsValid: true);
            if (result.Success)
                _nearestSpot.SetOccupied(true);
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

            var gameState = GameManager.Instance?.State;
            if (gameState != null)
                gameState.SetHiding(IsPlayerHiding);

            if (state == HidingState.Hidden)
            {
                transform.position = _system.CurrentPlayerPosition;
            }
            else if (state == HidingState.Visible)
            {
                if (_system.CurrentPlayerPosition != Vector3.zero)
                    transform.position = _system.CurrentPlayerPosition;
                if (_nearestSpot != null)
                    _nearestSpot.SetOccupied(false);
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
                if (spot == null || spot.IsOccupied) continue;

                float dist = Vector3.Distance(pos, spot.transform.position);
                if (dist <= spot.InteractionRadius && dist < bestDistance)
                {
                    bestDistance = dist;
                    bestSpot = spot;
                }
            }

            _nearestSpot = bestSpot;
        }
    }
}
