using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.Combat
{
    /// <summary>
    /// Client-only view state for short-lived damage flash presentation.
    /// Presence: client world only, never server, never replicated.
    /// </summary>
    public struct DamageFeedbackViewState : IViewComponent
    {
        public bool IsActive;
        public float DamageAmount;
        public float RemainingLifetime;
        public float Intensity;
    }
}
