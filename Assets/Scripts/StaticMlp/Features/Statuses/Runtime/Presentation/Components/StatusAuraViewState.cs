using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.Statuses
{
    public struct StatusAuraViewState : IViewComponent
    {
        public StatusVisualFlags Flags;
        public float HealthNormalized;
        public bool IsDead;
    }
}
