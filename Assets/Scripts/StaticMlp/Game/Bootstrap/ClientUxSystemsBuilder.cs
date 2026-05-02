using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Game.Bootstrap {
    public readonly struct ClientUxSystemsBuilder {
        public void Add<TSystem>(TSystem system, short order) where TSystem : ISystem {
            ClientUxSys.Add(system, order);
        }
    }
}
