using System;
using System.Collections.Generic;
using NUnit.Framework;
using Lilo.Systems.Battery;

namespace Lilo.Tests.Battery
{
    /// <summary>EditMode tests for KeyboardBatteryRoster (3-of-7 dispenser selection).</summary>
    public class KeyboardBatteryRosterTests
    {
        [Test]
        public void PicksExactlyThreeDistinctIndicesOutOfSeven()
        {
            int[] picked = KeyboardBatteryRoster.PickLoadedIndices(3, 7, new Random(42));

            Assert.AreEqual(3, picked.Length);
            Assert.AreEqual(3, new HashSet<int>(picked).Count);
            foreach (int i in picked)
                Assert.That(i, Is.InRange(0, 6));
        }

        [Test]
        public void ClampsWhenCountExceedsTotal()
        {
            int[] picked = KeyboardBatteryRoster.PickLoadedIndices(9, 7, new Random(1));

            Assert.AreEqual(7, picked.Length);
            Assert.AreEqual(7, new HashSet<int>(picked).Count);
        }

        [Test]
        public void EmptyWhenNothingToPick()
        {
            Assert.IsEmpty(KeyboardBatteryRoster.PickLoadedIndices(0, 7, new Random(1)));
            Assert.IsEmpty(KeyboardBatteryRoster.PickLoadedIndices(3, 0, new Random(1)));
            Assert.IsEmpty(KeyboardBatteryRoster.PickLoadedIndices(-2, 7, new Random(1)));
        }

        [Test]
        public void SeededRngIsDeterministic()
        {
            int[] first = KeyboardBatteryRoster.PickLoadedIndices(3, 7, new Random(7));
            int[] second = KeyboardBatteryRoster.PickLoadedIndices(3, 7, new Random(7));

            CollectionAssert.AreEqual(first, second);
        }

        [Test]
        public void NullRngThrows()
        {
            Assert.Throws<ArgumentNullException>(() =>
                KeyboardBatteryRoster.PickLoadedIndices(3, 7, null));
        }
    }
}
