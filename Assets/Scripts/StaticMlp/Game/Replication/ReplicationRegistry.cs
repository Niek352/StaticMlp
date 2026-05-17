using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Diagnostics;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public static class ReplicationRegistry {
        private static readonly Guid NetworkIdentityGuid = new NetworkIdentity().Config().Guid!.Value;

        static ReplicationRegistry() {
            NetworkTrafficProfiler.ComponentNameResolver = GetComponentDisplayName;
            NetworkTrafficProfiler.ComponentDeliveryResolver = GetComponentDelivery;
        }

        private static readonly Dictionary<ushort, IComponentHandler> ComponentHandlers = new();
        private static readonly Dictionary<Guid, IComponentHandler> ComponentHandlersByGuid = new();
        private static readonly Dictionary<Type, IComponentHandler> ComponentHandlersByType = new();
        private static readonly Dictionary<byte, NetworkEntityInfo> NetworkEntitiesByType = new();

        public static void Clear() {
            ComponentHandlers.Clear();
            ComponentHandlersByGuid.Clear();
            ComponentHandlersByType.Clear();
            NetworkEntitiesByType.Clear();
        }

        public static void RegisterComponent<T>(
            ushort typeId,
            ReplicationAuthority authority,
            ReplicationAudience audience,
            NetDelivery delivery,
            ushort sendRate,
            Action registerClientTypes = null,
            Action<ClientCoreSystemsBuilder> registerClientSystems = null,
            Action<CW.Entity> initializeClientState = null)
            where T : struct, IComponent, IComponentConfig<T>, ITrackableChanged {
            var handler = new ComponentHandler<T>(
                typeId,
                new T().Config().Guid.Value,
                authority,
                audience,
                delivery,
                sendRate,
                registerClientTypes,
                registerClientSystems,
                initializeClientState);
            ComponentHandlers[typeId] = handler;
            ComponentHandlersByGuid[handler.Guid] = handler;
            ComponentHandlersByType[typeof(T)] = handler;
        }

        public static void RegisterNetworkEntity(
            byte entityTypeId,
            ushort networkSchemaVersion,
            ushort defaultNetworkArchetypeId) {
            NetworkEntitiesByType[entityTypeId] = new NetworkEntityInfo(
                networkSchemaVersion,
                defaultNetworkArchetypeId);
        }

        public static ushort GetNetworkSchemaVersion(byte entityType) {
            return NetworkEntitiesByType.TryGetValue(entityType, out var info)
                ? info.NetworkSchemaVersion
                : (ushort)0;
        }

        public static ushort GetDefaultNetworkArchetypeId(byte entityType) {
            return NetworkEntitiesByType.TryGetValue(entityType, out var info)
                ? info.DefaultNetworkArchetypeId
                : (ushort)0;
        }

        public static bool IsServerAuthority(ushort componentTypeId) {
            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                   && handler.Authority == ReplicationAuthority.Server;
        }

        public static bool IsOwnerAuthority(ushort componentTypeId) {
            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                   && handler.Authority == ReplicationAuthority.Owner;
        }

        public static bool IsReplicatedComponentRegistered(Type componentType) {
            if (componentType == null)
                throw new ArgumentNullException(nameof(componentType));

            return ComponentHandlersByType.ContainsKey(componentType);
        }

        public static bool HasReplicatedComponent(SW.Entity entity, Type componentType) {
            if (componentType == null)
                throw new ArgumentNullException(nameof(componentType));

            if (!ComponentHandlersByType.TryGetValue(componentType, out var handler))
                throw new InvalidOperationException(
                    $"Replicated component `{componentType.FullName}` is not registered in {nameof(ReplicationRegistry)}.");

            return handler.Has(entity);
        }

        public static string GetComponentDisplayName(ushort componentTypeId) {
            if (componentTypeId == NetworkIdentityReplication.TypeId)
                return nameof(NetworkIdentity);

            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                ? handler.ComponentDisplayName
                : $"Component({componentTypeId})";
        }

        public static NetDelivery GetComponentDelivery(ushort componentTypeId) {
            if (componentTypeId == NetworkIdentityReplication.TypeId)
                return NetDelivery.ReliableSequenced;

            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                ? handler.Delivery
                : NetDelivery.Unreliable;
        }

        public static byte[] CreateInitialStateSnapshot(SW.Entity e, NetworkPeerId peer) {
            using var writer = SW.Serializer.CreateFilteredEntitiesSnapshotWriter(
                guid => guid == NetworkIdentityGuid || CanWriteInitialComponent(e, peer, guid));
            writer.Write(e);
            return writer.CreateSnapshot();
        }

        public static byte[] CreatePublicClusterStateSnapshot(ushort clusterId, bool gzip) {
            using var writer = SW.Serializer.CreateFilteredEntitiesSnapshotWriter(
                guid => guid == NetworkIdentityGuid || CanWritePublicInitialComponent(guid));

            ReadOnlySpan<ushort> clusters = stackalloc ushort[] { clusterId };
            foreach (var e in SW.Query<All<NetworkedTag, NetworkIdentity>>().Entities(clusters: clusters))
                writer.Write(e);

            return writer.CreateSnapshot(gzip);
        }

        public static void ApplyServerSnapshot(byte[] payload, FilteredEntitySnapshotLoadMode mode, bool gzip = false) {
            CW.Serializer.LoadFilteredEntitiesSnapshot(
                payload,
                new FilteredEntitySnapshotReadOptions<ClientCoreWT>(
                    mode,
                    CanClientReadServerComponent,
                    beforeApplyComponent: OnClientBeforeApplyComponent,
                    afterApplyComponent: OnClientAfterApplyComponent),
                gzip);
        }

        public static void ApplyClientOwnerSnapshot(byte[] payload, NetworkPeerId sourcePeer) {
            ApplyClientOwnerSnapshot(payload, sourcePeer, relayBuffer: null);
        }

        public static void ApplyClientOwnerSnapshot(byte[] payload, NetworkPeerId sourcePeer, ServerRelayBuffer relayBuffer) {
            SW.Serializer.LoadFilteredEntitiesSnapshot(
                payload,
                new FilteredEntitySnapshotReadOptions<ServerWT>(
                    FilteredEntitySnapshotLoadMode.PatchExistingOnly,
                    (entity, guid) => CanServerReadClientComponent(entity, guid, sourcePeer),
                    afterApplyComponent: (entity, guid) => RelayAppliedClientOwnerComponent(entity, guid, sourcePeer, relayBuffer)));
        }

        public static void CollectClientOwnedDirty(NetOutbox outbox, NetworkPeerId peer) {
            foreach (var e in CW.Query<All<LocalOwned, NetworkedTag, NetworkIdentity, NetworkDirty, NetworkReplicationState>>().Entities()) {
                foreach (var handler in ComponentHandlers.Values) {
                    if (handler.Authority != ReplicationAuthority.Owner || !handler.HasChanged(e))
                        continue;

                    outbox.EnqueueEntitySnapshot(peer, CreateClientDirtySnapshot(e, handler.Guid), handler.Delivery);
                }

                e.Delete<NetworkDirty>();
            }
        }

        public static void CollectServerAuthorityDirty(NetOutbox outbox, IReadOnlyList<NetworkPeerId> peers) {
            foreach (var e in SW.Query<All<NetworkedTag, NetworkIdentity, NetworkDirty, NetworkReplicationState>>().Entities()) {
                foreach (var handler in ComponentHandlers.Values) {
                    var canCollect = CanCollectDirtyOnServer(e, handler);
                    var hasChanged = handler.HasChanged(e);
                    if (!canCollect || !hasChanged)
                        continue;

                    var payload = CreateServerDirtySnapshot(e, handler.Guid);
                    for (var i = 0; i < peers.Count; i++) {
                        var peer = peers[i];
                        if (!CanSendToPeer(e, peer, handler.Audience))
                            continue;

                        outbox.EnqueueEntitySnapshot(peer, payload, handler.Delivery);
                    }
                }

                e.Delete<NetworkDirty>();
            }
        }

        public static void RegisterClientCoreGeneratedTypes() {
            foreach (var handler in ComponentHandlers.Values)
                handler.RegisterClientTypes();
        }

        public static void RegisterClientCoreInterpolationSystems(ClientCoreSystemsBuilder systems) {
            foreach (var handler in ComponentHandlers.Values)
                handler.RegisterClientSystems(systems);
        }

        public static void InitializeClientCoreInterpolatedState(CW.Entity e) {
            foreach (var handler in ComponentHandlers.Values)
                handler.AfterClientApply(e);
        }

        private static byte[] CreateClientDirtySnapshot(CW.Entity e, Guid componentGuid) {
            using var writer = CW.Serializer.CreateFilteredEntitiesSnapshotWriter(guid => guid == componentGuid);
            writer.Write(e);
            return writer.CreateSnapshot();
        }

        private static byte[] CreateServerDirtySnapshot(SW.Entity e, Guid componentGuid) {
            using var writer = SW.Serializer.CreateFilteredEntitiesSnapshotWriter(guid => guid == componentGuid);
            writer.Write(e);
            return writer.CreateSnapshot();
        }

        private static bool CanWriteInitialComponent(SW.Entity e, NetworkPeerId peer, Guid guid) {
            return ComponentHandlersByGuid.TryGetValue(guid, out var handler)
                   && handler.Has(e)
                   && CanSendToPeer(e, peer, handler.Audience);
        }

        private static bool CanWritePublicInitialComponent(Guid guid) {
            return ComponentHandlersByGuid.TryGetValue(guid, out var handler)
                   && handler.Audience == ReplicationAudience.All;
        }

        private static bool CanClientReadServerComponent(CW.Entity entity, Guid guid) {
            return guid == NetworkIdentityGuid || ComponentHandlersByGuid.ContainsKey(guid);
        }

        private static bool CanServerReadClientComponent(SW.Entity entity, Guid guid, NetworkPeerId sourcePeer) {
            return ComponentHandlersByGuid.TryGetValue(guid, out var handler)
                   && handler.Authority == ReplicationAuthority.Owner
                   && entity.Has<NetworkIdentity>()
                   && entity.Read<NetworkIdentity>().Owner == sourcePeer;
        }

        private static void OnClientBeforeApplyComponent(CW.Entity entity, Guid guid) {
            if (ComponentHandlersByGuid.TryGetValue(guid, out var handler))
                handler.BeforeClientApply(entity);
        }

        private static void OnClientAfterApplyComponent(CW.Entity entity, Guid guid) {
            if (ComponentHandlersByGuid.TryGetValue(guid, out var handler))
                handler.AfterClientApply(entity);
        }

        private static void RelayAppliedClientOwnerComponent(
            SW.Entity entity,
            Guid guid,
            NetworkPeerId sourcePeer,
            ServerRelayBuffer relayBuffer) {
            if (relayBuffer == null)
                return;

            if (!ComponentHandlersByGuid.TryGetValue(guid, out var handler))
                return;

            if (handler.Authority != ReplicationAuthority.Owner)
                return;

            relayBuffer.Add(sourcePeer, CreateServerDirtySnapshot(entity, guid), handler.Delivery);
        }

        private static bool CanCollectDirtyOnServer(SW.Entity e, IComponentHandler handler) {
            if (handler.Authority == ReplicationAuthority.Server)
                return true;

            return handler.Authority == ReplicationAuthority.Owner && e.Has<ServerOwned>();
        }

        private static bool CanSendToPeer(SW.Entity e, NetworkPeerId peer, ReplicationAudience audience) {
            return audience == ReplicationAudience.All
                   || e.Has<NetworkIdentity>() && e.Read<NetworkIdentity>().Owner == peer;
        }

        private interface IComponentHandler {
            Guid Guid { get; }
            Type ComponentType { get; }
            string ComponentDisplayName { get; }
            ReplicationAuthority Authority { get; }
            ReplicationAudience Audience { get; }
            NetDelivery Delivery { get; }
            bool Has(SW.Entity e);
            bool HasChanged(CW.Entity e);
            bool HasChanged(SW.Entity e);
            void RegisterClientTypes();
            void RegisterClientSystems(ClientCoreSystemsBuilder systems);
            void BeforeClientApply(CW.Entity e);
            void AfterClientApply(CW.Entity e);
        }

        private sealed class ComponentHandler<T> : IComponentHandler
            where T : struct, IComponent, ITrackableChanged {
            private readonly Action _registerClientTypes;
            private readonly Action<ClientCoreSystemsBuilder> _registerClientSystems;
            private readonly Action<CW.Entity> _initializeClientState;

            public ComponentHandler(
                ushort typeId,
                Guid guid,
                ReplicationAuthority authority,
                ReplicationAudience audience,
                NetDelivery delivery,
                ushort sendRate,
                Action registerClientTypes,
                Action<ClientCoreSystemsBuilder> registerClientSystems,
                Action<CW.Entity> initializeClientState) {
                TypeId = typeId;
                Guid = guid;
                Authority = authority;
                Audience = audience;
                Delivery = delivery;
                SendRate = sendRate;
                _registerClientTypes = registerClientTypes;
                _registerClientSystems = registerClientSystems;
                _initializeClientState = initializeClientState;
                ComponentDisplayName = typeof(T).Name;
            }

            private ushort TypeId { get; }
            private ushort SendRate { get; }
            public Guid Guid { get; }
            public Type ComponentType => typeof(T);
            public string ComponentDisplayName { get; }
            public ReplicationAuthority Authority { get; }
            public ReplicationAudience Audience { get; }
            public NetDelivery Delivery { get; }

            public bool Has(SW.Entity e) => e.Has<T>();
            public bool HasChanged(CW.Entity e) => e.Has<T>() && e.HasChanged<T>();
            public bool HasChanged(SW.Entity e) => e.Has<T>() && e.HasChanged<T>();

            public void RegisterClientTypes() {
                _registerClientTypes?.Invoke();
            }

            public void RegisterClientSystems(ClientCoreSystemsBuilder systems) {
                _registerClientSystems?.Invoke(systems);
            }

            public void BeforeClientApply(CW.Entity e) {
                if (_initializeClientState == null || !e.Has<Interpolated<T>>())
                    return;

                e.Set(new InterpolatedPrevious<T>(e.Read<Interpolated<T>>().Value));
            }

            public void AfterClientApply(CW.Entity e) {
                _initializeClientState?.Invoke(e);
                if (_initializeClientState == null || !e.Has<Interpolated<T>>())
                    return;

                ref var clock = ref e.Mut<InterpolatedClock<T>>();
                clock.StartedAt = Time.time;
                clock.Duration = SendRate == 0 ? 0f : 1f / SendRate;
            }
        }

        private readonly struct NetworkEntityInfo {
            public readonly ushort NetworkSchemaVersion;
            public readonly ushort DefaultNetworkArchetypeId;

            public NetworkEntityInfo(ushort networkSchemaVersion, ushort defaultNetworkArchetypeId) {
                NetworkSchemaVersion = networkSchemaVersion;
                DefaultNetworkArchetypeId = defaultNetworkArchetypeId;
            }
        }
    }
}
