using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public readonly struct NetworkEventFromServer<TEvent> : IEvent where TEvent : struct, IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly TEvent Value;

        public NetworkEventFromServer(NetworkPeerId sourcePeer, in TEvent value)
        {
            SourcePeer = sourcePeer;
            Value = value;
        }
    }
}
