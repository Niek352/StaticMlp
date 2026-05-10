using StaticMlp.Features.EcsViews;

namespace StaticMlp.Features.Combat
{
    public struct PassiveAutoAttackTargetViewState : IViewComponent
    {
        public bool IsHighlighted;
        public float HighlightIntensity;
        public float RemainingFade;
    }
}
