using System;
using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Networking.Transport;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

namespace StaticMlp.Networking.Unity {
    public sealed class StaticMlpMultiplayerBootstrap : MonoBehaviour {
        public enum RunMode {
            Server,
            Client,
            Host
        }

        [Header("Mode")]
        [SerializeField] private RunMode runMode = RunMode.Host;
        [SerializeField] private bool useMultiplayerPlayModeTags = true;
        [SerializeField] private bool startOnAwake = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool enableLogs = true;

        [Header("Transport")]
        [SerializeField] private string connectHost = "127.0.0.1";
        [SerializeField] private ushort port = 7777;

        [Header("Input")]
        [SerializeField] private bool bindLegacyInputAxes = true;
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";

        private UtpTransportContext _serverTransport;
        private UtpTransportContext _clientTransport;
        private bool _serverStarted;
        private bool _clientStarted;

        private void Awake() {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            if (startOnAwake)
                StartMultiplayer();
        }

        public void StartMultiplayer() {
            UtpTransportContext.EnableLogs = enableLogs;

            if (useMultiplayerPlayModeTags && MultiplayerPlayModeTools.TryGetRunMode(out var taggedRunMode, out var tag)) {
                runMode = taggedRunMode;
                Log($"Run mode resolved from Multiplayer Play Mode tag '{tag}'");
            }

            Log($"Starting multiplayer as {runMode} on port {port}");

            if (runMode == RunMode.Server || runMode == RunMode.Host)
                StartServerSide();

            if (runMode == RunMode.Client || runMode == RunMode.Host)
                StartClientSide();
        }

        private void StartServerSide() {
            if (_serverStarted) {
                Log("Server side is already started");
                return;
            }

            Log("Creating server world");
            MultiplayerWorldBootstrap.CreateServer(DefaultWorldConfig());
            Log($"Starting server transport on 0.0.0.0:{port}");
            _serverTransport = UtpTransportStartup.StartServer(port);
            MultiplayerSystemBootstrap.CreateServerSystems();
            _serverStarted = true;
            Log("Server systems initialized");
        }

        private void StartClientSide() {
            if (_clientStarted) {
                Log("Client side is already started");
                return;
            }

            Log("Creating client worlds");
            MultiplayerWorldBootstrap.CreateClientCore(DefaultWorldConfig());
            MultiplayerWorldBootstrap.CreateClientUx(DefaultWorldConfig());

            if (bindLegacyInputAxes) {
                NetworkInput.MoveProvider = ReadLegacyMoveInput;
                Log($"Bound legacy input axes: {horizontalAxis}/{verticalAxis}");
            }

            Log($"Starting client transport to {connectHost}:{port}");
            _clientTransport = UtpTransportStartup.StartClient(connectHost, port);
            MultiplayerSystemBootstrap.CreateClientCoreSystems();
            _clientStarted = true;
            Log("Client systems initialized");
        }

        private void Update() {
            if (_serverStarted)
                MultiplayerSystemBootstrap.UpdateServerFrame();

            if (_clientStarted)
                MultiplayerSystemBootstrap.UpdateClientCoreFrame();
        }

        private Vector2 ReadLegacyMoveInput() {
            return new Vector2(
                Input.GetAxisRaw(horizontalAxis),
                Input.GetAxisRaw(verticalAxis)
            );
        }

        private static WorldConfig DefaultWorldConfig() {
            return new WorldConfig {
                TrackCreated = true,
                TrackingBufferSize = 64
            };
        }

        private void OnDestroy() {
            Shutdown();
        }

        private void OnApplicationQuit() {
            Shutdown();
        }

        public void Shutdown() {
            if (_clientStarted) {
                Log("Shutting down client side");

                if (ClientCoreSys.IsInitialized)
                    ClientCoreSys.Destroy();

                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                if (UXW.Status != WorldStatus.NotCreated)
                    UXW.Destroy();

                _clientTransport?.Dispose();
                _clientTransport = null;
                _clientStarted = false;
                Log("Client side stopped");
            }

            if (_serverStarted) {
                Log("Shutting down server side");

                if (ServerSys.IsInitialized)
                    ServerSys.Destroy();

                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                _serverTransport?.Dispose();
                _serverTransport = null;
                _serverStarted = false;
                Log("Server side stopped");
            }
        }

        private void Log(string message) {
            if (enableLogs)
                Debug.Log($"[StaticMlpBootstrap] {message}", this);
        }
    }

    public static class MultiplayerPlayModeTools {
        public const string CLIENT_TAG = "IsClient";
        public const string SERVER_TAG = "IsServer";
        public const string HOST_TAG = "IsHost";

        public static bool TryGetRunMode(out StaticMlpMultiplayerBootstrap.RunMode runMode, out string tag) {
            var tags = CurrentPlayer.Tags;
            foreach (var temp in tags)
            {
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
