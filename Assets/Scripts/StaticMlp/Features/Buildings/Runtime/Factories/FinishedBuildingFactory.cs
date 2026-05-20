using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    internal sealed class FinishedBuildingFactory : NetEntityFactory<FinishedBuildingNetworkEntity>
    {
        public EntityGID Spawn(
            NetworkPeerId owner,
            NetworkAuthority authority,
            ushort networkArchetypeId,
            in FinishedBuildingSpawnSpec spec)
        {
            var entity = CreateEntity(owner, authority, networkArchetypeId);
            Configure(entity, spec);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in FinishedBuildingSpawnSpec spec)
        {
            InitializeFinishedBuilding(entity, in spec);
        }

        private static void InitializeFinishedBuilding(SW.Entity entity, in FinishedBuildingSpawnSpec spec)
        {
            var constructionResources = SettlementConstructionRules.CreateResources(spec.Definition.ConstructionCost);
            SettlementConstructionRules.MarkAllResourcesDelivered(ref constructionResources);

            entity.Set(new SettlementAnchorRef(spec.AnchorId));
            entity.Set<FinishedBuildingTag>();
            entity.Set(new ConstructionSiteState
            {
                BuildingId = spec.Definition.Id.Value,
                Phase = ConstructionPhase.Completed
            });
            entity.Set(spec.Transform);
            entity.Set(constructionResources);
            entity.Set(new ConstructionProgress
            {
                BuildWorkRequired = spec.Definition.BuildWorkRequired,
                BuildWorkDone = spec.Definition.BuildWorkRequired
            });
            entity.Set(new BuildingFootprint(spec.Definition.FootprintWidth, spec.Definition.FootprintLength));
        }
    }
}
