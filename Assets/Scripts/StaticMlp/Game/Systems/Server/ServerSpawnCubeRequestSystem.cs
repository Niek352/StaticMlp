using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Game.Systems.Server
{
    public sealed class ServerSpawnCubeRequestSystem : ISystem
    {
        private readonly float _spawnDistance;
        private readonly float _spawnHeight;

        public ServerSpawnCubeRequestSystem(float spawnDistance = 1.7f, float spawnHeight = 1.2f)
        {
            _spawnDistance = spawnDistance;
            _spawnHeight = spawnHeight;
        }

        public void Update()
        {
            NetworkEvents.ForEachServer<SpawnPhysicsCubeRequestEvent>(Handle);
        }

        private void Handle(NetworkPeerId sourcePeer, in SpawnPhysicsCubeRequestEvent request)
        {
            if (!TryGetPlayerState(sourcePeer, out var playerState))
                return;

            var rotation = Quaternion.Euler(0f, request.CameraYaw, 0f);
            var forward = rotation * Vector3.forward;
            var spawnPosition = playerState.Position + forward * _spawnDistance + Vector3.up * _spawnHeight;
            ServerSpawns.ServerSpawnPhysicsCube(sourcePeer, spawnPosition, rotation);
        }

        private static bool TryGetPlayerState(NetworkPeerId owner, out CharacterNetState state)
        {
            foreach (var e in SW.Query<All<PlayerTag, NetworkIdentity, CharacterNetState>>().Entities())
            {
                ref readonly var identity = ref e.Read<NetworkIdentity>();
                if (identity.Owner != owner)
                    continue;

                state = e.Read<CharacterNetState>();
                return true;
            }

            state = default;
            return false;
        }
    }
}
