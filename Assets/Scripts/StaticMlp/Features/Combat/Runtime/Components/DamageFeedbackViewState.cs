using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.Combat
{
    public struct DamageFeedbackViewState : IViewComponent
    {
        public bool IsActive;
        public float DamageAmount;
        public float RemainingLifetime;
        public float Intensity;
    }
}
