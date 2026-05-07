using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public sealed class ClientProjectionRebuildSystem : ISystem
    {
        public void Update()
        {
            ProjectionRegistry.Rebuild();
            CW.GetResource<ClientPendingRequests>().ProjectAll();
        }
    }
}
