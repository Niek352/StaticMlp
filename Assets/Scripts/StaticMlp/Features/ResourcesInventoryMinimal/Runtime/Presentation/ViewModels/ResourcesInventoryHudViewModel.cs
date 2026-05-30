using System;
using Aspid.MVVM;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    [ViewModel]
    public sealed partial class ResourcesInventoryHudViewModel
    {
        [OneWayBind] private ResourcesInventoryHudPresentation _presentation;

        public event Action Changed;

        public void Apply(in ResourcesInventoryHudViewData data)
        {
            Presentation = data.Presentation;
        }

        partial void OnPresentationChanged(ResourcesInventoryHudPresentation newValue)
        {
            Changed?.Invoke();
        }
    }
}
