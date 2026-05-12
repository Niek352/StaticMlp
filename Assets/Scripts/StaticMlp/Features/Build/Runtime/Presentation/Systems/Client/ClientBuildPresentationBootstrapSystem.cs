using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public sealed class ClientBuildPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new BuildPreparationScreenState
            {
                SelectedPrimaryModuleId = BuildModuleCatalog.PoisonArrowModuleId
            });
        }
    }
}
