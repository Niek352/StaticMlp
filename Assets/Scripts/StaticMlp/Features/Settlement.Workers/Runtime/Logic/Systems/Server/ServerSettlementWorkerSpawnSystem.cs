using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

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

            SettlementWorkerSpawner.Spawn(new SettlementWorkerSpawnSpec(
                seed.Anchor,
                seed.Role,
                profile.NetworkArchetypeId,
                profile.BehaviorId,
                profile.MaxHealth,
                seed.Position,
                seed.Rotation));
        }

        private static bool HasAnyWorker()
        {
            foreach (var _ in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity>>().Entities())
                return true;

            return false;
        }
    }
}
