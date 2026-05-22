using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class ClientLoadoutPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            // No IResource presentation states to initialize; bridges read directly from logic components.
        }
    }
}
