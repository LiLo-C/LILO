using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

/// <summary>
/// Command-line iOS build entry point.
/// Usage: unity build . --target iOS --execute-method LiloIOSBuild.Build --output-path Builds/calzy
/// </summary>
public static class LiloIOSBuild
{
    public static void Build()
    {
        string outputPath = GetArgumentValue("-buildOutput");
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new InvalidOperationException("Missing -buildOutput. Pass --output-path to the Unity CLI.");

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new InvalidOperationException("No enabled scenes found in Build Settings.");

        Directory.CreateDirectory(outputPath);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException(
                $"iOS build failed with result {report.summary.result}. See the Unity Editor log for details.");

        UnityEngine.Debug.Log($"LILO iOS Xcode project generated at {outputPath}");
    }

    private static string GetArgumentValue(string argumentName)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argumentName)
                return args[i + 1];
        }

        return string.Empty;
    }
}
