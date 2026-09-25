using Lilo.Config;
using Lilo.State;
using Lilo.Systems.Flashlight;
using UnityEngine;

namespace Lilo.Systems.Debug
{
    public struct NoiseReadout
    {
        public float Radius;
        public NoiseSource Source;
    }

    public struct DebugOverlaySnapshot
    {
        public FlashlightLightState LightState;
        public float LightRadius;
        public float LightIntensity;
        public float BatteryFraction;
        public bool MonsterAvailable;
        public MonsterState MonsterState;
        public float MonsterDistance;
        public float NoiseRadius;
        public NoiseSource NoiseSource;
        public bool IsHiding;
    }

    public static class DebugOverlaySystem
    {
        public static NoiseReadout ClassifyNoise(
            GameConfig config,
            bool isHiding,
            bool isMoving,
            bool isSprinting,
            float largestPulseRadius)
        {
            if (isHiding)
                return new NoiseReadout { Radius = 0f, Source = NoiseSource.Hiding };

            float movementRadius = Lilo.Systems.Monster.MonsterNoise.MovementRadius(config, false, isMoving, isSprinting);
            if (largestPulseRadius > movementRadius && largestPulseRadius > 0f)
                return new NoiseReadout { Radius = largestPulseRadius, Source = NoiseSource.Pulse };
            if (movementRadius > 0f)
            {
                return new NoiseReadout
                {
                    Radius = movementRadius,
                    Source = isSprinting ? NoiseSource.Sprint : NoiseSource.Walk,
                };
            }
            return new NoiseReadout { Radius = 0f, Source = NoiseSource.Silent };
        }

        public static string Format(DebugOverlaySnapshot snapshot)
        {
            string monster = snapshot.MonsterAvailable
                ? $"{snapshot.MonsterState}  d={snapshot.MonsterDistance:0.0}"
                : "—";
            return $"LIGHT    {snapshot.LightState}  r={snapshot.LightRadius:0.0}  i={snapshot.LightIntensity:0.00}\n"
                + $"BATTERY  {snapshot.BatteryFraction * 100f:0}%\n"
                + $"MONSTER  {monster}\n"
                + $"NOISE    {snapshot.NoiseRadius:0.0}  {snapshot.NoiseSource}";
        }
    }
}
