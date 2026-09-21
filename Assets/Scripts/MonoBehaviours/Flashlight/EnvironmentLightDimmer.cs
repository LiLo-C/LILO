using UnityEngine;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>Playtest-only environment dimmer; leaves the player flashlight untouched.</summary>
    public sealed class EnvironmentLightDimmer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float intensityMultiplier = 0.8f;

        private void Awake()
        {
            GameObject player = GameObject.Find("PlayerCharacter");
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (player != null && light.transform.IsChildOf(player.transform))
                    continue;
                light.intensity *= intensityMultiplier;
            }
        }
    }
}
