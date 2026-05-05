using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerSteamTransportPollSystem : ISystem {
        public void Update() {
            SW.GetResource<SteamTransportContext>().Poll();
        }
    }
}
