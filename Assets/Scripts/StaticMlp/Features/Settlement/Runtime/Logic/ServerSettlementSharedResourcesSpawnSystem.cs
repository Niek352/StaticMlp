using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerSettlementSharedResourcesSpawnSystem : ISystem
    {
        private bool _spawned;

        public void Update()
        {
            if (_spawned)
                return;

            foreach (var _ in SW.Query<All<SettlementResourceStorageTag, SettlementSharedResources>>().Entities())
            {
                _spawned = true;
                return;
            }

            var settlementSeed = SW.GetResource<Stage1SettlementSeed>();
            NetworkEntitySpawner.SpawnServerEntity<SettlementSharedResourcesNetworkEntity>(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                SettlementNetworkArchetypeIds.ResourceStorage,
                entity =>
                {
                    entity.Set<SettlementResourceStorageTag>();
                    entity.Set(new SettlementSharedResources
                    {
                        Wood = settlementSeed.GetStartingResourceAmount(ResourceCatalog.WoodId),
                        Stone = settlementSeed.GetStartingResourceAmount(ResourceCatalog.StoneId)
                    });
                });

            _spawned = true;
        }
    }
}
