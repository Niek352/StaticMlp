using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct DepositConstructionResourcesEvent : IEvent
    {
        public readonly EntityGID Site;
        public readonly int Wood;
        public readonly int Stone;
        public readonly int Planks;
        public readonly int SimpleParts;

        public DepositConstructionResourcesEvent(
            EntityGID site,
            int wood,
            int stone,
            int planks = 0,
            int simpleParts = 0)
        {
            Site = site;
            Wood = wood;
            Stone = stone;
            Planks = planks;
            SimpleParts = simpleParts;
        }
    }
}
