using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Audio;
using Lilo.MonoBehaviours.Monster;
using Lilo.State;
using UnityEngine;

namespace Lilo.MonoBehaviours.Interaction
{
    /// <summary>Collects the floor-specific key needed to unlock that floor's exit.</summary>
    public sealed class AccessKeyPickup : MonoBehaviour
    {
        public const float DefaultInteractionRadius = 1.8f;

        [SerializeField] private string keyId;
        [SerializeField, Min(0.25f)] private float interactRadius = DefaultInteractionRadius;
        [SerializeField] private GameConfig config;
        [SerializeField] private SfxController sfx;
        [SerializeField] private MonsterAIController monster;

        private Transform _player;
        private bool _collected;
        private PlayerXRayOutline _outline;
        private AccessKeySpawnTable _supportingTable;

        public float InteractionRadius => interactRadius;

        public static string KeyIdForFloor(FloorId floor) => $"access-key-{floor}";

        public void Configure(string id, GameConfig gameConfig, AccessKeySpawnTable supportingTable = null)
        {
            keyId = id;
            config = gameConfig;
            _supportingTable = supportingTable;
        }

        private void Awake()
        {
            _outline = GetComponent<PlayerXRayOutline>();
            if (_outline == null)
                _outline = gameObject.AddComponent<PlayerXRayOutline>();
            _outline.ConfigureOutline(PlayerXRayOutline.DefaultOutlineColor, 0.008f, true);
            _outline.EnablePickupPulse();
            _outline.SetOutlineVisible(true);
        }

        private void Start()
        {
            if (config == null) config = GameManager.Instance?.Config;
            _player = GameObject.Find("PlayerCharacter")?.transform;
            sfx = sfx != null ? sfx : GameObject.Find("SfxController")?.GetComponent<SfxController>();
            monster = monster != null ? monster : GameObject.Find("MonsterPlaceholder")?.GetComponent<MonsterAIController>();

            // This pickup is distance-checked; its tiny decorative mesh must not block movement.
            Collider pickupCollider = GetComponent<Collider>();
            if (pickupCollider != null) pickupCollider.enabled = false;
        }

        private void Update()
        {
            if (_collected || _player == null) return;
            GameState state = GameManager.Instance?.State;
            if (state == null || string.IsNullOrEmpty(keyId)) return;
            if (state.HasKey(keyId))
            {
                HideCollectedObject();
                return;
            }
            Vector2 pickupXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerXZ = new Vector2(_player.position.x, _player.position.z);
            if (Vector2.Distance(pickupXZ, playerXZ) > interactRadius
                || Mathf.Abs(transform.position.y - _player.position.y) > 2.5f
                || !HasClearPickupPath(_player))
                return;

            if (!state.AddKey(keyId)) return;

            _collected = true;
            Debug.Log($"[AccessKey] Collected key for {state.CurrentFloor}.");
            if (sfx != null) sfx.PlayKeyPickup();
            if (monster != null && config != null)
                monster.EmitPulse(transform.position, config.noiseInteract);
            HideCollectedObject();
        }

        private void HideCollectedObject()
        {
            _collected = true;
            _outline?.SetOutlineVisible(false);
            gameObject.SetActive(false);
        }

        public bool HasClearPickupPath(Transform player)
        {
            if (player == null) return false;
            // Cast from the player's upper body to just above the key. A wall collider
            // between them blocks pickup, while the key's own collider never does.
            CharacterController body = player.GetComponent<CharacterController>();
            Vector3 origin = body != null
                ? player.TransformPoint(body.center + Vector3.up * (body.height * 0.3f))
                : player.position + Vector3.up * 1.45f;
            Vector3 target = transform.position + Vector3.up * 0.08f;
            Vector3 offset = target - origin;
            float distance = offset.magnitude;
            if (distance <= 0.01f)
                return true;

            RaycastHit[] hits = Physics.RaycastAll(origin, offset / distance, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                Transform hitTransform = hit.collider.transform;
                if (hitTransform == player || hitTransform.IsChildOf(player))
                    continue;
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                    continue;
                // The spawn authoring already allows this table's coarse desk
                // proxy to overlap the key. Apply the same rule when reaching
                // for it, while still treating actual walls as blockers.
                if (_supportingTable != null
                    && (hitTransform == _supportingTable.transform
                        || hitTransform.IsChildOf(_supportingTable.transform)
                        || _supportingTable.IsPickupReachProxy(hit.collider)))
                    continue;
                return false;
            }

            return true;
        }
    }
}
