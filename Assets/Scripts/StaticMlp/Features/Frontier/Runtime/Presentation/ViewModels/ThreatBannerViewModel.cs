using System;
using Aspid.MVVM;

namespace StaticMlp.Features.Frontier
{
    [ViewModel]
    public sealed partial class ThreatBannerViewModel
    {
        [OneWayBind] private ThreatBannerState _state;

        public event Action Changed;

        public void Apply(in ThreatBannerViewData data)
        {
            State = data.State;
        }

        partial void OnStateChanged(ThreatBannerState newValue)
        {
            Changed?.Invoke();
        }
    }
}
