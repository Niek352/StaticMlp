using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public static class BuildingMenuRuntime
    {
        public static bool IsOpen { get; private set; }
        public static bool HasSelection { get; private set; }
        public static ushort SelectedBuildingId { get; private set; }
        public static int SelectionFrame { get; private set; } = -1;

        public static void Publish(in BuildingMenuState state, bool selectedThisFrame = false)
        {
            IsOpen = state.IsOpen;
            HasSelection = state.HasSelection;
            SelectedBuildingId = state.SelectedBuildingId;

            if (selectedThisFrame)
                SelectionFrame = Time.frameCount;
        }

        public static void Clear()
        {
            IsOpen = false;
            HasSelection = false;
            SelectedBuildingId = 0;
            SelectionFrame = -1;
        }
    }
}
