using Lilo.Config;
using UnityEngine;

namespace Lilo.Systems.Movement
{
    /// <summary>
    /// Pure, engine-lifecycle-independent movement math (spec FR-010). Deflection + config in,
    /// resultant velocity + IsSprinting out. No stamina, no sprint/battery coupling (FR-008/FR-009).
    /// </summary>
    public static class MovementSystem
    {
        public static MovementResult Compute(MovementInput input, GameConfig config)
        {
            // US3: dead zone first — below it is exactly zero, not just small (FR-002). Inclusive
            // upper bound: exactly at the dead zone already produces movement (spec Edge Cases).
            if (input.Magnitude < config.joystickDeadZone)
            {
                return MovementResult.Zero;
            }

            // US2: sprint threshold is inclusive and re-evaluated every call — never sticky (FR-004/FR-006).
            bool isSprinting = input.Magnitude >= config.sprintJoystickThreshold;
            float topSpeed = isSprinting ? config.walkSpeed * config.sprintMultiplier : config.walkSpeed;

            // Direction is normalized, while magnitude remains proportional to joystick
            // deflection: a light pull walks and a full pull reaches the sprint speed.
            Vector2 velocity = input.Direction * topSpeed * input.Magnitude;
            return new MovementResult(velocity, isSprinting);
        }
    }
}
