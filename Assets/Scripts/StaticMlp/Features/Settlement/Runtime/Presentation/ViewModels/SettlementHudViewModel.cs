using System.Text;
using Aspid.MVVM;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    [ViewModel]
    public sealed partial class SettlementHudViewModel
    {
        [OneWayBind] private string _summary;
        [OneWayBind] private bool _canOpenLoadoutPreparation;
        [OneWayBind] private bool _canOpenExpeditionSelection;

        public static string DescribeBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Apply(in SettlementHudViewData data)
        {
            CanOpenLoadoutPreparation = data.Settlement.LoadoutPreparationAction.Enabled;
            CanOpenExpeditionSelection = data.Settlement.ExpeditionSelectionAction.Enabled;
            Summary = BuildSummary(in data);
            OpenLoadoutPreparationCommand.NotifyCanExecuteChanged();
            OpenExpeditionSelectionCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanOpenLoadout))]
        private void OpenLoadoutPreparation()
        {
            CW.SendEvent(new SettlementHudOpenLoadoutPreparationIntent());
        }

        [RelayCommand(CanExecute = nameof(CanOpenExpedition))]
        private void OpenExpeditionSelection()
        {
            CW.SendEvent(new SettlementHudOpenExpeditionSelectionIntent());
        }

        [RelayCommand]
        private void CloseHud()
        {
            CW.SendEvent(new SettlementHudCloseIntent());
        }

        private bool CanOpenLoadout()
        {
            return CanOpenLoadoutPreparation;
        }

        private bool CanOpenExpedition()
        {
            return CanOpenExpeditionSelection;
        }

        private static string BuildSummary(in SettlementHudViewData data)
        {
            var settlement = data.Settlement;
            var hintLine = string.IsNullOrEmpty(settlement.HintDisplayName)
                ? string.Empty
                : $"\nHint: {settlement.HintDisplayName}";

            return
                $"Objective: {settlement.ObjectiveDisplayName}\n" +
                $"Actions: {FormatActionStatus(in settlement.LoadoutPreparationAction)} / {FormatActionStatus(in settlement.ExpeditionSelectionAction)}\n" +
                $"Camp stage: {settlement.SettlementStage}\n" +
                $"Resources: {FormatResources(in settlement)}\n" +
                $"Workers: {settlement.AssignedWorkers}/{settlement.TotalWorkers} assigned\n" +
                $"Prepared build: {DescribeBuild(data.Loadout.PreparedPrimaryModuleId)}\n" +
                $"Expedition: {data.Expedition.Availability} / {data.Expedition.Activity}\n" +
                $"Threat: {data.Threat.ThreatPhase} / Raid {data.Raid.Status}\n" +
                $"Boss flags: Unlocked={data.Progression.HasBossUnlocked} Tokens={data.Progression.BossPreparationTokens}" +
                hintLine;
        }

        private static string FormatResources(in SettlementHudState state)
        {
            if (state.Resources.Length == 0)
                return string.Empty;

            var builder = new StringBuilder();
            for (var i = 0; i < state.Resources.Length; i++)
            {
                if (i > 0)
                    builder.Append(" / ");

                var resource = state.Resources[i];
                builder.Append("Resource ");
                builder.Append(resource.Id.Value);
                builder.Append(' ');
                builder.Append(resource.Amount);
            }

            return builder.ToString();
        }

        private static string FormatActionStatus(in HudActionPresentation action)
        {
            var label = string.IsNullOrEmpty(action.Label) ? "Action" : action.Label;
            if (action.Enabled)
                return $"{label}: ready -> {action.EffectDescription}";

            return string.IsNullOrEmpty(action.DisabledReason)
                ? $"{label}: locked"
                : $"{label}: locked - {action.DisabledReason}";
        }
    }
}
