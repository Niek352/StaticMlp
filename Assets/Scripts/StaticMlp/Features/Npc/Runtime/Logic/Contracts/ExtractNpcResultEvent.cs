using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Npc
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct ExtractNpcResultEvent : IEvent, IRequestResult, IEventConfig<ExtractNpcResultEvent>
    {
        public RequestId RequestId { get; set; }
        public RequestStatus Status { get; set; }
        public EntityGID Target { get; set; }
        public EntityGID RosterRecord { get; set; }
        public NpcAcquisitionResult AcquisitionResult { get; set; }

        public EventTypeConfig<ExtractNpcResultEvent> Config() =>
            new(guid: new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"));
    }
}
