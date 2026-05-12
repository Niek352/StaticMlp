using System;
using System.Reflection;
using System.Threading.Tasks;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Frontier;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace StaticMlp.Composition
{
    public sealed class StaticMlpMultiplayerBootstrap : MonoBehaviour
    {
        private const string STEAM_LOBBY_HOST_KEY = "host_steam_id";
        private const string STEAM_LOBBY_VIRTUAL_PORT_KEY = "virtual_port";
        private const int MAX_SERVER_STEPS_PER_FRAME = 4;

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

        [Header("Transport")] [SerializeField] private TransportBackend transportBackend = TransportBackend.Utp;
        [SerializeField] private string connectHost = "127.0.0.1";
        [SerializeField] private ushort port = 7777;
        [SerializeField] private uint steamAppId = 480;
        [SerializeField] private int steamVirtualPort = 0;
        [SerializeField] private string connectSteamId = string.Empty;
        [SerializeField] private int steamLobbyMaxMembers = 4;
        [Header("Debug Network")] [Min(0)] [SerializeField] private int debugPingLatencyMs;
        [Header("Simulation")] [Min(1)] [SerializeField] private int serverTickRateHz = 30;

        [Header("UI")] [SerializeField] private bool showMultiplayerUi = true;
        [SerializeField] private string multiplayerUiResourcePath = "Views/MultiplayerStatusUi";

        private System.IDisposable _serverTransport;
        private System.IDisposable _clientTransport;
        private IMvcManager _clientMvcManager;
        private MultiplayerStatusUi _multiplayerStatusUi;
        private GameObject _multiplayerStatusUiInstance;
        private bool _serverStarted;
        private bool _clientStarted;
        private bool _steamInitialized;
        private string _steamInitializationFailure = string.Empty;
        private ulong _pendingSteamLobbyId;
        private ulong _pendingSteamHostSteamId;
        private Lobby? _activeSteamLobby;
        private int _appliedDebugPingLatencyMs = -1;
        private float _serverSimulationAccumulator;

        public RunMode CurrentRunMode => runMode;
        public TransportBackend CurrentTransportBackend => transportBackend;
        public bool IsServerStarted => _serverStarted;
        public bool IsClientStarted => _clientStarted;
        public bool IsRunning => _serverStarted || _clientStarted;
        public NetworkPeerId LocalPeerId => NetworkRuntime.LocalPeerId;
        public string ConnectHost => connectHost;
        public ushort Port => port;
        public bool UsesSteamTransport => transportBackend == TransportBackend.Steam;
        public bool CanInviteFriend => UsesSteamTransport && _serverStarted && _steamInitialized && _activeSteamLobby.HasValue;
        public bool CanStartClientManually => !UsesSteamTransport || _pendingSteamLobbyId != 0 ||
                                              _pendingSteamHostSteamId != 0 || ulong.TryParse(connectSteamId, out _);
        public string ConnectSteamId => connectSteamId;
        public int DebugPingLatencyMs => debugPingLatencyMs;
        public string LocalSteamName => _steamInitialized ? SteamClient.Name : "-";
        public ulong LocalSteamId => _steamInitialized ? (ulong)SteamClient.SteamId : 0;
        public ulong ActiveSteamLobbyId => _activeSteamLobby.HasValue ? (ulong)_activeSteamLobby.Value.Id : 0;
        public string SteamInitializationFailure => _steamInitializationFailure;

        private void Awake()
        {
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            if (UsesSteamTransport && EnsureSteamInitialized()) {
                SteamClient.RunCallbacks();
            }

            CreateMultiplayerUi();

            if (startOnAwake) {
                if (UsesSteamTransport && _pendingSteamLobbyId != 0) {
                    BeginJoinPendingSteamLobby();
                    return;
                }

                StartMultiplayer();
            }
        }

        private void OnDestroy()
        {
            Shutdown();
            ShutdownSteam();
            DestroyMultiplayerUi();
        }

        private void OnApplicationQuit()
        {
            Shutdown();
            ShutdownSteam();
        }

        public void StartMultiplayer()
        {
            StartMultiplayer(resolvePlayModeTags: true);
        }

        private void StartMultiplayer(bool resolvePlayModeTags)
        {
            UtpTransportContext.EnableLogs = enableLogs;
            SteamTransportContext.EnableLogs = enableLogs;

            if (resolvePlayModeTags && useMultiplayerPlayModeTags &&
                MultiplayerPlayModeTools.TryGetRunMode(out var taggedRunMode, out var tag))
            {
                runMode = taggedRunMode;
                Log($"Run mode resolved from Multiplayer Play Mode tag '{tag}'");
            }

            Log($"Starting multiplayer as {runMode} on port {port}");
            GameplayFeatureDiscovery.RegisterNetworkEvents();
            GameplayFeatureDiscovery.RegisterReplicationComponents();
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
            if (UsesSteamTransport && _pendingSteamLobbyId != 0) {
                BeginJoinPendingSteamLobby();
                return;
            }

            RestartAs(RunMode.Client);
        }

        public void Disconnect()
        {
            Shutdown();
        }

        public void SetConnectSteamId(string value)
        {
            connectSteamId = value?.Trim() ?? string.Empty;
        }

        public void ConnectSteamById()
        {
            if (!UsesSteamTransport)
                return;

            StartClient();
        }

        public void SetTransportBackend(TransportBackend backend)
        {
            if (IsRunning)
                throw new InvalidOperationException("Cannot change transport while multiplayer is running. Disconnect first.");

            transportBackend = backend;

            if (backend == TransportBackend.Steam) {
                EnsureSteamInitialized();
            } else {
                ShutdownSteam();
            }

            ApplyRuntimeDebugPingLatency(force: true);
        }

        public void SetDebugPingLatency(int pingLatencyMs)
        {
            debugPingLatencyMs = Mathf.Max(0, pingLatencyMs);
            ApplyRuntimeDebugPingLatency(force: true);
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

            if (transportBackend == TransportBackend.Steam && !EnsureSteamInitialized())
                return;

            Log("Creating server world");
            MultiplayerWorldBootstrap.CreateServer(DefaultWorldConfig(),
                NetworkEventRegistry.RegisterServerWorldTypes,
                ecsTypeAssemblies: GameplayAssemblies());
            SW.SetResource(new SimulationTime
            {
                FixedStepSeconds = 1f / Mathf.Max(1, serverTickRateHz),
            });
            SW.SetResource(Stage1SettlementSeedManifest.CreateResource());
            SW.SetResource(Stage1FrontierSeedManifest.CreateResource());
            SW.SetResource(Stage1ProgressionSeedManifest.CreateResource());
            _serverSimulationAccumulator = 0f;

            if (transportBackend == TransportBackend.Steam) {
                Log($"Starting Steam server transport as steamId={LocalSteamId} on virtual port {steamVirtualPort}");
                _serverTransport = SteamTransportStartup.StartServer(steamVirtualPort);
            } else {
                Log($"Starting server transport on 0.0.0.0:{port}");
                _serverTransport = UtpTransportStartup.StartServer(port, debugPingLatencyMs);
            }

            MultiplayerSystemBootstrap.CreateServerSystems(transportBackend);
            _serverStarted = true;
            Log("Server systems initialized");

            if (transportBackend == TransportBackend.Steam)
                BeginCreateSteamLobby();
        }

        private void StartClientSide()
        {
            if (_clientStarted)
            {
                Log("Client side is already started");
                return;
            }

            if (transportBackend == TransportBackend.Steam && !EnsureSteamInitialized())
                return;

            Log("Creating client world");
            MultiplayerWorldBootstrap.CreateClientCore(DefaultWorldConfig(),
                RegisterClientCoreGeneratedTypes, ecsTypeAssemblies: GameplayAssemblies());

            if (transportBackend == TransportBackend.Steam) {
                var hostSteamId = ResolveSteamHostId();
                Log($"Starting Steam client transport to steamId={hostSteamId} on virtual port {steamVirtualPort}");
                _clientTransport = SteamTransportStartup.StartClient((SteamId)hostSteamId, steamVirtualPort);
            } else {
                Log($"Starting client transport to {connectHost}:{port}");
                _clientTransport = UtpTransportStartup.StartClient(connectHost, port, debugPingLatencyMs);
            }

            _clientMvcManager = new MvcManager(new WindowStackManager());
            CW.SetResource(new MvcManagerResource(_clientMvcManager));
            MultiplayerSystemBootstrap.CreateClientCoreSystems(transportBackend);
            _clientStarted = true;
            Log("Client systems initialized");
        }

        private void Update()
        {
            ApplyRuntimeDebugPingLatency();

            if (_serverStarted)
                UpdateServerSimulation();

            if (_clientStarted)
                MultiplayerSystemBootstrap.UpdateClientCoreFrame();
        }

        private void UpdateServerSimulation()
        {
            var simulationTime = SW.GetResource<SimulationTime>();
            var fixedStepSeconds = simulationTime.FixedStepSeconds;
            if (fixedStepSeconds <= 0f)
                throw new InvalidOperationException("Server simulation step must be greater than zero.");

            _serverSimulationAccumulator += Mathf.Max(0f, Time.deltaTime);

            var stepCount = 0;
            while (_serverSimulationAccumulator >= fixedStepSeconds && stepCount < MAX_SERVER_STEPS_PER_FRAME)
            {
                _serverSimulationAccumulator -= fixedStepSeconds;
                simulationTime.AdvanceTick();
                MultiplayerSystemBootstrap.UpdateServerStep();
                stepCount++;
            }

            if (_serverSimulationAccumulator >= fixedStepSeconds)
            {
                _serverSimulationAccumulator = fixedStepSeconds;
                Debug.LogWarning(
                    $"[StaticMlpBootstrap] Server simulation exceeded {MAX_SERVER_STEPS_PER_FRAME} steps in a single frame. Clamping accumulator.",
                    this);
            }
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

        private static void RegisterClientCoreGeneratedTypes()
        {
            ReplicationRegistry.RegisterClientCoreGeneratedTypes();
            NetworkEventRegistry.RegisterClientWorldTypes();
            ProjectionRegistry.RegisterClientWorldTypes();
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

                _clientMvcManager?.Dispose();
                _clientMvcManager = null;

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

                if (SW.Status != WorldStatus.NotCreated && SW.HasResource<AiNavigationRuntime>())
                {
                    var navigationRuntime = SW.GetResource<AiNavigationRuntime>();
                    navigationRuntime?.Dispose();
                }

                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                _serverSimulationAccumulator = 0f;
                _serverStarted = false;
                Log("Server side stopped");
            }

            _pendingSteamHostSteamId = 0;
            LeaveActiveSteamLobby();
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

        private void ApplyRuntimeDebugPingLatency(bool force = false)
        {
            debugPingLatencyMs = Mathf.Max(0, debugPingLatencyMs);
            if (!force && _appliedDebugPingLatencyMs == debugPingLatencyMs)
                return;

            if (transportBackend == TransportBackend.Steam) {
                if (_steamInitialized)
                    SteamTransportStartup.ConfigureDebugPingLatency(debugPingLatencyMs);
            } else {
                if (_serverStarted)
                    SW.GetResource<UtpTransportContext>().ConfigureDebugPingLatency(debugPingLatencyMs);

                if (_clientStarted)
                    CW.GetResource<UtpTransportContext>().ConfigureDebugPingLatency(debugPingLatencyMs);
            }

            _appliedDebugPingLatencyMs = debugPingLatencyMs;
        }

        public void InviteFriend()
        {
            if (!CanInviteFriend)
                return;

            SteamFriends.OpenGameInviteOverlay(_activeSteamLobby.Value.Id);
            Log($"Opened Steam invite overlay for lobby {(ulong)_activeSteamLobby.Value.Id}");
        }

        private bool EnsureSteamInitialized()
        {
            if (_steamInitialized)
                return true;

            _steamInitializationFailure = string.Empty;

            try
            {
                SteamClient.Init(steamAppId, asyncCallbacks: false);
            }
            catch (Exception e)
            {
                _steamInitializationFailure = BuildSteamInitializationFailureMessage(e);
                Debug.LogWarning($"[StaticMlpBootstrap] {_steamInitializationFailure}", this);
                return false;
            }

            SteamFriends.OnGameLobbyJoinRequested += HandleSteamLobbyJoinRequested;
            _steamInitialized = true;
            _steamInitializationFailure = string.Empty;
            SteamTransportStartup.ConfigureDebugPingLatency(debugPingLatencyMs);
            Log($"Steam initialized as {SteamClient.Name} ({(ulong)SteamClient.SteamId})");
            return true;
        }

        private void ShutdownSteam()
        {
            _steamInitializationFailure = string.Empty;

            if (!_steamInitialized)
                return;

            SteamTransportStartup.ConfigureDebugPingLatency(0);
            LeaveActiveSteamLobby();
            SteamFriends.OnGameLobbyJoinRequested -= HandleSteamLobbyJoinRequested;
            SteamClient.Shutdown();
            _steamInitialized = false;
        }

        private void HandleSteamLobbyJoinRequested(Lobby lobby, SteamId friendId)
        {
            _pendingSteamLobbyId = (ulong)lobby.Id;
            Log($"Steam lobby join requested: lobby={_pendingSteamLobbyId}, inviter={(ulong)friendId}");

            if (startOnAwake || IsRunning)
                BeginJoinPendingSteamLobby();
        }

        private void BeginJoinPendingSteamLobby()
        {
            if (_pendingSteamLobbyId == 0)
                return;

            BeginJoinSteamLobby(_pendingSteamLobbyId);
        }

        private async void BeginCreateSteamLobby()
        {
            try {
                await EnsureSteamLobbyAsync();
            } catch (Exception ex) {
                Debug.LogException(ex, this);
            }
        }

        private async void BeginJoinSteamLobby(ulong lobbyIdValue)
        {
            try {
                await JoinSteamLobbyAndStartClientAsync(lobbyIdValue);
            } catch (Exception ex) {
                Debug.LogException(ex, this);
            }
        }

        private async Task EnsureSteamLobbyAsync()
        {
            if (!UsesSteamTransport || !_serverStarted || !_steamInitialized || _activeSteamLobby.HasValue)
                return;

            var lobbyResult = await SteamMatchmaking.CreateLobbyAsync(Mathf.Clamp(steamLobbyMaxMembers, 2, 250));
            if (!lobbyResult.HasValue)
                throw new InvalidOperationException("Steam lobby creation failed.");

            var lobby = lobbyResult.Value;
            lobby.SetFriendsOnly();
            lobby.SetJoinable(true);
            lobby.SetData(STEAM_LOBBY_HOST_KEY, LocalSteamId.ToString());
            lobby.SetData(STEAM_LOBBY_VIRTUAL_PORT_KEY, steamVirtualPort.ToString());

            _activeSteamLobby = lobby;
            Log($"Steam lobby created: lobbyId={(ulong)lobby.Id}");
        }

        private async Task JoinSteamLobbyAndStartClientAsync(ulong lobbyIdValue)
        {
            if (!EnsureSteamInitialized())
                throw new InvalidOperationException(_steamInitializationFailure);

            var lobbyResult = await SteamMatchmaking.JoinLobbyAsync((SteamId)lobbyIdValue);
            if (!lobbyResult.HasValue)
                throw new InvalidOperationException($"Failed to join Steam lobby {lobbyIdValue}.");

            var lobby = lobbyResult.Value;
            var hostSteamId = ResolveLobbyHostSteamId(lobby);

            if (hostSteamId == 0)
                throw new InvalidOperationException($"Steam lobby {lobbyIdValue} did not provide a valid host SteamId.");

            if (IsRunning)
                Shutdown();

            _activeSteamLobby = lobby;
            _pendingSteamLobbyId = 0;
            _pendingSteamHostSteamId = hostSteamId;
            runMode = RunMode.Client;
            StartClientSide();
        }

        private ulong ResolveSteamHostId()
        {
            if (_pendingSteamHostSteamId != 0)
                return _pendingSteamHostSteamId;

            if (runMode == RunMode.Host && _serverStarted && LocalSteamId != 0)
                return LocalSteamId;

            if (ulong.TryParse(connectSteamId, out var configuredSteamId) && configuredSteamId != 0)
                return configuredSteamId;

            throw new InvalidOperationException("Steam client start requires a pending invite or a configured connectSteamId.");
        }

        private static ulong ResolveLobbyHostSteamId(Lobby lobby)
        {
            var hostValue = lobby.GetData(STEAM_LOBBY_HOST_KEY);
            if (ulong.TryParse(hostValue, out var hostSteamId) && hostSteamId != 0)
                return hostSteamId;

            return (ulong)lobby.Owner.Id;
        }

        private void LeaveActiveSteamLobby()
        {
            if (!_activeSteamLobby.HasValue)
                return;

            _activeSteamLobby.Value.Leave();
            _activeSteamLobby = null;
        }

        private string BuildSteamInitializationFailureMessage(Exception exception)
        {
            if (exception is null)
                return "Steam initialization failed.";

            var message = exception.Message?.Trim();
            if (string.IsNullOrEmpty(message))
                return "Steam initialization failed.";

            var lowerMessage = message.ToLowerInvariant();
            if (lowerMessage.Contains("steam") &&
                (lowerMessage.Contains("not running") || lowerMessage.Contains("no connection")))
            {
                return "Steam transport selected, but Steam is not running. Start Steam and retry, or switch transport to UTP.";
            }

            return $"Steam initialization failed: {message}";
        }
    }
}
