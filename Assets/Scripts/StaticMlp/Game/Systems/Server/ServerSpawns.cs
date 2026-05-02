using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Game.Systems.Server {
    public static class ServerSpawns {
        public static EntityGID ServerSpawnPlayer(NetworkPeerId owner, Vector3 spawnPosition) {
            var e = SW.NewEntity<Default>();

            e.Set(new NetworkIdentity {
                Owner = owner,
                Authority = NetworkAuthority.Owner,
                PrefabId = Prefabs.Player
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

        public static EntityGID ServerSpawnOwnedMonster(Vector3 spawnPosition) {
            var e = SW.NewEntity<Default>();

            e.Set(new NetworkIdentity {
                Owner = new NetworkPeerId(0),
                Authority = NetworkAuthority.Server,
                PrefabId = Prefabs.Monster
            });

            e.Set<NetworkedTag>();
            e.Set<MonsterTag>();
            e.Set(new CharacterNetState {
                Position = spawnPosition,
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity
            });

            OwnershipTags.ApplyForServer(e, new NetworkPeerId(0), NetworkAuthority.Server);
            SpawnBroadcaster.SendSpawn(e);
            return e.GID;
        }
    }
}
