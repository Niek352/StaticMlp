using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public sealed class ClientRequestRuntimeInitSystem : ISystem
    {
        public void Init()
        {
            if (!CW.HasResource<ClientPendingRequests>())
                CW.SetResource(new ClientPendingRequests());
        }
    }
}
