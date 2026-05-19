using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Npc
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct ExtractNpcRequestEvent : IEvent, IRequest<ExtractNpcResultEvent>, IEventConfig<ExtractNpcRequestEvent>
    {
        public RequestId RequestId { get; set; }
        public EntityGID Target { get; set; }

        public ExtractNpcRequestEvent(EntityGID target)
        {
            RequestId = default;
            Target = target;
        }

        public EventTypeConfig<ExtractNpcRequestEvent> Config() =>
            new(guid: new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"));
    }
}
