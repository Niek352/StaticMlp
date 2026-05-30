using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ClientResourcesInventoryPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.NewEntity<Default>().Set(new ResourcesInventoryHudViewData());
        }
    }
}
