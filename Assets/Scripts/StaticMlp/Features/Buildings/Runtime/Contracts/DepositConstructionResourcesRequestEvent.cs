using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct DepositConstructionResourcesRequestEvent : IEvent, IRequest<DepositConstructionResourcesResultEvent>, IEventConfig<DepositConstructionResourcesRequestEvent>
    {
        public RequestId RequestId { get; set; }
        public EntityGID Site { get; set; }
        public int Wood { get; set; }
        public int Stone { get; set; }
        public int Planks { get; set; }
        public int SimpleParts { get; set; }

        public DepositConstructionResourcesRequestEvent(
            EntityGID site,
            int wood,
            int stone,
            int planks = 0,
            int simpleParts = 0)
        {
            RequestId = default;
            Site = site;
            Wood = wood;
            Stone = stone;
            Planks = planks;
            SimpleParts = simpleParts;
        }

        public EventTypeConfig<DepositConstructionResourcesRequestEvent> Config() =>
            new(guid: new Guid("5d30f087-d3b5-411c-b3be-92faf9f9551f"));
    }
}
