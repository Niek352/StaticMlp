using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Game.Systems.Server {
    public static class ServerSpawns {
        private const ushort NETWORKED_ENTITY_CLUSTER = 1;

        public static EntityGID ServerSpawnPlayer(NetworkPeerId owner, Vector3 spawnPosition) {
            EnsureNetworkedEntityCluster();
            var e = SW.NewEntity<Default>(NETWORKED_ENTITY_CLUSTER);

            e.Set(new NetworkIdentity {
                Owner = owner,
                Authority = NetworkAuthority.Owner,
                NetworkArchetypeId = BuiltinGameplayFeature.PLAYER
            });

            e.Set<NetworkedTag>();
            e.Set<PlayerTag>();
            e.Set(new CharacterNetState {
                Position = spawnPosition,
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity
            });

            OwnershipTags.ApplyForServer(e, owner, NetworkAuthority.Owner);
            SpawnBroadcaster.SendSpawn(e);
            return e.GID;
        }

        public static EntityGID ServerSpawnPhysicsCube(NetworkPeerId owner, Vector3 spawnPosition, Quaternion rotation) {
            EnsureNetworkedEntityCluster();
            var e = SW.NewEntity<Default>(NETWORKED_ENTITY_CLUSTER);

            e.Set(new NetworkIdentity {
                Owner = owner,
                Authority = NetworkAuthority.Server,
                NetworkArchetypeId = BuiltinGameplayFeature.PHYSICS_CUBE
            });

            e.Set<NetworkedTag>();
            e.Set<CubeTag>();
            e.Set(new PhysicsCubeNetState {
                Position = spawnPosition,
                Velocity = Vector3.zero,
                Rotation = rotation
            });

            OwnershipTags.ApplyForServer(e, owner, NetworkAuthority.Server);
            SpawnBroadcaster.SendSpawn(e);
            return e.GID;
        }

        private static void EnsureNetworkedEntityCluster() {
            if (!SW.ClusterIsRegistered(NETWORKED_ENTITY_CLUSTER))
                SW.RegisterCluster(NETWORKED_ENTITY_CLUSTER);
        }
    }
}
