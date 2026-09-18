using UnityEngine;

namespace Lilo.Systems.Movement
{
    /// <summary>Per-frame joystick reading: direction (normalized) + magnitude in 0..1.</summary>
    public readonly struct MovementInput
    {
        public readonly Vector2 Direction;
        public readonly float Magnitude;

        public MovementInput(Vector2 direction, float magnitude)
        {
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.zero;
            Magnitude = Mathf.Clamp01(magnitude);
        }

        public static readonly MovementInput Zero = new MovementInput(Vector2.zero, 0f);
    }
}
