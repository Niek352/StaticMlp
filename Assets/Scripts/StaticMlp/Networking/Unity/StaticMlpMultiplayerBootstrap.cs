using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;
using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StaticMlp.Networking.Unity
{
    public sealed class StaticMlpMultiplayerBootstrap : MonoBehaviour
    {
        public enum RunMode
        {
            Server,
            Client,
            Host
        }

        [Header("Mode")] [SerializeField] private RunMode runMode = RunMode.Host;
        [SerializeField] private bool useMultiplayerPlayModeTags = true;
        [SerializeField] private bool startOnAwake = true;
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool enableLogs = true;

        [Header("Transport")] [SerializeField] private string connectHost = "127.0.0.1";
        [SerializeField] private ushort port = 7777;

        [Header("UI")] [SerializeField] private bool showMultiplayerUi = true;
        [SerializeField] private string multiplayerUiResourcePath = "Views/MultiplayerStatusUi";

        [Header("Input")] [FormerlySerializedAs("bindLegacyInputAxes")] [SerializeField]
        private bool bindDefaultMoveInput = true;

        private UtpTransportContext _serverTransport;
        private UtpTransportContext _clientTransport;
        private MultiplayerStatusUi _multiplayerStatusUi;
        private GameObject _multiplayerStatusUiInstance;
        private bool _serverStarted;
        private bool _clientStarted;

        public RunMode CurrentRunMode => runMode;
        public bool IsServerStarted => _serverStarted;
        public bool IsClientStarted => _clientStarted;
        public bool IsRunning => _serverStarted || _clientStarted;
        public NetworkPeerId LocalPeerId => NetworkRuntime.LocalPeerId;
        public string ConnectHost => connectHost;
        public ushort Port => port;

        private void Awake()
        {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            CreateMultiplayerUi();

            if (startOnAwake)
                StartMultiplayer();
        }

        private void OnDestroy()
        {
            Shutdown();
            DestroyMultiplayerUi();
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }

        public void StartMultiplayer()
        {
            StartMultiplayer(resolvePlayModeTags: true);
        }

        private void StartMultiplayer(bool resolvePlayModeTags)
        {
            UtpTransportContext.EnableLogs = enableLogs;

            if (resolvePlayModeTags && useMultiplayerPlayModeTags &&
                MultiplayerPlayModeTools.TryGetRunMode(out var taggedRunMode, out var tag))
            {
                runMode = taggedRunMode;
                Log($"Run mode resolved from Multiplayer Play Mode tag '{tag}'");
            }

            Log($"Starting multiplayer as {runMode} on port {port}");
            GameplayFeatureDiscovery.RegisterPrefabs();

            if (runMode is RunMode.Server or RunMode.Host)
                StartServerSide();

            if (runMode is RunMode.Client or RunMode.Host)
                StartClientSide();
        }

        public void StartHost()
        {
            RestartAs(RunMode.Host);
        }

        public void StartServer()
        {
            RestartAs(RunMode.Server);
        }

        public void StartClient()
        {
            RestartAs(RunMode.Client);
        }

        public void Disconnect()
        {
            Shutdown();
        }

        private void RestartAs(RunMode nextRunMode)
        {
            if (IsRunning)
                Shutdown();

            runMode = nextRunMode;
            StartMultiplayer(resolvePlayModeTags: false);
        }

        private void StartServerSide()
        {
            if (_serverStarted)
            {
                Log("Server side is already started");
                return;
            }

            Log("Creating server world");
            MultiplayerWorldBootstrap.CreateServer(DefaultWorldConfig(), registerGeneratedTypes: null,
                ecsTypeAssemblies: GameplayAssemblies());
            Log($"Starting server transport on 0.0.0.0:{port}");
            _serverTransport = UtpTransportStartup.StartServer(port);
            MultiplayerSystemBootstrap.CreateServerSystems();
            _serverStarted = true;
            Log("Server systems initialized");
        }

        private void StartClientSide()
        {
            if (_clientStarted)
            {
                Log("Client side is already started");
                return;
            }

            Log("Creating client world");
            MultiplayerWorldBootstrap.CreateClientCore(DefaultWorldConfig(),
                ReplicationRegistry.RegisterClientCoreGeneratedTypes, ecsTypeAssemblies: GameplayAssemblies());

            if (bindDefaultMoveInput)
            {
                NetworkInput.MoveProvider = ReadMoveInput;
                NetworkInput.SpawnCubeWasPressedProvider = ReadSpawnCubeInput;
                Log("Bound default move input");
            }

            Log($"Starting client transport to {connectHost}:{port}");
            _clientTransport = UtpTransportStartup.StartClient(connectHost, port);
            MultiplayerSystemBootstrap.CreateClientCoreSystems();
            _clientStarted = true;
            Log("Client systems initialized");
        }

        private void Update()
        {
            if (_serverStarted)
                MultiplayerSystemBootstrap.UpdateServerFrame();

            if (_clientStarted)
                MultiplayerSystemBootstrap.UpdateClientCoreFrame();
        }

        private static Vector2 ReadMoveInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return Vector2.zero;
            var move = Vector2.zero;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                move.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                move.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                move.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                move.y += 1f;

            return move != Vector2.zero ? move : Vector2.zero;
        }

        private static bool ReadSpawnCubeInput()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.cKey.wasPressedThisFrame;
        }

        private static WorldConfig DefaultWorldConfig()
        {
            return new WorldConfig
            {
                TrackCreated = true,
                TrackingBufferSize = 64
            };
        }

        private static Assembly[] GameplayAssemblies()
        {
            return GameplayFeatureDiscovery.GetEcsTypeAssemblies();
        }

        public void Shutdown()
        {
            if (_clientStarted)
            {
                Log("Shutting down client side");

                _clientTransport?.Dispose();
                _clientTransport = null;

                if (ClientCoreSys.IsInitialized)
                    ClientCoreSys.Destroy();

                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                _clientStarted = false;
                Log("Client side stopped");
            }

            if (_serverStarted)
            {
                Log("Shutting down server side");

                _serverTransport?.Dispose();
                _serverTransport = null;

                if (ServerSys.IsInitialized)
                    ServerSys.Destroy();

                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                _serverStarted = false;
                Log("Server side stopped");
            }
        }

        private void CreateMultiplayerUi()
        {
            if (!showMultiplayerUi)
                return;

            var prefab = Resources.Load<GameObject>(multiplayerUiResourcePath);
            if (prefab == null)
                throw new MissingReferenceException($"Multiplayer UI prefab resource not found: {multiplayerUiResourcePath}");

            _multiplayerStatusUiInstance = Instantiate(prefab);
            _multiplayerStatusUi = _multiplayerStatusUiInstance.GetComponent<MultiplayerStatusUi>();
            if (_multiplayerStatusUi == null)
                throw new MissingComponentException($"Multiplayer UI prefab must have {nameof(MultiplayerStatusUi)} on its root.");

            _multiplayerStatusUi.Bind(this);

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(_multiplayerStatusUiInstance);
        }

        private void DestroyMultiplayerUi()
        {
            if (_multiplayerStatusUiInstance == null)
                return;

            Destroy(_multiplayerStatusUiInstance);
            _multiplayerStatusUiInstance = null;
            _multiplayerStatusUi = null;
        }

        private void Log(string message)
        {
            if (enableLogs)
                Debug.Log($"[StaticMlpBootstrap] {message}", this);
        }
    }
}
