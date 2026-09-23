using System.Collections.Generic;
using UnityEngine;

namespace Lilo.Config
{
    /// <summary>
    /// The single tunables source (GDD Ch. 17). One production asset at Assets/Config/GameConfig.asset.
    /// Every runtime system reads numbers from here — never a second config source or a hardcoded literal.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Lilo/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("17.1 Player")]
        public float walkSpeed = 1.0f;
        public float sprintMultiplier = 1.6f;
        [Tooltip("Feel value — TBD on device, not a fabricated final number.")]
        public float sprintJoystickThreshold = 0.9f;
        [Tooltip("Feel value — TBD on device, not a fabricated final number.")]
        public float interactionRadius = 60.0f;
        [Tooltip("Provisional current-scene world scale value; re-confirm on device before locking.")]
        public float playerRadius = 0.5f;

        [Header("Hiding (hiding/001)")]
        [Tooltip("Transition duration for entering/exiting hiding spots (seconds). Feel value.")]
        public float hidingTransitionDuration = 0.5f;

        [Header("15.3 Joystick presentation (movement-and-camera/001)")]
        [Tooltip("Feel value — no prior tuning, pending on-device pass. 0 is a placeholder, not a locked default.")]
        public float joystickDeadZone = 0f;
        [Tooltip("Fraction of screen height (0–1). 0.35 = 35% of screen height. Scales across all devices.")]
        public float joystickDiameter = 0.18f;
        [Tooltip("Feel value — no prior tuning, pending on-device pass.")]
        public float joystickOpacity = 0f;
        [Tooltip("Feel value — no prior tuning, pending on-device pass.")]
        public Vector2 joystickCenterOffset = Vector2.zero;

        [Header("Ch. 13 Camera follow (movement-and-camera/003)")]
        [Tooltip("Carried over from prior on-device tuning — re-confirm.")]
        public float cameraFollowLerpFactor = 0.12f;
        [Tooltip("Carried over from prior on-device tuning — re-confirm.")]
        public float cameraBoundsInset = 80f;

        [Header("flashlight-and-battery/001+003+005+006 — feel content, carried over, re-confirm")]
        public float flashlightNormalRadius = 220f;
        [Tooltip("Flashlight intensity at full charge.")]
        public float flashlightNormalIntensity = 1.0f;
        [Tooltip("Flashlight intensity at Compact Darkness.")]
        public float flashlightCompactDarknessIntensity = 0.2f;
        public float lightRadiusEaseRate = 4.0f;
        public bool shadowsEnabled = true;
        [Tooltip("Single tunable for out-of-flashlight readability. 0 = pure black outside the beam.")]
        public float readabilityFillIntensity = 0.15f;

        [Header("17.2 Light & Battery")]
        public float batteryDuration = 180f;
        public int batterySlots = 1;
        public float lightStateFlickerStart = 0.30f;
        public float lightStateCriticalStart = 0.10f;
        public float compactDarknessRadius = 0.10f;
        public int batteryCountFloor52Min = 3;
        public int batteryCountFloor52Max = 5;
        public int batteryMaxActiveFloor51 = 2;
        public float batteryRespawnFloor51 = 30f;
        public int batteryMaxActiveFloor50 = 1;
        public float batteryRespawnFloor50 = 60f;
        [Tooltip("Keyboards loaded with a battery per floor entry (e.g. 3 of 7, randomized). Keyboard floors use this instead of the loose-spawn caps above.")]
        public int keyboardBatteryCountPerFloor = 3;

        [Header("17.3 Noise")]
        [Tooltip("Feel value — TBD on device; must be locked before Fase 2 per GDD Ch. 21.")]
        public float noiseBaseRadius = 1.0f;
        public float noiseWalk = 1.0f;
        public float noiseSprint = 3.0f;
        public float noiseInteract = 2.0f;
        public float noiseBatterySwap = 1.5f;
        public float noiseHiding = 0f;

        [Header("17.4 Monster (per floor)")]
        public MonsterTuningProfile monsterTuningFloor51 = new MonsterTuningProfile
        {
            monsterActive = true,
            patrolSpeed = 1.0f,
            chaseSpeed = 1.4f,
            investigateDuration = 4f,
            alertDuration = 2f,
            chaseHoldDuration = 3f,
            searchDuration = 6f,
        };
        public MonsterTuningProfile monsterTuningFloor50 = new MonsterTuningProfile
        {
            monsterActive = true,
            patrolSpeed = 1.2f,
            chaseSpeed = 1.5f,
            investigateDuration = 6f,
            alertDuration = 2f,
            chaseHoldDuration = 5f,
            searchDuration = 8f,
        };
        public bool monsterActiveFloor52 = false;

        public MonsterTuningProfile GetMonsterProfile(FloorId floor)
        {
            switch (floor)
            {
                case FloorId.Floor50:
                    return monsterTuningFloor50;
                case FloorId.Floor51:
                    return monsterTuningFloor51;
                default:
                    return new MonsterTuningProfile { monsterActive = monsterActiveFloor52 };
            }
        }

