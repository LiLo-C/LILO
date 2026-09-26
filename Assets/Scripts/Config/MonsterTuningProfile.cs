using System;
using UnityEngine;

namespace Lilo.Config
{
    /// <summary>Per-floor monster tuning (GDD Ch. 17.4). Plain C# — no MonoBehaviour/Component.</summary>
    [Serializable]
    public struct MonsterTuningProfile
    {
        public bool monsterActive;
        public float patrolSpeed;
        public float chaseSpeed;
        [Tooltip("Per-floor multiplier for scene-authored patrol and chase speeds.")]
        public float movementSpeedMultiplier;
        [Tooltip("Per-floor multiplier for movement, interaction, and objective noise radii.")]
        public float noiseSensitivityMultiplier;
        [Tooltip("Per-floor multiplier for monster visual detection range.")]
        public float visionRangeMultiplier;
        public float investigateDuration;
        public float alertDuration;
        public float chaseHoldDuration;
        public float searchDuration;
    }
}
