using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Effects
{
    public sealed class ServerEffectCleanupSystem : ISystem
    {
        private readonly List<EntityGID> _processedEffects = new();

        public void Update()
        {
            _processedEffects.Clear();

            foreach (var effect in SW.Query<All<EffectTag, EffectProcessedTag>>().Entities())
                _processedEffects.Add(effect.GID);

            foreach (var effect in SW.Query<All<EffectTag, EffectRejectedTag>>().Entities())
                _processedEffects.Add(effect.GID);

            for (var i = 0; i < _processedEffects.Count; i++)
            {
                if (_processedEffects[i].TryUnpack<ServerWT>(out var effect))
                    effect.Destroy();
            }
        }
    }
}
