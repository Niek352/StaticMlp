using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement.Workers
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct SetSettlementWorkerAssignmentResultEvent : IEvent, IRequestResult,
        IEventConfig<SetSettlementWorkerAssignmentResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40172;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Worker { get; set; }
        public ushort AnchorId { get; set; }
        public SettlementWorkerAssignmentStatus AssignmentStatus { get; set; }

        public EventTypeConfig<SetSettlementWorkerAssignmentResultEvent> Config() =>
            new(guid: new Guid("7e6fce4d-cf07-4db8-b036-c682ef468c3b"));

        public static byte[] Write(in SetSettlementWorkerAssignmentResultEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(24);
            writer.WriteUint(evt.RequestId.Value);
            writer.WriteByte((byte)evt.Status);
            writer.WriteUlong(evt.Worker.Raw);
            writer.WriteUshort(evt.AnchorId);
            writer.WriteByte((byte)evt.AssignmentStatus);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out SetSettlementWorkerAssignmentResultEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new SetSettlementWorkerAssignmentResultEvent
                {
                    RequestId = new RequestId(reader.ReadUint()),
                    Status = (RequestStatus)reader.ReadByte(),
                    Worker = new EntityGID(reader.ReadUlong()),
                    AnchorId = reader.ReadUshort(),
                    AssignmentStatus = (SettlementWorkerAssignmentStatus)reader.ReadByte()
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
