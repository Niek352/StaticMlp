using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerBotAiDeathSystem : ISystem
    {
        private readonly List<EntityGID> _pendingDeaths = new();

        public void Update()
        {
            _pendingDeaths.Clear();

            foreach (var entity in SW.Query<All<AiAgentTag, IsDiedTag>>().Entities())
                _pendingDeaths.Add(entity.GID);

            for (var i = 0; i < _pendingDeaths.Count; i++)
            {
                if (!_pendingDeaths[i].TryUnpack<ServerWT>(out var entity)
                    || !entity.Has<AiAgentTag>()
                    || !entity.Has<IsDiedTag>())
                    continue;

                NetworkEntityDespawner.DespawnAndDestroy(entity);
            }
        }
    }
}
