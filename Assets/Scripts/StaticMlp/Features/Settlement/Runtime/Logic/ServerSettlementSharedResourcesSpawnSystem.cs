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

            SW.GetResource<SettlementSharedResourcesFactory>().Spawn(SW.GetResource<SettlementSeed>());

            _spawned = true;
        }
    }
}
