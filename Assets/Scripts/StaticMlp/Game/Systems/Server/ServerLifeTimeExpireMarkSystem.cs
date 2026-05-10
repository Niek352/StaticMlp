using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Game.Systems.Server
{
    public sealed class ServerLifeTimeExpireMarkSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<LifeTime>, None<IsDestroyed>>().Entities())
            {
                if (entity.Read<LifeTime>().RemainingTime <= 0f)
                    entity.Set<IsDestroyed>();
            }
        }
    }
}
