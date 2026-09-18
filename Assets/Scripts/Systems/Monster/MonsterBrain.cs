using System;
using System.Collections.Generic;
using UnityEngine;
using Lilo.Config;
using Lilo.State;

namespace Lilo.Systems.Monster
{
    /// <summary>One-shot noise pulse (interaction, battery swap — spec 002 FR-012).</summary>
    [Serializable]
    public struct NoisePulse
    {
        public Vector3 Position;
        public float Radius;
    }

    [Serializable]
    public struct MonsterBrainState
    {
        public MonsterState State;
        public float Timer;
        public Vector3 Target;
        public Vector3 LastKnown;
        public int WaypointIndex;
        public Vector3 SearchPoint;
        public float SearchRetargetTimer;
        public bool CatchFired;
    }

    public struct MonsterBrainInput
    {
        public float DeltaTime;
        public Vector3 MonsterPosition;
        public Vector3 PlayerPosition;
        public float MovementNoiseRadius;
        public List<NoisePulse> Pulses;
        public MonsterTuningProfile Profile;
        public float WalkSpeed;
        public float ChaseTriggerDistance;
        public float SearchRadius;
        public float CatchRadius;
        public Vector3[] Waypoints;
        public bool ArrivedAtTarget;
        public System.Random Rng;
    }

    public struct MonsterBrainOutput
    {
        public Vector3 MoveTarget;
        public float Speed;
    }

    /// <summary>
    /// Pure monster state machine (GDD Ch. 6.1, spec 002 FR-005–FR-010, FR-013–FR-014).
    /// No MonoBehaviour, no navigation — the controller owns movement and arrival.
    /// Distances are measured on the XZ plane; detection is strictly less-than.
    /// </summary>
    public static class MonsterBrain
    {
        public const float SearchRetargetInterval = 2f;

