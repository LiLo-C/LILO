using UnityEngine;

namespace Lilo.Systems.Movement
{
    /// <summary>Resultant world-space velocity plus whether this frame counts as sprinting.</summary>
    public readonly struct MovementResult
    {
        public readonly Vector2 Velocity;
        public readonly bool IsSprinting;

        public MovementResult(Vector2 velocity, bool isSprinting)
        {
            Velocity = velocity;
            IsSprinting = isSprinting;
        }

        public static readonly MovementResult Zero = new MovementResult(Vector2.zero, false);
    }
}
