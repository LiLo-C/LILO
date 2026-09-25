using System.Collections.Generic;
using UnityEngine;

namespace Lilo.Systems.Monster
{
    public readonly struct MonsterSpawnCandidate
    {
        public readonly Vector3 Position;
        public readonly bool IsReachable;
        public readonly bool IsVisibleFromEntry;

        public MonsterSpawnCandidate(Vector3 position, bool isReachable, bool isVisibleFromEntry)
        {
            Position = position;
            IsReachable = isReachable;
            IsVisibleFromEntry = isVisibleFromEntry;
        }
    }

    /// <summary>Pure preset validation and selection for spec 002 FR-002/FR-003.</summary>
    public static class MonsterSpawnSelector
    {
        public static bool IsValid(
            MonsterSpawnCandidate candidate,
            Vector3 playerEntry,
            IReadOnlyList<Vector3> objectives,
            float minimumEntryDistance,
            float objectiveClearance)
        {
            if (!candidate.IsReachable || candidate.IsVisibleFromEntry)
                return false;
            if (MonsterBrain.DistXZ(candidate.Position, playerEntry) < minimumEntryDistance)
                return false;

            if (objectives != null)
            {
                for (int i = 0; i < objectives.Count; i++)
                {
                    if (MonsterBrain.DistXZ(candidate.Position, objectives[i]) < objectiveClearance)
                        return false;
                }
            }

            return true;
        }

        public static int ChooseValidIndex(
            IReadOnlyList<MonsterSpawnCandidate> candidates,
            Vector3 playerEntry,
            IReadOnlyList<Vector3> objectives,
            float minimumEntryDistance,
            float objectiveClearance,
            System.Random rng)
        {
            if (candidates == null || rng == null)
                return -1;

            var validIndices = new List<int>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (IsValid(candidates[i], playerEntry, objectives, minimumEntryDistance, objectiveClearance))
                    validIndices.Add(i);
            }

            return validIndices.Count == 0 ? -1 : validIndices[rng.Next(validIndices.Count)];
        }

        /// <summary>
        /// Picks a safe candidate inside the spawn distance band, preferring the
        /// middle of the band so large floors do not place the monster at the far edge.
        /// </summary>
        public static int ChooseBalancedIndex(
            IReadOnlyList<MonsterSpawnCandidate> candidates,
            Vector3 playerEntry,
            IReadOnlyList<Vector3> objectives,
            float minimumEntryDistance,
            float maximumEntryDistance,
            float objectiveClearance,
            System.Random rng)
        {
            if (candidates == null || rng == null || maximumEntryDistance < minimumEntryDistance)
                return -1;

            float preferredDistance = (minimumEntryDistance + maximumEntryDistance) * 0.5f;
            int best = -1;
            float bestScore = float.PositiveInfinity;
            for (int i = 0; i < candidates.Count; i++)
            {
                MonsterSpawnCandidate candidate = candidates[i];
                float distance = MonsterBrain.DistXZ(candidate.Position, playerEntry);
                if (!IsValid(candidate, playerEntry, objectives, minimumEntryDistance, objectiveClearance)
                    || distance > maximumEntryDistance)
                    continue;

                float score = Mathf.Abs(distance - preferredDistance)
                    + (float)rng.NextDouble() * 0.75f;
                if (score >= bestScore) continue;
                best = i;
                bestScore = score;
            }
            return best;
        }

        /// <summary>
        /// Selects a reachable, reasonably distant fallback if strict candidate
        /// validation yields no result. A compact floor can still use its farthest point.
        /// </summary>
        public static int ChooseReachableFallbackIndex(
            IReadOnlyList<MonsterSpawnCandidate> candidates,
            Vector3 playerEntry,
            IReadOnlyList<Vector3> objectives,
            float minimumEntryDistance,
            float objectiveClearance,
            System.Random rng)
        {
            if (candidates == null || rng == null)
                return -1;

            int best = -1;
            float bestScore = float.NegativeInfinity;
            float minimumFallbackDistance = Mathf.Max(2.5f, minimumEntryDistance * 0.5f);
            float preferredDistance = minimumEntryDistance + Mathf.Max(2f, minimumEntryDistance * 0.4f);
            for (int i = 0; i < candidates.Count; i++)
            {
                MonsterSpawnCandidate candidate = candidates[i];
                if (!candidate.IsReachable) continue;

                float entryDistance = MonsterBrain.DistXZ(candidate.Position, playerEntry);
                if (entryDistance < minimumFallbackDistance) continue;

                float objectiveDistance = float.PositiveInfinity;
                if (objectives != null)
                {
                    for (int j = 0; j < objectives.Count; j++)
                        objectiveDistance = Mathf.Min(objectiveDistance,
                            MonsterBrain.DistXZ(candidate.Position, objectives[j]));
                }

                float score = -Mathf.Abs(entryDistance - preferredDistance)
                    + Mathf.Min(objectiveDistance, objectiveClearance * 2f) * 1.5f;
                if (candidate.IsVisibleFromEntry) score -= 3f;
                score += (float)rng.NextDouble() * 0.5f;
                if (score <= bestScore) continue;

                best = i;
                bestScore = score;
            }

            if (best >= 0) return best;

            // On a very compact floor, use the farthest reachable location even
            // when the preferred entry distance cannot be met.
            for (int i = 0; i < candidates.Count; i++)
            {
                MonsterSpawnCandidate candidate = candidates[i];
                if (!candidate.IsReachable) continue;
                float score = MonsterBrain.DistXZ(candidate.Position, playerEntry)
                    + (float)rng.NextDouble() * 0.5f;
                if (score <= bestScore) continue;
                best = i;
                bestScore = score;
            }
            return best;
        }
    }
}
