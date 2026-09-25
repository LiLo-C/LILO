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
        [Min(0f)] public float playerWalkSpeed = 1f;
        [Min(0f)] public float playerSprintSpeed = 3f;

        [Header("Monster")]
        [Tooltip("Base monster patrol speed before GameConfig's monster speed multiplier.")]
        [Min(0f)] public float monsterPatrolSpeed = 0.5f;
        [Tooltip("Base monster chase speed before GameConfig's monster speed multiplier.")]
        [Min(0f)] public float monsterChaseSpeed = 1.5f;
    }
}
