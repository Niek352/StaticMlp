using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatLogicFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            CombatNetworkEvents.Register();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerCombatConfigInitSystem(), GameplaySystemOrder.Gameplay - 130);
            systems.Add(new ServerReceiveCombatCommandsSystem(), GameplaySystemOrder.Gameplay - 38);
            systems.Add(new ServerPassiveAutoAttackRequestSystem(), GameplaySystemOrder.Gameplay - 37);
            systems.Add(new ServerValidateCombatCommandsSystem(), GameplaySystemOrder.Gameplay - 36);
            systems.Add(new ServerAbilityCastSystem(), GameplaySystemOrder.Gameplay - 35);
            systems.Add(new ServerHitToEffectSystem(), GameplaySystemOrder.Gameplay - 34);
            systems.Add(new ServerDamageApplySystem(), GameplaySystemOrder.Gameplay - 27);
            systems.Add(new ServerHealthDeathMarkSystem(), GameplaySystemOrder.Gameplay - 25);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientCombatConfigInitSystem(), GameplaySystemOrder.Gameplay - 10);
            systems.Add(new ClientCombatAbilitySelectionSystem(), GameplaySystemOrder.Gameplay);
            systems.Add(new ClientPassiveAutoAttackTargetingSystem(), GameplaySystemOrder.Gameplay + 10);
            systems.Add(new ClientPassiveAutoAttackIntentSystem(), GameplaySystemOrder.Gameplay + 20);
            systems.Add(new ClientPassiveAutoAttackSendSystem(), GameplaySystemOrder.Gameplay + 30);
            systems.Add(new ClientCombatResultReconcileSystem(), GameplaySystemOrder.Gameplay + 35);
        }
    }
}
