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
        public float walkSpeed = 1f;
        public float sprintMultiplier = 3f;
        [Tooltip("Feel value — TBD on device, not a fabricated final number.")]
        public float sprintJoystickThreshold = 0.9f;
        [Tooltip("Feel value — TBD on device, not a fabricated final number.")]
        public float interactionRadius = 60.0f;
        [Tooltip("Provisional current-scene world scale value; re-confirm on device before locking.")]
        public float playerRadius = 0.25f;

        [Header("Hiding (hiding/001)")]
        [Tooltip("Transition duration for entering/exiting hiding spots (seconds). Feel value.")]
        public float hidingTransitionDuration = 0.5f;

        [Header("15.3 Joystick presentation (movement-and-camera/001)")]
        [Tooltip("Feel value — no prior tuning, pending on-device pass. 0 is a placeholder, not a locked default.")]
        public float joystickDeadZone = 0f;
        [Tooltip("Fraction of rendered screen height (0–1); the joystick converts pixels to canvas units and uses a smaller fraction on tablets.")]
        public float joystickDiameter = 0.18f;
        [Tooltip("Feel value — no prior tuning, pending on-device pass.")]
        public float joystickOpacity = 0f;
        [Tooltip("Feel value — no prior tuning, pending on-device pass.")]
        public Vector2 joystickCenterOffset = Vector2.zero;

        [Header("Ch. 13 Camera follow (movement-and-camera/003)")]
        [Tooltip("Carried over from prior on-device tuning — re-confirm.")]
        public float cameraFollowLerpFactor = 0.12f;
        [Tooltip("Carried over from prior on-device tuning — re-confirm.")]
        public float cameraBoundsInset = 0f;

        [Header("flashlight-and-battery/001+003+005+006 — feel content, carried over, re-confirm")]
        public float flashlightNormalRadius = 220f;
        [Tooltip("Flashlight intensity at full charge.")]
        public float flashlightNormalIntensity = 1.0f;
        [Tooltip("Minimum flashlight intensity at 0% battery, so the compact screen remains visible.")]
        public float flashlightCompactDarknessIntensity = 0.5f;
        public float lightRadiusEaseRate = 4.0f;
        public bool shadowsEnabled = true;
        [Tooltip("Single tunable for out-of-flashlight readability. 0 = pure black outside the beam.")]
        public float readabilityFillIntensity = 0.15f;

        [Header("17.2 Light & Battery")]
        public float batteryDuration = 180f;
        public int batterySlots = 1;
        public float lightStateFlickerStart = 0.30f;
        public float lightStateCriticalStart = 0.10f;
        [Tooltip("Random seconds between lamp flickers at full charge.")]
        public Vector2 flickerIntervalFullBattery = new Vector2(18f, 28f);
        [Tooltip("Random seconds between lamp flickers just before the battery is empty.")]
        public Vector2 flickerIntervalLowBattery = new Vector2(1f, 2.5f);
        [Tooltip("Fraction of lamp brightness lost during a flicker at full charge.")]
        [Range(0f, 1f)] public float flickerDimFullBattery = 0.25f;
        [Tooltip("Fraction of lamp brightness lost during a flicker near empty.")]
        [Range(0f, 1f)] public float flickerDimLowBattery = 0.95f;
        public float compactDarknessRadius = 0.10f;
        public int batteryCountFloor52Min = 3;
        public int batteryCountFloor52Max = 5;
        public int batteryMaxActiveFloor51 = 2;
        public float batteryRespawnFloor51 = 30f;
        public int batteryMaxActiveFloor50 = 1;
        public float batteryRespawnFloor50 = 60f;
        [Tooltip("Keyboards loaded with a battery per floor entry (e.g. 3 of 7, randomized). Keyboard floors use this instead of the loose-spawn caps above.")]
        public int keyboardBatteryCountPerFloor = 3;
        [Tooltip("Lamp charge gained per battery pickup, as a fraction of full (0.25 = +25%). Clamped at full.")]
        public float keyboardBatteryChargeFraction = 0.25f;

        [Header("Access key placement")]
        [Tooltip("Minimum horizontal distance from the player when an access key is placed.")]
        public float accessKeySpawnMinDistance = 5f;
        [Tooltip("Preferred maximum horizontal distance for access key placement.")]
        public float accessKeySpawnMaxDistance = 8f;

        [Header("17.3 Noise")]
        [Tooltip("Maximum walk noise radius is the base radius times noiseWalk.")]
        public float noiseBaseRadius = 3.0f;
        public float noiseWalk = 1.0f;
        public float noiseSprint = 3.0f;
        public float noiseInteract = 2.0f;
        public float noiseBatterySwap = 1.5f;
        public float noiseHiding = 0f;
        [Tooltip("Legacy setting. Current monster hearing uses noise radius in every direction.")]
        [Range(0f, 80f)] public float monsterHearingRearBlindSpotAngle = 32f;

        [Header("17.4 Monster (per floor)")]
        [Tooltip("Multiplier for all monster movement and its walk animation. 2 = twice the base speed.")]
        public float monsterSpeedMultiplier = 2.0f;
        public MonsterTuningProfile monsterTuningFloor51 = new MonsterTuningProfile
        {
            monsterActive = true,
            patrolSpeed = 0.25f,
            chaseSpeed = 0.745f,
            movementSpeedMultiplier = 0.5f,
            noiseSensitivityMultiplier = 1f,
            visionRangeMultiplier = 1f,
            investigateDuration = 4f,
            alertDuration = 1f,
            chaseHoldDuration = 3f,
            searchDuration = 6f,
        };
        public MonsterTuningProfile monsterTuningFloor50 = new MonsterTuningProfile
        {
            monsterActive = true,
            patrolSpeed = 0.5f,
            chaseSpeed = 1.49f,
            movementSpeedMultiplier = 0.75f,
            noiseSensitivityMultiplier = 1.125f,
            visionRangeMultiplier = 0.75f,
            investigateDuration = 6f,
            alertDuration = 1f,
            chaseHoldDuration = 5f,
            searchDuration = 8f,
        };
        public bool monsterActiveFloor52 = false;

        public MonsterTuningProfile GetMonsterProfile(FloorId floor)
        {
            MonsterTuningProfile profile;
            switch (floor)
            {
                case FloorId.Floor50:
                    profile = monsterTuningFloor50;
                    break;
                case FloorId.Floor51:
                    profile = monsterTuningFloor51;
                    break;
                default:
                    profile = new MonsterTuningProfile { monsterActive = monsterActiveFloor52 };
                    break;
            }

            // Older serialized tuning assets predate these per-floor multipliers.
            if (profile.movementSpeedMultiplier <= 0f) profile.movementSpeedMultiplier = 1f;
            if (profile.noiseSensitivityMultiplier <= 0f) profile.noiseSensitivityMultiplier = 1f;
            if (profile.visionRangeMultiplier <= 0f) profile.visionRangeMultiplier = 1f;
            return profile;
        }

        public int GetLockedDoorCount(FloorId floor)
        {
            switch (floor)
            {
                case FloorId.Floor52: return lockedDoorsFloor52;
                case FloorId.Floor51: return lockedDoorsFloor51;
                case FloorId.Floor50: return lockedDoorsFloor50;
                default: return 0;
            }
        }

        [Header("002 Monster behavior (FR-018 new keys)")]
        [Tooltip("Maximum distance for direct sight detection.")]
        [Min(1f)] public float monsterVisionRange = 9f;
        [Tooltip("Monster's normal field of view, in degrees.")]
        [Range(10f, 180f)] public float monsterVisionAngle = 100f;
        [Tooltip("Close-range detection that turns INVESTIGATE into CHASE. Feel value — tune on device.")]
        public float chaseTriggerDistance = 2.5f;
        [Tooltip("Wander radius around last known position in SEARCH. Feel value — tune on device.")]
        public float searchRadius = 4f;
        [Tooltip("Touch distance that fires the catch outcome. Feel value — tune on device.")]
        public float catchRadius = 0.9f;
        [Tooltip("Stuck safeguard window (spec 002 edge cases).")]
        public float monsterStuckTimeout = 2f;
        [Tooltip("Minimum player distance when placing the monster at floor entry.")]
        public float monsterSpawnMinDistance = 8f;
        [Tooltip("Maximum preferred player distance for the monster's initial spawn.")]
        public float monsterSpawnMaxDistance = 15f;
        public float monsterSpawnObjectiveClearance = 3f;
        public float monsterSpawnSafeSeconds = 5f;

        [Header("17.5 Progression")]
        public int lives = 3;
        public int floorCount = 3;
        public bool checkpointPerFloor = true;
        public int lockedDoorsFloor52 = 1;
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
            if (flickerIntervalFullBattery.x <= 0f || flickerIntervalFullBattery.y < flickerIntervalFullBattery.x)
                failures.Add(new ConfigValidationFailure(nameof(flickerIntervalFullBattery), flickerIntervalFullBattery.ToString(), "interval must have 0 < min <= max"));
            if (flickerIntervalLowBattery.x <= 0f || flickerIntervalLowBattery.y < flickerIntervalLowBattery.x)
                failures.Add(new ConfigValidationFailure(nameof(flickerIntervalLowBattery), flickerIntervalLowBattery.ToString(), "interval must have 0 < min <= max"));
            if (flickerIntervalLowBattery.x > flickerIntervalFullBattery.x || flickerIntervalLowBattery.y > flickerIntervalFullBattery.y)
                failures.Add(new ConfigValidationFailure(nameof(flickerIntervalLowBattery), flickerIntervalLowBattery.ToString(), "low-battery intervals must be no longer than full-battery intervals"));
            if (flickerDimFullBattery < 0f || flickerDimLowBattery > 1f || flickerDimFullBattery > flickerDimLowBattery)
                failures.Add(new ConfigValidationFailure(nameof(flickerDimFullBattery), $"{flickerDimFullBattery}..{flickerDimLowBattery}", "dim fractions must satisfy 0 <= full <= low <= 1"));
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
            if (keyboardBatteryChargeFraction <= 0f || keyboardBatteryChargeFraction > 1f)
                failures.Add(new ConfigValidationFailure(nameof(keyboardBatteryChargeFraction), keyboardBatteryChargeFraction.ToString(), "must be in (0, 1]"));
            if (accessKeySpawnMinDistance <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(accessKeySpawnMinDistance), accessKeySpawnMinDistance.ToString(), "must be > 0"));
            if (accessKeySpawnMaxDistance < accessKeySpawnMinDistance)
                failures.Add(new ConfigValidationFailure(nameof(accessKeySpawnMaxDistance), accessKeySpawnMaxDistance.ToString(), "must be >= accessKeySpawnMinDistance"));

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
            if (monsterHearingRearBlindSpotAngle < 0f || monsterHearingRearBlindSpotAngle > 80f)
                failures.Add(new ConfigValidationFailure(nameof(monsterHearingRearBlindSpotAngle), monsterHearingRearBlindSpotAngle.ToString(), "must be between 0 and 80 degrees"));

            // 17.4 Monster
            if (monsterSpeedMultiplier <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(monsterSpeedMultiplier), monsterSpeedMultiplier.ToString(), "must be > 0"));
            ValidateMonsterProfile(monsterTuningFloor51, nameof(monsterTuningFloor51), failures);
            ValidateMonsterProfile(monsterTuningFloor50, nameof(monsterTuningFloor50), failures);

            // 002 Monster behavior (FR-018)
            if (monsterVisionRange <= 0f)
                failures.Add(new ConfigValidationFailure(nameof(monsterVisionRange), monsterVisionRange.ToString(), "must be > 0"));
            if (monsterVisionAngle < 10f || monsterVisionAngle > 180f)
                failures.Add(new ConfigValidationFailure(nameof(monsterVisionAngle), monsterVisionAngle.ToString(), "must be between 10 and 180 degrees"));
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
            if (monsterSpawnMaxDistance < monsterSpawnMinDistance)
                failures.Add(new ConfigValidationFailure(nameof(monsterSpawnMaxDistance), monsterSpawnMaxDistance.ToString(), "must be >= monsterSpawnMinDistance"));
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
            if (profile.movementSpeedMultiplier <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.movementSpeedMultiplier", profile.movementSpeedMultiplier.ToString(), "must be > 0"));
            if (profile.noiseSensitivityMultiplier < 1f)
                failures.Add(new ConfigValidationFailure($"{label}.noiseSensitivityMultiplier", profile.noiseSensitivityMultiplier.ToString(), "must be >= 1"));
            if (profile.chaseSpeed * monsterSpeedMultiplier >= sprintMultiplier)
                failures.Add(new ConfigValidationFailure($"{label}.chaseSpeed", profile.chaseSpeed.ToString(), "chase speed after multiplier must be < sprintMultiplier (hard rule, GDD 6.2)"));
            if (profile.investigateDuration <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.investigateDuration", profile.investigateDuration.ToString(), "must be > 0"));
            if (profile.chaseHoldDuration <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.chaseHoldDuration", profile.chaseHoldDuration.ToString(), "must be > 0"));
            if (profile.searchDuration <= 0f)
                failures.Add(new ConfigValidationFailure($"{label}.searchDuration", profile.searchDuration.ToString(), "must be > 0"));
        }
    }
}
