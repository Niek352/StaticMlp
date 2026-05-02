using System;
using System.Collections.Generic;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;

namespace StaticMlp.Networking.Replication {
    public static class Prefabs {
        public const ushort Player = 1;
        public const ushort Monster = 2;
    }

    public static class PrefabRegistry {
        private static readonly Dictionary<ushort, Action<CW.Entity>> ClientFactories = new();
        private static readonly Dictionary<ushort, Action<SW.Entity>> ServerFactories = new();

        static PrefabRegistry() {
            RegisterClient(Prefabs.Player, e => {
                e.Set<PlayerTag>();
                e.Set(new ViewTransform {
                    RenderRotation = UnityEngine.Quaternion.identity
                });
            });

            RegisterServer(Prefabs.Player, e => e.Set<PlayerTag>());

            RegisterClient(Prefabs.Monster, e => {
                e.Set<MonsterTag>();
                e.Set(new ViewTransform {
                    RenderRotation = UnityEngine.Quaternion.identity
                });
            });

            RegisterServer(Prefabs.Monster, e => e.Set<MonsterTag>());
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
