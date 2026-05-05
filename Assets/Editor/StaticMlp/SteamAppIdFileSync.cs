using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StaticMlp.Editor {
    [InitializeOnLoad]
    public sealed class SteamAppIdFileSync : IPostprocessBuildWithReport {
        private const string STEAM_APP_ID = "480";
        private const string FILE_NAME = "steam_appid.txt";

        static SteamAppIdFileSync() {
            EnsureEditorSteamAppIdFile();
        }

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report) {
            EnsureBuildSteamAppIdFile(report.summary.outputPath);
        }

        private static void EnsureEditorSteamAppIdFile() {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
                return;

            WriteSteamAppId(Path.Combine(projectRoot, FILE_NAME));
        }

        private static void EnsureBuildSteamAppIdFile(string outputPath) {
            if (string.IsNullOrEmpty(outputPath))
                return;

            var buildDirectory = Directory.Exists(outputPath)
                ? outputPath
                : Path.GetDirectoryName(outputPath);

            if (string.IsNullOrEmpty(buildDirectory))
                return;

            WriteSteamAppId(Path.Combine(buildDirectory, FILE_NAME));
        }

        private static void WriteSteamAppId(string path) {
            if (File.Exists(path) && File.ReadAllText(path).Trim() == STEAM_APP_ID)
                return;

            File.WriteAllText(path, STEAM_APP_ID + "\n");
            Debug.Log($"[SteamAppIdFileSync] Wrote {FILE_NAME} to {path}");
        }
    }
}
