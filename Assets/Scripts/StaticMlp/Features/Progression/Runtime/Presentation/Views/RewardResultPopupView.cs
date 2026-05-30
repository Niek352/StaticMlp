using Aspid.StaticEcs.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Progression
{
    public sealed class RewardResultPopupView : EcsWindowViewBase<RewardResultPopupSlot, RewardResultPopupViewModel>
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(RewardResultPopupView)} requires {nameof(panelRoot)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(RewardResultPopupView)} requires {nameof(closeButton)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(RewardResultPopupView)} requires {nameof(summaryLabel)}.");
        }

        protected override void OnViewModelBound(RewardResultPopupViewModel viewModel)
        {
            viewModel.Changed += Render;
            Render();
        }

        protected override void OnViewModelUnbound(RewardResultPopupViewModel viewModel)
        {
            viewModel.Changed -= Render;
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        private void Render()
        {
            var data = BoundViewModel.Data;
            Render(in data);
        }

        private void Render(in RewardResultPopupViewData data)
        {
            panelRoot.SetActive(true);
            summaryLabel.text =
                $"Reward: {RewardResultPopupViewModel.DescribeReward(data.RewardPackageId)}\n" +
                $"Wood: {data.GrantedWood}\n" +
                $"Stone: {data.GrantedStone}\n" +
                $"WarCache: {data.GrantsRecoveredWarCacheFlag}\n" +
                $"ThreatRaised: {data.ThreatRaised}";
        }

        private void HandleCloseClicked()
        {
            BoundViewModel.ClosePopup();
        }
    }
}
