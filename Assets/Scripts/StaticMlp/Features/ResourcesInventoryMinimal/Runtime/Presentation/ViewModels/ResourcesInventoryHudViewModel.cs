using Aspid.StaticEcs.Windows;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryHudViewModel : EcsWindowViewModelBase
    {
        public ResourcesInventoryHudPresentation Presentation { get; private set; }

        public void Sync(in ResourcesInventoryHudPresentation presentation)
        {
            Presentation = presentation;
            NotifyChanged();
        }
    }
}
