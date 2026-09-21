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

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= interactRadius)
            {
                _triggered = true;
                var state = GameManager.Instance?.State;

                if (!string.IsNullOrEmpty(nextSceneName)
                    && Application.CanStreamedLevelBeLoaded(nextSceneName))
                {
                    if (state != null && advanceToNextFloor)
                        state.AdvanceToFloor(nextFloor);
                    Debug.Log($"[ExitDoor] Player escaped — loading {nextSceneName}.");
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    if (state != null)
                        state.SetOutcome(RunOutcome.GoodEnding);
                    Debug.Log("[ExitDoor] Player escaped — GoodEnding recorded for the playable slice.");
                    enabled = false;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
