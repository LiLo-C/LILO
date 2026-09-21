using UnityEngine;

namespace Lilo.MonoBehaviours
{
    /// <summary>
    /// Scene-local speed tuning exposed in the Inspector. Both the player and monster
    /// adapters read this same component, so playtest tuning does not require editing a prefab.
    /// </summary>
    public sealed class GameplaySpeedSettings : MonoBehaviour
    {
        [Header("Player")]
        [Min(0f)] public float playerWalkSpeed = 4f;
        [Min(0f)] public float playerSprintSpeed = 6.4f;

        [Header("Monster")]
        [Min(0f)] public float monsterPatrolSpeed = 4f;
        [Min(0f)] public float monsterChaseSpeed = 2.8f;
    }
}
