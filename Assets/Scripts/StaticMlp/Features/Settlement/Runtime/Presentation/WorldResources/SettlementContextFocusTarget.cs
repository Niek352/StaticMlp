using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementContextFocusTarget : IResource
    {
        /// <summary>
        /// Written each frame by the client raycast/crosshair producer before Stage1 context session sync.
        /// Leave default when no valid construction-site target is under the pointer.
        /// </summary>
        public EntityGID Target;

        public bool HasTarget => Target.Raw != 0ul;
    }
}
