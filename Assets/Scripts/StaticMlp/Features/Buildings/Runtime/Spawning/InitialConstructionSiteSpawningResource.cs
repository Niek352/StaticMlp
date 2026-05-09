using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Buildings
{
    public sealed class InitialConstructionSiteSpawningResource : IResource
    {
        public InitialConstructionSiteSpawningResource(InitialConstructionSiteDefinition[] sites)
        {
            Sites = sites ?? System.Array.Empty<InitialConstructionSiteDefinition>();
        }

        public InitialConstructionSiteDefinition[] Sites { get; }
    }
}
