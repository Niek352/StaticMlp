using TMPro;
using StaticMlp.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Composition
{
    public sealed class MultiplayerStatusUi : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button serverButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button utpTransportButton;
        [SerializeField] private Button steamTransportButton;
        [SerializeField] private Button inviteButton;
        [SerializeField] private Button copySteamIdButton;
        [SerializeField] private Button pasteSteamIdButton;
        [SerializeField] private Button connectSteamButton;
        [SerializeField] private TMP_InputField steamIdInput;
        [SerializeField] private Button disconnectButton;

        private StaticMlpMultiplayerBootstrap _bootstrap;

        public void Bind(StaticMlpMultiplayerBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            Refresh();
        }

        private void Awake()
        {
            if (statusText == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(statusText)}.");
            if (hostButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(hostButton)}.");
            if (serverButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(serverButton)}.");
            if (clientButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(clientButton)}.");
            if (utpTransportButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(utpTransportButton)}.");
            if (steamTransportButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(steamTransportButton)}.");
            if (inviteButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(inviteButton)}.");
            if (copySteamIdButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(copySteamIdButton)}.");
            if (pasteSteamIdButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(pasteSteamIdButton)}.");
            if (connectSteamButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(connectSteamButton)}.");
            if (steamIdInput == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(steamIdInput)}.");
            if (disconnectButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(disconnectButton)}.");
        }

        private void OnEnable()
        {
            hostButton.onClick.AddListener(StartHost);
            serverButton.onClick.AddListener(StartServer);
            clientButton.onClick.AddListener(StartClient);
            utpTransportButton.onClick.AddListener(UseUtpTransport);
            steamTransportButton.onClick.AddListener(UseSteamTransport);
            inviteButton.onClick.AddListener(InviteFriend);
            copySteamIdButton.onClick.AddListener(CopySteamId);
            pasteSteamIdButton.onClick.AddListener(PasteSteamId);
            connectSteamButton.onClick.AddListener(ConnectSteam);
            steamIdInput.onValueChanged.AddListener(OnSteamIdChanged);
            disconnectButton.onClick.AddListener(Disconnect);
        }

        private void OnDisable()
        {
            hostButton.onClick.RemoveListener(StartHost);
            serverButton.onClick.RemoveListener(StartServer);
            clientButton.onClick.RemoveListener(StartClient);
            utpTransportButton.onClick.RemoveListener(UseUtpTransport);
            steamTransportButton.onClick.RemoveListener(UseSteamTransport);
            inviteButton.onClick.RemoveListener(InviteFriend);
            copySteamIdButton.onClick.RemoveListener(CopySteamId);
            pasteSteamIdButton.onClick.RemoveListener(PasteSteamId);
            connectSteamButton.onClick.RemoveListener(ConnectSteam);
            steamIdInput.onValueChanged.RemoveListener(OnSteamIdChanged);
            disconnectButton.onClick.RemoveListener(Disconnect);
        }

        private void Update()
        {
            Refresh();
        }

        private void StartHost()
        {
            _bootstrap.StartHost();
        }

        private void StartServer()
        {
            _bootstrap.StartServer();
        }

        private void StartClient()
        {
            _bootstrap.StartClient();
        }

        private void UseUtpTransport()
        {
            _bootstrap.SetTransportBackend(TransportBackend.Utp);
        }

        private void UseSteamTransport()
        {
            _bootstrap.SetTransportBackend(TransportBackend.Steam);
        }

        private void Disconnect()
        {
            _bootstrap.Disconnect();
        }

        private void InviteFriend()
        {
            _bootstrap.InviteFriend();
        }

        private void CopySteamId()
        {
            var steamId = _bootstrap.LocalSteamId;
            if (steamId == 0)
                return;

            GUIUtility.systemCopyBuffer = steamId.ToString();
        }

        private void PasteSteamId()
        {
            steamIdInput.text = GUIUtility.systemCopyBuffer?.Trim() ?? string.Empty;
        }

        private void ConnectSteam()
        {
            _bootstrap.ConnectSteamById();
        }

        private void OnSteamIdChanged(string value)
        {
            _bootstrap.SetConnectSteamId(value);
        }

        private void Refresh()
        {
            if (_bootstrap == null)
                return;

            var mode = _bootstrap.IsRunning ? _bootstrap.CurrentRunMode.ToString() : "Disconnected";
            var server = _bootstrap.IsServerStarted ? "on" : "off";
            var client = _bootstrap.IsClientStarted ? "on" : "off";
            var peer = _bootstrap.LocalPeerId.Value == 0 ? "-" : _bootstrap.LocalPeerId.Value.ToString();
            var transport = _bootstrap.CurrentTransportBackend.ToString();
            var debugPing = _bootstrap.DebugPingLatencyMs <= 0 ? "off" : $"{_bootstrap.DebugPingLatencyMs} ms RTT";
            var status = $"Transport: {transport}\nMultiplayer: {mode}\nServer: {server}  Client: {client}\nPeer: {peer}\nDebug ping: {debugPing}";

            if (_bootstrap.UsesSteamTransport) {
                var steamId = _bootstrap.LocalSteamId == 0 ? "-" : _bootstrap.LocalSteamId.ToString();
                var lobbyId = _bootstrap.ActiveSteamLobbyId == 0 ? "-" : _bootstrap.ActiveSteamLobbyId.ToString();
                status += $"\nSteam: {_bootstrap.LocalSteamName} ({steamId})\nLobby: {lobbyId}";

                if (!string.IsNullOrEmpty(_bootstrap.SteamInitializationFailure))
                    status += $"\nSteam status: {_bootstrap.SteamInitializationFailure}";
            } else {
                status += $"  Port: {_bootstrap.Port}";
            }

            statusText.text = status;
            if (steamIdInput.text != _bootstrap.ConnectSteamId)
                steamIdInput.SetTextWithoutNotify(_bootstrap.ConnectSteamId);

            clientButton.interactable = _bootstrap.CanStartClientManually;
            var canChangeTransport = !_bootstrap.IsRunning;
            utpTransportButton.interactable = canChangeTransport;
            steamTransportButton.interactable = canChangeTransport;
            inviteButton.interactable = _bootstrap.CanInviteFriend;
            copySteamIdButton.interactable = _bootstrap.UsesSteamTransport && _bootstrap.LocalSteamId != 0;
            pasteSteamIdButton.interactable = _bootstrap.UsesSteamTransport;
            connectSteamButton.interactable = _bootstrap.UsesSteamTransport && _bootstrap.CanStartClientManually;
            steamIdInput.interactable = _bootstrap.UsesSteamTransport;
            disconnectButton.interactable = _bootstrap.IsRunning;
        }
    }
}
