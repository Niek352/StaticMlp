using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement.Workers
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct SetSettlementWorkerAssignmentRequestEvent : IEvent,
        IRequest<SetSettlementWorkerAssignmentResultEvent>, IEventConfig<SetSettlementWorkerAssignmentRequestEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40171;

        public RequestId RequestId { get; set; }
        public EntityGID Worker { get; set; }
        public ushort AnchorId { get; set; }
        public bool Assigned { get; set; }

        public SetSettlementWorkerAssignmentRequestEvent(EntityGID worker, SettlementAnchorId anchor, bool assigned)
        {
            RequestId = default;
            Worker = worker;
            AnchorId = anchor.Value;
            Assigned = assigned;
        }

        public EventTypeConfig<SetSettlementWorkerAssignmentRequestEvent> Config() =>
            new(guid: new Guid("be4c4d8c-c0f4-4a34-a072-a4a274a01bb8"));

        public static byte[] Write(in SetSettlementWorkerAssignmentRequestEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(24);
            writer.WriteUint(evt.RequestId.Value);
            writer.WriteUlong(evt.Worker.Raw);
            writer.WriteUshort(evt.AnchorId);
            writer.WriteByte(evt.Assigned ? (byte)1 : (byte)0);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out SetSettlementWorkerAssignmentRequestEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new SetSettlementWorkerAssignmentRequestEvent
                {
                    RequestId = new RequestId(reader.ReadUint()),
                    Worker = new EntityGID(reader.ReadUlong()),
                    AnchorId = reader.ReadUshort(),
                    Assigned = reader.ReadByte() != 0
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
