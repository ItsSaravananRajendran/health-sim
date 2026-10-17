using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PlateCheck.EditorTools
{
    /// <summary>
    /// WebGL build for the CLI:
    /// unity build . --target WebGL --execute-method PlateCheck.EditorTools.BuildScript.BuildWebGL -o Builds/WebGL
    /// </summary>
    public static class BuildScript
    {
        const string DefaultOutput = "Builds/WebGL";

        [MenuItem("Plate Check/Build WebGL")]
        public static void BuildWebGL()
        {
            SceneBuilder.ConfigureProject(); // compression Disabled, Clinic in build settings
            var options = new BuildPlayerOptions
            {
                scenes = new[] { SceneBuilder.ScenePath },
                locationPathName = ArgValue("-buildOutput") ?? DefaultOutput,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[PlateCheck] WebGL build {summary.result}: {summary.outputPath}, " +
                      $"{summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors");
            if (Application.isBatchMode && summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
        }

        static string ArgValue(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
