using System;
using Unity.Multiplayer.PlayMode;

namespace StaticMlp.Networking.Unity {
    public static class MultiplayerPlayModeTools {
        public const string CLIENT_TAG = "IsClient";
        public const string SERVER_TAG = "IsServer";
        public const string HOST_TAG = "IsHost";

        public static bool TryGetRunMode(out StaticMlpMultiplayerBootstrap.RunMode runMode, out string tag) {
            var tags = CurrentPlayer.Tags;
            foreach (var temp in tags) {
                tag = temp;
                if (string.Equals(tag, CLIENT_TAG, StringComparison.Ordinal)) {
                    runMode = StaticMlpMultiplayerBootstrap.RunMode.Client;
                    return true;
                }

                if (string.Equals(tag, SERVER_TAG, StringComparison.Ordinal)) {
                    runMode = StaticMlpMultiplayerBootstrap.RunMode.Server;
                    return true;
                }

                if (string.Equals(tag, HOST_TAG, StringComparison.Ordinal)) {
                    runMode = StaticMlpMultiplayerBootstrap.RunMode.Host;
                    return true;
                }
            }

            tag = string.Empty;
            runMode = default;
            return false;
        }
    }
}
