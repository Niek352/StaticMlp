using System;
using StaticMlp.Features.BuildingCatalog;
using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingMenuView : PrefabViewBase
    {
        [Header("Hierarchy")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Cards")]
        [SerializeField] private GameObject[] _cardRoots;
        [SerializeField] private Button[] _cardButtons;
        [SerializeField] private TextMeshProUGUI[] _cardNameLabels;
        [SerializeField] private TextMeshProUGUI[] _cardCategoryLabels;
        [SerializeField] private TextMeshProUGUI[] _cardCostLabels;

        [Header("Categories")]
        [SerializeField] private TextMeshProUGUI[] _categoryLabels;

        [Header("Buttons")]
        [SerializeField] private Button _closeButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _selectedBuildingLabel;

        private BuildingId[] _renderedCardIds;
        private bool[] _renderedCardAvailability;
        private int _visibleCardCount;
        private UnityAction[] _cardClickActions;
        private UnityAction _closeClickAction;
        private Action<BuildingId> _onBuildingClicked;
        private Action _onCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_panelRoot == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_panelRoot)}.");
            if (_closeButton == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_closeButton)}.");
            if (_selectedBuildingLabel == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_selectedBuildingLabel)}.");

            CheckCardBindings();
            CheckCategoryBindings();

            _renderedCardIds = new BuildingId[_cardButtons.Length];
            _renderedCardAvailability = new bool[_cardButtons.Length];
            _panelRoot.SetActive(false);
        }

        public void Bind(Action<BuildingId> buildingClicked, Action closeClicked)
        {
            if (_onBuildingClicked != null || _onCloseClicked != null)
                throw new InvalidOperationException($"{nameof(BuildingMenuView)} is already bound to a controller.");

            _onBuildingClicked = buildingClicked ?? throw new ArgumentNullException(nameof(buildingClicked));
            _onCloseClicked = closeClicked ?? throw new ArgumentNullException(nameof(closeClicked));

            _cardClickActions = new UnityAction[_cardButtons.Length];
            for (var i = 0; i < _cardButtons.Length; i++)
            {
                var slotIndex = i;
                _cardClickActions[i] = () => HandleCardClicked(slotIndex);
                _cardButtons[i].onClick.AddListener(_cardClickActions[i]);
            }

            _closeClickAction = HandleCloseClicked;
            _closeButton.onClick.AddListener(_closeClickAction);
        }

        public void Unbind()
        {
            if (_cardClickActions != null)
            {
                for (var i = 0; i < _cardClickActions.Length; i++)
                    _cardButtons[i].onClick.RemoveListener(_cardClickActions[i]);
            }

            if (_closeClickAction != null)
                _closeButton.onClick.RemoveListener(_closeClickAction);

            _cardClickActions = null;
            _closeClickAction = null;
            _onBuildingClicked = null;
            _onCloseClicked = null;
        }

        public void Render(in BuildingMenuPresentation presentation)
        {
            _panelRoot.SetActive(presentation.IsOpen);
            _selectedBuildingLabel.text = presentation.SelectedBuildingName;
            RenderCategories(presentation.Categories);
            RenderCards(presentation.Cards);
        }

        private void HandleCardClicked(int slotIndex)
        {
            if (slotIndex >= _visibleCardCount)
                throw new InvalidOperationException($"{nameof(BuildingMenuView)} received a click from hidden card slot {slotIndex}.");
            if (!_renderedCardAvailability[slotIndex])
                throw new InvalidOperationException($"{nameof(BuildingMenuView)} received a click from locked card slot {slotIndex}.");

            _onBuildingClicked.Invoke(_renderedCardIds[slotIndex]);
        }

        private void HandleCloseClicked()
        {
            _onCloseClicked.Invoke();
        }

        private void RenderCategories(BuildingMenuCategoryPresentation[] categories)
        {
            if (categories.Length > _categoryLabels.Length)
                throw new InvalidOperationException(
                    $"{nameof(BuildingMenuView)} has {_categoryLabels.Length} category labels for {categories.Length} categories.");

            for (var i = 0; i < categories.Length; i++)
            {
                _categoryLabels[i].gameObject.SetActive(true);
                _categoryLabels[i].text = categories[i].IsSelected
                    ? $"> {categories[i].Label} ({categories[i].CardCount})"
                    : $"{categories[i].Label} ({categories[i].CardCount})";
            }

            for (var i = categories.Length; i < _categoryLabels.Length; i++)
                _categoryLabels[i].gameObject.SetActive(false);
        }

        private void RenderCards(BuildingMenuCardPresentation[] cards)
        {
            if (cards.Length > _cardButtons.Length)
                throw new InvalidOperationException(
                    $"{nameof(BuildingMenuView)} has {_cardButtons.Length} card slots for {cards.Length} building cards.");

            _visibleCardCount = cards.Length;

            for (var i = 0; i < cards.Length; i++)
            {
                _renderedCardIds[i] = cards[i].BuildingId;
                _cardRoots[i].SetActive(true);
                _cardNameLabels[i].text = cards[i].IsSelected
                    ? $"> {cards[i].DisplayName}"
                    : cards[i].DisplayName;
                _cardCategoryLabels[i].text = cards[i].CategoryLabel;
                _cardCostLabels[i].text = cards[i].IsAvailable
                    ? cards[i].CostLabel
                    : $"Locked: {cards[i].LockedReason}";
                _cardButtons[i].interactable = cards[i].IsAvailable;
                _renderedCardAvailability[i] = cards[i].IsAvailable;
            }

            for (var i = cards.Length; i < _cardButtons.Length; i++)
            {
                _cardRoots[i].SetActive(false);
                _renderedCardIds[i] = default;
                _renderedCardAvailability[i] = false;
                _cardButtons[i].interactable = false;
            }
        }

        private void CheckCardBindings()
        {
            if (_cardRoots == null || _cardRoots.Length == 0)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_cardRoots)}.");
            if (_cardButtons == null || _cardButtons.Length != _cardRoots.Length)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires one {nameof(_cardButtons)} entry per card root.");
            if (_cardNameLabels == null || _cardNameLabels.Length != _cardRoots.Length)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires one {nameof(_cardNameLabels)} entry per card root.");
            if (_cardCategoryLabels == null || _cardCategoryLabels.Length != _cardRoots.Length)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires one {nameof(_cardCategoryLabels)} entry per card root.");
            if (_cardCostLabels == null || _cardCostLabels.Length != _cardRoots.Length)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires one {nameof(_cardCostLabels)} entry per card root.");

            for (var i = 0; i < _cardRoots.Length; i++)
            {
                if (_cardRoots[i] == null)
                    throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_cardRoots)}[{i}].");
                if (_cardButtons[i] == null)
                    throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_cardButtons)}[{i}].");
                if (_cardNameLabels[i] == null)
                    throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_cardNameLabels)}[{i}].");
                if (_cardCategoryLabels[i] == null)
                    throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_cardCategoryLabels)}[{i}].");
                if (_cardCostLabels[i] == null)
                    throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_cardCostLabels)}[{i}].");
            }
        }

        private void CheckCategoryBindings()
        {
            if (_categoryLabels == null || _categoryLabels.Length == 0)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_categoryLabels)}.");

            for (var i = 0; i < _categoryLabels.Length; i++)
            {
                if (_categoryLabels[i] == null)
                    throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(_categoryLabels)}[{i}].");
            }
        }
    }
}
