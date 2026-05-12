using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingEntityFactory : IResource
    {
        private readonly FinishedBuildingFactory _finishedBuildingFactory = new ();
        private readonly ConstructionSiteFactory _constructionSiteFactory = new ();
        public EntityGID SpawnConstructionSite(in ConstructionSiteSpawnSpec spec)
        {
            if (spec.AnchorId.Value == 0)
                throw new InvalidOperationException("Construction site spawn requires a non-zero settlement anchor id.");

            if (!BuildingNetworkCatalog.TryGet(spec.Definition.Id, out var network))
                throw new InvalidOperationException($"Missing network catalog entry for building {spec.Definition.Id}.");

            return _constructionSiteFactory.Spawn(
                spec.Owner,
                NetworkAuthority.Server,
                network.BlueprintArchetypeId,
                spec);
        }

        public EntityGID SpawnFinishedBuilding(in FinishedBuildingSpawnSpec spec)
        {
            if (spec.AnchorId.Value == 0)
                throw new InvalidOperationException("Finished building spawn requires a non-zero settlement anchor id.");

            if (!BuildingNetworkCatalog.TryGet(spec.Definition.Id, out var network))
                throw new InvalidOperationException($"Missing network catalog entry for building {spec.Definition.Id}.");

            return _finishedBuildingFactory.Spawn(
                spec.Owner,
                NetworkAuthority.Server,
                network.FinishedArchetypeId,
                spec);
        }

        

        
    }
}
