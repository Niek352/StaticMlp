using StaticMlp.Game.Bootstrap;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.CampFlow
{
    public sealed class CampFlowGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<CampFlowViewState>();
        }

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(SettlementNetworkArchetypeIds.CampAnchor, _ => { });
            NetArchetypeRegistry.RegisterServer(SettlementNetworkArchetypeIds.CampAnchor, _ => { });
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(new CampFlowAnchorFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerCampFlowAnchorSpawnSystem(), (short)(GameplaySystemOrder.ServerConnectionGameplay - 20));
            systems.Add(new ServerCampFlowProgressionSystem(), GameplaySystemOrder.Gameplay - 137);
            systems.Add(new ServerCampFlowViewStateSystem(), GameplaySystemOrder.Gameplay - 20);
        }
    }
}
