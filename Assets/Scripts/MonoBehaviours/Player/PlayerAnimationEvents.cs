using UnityEngine;

namespace Lilo.MonoBehaviours.Player
{
    /// <summary>Receives footstep AnimationEvents from the imported character clips.</summary>
    public sealed class PlayerAnimationEvents : MonoBehaviour
    {
        public void OnFootstep()
        {
            // Audio will be wired here when the player audio system is added.
        }
    }
}
