using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Build
{
    public sealed class BuildLogicFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            BuildNetworkEvents.Register();
            ProjectionRegistry.Register<BossBuildPreparationState>();
            ProjectionRegistry.Register<BossPreparedBuildSnapshot>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerBossBuildPreparationAnchorInitSystem(), GameplaySystemOrder.Gameplay - 141);
            systems.Add(new ServerBuildSelectionInitSystem(), GameplaySystemOrder.Gameplay - 140);
            systems.Add(new ServerReceivePrepareBuildCommandSystem(), GameplaySystemOrder.Gameplay - 139);
            systems.Add(new ServerPreparedBuildSnapshotSystem(), GameplaySystemOrder.Gameplay - 138);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientBuildSelectionSystem(), GameplaySystemOrder.Gameplay - 20);
        }
    }
}
