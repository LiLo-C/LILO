using UnityEngine;
using UnityEngine.Rendering;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>
    /// Removes global/environment lighting so the player flashlight defines visibility.
    /// Lights parented to the player are deliberately preserved.
    /// </summary>
    public sealed class EnvironmentLightDimmer : MonoBehaviour
    {
        [SerializeField] private bool disableEnvironmentLights = true;
        [SerializeField] private bool disableBakedLightmaps = true;

        private void Start()
        {
            GameObject player = GameObject.Find("PlayerCharacter");
            foreach (Light light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (player != null && light.transform.IsChildOf(player.transform))
                    continue;

                if (disableEnvironmentLights)
                    light.enabled = false;
            }

            // Do not multiply an inherited scene value: it still leaves an even wash of
            // light across every room. Set the global contribution explicitly to black.
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientSkyColor = Color.black;
            RenderSettings.ambientEquatorColor = Color.black;
            RenderSettings.ambientGroundColor = Color.black;
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.reflectionIntensity = 0f;
            RenderSettings.skybox = null;

            if (disableBakedLightmaps)
                LightmapSettings.lightmaps = new LightmapData[0];

            DynamicGI.UpdateEnvironment();
        }
    }
}
