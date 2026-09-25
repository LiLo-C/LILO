using UnityEngine;
using UnityEngine.Rendering;

namespace Lilo.MonoBehaviours.Flashlight
{
    /// <summary>
    /// Removes every scene light except the battery-controlled player lamp, so no static
    /// environment source can remain lit beside it.
    /// </summary>
    public sealed class EnvironmentLightDimmer : MonoBehaviour
    {
        [SerializeField] private bool disableBakedLightmaps = true;
        [SerializeField] private Light readabilityFillLight;

        private void Start()
        {
            if (readabilityFillLight == null)
                readabilityFillLight = GameObject.Find("ReadabilityFillLight")?.GetComponent<Light>();

            GameObject player = GameObject.Find("PlayerCharacter");
            Light playerLamp = player != null
                ? player.GetComponent<LightingRig>()?.PlayerFlashlight
                : null;
            if (playerLamp == null && player != null)
                playerLamp = player.transform.Find("PlayerFlashlight")?.GetComponent<Light>();

            foreach (Light light in FindObjectsByType<Light>())
            {
                bool isPlayerLamp = light == playerLamp
                    || (player != null
                        && light.gameObject.name == "PlayerFlashlight"
                        && light.transform.IsChildOf(player.transform));
                if (isPlayerLamp)
                {
                    light.enabled = true;
                    continue;
                }

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
