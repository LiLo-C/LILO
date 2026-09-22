using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.Systems.Hiding;

namespace Lilo.Tests.Hiding
{
    public class HidingSystemTransitionTimingTests
    {
        private GameConfig _config;
        private HidingSystem _system;
        private HidingSpotData _spot;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();
            _config.hidingTransitionDuration = 0.5f;
            _system = new HidingSystem(_config);
            _spot = new HidingSpotData(new Vector3(10f, 0f, 10f), new Vector3(10f, 0f, 8f));
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }
        }

        [Test]
        public void Tick_WhileEntering_BelowDuration_KeepsStateEntering()
        {
            _system.TryEnter(_spot, anchorsValid: true);

            _system.Tick(0.2f);
            Assert.AreEqual(HidingState.Entering, _system.CurrentState);

            _system.Tick(0.25f);
            Assert.AreEqual(HidingState.Entering, _system.CurrentState);
        }

        [Test]
        public void Tick_WhileEntering_ReachesDuration_TransitionsToHiddenAndPlacesAtHideAnchor()
        {
            _system.TryEnter(_spot, anchorsValid: true);

            _system.Tick(0.5f);

            Assert.AreEqual(HidingState.Hidden, _system.CurrentState);
            Assert.AreEqual(_spot.HideAnchor, _system.CurrentPlayerPosition);
        }

        [Test]
        public void Tick_WhileExiting_ReachesDuration_TransitionsToVisibleAndPlacesAtExitAnchor()
        {
            _system.TryEnter(_spot, anchorsValid: true);
            _system.Tick(0.5f);
            Assert.AreEqual(HidingState.Hidden, _system.CurrentState);

            var exitResult = _system.TryExit(anchorsValid: true);
            Assert.IsTrue(exitResult.Success);
            Assert.AreEqual(HidingState.Exiting, _system.CurrentState);

            _system.Tick(0.2f);
            Assert.AreEqual(HidingState.Exiting, _system.CurrentState);

            _system.Tick(0.3f);
            Assert.AreEqual(HidingState.Visible, _system.CurrentState);
            Assert.AreEqual(_spot.ExitAnchor, _system.CurrentPlayerPosition);
            Assert.IsFalse(_system.CurrentSpot.IsOccupied);
        }
    }
}
