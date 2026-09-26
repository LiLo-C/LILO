using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.AI.Navigation;
using Lilo.Config;
using Lilo.State;
using Lilo.Systems.Monster;
using Lilo.MonoBehaviours.Audio;
using Lilo.MonoBehaviours.Hiding;
using Lilo.MonoBehaviours.Player;
using StarterAssets;

namespace Lilo.MonoBehaviours.Monster
{
    /// <summary>
    /// Thin per-frame adapter over the pure MonsterBrain (spec 002). Owns movement
    /// (NavMeshAgent), arrival/stuck sensing, noise gathering, animator wiring, and
    /// the catch sequence. Dev arena runs the Floor 51 profile until spec 004
    /// provides per-floor wiring.
    ///
    /// Animator mapping (Eggy.controller, all triggers):
    /// - moving (any state) -&gt; "walk" trigger whenever the animator sits in idle
    ///   (the pack's walk state auto-returns to idle, so it is re-fired as needed).
    /// - CATCH -&gt; "attack" trigger once.
    /// - "damaged"/"death" have no gameplay source yet (reserved for later specs).
    ///
    /// FR-017: CurrentState + DistanceToPlayer are read-only for audio (013) /
    /// haptics (014); those systems must not change monster behavior.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterAIController : MonoBehaviour
    {
        private const int RandomSpawnSamples = 48;
        [SerializeField] private GameConfig config;
        [SerializeField] private FloorId floorProfile = FloorId.Floor51;
        [Header("Arena wiring (auto-found by name if empty)")]
        [SerializeField] private Transform patrolRoute;
        [SerializeField] private Transform spawnPresets;
        [SerializeField] private Transform spawnObjectives;
        [SerializeField] private Transform player;

        [Header("Debug (arena testing only — never enable in release)")]
        public bool debugForceMoveNoise;
        public bool debugForceSprintNoise;

        [Header("Audio")]
        [SerializeField] private SfxController sfx;
        [SerializeField] private GameplaySpeedSettings speedSettings;

        /// <summary>
        /// Instance-level one-shot noise pulses (interact/battery, specs 005/006).
        /// Emitters call this on the scene's single MonsterAIController; until those
        /// specs land nothing emits and movement noise is the only channel.
        /// </summary>
        public void EmitPulse(Vector3 position, float radius, float ttl = 0.25f,
            Lilo.Systems.Monster.NoiseSource source = Lilo.Systems.Monster.NoiseSource.Pulse)
        {
            if (radius > 0f)
                _pulses.Add(new PendingPulse(position, radius, Time.time + ttl, source));
        }

        public MonsterState CurrentState => _brain.State;
        public float DistanceToPlayer => player != null ? MonsterBrain.DistXZ(transform.position, player.position) : -1f;

        /// <summary>Most recent movement noise radius computed this frame. Telemetry only.</summary>
        public float CurrentNoiseRadius { get; private set; }

        /// <summary>Most recent movement noise source. Telemetry only.</summary>
        public Lilo.Systems.Monster.NoiseSource CurrentNoiseSource { get; private set; }

        public event Action PlayerCaught;

        private struct PendingPulse
        {
            public Vector3 Position;
            public float Radius;
            public float Expiry;
            public Lilo.Systems.Monster.NoiseSource Source;
            public PendingPulse(Vector3 position, float radius, float expiry,
                Lilo.Systems.Monster.NoiseSource source)
            {
                Position = position;
                Radius = radius;
                Expiry = expiry;
                Source = source;
            }
        }

        private System.Random _rng;

        private NavMeshAgent _agent;
        private Animator _animator;
        private PlayerMovementController _playerMovement;
        private StarterAssetsInputs _starterAssetsInput;
        private MonsterTuningProfile _profile;
        private MonsterBrainState _brain;
        private Vector3[] _waypoints = System.Array.Empty<Vector3>();
        private readonly List<PendingPulse> _pulses = new List<PendingPulse>();
        private readonly List<NoisePulse> _pulsesBuffer = new List<NoisePulse>();
        private Vector3 _lastPlayerPos;
        private bool _hasLastPlayerPos;
        private Vector3 _stuckCheckPos;
        private float _stuckTimer;
        private bool _forceArrived;
        private bool _catchRunning;
        private NavMeshPath _movementPath;
        private float _nextRepathTime;
        private Vector3 _requestedTarget;
        private bool _hasRequestedTarget;
        private MonsterState _movementState;
        private MonsterState _lastLoggedState = (MonsterState)(-1);

        private void Awake()
        {
            // TickCount can repeat when scenes restart quickly. A fresh seed keeps
            // both the initial spawn and later search targets varied between runs.
            _rng = new System.Random(System.Guid.NewGuid().GetHashCode());

            GameConfig cfg = config != null ? config : GameManager.Instance?.Config;
            if (cfg == null)
            {
                Debug.LogError("[Monster] No GameConfig (field empty and no GameManager) — disabling.");
                enabled = false;
                return;
            }
            config = cfg;
            if (sfx == null)
            {
                var sfxGo = GameObject.Find("SfxController");
                if (sfxGo != null)
                    sfx = sfxGo.GetComponent<SfxController>();
            }
            if (speedSettings == null)
                speedSettings = FindAnyObjectByType<GameplaySpeedSettings>();
            FloorId activeFloor = GameManager.Instance != null
                ? GameManager.Instance.State.CurrentFloor
                : floorProfile;
            _profile = cfg.GetMonsterProfile(activeFloor);
            if (!_profile.monsterActive)
            {
                gameObject.SetActive(false); // Floor 52: no monster, no detection (US5).
                return;
            }

            if (patrolRoute == null) patrolRoute = GameObject.Find("MonsterPatrolRoute")?.transform;
            if (spawnPresets == null) spawnPresets = GameObject.Find("MonsterSpawns")?.transform;
            if (spawnObjectives == null) spawnObjectives = GameObject.Find("MonsterSpawnObjectives")?.transform;
            var playerGo = player != null ? player.gameObject : GameObject.Find("PlayerCharacter");
            if (playerGo != null)
            {
                player = playerGo.transform;
                _playerMovement = playerGo.GetComponent<PlayerMovementController>();
                _starterAssetsInput = playerGo.GetComponent<StarterAssetsInputs>();
            }
            if (player == null || patrolRoute == null || patrolRoute.childCount == 0)
            {
                Debug.LogError("[Monster] Missing player, patrol route, or waypoints — run LILO/Setup Monster Arena.");
                enabled = false;
                return;
            }

            _agent = GetComponent<NavMeshAgent>();
            _movementPath = new NavMeshPath();
            _agent.radius = 0.5f;
            _agent.height = 2f;
            _agent.baseOffset = 0f;
            _agent.stoppingDistance = 0.3f;
            _agent.angularSpeed = 240f;
            _agent.updateRotation = false;
            _agent.autoRepath = true;
            _agent.acceleration = 12f;
            _agent.autoBraking = true;

            AlignVisibleModel();
            // Inactive legacy models may still contain an Animator. Never drive them.
            _animator = GetComponentInChildren<Animator>();
            if (_animator != null) _animator.applyRootMotion = false;
            if (_animator == null)
                Debug.LogWarning("[Monster] No Animator under monster; using the visible placeholder without animation.");
            else if (_animator.runtimeAnimatorController == null)
                Debug.LogWarning($"[Monster] Animator '{_animator.name}' has no Animator Controller; monster movement will continue without animation.", _animator);

            if (!EnsureNavMesh())
                return;

            var points = new List<Vector3>();
            foreach (Transform child in patrolRoute)
            {
                if (NavMesh.SamplePosition(child.position, out NavMeshHit waypoint, 3f, _agent.areaMask)
                    && IsReachable(player.position, waypoint.position))
                    points.Add(waypoint.position);
            }
            if (points.Count == 0)
            {
                Debug.LogError("[Monster] No reachable patrol waypoints after rebuilding navigation.");
                enabled = false;
                return;
            }
            _waypoints = points.ToArray();

            if (!PlaceAtValidatedSpawn())
                return;

            _brain = new MonsterBrainState
            {
                State = MonsterState.Patrol,
                WaypointIndex = NearestWaypoint(transform.position, _waypoints),
            };
            _stuckCheckPos = transform.position;
        }

        private bool EnsureNavMesh()
        {
            var surfaceGo = GameObject.Find("NavMesh");
            var surface = surfaceGo != null ? surfaceGo.GetComponent<NavMeshSurface>() : null;
            if (surface == null)
            {
                Debug.LogError("[Monster] No NavMesh surface — run LILO/Setup Monster Arena.");
                enabled = false;
                return false;
            }
            // Scene geometry changes frequently. A serialized bake can still describe
            // an older wall layout, letting the agent path straight through it.
            // Build from the same colliders that stop the player on this floor.
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            if (surface.navMeshData != null
                && NavMesh.SamplePosition(player.position, out _, 1.5f, NavMesh.AllAreas))
                return true;

            // An authored floor without a ground collider still needs a walkable
            // surface. Keep its render-mesh bake as a safe fallback.
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.BuildNavMesh();
            if (surface.navMeshData != null
                && NavMesh.SamplePosition(player.position, out _, 1.5f, NavMesh.AllAreas))
            {
                Debug.LogWarning("[Monster] Floor ground has no usable collider; using render meshes for navigation.", surface);
                return true;
            }

            Debug.LogError("[Monster] No walkable NavMesh at the player after rebuilding the floor.", surface);
            enabled = false;
            return false;
        }

        private bool PlaceAtValidatedSpawn()
        {
            if (spawnPresets == null || spawnPresets.childCount < 2)
            {
                Debug.LogError("[Monster] At least two authored spawn presets are required.");
                enabled = false;
                return false;
            }

            Vector3 entry = player.position;
            var objectives = new List<Vector3>();
            if (spawnObjectives != null)
            {
                foreach (Transform objective in spawnObjectives)
                    objectives.Add(objective.position);
            }

            var candidates = new List<MonsterSpawnCandidate>(spawnPresets.childCount + RandomSpawnSamples);
            var positions = new List<Vector3>(spawnPresets.childCount + RandomSpawnSamples);
            foreach (Transform spawn in spawnPresets)
            {
                positions.Add(spawn.position);
                candidates.Add(new MonsterSpawnCandidate(
                    spawn.position,
                    IsReachable(entry, spawn.position),
                    IsVisibleFromEntry(entry, spawn.position)));
            }

            // Presets ensure hand-authored fallback positions remain available. Add
            // random points sampled from the baked NavMesh so runs are not limited
            // to the same small set of fixed locations.
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            int triangleCount = triangulation.indices != null ? triangulation.indices.Length / 3 : 0;
            for (int i = 0; i < RandomSpawnSamples && triangleCount > 0; i++)
            {
                Vector3 point = RandomPointOnNavMeshTriangle(triangulation, _rng);
                positions.Add(point);
                candidates.Add(new MonsterSpawnCandidate(
                    point,
                    IsReachable(entry, point),
                    IsVisibleFromEntry(entry, point)));
            }

            int selected = MonsterSpawnSelector.ChooseBalancedIndex(
                candidates,
                entry,
                objectives,
                config.monsterSpawnMinDistance,
                config.monsterSpawnMaxDistance,
                config.monsterSpawnObjectiveClearance,
                _rng);
            if (selected < 0)
            {
                selected = MonsterSpawnSelector.ChooseReachableFallbackIndex(
                    candidates,
                    entry,
                    objectives,
                    config.monsterSpawnMinDistance,
                    config.monsterSpawnObjectiveClearance,
                    _rng);
                if (selected < 0)
                {
                    Debug.LogError("[Monster] No reachable NavMesh spawn candidate is available; check the baked floor NavMesh and spawn area mask.");
                    enabled = false;
                    return false;
                }
                Debug.LogWarning("[Monster] No spawn met every safety check; using the safest reachable fallback so the monster AI stays active.");
            }

            Vector3 selectedPosition = positions[selected];
            if (!_agent.Warp(selectedPosition))
            {
                Debug.LogError($"[Monster] Validated spawn at {selectedPosition} was not on the NavMesh.");
                enabled = false;
                return false;
            }
            Debug.Log($"[Monster] Randomized spawn selected at ({selectedPosition.x:0.00}, {selectedPosition.y:0.00}, {selectedPosition.z:0.00}) from {candidates.Count} authored and sampled candidates.");
            return true;
        }

        private static Vector3 RandomPointOnNavMeshTriangle(NavMeshTriangulation triangulation, System.Random rng)
        {
            int triangleCount = triangulation.indices.Length / 3;
            int triangle = rng.Next(triangleCount) * 3;
            Vector3 a = triangulation.vertices[triangulation.indices[triangle]];
            Vector3 b = triangulation.vertices[triangulation.indices[triangle + 1]];
            Vector3 c = triangulation.vertices[triangulation.indices[triangle + 2]];

            // Square-root barycentric sampling distributes points evenly within
            // the selected triangle instead of clustering near one vertex.
            float root = Mathf.Sqrt((float)rng.NextDouble());
            float along = (float)rng.NextDouble();
            return (1f - root) * a + root * (1f - along) * b + root * along * c;
        }

        private static bool IsReachable(Vector3 entry, Vector3 spawn)
        {
            if (!NavMesh.SamplePosition(entry, out NavMeshHit entryHit, 2f, NavMesh.AllAreas)
                || !NavMesh.SamplePosition(spawn, out NavMeshHit spawnHit, 2f, NavMesh.AllAreas))
                return false;

            var path = new NavMeshPath();
            return NavMesh.CalculatePath(entryHit.position, spawnHit.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete;
        }

        private static bool IsVisibleFromEntry(Vector3 entry, Vector3 spawn)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (camera == null)
                return false;

            Vector3 viewport = camera.WorldToViewportPoint(spawn + Vector3.up);
            bool onScreen = viewport.z > 0f
                && viewport.x >= 0f && viewport.x <= 1f
                && viewport.y >= 0f && viewport.y <= 1f;
            if (!onScreen)
                return false;

            Vector3 start = entry + Vector3.up;
            Vector3 end = spawn + Vector3.up;
            Vector3 direction = (end - start).normalized;
            start += direction * 0.6f;
            return !Physics.Linecast(start, end, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        private void Update()
        {
            if (_catchRunning)
                return;

            if (_brain.State == MonsterState.Catch && _brain.CatchFired)
            {
                StartCoroutine(CatchSequence());
                return;
            }

            float dt = Time.deltaTime;

            // Player velocity from position delta (robust to any movement source).
            Vector3 playerPos = player.position;
            float playerSpeed = 0f;
            if (_hasLastPlayerPos && dt > 0f)
                playerSpeed = MonsterBrain.DistXZ(playerPos, _lastPlayerPos) / dt;
            _lastPlayerPos = playerPos;
            _hasLastPlayerPos = true;

            bool moving = playerSpeed > 0.05f || debugForceMoveNoise;
            float walkSpeed = speedSettings != null ? speedSettings.playerWalkSpeed : config.walkSpeed;
            float sprintSpeed = speedSettings != null
                ? speedSettings.playerSprintSpeed
                : config.walkSpeed * config.sprintMultiplier;
            bool sprinting = debugForceSprintNoise
                || (_playerMovement != null && _playerMovement.IsSprinting)
                || (_starterAssetsInput != null && _starterAssetsInput.sprint)
                || (sprintSpeed > walkSpeed
                    && playerSpeed >= (walkSpeed + sprintSpeed) * 0.5f);
            bool hiding = (GameManager.Instance != null && GameManager.Instance.State.IsHiding)
                || HidingController.IsPlayerHiding;
            float moveRadius = MonsterNoise.MovementRadius(config, hiding, moving, sprinting);
            // Sound travels by radius in every direction; facing only limits vision.
            moveRadius *= _profile.noiseSensitivityMultiplier;
            CurrentNoiseRadius = moveRadius;
            CurrentNoiseSource = hiding
                ? Lilo.Systems.Monster.NoiseSource.Hiding
                : moveRadius <= 0f
                    ? Lilo.Systems.Monster.NoiseSource.Silent
                    : sprinting
                        ? Lilo.Systems.Monster.NoiseSource.Sprint
                        : Lilo.Systems.Monster.NoiseSource.Walk;

            _pulses.RemoveAll(p => Time.time > p.Expiry);
            float largestPulse = 0f;
            foreach (var pulse in _pulses)
            {
                float detectingRadius = pulse.Radius * _profile.noiseSensitivityMultiplier;
                if (detectingRadius <= largestPulse) continue;
                largestPulse = detectingRadius;
                if (largestPulse > CurrentNoiseRadius)
                {
                    CurrentNoiseRadius = largestPulse;
                    CurrentNoiseSource = pulse.Source;
                }
            }
            List<NoisePulse> pulses = null;
            if (_pulses.Count > 0)
            {
                _pulsesBuffer.Clear();
                foreach (var p in _pulses)
                {
                    _pulsesBuffer.Add(new NoisePulse
                    {
                        Position = p.Position,
                        Radius = p.Radius * _profile.noiseSensitivityMultiplier,
                    });
                }
                pulses = _pulsesBuffer;
            }
            if (ScratchMarkTrail.TryGetLatestNear(transform.position, out Vector3 scratchMark))
            {
                if (pulses == null)
                {
                    _pulsesBuffer.Clear();
                    pulses = _pulsesBuffer;
                }
                pulses.Add(new NoisePulse
                {
                    Position = scratchMark,
                    Radius = 8f * _profile.noiseSensitivityMultiplier,
                });
            }

            if (hiding)
            {
                _pulses.Clear();
                pulses = null;
                CurrentNoiseRadius = 0f;
                CurrentNoiseSource = Lilo.Systems.Monster.NoiseSource.Hiding;
            }

            bool arrived = _hasRequestedTarget
                && (_brain.Target - _requestedTarget).sqrMagnitude < 0.04f
                && !_agent.pathPending && _agent.hasPath
                && _agent.pathStatus == NavMeshPathStatus.PathComplete
                && _agent.remainingDistance <= _agent.stoppingDistance;
            bool consumedForce = _forceArrived;
            _forceArrived = false;

            var input = new MonsterBrainInput
            {
                DeltaTime = dt,
                MonsterPosition = transform.position,
                PlayerPosition = playerPos,
                MovementNoiseRadius = moveRadius,
                Pulses = pulses,
                Profile = _profile,
                WalkSpeed = config.walkSpeed,
                PatrolSpeed = speedSettings != null
                    ? speedSettings.monsterPatrolSpeed * _profile.movementSpeedMultiplier
                    : 0f,
                ChaseSpeed = speedSettings != null
                    ? speedSettings.monsterChaseSpeed * _profile.movementSpeedMultiplier
                    : 0f,
                ChaseTriggerDistance = config.chaseTriggerDistance,
                PlayerVisible = !hiding && CanSeePlayer(playerPos,
                    _brain.State == MonsterState.Chase || _brain.State == MonsterState.Alert),
                SearchRadius = config.searchRadius,
                CatchRadius = config.catchRadius,
                Waypoints = _waypoints,
                ArrivedAtTarget = arrived || consumedForce,
                Rng = _rng,
                IsPlayerHidden = hiding,
                HidingRevealed = hiding && HidingController.IsPlayerExposed,
            };
            MonsterBrainOutput output = MonsterBrain.Step(ref _brain, input);
            if (_brain.State != _lastLoggedState)
            {
                Debug.Log($"[Monster] {_lastLoggedState} -> {_brain.State} (target={_brain.Target})");
                MonsterHaptics.OnStateChanged(_brain.State);
                if (_brain.State == MonsterState.Chase && sfx != null)
                {
                    sfx.PlayBehindYou(transform.position);
                    sfx.PlayChaseBgm();
                }
                else
                {
                    sfx?.StopChaseBgm();
                }
                _lastLoggedState = _brain.State;
            }

            if (output.CaughtThisStep)
            {
                StartCoroutine(CatchSequence());
                return;
            }

            UpdateMovement(output, input.HidingRevealed);
            UpdateFacing(dt);

            // Stuck safeguard (spec 002 edge cases): never stall forever.
            if (!_agent.isStopped && !arrived)
            {
                if (MonsterBrain.DistXZ(transform.position, _stuckCheckPos) < 0.05f)
                    _stuckTimer += dt;
                else
                {
                    _stuckTimer = 0f;
                    _stuckCheckPos = transform.position;
                }
                if (_stuckTimer >= config.monsterStuckTimeout)
                {
                    _stuckTimer = 0f;
                    _stuckCheckPos = transform.position;
                    OnStuck();
                }
            }
            else
            {
                _stuckTimer = 0f;
                _stuckCheckPos = transform.position;
            }

            DriveAnimator();
        }

        private void AlignVisibleModel()
        {
            // Imported/customized visuals can be offset from their navigation root.
            // Center each active visual branch over the capsule; preserve its authored
            // hover height and internal arrangement (clothes, smoke, bones).
            foreach (Transform child in transform)
            {
                Renderer[] renderers = child.GetComponentsInChildren<Renderer>();
                bool found = false;
                Bounds bounds = default;
                foreach (Renderer renderer in renderers)
                {
                    if (!renderer.enabled || renderer is ParticleSystemRenderer) continue;
                    if (!found) { bounds = renderer.bounds; found = true; }
                    else bounds.Encapsulate(renderer.bounds);
                }
                if (!found) continue;
                Vector3 offset = transform.position - bounds.center;
                offset.y = 0f;
                child.position += offset;
            }
        }

        private void UpdateMovement(MonsterBrainOutput output, bool hidingRevealed)
        {
            float speed = output.Speed * config.monsterSpeedMultiplier;
            _agent.isStopped = speed < 0.01f;
            if (_agent.isStopped) return;
            _agent.speed = speed;
            // Patrol flows through waypoints; investigations still brake at their goal.
            _agent.autoBraking = _brain.State != MonsterState.Patrol;
            bool stateChanged = _movementState != _brain.State;
            bool targetChanged = !_hasRequestedTarget
                || (output.MoveTarget - _requestedTarget).sqrMagnitude > 0.04f;
            if (!stateChanged && Time.time < _nextRepathTime) return;
            if (!stateChanged && !targetChanged && _agent.hasPath && !_agent.isPathStale) return;
            _nextRepathTime = Time.time + 0.2f;
            _movementState = _brain.State;
            _requestedTarget = output.MoveTarget;
            _hasRequestedTarget = true;
            if (NavMesh.SamplePosition(output.MoveTarget, out NavMeshHit hit,
                    hidingRevealed ? 2.5f : 1f, _agent.areaMask)
                && _agent.CalculatePath(hit.position, _movementPath)
                && _movementPath.status == NavMeshPathStatus.PathComplete)
            {
                _agent.SetPath(_movementPath);
            }
            else
            {
                // Do not continue toward an obsolete goal after an unreachable retarget.
                _agent.ResetPath();
                if (_brain.State != MonsterState.Chase) _forceArrived = true;
            }
        }

        private void UpdateFacing(float dt)
        {
            Vector3 direction = _brain.State == MonsterState.Alert
                ? _brain.Target - transform.position
                : _agent.desiredVelocity;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(direction), _agent.angularSpeed * dt);
        }

        private bool CanSeePlayer(Vector3 playerPosition, bool ignoreFieldOfView)
        {
            Vector3 toPlayer = playerPosition - transform.position;
            Vector3 flatToPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
            float visionRange = config.monsterVisionRange * _profile.visionRangeMultiplier;
            if (flatToPlayer.sqrMagnitude > visionRange * visionRange || flatToPlayer.sqrMagnitude < 0.0001f)
                return flatToPlayer.sqrMagnitude < 0.0001f;

            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (!ignoreFieldOfView && Vector3.Angle(flatForward, flatToPlayer) > config.monsterVisionAngle * 0.5f)
                return false;

            Vector3 eye = transform.position + Vector3.up * 1.4f;
            Vector3 target = playerPosition + Vector3.up * 1f;
            Vector3 direction = target - eye;
            RaycastHit[] hits = Physics.RaycastAll(eye, direction.normalized, direction.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            float nearestDistance = float.MaxValue;
            Transform nearest = null;
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    continue;
                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearest = hit.transform;
                }
            }

            if (nearest != null)
                return nearest == player || nearest.IsChildOf(player);

            return true;
        }

        private void OnStuck()
        {
            Debug.LogWarning($"[Monster] Stuck safeguard in {_brain.State} — retrying navigation.");
            _hasRequestedTarget = false;
            _nextRepathTime = 0f;
            if (_brain.State != MonsterState.Chase)
                _forceArrived = true; // patrol/investigate/search advance on arrival.
        }

        private void DriveAnimator()
        {
            if (!HasPlayableAnimator() || _brain.State == MonsterState.Catch)
                return;
            bool moving = _agent.velocity.magnitude > 0.15f;
            // Match the walk cycle to the multiplier applied to NavMesh movement.
            _animator.speed = moving
                ? 0.5f * config.monsterSpeedMultiplier * _profile.movementSpeedMultiplier
                : 1f;
            if (!moving || _animator.IsInTransition(0))
                return;
            // The pack's walk state auto-returns to idle, so re-fire while moving.
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
                _animator.SetTrigger("walk");
        }

        /// <summary>Reserved: no gameplay source damages the monster yet.</summary>
        public void TriggerDamaged()
        {
            if (HasPlayableAnimator()) _animator.SetTrigger("damaged");
        }

        /// <summary>Reserved: the monster never dies (spec 007 kills the player, not it).</summary>
        public void TriggerDeath()
        {
            if (HasPlayableAnimator()) _animator.SetTrigger("death");
        }

        private bool HasPlayableAnimator()
        {
            return _animator != null
                && _animator.isActiveAndEnabled
                && _animator.gameObject.activeInHierarchy
                && _animator.runtimeAnimatorController != null
                && _animator.isInitialized;
        }

        private IEnumerator CatchSequence()
        {
            _catchRunning = true;
            PlayerCaught?.Invoke();
            sfx?.PlayPlayerCaught();
            Time.timeScale = 0f;
            HideGameplayInterface();
            SuspendFollowCamera();

            if (HasPlayableAnimator())
            {
                _animator.speed = 1f;
                _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                _animator.SetTrigger("attack");
            }
            if (_playerMovement != null) _playerMovement.enabled = false; // input stops (US4).
            if (player != null)
            {
                var thirdPerson = player.GetComponent<ThirdPersonController>();
                if (thirdPerson != null) thirdPerson.enabled = false;
            }
            if (_starterAssetsInput != null)
            {
                _starterAssetsInput.MoveInput(Vector2.zero);
                _starterAssetsInput.SprintInput(false);
                _starterAssetsInput.JumpInput(false);
            }
            _agent.isStopped = true;
            var state = GameManager.Instance?.State;
            if (state != null)
            {
                state.LoseLife();
                state.SetRespawnMessage(state.Lives switch
                {
                    2 => "WAIT WHAT WAS THAT? WHAT IS HAPPENING?!",
                    1 => "I FELT IT ALL THROUGH MY SKIN OH GOD",
                    _ => "NO NO NO I DON'T WANT TO FEEL IT AGAIN",
                });
                if (state.Lives > 0)
                    state.ResetForFloorRestart(config);
                else
                    state.SetOutcome(RunOutcome.BadEnding);
            }
            Debug.Log($"[Monster] Player caught — lives remaining: {state?.Lives ?? -1}.");
            yield return PlayDeathCinematic();

            string respawnScene = state != null && state.Lives <= 0
                ? "EpilogueBad"
                : ResolveRespawnScene(state);
            Debug.Log($"[Monster] Respawning on {respawnScene} for floor {state?.CurrentFloor.ToString() ?? "active scene"}.");
            state?.MarkLifeVoiceOverReady();
            Lilo.MonoBehaviours.Input.MobileControlsBootstrap.PrepareForSceneReload();
            // CatchSequence freezes gameplay while its death cinematic runs. Restore
            // scaled time before loading the ending/respawn scene so its fades and
            // typewriter timeline can advance instead of remaining on a black frame.
            Time.timeScale = 1f;
            SceneManager.LoadScene(respawnScene);
        }

        private static void HideGameplayInterface()
        {
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
                canvas.enabled = false;

            foreach (GraphicRaycaster raycaster in FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Include))
                raycaster.enabled = false;
        }

