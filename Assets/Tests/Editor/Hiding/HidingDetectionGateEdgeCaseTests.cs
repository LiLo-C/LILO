using NUnit.Framework;
using UnityEngine;
using Lilo.Systems.Hiding;
using Lilo.Systems.Monster;

namespace Lilo.Tests.Hiding
{
    public class HidingDetectionGateEdgeCaseTests
    {
        [Test]
        public void Apply_IsPureAndStatelessAcrossRepeatedCalls()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(3f, 0f, 3f));

            var res1 = HidingDetectionGate.Apply(raw, HidingState.Hidden);
            var res2 = HidingDetectionGate.Apply(raw, HidingState.Hidden);
            var res3 = HidingDetectionGate.Apply(raw, HidingState.Hidden);

            Assert.IsFalse(res1.IsDetected);
            Assert.IsFalse(res2.IsDetected);
            Assert.IsFalse(res3.IsDetected);
        }

        [Test]
        public void Apply_AfterResetToVisible_ResumesNormalBaselineImmediately()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(1f, 0f, 1f));

            var hiddenResult = HidingDetectionGate.Apply(raw, HidingState.Hidden);
            Assert.IsFalse(hiddenResult.IsDetected);

            // Reset happened (e.g. death or floor transition back to Visible)
            var postResetResult = HidingDetectionGate.Apply(raw, HidingState.Visible);
            Assert.IsTrue(postResetResult.IsDetected);
        }

        [Test]
        public void Apply_IsIndependentOfSpotIdentity()
        {
            var raw = new MonsterDetectionSignal(true, new Vector3(4f, 0f, 4f));

            // Gate only takes enum and signal, proving spot identity does not factor into gating
            var result = HidingDetectionGate.Apply(raw, HidingState.Hidden);
            Assert.IsFalse(result.IsDetected);
        }

        [Test]
        public void HidingDetectionGate_PublicApi_ContainsNoCatchSignalCancellation()
        {
            var methods = typeof(HidingDetectionGate).GetMethods();
            foreach (var m in methods)
            {
                Assert.IsFalse(m.Name.ToLower().Contains("cancelcatch"));
                Assert.IsFalse(m.Name.ToLower().Contains("abortcatch"));
            }
        }
    }
}
