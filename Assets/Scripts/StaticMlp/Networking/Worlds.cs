using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking {
    public struct ServerWT : IWorldType { }
    public abstract class SW : World<ServerWT> { }

    public struct ClientCoreWT : IWorldType { }
    public abstract class CW : World<ClientCoreWT> { }

    public struct ClientUxWT : IWorldType { }
    public abstract class UXW : World<ClientUxWT> { }

    public struct ServerSystemsT : ISystemsType { }
    public abstract class ServerSys : SW.Systems<ServerSystemsT> { }

    public struct ClientCoreSystemsT : ISystemsType { }
    public abstract class ClientCoreSys : CW.Systems<ClientCoreSystemsT> { }

    public struct ClientUxSystemsT : ISystemsType { }
    public abstract class ClientUxSys : UXW.Systems<ClientUxSystemsT> { }

    public static class MultiplayerWorldBootstrap {
        public static void CreateServer(WorldConfig config = default) {
            SW.Create(config);
            SW.Types().RegisterAll();
            SW.Initialize();
        }

        public static void CreateClientCore(WorldConfig config = default) {
            CW.Create(config);
            CW.Types().RegisterAll();
            CW.Initialize();
        }

        public static void CreateClientUx(WorldConfig config = default) {
            UXW.Create(config);
            UXW.Types().RegisterAll();
            UXW.Initialize();
        }

        public static void TickServer() => SW.Tick();
        public static void TickClientCore() => CW.Tick();
        public static void TickClientUx() => UXW.Tick();
    }
}
