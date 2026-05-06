namespace StaticMlp.Features.Buildings
{
    public readonly struct BuildingMenuPresentation
    {
        public readonly bool IsOpen;
        public readonly string SelectedBuildingName;
        public readonly int CostWood;
        public readonly int CostStone;

        public BuildingMenuPresentation(
            bool isOpen,
            string selectedBuildingName,
            int costWood,
            int costStone)
        {
            IsOpen = isOpen;
            SelectedBuildingName = selectedBuildingName;
            CostWood = costWood;
            CostStone = costStone;
        }
    }
}
