using System.Collections.Generic;
using Lilo.Config;

namespace Lilo.State
{
    public enum RunOutcome
    {
        InProgress,
        GoodEnding,
        BadEnding,
    }

    /// <summary>
    /// Plain data object (no MonoBehaviour) holding everything that must survive a scene change:
    /// lives, current floor, checkpoint, key inventory, battery slot, hiding state, run outcome.
    /// Owned by exactly one GameManager. Consumers mutate it only through the named operations
    /// below — never replace the instance (spec 002 FR-003).
    /// </summary>
    public class GameState
    {
        public int Lives { get; private set; }
        public FloorId CurrentFloor { get; private set; }
        public bool IsHiding { get; private set; }
        public RunOutcome Outcome { get; private set; }

        public float InstalledBatteryCharge { get; private set; }
        public bool SpareBatterySlotOccupied { get; private set; }
        public float SpareBatteryCharge { get; private set; }

        private readonly HashSet<string> _keyInventory = new HashSet<string>();
        public IReadOnlyCollection<string> KeyInventory => _keyInventory;

        public GameState(GameConfig config)
        {
            StartNewRun(config);
        }

        /// <summary>New run: full lives, first floor, empty inventory, full battery (spec 002 FR-004).</summary>
        public void StartNewRun(GameConfig config)
        {
            Lives = config.lives;
            CurrentFloor = FloorId.Floor52;
            IsHiding = false;
            Outcome = RunOutcome.InProgress;
            InstalledBatteryCharge = config.batteryDuration;
            SpareBatterySlotOccupied = false;
            SpareBatteryCharge = 0f;
            _keyInventory.Clear();
        }

        /// <summary>Floor restart after a death: battery/keys reset for the CURRENT floor; lives already decremented by the caller (spec 002 FR-004).</summary>
        public void ResetForFloorRestart(GameConfig config)
        {
            IsHiding = false;
            InstalledBatteryCharge = config.batteryDuration;
            SpareBatterySlotOccupied = false;
            SpareBatteryCharge = 0f;
            _keyInventory.Clear();
        }

        public void AdvanceToFloor(FloorId nextFloor) => CurrentFloor = nextFloor;

        public void LoseLife() => Lives = System.Math.Max(0, Lives - 1);

        public void SetHiding(bool hiding) => IsHiding = hiding;

        public void SetInstalledBatteryCharge(float charge) => InstalledBatteryCharge = charge;

        public void PickUpSpareBattery(float charge)
        {
            SpareBatterySlotOccupied = true;
            SpareBatteryCharge = charge;
        }

        public void InstallSpareBattery(float fullChargeDuration)
        {
            InstalledBatteryCharge = fullChargeDuration;
            SpareBatterySlotOccupied = false;
            SpareBatteryCharge = 0f;
        }

        public bool AddKey(string keyId) => _keyInventory.Add(keyId);

        public bool RemoveKey(string keyId) => _keyInventory.Remove(keyId);

        /// <summary>Ending reached: run outcome recorded, everything else left as-is for the ending scene to read (spec 002 FR-004).</summary>
        public void SetOutcome(RunOutcome outcome) => Outcome = outcome;
    }
}
