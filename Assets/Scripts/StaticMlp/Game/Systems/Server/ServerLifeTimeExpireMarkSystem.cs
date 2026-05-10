using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Game.Systems.Server
{
    public sealed class ServerLifeTimeExpireMarkSystem : ISystem
    {
        public void Update()
        {
            var currentTick = SW.GetResource<SimulationTime>().ServerTick;
            foreach (var entity in SW.Query<All<LifeTime>, None<IsDestroyed>>().Entities())
            {
                if (currentTick >= entity.Read<LifeTime>().EndTick)
                    entity.Set<IsDestroyed>();
            }
        }
    }
}
