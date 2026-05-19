using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class ClientLoadoutPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new LoadoutPreparationScreenState
            {
                SelectedPrimaryModuleId = LoadoutModuleCatalog.PoisonArrowModuleId
            });
        }
    }
}
