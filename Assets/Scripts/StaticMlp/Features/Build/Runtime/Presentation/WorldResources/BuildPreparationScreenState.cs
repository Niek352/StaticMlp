using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Build
{
    public struct BuildPreparationScreenState : IResource
    {
        public bool IsAvailable;
        public bool CanConfirm;
        public bool IsBossCommitted;
        public BuildModuleId SelectedPrimaryModuleId;
        public bool PoisonArrowAvailable;
        public bool FireFlaskAvailable;
        public bool PoisonArrowSelected;
        public bool FireFlaskSelected;
    }
}
