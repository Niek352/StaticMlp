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
            systems.Add(new ServerCombatAutoAttackInitSystem(), GameplaySystemOrder.Gameplay - 130);
            systems.Add(new ServerPassiveAutoAttackRequestSystem(), GameplaySystemOrder.Gameplay - 45);
            systems.Add(new ServerDamageApplySystem(), GameplaySystemOrder.Gameplay - 38);
            systems.Add(new ServerHealthDeathMarkSystem(), GameplaySystemOrder.Gameplay - 37);
            systems.Add(new ServerEffectCleanupSystem(), GameplaySystemOrder.Gameplay - 36);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientPassiveAutoAttackInitSystem(), GameplaySystemOrder.Gameplay - 10);
            systems.Add(new ClientPassiveAutoAttackTargetingSystem(), GameplaySystemOrder.Gameplay + 10);
            systems.Add(new ClientPassiveAutoAttackIntentSystem(), GameplaySystemOrder.Gameplay + 20);
            systems.Add(new ClientPassiveAutoAttackSendSystem(), GameplaySystemOrder.Gameplay + 30);
            systems.Add(new ClientPassiveAutoAttackPresentationSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new ClientDamageFeedbackPresentationSystem(), (short)(ViewSystemOrder.BuildPresentationState + 10));
        }

        public override void RegisterClientViewSync(ViewSyncBuilder sync)
        {
            sync.Register<PassiveAutoAttackViewState>();
            sync.Register<PassiveAutoAttackTargetViewState>();
            sync.Register<DamageFeedbackViewState>();
        }
    }
}
