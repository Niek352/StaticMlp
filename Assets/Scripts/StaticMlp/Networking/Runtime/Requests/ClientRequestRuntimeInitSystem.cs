using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Requests
{
    public sealed class ClientRequestRuntimeInitSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new ClientPendingRequests());
        }
    }
}
