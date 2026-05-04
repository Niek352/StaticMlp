namespace StaticMlp.Networking.Replication
{
    public static class NetworkEvents
    {
        public static readonly NetworkPeerId ServerPeer = new(0);

        public static bool ClientCanSendToServer =>
            NetworkRuntime.LocalPeerId.Value != 0
            && CW.IsWorldInitialized
            && CW.HasResource<NetOutbox>();
    }
}
