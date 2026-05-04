using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct BuildConstructionRequestEvent : IEvent, IEventConfig<BuildConstructionRequestEvent>
    {
        public EntityGID Site;
        public float WorkAmount;

        public BuildConstructionRequestEvent(EntityGID site, float workAmount)
        {
            Site = site;
            WorkAmount = workAmount;
        }

        public EventTypeConfig<BuildConstructionRequestEvent> Config() =>
            new(guid: new Guid("50a2bb5d-99e0-4f12-abb7-d181b1133cb0"));
    }
}
