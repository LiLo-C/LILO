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
        [Tooltip("Monster patrol speed as a fraction of Eddie's walk speed.")]
        [Min(0f)] public float monsterPatrolSpeed = 0.5f;
        [Tooltip("Monster chase speed as a fraction of Eddie's sprint speed.")]
        [Min(0f)] public float monsterChaseSpeed = 1.5f;
    }
}
