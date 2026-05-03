namespace StaticMlp.Features.Buildings
{
    public static class BuildingMenuCommands
    {
        private static bool _toggleRequested;
        private static bool _openRequested;
        private static bool _closeRequested;
        private static bool _selectRequested;
        private static ushort _selectedBuildingId;

        public static void Toggle() => _toggleRequested = true;
        public static void Open() => _openRequested = true;
        public static void Close() => _closeRequested = true;

        public static void Select(ushort buildingId)
        {
            _selectedBuildingId = buildingId;
            _selectRequested = true;
        }

        public static void Consume(
            out bool toggleRequested,
            out bool openRequested,
            out bool closeRequested,
            out bool selectRequested,
            out ushort selectedBuildingId)
        {
            toggleRequested = _toggleRequested;
            openRequested = _openRequested;
            closeRequested = _closeRequested;
            selectRequested = _selectRequested;
            selectedBuildingId = _selectedBuildingId;

            _toggleRequested = false;
            _openRequested = false;
            _closeRequested = false;
            _selectRequested = false;
            _selectedBuildingId = 0;
        }
    }
}
