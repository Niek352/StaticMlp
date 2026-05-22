using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement.Workers
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct SetBuildingWorkerAssignmentRequestEvent : IEvent,
        IRequest<SetBuildingWorkerAssignmentResultEvent>, IEventConfig<SetBuildingWorkerAssignmentRequestEvent>
    {
        public const ushort NETWORK_EVENT_ID = 40173;

        public RequestId RequestId { get; set; }
        public EntityGID Worker { get; set; }
        public EntityGID Building { get; set; }
        public byte SlotIndex { get; set; }
        public bool Assigned { get; set; }

        public SetBuildingWorkerAssignmentRequestEvent(
            EntityGID worker,
            EntityGID building,
            byte slotIndex,
            bool assigned)
        {
            RequestId = default;
            Worker = worker;
            Building = building;
            SlotIndex = slotIndex;
            Assigned = assigned;
        }

        public EventTypeConfig<SetBuildingWorkerAssignmentRequestEvent> Config() =>
            new(guid: new Guid("12082f54-6fde-4583-b6c6-61879520d99c"));
    }
}
