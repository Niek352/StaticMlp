using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceNodeViewState : IViewComponent
    {
        public ushort KindIdValue;
        public int RemainingAmount;
        public int MaxAmount;
        public OpenWorldResourceOverlayFlags Flags;
        public float Scale;
        public int PreviousRemainingAmount;
        public float HitFlashIntensity;
        public float DepletionPulseIntensity;
    }
}
