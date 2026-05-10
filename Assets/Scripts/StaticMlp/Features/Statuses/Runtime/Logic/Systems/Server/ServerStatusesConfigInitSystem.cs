using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public sealed class ServerStatusesConfigInitSystem : ISystem
    {
        public void Init()
        {
            SW.SetResource(new StatusesConfig());
        }
    }
}
