using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerTransportCompleteSystem : ISystem {
        public void Update() {
            SW.GetResource<UtpTransportContext>().TransportJobHandle.Complete();
        }
    }
}
