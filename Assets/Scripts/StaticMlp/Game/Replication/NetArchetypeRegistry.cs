using System;
using System.Collections.Generic;

namespace StaticMlp.Networking.Replication {
    public static class NetArchetypeRegistry {
        private static readonly Dictionary<ushort, Action<CW.Entity>> ClientFactories = new();
        private static readonly Dictionary<ushort, Action<SW.Entity>> ServerFactories = new();

        public static void Clear() {
            ClientFactories.Clear();
            ServerFactories.Clear();
        }

        public static void RegisterClient(ushort networkArchetypeId, Action<CW.Entity> apply) {
            ClientFactories.TryGetValue(networkArchetypeId, out var existing);
            ClientFactories[networkArchetypeId] = existing + apply;
        }

        public static void RegisterServer(ushort networkArchetypeId, Action<SW.Entity> apply) {
            ServerFactories.TryGetValue(networkArchetypeId, out var existing);
            ServerFactories[networkArchetypeId] = existing + apply;
        }

        public static void Apply(ushort networkArchetypeId, CW.Entity e) {
            if (ClientFactories.TryGetValue(networkArchetypeId, out var apply))
                apply(e);
        }

        public static void Apply(ushort networkArchetypeId, SW.Entity e) {
            if (ServerFactories.TryGetValue(networkArchetypeId, out var apply))
                apply(e);
        }
    }
}
