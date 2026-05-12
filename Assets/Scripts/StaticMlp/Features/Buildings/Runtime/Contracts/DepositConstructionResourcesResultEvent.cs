using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct DepositConstructionResourcesResultEvent : IEvent, IRequestResult, IEventConfig<DepositConstructionResourcesResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 32404;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Site;
        public int AcceptedWood;
        public int AcceptedStone;

        public EventTypeConfig<DepositConstructionResourcesResultEvent> Config() =>
            new(guid: new Guid("f5910f59-cf18-4370-bfad-7251af0cb3d4"));

        public static byte[] Write(in DepositConstructionResourcesResultEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(32);
            writer.WriteUint(evt.RequestId.Value);
            writer.WriteByte((byte)evt.Status);
            writer.WriteUlong(evt.Site.Raw);
            writer.WriteInt(evt.AcceptedWood);
            writer.WriteInt(evt.AcceptedStone);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out DepositConstructionResourcesResultEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new DepositConstructionResourcesResultEvent
                {
                    RequestId = new RequestId(reader.ReadUint()),
                    Status = (RequestStatus)reader.ReadByte(),
                    Site = new EntityGID(reader.ReadUlong()),
                    AcceptedWood = reader.ReadInt(),
                    AcceptedStone = reader.ReadInt()
                };
                return true;
            }
            catch
            {
                evt = default;
                return false;
            }
        }
    }
}
