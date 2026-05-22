using System;
using System.Text;
using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1HudView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button expeditionButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        private Action _onCloseClicked;
        private Action _onBuildClicked;
        private Action _onExpeditionClicked;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(Stage1HudView)} requires {nameof(panelRoot)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(Stage1HudView)} requires {nameof(closeButton)}.");
            if (buildButton == null)
                throw new MissingReferenceException($"{nameof(Stage1HudView)} requires {nameof(buildButton)}.");
            if (expeditionButton == null)
                throw new MissingReferenceException($"{nameof(Stage1HudView)} requires {nameof(expeditionButton)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(Stage1HudView)} requires {nameof(summaryLabel)}.");
        }

        public void Bind(Action onBuildClicked, Action onExpeditionClicked, Action onCloseClicked)
        {
            _onBuildClicked = onBuildClicked ?? throw new ArgumentNullException(nameof(onBuildClicked));
            _onExpeditionClicked = onExpeditionClicked ?? throw new ArgumentNullException(nameof(onExpeditionClicked));
            _onCloseClicked = onCloseClicked ?? throw new ArgumentNullException(nameof(onCloseClicked));
        }

        public void Unbind()
        {
            _onCloseClicked = null;
            _onBuildClicked = null;
            _onExpeditionClicked = null;
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

        public void Render(in Stage1HudState state)
        {
            panelRoot.SetActive(true);
            var hintLine = string.IsNullOrEmpty(state.ObjectiveHint)
                ? string.Empty
                : $"\nHint: {state.ObjectiveHint}";
            summaryLabel.text =
                $"Objective: {Stage1HudController.DescribeObjective(state.Objective)}\n" +
                $"Camp stage: {state.SettlementStage}\n" +
                $"Resources: {FormatResources(in state)}\n" +
                $"Workers: {state.AssignedWorkers}/{state.TotalWorkers} assigned\n" +
                $"Prepared build: {Stage1HudController.DescribeBuild(state.PreparedPrimaryModuleId)}\n" +
                $"Expedition: {state.ExpeditionAvailability} / {state.ExpeditionActivity}\n" +
                $"Threat: {state.ThreatPhase} / Raid {state.RaidScheduleStatus}\n" +
                $"Boss flags: Unlocked={state.HasBossUnlocked} Tokens={state.BossPreparationTokens}" +
                hintLine;

            buildButton.interactable = state.CanOpenLoadoutPreparation;
            expeditionButton.interactable = state.CanOpenExpeditionSelection;
        }

        private void HandleCloseClicked()
        {
            _onCloseClicked.Invoke();
        }

        private void HandleBuildClicked()
        {
            _onBuildClicked.Invoke();
        }

        private void HandleExpeditionClicked()
        {
            _onExpeditionClicked.Invoke();
        }

        private static string FormatResources(in Stage1HudState state)
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
    }
}
