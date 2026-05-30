using Aspid.StaticEcs.Windows;
using TMPro;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public sealed class ThreatBannerView : EcsWindowViewBase<ThreatBannerSlot, ThreatBannerViewModel>
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(ThreatBannerView)} requires {nameof(panelRoot)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(ThreatBannerView)} requires {nameof(summaryLabel)}.");
        }

        protected override void OnViewModelBound(ThreatBannerViewModel viewModel)
        {
            viewModel.Changed += Render;
            Render();
        }

        protected override void OnViewModelUnbound(ThreatBannerViewModel viewModel)
        {
            viewModel.Changed -= Render;
        }

        private void Render()
        {
            var state = BoundViewModel.State;
            Render(in state);
        }

        private void Render(in ThreatBannerState state)
        {
            panelRoot.SetActive(state.IsVisible);
            summaryLabel.text =
                $"Threat: {state.Phase} | Raid: {state.RaidStatus} | ActivateAtTick: {state.ActivateAtTick}";
        }
    }
}
