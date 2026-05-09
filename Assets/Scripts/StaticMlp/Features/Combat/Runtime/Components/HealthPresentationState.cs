using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct HealthPresentationState : IComponent
    {
        public float LastKnownCurrent;
        public bool IsInitialized;
    }
}
