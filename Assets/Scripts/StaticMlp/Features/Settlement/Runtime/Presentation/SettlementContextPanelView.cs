using System;
using System.Text;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using Aspid.StaticEcs.Windows;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementContextPanelView : EcsWindowViewBase<SettlementContextPanelSlot, SettlementContextPanelViewModel>
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TextMeshProUGUI primaryButtonLabel;
        [SerializeField] private TextMeshProUGUI secondaryButtonLabel;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(SettlementContextPanelView)} requires {nameof(panelRoot)}.");
            if (primaryButton == null)
                throw new MissingReferenceException($"{nameof(SettlementContextPanelView)} requires {nameof(primaryButton)}.");
            if (secondaryButton == null)
                throw new MissingReferenceException($"{nameof(SettlementContextPanelView)} requires {nameof(secondaryButton)}.");
            if (primaryButtonLabel == null)
                throw new MissingReferenceException($"{nameof(SettlementContextPanelView)} requires {nameof(primaryButtonLabel)}.");
            if (secondaryButtonLabel == null)
                throw new MissingReferenceException($"{nameof(SettlementContextPanelView)} requires {nameof(secondaryButtonLabel)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(SettlementContextPanelView)} requires {nameof(summaryLabel)}.");
        }

        protected override void OnViewModelBound(SettlementContextPanelViewModel viewModel)
        {
            viewModel.Changed += Render;
        }

        protected override void OnViewModelUnbound(SettlementContextPanelViewModel viewModel)
        {
            viewModel.Changed -= Render;
        }

        private void OnEnable()
        {
            primaryButton.onClick.AddListener(HandlePrimaryClicked);
            secondaryButton.onClick.AddListener(HandleSecondaryClicked);
        }

        private void OnDisable()
        {
            primaryButton.onClick.RemoveListener(HandlePrimaryClicked);
            secondaryButton.onClick.RemoveListener(HandleSecondaryClicked);
        }

        private void Render()
        {
            var viewModel = BoundViewModel;
            Render(in viewModel.Building, in viewModel.Worker, viewModel.Mode);
        }

        private void Render(
            in BuildingContextPanelState building,
            in WorkerContextPanelState worker,
            SettlementContextPanelMode mode)
        {
            if (mode == SettlementContextPanelMode.Building)
                RenderBuilding(in building);
            else
                RenderWorker(in worker);
        }


        private void RenderBuilding(in BuildingContextPanelState state)
        {
            panelRoot.SetActive(true);
            primaryButton.gameObject.SetActive(true);
            secondaryButton.gameObject.SetActive(true);

            primaryButton.interactable = state.PrimaryBuildingAction.Enabled;
            secondaryButton.interactable = state.SecondaryBuildingAction.Enabled;
            primaryButtonLabel.text = state.PrimaryBuildingAction.Label;
            secondaryButtonLabel.text = state.SecondaryBuildingAction.Label;

            var summaryBuilder = new StringBuilder();
            summaryBuilder.Append(state.BuildingDisplayName);
            summaryBuilder.Append("\nPhase: ");
            summaryBuilder.Append(state.ConstructionPhase);

            if (state.ConstructionResources.Length > 0)
            {
                summaryBuilder.Append("\nResources: ");
                summaryBuilder.Append(FormatConstructionResources(in state));
            }

            summaryBuilder.Append("\nProgress: ");
            summaryBuilder.Append(Mathf.RoundToInt(state.Progress01 * 100f));
            summaryBuilder.Append('%');

            AppendActionStatus(summaryBuilder, "Primary", in state.PrimaryBuildingAction);
            AppendActionStatus(summaryBuilder, "Secondary", in state.SecondaryBuildingAction);

            if (state.HasOpenedBuildingAction)
            {
                summaryBuilder.Append("\n\n");
                summaryBuilder.Append(state.OpenedBuildingActionLabel);
                summaryBuilder.Append("\n");
                summaryBuilder.Append(state.OpenedBuildingActionSummary);
            }

            summaryLabel.text = summaryBuilder.ToString();
        }

        private void RenderWorker(in WorkerContextPanelState state)
        {
            panelRoot.SetActive(true);
            primaryButton.gameObject.SetActive(true);
            secondaryButton.gameObject.SetActive(false);

            if (!state.HasWorker)
            {
                primaryButton.interactable = false;
                primaryButtonLabel.text = "No worker";
                summaryLabel.text = "No worker available.";
                return;
            }

            primaryButton.interactable = state.CanToggleWorkerAssignment;
            primaryButtonLabel.text = state.WorkerAssigned ? "Unassign" : "Assign";

            var summaryBuilder = new StringBuilder();
            var assignedCount = 0;
            for (var i = 0; i < state.Workers.Length; i++)
            {
                if (state.Workers[i].IsAssigned)
                    assignedCount++;
            }

            summaryBuilder.Append("Workers: ");
            summaryBuilder.Append(assignedCount);
            summaryBuilder.Append(" / ");
            summaryBuilder.Append(state.Workers.Length);
            summaryBuilder.Append(" assigned\n");

            for (var i = 0; i < state.Workers.Length; i++)
            {
                var worker = state.Workers[i];
                var roleName = WorkerRoleCatalog.TryGet(worker.Role, out var roleDef)
                    ? roleDef.AllowedJobs.ToString()
                    : $"Role {worker.Role.Value}";
                summaryBuilder.Append(roleName);
                summaryBuilder.Append(": ");

                if (worker.IsBuildingAssignment && worker.Building.Raw != 0)
                {
                    var buildingName = ResolveBuildingDisplayName(worker.Building);
                    summaryBuilder.Append(buildingName);
                    summaryBuilder.Append(" #");
                    summaryBuilder.Append(worker.SlotIndex + 1);
                }
                else if (worker.IsAssigned)
                {
                    summaryBuilder.Append("Camp Builder");
                }
                else
                {
                    summaryBuilder.Append("Unassigned");
                }

                if (worker.BlockingReason != SettlementWorkerBlockingReason.None)
                {
                    summaryBuilder.Append(" [");
                    summaryBuilder.Append(worker.BlockingReason);
                    summaryBuilder.Append("]");
                }
                summaryBuilder.Append("\n");
            }

            summaryBuilder.Append("\nPrimary: ");
            summaryBuilder.Append(state.WorkerAssigned ? "Unassign" : "Assign");
            summaryBuilder.Append(state.CanToggleWorkerAssignment ? " (ready)" : " (locked)");

            if (state.WorkerBlockingReason != SettlementWorkerBlockingReason.None)
            {
                summaryBuilder.Append("\n");
                summaryBuilder.Append(FormatWorkerBlockingReason(state.WorkerBlockingReason));
            }

            summaryLabel.text = summaryBuilder.ToString();
        }

        private void HandlePrimaryClicked()
        {
            BoundViewModel.HandlePrimaryAction();
        }

        private void HandleSecondaryClicked()
        {
            BoundViewModel.HandleSecondaryAction();
        }

        private static string FormatConstructionResources(in BuildingContextPanelState state)
        {
            if (state.ConstructionResources.Length == 0)
                return string.Empty;

            var builder = new StringBuilder();
            for (var i = 0; i < state.ConstructionResources.Length; i++)
            {
                if (i > 0)
                    builder.Append(" / ");

                var resource = state.ConstructionResources[i];
                builder.Append(ResourceCatalog.Get(resource.Id).DisplayName);
                builder.Append(' ');
                builder.Append(resource.Delivered);
                builder.Append(" / ");
                builder.Append(resource.Required);
            }

            return builder.ToString();
        }

        private static void AppendActionStatus(
            StringBuilder builder,
            string slot,
            in BuildingAvailableActionPresentation action)
        {
            builder.Append('\n');
            builder.Append(slot);
            builder.Append(": ");
            builder.Append(action.Label);
            builder.Append(action.Enabled ? " (ready)" : " (locked)");

            if (!string.IsNullOrEmpty(action.InputHint))
            {
                builder.Append(" via ");
                builder.Append(action.InputHint);
            }

            if (!string.IsNullOrEmpty(action.EffectDescription))
            {
                builder.Append("\nEffect: ");
                builder.Append(action.EffectDescription);
            }

            if (!action.Enabled && !string.IsNullOrEmpty(action.DisabledReason))
            {
                builder.Append("\nBlocked: ");
                builder.Append(action.DisabledReason);
            }
        }

        private static string FormatWorkerBlockingReason(SettlementWorkerBlockingReason reason)
        {
            return reason.ToString();
        }

        private static string ResolveBuildingDisplayName(EntityGID buildingGid)
        {
            if (!buildingGid.TryUnpack<ClientCoreWT>(out var building))
                return "Building";

            if (!building.Has<ConstructionSiteState>())
                return "Building";

            ref readonly var site = ref building.Read<ConstructionSiteState>();
            var definition = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
            return definition.DisplayName;
        }
    }
}
