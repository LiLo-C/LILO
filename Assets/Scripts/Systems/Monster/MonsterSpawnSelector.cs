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
        /// Picks randomly from safe candidates inside the spawn distance band.
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

            var validIndices = new List<int>();
            for (int i = 0; i < candidates.Count; i++)
            {
                MonsterSpawnCandidate candidate = candidates[i];
                float distance = MonsterBrain.DistXZ(candidate.Position, playerEntry);
                if (!IsValid(candidate, playerEntry, objectives, minimumEntryDistance, objectiveClearance)
                    || distance > maximumEntryDistance)
                    continue;
                validIndices.Add(i);
            }
            return validIndices.Count == 0 ? -1 : validIndices[rng.Next(validIndices.Count)];
        }

        /// <summary>
        /// Selects a random reachable fallback that still respects minimum player
        /// distance, preferring candidates clear of objectives.
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

            var farReachable = new List<int>();
            var farClear = new List<int>();
            var farClearHidden = new List<int>();
            for (int i = 0; i < candidates.Count; i++)
            {
                MonsterSpawnCandidate candidate = candidates[i];
                if (!candidate.IsReachable) continue;

                float entryDistance = MonsterBrain.DistXZ(candidate.Position, playerEntry);
                if (entryDistance < minimumEntryDistance) continue;
                farReachable.Add(i);

                float objectiveDistance = float.PositiveInfinity;
                if (objectives != null)
                {
                    for (int j = 0; j < objectives.Count; j++)
                        objectiveDistance = Mathf.Min(objectiveDistance,
                            MonsterBrain.DistXZ(candidate.Position, objectives[j]));
                }

                if (objectiveDistance >= objectiveClearance)
                {
                    farClear.Add(i);
                    if (!candidate.IsVisibleFromEntry)
                        farClearHidden.Add(i);
                }
            }

            // Never trade the minimum player distance for a successful spawn.
            List<int> choices = farClearHidden.Count > 0 ? farClearHidden
                : farClear.Count > 0 ? farClear
                : farReachable;
            return choices.Count == 0 ? -1 : choices[rng.Next(choices.Count)];
        }
    }
}
