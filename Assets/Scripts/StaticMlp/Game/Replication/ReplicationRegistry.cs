using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Diagnostics;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication
{
    public static class ReplicationRegistry
    {
        static ReplicationRegistry()
        {
            NetworkTrafficProfiler.ComponentNameResolver = GetComponentDisplayName;
            NetworkTrafficProfiler.ComponentDeliveryResolver = GetComponentDelivery;
        }

        public delegate ComponentDelta ComponentDeltaWriter<T>(EntityGID gid, in T state)
            where T : struct, IComponent, ITrackableChanged;

        public delegate T ComponentDeltaReader<T>(byte[] payload)
            where T : struct, IComponent, ITrackableChanged;

        private static readonly Dictionary<ushort, IComponentHandler> ComponentHandlers = new();
        private static readonly Dictionary<byte, NetworkEntityInfo> NetworkEntitiesByType = new();

        public static void Clear()
        {
            ComponentHandlers.Clear();
            NetworkEntitiesByType.Clear();
        }

        public static void RegisterComponent<T>(
            ushort typeId,
            ReplicationAuthority authority,
            ReplicationAudience audience,
            NetDelivery delivery,
            ComponentDeltaWriter<T> writer,
            ComponentDeltaReader<T> reader,
            Action<CW.Entity, byte[]> clientApply = null,
            Action registerClientTypes = null,
            Action<ClientCoreSystemsBuilder> registerClientSystems = null,
            Action<CW.Entity> initializeClientState = null)
            where T : struct, IComponent, ITrackableChanged
        {
            ComponentHandlers[typeId] = new ComponentHandler<T>(
                typeId,
                authority,
                audience,
                delivery,
                writer,
                reader,
                clientApply,
                registerClientTypes,
                registerClientSystems,
                initializeClientState);
        }

        public static void RegisterNetworkEntity(
            byte entityTypeId,
            ushort networkSchemaVersion,
            ushort defaultNetworkArchetypeId)
        {
            NetworkEntitiesByType[entityTypeId] = new NetworkEntityInfo(
                networkSchemaVersion,
                defaultNetworkArchetypeId);
        }

        public static ushort GetNetworkSchemaVersion(byte entityType)
        {
            return NetworkEntitiesByType.TryGetValue(entityType, out var info)
                ? info.NetworkSchemaVersion
                : (ushort)0;
        }

        public static ushort GetDefaultNetworkArchetypeId(byte entityType)
        {
            return NetworkEntitiesByType.TryGetValue(entityType, out var info)
                ? info.DefaultNetworkArchetypeId
                : (ushort)0;
        }

        public static bool IsServerAuthority(ushort componentTypeId)
        {
            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                   && handler.Authority == ReplicationAuthority.Server;
        }

        public static bool IsOwnerAuthority(ushort componentTypeId)
        {
            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                   && handler.Authority == ReplicationAuthority.Owner;
        }

        public static string GetComponentDisplayName(ushort componentTypeId)
        {
            if (componentTypeId == NetworkIdentityReplication.TypeId)
                return nameof(NetworkIdentity);

            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                ? handler.ComponentDisplayName
                : $"Component({componentTypeId})";
        }

        public static NetDelivery GetComponentDelivery(ushort componentTypeId)
        {
            if (componentTypeId == NetworkIdentityReplication.TypeId)
                return NetDelivery.ReliableSequenced;

            return ComponentHandlers.TryGetValue(componentTypeId, out var handler)
                ? handler.Delivery
                : NetDelivery.Unreliable;
        }

        public static void ApplyDelta(CW.Entity e, ComponentDelta delta)
        {
            if (ComponentHandlers.TryGetValue(delta.ComponentTypeId, out var handler))
            {
                handler.Apply(e, delta.Payload);
                return;
            }

            if (delta.ComponentTypeId == NetworkIdentityReplication.TypeId)
                e.Set(NetworkIdentityReplication.Read(delta.Payload));
        }

        public static void ApplyDelta(SW.Entity e, ComponentDelta delta)
        {
            if (ComponentHandlers.TryGetValue(delta.ComponentTypeId, out var handler))
            {
                handler.Apply(e, delta.Payload);
                return;
            }

            if (delta.ComponentTypeId == NetworkIdentityReplication.TypeId)
                e.Set(NetworkIdentityReplication.Read(delta.Payload));
        }

        public static void ApplyInitialState(CW.Entity e, List<ComponentDelta> components)
        {
            for (var i = 0; i < components.Count; i++)
                ApplyDelta(e, components[i]);
        }

        public static void CollectClientOwnedDirty(NetOutbox outbox, NetworkPeerId peer)
        {
            foreach (var e in CW.Query<All<LocalOwned, NetworkedTag, NetworkIdentity, NetworkDirty, NetworkReplicationState>>().Entities())
            {
                CollectDirty(e, outbox, peer, ReplicationAuthority.Owner);
                e.Delete<NetworkDirty>();
            }
        }

        public static void CollectServerAuthorityDirty(NetOutbox outbox, IReadOnlyList<NetworkPeerId> peers)
        {
            foreach (var e in SW.Query<All<NetworkedTag, NetworkIdentity, NetworkDirty, NetworkReplicationState>>().Entities())
            {
                CollectDirty(e, outbox, peers, ReplicationAuthority.Server);
                e.Delete<NetworkDirty>();
            }
        }

        public static void CollectInitialState(SW.Entity e, NetworkPeerId peer, List<ComponentDelta> components)
        {
            if (e.Has<NetworkIdentity>())
                components.Add(NetworkIdentityReplication.CreateDelta(e.GID, e.Read<NetworkIdentity>()));

            foreach (var handler in ComponentHandlers.Values)
            {
                if (!handler.Has(e) || !CanSendToPeer(e, peer, handler.Audience))
                    continue;

                components.Add(handler.CreateDelta(e));
            }
        }

        public static void RegisterClientCoreGeneratedTypes()
        {
            foreach (var handler in ComponentHandlers.Values)
                handler.RegisterClientTypes();
        }

        public static void RegisterClientCoreInterpolationSystems(ClientCoreSystemsBuilder systems)
        {
            foreach (var handler in ComponentHandlers.Values)
                handler.RegisterClientSystems(systems);
        }

        public static void InitializeClientCoreInterpolatedState(CW.Entity e)
        {
            foreach (var handler in ComponentHandlers.Values)
                handler.InitializeClientState(e);
        }

        private static void CollectDirty(
            CW.Entity e,
            NetOutbox outbox,
            NetworkPeerId peer,
            ReplicationAuthority authority)
        {
            foreach (var handler in ComponentHandlers.Values)
            {
                if (handler.Authority != authority || !handler.HasChanged(e))
                    continue;

                outbox.EnqueueComponentDelta(peer, handler.CreateDelta(e), handler.Delivery);
            }
        }

        private static void CollectDirty(
            SW.Entity e,
            NetOutbox outbox,
            IReadOnlyList<NetworkPeerId> peers,
            ReplicationAuthority authority)
        {
            foreach (var handler in ComponentHandlers.Values)
            {
                if (handler.Authority != authority || !handler.HasChanged(e))
                    continue;

                var delta = handler.CreateDelta(e);
                for (var i = 0; i < peers.Count; i++)
                {
                    var peer = peers[i];
                    if (!CanSendToPeer(e, peer, handler.Audience))
                        continue;

                    outbox.EnqueueComponentDelta(peer, delta, handler.Delivery);
                }
            }
        }

        private static bool CanSendToPeer(SW.Entity e, NetworkPeerId peer, ReplicationAudience audience)
        {
            return audience == ReplicationAudience.All
                   || e.Has<NetworkIdentity>() && e.Read<NetworkIdentity>().Owner == peer;
        }

        private interface IComponentHandler
        {
            string ComponentDisplayName { get; }
            ReplicationAuthority Authority { get; }
            ReplicationAudience Audience { get; }
            NetDelivery Delivery { get; }
            void Apply(CW.Entity e, byte[] payload);
            void Apply(SW.Entity e, byte[] payload);
            bool Has(SW.Entity e);
            bool HasChanged(CW.Entity e);
            bool HasChanged(SW.Entity e);
            ComponentDelta CreateDelta(CW.Entity e);
            ComponentDelta CreateDelta(SW.Entity e);
            void RegisterClientTypes();
            void RegisterClientSystems(ClientCoreSystemsBuilder systems);
            void InitializeClientState(CW.Entity e);
        }

        private sealed class ComponentHandler<T> : IComponentHandler
            where T : struct, IComponent, ITrackableChanged
        {
            private readonly ComponentDeltaWriter<T> _writer;
            private readonly ComponentDeltaReader<T> _reader;
            private readonly Action<CW.Entity, byte[]> _clientApply;
            private readonly Action _registerClientTypes;
            private readonly Action<ClientCoreSystemsBuilder> _registerClientSystems;
            private readonly Action<CW.Entity> _initializeClientState;

            public ComponentHandler(
                ushort typeId,
                ReplicationAuthority authority,
                ReplicationAudience audience,
                NetDelivery delivery,
                ComponentDeltaWriter<T> writer,
                ComponentDeltaReader<T> reader,
                Action<CW.Entity, byte[]> clientApply,
                Action registerClientTypes,
                Action<ClientCoreSystemsBuilder> registerClientSystems,
                Action<CW.Entity> initializeClientState)
            {
                TypeId = typeId;
                Authority = authority;
                Audience = audience;
                Delivery = delivery;
                _writer = writer ?? throw new ArgumentNullException(nameof(writer));
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
                _clientApply = clientApply ?? ApplyClientState;
                _registerClientTypes = registerClientTypes;
                _registerClientSystems = registerClientSystems;
                _initializeClientState = initializeClientState;
                ComponentDisplayName = typeof(T).Name;
            }

            private ushort TypeId { get; }
            public string ComponentDisplayName { get; }
            public ReplicationAuthority Authority { get; }
            public ReplicationAudience Audience { get; }
            public NetDelivery Delivery { get; }

            public void Apply(CW.Entity e, byte[] payload)
            {
                _clientApply(e, payload);
            }

            public void Apply(SW.Entity e, byte[] payload)
            {
                e.Set(_reader(payload));
            }

            public bool Has(SW.Entity e) => e.Has<T>();

            public bool HasChanged(CW.Entity e)
            {
                return e.Has<T>() && e.HasChanged<T>();
            }

            public bool HasChanged(SW.Entity e)
            {
                return e.Has<T>() && e.HasChanged<T>();
            }

            public ComponentDelta CreateDelta(CW.Entity e)
            {
                ref readonly var state = ref e.Read<T>();
                return _writer(e.GID, in state);
            }

            public ComponentDelta CreateDelta(SW.Entity e)
            {
                ref readonly var state = ref e.Read<T>();
                return _writer(e.GID, in state);
            }

            public void RegisterClientTypes()
            {
                _registerClientTypes?.Invoke();
            }

            public void RegisterClientSystems(ClientCoreSystemsBuilder systems)
            {
                _registerClientSystems?.Invoke(systems);
            }

            public void InitializeClientState(CW.Entity e)
            {
                _initializeClientState?.Invoke(e);
            }

            private void ApplyClientState(CW.Entity e, byte[] payload)
            {
                e.Set(_reader(payload));
            }
        }

        private readonly struct NetworkEntityInfo
        {
            public readonly ushort NetworkSchemaVersion;
            public readonly ushort DefaultNetworkArchetypeId;

            public NetworkEntityInfo(ushort networkSchemaVersion, ushort defaultNetworkArchetypeId)
            {
                NetworkSchemaVersion = networkSchemaVersion;
                DefaultNetworkArchetypeId = defaultNetworkArchetypeId;
            }
        }
    }
}
