using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientBuildingMenuPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.NewEntity<Default>().Set(new BuildingMenuViewData());
        }
    }
}
