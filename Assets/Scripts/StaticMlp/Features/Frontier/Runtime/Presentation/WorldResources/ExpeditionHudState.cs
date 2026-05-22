using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Frontier
{
    public struct ExpeditionHudState : IResource
    {
        public ExpeditionAvailabilityStatus Availability;
        public ExpeditionActivityStatus Activity;
    }
}
