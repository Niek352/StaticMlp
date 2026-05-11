using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking {
    public abstract class CW : World<ClientCoreWT> {
        public static bool SendToServerEvent<TEvent>(in TEvent evt) where TEvent : struct, IEvent {
            return NetworkEventRegistry.TryCreatePacket(NetworkEvents.ServerPeer, in evt, out var packet)
                   && SendEvent(packet);
        }
    }
}
