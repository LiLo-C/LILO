using UnityEngine;
using Lilo.Config;
using Lilo.MonoBehaviours.Audio;
using Lilo.MonoBehaviours.Monster;

namespace Lilo.MonoBehaviours.Interaction
{
    /// <summary>
    /// Collectible battery (spec 006). Distance-based check each frame — when the player
    /// is within interactRadius, the battery is picked up and a noise pulse fires.
    /// iOS-friendly, no triggers or Rigidbody needed.
    /// </summary>
    public class BatteryPickup : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private float interactRadius = 2f;
        [Tooltip("Reference to the scene's MonsterAIController for noise pulse emission.")]
        [SerializeField] private MonsterAIController monster;
        [SerializeField] private SfxController sfx;

        private Transform _player;
        private bool _pickedUp;

        private void Start()
        {
            var playerGo = GameObject.Find("PlayerCharacter");
            if (playerGo != null)
                _player = playerGo.transform;

            if (config == null)
                config = GameManager.Instance?.Config;

            if (monster == null)
            {
                var monsterGo = GameObject.Find("MonsterPlaceholder");
                if (monsterGo != null) monster = monsterGo.GetComponent<MonsterAIController>();
            }
            if (sfx == null)
            {
                var sfxGo = GameObject.Find("SfxController");
                if (sfxGo != null) sfx = sfxGo.GetComponent<SfxController>();
            }
        }

        private void Update()
        {
            if (_player == null || _pickedUp) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= interactRadius)
            {
                PickUp();
            }
        }

        private void PickUp()
        {
            _pickedUp = true;

            var state = GameManager.Instance?.State;
            if (state != null && config != null)
                state.AddInstalledBatteryCharge(config.batteryDuration * 0.25f, config.batteryDuration);

            Debug.Log("[Battery] Picked up — restored 25% charge.");
            if (sfx != null) sfx.PlayBatteryPickup();

            if (monster != null && config != null)
                monster.EmitPulse(transform.position, config.noiseBatterySwap);

            gameObject.SetActive(false);
        }
    }
}
