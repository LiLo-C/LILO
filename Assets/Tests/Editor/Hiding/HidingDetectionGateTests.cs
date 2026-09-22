using NUnit.Framework;
using UnityEngine;
using Lilo.Systems.Hiding;
using Lilo.Systems.Monster;

namespace Lilo.Tests.Hiding
{
    public class HidingDetectionGateTests
    {
        [Test]
        public void Apply_WhenHidden_SuppressesDetection()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(5f, 0f, 5f));

            var gated = HidingDetectionGate.Apply(raw, HidingState.Hidden);

            Assert.IsFalse(gated.IsDetected);
            Assert.AreEqual(raw.SourcePosition, gated.SourcePosition);
        }

        [Test]
        public void Apply_WhenVisible_PassesSignalThroughUnchanged()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(5f, 0f, 5f));

            var gated = HidingDetectionGate.Apply(raw, HidingState.Visible);

            Assert.IsTrue(gated.IsDetected);
            Assert.AreEqual(raw.SourcePosition, gated.SourcePosition);
        }

        [Test]
        public void Apply_WhenEntering_DoesNotGrantImmunityEarly()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(5f, 0f, 5f));

            var gated = HidingDetectionGate.Apply(raw, HidingState.Entering);

            Assert.IsTrue(gated.IsDetected);
            Assert.AreEqual(raw.SourcePosition, gated.SourcePosition);
        }

        [Test]
        public void Apply_WhenExiting_EndsImmunityImmediately()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(5f, 0f, 5f));

            var gated = HidingDetectionGate.Apply(raw, HidingState.Exiting);

            Assert.IsTrue(gated.IsDetected);
            Assert.AreEqual(raw.SourcePosition, gated.SourcePosition);
        }

        [Test]
        public void Apply_HiddenThenExitingThenVisible_ResumesNormalDetection()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(2f, 0f, 2f));

            var step1 = HidingDetectionGate.Apply(raw, HidingState.Hidden);
            Assert.IsFalse(step1.IsDetected);

            var step2 = HidingDetectionGate.Apply(raw, HidingState.Exiting);
            Assert.IsTrue(step2.IsDetected);

            var step3 = HidingDetectionGate.Apply(raw, HidingState.Visible);
            Assert.IsTrue(step3.IsDetected);
        }

        [Test]
        public void Apply_WhenUndetected_NeverInvertsToDetected()
        {
            var raw = new MonsterDetectionSignal(false, Vector3.zero);

            var gated = HidingDetectionGate.Apply(raw, HidingState.Hidden);

            Assert.IsFalse(gated.IsDetected);
        }
    }
}
