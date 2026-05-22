using System;
using System.Text;
using Code.EcsUi.Mvc;
using StaticMlp.Features.Buildings;
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

        public void Render(in BuildingManagementPanelState state)
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

        private static string FormatConstructionResources(in BuildingManagementPanelState state)
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
    }
}
