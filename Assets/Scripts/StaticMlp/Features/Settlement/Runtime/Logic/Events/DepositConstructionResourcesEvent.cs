using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct DepositConstructionResourcesEvent : IEvent
    {
        public readonly EntityGID Site;
        public readonly int Wood;
        public readonly int Stone;

        public DepositConstructionResourcesEvent(EntityGID site, int wood, int stone)
        {
            Site = site;
            Wood = wood;
            Stone = stone;
        }
    }
}