        private static void SuspendFollowCamera()
        {
            var follow = FindAnyObjectByType<global::CameraFollow>();
            if (follow != null) follow.enabled = false;
            var followController = FindAnyObjectByType<Lilo.MonoBehaviours.Camera.CameraFollowController>();
            if (followController != null) followController.enabled = false;
        }

        private IEnumerator PlayDeathCinematic()
        {
            UnityEngine.Camera gameplayCamera = UnityEngine.Camera.main;
            if (gameplayCamera == null)
                gameplayCamera = FindAnyObjectByType<UnityEngine.Camera>();

            Vector3 cameraStart = gameplayCamera != null ? gameplayCamera.transform.position : Vector3.zero;
            Quaternion cameraStartRotation = gameplayCamera != null ? gameplayCamera.transform.rotation : Quaternion.identity;
            float sizeStart = gameplayCamera != null ? gameplayCamera.orthographicSize : 0f;
            Vector3 focus = player != null ? player.position + Vector3.up * 1.15f : transform.position + Vector3.up;
            Vector3 cameraClose = focus - cameraStartRotation * Vector3.forward * 4f;
            float sizeClose = gameplayCamera != null && gameplayCamera.orthographic
                ? Mathf.Min(sizeStart, 4.5f)
                : sizeStart;

            Canvas deathCanvas = CreateDeathOverlay(out Image overlay);
            float zoomDuration = 0.8f;
            float elapsed = 0f;
            while (elapsed < zoomDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / zoomDuration));
                if (gameplayCamera != null)
                {
                    gameplayCamera.transform.position = Vector3.Lerp(cameraStart, cameraClose, t);
                    gameplayCamera.transform.rotation = cameraStartRotation;
                    if (gameplayCamera.orthographic)
                        gameplayCamera.orthographicSize = Mathf.Lerp(sizeStart, sizeClose, t);
                }
                overlay.color = new Color(0.38f, 0.015f, 0.025f, 0.18f * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            // Let the attack read clearly before the image closes to black.
            yield return new WaitForSecondsRealtime(0.65f);
            elapsed = 0f;
            const float fadeDuration = 0.75f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                overlay.color = Color.Lerp(new Color(0.18f, 0f, 0.015f, 0.18f), Color.black, t);
                yield return null;
            }
            overlay.color = Color.black;
            yield return new WaitForSecondsRealtime(0.2f);
            if (deathCanvas != null)
                Destroy(deathCanvas.gameObject);
        }

        private static Canvas CreateDeathOverlay(out Image overlay)
        {
            var canvasObject = new GameObject("DeathCinematicCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var overlayObject = new GameObject("Fade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayObject.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = overlayObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            overlay = overlayObject.GetComponent<Image>();
            overlay.color = new Color(0.38f, 0.015f, 0.025f, 0f);
            overlay.raycastTarget = false;
            return canvas;
        }

        private void OnDisable()
        {
            MonsterHaptics.Stop();
        }

        private static string ResolveRespawnScene(GameState state)
        {
            // Keep the respawn tied to the persistent floor state. This prevents a
            // Floor 2 death from falling back to the scene that started the run.
            if (state != null)
            {
                string floorScene = state.CurrentFloor switch
                {
                    FloorId.Floor50 => "OfficeLevel3",
                    FloorId.Floor51 => "OfficeLevel2",
                    FloorId.Floor52 => "OfficeLevel1",
                    _ => string.Empty,
                };

                if (!string.IsNullOrEmpty(floorScene)
                    && Application.CanStreamedLevelBeLoaded(floorScene))
                    return floorScene;
            }

            return SceneManager.GetActiveScene().name;
        }

        private static int NearestWaypoint(Vector3 pos, Vector3[] waypoints)
        {
            int best = 0;
            float bestD = MonsterBrain.DistXZ(pos, waypoints[0]);
            for (int k = 1; k < waypoints.Length; k++)
            {
                float d = MonsterBrain.DistXZ(pos, waypoints[k]);
                if (d < bestD)
                {
                    bestD = d;
                    best = k;
                }
            }
            return best;
        }
    }
}
