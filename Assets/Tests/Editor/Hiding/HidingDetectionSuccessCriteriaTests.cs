using NUnit.Framework;
using UnityEngine;
using Lilo.Systems.Hiding;
using Lilo.Systems.Monster;

namespace Lilo.Tests.Hiding
{
    public class HidingDetectionSuccessCriteriaTests
    {
        [Test]
        public void SuccessCriteria_SC001_HiddenTrialsProduceZeroDetections()
        {
            var rng = new System.Random(42);
            for (int i = 0; i < 100; i++)
            {
                var randomPos = new Vector3((float)rng.NextDouble() * 10f, 0f, (float)rng.NextDouble() * 10f);
                var rawSignal = new MonsterDetectionSignal(true, randomPos);

                var gated = HidingDetectionGate.Apply(rawSignal, HidingState.Hidden);

                Assert.IsFalse(gated.IsDetected, $"Trial {i} failed: expected false while Hidden.");
            }
        }

        [Test]
        public void SuccessCriteria_SC002_VisibleAndPostExitTrialsMatchBaselineExactly()
        {
            var rng = new System.Random(42);
            for (int i = 0; i < 100; i++)
            {
                var randomPos = new Vector3((float)rng.NextDouble() * 10f, 0f, (float)rng.NextDouble() * 10f);
                bool baselineDetection = rng.NextDouble() > 0.5;
                var rawSignal = new MonsterDetectionSignal(baselineDetection, randomPos);

                // Visible baseline check
                var visibleGated = HidingDetectionGate.Apply(rawSignal, HidingState.Visible);
                Assert.AreEqual(baselineDetection, visibleGated.IsDetected, $"Trial {i} (Visible) did not match baseline.");

                // Post-exit baseline check
                var postExitGated = HidingDetectionGate.Apply(rawSignal, HidingState.Visible);
                Assert.AreEqual(baselineDetection, postExitGated.IsDetected, $"Trial {i} (Post-Exit) did not match baseline.");
            }
        }
    }
}
