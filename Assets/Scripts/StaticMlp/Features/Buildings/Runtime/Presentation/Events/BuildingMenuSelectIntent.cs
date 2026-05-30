using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Buildings
{
    public readonly struct BuildingMenuSelectIntent : IEvent
    {
        public readonly BuildingId BuildingId;

        public BuildingMenuSelectIntent(BuildingId buildingId)
        {
            BuildingId = buildingId;
        }
    }
}
