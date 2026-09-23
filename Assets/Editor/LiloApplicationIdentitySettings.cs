using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Lilo.Editor
{
    /// <summary>
    /// Keeps the shared Player Settings identity aligned with the LILO iOS app,
    /// including when a build profile or a local editor setting changed it.
    /// </summary>
    [InitializeOnLoad]
    public sealed class LiloApplicationIdentitySettings : IPreprocessBuildWithReport
    {
        public const string CompanyName = "EGS";
        public const string ProductName = "LILO: Lights In Lights Out";
        public const string IOSBundleIdentifier = "com.LILO.EGS";

        public int callbackOrder => -1000;

        static LiloApplicationIdentitySettings()
        {
            EditorApplication.delayCall += Apply;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Apply();
        }

        private static void Apply()
        {
            bool changed = false;
            if (PlayerSettings.companyName != CompanyName)
            {
                PlayerSettings.companyName = CompanyName;
                changed = true;
            }

            if (PlayerSettings.productName != ProductName)
            {
                PlayerSettings.productName = ProductName;
                changed = true;
            }

            if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS) != IOSBundleIdentifier)
            {
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, IOSBundleIdentifier);
                changed = true;
            }

            if (changed)
                Debug.Log($"[LILO] Applied Player Settings identity: {CompanyName}, {ProductName}, {IOSBundleIdentifier}.");
        }
    }
}
