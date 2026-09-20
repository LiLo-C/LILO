using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lilo.MonoBehaviours.Interaction
{
    /// <summary>
    /// Arena exit door (spec 005). Distance-based check each frame — when the player
    /// is within interactRadius, the scene reloads (escape). iOS-friendly, no triggers.
    /// </summary>
    public class ExitDoorInteraction : MonoBehaviour
    {
        [SerializeField] private float interactRadius = 2f;

        private Transform _player;
        private bool _triggered;

        private void Start()
        {
            var playerGo = GameObject.Find("PlayerCharacter");
            if (playerGo != null)
                _player = playerGo.transform;
        }

        private void Update()
        {
            if (_player == null || _triggered) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist <= interactRadius)
            {
                _triggered = true;
                Debug.Log("[ExitDoor] Player escaped — reloading scene.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
