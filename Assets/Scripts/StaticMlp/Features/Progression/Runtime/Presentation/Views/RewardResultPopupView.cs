using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Progression
{
    public sealed class RewardResultPopupView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        private System.Action _onCloseClicked;

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

        public void Bind(System.Action onCloseClicked)
        {
            _onCloseClicked = onCloseClicked ?? throw new System.ArgumentNullException(nameof(onCloseClicked));
        }

        public void Unbind()
        {
            _onCloseClicked = null;
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public void Render(in RewardResultPopupViewData data)
        {
            panelRoot.SetActive(true);
            summaryLabel.text =
                $"Reward: {RewardResultPopupController.DescribeReward(data.RewardPackageId)}\n" +
                $"Wood: {data.GrantedWood}\n" +
                $"Stone: {data.GrantedStone}\n" +
                $"WarCache: {data.GrantsRecoveredWarCacheFlag}\n" +
                $"ThreatRaised: {data.ThreatRaised}";
        }

        private void HandleCloseClicked()
        {
            _onCloseClicked.Invoke();
        }
    }
}
