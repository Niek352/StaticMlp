using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Features.Shared;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerHealthDeathMarkSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<Health>, None<IsDiedTag>>().Entities())
            {
                if (entity.Read<Health>().Current <= 0f)
                    entity.Set<IsDiedTag>();
            }
        }
    }
}
