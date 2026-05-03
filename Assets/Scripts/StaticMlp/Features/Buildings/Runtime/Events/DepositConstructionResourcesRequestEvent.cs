using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public readonly struct DepositConstructionResourcesRequestEvent
    {
        public readonly EntityGID Site;
        public readonly int Wood;
        public readonly int Stone;

        public DepositConstructionResourcesRequestEvent(EntityGID site, int wood, int stone)
        {
            Site = site;
            Wood = wood;
            Stone = stone;
        }
    }
}
