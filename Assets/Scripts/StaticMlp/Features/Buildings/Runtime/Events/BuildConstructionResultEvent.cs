using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct BuildConstructionResultEvent : IEvent, IRequestResult, IEventConfig<BuildConstructionResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 31659;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Site { get; set; }
        public float AcceptedWork { get; set; }
        public float BuildWorkDone { get; set; }

        public EventTypeConfig<BuildConstructionResultEvent> Config() =>
            new(guid: new Guid("998f596e-433b-4b10-b557-03b292444df2"));

        public static byte[] Write(in BuildConstructionResultEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(28);
            writer.WriteUint(evt.RequestId.Value);
            writer.WriteByte((byte)evt.Status);
            writer.WriteUlong(evt.Site.Raw);
            writer.WriteFloat(evt.AcceptedWork);
            writer.WriteFloat(evt.BuildWorkDone);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out BuildConstructionResultEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new BuildConstructionResultEvent
                {
                    RequestId = new RequestId(reader.ReadUint()),
                    Status = (RequestStatus)reader.ReadByte(),
                    Site = new EntityGID(reader.ReadUlong()),
                    AcceptedWork = reader.ReadFloat(),
                    BuildWorkDone = reader.ReadFloat()
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
