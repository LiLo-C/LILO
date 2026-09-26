using UnityEngine;
using UnityEngine.SceneManagement;
using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Monster;
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
                Debug.LogWarning("[ExitDoor] No mesh or collider bounds found for the exit highlight.", this);
                return;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("[ExitDoor] Could not find a shader for the visible exit frame.", this);
                return;
            }

            _exitFrameMaterial = new Material(shader) { name = "Exit Door Frame (Runtime)" };
            if (_exitFrameMaterial.HasProperty(BaseColorId)) _exitFrameMaterial.SetColor(BaseColorId, ExitFrameColor);
            if (_exitFrameMaterial.HasProperty(ColorId)) _exitFrameMaterial.SetColor(ColorId, ExitFrameColor);
            _exitFrameMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

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
                var edgeObject = new GameObject($"Exit Highlight Edge {i + 1}");
                edgeObject.transform.SetParent(transform, false);
                var line = edgeObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.SetPosition(0, corners[BoxEdges[i].a]);
                line.SetPosition(1, corners[BoxEdges[i].b]);
                line.startWidth = 0.035f;
                line.endWidth = 0.035f;
                line.numCapVertices = 3;
                line.alignment = LineAlignment.View;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.sharedMaterial = _exitFrameMaterial;
                line.startColor = ExitFrameColor;
                line.endColor = ExitFrameColor;
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
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
