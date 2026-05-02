using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Game.Bootstrap {
    public readonly struct ClientCoreSystemsBuilder {
        public void Add<TSystem>(TSystem system, short order) where TSystem : ISystem {
            ClientCoreSys.Add(system, order);
        }
    }
}
