using Lilo.Config;
using Lilo.MonoBehaviours.Interaction;
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

        [Test]
        public void PickupIgnoresItsCoarseFurnitureProxiesButStillBlocksCubicleWalls()
        {
            var cubicle = new GameObject("Pickup Test Cubicle");
            var player = new GameObject("Pickup Test Player");
            var key = new GameObject("Pickup Test Key");
            try
            {
                cubicle.transform.position = new Vector3(1000f, 0f, 1000f);
                player.transform.position = cubicle.transform.position + Vector3.up * 0.61f;
                var body = player.AddComponent<CharacterController>();
                body.height = 1.8f;
                body.center = new Vector3(0f, 0.93f, 0f);
                body.radius = 0.18f;

                var tableObject = new GameObject("Approved Table");
                tableObject.transform.SetParent(cubicle.transform, false);
                var table = tableObject.AddComponent<AccessKeySpawnTable>();
                var surface = new GameObject("Surface").transform;
                surface.SetParent(table.transform, false);
                surface.localPosition = new Vector3(0f, 1.86f, 1.4f);

                // Match the Level 2 proxy heights, which exceed the actual key.
                var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                desk.name = "DeskCollider";
                desk.transform.SetParent(cubicle.transform, false);
                desk.transform.localPosition = new Vector3(0f, 1.9f, 1.3f);
                desk.transform.localScale = new Vector3(1.2f, 1.54f, 0.84f);
                var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.name = "ChairCollider";
                chair.transform.SetParent(cubicle.transform, false);
                chair.transform.localPosition = new Vector3(0f, 1.69f, 0.6f);
                chair.transform.localScale = new Vector3(0.82f, 1.06f, 0.66f);

                table.Configure(surface, 0.12f, new[] { desk.GetComponent<Collider>() });
                Assert.IsTrue(table.TryGetKeyPosition(out Vector3 position));
                key.transform.position = position;
                var pickup = key.AddComponent<AccessKeyPickup>();
                pickup.Configure("test-key", _config, table);
                Physics.SyncTransforms();
                Assert.IsTrue(pickup.HasClearPickupPath(player.transform),
                    "The supporting desk and its chair proxies must not prevent reaching the tabletop.");

                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "WallNorth";
                wall.transform.SetParent(cubicle.transform, false);
                wall.transform.localPosition = new Vector3(0f, 1.5f, 0.9f);
                wall.transform.localScale = new Vector3(3f, 3f, 0.1f);
                Physics.SyncTransforms();
                Assert.IsFalse(pickup.HasClearPickupPath(player.transform),
                    "A wall in the same cubicle must still block pickup.");
            }
            finally
            {
                Object.DestroyImmediate(key);
                Object.DestroyImmediate(player);
                Object.DestroyImmediate(cubicle);
                Physics.SyncTransforms();
            }
        }
    }
}
