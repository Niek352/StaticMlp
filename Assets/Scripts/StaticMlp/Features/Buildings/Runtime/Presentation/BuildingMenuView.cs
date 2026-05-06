using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;
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
            closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            woodenHutButton.onClick.RemoveListener(SelectWoodenHut);
            closeButton.onClick.RemoveListener(Close);
        }

        private void Update()
        {
            if (!CW.HasResource<BuildingMenuState>())
            {
                panelRoot.SetActive(false);
                return;
            }

            var state = CW.GetResource<BuildingMenuState>();
            panelRoot.SetActive(state.IsOpen);

            if (state.HasSelection
                && StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(
                    new BuildingId(state.SelectedBuildingId),
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
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.IsOpen = true;
        }

        public void Close()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.IsOpen = false;
            state.ClearSelection();
        }

        public void SelectWoodenHut()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.Select(StaticMlp.Features.BuildingCatalog.BuildingCatalog.WoodenHutId.Value, Time.frameCount);
        }
    }
}
