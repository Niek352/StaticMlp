using Aspid.MVVM;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    [ViewModel]
    public sealed partial class LoadoutPreparationViewModel
    {
        [OneWayBind] private string _summary;
        [OneWayBind] private bool _isAvailable;
        [OneWayBind] private bool _canPrepareBoss;
        [OneWayBind] private bool _isBossCommitted;
        [OneWayBind] private string _selectedPrimaryModuleName;
        [OneWayBind] private bool _poisonArrowSelected;
        [OneWayBind] private bool _fireFlaskSelected;
        [OneWayBind] private bool _poisonArrowInteractable;
        [OneWayBind] private bool _fireFlaskInteractable;
        [OneWayBind] private bool _confirmInteractable;

        public static string DescribeModule(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "None";
        }

        public void Sync(
            bool isAvailable,
            bool canPrepareBoss,
            bool isBossCommitted,
            LoadoutModuleId selectedPrimaryModuleId)
        {
            IsAvailable = isAvailable;
            CanPrepareBoss = canPrepareBoss;
            IsBossCommitted = isBossCommitted;
            SelectedPrimaryModuleName = DescribeModule(selectedPrimaryModuleId);
            PoisonArrowSelected = selectedPrimaryModuleId == LoadoutModuleCatalog.PoisonArrowModuleId;
            FireFlaskSelected = selectedPrimaryModuleId == LoadoutModuleCatalog.FireFlaskModuleId;
            PoisonArrowInteractable = !isBossCommitted;
            FireFlaskInteractable = !isBossCommitted;
            ConfirmInteractable = isAvailable && !isBossCommitted;
            Summary = CreateSummary();

            SelectPoisonArrowCommand.NotifyCanExecuteChanged();
            SelectFireFlaskCommand.NotifyCanExecuteChanged();
            ConfirmBuildCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanSelectPoisonArrow))]
        private void SelectPoisonArrow()
        {
            SelectModule(LoadoutModuleCatalog.PoisonArrowModuleId);
        }

        [RelayCommand(CanExecute = nameof(CanSelectFireFlask))]
        private void SelectFireFlask()
        {
            SelectModule(LoadoutModuleCatalog.FireFlaskModuleId);
        }

        [RelayCommand(CanExecute = nameof(CanConfirmBuild))]
        private void ConfirmBuild()
        {
            CW.SendEvent(new LoadoutPreparationConfirmIntent());
        }

        [RelayCommand]
        private void Close()
        {
            CW.SendEvent(new LoadoutPreparationCloseIntent());
        }

        private bool CanSelectPoisonArrow()
        {
            return PoisonArrowInteractable;
        }

        private bool CanSelectFireFlask()
        {
            return FireFlaskInteractable;
        }

        private bool CanConfirmBuild()
        {
            return ConfirmInteractable;
        }

        private static void SelectModule(LoadoutModuleId moduleId)
        {
            CW.SendEvent(new LoadoutPreparationSelectModuleIntent(moduleId));
        }

        private string CreateSummary()
        {
            return $"Build Preparation\n" +
                   $"Available: {IsAvailable}\n" +
                   $"Boss committed: {IsBossCommitted}\n" +
                   $"Boss preparation: {CanPrepareBoss}\n" +
                   $"Selected: {SelectedPrimaryModuleName}";
        }
    }
}
