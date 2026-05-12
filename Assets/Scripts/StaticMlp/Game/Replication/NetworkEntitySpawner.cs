using System;
using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication
{
    public static class NetworkEntitySpawner
    {
        public const ushort NETWORKED_ENTITY_CLUSTER = 1;

        public static EntityGID SpawnServerEntity<TNetworkEntityType>(
            NetworkPeerId owner,
            NetworkAuthority authority,
            Action<SW.Entity> initialize)
            where TNetworkEntityType : struct, INetworkEntityType
        {
            return SpawnServerEntity<TNetworkEntityType>(
                owner,
                authority,
                default(TNetworkEntityType).DefaultNetworkArchetypeId(),
                initialize);
        }

        public static EntityGID SpawnServerEntity<TNetworkEntityType>(
            NetworkPeerId owner,
            NetworkAuthority authority,
            ushort networkArchetypeId,
            Action<SW.Entity> initialize)
            where TNetworkEntityType : struct, INetworkEntityType
        {
            EnsureNetworkedEntityCluster();
            var entity = SW.NewEntity<TNetworkEntityType>(NETWORKED_ENTITY_CLUSTER);
            InitializeNetworkEntity(entity, owner, authority, networkArchetypeId);

            initialize?.Invoke(entity);
            ValidateManifestComponents<TNetworkEntityType>(entity);

            OwnershipTags.ApplyForServer(entity, owner, authority);
            SpawnBroadcaster.SendSpawn(entity);
            return entity.GID;
        }

        private static void InitializeNetworkEntity(
            SW.Entity entity,
            NetworkPeerId owner,
            NetworkAuthority authority,
            ushort networkArchetypeId)
        {
            entity.Set(new NetworkIdentity
            {
                Owner = owner,
                Authority = authority,
                NetworkArchetypeId = networkArchetypeId
            });
            entity.Set<NetworkedTag>();
            entity.Set(new NetworkReplicationState());
        }

        private static void EnsureNetworkedEntityCluster()
        {
            if (!SW.ClusterIsRegistered(NETWORKED_ENTITY_CLUSTER))
                SW.RegisterCluster(NETWORKED_ENTITY_CLUSTER);
        }

        private static void ValidateManifestComponents<TNetworkEntityType>(SW.Entity entity)
            where TNetworkEntityType : struct, INetworkEntityType
        {
            var manifest = typeof(TNetworkEntityType).GetCustomAttribute<NetworkEntityManifestAttribute>();
            if (manifest == null)
                throw new InvalidOperationException(
                    $"Network entity type `{typeof(TNetworkEntityType).FullName}` is missing {nameof(NetworkEntityManifestAttribute)}.");

            for (var i = 0; i < manifest.ReplicatedComponents.Length; i++)
            {
                var componentType = manifest.ReplicatedComponents[i];
                if (!ReplicationRegistry.IsReplicatedComponentRegistered(componentType))
                {
                    throw new InvalidOperationException(
                        $"Network entity `{typeof(TNetworkEntityType).Name}` declares replicated component `{componentType.FullName}` in its manifest, but that component is not registered in {nameof(ReplicationRegistry)}.");
                }

                if (!ReplicationRegistry.HasReplicatedComponent(entity, componentType))
                {
                    throw new InvalidOperationException(
                        $"Network entity `{typeof(TNetworkEntityType).Name}` was spawned without required manifest component `{componentType.FullName}`.");
                }
            }
        }
    }
}