        [Header("002 Monster behavior (FR-018 new keys)")]
        [Tooltip("Close-range detection that turns INVESTIGATE into CHASE. Feel value — tune on device.")]
        public float chaseTriggerDistance = 2.5f;
        [Tooltip("Wander radius around last known position in SEARCH. Feel value — tune on device.")]
        public float searchRadius = 4f;
        [Tooltip("Touch distance that fires the catch outcome. Feel value — tune on device.")]
        public float catchRadius = 0.9f;
        [Tooltip("Stuck safeguard window (spec 002 edge cases).")]
        public float monsterStuckTimeout = 2f;
        [Tooltip("Spawn validity (FR-003); full automated checks land with spec 004 floor data.")]
        public float monsterSpawnMinDistance = 8f;
        public float monsterSpawnObjectiveClearance = 3f;
        public float monsterSpawnSafeSeconds = 5f;

        [Header("17.5 Progression")]
        public int lives = 3;
        public int floorCount = 3;
        public bool checkpointPerFloor = true;
        public int lockedDoorsFloor52 = 0;
        public int lockedDoorsFloor51 = 1;
        public int lockedDoorsFloor50 = 3;
        public float targetFloorDuration = 300f;

        /// <summary>Aggregates every field's validation rule. Empty result = valid config.</summary>
        public List<ConfigValidationFailure> Validate()
        {
            var failures = new List<ConfigValidationFailure>();

            // 17.1 Player
            if (walkSpeed <= 0)
                failures.Add(new ConfigValidationFailure(nameof(walkSpeed), walkSpeed.ToString(), "must be > 0"));
            if (sprintMultiplier <= 1.0f)
                failures.Add(new ConfigValidationFailure(nameof(sprintMultiplier), sprintMultiplier.ToString(), "must be > 1.0 (sprint faster than walk)"));
            if (sprintJoystickThreshold < 0f || sprintJoystickThreshold > 1f)
                failures.Add(new ConfigValidationFailure(nameof(sprintJoystickThreshold), sprintJoystickThreshold.ToString(), "must be in [0, 1]"));
            if (interactionRadius <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(interactionRadius), interactionRadius.ToString(), "must be > 0"));
            if (playerRadius <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(playerRadius), playerRadius.ToString(), "must be > 0"));
            if (hidingTransitionDuration < 0f)
                failures.Add(new ConfigValidationFailure(nameof(hidingTransitionDuration), hidingTransitionDuration.ToString(), "must be >= 0"));
            if (flashlightNormalRadius <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(flashlightNormalRadius), flashlightNormalRadius.ToString(), "must be > 0"));
            if (lightRadiusEaseRate < 0f)
                failures.Add(new ConfigValidationFailure(nameof(lightRadiusEaseRate), lightRadiusEaseRate.ToString(), "must be >= 0"));
            if (readabilityFillIntensity < 0f)
                failures.Add(new ConfigValidationFailure(nameof(readabilityFillIntensity), readabilityFillIntensity.ToString(), "must be >= 0"));
            if (joystickDeadZone >= sprintJoystickThreshold)
                failures.Add(new ConfigValidationFailure(nameof(joystickDeadZone), joystickDeadZone.ToString(), "must be < sprintJoystickThreshold (leaves no walking band otherwise, spec FR-013)"));
            if (joystickDiameter < 0.05f || joystickDiameter > 0.8f)
                failures.Add(new ConfigValidationFailure(nameof(joystickDiameter), joystickDiameter.ToString(), "must be in [0.05, 0.8] (fraction of screen height)"));

            // 17.2 Light & Battery
            if (batteryDuration <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(batteryDuration), batteryDuration.ToString(), "must be > 0"));
            if (batterySlots < 0)
                failures.Add(new ConfigValidationFailure(nameof(batterySlots), batterySlots.ToString(), "must be >= 0"));
            if (lightStateCriticalStart <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(lightStateCriticalStart), lightStateCriticalStart.ToString(), "must be > 0"));
            if (lightStateFlickerStart >= 1f)
                failures.Add(new ConfigValidationFailure(nameof(lightStateFlickerStart), lightStateFlickerStart.ToString(), "must be < 1"));
            if (lightStateCriticalStart >= lightStateFlickerStart)
                failures.Add(new ConfigValidationFailure(nameof(lightStateCriticalStart), lightStateCriticalStart.ToString(), "must be < lightStateFlickerStart (dead-zone ordering)"));
            if (compactDarknessRadius <= 0f || compactDarknessRadius > 0.5f)
                failures.Add(new ConfigValidationFailure(nameof(compactDarknessRadius), compactDarknessRadius.ToString(), "must be in (0, 0.5]"));
            if (batteryCountFloor52Min <= 0 || batteryCountFloor52Max <= 0 || batteryCountFloor52Min > batteryCountFloor52Max)
                failures.Add(new ConfigValidationFailure(nameof(batteryCountFloor52Min), $"{batteryCountFloor52Min}..{batteryCountFloor52Max}", "min/max must be > 0 and min <= max"));
            if (batteryMaxActiveFloor51 <= 0)
                failures.Add(new ConfigValidationFailure(nameof(batteryMaxActiveFloor51), batteryMaxActiveFloor51.ToString(), "must be > 0"));
            if (batteryRespawnFloor51 <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(batteryRespawnFloor51), batteryRespawnFloor51.ToString(), "must be > 0"));
            if (batteryMaxActiveFloor50 <= 0)
                failures.Add(new ConfigValidationFailure(nameof(batteryMaxActiveFloor50), batteryMaxActiveFloor50.ToString(), "must be > 0"));
            if (batteryRespawnFloor50 <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(batteryRespawnFloor50), batteryRespawnFloor50.ToString(), "must be > 0"));
            if (keyboardBatteryCountPerFloor < 0)
                failures.Add(new ConfigValidationFailure(nameof(keyboardBatteryCountPerFloor), keyboardBatteryCountPerFloor.ToString(), "must be >= 0"));

            // 17.3 Noise
            if (noiseBaseRadius <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(noiseBaseRadius), noiseBaseRadius.ToString(), "must be > 0"));
            if (noiseWalk <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(noiseWalk), noiseWalk.ToString(), "must be > 0"));
            if (noiseSprint <= noiseWalk)
                failures.Add(new ConfigValidationFailure(nameof(noiseSprint), noiseSprint.ToString(), "must be > noiseWalk"));
            if (noiseInteract <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(noiseInteract), noiseInteract.ToString(), "must be > 0"));
            if (noiseBatterySwap <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(noiseBatterySwap), noiseBatterySwap.ToString(), "must be > 0"));
            if (noiseHiding < 0f)
                failures.Add(new ConfigValidationFailure(nameof(noiseHiding), noiseHiding.ToString(), "must be >= 0"));

            // 17.4 Monster
            ValidateMonsterProfile(monsterTuningFloor51, nameof(monsterTuningFloor51), failures);
            ValidateMonsterProfile(monsterTuningFloor50, nameof(monsterTuningFloor50), failures);

            // 002 Monster behavior (FR-018)
            if (chaseTriggerDistance <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(chaseTriggerDistance), chaseTriggerDistance.ToString(), "must be > 0"));
            if (searchRadius <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(searchRadius), searchRadius.ToString(), "must be > 0"));
            if (catchRadius <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(catchRadius), catchRadius.ToString(), "must be > 0"));
            if (monsterStuckTimeout <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(monsterStuckTimeout), monsterStuckTimeout.ToString(), "must be > 0"));
            if (monsterSpawnMinDistance <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(monsterSpawnMinDistance), monsterSpawnMinDistance.ToString(), "must be > 0"));
            if (monsterSpawnObjectiveClearance <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(monsterSpawnObjectiveClearance), monsterSpawnObjectiveClearance.ToString(), "must be > 0"));
            if (monsterSpawnSafeSeconds <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(monsterSpawnSafeSeconds), monsterSpawnSafeSeconds.ToString(), "must be > 0"));

            // 17.5 Progression
            if (lives <= 0)
                failures.Add(new ConfigValidationFailure(nameof(lives), lives.ToString(), "must be > 0"));
            if (floorCount <= 0)
                failures.Add(new ConfigValidationFailure(nameof(floorCount), floorCount.ToString(), "must be > 0"));
            if (lockedDoorsFloor52 < 0)
                failures.Add(new ConfigValidationFailure(nameof(lockedDoorsFloor52), lockedDoorsFloor52.ToString(), "must be >= 0"));
            if (lockedDoorsFloor51 < 0)
                failures.Add(new ConfigValidationFailure(nameof(lockedDoorsFloor51), lockedDoorsFloor51.ToString(), "must be >= 0"));
            if (lockedDoorsFloor50 < 0)
                failures.Add(new ConfigValidationFailure(nameof(lockedDoorsFloor50), lockedDoorsFloor50.ToString(), "must be >= 0"));
            if (targetFloorDuration <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(targetFloorDuration), targetFloorDuration.ToString(), "must be > 0"));

            return failures;
        }

        private void ValidateMonsterProfile(MonsterTuningProfile profile, string label, List<ConfigValidationFailure> failures)
        {
            if (!profile.monsterActive)
                return;

            if (profile.patrolSpeed <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.patrolSpeed", profile.patrolSpeed.ToString(), "must be > 0"));
            if (profile.chaseSpeed <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.chaseSpeed", profile.chaseSpeed.ToString(), "must be > 0"));
            if (profile.chaseSpeed >= sprintMultiplier)
                failures.Add(new ConfigValidationFailure($"{label}.chaseSpeed", profile.chaseSpeed.ToString(), "must be < sprintMultiplier (hard rule, GDD 6.2)"));
            if (profile.investigateDuration <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.investigateDuration", profile.investigateDuration.ToString(), "must be > 0"));
            if (profile.chaseHoldDuration <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.chaseHoldDuration", profile.chaseHoldDuration.ToString(), "must be > 0"));
            if (profile.searchDuration <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.searchDuration", profile.searchDuration.ToString(), "must be > 0"));
        }
    }
}
