using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Features.Builtin;
using StaticMlp.Game.NetworkEntityTypes;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Game.Systems.Server {
    public static class ServerSpawns {
        public static EntityGID ServerSpawnPlayer(NetworkPeerId owner, Vector3 spawnPosition) {
            return NetworkEntitySpawner.SpawnServerEntity<PlayerNetworkEntity>(
                owner,
                NetworkAuthority.Owner,
                BuiltinGameplayFeature.PLAYER,
                entity => {
                    entity.Set<PlayerTag>();
                    entity.Set(new CharacterNetState {
                        Position = spawnPosition,
                        Velocity = Vector3.zero,
                        Rotation = Quaternion.identity
                    });
                });
        }

        public static EntityGID ServerSpawnPhysicsCube(NetworkPeerId owner, Vector3 spawnPosition, Quaternion rotation) {
            return NetworkEntitySpawner.SpawnServerEntity<PhysicsCubeNetworkEntity>(
                owner,
                NetworkAuthority.Server,
                BuiltinGameplayFeature.PHYSICS_CUBE,
                entity => {
                    entity.Set<CubeTag>();
                    entity.Set(new PhysicsCubeNetState {
                        Position = spawnPosition,
                        Velocity = Vector3.zero,
                        Rotation = rotation
                    });
                });
        }
    }
}
