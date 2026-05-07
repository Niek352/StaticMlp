using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct BuildConstructionRequestEvent : IEvent, IRequest<BuildConstructionResultEvent>, IEventConfig<BuildConstructionRequestEvent>
    {
        public RequestId RequestId { get; set; }
        public EntityGID Site { get; set; }
        public float WorkAmount { get; set; }

        public BuildConstructionRequestEvent(EntityGID site, float workAmount)
        {
            RequestId = default;
            Site = site;
            WorkAmount = workAmount;
        }

        public EventTypeConfig<BuildConstructionRequestEvent> Config() =>
            new(guid: new Guid("50a2bb5d-99e0-4f12-abb7-d181b1133cb0"));
    }
}
