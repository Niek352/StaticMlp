using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;

namespace StaticMlp.Networking.Replication
{
    internal struct NetworkEventPacket : IEvent, IEventConfig<NetworkEventPacket>
    {
        public NetworkPeerId SourcePeer;
        public NetworkPeerId TargetPeer;
        public ushort EventTypeId;
        public NetDelivery Delivery;
        public byte[] Payload;

        public NetworkEventPacket(
            NetworkPeerId sourcePeer,
            NetworkPeerId targetPeer,
            ushort eventTypeId,
            NetDelivery delivery,
            byte[] payload)
        {
            SourcePeer = sourcePeer;
            TargetPeer = targetPeer;
            EventTypeId = eventTypeId;
            Delivery = delivery;
            Payload = payload ?? Array.Empty<byte>();
        }

        public EventTypeConfig<NetworkEventPacket> Config() =>
            new(guid: new Guid("f4372422-950d-4411-b8e5-446fa688dc3e"));

        public void Write(ref BinaryPackWriter writer)
        {
            writer.WriteUshort(SourcePeer.Value);
            writer.WriteUshort(TargetPeer.Value);
            writer.WriteUshort(EventTypeId);
            writer.WriteByte((byte)Delivery);
            writer.WriteInt(Payload?.Length ?? 0);
            if (Payload != null)
                writer.WriteBytes(Payload);
        }

        public void Read(ref BinaryPackReader reader, byte version)
        {
            SourcePeer = new NetworkPeerId(reader.ReadUshort());
            TargetPeer = new NetworkPeerId(reader.ReadUshort());
            EventTypeId = reader.ReadUshort();
            Delivery = (NetDelivery)reader.ReadByte();

            var payloadLength = reader.ReadInt();
            Payload = payloadLength > 0
                ? reader.ReadBytesAsSpan((uint)payloadLength).ToArray()
                : Array.Empty<byte>();
        }
    }
}