        public static MonsterBrainOutput Step(ref MonsterBrainState s, MonsterBrainInput i)
        {
            // CATCH in any state (FR-010). Fires once; controller consumes CatchFired.
            if (s.State != MonsterState.Catch && DistXZ(i.MonsterPosition, i.PlayerPosition) < i.CatchRadius)
            {
                s.State = MonsterState.Catch;
                s.CatchFired = true;
                s.Timer = 0f;
                return new MonsterBrainOutput { MoveTarget = i.MonsterPosition, Speed = 0f };
            }

            // Detection: monster learns only a world position (FR-014).
            // Simultaneous noises: the largest DETECTING radius wins.
            bool detected = false;
            Vector3 point = default;
            float bestRadius = 0f;
            if (i.MovementNoiseRadius > 0f
                && DistXZ(i.MonsterPosition, i.PlayerPosition) < i.MovementNoiseRadius
                && i.MovementNoiseRadius > bestRadius)
            {
                detected = true;
                point = i.PlayerPosition;
                bestRadius = i.MovementNoiseRadius;
            }
            if (i.Pulses != null)
            {
                foreach (var p in i.Pulses)
                {
                    if (p.Radius > 0f
                        && DistXZ(i.MonsterPosition, p.Position) < p.Radius
                        && p.Radius > bestRadius)
                    {
                        detected = true;
                        point = p.Position;
                        bestRadius = p.Radius;
                    }
                }
            }

            float patrolSpeed = i.Profile.patrolSpeed * i.WalkSpeed;
            float chaseSpeed = i.Profile.chaseSpeed * i.WalkSpeed;

            switch (s.State)
            {
                case MonsterState.Patrol:
                    if (detected)
                    {
                        s.State = MonsterState.Investigate;
                        s.Target = point;
                        s.Timer = 0f;
                    }
                    else if (HasWaypoints(i))
                    {
                        s.Target = i.Waypoints[s.WaypointIndex % i.Waypoints.Length];
                        if (i.ArrivedAtTarget)
                            s.WaypointIndex = (s.WaypointIndex + 1) % i.Waypoints.Length;
                    }
                    else
                    {
                        s.Target = i.MonsterPosition;
                    }
                    return new MonsterBrainOutput { MoveTarget = s.Target, Speed = patrolSpeed };

                case MonsterState.Investigate:
                    if (detected)
                    {
                        if (DistXZ(i.MonsterPosition, point) < i.ChaseTriggerDistance)
                        {
                            s.State = MonsterState.Chase;
                            s.LastKnown = point;
                            s.Target = point;
                            s.Timer = 0f;
                        }
                        else
                        {
                            s.Target = point;
                            s.Timer = 0f;
                        }
                    }
                    else if (i.ArrivedAtTarget)
                    {
                        s.Timer += i.DeltaTime;
                        if (s.Timer >= i.Profile.investigateDuration)
                            ReturnToPatrol(ref s, i);
                    }
                    return new MonsterBrainOutput { MoveTarget = s.Target, Speed = patrolSpeed };

                case MonsterState.Chase:
                    if (detected)
                    {
                        s.LastKnown = point;
                        s.Target = point;
                        s.Timer = 0f;
                    }
                    else
                    {
                        s.Timer += i.DeltaTime;
                        if (s.Timer >= i.Profile.chaseHoldDuration)
                        {
                            s.State = MonsterState.Search;
                            s.Timer = 0f;
                            s.SearchPoint = s.LastKnown;
                            s.SearchRetargetTimer = 0f;
                            s.Target = s.LastKnown;
                        }
                    }
                    return new MonsterBrainOutput { MoveTarget = s.Target, Speed = chaseSpeed };

                case MonsterState.Search:
                    if (detected)
                    {
                        s.State = MonsterState.Chase;
                        s.LastKnown = point;
                        s.Target = point;
                        s.Timer = 0f;
                    }
                    else
                    {
                        s.Timer += i.DeltaTime;
                        if (s.Timer >= i.Profile.searchDuration)
                        {
                            ReturnToPatrol(ref s, i);
                        }
                        else
                        {
                            s.SearchRetargetTimer -= i.DeltaTime;
                            if (s.SearchRetargetTimer <= 0f || i.ArrivedAtTarget)
                            {
                                s.SearchPoint = RandomPointAround(s.LastKnown, i.SearchRadius, i.Rng);
                                s.SearchRetargetTimer = SearchRetargetInterval;
                            }
                            s.Target = s.SearchPoint;
                        }
                    }
                    return new MonsterBrainOutput { MoveTarget = s.Target, Speed = patrolSpeed };

                default: // Catch — terminal, controller runs the sequence.
                    return new MonsterBrainOutput { MoveTarget = i.MonsterPosition, Speed = 0f };
            }
        }

        private static void ReturnToPatrol(ref MonsterBrainState s, MonsterBrainInput i)
        {
            s.State = MonsterState.Patrol;
            s.Timer = 0f;
            s.WaypointIndex = HasWaypoints(i) ? NearestWaypointIndex(i.MonsterPosition, i.Waypoints) : 0;
            s.Target = HasWaypoints(i) ? i.Waypoints[s.WaypointIndex] : i.MonsterPosition;
        }

        private static bool HasWaypoints(MonsterBrainInput i) => i.Waypoints != null && i.Waypoints.Length > 0;

        private static int NearestWaypointIndex(Vector3 pos, Vector3[] waypoints)
        {
            int best = 0;
            float bestD = DistXZ(pos, waypoints[0]);
            for (int k = 1; k < waypoints.Length; k++)
            {
                float d = DistXZ(pos, waypoints[k]);
                if (d < bestD)
                {
                    bestD = d;
                    best = k;
                }
            }
            return best;
        }

        private static Vector3 RandomPointAround(Vector3 center, float radius, System.Random rng)
        {
            if (rng == null || radius <= 0f)
                return center;
            double angle = rng.NextDouble() * Math.PI * 2.0;
            double r = Math.Sqrt(rng.NextDouble()) * radius;
            return new Vector3(
                center.x + (float)(Math.Cos(angle) * r),
                center.y,
                center.z + (float)(Math.Sin(angle) * r));
        }

        public static float DistXZ(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }
    }
}
