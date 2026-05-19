using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Npc
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct RescueNpcRequestEvent : IEvent, IRequest<RescueNpcResultEvent>, IEventConfig<RescueNpcRequestEvent>
    {
        public RequestId RequestId { get; set; }
        public EntityGID RescueSite { get; set; }

        public RescueNpcRequestEvent(EntityGID rescueSite)
        {
            RequestId = default;
            RescueSite = rescueSite;
        }

        public EventTypeConfig<RescueNpcRequestEvent> Config() =>
            new(guid: new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"));
    }
}
