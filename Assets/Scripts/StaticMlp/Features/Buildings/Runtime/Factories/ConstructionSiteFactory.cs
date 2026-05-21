using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    internal sealed class ConstructionSiteFactory : NetEntityFactory<ConstructionSiteNetworkEntity>
    {
        public EntityGID Spawn(
            NetworkPeerId owner,
            NetworkAuthority authority,
            ushort networkArchetypeId,
            in ConstructionSiteSpawnSpec spec)
        {
            var entity = CreateEntity(owner, authority, networkArchetypeId);
            Configure(entity, spec);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in ConstructionSiteSpawnSpec spec)
        {
            InitializeConstructionSite(entity, in spec);
        }

        private static void InitializeConstructionSite(SW.Entity entity, in ConstructionSiteSpawnSpec spec)
        {
            entity.Set(new SettlementAnchorRef(spec.AnchorId));
            entity.Set<ConstructionSiteTag>();
            entity.Set(new ConstructionSiteState
            {
                BuildingId = spec.Definition.Id.Value,
                Phase = ConstructionPhase.WaitingForResources
            });
            entity.Set(new ConstructionTransform
            {
                Position = spec.Position,
                Rotation = spec.Rotation
            });
            entity.Set(new ConstructionResources());
            ConstructionResourcesAccess.InitializeRows(entity, spec.Definition.ConstructionCost);
            entity.Set(new ConstructionProgress
            {
                BuildWorkRequired = spec.Definition.BuildWorkRequired
            });
            entity.Set(new BuildingFootprint(spec.Definition.FootprintWidth, spec.Definition.FootprintLength));

            if (spec.StartReadyToBuild)
            {
                ref var siteState = ref ReplicationMut.Mut<ConstructionSiteState>(entity);
                SettlementConstructionRules.MarkAllResourcesDelivered(entity);
                siteState.Phase = ConstructionPhase.ReadyToBuild;
            }

            if (spec.InitialBuildWork <= 0f)
                return;

            ref var buildState = ref ReplicationMut.Mut<ConstructionSiteState>(entity);
            ref var progress = ref ReplicationMut.Mut<ConstructionProgress>(entity);
            SettlementConstructionRules.ApplyBuildWork(
                entity,
                ref buildState,
                ref progress,
                spec.InitialBuildWork,
                progress.BuildWorkRequired);
        }
    }
}
