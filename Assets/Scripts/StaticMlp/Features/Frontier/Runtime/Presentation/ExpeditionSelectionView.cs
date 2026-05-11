using System;
using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Frontier
{
    public sealed class ExpeditionSelectionView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button startButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        private Action _onStartClicked;
        private Action _onCloseClicked;

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

        public void Bind(Action onStartClicked, Action onCloseClicked)
        {
            _onStartClicked = onStartClicked ?? throw new ArgumentNullException(nameof(onStartClicked));
            _onCloseClicked = onCloseClicked ?? throw new ArgumentNullException(nameof(onCloseClicked));
        }

        public void Unbind()
        {
            _onStartClicked = null;
            _onCloseClicked = null;
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

        public void Render(in ExpeditionSelectionScreenState state)
        {
            panelRoot.SetActive(true);
            summaryLabel.text =
                $"Expedition Selection\n" +
                $"Destination: Nearby Raider Camp\n" +
                $"Availability: {state.AvailabilityStatus}\n" +
                $"Activity: {state.ActivityStatus}\n" +
                $"Prepared build: {ExpeditionSelectionController.DescribePreparedBuild(state.PreparedPrimaryModuleId)}\n" +
                $"Reward: Recovered War Cache\n" +
                $"Threat: {state.ThreatPhase}";
            startButton.interactable = state.CanStart;
        }

        private void HandleStartClicked() => _onStartClicked.Invoke();
        private void HandleCloseClicked() => _onCloseClicked.Invoke();
    }
}
