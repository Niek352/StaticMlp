using Aspid.MVVM;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    [ViewModel]
    public sealed partial class BuildingMenuCardViewModel
    {
        [OneWayBind] private string _displayName = string.Empty;
        [OneWayBind] private string _categoryLabel = string.Empty;
        [OneWayBind] private string _costLabel = string.Empty;
        [OneWayBind] private bool _isSelected;
        [OneWayBind] private bool _isAvailable;
        [OneWayBind] private string _lockedReason = string.Empty;
        [OneWayBind] private bool _lockedReasonVisible;

        private BuildingId _buildingId;

        public void Apply(in BuildingMenuCardPresentation card)
        {
            _buildingId = card.BuildingId;
            DisplayName = card.DisplayName;
            CategoryLabel = card.CategoryLabel;
            CostLabel = card.CostLabel;
            IsSelected = card.IsSelected;
            IsAvailable = card.IsAvailable;
            LockedReason = card.LockedReason;
            LockedReasonVisible = !card.IsAvailable && !string.IsNullOrEmpty(card.LockedReason);

            SelectCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanSelect))]
        private void Select()
        {
            CW.SendEvent(new BuildingMenuSelectIntent(_buildingId));
        }

        private bool CanSelect()
        {
            return IsAvailable;
        }
    }
}
