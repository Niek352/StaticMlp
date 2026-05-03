using StaticMlp.Features.BuildingCatalog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingMenuView : MonoBehaviour
    {
        [Header("Hierarchy")]
        [SerializeField] private GameObject panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button woodenHutButton;
        [SerializeField] private Button closeButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI selectedBuildingLabel;
        [SerializeField] private TextMeshProUGUI costLabel;

        private void Awake()
        {
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
        }

        private void OnEnable()
        {
            woodenHutButton.onClick.AddListener(SelectWoodenHut);
            closeButton.onClick.AddListener(BuildingMenuCommands.Close);
        }

        private void OnDisable()
        {
            woodenHutButton.onClick.RemoveListener(SelectWoodenHut);
            closeButton.onClick.RemoveListener(BuildingMenuCommands.Close);
        }

        private void Update()
        {
            panelRoot.SetActive(BuildingMenuRuntime.IsOpen);

            if (BuildingMenuRuntime.HasSelection
                && StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(
                    new BuildingId(BuildingMenuRuntime.SelectedBuildingId),
                    out var selected))
            {
                selectedBuildingLabel.text = selected.DisplayName;
                costLabel.text = $"Wood {selected.CostWood}  Stone {selected.CostStone}";
                return;
            }

            if (StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(
                    StaticMlp.Features.BuildingCatalog.BuildingCatalog.WoodenHutId,
                    out var woodenHut))
            {
                selectedBuildingLabel.text = woodenHut.DisplayName;
                costLabel.text = $"Wood {woodenHut.CostWood}  Stone {woodenHut.CostStone}";
            }
        }

        public void Open()
        {
            BuildingMenuCommands.Open();
        }

        public void Close()
        {
            BuildingMenuCommands.Close();
        }

        public void SelectWoodenHut()
        {
            BuildingMenuCommands.Select(StaticMlp.Features.BuildingCatalog.BuildingCatalog.WoodenHutId.Value);
        }
    }
}
