using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceNodeViewState : IViewComponent
    {
        public ushort KindIdValue;
        public int RemainingAmount;
        public float Scale;
    }
}
