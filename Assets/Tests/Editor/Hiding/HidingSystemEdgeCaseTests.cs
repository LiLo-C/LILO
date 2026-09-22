using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.Systems.Hiding;

namespace Lilo.Tests.Hiding
{
    public class HidingSystemEdgeCaseTests
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
            _spot = new HidingSpotData(new Vector3(5f, 0f, 5f), new Vector3(5f, 0f, 3f));
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
        public void ForceExit_FromHiddenOrTransitioning_RestoresVisibleStateAndFiresEvent()
        {
            _system.TryEnter(_spot, anchorsValid: true);
            _system.Tick(0.5f);
            Assert.AreEqual(HidingState.Hidden, _system.CurrentState);

            var states = new List<HidingState>();
            _system.StateChanged += s => states.Add(s);

            _system.ForceExit();

            Assert.AreEqual(HidingState.Visible, _system.CurrentState);
            Assert.IsFalse(_system.CurrentSpot.IsOccupied);
            Assert.AreEqual(1, states.Count);
            Assert.AreEqual(HidingState.Visible, states[0]);
        }

        [Test]
        public void ForceExit_WhenAlreadyVisible_IsSafeNoOpAndFiresZeroEvents()
        {
            var states = new List<HidingState>();
            _system.StateChanged += s => states.Add(s);

            _system.ForceExit();

            Assert.AreEqual(HidingState.Visible, _system.CurrentState);
            Assert.IsFalse(_system.CurrentSpot.IsOccupied);
            Assert.AreEqual(0, states.Count);
        }

        [Test]
        public void ForceExit_CalledMidTransition_ResolvesImmediatelyToVisible()
        {
            _system.TryEnter(_spot, anchorsValid: true);
            _system.Tick(0.2f); // partially through Entering
            Assert.AreEqual(HidingState.Entering, _system.CurrentState);

            _system.ForceExit();

            Assert.AreEqual(HidingState.Visible, _system.CurrentState);
            Assert.AreEqual(0f, _system.TransitionTimer);

            // Ticking afterward does not resolve into Hidden
            _system.Tick(0.5f);
            Assert.AreEqual(HidingState.Visible, _system.CurrentState);
        }

        [Test]
        public void HidingSystem_PublicApi_ContainsNoMonsterCoupling()
        {
            var methods = typeof(HidingSystem).GetMethods();
            foreach (var m in methods)
            {
                Assert.IsFalse(m.Name.ToLower().Contains("monster"));
                foreach (var p in m.GetParameters())
                {
                    Assert.IsFalse(p.ParameterType.Name.ToLower().Contains("monster"));
                }
            }
        }
    }
}
