using Lilo.Config;
using Lilo.MonoBehaviours;
using Lilo.MonoBehaviours.Interaction;
using UnityEngine;

namespace Lilo.Systems.GameLoop
{
    /// <summary>Small scene adapter that connects authored floors to the persistent run state.</summary>
    // GameManager initializes the persistent run first (-1000). Apply this scene's
    // floor before gameplay components (including MonsterAIController) read it.
    [DefaultExecutionOrder(-900)]
    public sealed class GameLoopSceneController : MonoBehaviour
    {
        [SerializeField] private GameLoopSceneRole role;
        [SerializeField] private FloorId floor = FloorId.Floor51;
        [SerializeField] private string nextSceneName;
        [SerializeField] private FloorId nextFloor = FloorId.Floor50;

        private void Awake()
        {
            SoundSettingsStore.Apply();
            if (role == GameLoopSceneRole.Gameplay)
            {
                GameManager.Instance?.State?.AdvanceToFloor(floor);
                var exit = FindAnyObjectByType<ExitDoorInteraction>();
                if (exit != null)
                    exit.ConfigureTransition(nextSceneName, !string.IsNullOrEmpty(nextSceneName), nextFloor);
            }
        }
    }
}
