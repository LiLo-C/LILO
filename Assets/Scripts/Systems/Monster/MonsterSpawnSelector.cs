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
    }
}
