using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking {
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
