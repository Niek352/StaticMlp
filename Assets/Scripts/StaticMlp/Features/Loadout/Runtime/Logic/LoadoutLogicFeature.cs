using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Loadout
{
    public sealed class LoadoutLogicFeature : GameplayFeature
    {
        public override void RegisterServerResources()
        {
            SlotRules.ValidateModuleCatalog(LoadoutModuleCatalog.All);
        }

        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<BossLoadoutPreparationState>();
            ProjectionRegistry.Register<BossPreparedLoadoutSnapshot>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerActiveModuleLoadoutInitSystem(), GameplaySystemOrder.Gameplay - 141);
            systems.Add(new ServerLoadoutSelectionInitSystem(), GameplaySystemOrder.Gameplay - 140);
            systems.Add(new ServerReceivePrepareLoadoutCommandSystem(), GameplaySystemOrder.Gameplay - 139);
            systems.Add(new ServerPreparedLoadoutSnapshotSystem(), GameplaySystemOrder.Gameplay - 138);
            systems.Add(new ServerActivateLoadoutModuleRequestSystem(), GameplaySystemOrder.Gameplay - 137);
            systems.Add(new ServerDeactivateLoadoutModuleRequestSystem(), GameplaySystemOrder.Gameplay - 136);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientLoadoutSelectionSystem(), GameplaySystemOrder.Gameplay - 20);
        }
    }
}
