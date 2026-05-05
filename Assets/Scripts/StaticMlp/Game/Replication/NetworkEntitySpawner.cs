using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication
{
    public static class NetworkEntitySpawner
    {
        public const ushort NetworkedEntityCluster = 1;

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
            var entity = SW.NewEntity<TNetworkEntityType>(NetworkedEntityCluster);
            InitializeNetworkEntity(entity, owner, authority, networkArchetypeId);

            initialize?.Invoke(entity);

            OwnershipTags.ApplyForServer(entity, owner, authority);
            SpawnBroadcaster.SendSpawn(entity);
            return entity.GID;
        }

        public static EntityGID SpawnServerEntity(
            NetworkPeerId owner,
            NetworkAuthority authority,
            ushort networkArchetypeId,
            Action<SW.Entity> initialize)
        {
            EnsureNetworkedEntityCluster();
            var entity = SW.NewEntity<Default>(NetworkedEntityCluster);
            InitializeNetworkEntity(entity, owner, authority, networkArchetypeId);

            initialize?.Invoke(entity);

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
            if (!SW.ClusterIsRegistered(NetworkedEntityCluster))
                SW.RegisterCluster(NetworkedEntityCluster);
        }
    }
}
