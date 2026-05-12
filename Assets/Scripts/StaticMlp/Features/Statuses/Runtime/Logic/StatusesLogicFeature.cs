using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public sealed class StatusesLogicFeature : GameplayFeature
    {
        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(StatusNetworkArchetypes.POISON, e => e.Set<PoisonStatus>());
            NetArchetypeRegistry.RegisterClient(StatusNetworkArchetypes.BURNING, e => e.Set<BurningStatus>());
            NetArchetypeRegistry.RegisterClient(StatusNetworkArchetypes.OILED, e => e.Set<OiledStatus>());
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(new StatusEntityFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerStatusesConfigInitSystem(), GameplaySystemOrder.Gameplay - 129);
            systems.Add(new ServerSynergyTriggerSystem(), GameplaySystemOrder.Gameplay - 32);
            systems.Add(new ServerApplyPoisonStatusSystem(), GameplaySystemOrder.Gameplay - 31);
            systems.Add(new ServerApplyBurningStatusSystem(), GameplaySystemOrder.Gameplay - 31);
            systems.Add(new ServerApplyOiledStatusSystem(), GameplaySystemOrder.Gameplay - 31);
            systems.Add(new ServerAreaEffectTickSystem(), GameplaySystemOrder.Gameplay - 29);
            systems.Add(new ServerPoisonStatusTickSystem(), GameplaySystemOrder.Gameplay - 28);
            systems.Add(new ServerBurningStatusTickSystem(), GameplaySystemOrder.Gameplay - 28);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientStatusesConfigInitSystem(), GameplaySystemOrder.Gameplay - 8);
        }
    }
}
