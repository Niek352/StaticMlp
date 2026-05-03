using FFS.Libraries.StaticEcs.Unity;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Composition
{
    public static class MultiplayerSystemBootstrap
    {
        public static void CreateServerSystems()
        {
            ServerSys.Create();
            ServerSys.Add(new ServerTransportCompleteSystem(), order: -1000);
            ServerSys.Add(new ServerRawInboxDrainSystem(), order: -900);
            ServerSys.Add(new ServerConnectionLifecycleSystem(), order: -850);
            ServerSys.Add(new ServerReceiveClientOwnedStateSystem(), order: -780);
            GameplayFeatureDiscovery.RegisterServerSystems(new ServerSystemsBuilder());
            ServerSys.Add(new ServerOwnedReplicationCollectSystem(), order: 500);
            ServerSys.Add(new ServerRelayClientOwnedStateSystem(), order: 550);
            ServerSys.Add(new ServerTransportSendSystem(), order: 700);
            ServerSys.Add(new ServerTransportScheduleSystem(), order: 1000);
            EcsDebug<ServerWT>.AddWorld<ServerSystemsT>();
            ServerSys.Initialize();
        }

        public static void CreateClientCoreSystems()
        {
            ClientCoreSys.Create();
            var viewFactory = new ResourcesEntityViewFactory(ViewRootProvider.Root);
            ClientCoreSys.Add(new ClientTransportCompleteSystem(), order: -1000);
            ClientCoreSys.Add(new ClientRawInboxDrainSystem(), order: -900);
            ClientCoreSys.Add(new ClientSnapshotApplySystem(), order: -810);
            ClientCoreSys.Add(new ClientSpawnApplySystem(), order: -800);
            ClientCoreSys.Add(new ClientDespawnApplySystem(), order: -790);
            ClientCoreSys.Add(new ClientOwnershipApplySystem(), order: -780);
            ClientCoreSys.Add(new ClientComponentDeltaApplySystem(), order: -770);
            var systemsBuilder = new ClientCoreSystemsBuilder();
            systemsBuilder.Add(new BindEntityViewSystem(viewFactory), ViewSystemOrder.BindViews);
            GameplayFeatureDiscovery.RegisterClientCoreSystems(systemsBuilder);
            GameplayFeatureDiscovery.RegisterClientViewSync(
                new ViewSyncBuilder(systemsBuilder, ViewSystemOrder.ApplyPresentationState));
            systemsBuilder.Add(new DestroyEntityViewSystem(viewFactory), ViewSystemOrder.DestroyViews);
            ClientCoreSys.Add(new ClientReplicationCollectSystem(), order: 500);
            ClientCoreSys.Add(new ClientTransportSendSystem(), order: 700);
            ClientCoreSys.Add(new ClientTransportScheduleSystem(), order: 1000);
            EcsDebug<ClientCoreWT>.AddWorld<ClientCoreSystemsT>();

            ClientCoreSys.Initialize();
        }

        public static void UpdateServerFrame()
        {
            ServerSys.Update();
            SW.Tick();
        }

        public static void UpdateClientCoreFrame()
        {
            ClientCoreSys.Update();
            CW.Tick();
        }
    }
}
