using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
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

        [Header("Vision")]
        [SerializeField, Min(1f)] private float visionRange = 12f;
        [SerializeField, Range(10f, 180f)] private float visionAngle = 110f;

        /// <summary>
        /// Instance-level one-shot noise pulses (interact/battery, specs 005/006).
        /// Emitters call this on the scene's single MonsterAIController; until those
        /// specs land nothing emits and movement noise is the only channel.
        /// </summary>
        public void EmitPulse(Vector3 position, float radius, float ttl = 0.25f)
        {
            if (radius > 0f)
                _pulses.Add(new PendingPulse(position, radius, Time.time + ttl));
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
            public PendingPulse(Vector3 position, float radius, float expiry)
            {
                Position = position;
                Radius = radius;
                Expiry = expiry;
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
        private MonsterState _lastLoggedState = (MonsterState)(-1);

        private void Awake()
        {
            _rng = new System.Random(System.Environment.TickCount);

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
                speedSettings = FindFirstObjectByType<GameplaySpeedSettings>();
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
            _agent.radius = 0.5f;
            _agent.height = 2f;
            _agent.baseOffset = 0f;
            _agent.stoppingDistance = 0.3f;
            _agent.angularSpeed = 360f;
            _agent.acceleration = 12f;
            _agent.autoBraking = true;

            _animator = GetComponentInChildren<Animator>(true);
            if (_animator == null)
                Debug.LogWarning("[Monster] No Animator under monster; using the visible placeholder without animation.");

            if (!EnsureNavMesh())
                return;

            var points = new List<Vector3>();
            foreach (Transform child in patrolRoute) points.Add(child.position);
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
            if (surface.navMeshData == null)
            {
                surface.BuildNavMesh();
                Debug.Log("[Monster] NavMesh baked at startup (dev arena).");
            }
            return surface.navMeshData != null;
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

            var candidates = new List<MonsterSpawnCandidate>(spawnPresets.childCount);
            var transforms = new List<Transform>(spawnPresets.childCount);
            foreach (Transform spawn in spawnPresets)
            {
                transforms.Add(spawn);
                candidates.Add(new MonsterSpawnCandidate(
                    spawn.position,
                    IsReachable(entry, spawn.position),
                    IsVisibleFromEntry(entry, spawn.position)));
            }

            int selected = MonsterSpawnSelector.ChooseValidIndex(
                candidates,
                entry,
                objectives,
                config.monsterSpawnMinDistance,
                config.monsterSpawnObjectiveClearance,
                _rng);
            if (selected < 0)
            {
                Debug.LogError("[Monster] No spawn preset passed reachability, distance, visibility, and objective-clearance validation.");
                enabled = false;
                return false;
            }

            if (!_agent.Warp(transforms[selected].position))
            {
                Debug.LogError($"[Monster] Validated spawn '{transforms[selected].name}' was not on the NavMesh.");
                enabled = false;
                return false;
            }
            return true;
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
            bool sprinting = debugForceSprintNoise
                || (_playerMovement != null ? _playerMovement.IsSprinting
                    : (_starterAssetsInput != null ? _starterAssetsInput.sprint
                        : playerSpeed > config.walkSpeed * config.sprintMultiplier * 0.9f));
            bool hiding = (GameManager.Instance != null && GameManager.Instance.State.IsHiding)
                || HidingController.IsPlayerHiding;
            float moveRadius = MonsterNoise.MovementRadius(config, hiding, moving, sprinting);
            CurrentNoiseRadius = moveRadius;
            CurrentNoiseSource = hiding
                ? Lilo.Systems.Monster.NoiseSource.Hiding
                : moveRadius <= 0f
                    ? Lilo.Systems.Monster.NoiseSource.Silent
                    : sprinting
                        ? Lilo.Systems.Monster.NoiseSource.Sprint
                        : Lilo.Systems.Monster.NoiseSource.Walk;

            _pulses.RemoveAll(p => Time.time > p.Expiry);
            if (!hiding)
            {
                float largestPulse = 0f;
                foreach (var pulse in _pulses)
                    largestPulse = Mathf.Max(largestPulse, pulse.Radius);
                if (largestPulse > CurrentNoiseRadius)
                {
                    CurrentNoiseRadius = largestPulse;
                    CurrentNoiseSource = Lilo.Systems.Monster.NoiseSource.Pulse;
                }
            }
            List<NoisePulse> pulses = null;
            if (_pulses.Count > 0)
            {
                _pulsesBuffer.Clear();
                foreach (var p in _pulses) _pulsesBuffer.Add(new NoisePulse { Position = p.Position, Radius = p.Radius });
                pulses = _pulsesBuffer;
            }

            bool arrived = !(_agent.pathPending)
                && (_agent.pathStatus == NavMeshPathStatus.PathInvalid
                    || _agent.remainingDistance <= _agent.stoppingDistance);
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
                PatrolSpeed = speedSettings != null ? speedSettings.monsterPatrolSpeed : 0f,
                ChaseSpeed = speedSettings != null ? speedSettings.monsterChaseSpeed : 0f,
                ChaseTriggerDistance = config.chaseTriggerDistance,
                PlayerVisible = !hiding && CanSeePlayer(playerPos), // hiding defeats sight (spec 001); brain logic untouched
                SearchRadius = config.searchRadius,
                CatchRadius = config.catchRadius,
                Waypoints = _waypoints,
                ArrivedAtTarget = arrived || consumedForce,
                Rng = _rng,
                IsPlayerHidden = hiding,
            };
            MonsterBrainOutput output = MonsterBrain.Step(ref _brain, input);

            if (_brain.State != _lastLoggedState)
            {
                Debug.Log($"[Monster] {_lastLoggedState} -> {_brain.State} (target={_brain.Target})");
                if (_brain.State == MonsterState.Chase && sfx != null)
                {
                    sfx.PlayBehindYou();
                    sfx.PlayHorrorChase();
                }
                else if (_brain.State == MonsterState.Catch && sfx != null)
                {
                    sfx.PlayHorrorChase();
                }
                _lastLoggedState = _brain.State;
            }

            if (output.CaughtThisStep)
            {
                StartCoroutine(CatchSequence());
                return;
            }

            _agent.isStopped = output.Speed < 0.01f;
            if (!_agent.isStopped)
            {
                _agent.speed = output.Speed;
                if ((_agent.destination - output.MoveTarget).sqrMagnitude > 0.01f)
                    _agent.SetDestination(output.MoveTarget);
            }

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

        private bool CanSeePlayer(Vector3 playerPosition)
        {
            Vector3 toPlayer = playerPosition - transform.position;
            Vector3 flatToPlayer = Vector3.ProjectOnPlane(toPlayer, Vector3.up);
            if (flatToPlayer.sqrMagnitude > visionRange * visionRange || flatToPlayer.sqrMagnitude < 0.0001f)
                return flatToPlayer.sqrMagnitude < 0.0001f;

            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            if (Vector3.Angle(flatForward, flatToPlayer) > visionAngle * 0.5f)
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
            Debug.LogWarning($"[Monster] Stuck safeguard in {_brain.State} — skipping ahead.");
            if (_brain.State == MonsterState.Chase)
                _agent.SetDestination(_brain.Target); // repath, keep pushing last known.
            else
                _forceArrived = true; // patrol/investigate/search advance on arrival.
        }

        private void DriveAnimator()
        {
            if (_animator == null || _brain.State == MonsterState.Catch)
                return;
            bool moving = _agent.velocity.magnitude > 0.15f;
            if (!moving || _animator.IsInTransition(0))
                return;
            // The pack's walk state auto-returns to idle, so re-fire while moving.
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("idle"))
                _animator.SetTrigger("walk");
        }

        /// <summary>Reserved: no gameplay source damages the monster yet.</summary>
        public void TriggerDamaged()
        {
            if (_animator != null) _animator.SetTrigger("damaged");
        }

        /// <summary>Reserved: the monster never dies (spec 007 kills the player, not it).</summary>
        public void TriggerDeath()
        {
            if (_animator != null) _animator.SetTrigger("death");
        }

        private IEnumerator CatchSequence()
        {
            _catchRunning = true;
            PlayerCaught?.Invoke();
            if (_animator != null) _animator.SetTrigger("attack");
            if (_playerMovement != null) _playerMovement.enabled = false; // input stops (US4).
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
                if (state.Lives > 0)
                    state.ResetForFloorRestart(config);
                else
                    state.SetOutcome(RunOutcome.BadEnding);
            }
            Debug.Log($"[Monster] Player caught — lives remaining: {state?.Lives ?? -1}.");
            yield return new WaitForSeconds(1.6f);

            string respawnScene = ResolveRespawnScene(state);
            Debug.Log($"[Monster] Respawning on {respawnScene} for floor {state?.CurrentFloor.ToString() ?? "active scene"}.");
            SceneManager.LoadScene(respawnScene);
        }

        private static string ResolveRespawnScene(GameState state)
        {
            // Keep the respawn tied to the persistent floor state. This prevents a
            // Floor 2 death from falling back to the scene that started the run.
            if (state != null)
            {
                string floorScene = state.CurrentFloor switch
                {
                    FloorId.Floor50 => "OfficeLevel2",
                    FloorId.Floor51 => "OfficeLevel1",
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
