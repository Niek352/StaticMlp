using Aspid.StaticEcs.Windows;

namespace StaticMlp.Features.Frontier
{
    public sealed class ThreatBannerViewModel : EcsWindowViewModelBase
    {
        public ThreatBannerState State { get; private set; }

        public void Sync(in ThreatBannerState state)
        {
            State = state;
            NotifyChanged();
        }
    }
}
