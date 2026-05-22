using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// World resource written every frame by <see cref="ClientInteractionFocusSystem"/>.
    /// Holds the closest interactable entity within focus range, or <see cref="InteractableKind.None"/>
    /// when no valid target is nearby.
    /// </summary>
    public struct InteractionFocus : IResource
    {
        public EntityGID        Target;
        public InteractableKind Kind;

        public bool HasFocus => Kind != InteractableKind.None;

        public void Clear()
        {
            Target = default;
            Kind   = InteractableKind.None;
        }
    }
}
