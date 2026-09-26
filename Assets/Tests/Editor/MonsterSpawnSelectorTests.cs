using System;
using System.Collections.Generic;
using Lilo.Systems.Monster;
using NUnit.Framework;
using UnityEngine;

namespace Lilo.Tests
{
    public class MonsterSpawnSelectorTests
    {
        private static readonly IReadOnlyList<Vector3> Objectives = new[] { new Vector3(10f, 0f, 0f) };

        [Test]
        public void IsValid_RequiresReachabilityDistanceOcclusionAndObjectiveClearance()
        {
            Vector3 entry = Vector3.zero;
            var valid = new MonsterSpawnCandidate(new Vector3(8f, 0f, 8f), true, false);

            Assert.IsTrue(MonsterSpawnSelector.IsValid(valid, entry, Objectives, 8f, 3f));
            Assert.IsFalse(MonsterSpawnSelector.IsValid(new MonsterSpawnCandidate(valid.Position, false, false), entry, Objectives, 8f, 3f));
            Assert.IsFalse(MonsterSpawnSelector.IsValid(new MonsterSpawnCandidate(valid.Position, true, true), entry, Objectives, 8f, 3f));
            Assert.IsFalse(MonsterSpawnSelector.IsValid(new MonsterSpawnCandidate(new Vector3(2f, 0f, 0f), true, false), entry, Objectives, 8f, 3f));
            Assert.IsFalse(MonsterSpawnSelector.IsValid(new MonsterSpawnCandidate(new Vector3(9f, 0f, 0f), true, false), entry, Objectives, 8f, 3f));
        }

        [Test]
        public void ChooseValidIndex_OnlyReturnsValidatedPreset()
        {
            var candidates = new[]
            {
                new MonsterSpawnCandidate(new Vector3(2f, 0f, 0f), true, false),
                new MonsterSpawnCandidate(new Vector3(8f, 0f, 8f), true, false),
                new MonsterSpawnCandidate(new Vector3(12f, 0f, 0f), true, true),
            };

            int selected = MonsterSpawnSelector.ChooseValidIndex(
                candidates,
                Vector3.zero,
                Objectives,
                8f,
                3f,
                new System.Random(7));

            Assert.AreEqual(1, selected);
        }

        [Test]
        public void ChooseValidIndex_ReturnsMinusOne_WhenNoPresetIsValid()
        {
            var candidates = new[]
            {
                new MonsterSpawnCandidate(new Vector3(1f, 0f, 0f), true, false),
                new MonsterSpawnCandidate(new Vector3(8f, 0f, 8f), false, false),
            };

            int selected = MonsterSpawnSelector.ChooseValidIndex(
                candidates,
                Vector3.zero,
                Objectives,
                8f,
                3f,
                new System.Random(7));

            Assert.AreEqual(-1, selected);
        }

        [Test]
        public void ChooseBalancedIndex_PrefersMiddleOfSafeSpawnBand()
        {
            var candidates = new[]
            {
                new MonsterSpawnCandidate(new Vector3(11.5f, 0f, 0f), true, false),
                new MonsterSpawnCandidate(new Vector3(16f, 0f, 0f), true, false),
            };

            int selected = MonsterSpawnSelector.ChooseBalancedIndex(
                candidates, Vector3.zero, System.Array.Empty<Vector3>(), 8f, 15f, 3f,
                new System.Random(7));

            Assert.AreEqual(0, selected);
        }

        [Test]
        public void RearBlindSpot_MufflesDirectlyBehindButNotFrontOrSide()
        {
            Vector3 observer = Vector3.zero;
            Vector3 forward = Vector3.forward;

            Assert.IsTrue(MonsterNoise.IsInRearBlindSpot(observer, forward,
                Vector3.back * 2f, 32f));
            Assert.IsFalse(MonsterNoise.IsInRearBlindSpot(observer, forward,
                Vector3.forward * 2f, 32f));
            Assert.IsFalse(MonsterNoise.IsInRearBlindSpot(observer, forward,
                Vector3.right * 2f, 32f));
        }

        [Test]
        public void ChooseReachableFallbackIndex_UsesReachableCandidateWhenVisibilityRejectsAll()
        {
            var candidates = new List<MonsterSpawnCandidate>
            {
                new MonsterSpawnCandidate(new Vector3(12f, 0f, 0f), false, false),
                new MonsterSpawnCandidate(new Vector3(10f, 0f, 0f), true, true),
                new MonsterSpawnCandidate(new Vector3(1f, 0f, 0f), true, false),
            };

            int selected = MonsterSpawnSelector.ChooseReachableFallbackIndex(
                candidates, Vector3.zero, Objectives, 8f, 3f, new System.Random(123));

            Assert.AreEqual(1, selected);
        }
    }
}
