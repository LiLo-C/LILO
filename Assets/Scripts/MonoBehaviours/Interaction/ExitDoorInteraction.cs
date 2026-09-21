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
        [SerializeField] private float interactRadius = 2f;
        [Tooltip("Optional next scene. If empty, this dev slice records a GoodEnding and stays in-scene.")]
        [SerializeField] private string nextSceneName;
        [Tooltip("Advance the persistent game state before loading the next scene.")]
        [SerializeField] private bool advanceToNextFloor;
        [SerializeField] private FloorId nextFloor = FloorId.Floor50;
        [SerializeField] private MonsterAIController monster;

        private Transform _player;
        private bool _triggered;

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
                _triggered = true;
                var state = GameManager.Instance?.State;

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
                    Debug.Log($"[ExitDoor] Player escaped — loading {nextSceneName}.");
                    SceneManager.LoadScene(nextSceneName);
                    return;
                }

                if (state != null)
                    state.SetOutcome(RunOutcome.GoodEnding);
                Debug.Log("[ExitDoor] Player escaped — GoodEnding recorded for the playable slice.");
                enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
