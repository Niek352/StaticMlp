using System;
using System.Diagnostics;
using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game
{
    public abstract class NetEntityFactory<TNetworkEntityType>
        where TNetworkEntityType : struct, INetworkEntityType
    {
        protected bool _debugTrackCreation;

        protected World<ServerWT>.Entity CreateEntity(
            NetworkPeerId owner,
            NetworkAuthority authority,
            ushort networkArchetypeId,
            ushort clusterId = 0)
        {
#if FFS_ECS_DEBUG
            if (_debugTrackCreation)
                throw new Exception($"Tried to create a second server-side instance of the same net-entity.");
            _debugTrackCreation = true;
#endif
            
            var entity = SW.NewEntity<TNetworkEntityType>(clusterId);
            InitializeNetworkEntity(entity, owner, authority, networkArchetypeId);
            OwnershipTags.ApplyForServer(entity, authority);
            return entity;
        }

        protected void SendEntity(World<ServerWT>.Entity entity)
        {
            CompleteEntityCreation(entity);
            SpawnBroadcaster.SendSpawn(entity);
        }

        protected void CompleteEntityCreation(World<ServerWT>.Entity entity)
        {
#if FFS_ECS_DEBUG
            if (!_debugTrackCreation)
                throw new Exception($"Tried to create before creating an actual world-space");
            _debugTrackCreation = false;
            ValidateManifestComponents(entity);
#endif
        }

        internal static void InitializeNetworkEntity(
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
        
        [Conditional("FFS_ECS_DEBUG")]
        internal static void ValidateManifestComponents(SW.Entity entity)
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
