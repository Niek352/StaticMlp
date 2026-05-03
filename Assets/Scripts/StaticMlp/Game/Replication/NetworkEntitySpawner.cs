using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Networking.Replication
{
    public static class NetworkEntitySpawner
    {
        public const ushort NetworkedEntityCluster = 1;

        public static EntityGID SpawnServerEntity(
            NetworkPeerId owner,
            NetworkAuthority authority,
            ushort networkArchetypeId,
            Action<SW.Entity> initialize)
        {
            EnsureNetworkedEntityCluster();
            var entity = SW.NewEntity<Default>(NetworkedEntityCluster);
            entity.Set(new NetworkIdentity
            {
                Owner = owner,
                Authority = authority,
                NetworkArchetypeId = networkArchetypeId
            });
            entity.Set<NetworkedTag>();

            initialize?.Invoke(entity);

            OwnershipTags.ApplyForServer(entity, owner, authority);
            SpawnBroadcaster.SendSpawn(entity);
            return entity.GID;
        }

        private static void EnsureNetworkedEntityCluster()
        {
            if (!SW.ClusterIsRegistered(NetworkedEntityCluster))
                SW.RegisterCluster(NetworkedEntityCluster);
        }
    }
}
