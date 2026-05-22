using StaticMlp.Features.AiBots;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;
using UnityEngine;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class SettlementWorkersGameplayFeature : GameplayFeature
    {
        private const string CHARACTER_VIEW_PATH = "Views/BotCharacterView";

        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<SettlementWorkerSummary>();
            ProjectionRegistry.Register<SettlementCampBuilderJobState>();
            ProjectionRegistry.Register<BuildingWorkerAssignmentState>();

            RequestRegistry.Register<SetSettlementWorkerAssignmentRequestEvent, SetSettlementWorkerAssignmentResultEvent>(
                new SetSettlementWorkerAssignmentHandler(),
                projector: null,
                serverOrder: GameplaySystemOrder.Gameplay - 46);
            RequestRegistry.Register<SetBuildingWorkerAssignmentRequestEvent, SetBuildingWorkerAssignmentResultEvent>(
                new SetBuildingWorkerAssignmentHandler(),
                projector: null,
                serverOrder: GameplaySystemOrder.Gameplay - 46);
        }

        public override void RegisterPrefabs()
        {
            //SettlementWorkersReplicationRegistration.Register();

            NetArchetypeRegistry.RegisterClient(SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER, e =>
            {
                e.Set<AiAgentTag>();
                e.Set<SettlementWorkerTag>();
                e.Set(new ViewPath(CHARACTER_VIEW_PATH));
                e.Set(new ViewTransform
                {
                    RenderRotation = Quaternion.identity
                });
            });

            NetArchetypeRegistry.RegisterServer(SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER, e =>
            {
                e.Set<AiAgentTag>();
                e.Set<SettlementWorkerTag>();
            });
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(new SettlementWorkerFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerSettlementWorkerSpawnSystem(), GameplaySystemOrder.ServerConnectionGameplay + 25);
            systems.Add(new ServerSettlementWorkerCampBuilderJobSystem(), GameplaySystemOrder.Gameplay - 45);
            systems.Add(new ServerSettlementWorkerTaskSyncSystem(), GameplaySystemOrder.Gameplay - 44);
        }
    }
}
