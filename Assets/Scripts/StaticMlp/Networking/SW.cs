using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking {
    public abstract class SW : World<ServerWT> {
        public static void SendToPeerEvent<TEvent>(NetworkPeerId peer, in TEvent evt) where TEvent : struct, IEvent
        {
            NetworkEventRegistry.CreatePacket(peer, in evt, out var packet);
            TryEnqueueNetworkEvent(in packet);
        }

        private static bool TryEnqueueNetworkEvent(in NetworkEventPacket packet) {
            GetResource<NetOutbox>().EnqueueNetworkEvent(in packet);
            return true;
        }
    }
}
