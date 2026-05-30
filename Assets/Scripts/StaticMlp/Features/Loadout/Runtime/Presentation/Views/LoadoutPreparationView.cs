using Aspid.StaticEcs.Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Loadout
{
    public sealed class LoadoutPreparationView : EcsWindowViewBase<LoadoutPreparationSlot, LoadoutPreparationViewModel>
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button poisonArrowButton;
        [SerializeField] private Button fireFlaskButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI summaryLabel;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(LoadoutPreparationView)} requires {nameof(panelRoot)}.");
            if (poisonArrowButton == null)
                throw new MissingReferenceException($"{nameof(LoadoutPreparationView)} requires {nameof(poisonArrowButton)}.");
            if (fireFlaskButton == null)
                throw new MissingReferenceException($"{nameof(LoadoutPreparationView)} requires {nameof(fireFlaskButton)}.");
            if (confirmButton == null)
                throw new MissingReferenceException($"{nameof(LoadoutPreparationView)} requires {nameof(confirmButton)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(LoadoutPreparationView)} requires {nameof(closeButton)}.");
            if (summaryLabel == null)
                throw new MissingReferenceException($"{nameof(LoadoutPreparationView)} requires {nameof(summaryLabel)}.");
        }

        protected override void OnViewModelBound(LoadoutPreparationViewModel viewModel)
        {
            viewModel.SummaryChanged += HandleSummaryChanged;
            Render();
        }

        protected override void OnViewModelUnbound(LoadoutPreparationViewModel viewModel)
        {
            viewModel.SummaryChanged -= HandleSummaryChanged;
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

        private void Render()
        {
            var viewModel = BoundViewModel;
            panelRoot.SetActive(true);
            summaryLabel.text = viewModel.Summary;

            poisonArrowButton.interactable = viewModel.PoisonArrowInteractable;
            fireFlaskButton.interactable = viewModel.FireFlaskInteractable;
            confirmButton.interactable = viewModel.ConfirmInteractable;
        }

        private void HandleSummaryChanged(string _) => Render();
        private void HandlePoisonArrowClicked() => BoundViewModel.SelectPoisonArrowCommand.Execute();
        private void HandleFireFlaskClicked() => BoundViewModel.SelectFireFlaskCommand.Execute();
        private void HandleConfirmClicked() => BoundViewModel.ConfirmBuildCommand.Execute();
        private void HandleCloseClicked() => BoundViewModel.CloseCommand.Execute();
    }
}
