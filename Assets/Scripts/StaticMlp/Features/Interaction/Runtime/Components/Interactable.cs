using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Carries the concrete interaction kind for an entity that has <see cref="InteractableTag"/>.
    /// Set once at spawn; not replicated — client-local only.
    /// </summary>
    public struct Interactable : IComponent
    {
        public InteractableKind Kind;
    }
}
