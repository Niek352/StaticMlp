using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Networking 
{
    public readonly struct NetworkEventPacket : IEvent
    {
        public readonly NetworkPeerId SourcePeer;
        public readonly NetworkPeerId TargetPeer;
        public readonly ushort EventTypeId;
        public readonly NetDelivery Delivery;
        public readonly int EstimatedPayloadSize;
        public readonly int ReceiveOrder;
        internal readonly INetworkEventPayload Payload;

        internal NetworkEventPacket(
            NetworkPeerId sourcePeer,
            NetworkPeerId targetPeer,
            ushort eventTypeId,
            NetDelivery delivery,
            int estimatedPayloadSize,
            INetworkEventPayload payload,
            int receiveOrder = 0) {
            SourcePeer = sourcePeer;
            TargetPeer = targetPeer;
            EventTypeId = eventTypeId;
            Delivery = delivery;
            EstimatedPayloadSize = estimatedPayloadSize;
            Payload = payload;
            ReceiveOrder = receiveOrder;
        }

        internal NetworkEventPacket WithReceiveOrder(int receiveOrder) =>
            new(SourcePeer, TargetPeer, EventTypeId, Delivery, EstimatedPayloadSize, Payload, receiveOrder);
    }
}
