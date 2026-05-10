using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    /// <summary>
    /// Client-only transient state for local presentation diffs such as damage flashes.
    /// Presence: client world only, never server, never replicated.
    /// </summary>
    public struct HealthPresentationState : IComponent
    {
        public float LastKnownCurrent;
        public bool IsInitialized;
    }
}
