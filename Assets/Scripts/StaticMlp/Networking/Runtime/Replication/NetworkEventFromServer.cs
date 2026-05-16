using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public readonly struct NetworkEventFromServer<TEvent> : IEvent where TEvent : struct, IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly TEvent Value;
        public readonly int ReceiveOrder;

        public NetworkEventFromServer(NetworkPeerId sourcePeer, in TEvent value, int receiveOrder = 0)
        {
            SourcePeer = sourcePeer;
            Value = value;
            ReceiveOrder = receiveOrder;
        }
    }
}
