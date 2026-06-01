using Aspid.MVVM;

namespace StaticMlp.Features.Frontier
{
    [ViewModel]
    public sealed partial class ThreatBannerViewModel
    {
        [OneWayBind] private bool _isVisible;
        [OneWayBind] private string _summary;

        public void Apply(in ThreatBannerViewData data)
        {
            var state = data.State;
            IsVisible = state.IsVisible;
            Summary = state.IsVisible
                ? $"Threat: {state.Phase}\nRaid: {state.RaidStatus}\nActivation tick: {state.ActivateAtTick}"
                : string.Empty;
        }
    }
}
