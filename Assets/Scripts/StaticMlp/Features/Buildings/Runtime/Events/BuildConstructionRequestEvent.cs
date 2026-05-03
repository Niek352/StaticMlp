using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public readonly struct BuildConstructionRequestEvent
    {
        public readonly EntityGID Site;
        public readonly float WorkAmount;

        public BuildConstructionRequestEvent(EntityGID site, float workAmount)
        {
            Site = site;
            WorkAmount = workAmount;
        }
    }
}
