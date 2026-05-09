using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerStatusExpireSystem : ISystem
    {
        private readonly Func<float> _deltaTimeProvider;

        public ServerStatusExpireSystem(Func<float> deltaTimeProvider = null)
        {
            _deltaTimeProvider = deltaTimeProvider ?? (() => Time.deltaTime);
        }

        public void Update()
        {
            _ = _deltaTimeProvider();

            foreach (var entity in SW.Query<All<PoisonStatus>>().Entities())
            {
                if (entity.Read<PoisonStatus>().RemainingTime <= 0f)
                    entity.Delete<PoisonStatus>();
            }

            foreach (var entity in SW.Query<All<BurningStatus>>().Entities())
            {
                if (entity.Read<BurningStatus>().RemainingTime <= 0f)
                    entity.Delete<BurningStatus>();
            }

            foreach (var entity in SW.Query<All<OiledStatus>>().Entities())
            {
                if (entity.Read<OiledStatus>().RemainingTime <= 0f)
                    entity.Delete<OiledStatus>();
            }
        }
    }
}
