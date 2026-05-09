using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.Combat
{
    /// <summary>
    /// Client-only presentation state built from replicated combat components.
    /// Presence: client world only, never server, never replicated.
    /// </summary>
    public struct CombatViewState : IViewComponent
    {
        public float HealthNormalized;
        public bool IsDead;
        public StatusVisualFlags StatusFlags;
    }
}
