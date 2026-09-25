using System.Collections.Generic;
using Lilo.Config;
using UnityEngine;

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
    /// lives, current floor, checkpoint, key inventory, installed battery charge, hiding state, run outcome.
    /// Owned by exactly one GameManager. Consumers mutate it only through the named operations
    /// below — never replace the instance (spec 002 FR-003).
    /// </summary>
    public class GameState
    {
        public int Lives { get; private set; }
        public FloorId CurrentFloor { get; private set; }
        public bool IsHiding { get; private set; }
        public RunOutcome Outcome { get; private set; }
        public string RespawnMessage { get; private set; }

        public float InstalledBatteryCharge { get; private set; }

        private readonly HashSet<string> _keyInventory = new HashSet<string>();
        private readonly Dictionary<FloorId, Vector3> _accessKeySpawnPositions = new Dictionary<FloorId, Vector3>();
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
            RespawnMessage = string.Empty;
            InstalledBatteryCharge = config.batteryDuration;
            _keyInventory.Clear();
            _accessKeySpawnPositions.Clear();
        }

        /// <summary>Floor restart after a death: battery/keys reset for the CURRENT floor; lives already decremented by the caller (spec 002 FR-004).</summary>
        public void ResetForFloorRestart(GameConfig config)
        {
            IsHiding = false;
            InstalledBatteryCharge = config.batteryDuration;
            _keyInventory.Clear();
        }

        public void AdvanceToFloor(FloorId nextFloor) => CurrentFloor = nextFloor;

        public void LoseLife() => Lives = System.Math.Max(0, Lives - 1);

        public void SetRespawnMessage(string message) => RespawnMessage = message ?? string.Empty;

        public string ConsumeRespawnMessage()
        {
            string message = RespawnMessage;
            RespawnMessage = string.Empty;
            return message;
        }

        public void SetHiding(bool hiding) => IsHiding = hiding;

        public void SetInstalledBatteryCharge(float charge) => InstalledBatteryCharge = charge;

        public void AddInstalledBatteryCharge(float charge, float maximumCharge)
        {
            InstalledBatteryCharge = (float)System.Math.Min(
                System.Math.Max(0d, maximumCharge),
                System.Math.Max(0d, InstalledBatteryCharge + charge));
        }

        public bool AddKey(string keyId) => _keyInventory.Add(keyId);

        public bool HasKey(string keyId) => _keyInventory.Contains(keyId);

        public bool RemoveKey(string keyId) => _keyInventory.Remove(keyId);

        public bool TryGetAccessKeySpawnPosition(FloorId floor, out Vector3 position)
        {
            return _accessKeySpawnPositions.TryGetValue(floor, out position);
        }

        public void SetAccessKeySpawnPosition(FloorId floor, Vector3 position)
        {
            _accessKeySpawnPositions[floor] = position;
        }

        /// <summary>Ending reached: run outcome recorded, everything else left as-is for the ending scene to read (spec 002 FR-004).</summary>
        public void SetOutcome(RunOutcome outcome) => Outcome = outcome;
    }
}
