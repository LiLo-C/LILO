using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.Systems.Hiding;

namespace Lilo.Tests.Hiding
{
    public class HidingSystemEventsTests
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
            _spot = new HidingSpotData(new Vector3(2f, 0f, 2f), new Vector3(2f, 0f, 0f));
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
        public void StateChanged_FiresExactlyOncePerValidTransition()
        {
            var receivedStates = new List<HidingState>();
            _system.StateChanged += state => receivedStates.Add(state);

            // 1. Enter: Visible -> Entering
            _system.TryEnter(_spot, anchorsValid: true);
            Assert.AreEqual(1, receivedStates.Count);
            Assert.AreEqual(HidingState.Entering, receivedStates[0]);

            // 2. Tick to completion: Entering -> Hidden
            _system.Tick(0.5f);
            Assert.AreEqual(2, receivedStates.Count);
            Assert.AreEqual(HidingState.Hidden, receivedStates[1]);

            // 3. Exit: Hidden -> Exiting
            _system.TryExit(anchorsValid: true);
            Assert.AreEqual(3, receivedStates.Count);
            Assert.AreEqual(HidingState.Exiting, receivedStates[2]);

            // 4. Tick to completion: Exiting -> Visible
            _system.Tick(0.5f);
            Assert.AreEqual(4, receivedStates.Count);
            Assert.AreEqual(HidingState.Visible, receivedStates[3]);
        }

        [Test]
        public void StateChanged_DoesNotFireOnFailedAttempts()
        {
            var receivedStates = new List<HidingState>();
            _system.StateChanged += state => receivedStates.Add(state);

            // Failed enter (invalid spot)
            _system.TryEnter(default, anchorsValid: true);
            Assert.AreEqual(0, receivedStates.Count);

            // Failed exit (while visible)
            _system.TryExit(anchorsValid: true);
            Assert.AreEqual(0, receivedStates.Count);
        }
    }
}
