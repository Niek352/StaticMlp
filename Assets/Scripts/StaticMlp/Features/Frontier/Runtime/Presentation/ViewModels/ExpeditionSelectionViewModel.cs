using Aspid.MVVM;
using StaticMlp.Features.Loadout;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    [ViewModel]
    public sealed partial class ExpeditionSelectionViewModel
    {
        [OneWayBind] private string _summary;
        [OneWayBind] private bool _canStart;

        private ExpeditionSelectionScreenState _state;

        public static string DescribePreparedBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Apply(in ExpeditionSelectionViewData data)
        {
            _state = data.State;
            CanStart = _state.CanStart;
            Summary = BuildSummary(in _state);
            StartExpeditionCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanStartExpedition))]
        private void StartExpedition()
        {
            CW.SendEvent(new ExpeditionSelectionStartIntent(
                _state.AnchorId,
                _state.ExpeditionId,
                _state.BossId,
                _state.IsBossEncounterMode));
        }

        [RelayCommand]
        private void Close()
        {
            CW.SendEvent(new ExpeditionSelectionCloseIntent());
        }

        private bool CanStartExpedition()
        {
            return CanStart;
        }

        private static string BuildSummary(in ExpeditionSelectionScreenState state)
        {
            return state.IsBossEncounterMode
                ? $"Boss Encounter\n" +
                  $"Target: Raider Chief\n" +
                  $"Status: {state.BossStatus}\n" +
                  $"Prepared build: {DescribePreparedBuild(state.PreparedPrimaryModuleId)}\n" +
                  $"Threat: {state.ThreatPhase}"
                : $"Expedition Selection\n" +
                  $"Destination: Nearby Raider Camp\n" +
                  $"Availability: {state.AvailabilityStatus}\n" +
                  $"Activity: {state.ActivityStatus}\n" +
                  $"Prepared build: {DescribePreparedBuild(state.PreparedPrimaryModuleId)}\n" +
                  $"Reward: Recovered War Cache\n" +
                  $"Threat: {state.ThreatPhase}";
        }
    }
}
