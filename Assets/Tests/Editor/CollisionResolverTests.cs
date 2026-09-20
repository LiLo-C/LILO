using NUnit.Framework;
using UnityEngine;
using Lilo.Systems.Collision;
using System.Collections.Generic;

namespace Lilo.Tests
{
    public class CollisionResolverTests
    {
        private static readonly Obstacle UnitBox = new Obstacle(
            new Vector2(0f, 0f),
            new Vector2(1f, 1f));

        [Test]
        public void ResolveSingle_SideOverlap_PushesOutWithoutChangingTangentialAxis()
        {
            CollisionResult result = CollisionResolver.ResolveSingle(
                new Vector2(-0.25f, 0.75f),
                0.5f,
                UnitBox);

            Assert.That(result.Position.x, Is.EqualTo(-0.5f).Within(0.0001f));
            Assert.That(result.Position.y, Is.EqualTo(0.75f).Within(0.0001f));
            Assert.IsTrue(result.Collided);
        }

        [Test]
        public void ResolveSingle_NoOverlapOrExactTouch_ReturnsInputUnchanged()
        {
            Vector2 clear = new Vector2(-0.75f, 0.75f);
            Vector2 touching = new Vector2(-0.5f, 0.75f);

            Assert.AreEqual(clear, CollisionResolver.ResolveSingle(clear, 0.5f, UnitBox).Position);
            CollisionResult touchResult = CollisionResolver.ResolveSingle(touching, 0.5f, UnitBox);
            Assert.AreEqual(touching, touchResult.Position);
            Assert.IsFalse(touchResult.Collided);
        }

        [TestCase(-0.49f)]
        [TestCase(-0.25f)]
        [TestCase(-0.01f)]
        public void ResolveSingle_HeadOnOverlap_StopsFlushAndIsIdempotent(float inputX)
        {
            Vector2 input = new Vector2(inputX, 0.5f);
            CollisionResult first = CollisionResolver.ResolveSingle(input, 0.5f, UnitBox);
            CollisionResult second = CollisionResolver.ResolveSingle(first.Position, 0.5f, UnitBox);

            Assert.That(first.Position.x, Is.EqualTo(-0.5f).Within(0.0001f));
            Assert.That(first.Position.y, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.AreEqual(first.Position, second.Position);
            Assert.IsFalse(second.Collided);
        }

        [Test]
        public void ResolveSingle_CornerOverlap_UsesRadialMinimumPushOut()
        {
            CollisionResult result = CollisionResolver.ResolveSingle(
                new Vector2(-0.25f, -0.25f),
                0.5f,
                UnitBox);

            float expected = -0.3535534f;
            Assert.That(result.Position.x, Is.EqualTo(expected).Within(0.0001f));
            Assert.That(result.Position.y, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void ResolveMany_PerpendicularWalls_ConvergesAndRemainsStable()
        {
            var obstacles = new List<Obstacle>
            {
                new Obstacle(new Vector2(0f, -10f), new Vector2(1f, 1f)),
                new Obstacle(new Vector2(-10f, 0f), new Vector2(1f, 1f)),
            };

            CollisionResult first = CollisionResolver.ResolveMany(new Vector2(-0.25f, -0.25f), 0.5f, obstacles);
            CollisionResult second = CollisionResolver.ResolveMany(first.Position, 0.5f, obstacles);

            Assert.That(first.Position.x, Is.EqualTo(-0.5f).Within(0.0001f));
            Assert.That(first.Position.y, Is.EqualTo(-0.5f).Within(0.0001f));
            Assert.AreEqual(first.Position, second.Position);
            Assert.IsFalse(second.Collided);
        }

        [Test]
        public void ResolveSingle_OverlappingSpawn_ChoosesNearestExpandedFace()
        {
            var obstacle = new Obstacle(Vector2.zero, new Vector2(2f, 2f));
            Vector2 input = new Vector2(0.25f, 0.75f);

            CollisionResult result = CollisionResolver.ResolveSingle(input, 0.5f, obstacle);

            Assert.AreEqual(new Vector2(-0.5f, 0.75f), result.Position);
            Assert.That(Vector2.Distance(input, result.Position), Is.EqualTo(0.75f).Within(0.0001f));
        }

        [TestCase(0f, 0f, 0f, 1f)]
        [TestCase(0f, 0f, 1f, 0f)]
        [TestCase(1f, 1f, 0f, 2f)]
        public void ResolveSingle_DegenerateObstacle_IsIgnored(float minX, float minY, float maxX, float maxY)
        {
            Vector2 input = new Vector2(0.25f, 0.25f);
            CollisionResult result = CollisionResolver.ResolveSingle(
                input,
                0.5f,
                new Obstacle(new Vector2(minX, minY), new Vector2(maxX, maxY)));

            Assert.AreEqual(input, result.Position);
            Assert.IsFalse(result.Collided);
            Assert.IsFalse(float.IsNaN(result.Position.x));
            Assert.IsFalse(float.IsInfinity(result.Position.y));
        }

        [Test]
        public void ResolveMany_ImpossibleNarrowGap_IsDeterministicAndBounded()
        {
            var obstacles = new List<Obstacle>
            {
                new Obstacle(new Vector2(-2f, -1f), new Vector2(0f, 1f)),
                new Obstacle(new Vector2(0.75f, -1f), new Vector2(2f, 1f)),
            };
            Vector2 input = new Vector2(0.375f, 0f);

            CollisionResult first = CollisionResolver.ResolveMany(input, 0.5f, obstacles);
            CollisionResult repeated = CollisionResolver.ResolveMany(input, 0.5f, obstacles);

            Assert.AreEqual(first.Position, repeated.Position);
            Assert.That(first.Position.x, Is.InRange(-0.5f, 1.25f));
            Assert.That(first.Position.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.IsFalse(float.IsNaN(first.Position.x));
            Assert.IsFalse(float.IsInfinity(first.Position.x));
        }
    }
}
