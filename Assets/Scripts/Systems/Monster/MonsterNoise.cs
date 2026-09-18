using Lilo.Config;

namespace Lilo.Systems.Monster
{
    /// <summary>
    /// Pure player-noise math (GDD Ch. 7.1, spec 002 FR-012). Movement noise is
    /// continuous; interaction/swap noise enters the brain as one-shot pulses.
    /// </summary>
    public static class MonsterNoise
    {
        public static float MovementRadius(GameConfig config, bool isHiding, bool isMoving, bool isSprinting)
        {
            if (config == null || isHiding || !isMoving)
                return 0f;

            float multiplier = isSprinting ? config.noiseSprint : config.noiseWalk;
            return config.noiseBaseRadius * multiplier;
        }
    }
}
