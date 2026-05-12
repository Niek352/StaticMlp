using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementSharedResourcesFactory : NetEntityFactory<SettlementSharedResourcesNetworkEntity>, IResource
    {
        public EntityGID Spawn(Stage1SettlementSeed seed)
        {
            var entity = CreateEntity(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                SettlementNetworkArchetypeIds.ResourceStorage);
            Configure(entity, seed);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in Stage1SettlementSeed seed)
        {
            entity.Set<SettlementResourceStorageTag>();
            entity.Set(new SettlementSharedResources
            {
                Wood = seed.GetStartingResourceAmount(ResourceCatalog.WoodId),
                Stone = seed.GetStartingResourceAmount(ResourceCatalog.StoneId)
            });
        }
    }
}
