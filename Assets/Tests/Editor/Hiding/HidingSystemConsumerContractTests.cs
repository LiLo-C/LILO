using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.Systems.Hiding;

namespace Lilo.Tests.Hiding
{
    public class HidingSystemConsumerContractTests
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
            _spot = new HidingSpotData(new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, 0f));
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
        public void StateChanged_FanOut_DeliversIdenticalTransitionsToAudioLightAndDetectionSubscribers()
        {
            // Three independent consumer listeners (audio, light, detection)
            var audioEvents = new List<HidingState>();
            var lightEvents = new List<HidingState>();
            var detectionEvents = new List<HidingState>();

            _system.StateChanged += state => audioEvents.Add(state);
            _system.StateChanged += state => lightEvents.Add(state);
            _system.StateChanged += state => detectionEvents.Add(state);

            // Run full lifecycle: Visible -> Entering -> Hidden -> Exiting -> Visible
            _system.TryEnter(_spot, anchorsValid: true);
            _system.Tick(0.5f);
            _system.TryExit(anchorsValid: true);
            _system.Tick(0.5f);

            var expectedTransitions = new List<HidingState>
            {
                HidingState.Entering,
                HidingState.Hidden,
                HidingState.Exiting,
                HidingState.Visible
            };

            CollectionAssert.AreEqual(expectedTransitions, audioEvents);
            CollectionAssert.AreEqual(expectedTransitions, lightEvents);
            CollectionAssert.AreEqual(expectedTransitions, detectionEvents);
        }
    }
}
