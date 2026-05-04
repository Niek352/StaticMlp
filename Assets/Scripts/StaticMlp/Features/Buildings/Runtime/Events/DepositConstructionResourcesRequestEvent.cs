using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct DepositConstructionResourcesRequestEvent : IEvent, IEventConfig<DepositConstructionResourcesRequestEvent>
    {
        public EntityGID Site;
        public int Wood;
        public int Stone;

        public DepositConstructionResourcesRequestEvent(EntityGID site, int wood, int stone)
        {
            Site = site;
            Wood = wood;
            Stone = stone;
        }

        public EventTypeConfig<DepositConstructionResourcesRequestEvent> Config() =>
            new(guid: new Guid("5d30f087-d3b5-411c-b3be-92faf9f9551f"));
    }
}
