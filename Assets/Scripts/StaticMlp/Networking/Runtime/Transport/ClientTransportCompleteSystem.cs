using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientTransportCompleteSystem : ISystem {
        public void Update() {
            CW.GetResource<UtpTransportContext>().TransportJobHandle.Complete();
        }
    }
}
