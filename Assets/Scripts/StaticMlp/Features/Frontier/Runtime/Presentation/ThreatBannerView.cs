using Aspid.MVVM;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    [View]
    public sealed partial class ThreatBannerView : MonoView, IView<ThreatBannerViewModel>
    {
        [RequireBinder(typeof(bool))]
        [SerializeField] private MonoBinder[] _isVisible;

        [RequireBinder(typeof(string))]
        [SerializeField] private MonoBinder[] _summary;
    }
}
