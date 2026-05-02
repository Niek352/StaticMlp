using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
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
            ref var inbox = ref SW.GetResource<NetInbox>();

            foreach (var evt in inbox.Events)
            {
                if (evt.EventTypeId != GameplayEventTypeIds.SpawnPhysicsCubeRequest)
                    continue;

                if (!TryGetPlayerState(evt.SourcePeer, out var playerState))
                    continue;

                var yaw = ReadYaw(evt.Payload);
                var rotation = Quaternion.Euler(0f, yaw, 0f);
                var forward = rotation * Vector3.forward;
                var spawnPosition = playerState.Position + forward * _spawnDistance + Vector3.up * _spawnHeight;
                ServerSpawns.ServerSpawnPhysicsCube(evt.SourcePeer, spawnPosition, rotation);
            }
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

        private static float ReadYaw(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return 0f;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return reader.ReadFloat();
        }
    }
}
