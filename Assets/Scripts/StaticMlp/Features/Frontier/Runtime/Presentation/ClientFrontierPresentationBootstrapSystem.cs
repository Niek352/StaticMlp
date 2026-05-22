using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientFrontierPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            // No IResource presentation states to initialize; bridges read directly from logic components.
        }
    }
}
