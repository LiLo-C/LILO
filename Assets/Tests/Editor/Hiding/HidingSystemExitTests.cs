using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.Systems.Hiding;

namespace Lilo.Tests.Hiding
{
    public class HidingSystemExitTests
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
            _spot = new HidingSpotData(new Vector3(4f, 0f, 4f), new Vector3(4f, 0f, 2f));
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
        public void TryExit_WhileHiddenWithValidExitAnchor_ReturnsSuccessAndEntersExiting()
        {
            _system.TryEnter(_spot, anchorsValid: true);
            _system.Tick(0.5f);
            Assert.AreEqual(HidingState.Hidden, _system.CurrentState);

            var result = _system.TryExit(anchorsValid: true);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(HidingState.Exiting, _system.CurrentState);
        }

        [Test]
        public void TryExit_WhileVisibleOrEnteringOrExiting_ReturnsFailure()
        {
            // From Visible
            var result1 = _system.TryExit(anchorsValid: true);
            Assert.IsFalse(result1.Success);
            Assert.AreEqual(HidingState.Visible, _system.CurrentState);

            // From Entering
            _system.TryEnter(_spot, anchorsValid: true);
            var result2 = _system.TryExit(anchorsValid: true);
            Assert.IsFalse(result2.Success);
            Assert.AreEqual(HidingState.Entering, _system.CurrentState);

            // Reach Hidden then Exiting
            _system.Tick(0.5f);
            _system.TryExit(anchorsValid: true);
            Assert.AreEqual(HidingState.Exiting, _system.CurrentState);

            // From Exiting
            var result3 = _system.TryExit(anchorsValid: true);
            Assert.IsFalse(result3.Success);
            Assert.AreEqual(HidingState.Exiting, _system.CurrentState);
        }

        [Test]
        public void TryExit_WithBlockedExitAnchor_ReturnsFailureAndRemainsHidden()
        {
            _system.TryEnter(_spot, anchorsValid: true);
            _system.Tick(0.5f);
            Assert.AreEqual(HidingState.Hidden, _system.CurrentState);

            var result = _system.TryExit(anchorsValid: false);

            Assert.IsFalse(result.Success);
            Assert.IsNotEmpty(result.FailureReason);
            Assert.AreEqual(HidingState.Hidden, _system.CurrentState);
        }
    }
}
