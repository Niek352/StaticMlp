using System;

namespace StaticMlp.Networking.Transport {
    public interface INetworkTransport {
        bool IsServer { get; }
        void Send(NetworkPeerId peer, ReadOnlySpan<byte> payload, NetDelivery delivery);
        bool TryReceive(out NetworkPeerId peer, out ReadOnlySpan<byte> payload);
        void Poll();
    }
}
