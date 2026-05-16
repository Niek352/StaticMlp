using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication
{
    public readonly struct NetworkEventFromClient<TEvent> : IEvent where TEvent : struct, IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly TEvent Value;
        public readonly int ReceiveOrder;

        public NetworkEventFromClient(NetworkPeerId sourcePeer, in TEvent value, int receiveOrder = 0)
        {
            SourcePeer = sourcePeer;
            Value = value;
            ReceiveOrder = receiveOrder;
        }

        public bool IsOwner(SW.Entity entity)
        {
            return NetworkEntityOwnership.IsOwnedBy(entity, SourcePeer);
        }
    }
}
