using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.Systems.Hiding;

namespace Lilo.Tests.Hiding
{
    public class HidingSystemEnterTests
    {
        private GameConfig _config;
        private HidingSystem _system;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();
            _config.hidingTransitionDuration = 0.5f;
            _system = new HidingSystem(_config);
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
        public void TryEnter_WithValidUnoccupiedSpotAndValidAnchors_ReturnsSuccessAndEntersEnteringState()
        {
            var spot = new HidingSpotData(new Vector3(1f, 0f, 1f), new Vector3(2f, 0f, 1f), isOccupied: false);

            var result = _system.TryEnter(spot, anchorsValid: true);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(HidingState.Entering, _system.CurrentState);
            Assert.IsTrue(_system.CurrentSpot.IsOccupied);
        }

        [Test]
        public void TryEnter_WithDefaultOrInvalidSpot_ReturnsFailureAndRemainsVisible()
        {
            var invalidSpot = default(HidingSpotData);

            var result = _system.TryEnter(invalidSpot, anchorsValid: true);

            Assert.IsFalse(result.Success);
            Assert.IsNotEmpty(result.FailureReason);
            Assert.AreEqual(HidingState.Visible, _system.CurrentState);
        }

        [Test]
        public void TryEnter_WithAlreadyOccupiedSpot_ReturnsFailureAndRemainsVisible()
        {
            var occupiedSpot = new HidingSpotData(new Vector3(1f, 0f, 1f), new Vector3(2f, 0f, 1f), isOccupied: true);

            var result = _system.TryEnter(occupiedSpot, anchorsValid: true);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(HidingState.Visible, _system.CurrentState);
        }

        [Test]
        public void TryEnter_CalledSecondTimeWhileEnteringOrHidden_ReturnsFailureAndDoesNotCorruptState()
        {
            var spot1 = new HidingSpotData(new Vector3(1f, 0f, 1f), new Vector3(2f, 0f, 1f));
            var spot2 = new HidingSpotData(new Vector3(5f, 0f, 5f), new Vector3(6f, 0f, 5f));

            var firstResult = _system.TryEnter(spot1, anchorsValid: true);
            Assert.IsTrue(firstResult.Success);
            Assert.AreEqual(HidingState.Entering, _system.CurrentState);

            var secondResult = _system.TryEnter(spot2, anchorsValid: true);
            Assert.IsFalse(secondResult.Success);
            Assert.AreEqual(HidingState.Entering, _system.CurrentState);
        }
    }
}
