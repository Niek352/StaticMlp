using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct ApplyConstructionBuildWorkEvent : IEvent
    {
        public readonly EntityGID Site;
        public readonly float WorkAmount;
        public readonly float MaxWork;

        public ApplyConstructionBuildWorkEvent(EntityGID site, float workAmount, float maxWork)
        {
            Site = site;
            WorkAmount = workAmount;
            MaxWork = maxWork;
        }
    }
}
