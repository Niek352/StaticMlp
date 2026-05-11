using System;
using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1ContextPanelView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TextMeshProUGUI primaryButtonLabel;
        [SerializeField] private TextMeshProUGUI secondaryButtonLabel;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        private Action _onPrimaryClicked;
        private Action _onSecondaryClicked;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(Stage1ContextPanelView)} requires {nameof(panelRoot)}.");
            if (primaryButton == null)
                throw new MissingReferenceException($"{nameof(Stage1ContextPanelView)} requires {nameof(primaryButton)}.");
            if (secondaryButton == null)
                throw new MissingReferenceException($"{nameof(Stage1ContextPanelView)} requires {nameof(secondaryButton)}.");
            if (primaryButtonLabel == null)
                throw new MissingReferenceException($"{nameof(Stage1ContextPanelView)} requires {nameof(primaryButtonLabel)}.");
            if (secondaryButtonLabel == null)
                throw new MissingReferenceException($"{nameof(Stage1ContextPanelView)} requires {nameof(secondaryButtonLabel)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(Stage1ContextPanelView)} requires {nameof(summaryLabel)}.");
        }

        public void Bind(Action onPrimaryClicked, Action onSecondaryClicked)
        {
            _onPrimaryClicked = onPrimaryClicked ?? throw new ArgumentNullException(nameof(onPrimaryClicked));
            _onSecondaryClicked = onSecondaryClicked ?? throw new ArgumentNullException(nameof(onSecondaryClicked));
        }

        public void Unbind()
        {
            _onPrimaryClicked = null;
            _onSecondaryClicked = null;
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

        public void Render(in Stage1ContextPanelState state)
        {
            panelRoot.SetActive(true);

            if (state.Mode == Stage1ContextPanelMode.Building)
            {
                var primaryActionAvailable = state.CanDepositResources || state.CanBuild;
                summaryLabel.text =
                    $"Building focus\n" +
                    $"Phase: {state.ConstructionPhase}\n" +
                    $"Wood: {state.WoodDelivered}/{state.WoodRequired}\n" +
                    $"Stone: {state.StoneDelivered}/{state.StoneRequired}\n" +
                    $"Progress: {Mathf.RoundToInt(state.Progress01 * 100f)}%";
                primaryButtonLabel.text = state.CanDepositResources ? "Deposit" : "Build";
                secondaryButtonLabel.text = "Inspect";
                primaryButton.interactable = primaryActionAvailable;
                secondaryButton.interactable = false;
                return;
            }

            summaryLabel.text =
                $"Worker focus\n" +
                $"Assigned: {state.WorkerAssigned}\n" +
                $"Task: {state.WorkerActiveTask}\n" +
                $"Blocked: {state.WorkerBlockingReason}";
            primaryButtonLabel.text = state.WorkerAssigned ? "Unassign Worker" : "Assign Worker";
            secondaryButtonLabel.text = "Refresh";
            primaryButton.interactable = state.CanToggleWorkerAssignment && state.HasWorker;
            secondaryButton.interactable = false;
        }

        private void HandlePrimaryClicked()
        {
            _onPrimaryClicked.Invoke();
        }

        private void HandleSecondaryClicked()
        {
            _onSecondaryClicked.Invoke();
        }
    }
}
