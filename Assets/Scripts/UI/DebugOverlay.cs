using UnityEngine;

namespace Lilo.UI
{
    /// <summary>Legacy scene compatibility component; gameplay debug UI is disabled.</summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        // Kept for compatibility with old serialized button callbacks.
        public void Toggle()
        {
            gameObject.SetActive(false);
        }
    }
}
