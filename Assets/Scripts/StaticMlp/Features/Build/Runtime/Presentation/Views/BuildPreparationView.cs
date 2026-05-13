using System;
using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Build
{
    public sealed class BuildPreparationView : PrefabViewBase
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button poisonArrowButton;
        [SerializeField] private Button fireFlaskButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        private Action _onPoisonArrowClicked;
        private Action _onFireFlaskClicked;
        private Action _onConfirmClicked;
        private Action _onCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(BuildPreparationView)} requires {nameof(panelRoot)}.");
            if (poisonArrowButton == null)
                throw new MissingReferenceException($"{nameof(BuildPreparationView)} requires {nameof(poisonArrowButton)}.");
            if (fireFlaskButton == null)
                throw new MissingReferenceException($"{nameof(BuildPreparationView)} requires {nameof(fireFlaskButton)}.");
            if (confirmButton == null)
                throw new MissingReferenceException($"{nameof(BuildPreparationView)} requires {nameof(confirmButton)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(BuildPreparationView)} requires {nameof(closeButton)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(BuildPreparationView)} requires {nameof(summaryLabel)}.");
        }

        public void Bind(
            Action onPoisonArrowClicked,
            Action onFireFlaskClicked,
            Action onConfirmClicked,
            Action onCloseClicked)
        {
            _onPoisonArrowClicked = onPoisonArrowClicked ?? throw new ArgumentNullException(nameof(onPoisonArrowClicked));
            _onFireFlaskClicked = onFireFlaskClicked ?? throw new ArgumentNullException(nameof(onFireFlaskClicked));
            _onConfirmClicked = onConfirmClicked ?? throw new ArgumentNullException(nameof(onConfirmClicked));
            _onCloseClicked = onCloseClicked ?? throw new ArgumentNullException(nameof(onCloseClicked));
        }

        public void Unbind()
        {
            _onPoisonArrowClicked = null;
            _onFireFlaskClicked = null;
            _onConfirmClicked = null;
            _onCloseClicked = null;
        }

        private void OnEnable()
        {
            poisonArrowButton.onClick.AddListener(HandlePoisonArrowClicked);
            fireFlaskButton.onClick.AddListener(HandleFireFlaskClicked);
            confirmButton.onClick.AddListener(HandleConfirmClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            poisonArrowButton.onClick.RemoveListener(HandlePoisonArrowClicked);
            fireFlaskButton.onClick.RemoveListener(HandleFireFlaskClicked);
            confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public void Render(in BuildPreparationScreenState state)
        {
            panelRoot.SetActive(true);
            summaryLabel.text =
                $"Build Preparation\n" +
                $"Available: {state.IsAvailable}\n" +
                $"Boss committed: {state.IsBossCommitted}\n" +
                $"Boss preparation: {state.CanPrepareBoss}\n" +
                $"Selected: {BuildPreparationController.DescribeModule(state.SelectedPrimaryModuleId)}";

            poisonArrowButton.interactable = state.PoisonArrowAvailable && !state.IsBossCommitted;
            fireFlaskButton.interactable = state.FireFlaskAvailable && !state.IsBossCommitted;
            confirmButton.interactable = state.CanConfirm;
        }

        private void HandlePoisonArrowClicked() => _onPoisonArrowClicked.Invoke();
        private void HandleFireFlaskClicked() => _onFireFlaskClicked.Invoke();
        private void HandleConfirmClicked() => _onConfirmClicked.Invoke();
        private void HandleCloseClicked() => _onCloseClicked.Invoke();
    }
}
