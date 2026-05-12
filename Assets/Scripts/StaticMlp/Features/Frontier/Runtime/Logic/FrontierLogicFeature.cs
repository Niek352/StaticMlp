using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public sealed class FrontierLogicFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            NetworkEventRegistry.Register<StartExpeditionRequestEvent>(
                StartExpeditionRequestEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                StartExpeditionRequestEvent.Write,
                StartExpeditionRequestEvent.TryRead);

            NetworkEventRegistry.Register<StartBossEncounterRequestEvent>(
                StartBossEncounterRequestEvent.NETWORK_EVENT_ID,
                NetDelivery.ReliableSequenced,
                StartBossEncounterRequestEvent.Write,
                StartBossEncounterRequestEvent.TryRead);

            ProjectionRegistry.Register<ExpeditionAvailabilityState>();
            ProjectionRegistry.Register<ActiveExpeditionState>();
            ProjectionRegistry.Register<ThreatState>();
            ProjectionRegistry.Register<RaidScheduleState>();
            ProjectionRegistry.Register<BossEncounterState>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerFrontierExpeditionAvailabilitySystem(), GameplaySystemOrder.Gameplay - 136);
            systems.Add(new ServerFrontierStartExpeditionSystem(), GameplaySystemOrder.Gameplay - 135);
            systems.Add(new ServerFrontierRaidActivationSystem(), GameplaySystemOrder.Gameplay - 96);
            systems.Add(new ServerFrontierExpeditionResolutionSystem(), GameplaySystemOrder.Gameplay - 94);
            systems.Add(new ServerFrontierProgressionFlagThreatEscalationSystem(), GameplaySystemOrder.Gameplay - 92);
            systems.Add(new ServerFrontierRaidResolutionSystem(), GameplaySystemOrder.Gameplay - 91);
            systems.Add(new ServerFrontierBossAvailabilitySystem(), GameplaySystemOrder.Gameplay - 88);
            systems.Add(new ServerFrontierStartBossEncounterSystem(), GameplaySystemOrder.Gameplay - 87);
            systems.Add(new ServerFrontierBossResolutionSystem(), GameplaySystemOrder.Gameplay - 86);
        }
    }
}
