using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Game.Bootstrap
{
    public readonly struct ServerSystemsBuilder
    {
        public void Add<TSystem>(TSystem system, short order) where TSystem : ISystem
        {
            ServerSys.Add(system, order);
        }
    }
}