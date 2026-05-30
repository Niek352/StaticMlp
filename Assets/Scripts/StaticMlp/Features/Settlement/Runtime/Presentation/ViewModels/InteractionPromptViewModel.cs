using Aspid.StaticEcs.Windows;

namespace StaticMlp.Features.Settlement
{
    public sealed class InteractionPromptViewModel : EcsWindowViewModelBase
    {
        public InteractionPromptState State { get; private set; }

        public void Sync(in InteractionPromptState state)
        {
            State = state;
            NotifyChanged();
        }
    }
}
