using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Systems.Server
{
    public sealed class ServerDestroyedEntityCleanupSystem : ISystem
    {
        private readonly List<EntityGID> _pendingDestroyed = new();

        public void Update()
        {
            _pendingDestroyed.Clear();

            foreach (var entity in SW.Query<All<IsDestroyed>>().Entities())
                _pendingDestroyed.Add(entity.GID);

            for (var i = 0; i < _pendingDestroyed.Count; i++)
            {
                if (!_pendingDestroyed[i].TryUnpack<ServerWT>(out var entity))
                    continue;

                if (entity.Has<NetworkedTag>() && entity.Has<NetworkIdentity>())
                    DespawnBroadcaster.SendDespawn(entity.GID);

                entity.Destroy();
            }
        }
    }
}
