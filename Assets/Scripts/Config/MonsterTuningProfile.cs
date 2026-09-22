using System;

namespace Lilo.Config
{
    /// <summary>Per-floor monster tuning (GDD Ch. 17.4). Plain C# — no MonoBehaviour/Component.</summary>
    [Serializable]
    public struct MonsterTuningProfile
    {
        public bool monsterActive;
        public float patrolSpeed;
        public float chaseSpeed;
        public float investigateDuration;
        public float alertDuration;
        public float chaseHoldDuration;
        public float searchDuration;
    }
}
