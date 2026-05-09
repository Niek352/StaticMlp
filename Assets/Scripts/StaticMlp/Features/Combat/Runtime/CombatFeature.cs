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
            systems.Add(new ServerReceiveCombatCommandsSystem(), GameplaySystemOrder.Gameplay - 38);
            systems.Add(new ServerPassiveAutoAttackRequestSystem(), GameplaySystemOrder.Gameplay - 37);
            systems.Add(new ServerValidateCombatCommandsSystem(), GameplaySystemOrder.Gameplay - 36);
            systems.Add(new ServerAbilityCastSystem(), GameplaySystemOrder.Gameplay - 35);
            systems.Add(new ServerHitToEffectSystem(), GameplaySystemOrder.Gameplay - 34);
            systems.Add(new ServerEffectPreprocessSystem(), GameplaySystemOrder.Gameplay - 33);
            systems.Add(new ServerSynergyTriggerSystem(), GameplaySystemOrder.Gameplay - 32);
            systems.Add(new ServerAddStatusApplySystem(), GameplaySystemOrder.Gameplay - 31);
            systems.Add(new ServerAreaEffectTickSystem(), GameplaySystemOrder.Gameplay - 29);
            systems.Add(new ServerStatusTickSystem(), GameplaySystemOrder.Gameplay - 28);
            systems.Add(new ServerDamageApplySystem(), GameplaySystemOrder.Gameplay - 27);
            systems.Add(new ServerStatusExpireSystem(), GameplaySystemOrder.Gameplay - 26);
            systems.Add(new ServerHealthDeathMarkSystem(), GameplaySystemOrder.Gameplay - 25);
            systems.Add(new ServerEffectCleanupSystem(), GameplaySystemOrder.Gameplay - 24);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientPassiveAutoAttackInitSystem(), GameplaySystemOrder.Gameplay - 10);
            systems.Add(new ClientCombatAbilitySelectionSystem(), GameplaySystemOrder.Gameplay);
            systems.Add(new ClientPassiveAutoAttackTargetingSystem(), GameplaySystemOrder.Gameplay + 10);
            systems.Add(new ClientPassiveAutoAttackIntentSystem(), GameplaySystemOrder.Gameplay + 20);
            systems.Add(new ClientPassiveAutoAttackSendSystem(), GameplaySystemOrder.Gameplay + 30);
            systems.Add(new ClientCombatResultReconcileSystem(), GameplaySystemOrder.Gameplay + 35);
            systems.Add(new ClientCombatVisualSpawnSystem(), GameplaySystemOrder.Gameplay + 36);
            systems.Add(new ClientCombatVisualLifetimeSystem(), GameplaySystemOrder.Gameplay + 37);
            systems.Add(new ClientPassiveAutoAttackPresentationSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new ClientCombatViewStateSystem(), (short)(ViewSystemOrder.BuildPresentationState + 5));
            systems.Add(new ClientDamageFeedbackPresentationSystem(), (short)(ViewSystemOrder.BuildPresentationState + 10));
        }

        public override void RegisterClientViewSync(ViewSyncBuilder sync)
        {
            sync.Register<CombatViewState>();
            sync.Register<CombatProjectileVisualState>();
            sync.Register<CombatEffectVisualState>();
            sync.Register<PassiveAutoAttackViewState>();
            sync.Register<PassiveAutoAttackTargetViewState>();
            sync.Register<DamageFeedbackViewState>();
        }
    }
}
