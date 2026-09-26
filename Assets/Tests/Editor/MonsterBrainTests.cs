using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Lilo.Config;
using Lilo.State;
using Lilo.Systems.Monster;

namespace Lilo.Tests
{
    /// <summary>EditMode tests for spec 002 SC-007: every brain transition, the
    /// strict-less-than boundary, the largest-radius rule, and config validation.</summary>
    public class MonsterBrainTests
    {
        private static readonly Vector3[] Route =
        {
            new Vector3(-7f, 0f, -7f),
            new Vector3(7f, 0f, -7f),
            new Vector3(7f, 0f, 7f),
            new Vector3(-7f, 0f, 7f),
        };

        private static MonsterTuningProfile Profile => new MonsterTuningProfile
        {
            monsterActive = true,
            patrolSpeed = 1.0f,
            chaseSpeed = 1.4f,
            investigateDuration = 4f,
            alertDuration = 2f,
            chaseHoldDuration = 3f,
            searchDuration = 6f,
        };

        private static MonsterBrainInput BaseInput()
        {
            return new MonsterBrainInput
            {
                DeltaTime = 0.1f,
                MonsterPosition = Vector3.zero,
                PlayerPosition = new Vector3(50f, 0f, 50f),
                MovementNoiseRadius = 0f,
                Profile = Profile,
                WalkSpeed = 1f,
                ChaseTriggerDistance = 2.5f,
                SearchRadius = 4f,
                CatchRadius = 0.9f,
                Waypoints = Route,
                ArrivedAtTarget = false,
                Rng = new System.Random(42),
            };
        }

        [Test]
        public void Patrol_AdvancesWaypoint_OnArrival()
        {
            var s = new MonsterBrainState { State = MonsterState.Patrol, WaypointIndex = 0, Target = Route[0] };
            var i = BaseInput();
            i.MonsterPosition = Route[0];
            i.ArrivedAtTarget = true;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Patrol, s.State);
            Assert.AreEqual(1, s.WaypointIndex);
            Assert.AreEqual(Route[1], s.Target);
        }

