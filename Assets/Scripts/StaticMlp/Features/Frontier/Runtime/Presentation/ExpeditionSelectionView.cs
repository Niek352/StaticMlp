using System;
using Aspid.StaticEcs.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Frontier
{
    public sealed class ExpeditionSelectionView : EcsWindowViewBase<ExpeditionSelectionSlot, ExpeditionSelectionViewModel>
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button startButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(ExpeditionSelectionView)} requires {nameof(panelRoot)}.");
            if (startButton == null)
                throw new MissingReferenceException($"{nameof(ExpeditionSelectionView)} requires {nameof(startButton)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(ExpeditionSelectionView)} requires {nameof(closeButton)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(ExpeditionSelectionView)} requires {nameof(summaryLabel)}.");
        }

        protected override void OnViewModelBound(ExpeditionSelectionViewModel viewModel)
        {
            viewModel.Changed += Render;
            Render();
        }

        protected override void OnViewModelUnbound(ExpeditionSelectionViewModel viewModel)
        {
            viewModel.Changed -= Render;
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(HandleStartClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(HandleStartClicked);
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        private void Render()
        {
            var state = BoundViewModel.State;
            Render(in state);
        }

        private void Render(in ExpeditionSelectionScreenState state)
        {
            panelRoot.SetActive(true);
            summaryLabel.text = state.IsBossEncounterMode
                ? $"Boss Encounter\n" +
                  $"Target: Raider Chief\n" +
                  $"Status: {state.BossStatus}\n" +
                  $"Prepared build: {ExpeditionSelectionViewModel.DescribePreparedBuild(state.PreparedPrimaryModuleId)}\n" +
                  $"Threat: {state.ThreatPhase}"
                : $"Expedition Selection\n" +
                  $"Destination: Nearby Raider Camp\n" +
                  $"Availability: {state.AvailabilityStatus}\n" +
                  $"Activity: {state.ActivityStatus}\n" +
                  $"Prepared build: {ExpeditionSelectionViewModel.DescribePreparedBuild(state.PreparedPrimaryModuleId)}\n" +
                  $"Reward: Recovered War Cache\n" +
                  $"Threat: {state.ThreatPhase}";
            startButton.interactable = state.CanStart;
        }

        private void HandleStartClicked() => BoundViewModel.StartExpedition();
        private void HandleCloseClicked() => BoundViewModel.Close();
    }
}
