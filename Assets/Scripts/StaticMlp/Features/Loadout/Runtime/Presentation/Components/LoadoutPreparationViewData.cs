using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public struct LoadoutPreparationViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public bool IsAvailable;
        public bool CanPrepareBoss;
        public bool IsBossCommitted;
        public LoadoutModuleId SelectedPrimaryModuleId;
    }
}
