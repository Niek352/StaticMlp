using TMPro;
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
            if (disconnectButton == null)
                throw new MissingReferenceException($"{nameof(MultiplayerStatusUi)} requires {nameof(disconnectButton)}.");
        }

        private void OnEnable()
        {
            hostButton.onClick.AddListener(StartHost);
            serverButton.onClick.AddListener(StartServer);
            clientButton.onClick.AddListener(StartClient);
            disconnectButton.onClick.AddListener(Disconnect);
        }

        private void OnDisable()
        {
            hostButton.onClick.RemoveListener(StartHost);
            serverButton.onClick.RemoveListener(StartServer);
            clientButton.onClick.RemoveListener(StartClient);
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

        private void Disconnect()
        {
            _bootstrap.Disconnect();
        }

        private void Refresh()
        {
            if (_bootstrap == null)
                return;

            var mode = _bootstrap.IsRunning ? _bootstrap.CurrentRunMode.ToString() : "Disconnected";
            var server = _bootstrap.IsServerStarted ? "on" : "off";
            var client = _bootstrap.IsClientStarted ? "on" : "off";
            var peer = _bootstrap.LocalPeerId.Value == 0 ? "-" : _bootstrap.LocalPeerId.Value.ToString();

            statusText.text = $"Multiplayer: {mode}\nServer: {server}  Client: {client}\nPeer: {peer}  Port: {_bootstrap.Port}";
            disconnectButton.interactable = _bootstrap.IsRunning;
        }
    }
}