        [Test]
        public void Patrol_SprintNoise_Investigates()
        {
            var s = new MonsterBrainState { State = MonsterState.Patrol };
            var i = BaseInput();
            i.PlayerPosition = new Vector3(2f, 0f, 0f); // within sprint radius 3
            i.MovementNoiseRadius = 3f;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);
            Assert.AreEqual(i.PlayerPosition, s.Target);
        }

        [Test]
        public void Detection_Boundary_IsStrictlyLessThan()
        {
            var i = BaseInput();
            i.PlayerPosition = new Vector3(3f, 0f, 0f);
            i.MovementNoiseRadius = 3f; // distance == radius: no detection.
            var s = new MonsterBrainState { State = MonsterState.Patrol };
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Patrol, s.State);

            i.MovementNoiseRadius = 3.01f; // strictly inside: detection.
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);
        }

        [Test]
        public void NoiseLadder_WalkSilent_SprintHeard()
        {
            var i = BaseInput();
            i.PlayerPosition = new Vector3(2f, 0f, 0f);
            var s = new MonsterBrainState { State = MonsterState.Patrol };

            i.MovementNoiseRadius = 1f; // walk radius 1.0, player at 2: silent.
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Patrol, s.State);

            i.MovementNoiseRadius = 3f; // sprint radius 3.0: heard.
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);
        }

        [Test]
        public void Investigate_CloseNoise_DoesNotBecomeChaseWithoutVision()
        {
            var i = BaseInput();
            var s = new MonsterBrainState { State = MonsterState.Investigate, Target = new Vector3(5f, 0f, 0f) };

            i.PlayerPosition = new Vector3(10f, 0f, 0f); // far: retarget, stay.
            i.MovementNoiseRadius = 30f;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);
            Assert.AreEqual(i.PlayerPosition, s.Target);

            i.PlayerPosition = new Vector3(1f, 0f, 0f); // within chaseTriggerDistance: chase.
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);

            i.PlayerVisible = true;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Alert, s.State);
        }

        [Test]
        public void VisiblePlayer_AlertsThenChasesAfterTwoSeconds()
        {
            var s = new MonsterBrainState { State = MonsterState.Patrol };
            var i = BaseInput();
            i.PlayerVisible = true;

            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Alert, s.State);

            for (int k = 0; k < 19; k++)
                MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Alert, s.State);

            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Chase, s.State);
        }

        [Test]
        public void Alert_InvestigatesLastKnownWhenVisionIsLost()
        {
            var s = new MonsterBrainState { State = MonsterState.Alert, Timer = 0.5f, LastKnown = new Vector3(4f, 0f, 2f) };
            var i = BaseInput();
            i.PlayerVisible = false;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);
            Assert.AreEqual(s.LastKnown, s.Target);
        }

        [Test]
        public void Chase_HoldsLastKnownWhenVisionIsLost()
        {
            var s = new MonsterBrainState { State = MonsterState.Chase };
            var i = BaseInput();
            i.PlayerVisible = false;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Chase, s.State);
            Assert.AreEqual(s.LastKnown, s.Target);
        }

        [Test]
        public void Investigate_Timeout_ReturnsToPatrol()
        {
            var s = new MonsterBrainState { State = MonsterState.Investigate, Target = new Vector3(5f, 0f, 0f) };
            var i = BaseInput();
            i.MonsterPosition = new Vector3(5f, 0f, 0f);
            i.ArrivedAtTarget = true;
            i.PlayerPosition = new Vector3(50f, 0f, 50f);
            for (int k = 0; k < 50; k++) // 5s > investigateDuration 4s.
                MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Patrol, s.State);
        }

        [Test]
        public void Chase_SearchesAfterMemoryExpires()
        {
            var s = new MonsterBrainState { State = MonsterState.Chase, LastKnown = new Vector3(5f, 0f, 0f), Target = new Vector3(5f, 0f, 0f) };
            var i = BaseInput();
            i.MonsterPosition = new Vector3(5f, 0f, 0f);
            i.PlayerPosition = new Vector3(50f, 0f, 50f);
            i.DeltaTime = Profile.chaseHoldDuration;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Search, s.State);
        }

        [Test]
        public void Chase_NewNoise_UpdatesLastKnown()
        {
            var s = new MonsterBrainState { State = MonsterState.Chase, LastKnown = new Vector3(5f, 0f, 0f) };
            var i = BaseInput();
            i.PlayerPosition = new Vector3(6f, 0f, 0f);
            i.MovementNoiseRadius = 30f;
            i.PlayerVisible = true;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Chase, s.State);
            Assert.AreEqual(i.PlayerPosition, s.LastKnown);
            Assert.AreEqual(0f, s.Timer);
        }

        [Test]
        public void Search_DetectedNoise_ReturnsToChase()
        {
            var s = new MonsterBrainState
            {
                State = MonsterState.Search,
                LastKnown = new Vector3(8f, 0f, 0f),
            };
            var i = BaseInput();
            i.PlayerPosition = new Vector3(2f, 0f, 0f);
            i.MovementNoiseRadius = 3f;
            i.PlayerVisible = true;

            MonsterBrain.Step(ref s, i);

            Assert.AreEqual(MonsterState.Alert, s.State);
            Assert.AreEqual(i.PlayerPosition, s.LastKnown);
        }

        [Test]
        public void Search_HearsNoiseBehindWall_InvestigatesSource()
        {
            var s = new MonsterBrainState { State = MonsterState.Search };
            var i = BaseInput();
            i.PlayerPosition = new Vector3(2f, 0f, 0f);
            i.MovementNoiseRadius = 3f;
            i.PlayerVisible = false;
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);
            Assert.AreEqual(i.PlayerPosition, s.Target);
        }

        [Test]
        public void LargestDetectingRadius_Wins()
        {
            var s = new MonsterBrainState { State = MonsterState.Patrol };
            var i = BaseInput();
            i.PlayerPosition = new Vector3(1f, 0f, 0f);
            i.MovementNoiseRadius = 2f;
            i.Pulses = new List<NoisePulse>
            {
                new NoisePulse { Position = new Vector3(0f, 0f, 5f), Radius = 10f },
            };
            MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Investigate, s.State);
            Assert.AreEqual(new Vector3(0f, 0f, 5f), s.Target);
        }

        [Test]
        public void Catch_FiresOnce_WhenTouching()
        {
            var s = new MonsterBrainState { State = MonsterState.Chase };
            var i = BaseInput();
            i.MonsterPosition = Vector3.zero;
            i.PlayerPosition = new Vector3(0.5f, 0f, 0f);
            i.PlayerVisible = true;
            var output = MonsterBrain.Step(ref s, i);
            Assert.AreEqual(MonsterState.Catch, s.State);
            Assert.IsTrue(s.CatchFired);
            Assert.AreEqual(0f, output.Speed);
            Assert.IsTrue(output.CaughtThisStep);

            output = MonsterBrain.Step(ref s, i); // stays caught, does not fire again.
            Assert.AreEqual(MonsterState.Catch, s.State);
            Assert.IsTrue(s.CatchFired);
            Assert.IsFalse(output.CaughtThisStep);
        }

        [Test]
        public void Investigate_CloseNoise_TransitionsBeforeCatch()
        {
            var s = new MonsterBrainState { State = MonsterState.Investigate };
            var i = BaseInput();
            i.PlayerPosition = new Vector3(0.5f, 0f, 0f);
            i.MovementNoiseRadius = 3f;
            i.PlayerVisible = true;

            MonsterBrainOutput output = MonsterBrain.Step(ref s, i);

            Assert.AreEqual(MonsterState.Alert, s.State);
            Assert.AreEqual(MonsterState.Alert, output.StateBeforeCatch);
            Assert.IsFalse(output.CaughtThisStep);
        }

        [Test]
        public void Noise_IdleAndHiding_AreSilent()
        {
            var cfg = ScriptableObject.CreateInstance<GameConfig>();
            Assert.AreEqual(0f, MonsterNoise.MovementRadius(cfg, isHiding: false, isMoving: false, isSprinting: false));
            Assert.AreEqual(0f, MonsterNoise.MovementRadius(cfg, isHiding: true, isMoving: true, isSprinting: true));
            Assert.AreEqual(cfg.noiseBaseRadius * cfg.noiseWalk,
                MonsterNoise.MovementRadius(cfg, isHiding: false, isMoving: true, isSprinting: false));
            Assert.AreEqual(cfg.noiseBaseRadius * cfg.noiseSprint,
                MonsterNoise.MovementRadius(cfg, isHiding: false, isMoving: true, isSprinting: true));
        }

        [Test]
        public void Config_RejectsChaseSpeed_AtOrAboveSprint()
        {
            var cfg = ScriptableObject.CreateInstance<GameConfig>();
            cfg.monsterTuningFloor51.chaseSpeed = cfg.sprintMultiplier; // == sprint: invalid.
            var failures = cfg.Validate();
            Assert.IsTrue(failures.Exists(f => f.ToString().Contains("monsterTuningFloor51.chaseSpeed")));
        }

        [Test]
        public void Config_RejectsNonPositiveMonsterSpeeds()
        {
            var cfg = ScriptableObject.CreateInstance<GameConfig>();
            cfg.monsterTuningFloor51.patrolSpeed = 0f;
            cfg.monsterTuningFloor50.chaseSpeed = 0f;

            var failures = cfg.Validate();

            Assert.IsTrue(failures.Exists(f => f.ToString().Contains("monsterTuningFloor51.patrolSpeed")));
            Assert.IsTrue(failures.Exists(f => f.ToString().Contains("monsterTuningFloor50.chaseSpeed")));
        }

        [Test]
        public void Floor52Profile_IsInactive_WhileLaterFloorsUseConfiguredProfiles()
        {
            var cfg = ScriptableObject.CreateInstance<GameConfig>();

            Assert.IsFalse(cfg.GetMonsterProfile(FloorId.Floor52).monsterActive);
            Assert.AreEqual(cfg.monsterTuningFloor51.chaseSpeed, cfg.GetMonsterProfile(FloorId.Floor51).chaseSpeed);
            Assert.AreEqual(cfg.monsterTuningFloor50.searchDuration, cfg.GetMonsterProfile(FloorId.Floor50).searchDuration);
        }

        [Test]
        public void Floor51KeepsItsSpeed_AndFloor50GetsReducedSpeedAndDetection()
        {
            var cfg = ScriptableObject.CreateInstance<GameConfig>();

            Assert.AreEqual(0.5f, cfg.GetMonsterProfile(FloorId.Floor51).movementSpeedMultiplier);
            Assert.AreEqual(0.75f, cfg.GetMonsterProfile(FloorId.Floor50).movementSpeedMultiplier);
            Assert.AreEqual(1.125f, cfg.GetMonsterProfile(FloorId.Floor50).noiseSensitivityMultiplier);
            Assert.AreEqual(0.75f, cfg.GetMonsterProfile(FloorId.Floor50).visionRangeMultiplier);
        }

        [Test]
        public void HiddenPlayer_ImmuneToCatchEvenWhenWithinCatchRadius()
        {
            var state = new MonsterBrainState { State = MonsterState.Patrol };
            var input = BaseInput();
            input.PlayerPosition = input.MonsterPosition; // Overlapping
            input.IsPlayerHidden = true;

            var output = MonsterBrain.Step(ref state, input);

            Assert.IsFalse(output.CaughtThisStep);
            Assert.AreNotEqual(MonsterState.Catch, state.State);
        }

        [Test]
        public void HiddenPlayer_ImmuneToMovementNoiseDetection()
        {
            var state = new MonsterBrainState { State = MonsterState.Patrol };
            var input = BaseInput();
            input.PlayerPosition = input.MonsterPosition + new Vector3(2f, 0f, 0f);
            input.MovementNoiseRadius = 5f;
            input.IsPlayerHidden = true;

            MonsterBrain.Step(ref state, input);

            Assert.AreEqual(MonsterState.Patrol, state.State);
        }

        [Test]
        public void HidingDuringChase_AlertsBrieflyThenPatrolsWithoutFollowingHideSpot()
        {
            var state = new MonsterBrainState
            {
                State = MonsterState.Chase,
                LastKnown = new Vector3(2f, 0f, 0f),
            };
            var input = BaseInput();
            input.IsPlayerHidden = true;
            input.PlayerPosition = new Vector3(2f, 0f, 0f);
            input.MovementNoiseRadius = 100f;
            input.Pulses = new List<NoisePulse>
            {
                new NoisePulse { Position = input.PlayerPosition, Radius = 100f },
            };
            input.DeltaTime = 0.5f;

            var output = MonsterBrain.Step(ref state, input);
            Assert.AreEqual(MonsterState.Alert, state.State);
            Assert.AreEqual(0f, output.Speed);
            Assert.IsFalse(output.CaughtThisStep);

            for (int k = 0; k < 4; k++)
                output = MonsterBrain.Step(ref state, input);

            Assert.AreEqual(MonsterState.Patrol, state.State);
            Assert.AreNotEqual(input.PlayerPosition, output.MoveTarget);
            Assert.IsFalse(output.CaughtThisStep);
        }

        [Test]
        public void HiddenPatrol_IgnoresNoiseUntilHidingIsRevealed()
        {
            var state = new MonsterBrainState { State = MonsterState.Patrol };
            var input = BaseInput();
            input.IsPlayerHidden = true;
            input.PlayerPosition = new Vector3(3f, 0f, 0f);
            input.Pulses = new List<NoisePulse>
            {
                new NoisePulse { Position = input.PlayerPosition, Radius = 20f },
            };

            MonsterBrain.Step(ref state, input);
            Assert.AreEqual(MonsterState.Patrol, state.State);

            input.HidingRevealed = true;
            MonsterBrain.Step(ref state, input);
            Assert.AreEqual(MonsterState.Chase, state.State);
            Assert.AreEqual(input.PlayerPosition, state.Target);
        }
    }
}
