using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct DepositConstructionResourcesEvent : IEvent
    {
        public readonly EntityGID Site;
        public readonly ResourceAmount[] Resources;

        public DepositConstructionResourcesEvent(
            EntityGID site,
            ResourceAmount[] resources)
        {
            Site = site;
            Resources = resources;
        }
    }
}
