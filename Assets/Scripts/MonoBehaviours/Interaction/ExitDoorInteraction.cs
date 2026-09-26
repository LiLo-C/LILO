using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Monster;
using Lilo.MonoBehaviours.Cinematics;
using Lilo.State;

namespace Lilo.MonoBehaviours.Interaction
{
    /// <summary>
    /// Arena exit door (spec 005). Distance-based check each frame — when the player
    /// is within interactRadius, the scene reloads (escape). iOS-friendly, no triggers.
    /// </summary>
    public class ExitDoorInteraction : MonoBehaviour
    {
        private static readonly Color ExitOutlineColor = new Color(0.35f, 1f, 0.62f, 1f);
        private static readonly Color ExitFrameColor = new Color(0.12f, 1f, 0.32f, 1f);
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly (int a, int b)[] BoxEdges =
        {
            (0, 1), (0, 2), (0, 4), (1, 3), (1, 5), (2, 3),
            (2, 6), (3, 7), (4, 5), (4, 6), (5, 7), (6, 7),
        };

        [SerializeField] private float interactRadius = 2f;
        [Tooltip("Optional next scene. If empty, this dev slice records a GoodEnding and stays in-scene.")]
        [SerializeField] private string nextSceneName;
        [Tooltip("Advance the persistent game state before loading the next scene.")]
        [SerializeField] private bool advanceToNextFloor;
        [SerializeField] private FloorId nextFloor = FloorId.Floor50;
        [SerializeField] private MonsterAIController monster;

        private Transform _player;
        private bool _triggered;
        private bool _blockedMessageShown;
        private Material _exitFrameMaterial;
        private GameObject _exitFrameRoot;

        public float InteractionRadius => interactRadius;
        public bool RequiresAccessKey
        {
            get
            {
                var manager = GameManager.Instance;
                return manager != null && manager.Config != null && manager.State != null
                    && manager.Config.GetLockedDoorCount(manager.State.CurrentFloor) > 0;
            }
        }
        public bool HasRequiredAccessKey
        {
            get
            {
                var state = GameManager.Instance?.State;
                return state != null && state.HasKey(AccessKeyPickup.KeyIdForFloor(state.CurrentFloor));
            }
        }

        private void Awake()
        {
            PlayerXRayOutline outline = GetComponent<PlayerXRayOutline>();
            if (outline == null)
                outline = gameObject.AddComponent<PlayerXRayOutline>();
            outline.ConfigureOutline(ExitOutlineColor, 0.012f, true);
            outline.SetOutlineVisible(true);
            CreateVisibleDoorFrame();
        }

        private void CreateVisibleDoorFrame()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;
            foreach (Renderer source in renderers)
            {
                if (source == null || source is LineRenderer) continue;
                if (!hasBounds)
                {
                    bounds = source.bounds;
                    hasBounds = true;
                }
                else
                    bounds.Encapsulate(source.bounds);
            }

            if (!hasBounds)
            {
                Collider targetCollider = GetComponentInChildren<Collider>();
                if (targetCollider != null)
                {
                    bounds = targetCollider.bounds;
                    hasBounds = true;
                }
            }

            if (!hasBounds || bounds.size.sqrMagnitude < 0.0001f)
            {
                // A few authored door placeholders are interaction transforms only,
                // with no renderer/collider to outline. Keep the exit discoverable.
                Debug.LogWarning("[ExitDoor] No mesh or collider bounds found; using a doorway-sized highlight.", this);
                bounds = new Bounds(transform.position + Vector3.up * 1.1f,
                    new Vector3(1.6f, 2.2f, 0.14f));
            }

            if (bounds.size.x > 5f || bounds.size.z > 5f || bounds.size.y > 5f)
                bounds = new Bounds(transform.position + Vector3.up * 1.1f,
                    new Vector3(1.6f, 2.2f, 0.14f));

            // Some floor scenes author the exit as a thin trigger pad at the
            // doorway. Raise a simple doorway-shaped frame there instead of
            // drawing an almost invisible outline flat against the floor.
            if (bounds.size.y < 0.25f)
            {
                bounds = new Bounds(
                    new Vector3(bounds.center.x, bounds.min.y + 1.1f, bounds.center.z),
                    new Vector3(1.6f, 2.2f, 0.14f));
            }

