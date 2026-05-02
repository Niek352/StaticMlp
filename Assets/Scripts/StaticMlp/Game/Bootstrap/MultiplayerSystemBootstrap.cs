using FFS.Libraries.StaticEcs.Unity;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Game.Bootstrap {
    public static class MultiplayerSystemBootstrap {
        public static void CreateServerSystems() {
            
            ServerSys.Create();
            ServerSys.Add(new ServerTransportCompleteSystem(), order: -1000);
            ServerSys.Add(new ServerRawInboxDrainSystem(), order: -900);
            ServerSys.Add(new ServerConnectionLifecycleSystem(), order: -850);
            ServerSys.Add(new ServerReceiveClientOwnedStateSystem(), order: -780);
            ServerSys.Add(new ServerAiSystem(), order: 0);
            ServerSys.Add(new ServerOwnedReplicationCollectSystem(), order: 500);
            ServerSys.Add(new ServerRelayClientOwnedStateSystem(), order: 550);
            ServerSys.Add(new ServerTransportSendSystem(), order: 700);
            ServerSys.Add(new ServerTransportScheduleSystem(), order: 1000);
            EcsDebug<ServerWT>.AddWorld<ServerSystemsT>();
            ServerSys.Initialize();
        }

        public static void CreateClientCoreSystems() {
            ClientCoreSys.Create();
            ClientCoreSys.Add(new ClientTransportCompleteSystem(), order: -1000);
            ClientCoreSys.Add(new ClientRawInboxDrainSystem(), order: -900);
            ClientCoreSys.Add(new ClientSpawnApplySystem(), order: -800);
            ClientCoreSys.Add(new ClientDespawnApplySystem(), order: -790);
            ClientCoreSys.Add(new ClientOwnershipApplySystem(), order: -780);
            ClientCoreSys.Add(new ClientComponentDeltaApplySystem(), order: -770);
            ClientCoreSys.Add(new LocalPlayerMovementSystem(), order: 0);
            ClientCoreSys.Add(new RemoteSmoothingSystem(), order: 300);
            ClientCoreSys.Add(new ClientReplicationCollectSystem(), order: 500);
            ClientCoreSys.Add(new ClientTransportSendSystem(), order: 700);
            ClientCoreSys.Add(new ClientTransportScheduleSystem(), order: 1000);
            EcsDebug<ClientCoreWT>.AddWorld<ClientCoreSystemsT>();
            
            ClientCoreSys.Initialize();
        }

        public static void UpdateServerFrame() {
            ServerSys.Update();
            SW.Tick();
        }

        public static void UpdateClientCoreFrame() {
            ClientCoreSys.Update();
            CW.Tick();
        }
    }
}
