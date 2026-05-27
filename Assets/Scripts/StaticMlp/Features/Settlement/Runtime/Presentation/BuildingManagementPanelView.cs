using System;
using System.Text;
using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Settlement
{
    public sealed class BuildingManagementPanelView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI primaryButtonLabel;
        [SerializeField] private TextMeshProUGUI secondaryButtonLabel;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        private Action _onPrimaryClicked;
        private Action _onSecondaryClicked;
        private Action _onCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(BuildingManagementPanelView)} requires {nameof(panelRoot)}.");
            if (primaryButton == null)
                throw new MissingReferenceException($"{nameof(BuildingManagementPanelView)} requires {nameof(primaryButton)}.");
            if (secondaryButton == null)
                throw new MissingReferenceException($"{nameof(BuildingManagementPanelView)} requires {nameof(secondaryButton)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(BuildingManagementPanelView)} requires {nameof(closeButton)}.");
            if (primaryButtonLabel == null)
                throw new MissingReferenceException($"{nameof(BuildingManagementPanelView)} requires {nameof(primaryButtonLabel)}.");
            if (secondaryButtonLabel == null)
                throw new MissingReferenceException($"{nameof(BuildingManagementPanelView)} requires {nameof(secondaryButtonLabel)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(BuildingManagementPanelView)} requires {nameof(summaryLabel)}.");
        }

        public void Bind(Action onPrimaryClicked, Action onSecondaryClicked, Action onCloseClicked)
        {
            _onPrimaryClicked = onPrimaryClicked ?? throw new ArgumentNullException(nameof(onPrimaryClicked));
            _onSecondaryClicked = onSecondaryClicked ?? throw new ArgumentNullException(nameof(onSecondaryClicked));
            _onCloseClicked = onCloseClicked ?? throw new ArgumentNullException(nameof(onCloseClicked));
        }

        public void Unbind()
        {
            _onPrimaryClicked = null;
            _onSecondaryClicked = null;
            _onCloseClicked = null;
        }

        private void OnEnable()
        {
            primaryButton.onClick.AddListener(HandlePrimaryClicked);
            secondaryButton.onClick.AddListener(HandleSecondaryClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            primaryButton.onClick.RemoveListener(HandlePrimaryClicked);
            secondaryButton.onClick.RemoveListener(HandleSecondaryClicked);
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public void Render(in BuildingPanelState state)
        {
            panelRoot.SetActive(true);

            RenderAction(primaryButton, primaryButtonLabel, in state.PrimaryAction);
            RenderAction(secondaryButton, secondaryButtonLabel, in state.SecondaryAction);

            summaryLabel.text = BuildSummary(in state);
        }

        private static void RenderAction(Button button, TextMeshProUGUI label, in BuildingPanelAction action)
        {
            button.gameObject.SetActive(action.IsDefined);
            if (!action.IsDefined)
                return;

            button.interactable = action.Enabled;
            label.text = action.Label;
        }

        private static string BuildSummary(in BuildingPanelState state)
        {
            var builder = new StringBuilder();
            switch (state.Kind)
            {
                case BuildingPanelKind.ConstructionSitePanel:
                    AppendConstruction(builder, in state.Construction);
                    AppendActionStatus(builder, in state.PrimaryAction);
                    AppendActionStatus(builder, in state.SecondaryAction);
                    break;
                case BuildingPanelKind.StockpilePanel:
                    AppendStockpile(builder, in state.Stockpile);
                    AppendActionStatus(builder, in state.PrimaryAction);
                    break;
                case BuildingPanelKind.ExtractionPanel:
                    AppendExtraction(builder, in state.Extraction);
                    AppendActionStatus(builder, in state.PrimaryAction);
                    AppendActionStatus(builder, in state.SecondaryAction);
                    break;
                case BuildingPanelKind.WorkbenchPanel:
                    AppendWorkbench(builder, in state.Workbench);
                    AppendActionStatus(builder, in state.PrimaryAction);
                    AppendActionStatus(builder, in state.SecondaryAction);
                    break;
                case BuildingPanelKind.ShelterPanel:
                    AppendShelter(builder, in state.Shelter);
                    break;
                case BuildingPanelKind.CampCorePanel:
                    AppendCampCore(builder, in state.CampCore);
                    break;
                default:
                    builder.Append("No building selected.");
                    break;
            }

            return builder.ToString();
        }

        private static void AppendConstruction(StringBuilder builder, in ConstructionPanelState state)
        {
            builder.Append(state.DisplayName);
            builder.Append("\nConstruction Site");
            builder.Append("\nPhase: ");
            builder.Append(state.Phase);
            builder.Append("\nResources: ");
            builder.Append(FormatConstructionResources(in state));
            builder.Append("\nProgress: ");
            builder.Append(Mathf.RoundToInt(state.Progress01 * 100f));
            builder.Append('%');
        }

        private static void AppendStockpile(StringBuilder builder, in StockpilePanelState state)
        {
            builder.Append(state.DisplayName);
            builder.Append("\nStockpile");
            builder.Append("\nUsed: ");
            builder.Append(state.UsedCapacity);
            builder.Append(" / ");
            builder.Append(state.Capacity);
            builder.Append("\nBuilding capacity: ");
            builder.Append(state.ContributedCapacity);

            if (state.Resources.Length == 0)
            {
                builder.Append("\nResources: empty");
                return;
            }

            builder.Append("\nResources: ");
            for (var i = 0; i < state.Resources.Length; i++)
            {
                if (i > 0)
                    builder.Append(" / ");

                var resource = state.Resources[i];
                builder.Append(ResourceCatalog.Get(resource.Id).DisplayName);
                builder.Append(' ');
                builder.Append(resource.Amount);
            }
        }

        private static void AppendExtraction(StringBuilder builder, in ExtractionPanelState state)
        {
            builder.Append(state.DisplayName);
            builder.Append("\nExtraction");
            builder.Append("\nOutput: ");
            builder.Append(ResourceCatalog.Get(state.OutputResource).DisplayName);
            builder.Append("\nBuffer: ");
            builder.Append(state.BufferAmount);
            builder.Append(" / ");
            builder.Append(state.BufferCapacity);
            builder.Append("\nWorkers: ");
            builder.Append(state.AssignedWorkerCount);
            builder.Append(" / ");
            builder.Append(state.WorkerSlotCount);

            for (var i = 0; i < state.WorkerSlots.Length; i++)
            {
                var slot = state.WorkerSlots[i];
                builder.Append("\nSlot ");
                builder.Append(slot.SlotIndex + 1);
                builder.Append(": ");
                builder.Append(slot.Assigned ? $"Worker {slot.Worker.Raw}" : "Empty");
            }
        }

        private static void AppendWorkbench(StringBuilder builder, in WorkbenchPanelState state)
        {
            builder.Append(state.DisplayName);
            builder.Append("\nWorkbench");
            builder.Append("\nStatus: ");
            builder.Append(state.Enabled ? "Enabled" : "Disabled");
            builder.Append("\nRecipe: ");
            builder.Append(state.RecipeName);
            builder.Append("\nWork: ");
            builder.Append(Mathf.RoundToInt(state.WorkDone));
            builder.Append(" / ");
            builder.Append(Mathf.RoundToInt(state.WorkRequired));
            builder.Append("\nOutput buffer: ");
            builder.Append(state.OutputAmount);
            builder.Append(" / ");
            builder.Append(state.OutputCapacity);
            builder.Append("\nWorkers: ");
            builder.Append(state.AssignedWorkerCount);
            builder.Append(" / ");
            builder.Append(state.WorkerSlotCount);

            for (var i = 0; i < state.WorkerSlots.Length; i++)
            {
                var slot = state.WorkerSlots[i];
                builder.Append("\nSlot ");
                builder.Append(slot.SlotIndex + 1);
                builder.Append(": ");
                builder.Append(slot.Assigned ? $"Worker {slot.Worker.Raw}" : "Empty");
            }

            AppendProductionBuffer(builder, "Inputs", in state.Inputs);
            AppendProductionBuffer(builder, "Outputs", in state.Outputs);
        }

        private static void AppendShelter(StringBuilder builder, in ShelterPanelState state)
        {
            builder.Append(state.DisplayName);
            builder.Append("\nShelter");
            builder.Append("\nBeds: ");
            builder.Append(state.FreeSlots);
            builder.Append(" free / ");
            builder.Append(state.SlotCount);
            builder.Append("\nStatus: ");
            builder.Append(state.Enabled ? "Enabled" : "Disabled");
            builder.Append("\nBed and rest actions are unavailable until server requests exist.");
        }

        private static void AppendCampCore(StringBuilder builder, in CampCorePanelState state)
        {
            builder.Append(state.DisplayName);
            builder.Append("\nCamp Core");
            builder.Append("\nPhase: ");
            builder.Append(state.Phase);
            builder.Append("\nProgress: ");
            builder.Append(Mathf.RoundToInt(state.Progress01 * 100f));
            builder.Append('%');
        }

        private void HandlePrimaryClicked()
        {
            _onPrimaryClicked.Invoke();
        }

        private void HandleSecondaryClicked()
        {
            _onSecondaryClicked.Invoke();
        }

        private void HandleCloseClicked()
        {
            _onCloseClicked.Invoke();
        }

        private static string FormatConstructionResources(in ConstructionPanelState state)
        {
            if (state.Resources.Length == 0)
                return "none";

            var builder = new StringBuilder();
            for (var i = 0; i < state.Resources.Length; i++)
            {
                if (i > 0)
                    builder.Append(" / ");

                var resource = state.Resources[i];
                builder.Append(ResourceCatalog.Get(resource.Id).DisplayName);
                builder.Append(' ');
                builder.Append(resource.Delivered);
                builder.Append(" / ");
                builder.Append(resource.Required);
            }

            return builder.ToString();
        }

        private static void AppendProductionBuffer(
            StringBuilder builder,
            string label,
            in Unity.Collections.FixedList128Bytes<ProductionResourceBufferEntry> amounts)
        {
            builder.Append('\n');
            builder.Append(label);
            builder.Append(": ");
            if (amounts.Length == 0)
            {
                builder.Append("none");
                return;
            }

            for (var i = 0; i < amounts.Length; i++)
            {
                if (i > 0)
                    builder.Append(" / ");

                var amount = amounts[i];
                builder.Append(ResourceCatalog.Get(amount.Id).DisplayName);
                builder.Append(' ');
                builder.Append(amount.Amount);
                builder.Append(" / ");
                builder.Append(amount.BatchAmount);
            }
        }

        private static void AppendActionStatus(StringBuilder builder, in BuildingPanelAction action)
        {
            if (!action.IsDefined)
                return;

            builder.Append('\n');
            builder.Append(action.Label);
            builder.Append(action.Enabled ? " (ready)" : " (locked)");

            if (!action.Enabled && !string.IsNullOrEmpty(action.DisabledReason))
            {
                builder.Append("\nBlocked: ");
                builder.Append(action.DisabledReason);
            }
        }
    }
}