            // Some placeholders extend below the walkable floor. Only frame the
            // part above that surface so the reveal doesn't draw buried bars.
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit floor, 2f, NavMesh.AllAreas)
                && floor.position.y + 0.02f < bounds.max.y)
            {
                Vector3 minimum = bounds.min;
                minimum.y = Mathf.Max(minimum.y, floor.position.y + 0.02f);
                bounds.SetMinMax(minimum, bounds.max);
            }

            Shader shader = Shader.Find("LILO/ExitDoorHighlight");
            if (shader == null)
            {
                Debug.LogError("[ExitDoor] Could not find a shader for the visible exit frame.", this);
                return;
            }

            _exitFrameMaterial = new Material(shader) { name = "Exit Door Frame (Runtime)" };
            if (_exitFrameMaterial.HasProperty(BaseColorId)) _exitFrameMaterial.SetColor(BaseColorId, ExitFrameColor);
            if (_exitFrameMaterial.HasProperty(ColorId)) _exitFrameMaterial.SetColor(ColorId, ExitFrameColor);
            // Level 2 has a wall between the camera and the exit. Render just the
            // frame above the scene depth so that wall can't hide the objective.
            // Keep the bars in an identity-transform root. Parenting them to a
            // scaled placeholder stretches the frame away from the actual door.
            _exitFrameRoot = new GameObject("Exit Door Highlight");
            SceneManager.MoveGameObjectToScene(_exitFrameRoot, gameObject.scene);

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
                new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z),
                new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z),
                new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z),
            };

            for (int i = 0; i < BoxEdges.Length; i++)
            {
                Vector3 start = corners[BoxEdges[i].a];
                Vector3 end = corners[BoxEdges[i].b];
                Vector3 delta = end - start;
                if (delta.sqrMagnitude < 0.0001f) continue;

                GameObject edgeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                edgeObject.name = $"Exit Highlight Edge {i + 1}";
                edgeObject.transform.SetParent(_exitFrameRoot.transform, false);
                edgeObject.transform.position = (start + end) * 0.5f;
                edgeObject.transform.rotation = Quaternion.LookRotation(delta.normalized);
                edgeObject.transform.localScale = new Vector3(0.045f, 0.045f, delta.magnitude);
                Collider edgeCollider = edgeObject.GetComponent<Collider>();
                if (edgeCollider != null) Destroy(edgeCollider);
                Renderer edgeRenderer = edgeObject.GetComponent<Renderer>();
                edgeRenderer.sharedMaterial = _exitFrameMaterial;
                edgeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                edgeRenderer.receiveShadows = false;
            }
        }

        public void ConfigureTransition(string sceneName, bool advance, FloorId destinationFloor)
        {
            nextSceneName = sceneName;
            advanceToNextFloor = advance;
            nextFloor = destinationFloor;
            _triggered = false;
        }

        private void Start()
        {
            var playerGo = GameObject.Find("PlayerCharacter");
            if (playerGo != null)
                _player = playerGo.transform;
            else
                Debug.LogWarning("[ExitDoor] PlayerCharacter not found — door interaction disabled.");

            if (monster == null)
            {
                var monsterGo = GameObject.Find("MonsterPlaceholder");
                if (monsterGo != null) monster = monsterGo.GetComponent<MonsterAIController>();
            }
        }

        private void Update()
        {
            if (_player == null || _triggered) return;

            // Exit areas live on the floor, while the character root is elevated by
            // the capsule height. Use horizontal distance so the floor Y offset does
            // not make a reachable green square fail its interaction check.
            Vector2 exitXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerXZ = new Vector2(_player.position.x, _player.position.z);
            float dist = Vector2.Distance(exitXZ, playerXZ);
            if (dist <= interactRadius)
            {
                var state = GameManager.Instance?.State;
                string requiredKeyId = state != null ? AccessKeyPickup.KeyIdForFloor(state.CurrentFloor) : string.Empty;
                bool requiresKey = RequiresAccessKey;
                if (requiresKey && (state == null || !state.HasKey(requiredKeyId)))
                {
                    if (!_blockedMessageShown)
                    {
                        Debug.Log("[ExitDoor] Locked — find this floor's access key first.", this);
                        _blockedMessageShown = true;
                    }
                    return;
                }

                _triggered = true;

                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
                    {
                        Debug.LogError($"[ExitDoor] Cannot load '{nextSceneName}'. "
                            + "Make sure the scene is enabled in Build Settings, then regenerate the iOS build.");
                        _triggered = false;
                        return;
                    }

                    if (nextSceneName == "OfficeLevel2" || nextSceneName == "OfficeLevel3")
                    {
                        if (!FloorTransitionController.Begin(nextSceneName, nextFloor))
                        {
                            _triggered = false;
                            return;
                        }
                        if (state != null && requiresKey)
                            state.RemoveKey(requiredKeyId);
                        return;
                    }

                    if (state != null && advanceToNextFloor)
                        state.AdvanceToFloor(nextFloor);
                    if (state != null && requiresKey)
                        state.RemoveKey(requiredKeyId);
                    if (state != null && (nextSceneName == "GoodEnding" || nextSceneName == "Epilogue"))
                        state.SetOutcome(RunOutcome.GoodEnding);
                    Debug.Log($"[ExitDoor] Player escaped — loading {nextSceneName}.");
                    Lilo.MonoBehaviours.Input.MobileControlsBootstrap.PrepareForSceneReload();
                    SceneManager.LoadScene(nextSceneName);
                    return;
                }

                if (state != null)
                    state.SetOutcome(RunOutcome.GoodEnding);
                if (state != null && requiresKey)
                    state.RemoveKey(requiredKeyId);
                Debug.Log("[ExitDoor] Player escaped — GoodEnding recorded for the playable slice.");
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_exitFrameMaterial != null)
                Destroy(_exitFrameMaterial);
            if (_exitFrameRoot != null)
                Destroy(_exitFrameRoot);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
