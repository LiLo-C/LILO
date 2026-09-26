using UnityEngine;

namespace Lilo.MonoBehaviours
{
    /// <summary>Applies the game's frame-rate cap before any scene starts.</summary>
    internal static class FrameRateBootstrap
    {
        private const int TargetFramesPerSecond = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyFrameRateLimit()
        {
            // targetFrameRate is ignored on desktop when VSync is enabled.
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFramesPerSecond;
        }
    }
}
