using StaticMlp.Game.Bootstrap;
using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            CombatNetworkEvents.Register();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerPassiveAutoAttackRequestSystem(), GameplaySystemOrder.Gameplay - 450);
            systems.Add(new ServerDamageApplySystem(), GameplaySystemOrder.Gameplay - 400);
            systems.Add(new ServerEffectCleanupSystem(), GameplaySystemOrder.Gameplay - 300);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientPassiveAutoAttackInitSystem(), GameplaySystemOrder.Gameplay - 10);
            systems.Add(new ClientPassiveAutoAttackTargetingSystem(), GameplaySystemOrder.Gameplay + 10);
            systems.Add(new ClientPassiveAutoAttackIntentSystem(), GameplaySystemOrder.Gameplay + 20);
            systems.Add(new ClientPassiveAutoAttackSendSystem(), GameplaySystemOrder.Gameplay + 30);
            systems.Add(new ClientPassiveAutoAttackPresentationSystem(), ViewSystemOrder.BuildPresentationState);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder sync)
        {
            sync.Register<PassiveAutoAttackViewState>();
            sync.Register<PassiveAutoAttackTargetViewState>();
        }
    }
}
