using System;
using System.Collections.Generic;

namespace StaticMlp.Networking.Replication {
    public static class PrefabRegistry {
        private static readonly Dictionary<ushort, Action<CW.Entity>> ClientFactories = new();
        private static readonly Dictionary<ushort, Action<SW.Entity>> ServerFactories = new();

        public static void Clear() {
            ClientFactories.Clear();
            ServerFactories.Clear();
        }

        public static void RegisterClient(ushort prefabId, Action<CW.Entity> apply) {
            ClientFactories[prefabId] = apply;
        }

        public static void RegisterServer(ushort prefabId, Action<SW.Entity> apply) {
            ServerFactories[prefabId] = apply;
        }

        public static void Apply(ushort prefabId, CW.Entity e) {
            if (ClientFactories.TryGetValue(prefabId, out var apply))
                apply(e);
        }

        public static void Apply(ushort prefabId, SW.Entity e) {
            if (ServerFactories.TryGetValue(prefabId, out var apply))
                apply(e);
        }
    }
}
