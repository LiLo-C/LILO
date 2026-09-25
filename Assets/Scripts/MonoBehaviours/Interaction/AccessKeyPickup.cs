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
        [SerializeField] private string keyId;
        [SerializeField, Min(0.25f)] private float interactRadius = 1.5f;
        [SerializeField] private GameConfig config;
        [SerializeField] private SfxController sfx;
        [SerializeField] private MonsterAIController monster;

        private Transform _player;
        private bool _collected;

        public static string KeyIdForFloor(FloorId floor) => $"access-key-{floor}";

        public void Configure(string id, GameConfig gameConfig)
        {
            keyId = id;
            config = gameConfig;
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
            if (Vector3.Distance(transform.position, _player.position) > interactRadius) return;

            GameState state = GameManager.Instance?.State;
            if (state == null || string.IsNullOrEmpty(keyId) || !state.AddKey(keyId)) return;

            _collected = true;
            Debug.Log($"[AccessKey] Collected key for {state.CurrentFloor}.");
            if (sfx != null) sfx.PlayKeyPickup();
            if (monster != null && config != null)
                monster.EmitPulse(transform.position, config.noiseInteract);
            gameObject.SetActive(false);
        }
    }
}
