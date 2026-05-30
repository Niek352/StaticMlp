using System;
using Aspid.MVVM;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    [ViewModel]
    public sealed partial class BuildingMenuViewModel
    {
        [OneWayBind] private BuildingMenuPresentation _presentation;

        public event Action Changed;

        public void Apply(in BuildingMenuViewData data)
        {
            Presentation = data.Presentation;
        }

        partial void OnPresentationChanged(BuildingMenuPresentation newValue)
        {
            Changed?.Invoke();
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
