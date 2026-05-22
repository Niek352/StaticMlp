using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Marker tag. All entities that can be selected by the proximity focus scan must carry this tag.
    /// Placed at spawn time alongside <see cref="Interactable"/> which stores the concrete kind.
    /// </summary>
    public struct InteractableTag : ITag { }
}
