using UnityEngine;

namespace Lilo.Systems.Monster
{
    /// <summary>
    /// One-shot detection result from the monster's perception systems (spec 002, hiding/002).
    /// Carries whether the player was detected and the world-space source position.
    /// </summary>
    public readonly struct MonsterDetectionSignal
    {
        public readonly bool IsDetected;
        public readonly Vector3 SourcePosition;

        public MonsterDetectionSignal(bool detected, Vector3 sourcePosition)
        {
            IsDetected = detected;
            SourcePosition = sourcePosition;
        }
    }
}
