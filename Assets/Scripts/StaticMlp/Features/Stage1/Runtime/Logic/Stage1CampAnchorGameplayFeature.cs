using StaticMlp.Game.Bootstrap;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Stage1
{
    public sealed class Stage1CampAnchorGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<Stage1FlowViewState>();
        }

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(SettlementNetworkArchetypeIds.CampAnchor, _ => { });
            NetArchetypeRegistry.RegisterServer(SettlementNetworkArchetypeIds.CampAnchor, _ => { });
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerStage1CampAnchorSpawnSystem(), (short)(GameplaySystemOrder.ServerConnectionGameplay - 20));
            systems.Add(new ServerStage1FlowSystem(), GameplaySystemOrder.Gameplay - 137);
            systems.Add(new ServerStage1FlowViewStateSystem(), GameplaySystemOrder.Gameplay - 20);
        }
    }
}
