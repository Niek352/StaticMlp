using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientSteamTransportPollSystem : ISystem {
        public void Update() {
            CW.GetResource<SteamTransportContext>().Poll();
        }
    }
}
