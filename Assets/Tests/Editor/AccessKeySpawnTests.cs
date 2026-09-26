using Lilo.Config;
using Lilo.State;
using NUnit.Framework;
using UnityEngine;

namespace Lilo.Tests
{
    public sealed class AccessKeySpawnTests
    {
        private GameConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void SameFloorRespawnKeepsKeyLocation_NewRunClearsIt()
        {
            var state = new GameState(_config);
            Vector3 firstLocation = new Vector3(4f, 1.2f, -3f);
            state.SetAccessKeySpawnPosition(FloorId.Floor52, firstLocation);
            state.AddKey("access-key-Floor52");

            state.ResetForFloorRestart(_config);

            Assert.IsFalse(state.HasKey("access-key-Floor52"));
            Assert.IsTrue(state.TryGetAccessKeySpawnPosition(FloorId.Floor52, out Vector3 respawnLocation));
            Assert.AreEqual(firstLocation, respawnLocation);

            state.StartNewRun(_config);
            Assert.IsFalse(state.TryGetAccessKeySpawnPosition(FloorId.Floor52, out _));
        }
    }
}
