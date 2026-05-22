using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement.Workers
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct SetBuildingWorkerAssignmentResultEvent : IEvent, IRequestResult,
        IEventConfig<SetBuildingWorkerAssignmentResultEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40174;

        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Worker { get; set; }
        public EntityGID Building { get; set; }
        public ushort AnchorId { get; set; }
        public byte SlotIndex { get; set; }
        public SettlementWorkerAssignmentStatus AssignmentStatus { get; set; }

        public EventTypeConfig<SetBuildingWorkerAssignmentResultEvent> Config() =>
            new(guid: new Guid("f591b513-010f-479b-bfc7-9b91423e0801"));
    }
}
