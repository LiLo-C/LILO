using Lilo.Config;
using Lilo.State;
using Lilo.Systems.Debug;
using NUnit.Framework;

namespace Lilo.Tests
{
    public class DebugOverlaySystemTests
    {
        private static GameConfig Config()
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<GameConfig>();
            config.noiseBaseRadius = 1f;
            config.noiseWalk = 1f;
            config.noiseSprint = 3f;
            return config;
        }

        [Test]
        public void ClassifyNoise_DistinguishesMovementPulseHidingAndSilence()
        {
            var config = Config();

            Assert.AreEqual(NoiseSource.Silent, DebugOverlaySystem.ClassifyNoise(config, false, false, false, 0f).Source);
            Assert.AreEqual(NoiseSource.Walk, DebugOverlaySystem.ClassifyNoise(config, false, true, false, 0f).Source);
            Assert.AreEqual(NoiseSource.Sprint, DebugOverlaySystem.ClassifyNoise(config, false, true, true, 0f).Source);
            Assert.AreEqual(NoiseSource.Pulse, DebugOverlaySystem.ClassifyNoise(config, false, true, false, 4f).Source);
            Assert.AreEqual(NoiseSource.Hiding, DebugOverlaySystem.ClassifyNoise(config, true, true, true, 4f).Source);
        }

        [Test]
        public void Format_ReportsAllTelemetryLines()
        {
            var text = DebugOverlaySystem.Format(new DebugOverlaySnapshot
            {
                LightState = Lilo.Systems.Flashlight.FlashlightLightState.Normal,
                LightRadius = 220f,
                LightIntensity = 1f,
                BatteryFraction = 0.84f,
                MonsterAvailable = true,
                MonsterState = MonsterState.Chase,
                MonsterDistance = 6.2f,
                NoiseRadius = 3f,
                NoiseSource = NoiseSource.Sprint,
            });

            StringAssert.Contains("LIGHT    Normal", text);
            StringAssert.Contains("BATTERY  84%", text);
            StringAssert.Contains("MONSTER  Chase", text);
            StringAssert.Contains("NOISE    3.0  Sprint", text);
        }

        [Test]
        public void Format_UsesDashWhenMonsterIsUnavailable()
        {
            var text = DebugOverlaySystem.Format(new DebugOverlaySnapshot { MonsterAvailable = false });
            StringAssert.Contains("MONSTER  —", text);
        }
    }
}
