using StaticMlp.Features.BuildingCatalog;
using Aspid.StaticEcs.Windows;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingMenuViewModel : EcsWindowViewModelBase
    {
        public BuildingMenuPresentation Presentation { get; private set; }

        public void Sync(in BuildingMenuState state)
        {
            Presentation = BuildingMenuPresentation.Create(in state, CW.GetResource<ClientSettlementUnlockState>());
            NotifyChanged();
        }

        public void CloseMenu()
        {
            CW.SendEvent(new BuildingMenuCloseIntent());
        }

        public void SelectBuilding(BuildingId buildingId)
        {
            CW.SendEvent(new BuildingMenuSelectIntent(buildingId));
        }
    }
}
