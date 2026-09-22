using UnityEngine;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>Playtest-only environment dimmer; leaves the player flashlight untouched.</summary>
    public sealed class EnvironmentLightDimmer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float intensityMultiplier = 0.2f;
        [SerializeField, Range(0f, 1f)] private float ambientIntensityMultiplier = 0.15f;
        [SerializeField] private bool disableBakedLightmaps = true;

        private void Awake()
        {
            GameObject player = GameObject.Find("PlayerCharacter");
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (player != null && light.transform.IsChildOf(player.transform))
                    continue;
                light.intensity *= intensityMultiplier;
            }

            RenderSettings.ambientIntensity *= ambientIntensityMultiplier;
            if (disableBakedLightmaps)
                LightmapSettings.lightmaps = new LightmapData[0];
        }
    }
}
