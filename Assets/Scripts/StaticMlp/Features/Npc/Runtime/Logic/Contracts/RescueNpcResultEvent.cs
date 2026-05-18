using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Npc
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct RescueNpcResultEvent : IEvent, IRequestResult, IEventConfig<RescueNpcResultEvent>
    {
        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID RescueSite { get; set; }
        public EntityGID RosterRecord { get; set; }
        public NpcAcquisitionResult AcquisitionResult { get; set; }

        public EventTypeConfig<RescueNpcResultEvent> Config() =>
            new(guid: new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"));
    }
}
