using System;
using System.Collections.Generic;

namespace Lilo.Systems.Battery
{
    /// <summary>
    /// Pure keyboard-battery selection math: pick N distinct slot indices out of a
    /// registry of M keyboards per floor entry. No MonoBehaviour or scene-graph
    /// dependency (constitution Principle III).
    /// </summary>
    public static class KeyboardBatteryRoster
    {
        /// <summary>
        /// Returns up to <paramref name="count"/> distinct indices in [0,
        /// <paramref name="total"/>), in selection order. Clamps when count exceeds
        /// total; returns empty when total is zero or count is not positive.
        /// </summary>
        public static int[] PickLoadedIndices(int count, int total, Random rng)
        {
            if (rng == null)
                throw new ArgumentNullException(nameof(rng));
            if (count <= 0 || total <= 0)
                return Array.Empty<int>();

            int take = Math.Min(count, total);
            var remaining = new List<int>(total);
            for (int i = 0; i < total; i++)
                remaining.Add(i);

            var picked = new int[take];
            for (int k = 0; k < take; k++)
            {
                int at = rng.Next(remaining.Count);
                picked[k] = remaining[at];
                remaining.RemoveAt(at);
            }
            return picked;
        }

        /// <summary>
        /// Adds a take's worth of charge to the lamp, clamped at full.
        /// Pure so the take-to-light math is unit-testable.
        /// </summary>
        public static float TopUpCharge(float currentCharge, float amount, float maxCharge)
        {
            if (maxCharge <= 0f) return 0f;
            float result = currentCharge + amount;
            if (result < 0f) return 0f;
            if (result > maxCharge) return maxCharge;
            return result;
        }
    }
}
