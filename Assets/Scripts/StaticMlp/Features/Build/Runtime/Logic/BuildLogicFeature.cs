using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Build
{
    public sealed class BuildLogicFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            BuildNetworkEvents.Register();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
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
