using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.State;

namespace Lilo.Tests.Battery
{
    /// <summary>
    /// Proves a taken battery actually extends light: pickup fills the spare,
    /// install consumes it and refills the installed charge (spec 008).
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
        public void TakeThenInstallRefillsEmptyLamp()
        {
            _state.SetInstalledBatteryCharge(0f);
            _state.PickUpSpareBattery(_config.batteryDuration);

            Assert.IsTrue(_state.SpareBatterySlotOccupied);

            _state.InstallSpareBattery(_config.batteryDuration);

            Assert.AreEqual(_config.batteryDuration, _state.InstalledBatteryCharge);
            Assert.IsFalse(_state.SpareBatterySlotOccupied);
            Assert.AreEqual(0f, _state.SpareBatteryCharge);
        }

        [Test]
        public void PickupFillsSpareWithFullCharge()
        {
            _state.PickUpSpareBattery(_config.batteryDuration);

            Assert.IsTrue(_state.SpareBatterySlotOccupied);
            Assert.AreEqual(_config.batteryDuration, _state.SpareBatteryCharge);
        }
    }
}
