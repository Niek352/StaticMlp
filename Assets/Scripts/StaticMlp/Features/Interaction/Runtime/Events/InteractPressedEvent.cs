using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Fired in the same frame the player presses Interact (E) while an interactable entity is focused.
    /// Consumed by feature systems that handle interaction for the given <see cref="Kind"/>.
    /// </summary>
    public struct InteractPressedEvent : IEvent
    {
        public EntityGID        Target;
        public InteractableKind Kind;
    }
}
