using System;
using UnityEngine;
using Lilo.Config;

namespace Lilo.Systems.Hiding
{
    /// <summary>
    /// Pure C# state machine for hiding (spec 001 FR-001, FR-002, FR-004).
    /// Manages explicit state transitions (Visible -> Entering -> Hidden -> Exiting -> Visible).
    /// No MonoBehaviour or scene-graph dependency (constitution Principle III).
    /// </summary>
    public class HidingSystem
    {
        private readonly GameConfig _config;

        public HidingState CurrentState { get; private set; } = HidingState.Visible;
        public HidingSpotData CurrentSpot { get; private set; }
        public Vector3 CurrentPlayerPosition { get; private set; }
        public float TransitionTimer { get; private set; }

        public event Action<HidingState> StateChanged;

        public HidingSystem(GameConfig config)
        {
            _config = config;
        }

        public HidingTransitionResult TryEnter(HidingSpotData spot, bool anchorsValid)
        {
            if (CurrentState != HidingState.Visible)
            {
                return HidingTransitionResult.Fail($"Cannot enter hiding while in state '{CurrentState}'.");
            }

            if (!spot.IsValid)
            {
                return HidingTransitionResult.Fail("No valid hiding spot supplied.");
            }

            if (spot.IsOccupied)
            {
                return HidingTransitionResult.Fail("Hiding spot is already occupied.");
            }

            if (!anchorsValid)
            {
                return HidingTransitionResult.Fail("Hiding spot anchors are blocked or invalid.");
            }

            CurrentSpot = spot.WithOccupied(true);
            TransitionTimer = 0f;
            CurrentState = HidingState.Entering;
            StateChanged?.Invoke(CurrentState);

            return HidingTransitionResult.Ok();
        }

        public void Tick(float deltaSeconds)
        {
            float duration = _config != null ? _config.hidingTransitionDuration : 0.5f;

            if (CurrentState == HidingState.Entering)
            {
                TransitionTimer += deltaSeconds;
                if (TransitionTimer >= duration)
                {
                    CurrentState = HidingState.Hidden;
                    CurrentPlayerPosition = CurrentSpot.HideAnchor;
                    StateChanged?.Invoke(CurrentState);
                }
            }
            else if (CurrentState == HidingState.Exiting)
            {
                TransitionTimer += deltaSeconds;
                if (TransitionTimer >= duration)
                {
                    CurrentState = HidingState.Visible;
                    CurrentPlayerPosition = CurrentSpot.ExitAnchor;
                    CurrentSpot = CurrentSpot.WithOccupied(false);
                    StateChanged?.Invoke(CurrentState);
                }
            }
        }

        public HidingTransitionResult TryExit(bool anchorsValid)
        {
            if (CurrentState != HidingState.Hidden)
            {
                return HidingTransitionResult.Fail($"Cannot exit hiding while in state '{CurrentState}'.");
            }

            if (!anchorsValid)
            {
                return HidingTransitionResult.Fail("Exit anchor is blocked or invalid.");
            }

            TransitionTimer = 0f;
            CurrentState = HidingState.Exiting;
            StateChanged?.Invoke(CurrentState);

            return HidingTransitionResult.Ok();
        }

        public void ForceExit()
        {
            bool wasNotVisible = CurrentState != HidingState.Visible;
            if (CurrentSpot.IsValid)
                CurrentPlayerPosition = CurrentSpot.ExitAnchor;
            CurrentSpot = CurrentSpot.WithOccupied(false);
            TransitionTimer = 0f;
            CurrentState = HidingState.Visible;
            if (wasNotVisible)
            {
                StateChanged?.Invoke(CurrentState);
            }
        }
    }
}
