using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Build
{
    public sealed class BuildLogicFeature : GameplayFeature
    {
        public override void RegisterServerResources()
        {
            SlotRules.ValidateModuleCatalog(BuildModuleCatalog.All);
        }

        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<BossBuildPreparationState>();
            ProjectionRegistry.Register<BossPreparedBuildSnapshot>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerActiveBuildModuleLoadoutInitSystem(), GameplaySystemOrder.Gameplay - 141);
            systems.Add(new ServerBuildSelectionInitSystem(), GameplaySystemOrder.Gameplay - 140);
            systems.Add(new ServerReceivePrepareBuildCommandSystem(), GameplaySystemOrder.Gameplay - 139);
            systems.Add(new ServerPreparedBuildSnapshotSystem(), GameplaySystemOrder.Gameplay - 138);
            systems.Add(new ServerActivateBuildModuleRequestSystem(), GameplaySystemOrder.Gameplay - 137);
            systems.Add(new ServerDeactivateBuildModuleRequestSystem(), GameplaySystemOrder.Gameplay - 136);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientBuildSelectionSystem(), GameplaySystemOrder.Gameplay - 20);
        }
    }
}
