using UnityEngine;

namespace Lilo.MonoBehaviours.Audio
{
    /// <summary>
    /// Legacy compatibility component. Player footsteps now come from the
    /// Starter Assets animation events; this component intentionally stays silent.
    /// </summary>
    public sealed class FootstepPlayer : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}
