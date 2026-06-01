using Aspid.MVVM;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    [View]
    public sealed partial class ResourcesInventoryHudView : MonoView, IView<ResourcesInventoryHudViewModel>
    {
        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _isReady;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;
    }
}
