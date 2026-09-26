using UnityEngine;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>Legacy scene compatibility component; flashlight telemetry is disabled.</summary>
    public sealed class LightDebugHud : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}
