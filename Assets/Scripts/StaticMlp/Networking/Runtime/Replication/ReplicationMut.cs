using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public static class ReplicationMut {
        // Gameplay writes to replicated components must use this helper.
        // Direct entity.Mut<T>() is reserved for generated/apply/bootstrap code with an explicit reason.
        public static ref T Mut<T>(SW.Entity entity)
            where T : struct, IComponent, ITrackableChanged {
            MarkDataDirty(entity);
            return ref entity.Mut<T>();
        }

        public static ref T Mut<T>(CW.Entity entity)
            where T : struct, IComponent, ITrackableChanged {
            MarkDataDirty(entity);
            return ref entity.Mut<T>();
        }

        public static void MarkDataDirty(SW.Entity entity) {
            ref var state = ref EnsureState(entity);
            unchecked {
                state.DataVersion++;
            }

            entity.Set<NetworkDirty>();
        }

        public static void MarkDataDirty(CW.Entity entity) {
            ref var state = ref EnsureState(entity);
            unchecked {
                state.DataVersion++;
            }

            entity.Set<NetworkDirty>();
        }

        private static ref NetworkReplicationState EnsureState(SW.Entity entity) {
            return ref entity.Add<NetworkReplicationState>();
        }

        private static ref NetworkReplicationState EnsureState(CW.Entity entity) {
            return ref entity.Add<NetworkReplicationState>();
        }
    }
}
