using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerTransportCompleteSystem : ISystem {
        public void Update() {
            if (SW.HasResource<UtpTransportContext>())
                SW.GetResource<UtpTransportContext>().TransportJobHandle.Complete();
        }
    }
}
