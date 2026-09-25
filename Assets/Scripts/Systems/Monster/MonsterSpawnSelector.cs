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
        /// Selects the safest reachable candidate if none pass every strict rule.
        /// Reachability is never relaxed; visibility and authored separation are
        /// preferences because they should not leave the monster disabled.
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

                float score = entryDistance + Mathf.Min(objectiveDistance, objectiveClearance * 2f) * 1.5f;
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
