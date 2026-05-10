using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public sealed class ClientStatusesConfigInitSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new StatusesConfig());
        }
    }
}
