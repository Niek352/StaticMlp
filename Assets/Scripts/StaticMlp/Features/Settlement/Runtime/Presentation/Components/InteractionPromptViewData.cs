using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct InteractionPromptViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public InteractionPromptState State;
    }
}
