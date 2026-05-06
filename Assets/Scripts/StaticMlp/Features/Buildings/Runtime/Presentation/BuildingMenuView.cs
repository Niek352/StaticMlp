using System;
using Code.EcsUi.Mvc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingMenuView : PrefabViewBase
    {
        [Header("Hierarchy")]
        [SerializeField] private GameObject panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button woodenHutButton;
        [SerializeField] private Button closeButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI selectedBuildingLabel;
        [SerializeField] private TextMeshProUGUI costLabel;

        private Action onWoodenHutClicked;
        private Action onCloseClicked;

        protected override void Awake()
        {
            base.Awake();

            if (panelRoot == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(panelRoot)}.");
            if (woodenHutButton == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(woodenHutButton)}.");
            if (closeButton == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(closeButton)}.");
            if (selectedBuildingLabel == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(selectedBuildingLabel)}.");
            if (costLabel == null)
                throw new MissingReferenceException($"{nameof(BuildingMenuView)} requires {nameof(costLabel)}.");

            panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            woodenHutButton.onClick.AddListener(HandleWoodenHutClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            woodenHutButton.onClick.RemoveListener(HandleWoodenHutClicked);
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        public void Bind(Action woodenHutClicked, Action closeClicked)
        {
            if (onWoodenHutClicked != null || onCloseClicked != null)
                throw new InvalidOperationException($"{nameof(BuildingMenuView)} is already bound to a controller.");

            onWoodenHutClicked = woodenHutClicked ?? throw new ArgumentNullException(nameof(woodenHutClicked));
            onCloseClicked = closeClicked ?? throw new ArgumentNullException(nameof(closeClicked));
        }

        public void Unbind()
        {
            onWoodenHutClicked = null;
            onCloseClicked = null;
        }

        public void Render(in BuildingMenuPresentation presentation)
        {
            panelRoot.SetActive(presentation.IsOpen);
            selectedBuildingLabel.text = presentation.SelectedBuildingName;
            costLabel.text = $"Wood {presentation.CostWood}  Stone {presentation.CostStone}";
        }

        private void HandleWoodenHutClicked()
        {
            onWoodenHutClicked.Invoke();
        }

        private void HandleCloseClicked()
        {
            onCloseClicked.Invoke();
        }
    }
}
