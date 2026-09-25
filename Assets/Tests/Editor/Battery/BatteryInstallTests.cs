using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.State;

namespace Lilo.Tests.Battery
{
    /// <summary>
    /// Proves collected batteries directly add charge and cannot overfill the lamp.
    /// </summary>
    public class BatteryInstallTests
    {
        private GameState _state;
        private GameConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();
            _state = new GameState(_config);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void BatteryAddsTwentyFivePercentToInstalledCharge()
        {
            _state.SetInstalledBatteryCharge(0f);
            _state.AddInstalledBatteryCharge(_config.batteryDuration * 0.25f, _config.batteryDuration);

            Assert.AreEqual(_config.batteryDuration * 0.25f, _state.InstalledBatteryCharge);
        }

        [Test]
        public void BatteryChargeIsClampedAtFull()
        {
            _state.SetInstalledBatteryCharge(_config.batteryDuration * 0.9f);
            _state.AddInstalledBatteryCharge(_config.batteryDuration * 0.25f, _config.batteryDuration);

            Assert.AreEqual(_config.batteryDuration, _state.InstalledBatteryCharge);
        }
    }
}
