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

            ProjectionRegistry.Register<ExpeditionAvailabilityState>();
            ProjectionRegistry.Register<ActiveExpeditionState>();
            ProjectionRegistry.Register<ThreatState>();
            ProjectionRegistry.Register<RaidScheduleState>();
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerFrontierAnchorInitSystem(), GameplaySystemOrder.Gameplay - 137);
            systems.Add(new ServerFrontierExpeditionAvailabilitySystem(), GameplaySystemOrder.Gameplay - 136);
            systems.Add(new ServerFrontierStartExpeditionSystem(), GameplaySystemOrder.Gameplay - 135);
            systems.Add(new ServerFrontierRaidActivationSystem(), GameplaySystemOrder.Gameplay - 96);
            systems.Add(new ServerFrontierExpeditionResolutionSystem(), GameplaySystemOrder.Gameplay - 94);
            systems.Add(new ServerFrontierRaidResolutionSystem(), GameplaySystemOrder.Gameplay - 93);
        }
    }
}
