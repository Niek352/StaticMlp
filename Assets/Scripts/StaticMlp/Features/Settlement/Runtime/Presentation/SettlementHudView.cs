using System;
using System.Text;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using Aspid.StaticEcs.Windows;
using StaticMlp.Features.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementHudView : EcsWindowViewBase<SettlementHudSlot, SettlementHudViewModel>
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button expeditionButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(SettlementHudView)} requires {nameof(panelRoot)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(SettlementHudView)} requires {nameof(closeButton)}.");
            if (buildButton == null)
                throw new MissingReferenceException($"{nameof(SettlementHudView)} requires {nameof(buildButton)}.");
            if (expeditionButton == null)
                throw new MissingReferenceException($"{nameof(SettlementHudView)} requires {nameof(expeditionButton)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(SettlementHudView)} requires {nameof(summaryLabel)}.");
        }

        protected override void OnViewModelBound(SettlementHudViewModel viewModel)
        {
            viewModel.Changed += Render;
        }

        protected override void OnViewModelUnbound(SettlementHudViewModel viewModel)
        {
            viewModel.Changed -= Render;
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
            buildButton.onClick.AddListener(HandleBuildClicked);
            expeditionButton.onClick.AddListener(HandleExpeditionClicked);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            buildButton.onClick.RemoveListener(HandleBuildClicked);
            expeditionButton.onClick.RemoveListener(HandleExpeditionClicked);
        }

        private void Render()
        {
            var viewModel = BoundViewModel;
            Render(
                in viewModel.Settlement,
                in viewModel.Expedition,
                in viewModel.Loadout,
                in viewModel.Threat,
                in viewModel.Raid,
                in viewModel.Boss,
                in viewModel.Progression);
        }

        private void Render(
            in SettlementHudState settlement,
            in ExpeditionHudState expedition,
            in LoadoutHudState loadout,
            in ThreatHudState threat,
            in RaidHudState raid,
            in BossHudState boss,
            in ProgressionHudState progression)
        {
            panelRoot.SetActive(true);
            var hintLine = string.IsNullOrEmpty(settlement.HintDisplayName)
                ? string.Empty
                : $"\nHint: {settlement.HintDisplayName}";
            summaryLabel.text =
                $"Objective: {settlement.ObjectiveDisplayName}\n" +
                $"Actions: {FormatActionStatus(in settlement.LoadoutPreparationAction)} / {FormatActionStatus(in settlement.ExpeditionSelectionAction)}\n" +
                $"Camp stage: {settlement.SettlementStage}\n" +
                $"Resources: {FormatResources(in settlement)}\n" +
                $"Workers: {settlement.AssignedWorkers}/{settlement.TotalWorkers} assigned\n" +
                $"Prepared build: {SettlementHudViewModel.DescribeBuild(loadout.PreparedPrimaryModuleId)}\n" +
                $"Expedition: {expedition.Availability} / {expedition.Activity}\n" +
                $"Threat: {threat.ThreatPhase} / Raid {raid.Status}\n" +
                $"Boss flags: Unlocked={progression.HasBossUnlocked} Tokens={progression.BossPreparationTokens}" +
                hintLine;

            buildButton.interactable = settlement.LoadoutPreparationAction.Enabled;
            expeditionButton.interactable = settlement.ExpeditionSelectionAction.Enabled;
        }

        private void HandleCloseClicked()
        {
            BoundViewModel.CloseHud();
        }

        private void HandleBuildClicked()
        {
            BoundViewModel.OpenLoadoutPreparation();
        }

        private void HandleExpeditionClicked()
        {
            BoundViewModel.OpenExpeditionSelection();
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
                builder.Append(ResourceCatalog.Get(resource.Id).DisplayName);
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
