#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class LinkCoreHaptics
{
    [PostProcessBuild]
    public static void LinkFramework(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        string projectPath = PBXProject.GetPBXProjectPath(buildPath);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        string frameworkTarget = project.GetUnityFrameworkTargetGuid();
        project.AddFrameworkToProject(frameworkTarget, "CoreHaptics.framework", false);
        project.WriteToFile(projectPath);
    }
}
#endif
