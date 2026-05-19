using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public struct LoadoutPreparationScreenState : IResource
    {
        public bool IsAvailable;
        public bool CanConfirm;
        public bool CanPrepareBoss;
        public bool IsBossCommitted;
        public LoadoutModuleId SelectedPrimaryModuleId;
        public bool PoisonArrowAvailable;
        public bool FireFlaskAvailable;
        public bool PoisonArrowSelected;
        public bool FireFlaskSelected;
    }
}
