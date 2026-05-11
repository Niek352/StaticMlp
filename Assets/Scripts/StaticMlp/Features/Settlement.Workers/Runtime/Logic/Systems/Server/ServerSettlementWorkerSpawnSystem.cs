using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class ServerSettlementWorkerSpawnSystem : ISystem
    {
        private bool _spawned;

        public void Update()
        {
            if (_spawned || HasAnyWorker())
            {
                _spawned = true;
                return;
            }

            var seed = SW.GetResource<Stage1SettlementSeed>();
            var workers = seed.InitialWorkers;
            for (var i = 0; i < workers.Length; i++)
                Spawn(workers[i]);

            _spawned = true;
        }

        private static void Spawn(Stage1SettlementWorkerSeed seed)
        {
            var profile = SettlementWorkerRuntimeProfileCatalog.Get(seed.Role);

            NetworkEntitySpawner.SpawnServerEntity<AiBotNetworkEntity>(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                profile.NetworkArchetypeId,
                entity =>
                {
                    entity.Set<AiAgentTag>();
                    entity.Set<SettlementWorkerTag>();
                    entity.Set(new SettlementWorkerIdentity
                    {
                        HomeAnchorId = seed.AnchorId,
                        RoleId = seed.RoleId
                    });
                    entity.Set(new SettlementWorkerAssignment
                    {
                        Status = SettlementWorkerAssignmentStatus.Unassigned,
                        AnchorId = 0
                    });
                    AiBotSpawns.InitializeServerAiAgent(
                        entity,
                        seed.Position,
                        seed.Rotation,
                        profile.BehaviorId,
                        profile.MaxHealth,
                        hunger: 0f,
                        fear: 0f,
                        leader: default);
                });
        }

        private static bool HasAnyWorker()
        {
            foreach (var _ in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity>>().Entities())
                return true;

            return false;
        }
    }
}
