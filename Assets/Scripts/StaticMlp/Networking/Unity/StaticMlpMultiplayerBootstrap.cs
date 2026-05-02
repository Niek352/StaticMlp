using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Networking.Transport;
using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        [FormerlySerializedAs("bindLegacyInputAxes")]
        [SerializeField] private bool bindDefaultMoveInput = true;
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
            MultiplayerWorldBootstrap.CreateServer(DefaultWorldConfig(), GameplayAssemblies());
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
            MultiplayerWorldBootstrap.CreateClientCore(DefaultWorldConfig(), GameplayAssemblies());
            MultiplayerWorldBootstrap.CreateClientUx(DefaultWorldConfig(), GameplayAssemblies());

            if (bindDefaultMoveInput) {
                NetworkInput.MoveProvider = ReadMoveInput;
                Log("Bound default move input");
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

        private Vector2 ReadMoveInput() {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null) {
                var move = Vector2.zero;

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    move.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    move.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    move.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    move.y += 1f;

                if (move != Vector2.zero)
                    return move;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(
                Input.GetAxisRaw(horizontalAxis),
                Input.GetAxisRaw(verticalAxis)
            );
#else
            return Vector2.zero;
#endif
        }

        private static WorldConfig DefaultWorldConfig() {
            return new WorldConfig {
                TrackCreated = true,
                TrackingBufferSize = 64
            };
        }

        private static Assembly[] GameplayAssemblies() {
            return new[] {
                typeof(CharacterNetState).Assembly,
                typeof(ViewTransform).Assembly
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

}
