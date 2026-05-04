using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication
{
    public readonly struct NetworkEventFromClient<TEvent> : IEvent where TEvent : struct, IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly TEvent Value;

        public NetworkEventFromClient(NetworkPeerId sourcePeer, in TEvent value)
        {
            SourcePeer = sourcePeer;
            Value = value;
        }

        public bool IsOwner(SW.Entity entity)
        {
            return NetworkEntityOwnership.IsOwnedBy(entity, SourcePeer);
        }
    }
}
